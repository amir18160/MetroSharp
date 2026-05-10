#!/usr/bin/env bash
# install-media-stack-ubuntu-new.sh
# Installs dotnet 9 (preferred via distro packages, fallback to dotnet-install),
# Prowlarr, qbittorrent-nox (webui port 8083),
# Caddy, and ffmpeg (with fallback to static build).
# Target: Ubuntu 22.04 / 24.04 and newer (auto-detected)
# Run as root (sudo)

set -euo pipefail
export DEBIAN_FRONTEND=noninteractive

if [ $(id -u) -ne 0 ]; then
  echo "This script must be run as root. Use sudo." >&2
  exit 1
fi

# Basic env
ARCH_RAW=$(dpkg --print-architecture 2>/dev/null || echo "amd64")
case "$ARCH_RAW" in
  amd64) ARCH=x64 ;;
  arm64) ARCH=arm64 ;;
  *) ARCH=x64 ;;
esac
UBUNTU_CODENAME=$(lsb_release -cs 2>/dev/null || echo unknown)
UBUNTU_VERSION=$(lsb_release -rs 2>/dev/null || echo 0.0)
echo "Detected Ubuntu codename=${UBUNTU_CODENAME} version=${UBUNTU_VERSION} arch=${ARCH_RAW}"

apt update
apt install -y wget curl gnupg ca-certificates lsb-release software-properties-common apt-transport-https jq unzip || true

#### helper: safe run with echo
run() { echo "+ $*"; "$@"; }

#### Install ffmpeg (preferred: apt; fallback: static build) ####
install_ffmpeg() {
  echo
  echo "=== ffmpeg installation ==="

  if command -v ffmpeg >/dev/null 2>&1; then
    echo "ffmpeg already installed at: $(command -v ffmpeg)"
    return 0
  fi

  echo "Attempting to install ffmpeg via apt..."
  apt update
  if apt install -y ffmpeg; then
    echo "ffmpeg installed via apt: $(command -v ffmpeg)"
    return 0
  fi

  echo "apt install failed or not available. Falling back to static build..."
  tmpdir=$(mktemp -d)
  pushd "$tmpdir" >/dev/null

  # choose static build by architecture
  case "$ARCH_RAW" in
    amd64) FFMPEG_TAR_URL="https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz" ;;
    arm64) FFMPEG_TAR_URL="https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-arm64-static.tar.xz" ;;
    *) FFMPEG_TAR_URL="https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz" ;;
  esac

  echo "Downloading $FFMPEG_TAR_URL"
  if ! curl -fsSL -o ffmpeg-static.tar.xz "$FFMPEG_TAR_URL"; then
    echo "Failed to download static ffmpeg from $FFMPEG_TAR_URL" >&2
    popd >/dev/null
    rm -rf "$tmpdir"
    return 1
  fi

  tar -xJf ffmpeg-static.tar.xz
  # extracted folder contains ffmpeg and ffprobe
  EXTRACT_DIR=$(find . -maxdepth 1 -type d -name "ffmpeg*static" | head -n1)
  if [ -z "$EXTRACT_DIR" ]; then
    # try alternative detection
    EXTRACT_DIR=$(tar -tf ffmpeg-static.tar.xz | head -n1 | cut -d/ -f1 || true)
  fi

  if [ -d "$EXTRACT_DIR" ]; then
    echo "Installing ffmpeg binaries to /usr/local/bin"
    cp -p "$EXTRACT_DIR"/ffmpeg /usr/local/bin/ || true
    cp -p "$EXTRACT_DIR"/ffprobe /usr/local/bin/ || true
  else
    # attempt to find binaries anywhere in extracted tree
    BIN_FFMPEG=$(find . -type f -name ffmpeg -perm /u+x | head -n1 || true)
    BIN_FFPROBE=$(find . -type f -name ffprobe -perm /u+x | head -n1 || true)
    if [ -n "$BIN_FFMPEG" ]; then
      cp -p "$BIN_FFMPEG" /usr/local/bin/ || true
    fi
    if [ -n "$BIN_FFPROBE" ]; then
      cp -p "$BIN_FFPROBE" /usr/local/bin/ || true
    fi
  fi

  chmod +x /usr/local/bin/ffmpeg || true
  chmod +x /usr/local/bin/ffprobe || true

  popd >/dev/null
  rm -rf "$tmpdir"

  # ensure /usr/local/bin is on PATH system-wide (it usually is)
  if [ ! -f /etc/profile.d/ffmpeg.sh ]; then
    echo "Exporting /usr/local/bin to PATH for all users (/etc/profile.d/ffmpeg.sh)"
    cat > /etc/profile.d/ffmpeg.sh <<'EOF'
