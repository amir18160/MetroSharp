#!/usr/bin/env bash
set -euo pipefail

# manage_api.sh
# Usage: ./manage_api.sh
# - checks services on ports 8083 and 9696
# - creates/starts systemd unit for API.dll with automatic restart
# - prompts to stop an existing running process if found

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CANDIDATE_DOTNET="$(command -v dotnet || true)"
SERVICE_NAME="api.service"
UNIT_PATH="/etc/systemd/system/${SERVICE_NAME}"
PORT_CHECKS=(8083 9696)
PORT_DESCS=("qbittorrent-nox (port 8083)" "prowlarr (port 9696)")

# Helper: check http on a port
check_http_port() {
  local port=$1
  local timeout=2
  if command -v curl >/dev/null 2>&1; then
    if curl -sS --max-time "${timeout}" "http://localhost:${port}/" >/dev/null 2>&1; then
      echo "up"
      return 0
    else
      echo "down"
      return 1
    fi
  elif command -v nc >/dev/null 2>&1; then
    if nc -z -w ${timeout} localhost "${port}" >/dev/null 2>&1; then
      echo "up"
      return 0
    else
      echo "down"
      return 1
    fi
  else
    echo "unknown"
    return 2
  fi
}

# Find API.dll: prefer local folder, then search one level down
find_api_dll() {
  # 1) same dir as script
  if [[ -f "${SCRIPT_DIR}/API.dll" ]]; then
    realpath "${SCRIPT_DIR}/API.dll"
    return 0
  fi

  # 2) current working dir
  if [[ -f "${PWD}/API.dll" ]]; then
    realpath "${PWD}/API.dll"
    return 0
  fi

  # 3) search up to depth 2
  local found
  found="$(find "${SCRIPT_DIR}" -maxdepth 2 -type f -name 'API.dll' 2>/dev/null | head -n1 || true)"
  if [[ -n "${found}" ]]; then
    realpath "${found}"
    return 0
  fi

  # 4) search cwd
  found="$(find "${PWD}" -maxdepth 2 -type f -name 'API.dll' 2>/dev/null | head -n1 || true)"
  if [[ -n "${found}" ]]; then
    realpath "${found}"
    return 0
  fi

  return 1
}

# Print port checks
echo "Checking required services (quick HTTP checks)..."
for i in "${!PORT_CHECKS[@]}"; do
  port="${PORT_CHECKS[$i]}"
  desc="${PORT_DESCS[$i]}"
  status="$(check_http_port "${port}" || true)"
  case "${status}" in
    up) printf "  - %s: \e[32mreachable\e[0m (port %s)\n" "${desc}" "${port}" ;;
    down) printf "  - %s: \e[33mnot reachable\e[0m (port %s)\n" "${desc}" "${port}" ;;
    unknown) printf "  - %s: \e[31mcould not check (curl or nc missing)\e[0m\n" "${desc}" ;;
  esac
done
echo

# Confirm dotnet exists
if [[ -z "${CANDIDATE_DOTNET}" ]]; then
  echo "ERROR: 'dotnet' not found in PATH. Install the .NET runtime/SDK before continuing."
  exit 2
fi

# Locate API.dll
API_DLL_PATH=""
if api_path="$(find_api_dll)"; then
  API_DLL_PATH="${api_path}"
  echo "Found API.dll -> ${API_DLL_PATH}"
else
  echo -e "\nERROR: API.dll not found. Please ensure API.dll is present in this repo's build output."
  echo "Search paths tried: ${SCRIPT_DIR} , ${PWD} and subfolders (depth 2)."
  exit 3
fi

API_DIR="$(dirname "${API_DLL_PATH}")"
DOTNET_BIN="${CANDIDATE_DOTNET}"
EXEC_CMD="${DOTNET_BIN} \"${API_DLL_PATH}\" --urls http://0.0.0.0:5159"
ENV_NAME="Production"

# Detect existing running service by systemd first, then by process match
is_service_active() {
  if command -v systemctl >/dev/null 2>&1; then
    if systemctl is-active --quiet "${SERVICE_NAME}"; then
      return 0
    fi
  fi
  return 1
}

find_running_pid() {
  # find dotnet processes running API.dll
  pgrep -af dotnet | grep -F "${API_DLL_PATH}" | awk '{print $1}' | head -n1 || true
}

# If service active or process running, prompt user
existing_action_required=false
if is_service_active; then
  echo "Service '${SERVICE_NAME}' is already active (systemd)."
  existing_action_required=true
else
  pid="$(find_running_pid || true)"
  if [[ -n "${pid}" ]]; then
    echo "A dotnet process running API.dll was found (PID ${pid})."
    existing_action_required=true
  fi
fi

if [[ "${existing_action_required}" = true ]]; then
  # Ask whether to stop
  while true; do
    read -rp $'Do you want to stop the running process/service and continue with creating/restarting the systemd unit? [y/N]: ' yn
    yn="${yn:-N}"
    case "${yn}" in
      [Yy]* )
        echo "Stopping..."
        if is_service_active; then
          echo " -> stopping systemd service ${SERVICE_NAME} ..."
          sudo systemctl stop "${SERVICE_NAME}" || true
        fi
        pid="$(find_running_pid || true)"
        if [[ -n "${pid}" ]]; then
          echo " -> killing PID ${pid} ..."
          sudo kill "${pid}" || true
        fi
        # give it a moment
        sleep 1
        break
        ;;
      [Nn]* | * )
        echo "Exiting without changes."
        exit 0
        ;;
    esac
  done
fi

# Prepare the unit content
# Determine the user to run as: prefer original sudo user or current user
RUN_AS_USER="${SUDO_USER:-${USER}}"

read -rp "Create/overwrite systemd unit at ${UNIT_PATH} and start the service as user '${RUN_AS_USER}'? [Y/n]: " confirm
confirm="${confirm:-Y}"
if [[ "${confirm}" =~ ^[Nn]$ ]]; then
  echo "Aborted by user."
  exit 0
fi

echo "Writing systemd unit to ${UNIT_PATH} (requires sudo)..."

sudo tee "${UNIT_PATH}" > /dev/null <<-UNIT
[Unit]
Description=API (dotnet) - API.dll
After=network.target

[Service]
# Run as a non-root user to reduce risk; change if needed
User=${RUN_AS_USER}
WorkingDirectory=${API_DIR}
Environment=ASPNETCORE_ENVIRONMENT=${ENV_NAME}
# ExecStart must be an absolute command
ExecStart=${DOTNET_BIN} ${API_DLL_PATH} --urls http://0.0.0.0:5159
Restart=on-failure
RestartSec=5
# Optional: limit restarts
StartLimitBurst=5
StartLimitIntervalSec=60

[Install]
WantedBy=multi-user.target
UNIT

echo "Reloading systemd daemon..."
sudo systemctl daemon-reload

echo "Enabling and starting ${SERVICE_NAME}..."
sudo systemctl enable --now "${SERVICE_NAME}"

if command -v systemctl >/dev/null 2>&1; then
  echo "Reloading Caddy to ensure proxy picks up API..."
  sudo systemctl reload caddy || sudo systemctl restart caddy || true
fi

echo
echo "Service ${SERVICE_NAME} should now be active. Checking status..."
sudo systemctl status "${SERVICE_NAME}" --no-pager

echo
echo "If you want to stop the service later: sudo systemctl stop ${SERVICE_NAME}"
echo "To view logs: sudo journalctl -u ${SERVICE_NAME} -f"
echo "To completely remove the unit: sudo systemctl disable --now ${SERVICE_NAME} && sudo rm -f ${UNIT_PATH} && sudo systemctl daemon-reload"