# Ensure /usr/local/bin is in PATH (for ffmpeg static installs)
export PATH="/usr/local/bin:$PATH"
EOF
    chmod 644 /etc/profile.d/ffmpeg.sh
  else
    # try to ensure the entry exists
    if ! grep -q "/usr/local/bin" /etc/profile.d/ffmpeg.sh 2>/dev/null; then
      echo "Prepending /usr/local/bin to PATH in /etc/profile.d/ffmpeg.sh"
      sed -i '1s|^|export PATH="/usr/local/bin:$PATH"\n|' /etc/profile.d/ffmpeg.sh || true
    fi
  fi

  # rehash to make ffmpeg available in current shell
  hash -r || true

  if command -v ffmpeg >/dev/null 2>&1; then
    echo "ffmpeg available at: $(command -v ffmpeg)"
    return 0
  else
    echo "ffmpeg installation failed." >&2
    return 1
  fi
}

#### 1) Install .NET 9 ####
echo
echo "=== .NET 9 installation ==="
# Try installing dotnet-sdk-9.0 from distro packages (preferred on 22.04+ where supported)
if apt-cache show dotnet-sdk-9.0 >/dev/null 2>&1; then
  echo "dotnet-sdk-9.0 available in apt - installing..."
  apt update
  apt install -y dotnet-sdk-9.0 || true
fi

# If not installed, attempt canonical backports or fallback to Microsoft's dotnet-install script
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found via apt — attempting canonical backports and then dotnet-install as fallback..."
  if ! apt-get install -y dotnet-sdk-9.0 >/dev/null 2>&1; then
    echo "apt install failed or package not present — using dotnet-install.sh fallback..."
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    DOTNET_INSTALL_DIR=/usr/share/dotnet
    mkdir -p "$DOTNET_INSTALL_DIR"
    /tmp/dotnet-install.sh --channel 9.0 --install-dir "$DOTNET_INSTALL_DIR" --architecture "$ARCH"
    ln -sf "$DOTNET_INSTALL_DIR/dotnet" /usr/bin/dotnet || true
  fi
fi

if command -v dotnet >/dev/null 2>&1; then
  echo "dotnet --info:" && dotnet --info | sed -n '1,6p'
else
  echo "dotnet installation failed - please check manually." >&2
fi


#### 2) Install qBittorrent-nox (headless) ####
echo
echo "=== qBittorrent-nox installation (webui port 8083) ==="
if ! grep -q "qbittorrent-team" /etc/apt/sources.list.d/* >/dev/null 2>&1; then
  add-apt-repository -y ppa:qbittorrent-team/qbittorrent-stable || true
fi
apt update
apt install -y qbittorrent-nox || true

QB_USER=qbittorrent
QB_HOME=/var/lib/qbittorrent
if ! id -u "$QB_USER" >/dev/null 2>&1; then
  useradd --system --home-dir "$QB_HOME" --shell /usr/sbin/nologin "$QB_USER" || true
fi
mkdir -p "$QB_HOME"
chown -R "$QB_USER:$QB_USER" "$QB_HOME" || true

cat > /etc/systemd/system/qbittorrent-nox.service <<'EOF'
[Unit]
Description=qBittorrent (nox)
After=network.target
StartLimitIntervalSec=0
StartLimitBurst=0

[Service]
User=qbittorrent
Group=qbittorrent
Type=simple
ExecStart=/usr/bin/qbittorrent-nox --webui-port=8083
Restart=always
RestartSec=5
LimitNOFILE=65536

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable --now qbittorrent-nox || true

#### 3) Install Caddy (official repo) ####
echo
echo "=== Caddy installation ==="
# Add Caddy GPG key and repo
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
cat > /etc/apt/sources.list.d/caddy-stable.list <<EOF
# Caddy stable
deb [signed-by=/usr/share/keyrings/caddy-stable-archive-keyring.gpg] https://dl.cloudsmith.io/public/caddy/stable/deb/ubuntu ${UBUNTU_CODENAME} main
EOF
apt update
apt install -y caddy || true
systemctl enable --now caddy || true

# configure caddy for reverse proxy
echo
echo "=== Configuring Caddy reverse-proxy for api/qbittorrent/prowlarr ==="

CADDYFILE_PATH="/etc/caddy/Caddyfile"
CF_CERT="/etc/ssl/cf_origin.pem"
CF_KEY="/etc/ssl/cf_origin.key"

# Ensure directory exists
mkdir -p "$(dirname "$CADDYFILE_PATH")"

if [ -f "$CF_CERT" ] && [ -f "$CF_KEY" ]; then
  echo "Found Cloudflare Origin cert and key at $CF_CERT / $CF_KEY - writing Caddyfile using those files..."
  cat > "$CADDYFILE_PATH" <<EOF
api.metromoviez.ir {
    route /* {
        uri strip_prefix /
        reverse_proxy localhost:5159/api{uri}
    }
    tls $CF_CERT $CF_KEY
    encode gzip
}

public.metromoviez.ir {
    reverse_proxy localhost:5159/public{uri}
    tls $CF_CERT $CF_KEY
    encode gzip
}

qbittorrent.metromoviez.ir {
    reverse_proxy localhost:8083 {
        header_up Host {host}
        header_up X-Forwarded-For {remote}
        header_up X-Forwarded-Proto {scheme}
    }
    tls $CF_CERT $CF_KEY
    encode gzip
}

prowlarr.metromoviez.ir {
    reverse_proxy localhost:9696 {
        header_up Host {host}
        header_up X-Forwarded-For {remote}
        header_up X-Forwarded-Proto {scheme}
    }
    tls $CF_CERT $CF_KEY
    encode gzip
}
EOF

  # set safe permissions so caddy can read the cert
  chown root:caddy "$CF_CERT" "$CF_KEY" || true
  chmod 640 "$CF_CERT" "$CF_KEY" || true

else
  echo "Cloudflare origin cert/key not found at $CF_CERT and $CF_KEY."
  echo "Writing fallback Caddyfile (note: with Cloudflare proxy enabled, ACME may fail)."
  cat > "$CADDYFILE_PATH" <<'EOF'
# Fallback Caddyfile — if Cloudflare proxy is ON, automatic Let's Encrypt may not work.
# Recommended: create Cloudflare Origin CA cert and place at /etc/ssl/cf_origin.pem (+ key).
api.metromoviez.ir {
     route /* {
            uri strip_prefix /
            reverse_proxy localhost:5159/api{uri}
     }
    encode gzip
}

public.metromoviez.ir {
    reverse_proxy localhost:5159/public{uri}
    tls $CF_CERT $CF_KEY
    encode gzip
}


qbittorrent.metromoviez.ir {
    reverse_proxy localhost:8083 {
        header_up Host {host}
        header_up X-Forwarded-For {remote}
        header_up X-Forwarded-Proto {scheme}
    }
    encode gzip
}

prowlarr.metromoviez.ir {
    reverse_proxy localhost:9696 {
        header_up Host {host}
        header_up X-Forwarded-For {remote}
        header_up X-Forwarded-Proto {scheme}
    }
    encode gzip
}
EOF
  echo
  echo "If Cloudflare proxy is enabled (orange cloud), create an Origin CA cert in Cloudflare and upload it to $CF_CERT and $CF_KEY, then re-run 'systemctl restart caddy'."
fi

# reload caddy to apply new config
echo "Reloading caddy to apply new Caddyfile..."
systemctl reload caddy || systemctl restart caddy || true


#### 4) Install Prowlarr (latest linux-core-x64 release) ####
echo
echo "=== Prowlarr installation ==="
PROWLARR_USER=prowlarr
PROWLARR_HOME=/opt/prowlarr
PROWLARR_DATA=/var/lib/prowlarr

if ! id -u "$PROWLARR_USER" >/dev/null 2>&1; then
  useradd --system --home-dir "$PROWLARR_DATA" --shell /usr/sbin/nologin "$PROWLARR_USER" || true
fi

mkdir -p "$PROWLARR_HOME" "$PROWLARR_DATA"
chown -R "$PROWLARR_USER:$PROWLARR_USER" "$PROWLARR_HOME" "$PROWLARR_DATA" || true

PROWLARR_URL=$(curl -s "https://api.github.com/repos/Prowlarr/Prowlarr/releases/latest" \
  | jq -r '.assets[] | select(.name | test("linux-core-x64.tar.gz$")) | .browser_download_url' \
  | head -n1)

if [ -z "$PROWLARR_URL" ] || [ "$PROWLARR_URL" = "null" ]; then
  echo "Could not determine Prowlarr download URL. Please check GitHub releases manually." >&2
else
  echo "Downloading Prowlarr from $PROWLARR_URL"
  tmpfile=$(mktemp)
  curl -L "$PROWLARR_URL" -o "$tmpfile"
  tar -xzf "$tmpfile" -C "$PROWLARR_HOME" --strip-components=1
  rm -f "$tmpfile"
  chown -R "$PROWLARR_USER:$PROWLARR_USER" "$PROWLARR_HOME" "$PROWLARR_DATA"

  # detect run method
  if [ -x "$PROWLARR_HOME/Prowlarr" ]; then
    PROWLARR_EXEC="$PROWLARR_HOME/Prowlarr"
  elif [ -f "$PROWLARR_HOME/Prowlarr.dll" ]; then
    PROWLARR_EXEC="/usr/bin/dotnet $PROWLARR_HOME/Prowlarr.dll"
  else
    echo "Warning: could not find Prowlarr executable in $PROWLARR_HOME" >&2
    PROWLARR_EXEC=""
  fi

  if [ -n "$PROWLARR_EXEC" ]; then
    cat > /etc/systemd/system/prowlarr.service <<EOF
[Unit]
Description=Prowlarr Daemon
After=network.target
StartLimitIntervalSec=0
StartLimitBurst=0

[Service]
User=${PROWLARR_USER}
Group=${PROWLARR_USER}
Type=simple
ExecStart=${PROWLARR_EXEC} --nobrowser --data=${PROWLARR_DATA}
Restart=always
RestartSec=5
LimitNOFILE=65536

[Install]
WantedBy=multi-user.target
EOF
    systemctl daemon-reload
    systemctl enable --now prowlarr || true
  fi
fi

#### Run ffmpeg installer now ####
install_ffmpeg || echo "ffmpeg installation reported failure."

#### Summary ###
echo
echo "=== Summary of services ==="
systemctl is-active --quiet prowlarr && echo "prowlarr active" || echo "prowlarr not active - check logs"
systemctl is-active --quiet qbittorrent-nox && echo "qbittorrent-nox active (webui port 8083)" || echo "qbittorrent-nox not active - check logs"
systemctl is-active --quiet caddy && echo "caddy active" || echo "caddy not active - check logs"

if command -v ffmpeg >/dev/null 2>&1; then
  echo "ffmpeg available: $(command -v ffmpeg)" && ffmpeg -version | head -n1
else
  echo "ffmpeg NOT available - please inspect the earlier logs." >&2
fi

echo
echo "Done. Check logs with"
echo "  journalctl -u qbittorrent-nox -f"
echo "  journalctl -u prowlarr -f"
echo "  journalctl -u caddy -f"

echo
echo "Notes"
echo " - qBittorrent web UI is configured on port 8083 in the systemd unit."
echo " - Both qBittorrent and Prowlarr systemd units use Restart=always and have rate-limits disabled to keep them restarting on failure."
echo " - ffmpeg will be installed via apt when possible; otherwise a static build is downloaded and binaries placed in /usr/local/bin and /etc/profile.d/ffmpeg.sh will ensure PATH contains /usr/local/bin."
