#!usrbinenv bash
# install-media-stack-ubuntu-new.sh
# Installs dotnet 9 (preferred via distro packages, fallback to dotnet-install),
# Playwright (.NET CLI + browsers), Prowlarr, qbittorrent-nox (webui port 8083),
# and Caddy.
# Target Ubuntu 22.04  24.04  newer (auto-detected)
# Run as root (sudo)

set -euo pipefail
export DEBIAN_FRONTEND=noninteractive

if [ $(id -u) -ne 0 ]; then
  echo This script must be run as root. Use sudo.
  exit 1
fi

# Basic env
ARCH=x64
UBUNTU_CODENAME=$(lsb_release -cs  echo unknown)
UBUNTU_VERSION=$(lsb_release -rs  echo 0.0)
echo Detected Ubuntu codename=${UBUNTU_CODENAME} version=${UBUNTU_VERSION}

apt update
apt -y upgrade
apt install -y wget curl gnupg ca-certificates lsb-release software-properties-common apt-transport-https jq unzip pwsh

#### 1) Install .NET 9 ####
echo
echo === .NET 9 installation ===
# Try installing dotnet-sdk-9.0 from distro packages (preferred on 22.04+ where supported)
if apt-cache show dotnet-sdk-9.0 devnull 2&1; then
  echo dotnet-sdk-9.0 available in apt - installing...
  apt update
  apt install -y dotnet-sdk-9.0  true
fi

# If not installed, attempt canonical backports or fallback to Microsoft's dotnet-install script
if ! command -v dotnet devnull 2&1; then
  echo dotnet not found via apt — attempting canonicalubuntu dotnet backports and then dotnet-install as fallback...
  # Attempt canonical backports (Ubuntu packaging)
  if ! apt-get install -y dotnet-sdk-9.0 devnull 2&1; then
    echo apt install failed or package not present — using dotnet-install.sh fallback...
    curl -fsSL httpsdot.netv1dotnet-install.sh -o tmpdotnet-install.sh
    chmod +x tmpdotnet-install.sh
    DOTNET_INSTALL_DIR=usrsharedotnet
    mkdir -p $DOTNET_INSTALL_DIR
    tmpdotnet-install.sh --channel 9.0 --install-dir $DOTNET_INSTALL_DIR --architecture x64
    ln -sf $DOTNET_INSTALL_DIRdotnet usrbindotnet  true
  fi
fi

echo dotnet info
if command -v dotnet devnull 2&1; then
  dotnet --info  sed -n '1,6p'  true
else
  echo dotnet installation failed - please check manually.
fi

#### 2) Install Playwright (.NET) and browsers ####
echo
echo === Playwright (.NET) installation ===
PLAYWRIGHT_TOOL_PATH=usrlocalbin
mkdir -p $PLAYWRIGHT_TOOL_PATH

# Install Playwright CLI tool (system-wide)
if ! command -v $PLAYWRIGHT_TOOL_PATHplaywright devnull 2&1; then
  dotnet tool install --tool-path $PLAYWRIGHT_TOOL_PATH Microsoft.Playwright.CLI  true
  chmod +x $PLAYWRIGHT_TOOL_PATHplaywright  true
fi

if command -v $PLAYWRIGHT_TOOL_PATHplaywright devnull 2&1; then
  PW=$PLAYWRIGHT_TOOL_PATHplaywright
  echo Running playwright install-deps (may call apt).
  # install-deps may use apt so run as root (we are root)
  $PW install-deps  true

  echo Installing browsers (Chromium, Firefox, WebKit) via Playwright CLI...
  $PW install  true
else
  echo Playwright CLI not found. You may need to install Microsoft.Playwright.CLI manually or use the dotnet package in a project and run the generated playwright.ps1 install commands.
fi

#### 3) Install qBittorrent-nox (headless) ####
echo
echo === qBittorrent-nox installation (webui port 8083) ===
# Add PPA (if available) for up-to-date qBittorrent; if add-apt-repository missing it's provided above
if ! grep -q qbittorrent-team etcaptsources.list.d 2devnull  true; then
  add-apt-repository -y ppaqbittorrent-teamqbittorrent-stable  true
fi
apt update
apt install -y qbittorrent-nox  true

# create system user  home
QB_USER=qbittorrent
QB_HOME=varlibqbittorrent
if ! id -u $QB_USER devnull 2&1; then
  useradd --system --home-dir $QB_HOME --shell usrsbinnologin $QB_USER  true
fi
mkdir -p $QB_HOME
chown -R $QB_USER$QB_USER $QB_HOME

# systemd service always restart, disable start rate limiting
cat  etcsystemdsystemqbittorrent-nox.service 'EOF'
[Unit]
Description=qBittorrent (nox)
After=network.target
StartLimitIntervalSec=0
StartLimitBurst=0

[Service]
User=qbittorrent
Group=qbittorrent
Type=simple
ExecStart=usrbinqbittorrent-nox --webui-port=8083
Restart=always
RestartSec=5
LimitNOFILE=65536

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable --now qbittorrent-nox  true

#### 4) Install Caddy (official repo) ####
echo
echo === Caddy installation ===
# Add Caddy GPG key and repo
curl -1sLf 'httpsdl.cloudsmith.iopubliccaddystablegpg.key'  gpg --dearmor -o usrsharekeyringscaddy-stable-archive-keyring.gpg
echo deb [signed-by=usrsharekeyringscaddy-stable-archive-keyring.gpg] httpsdl.cloudsmith.iopubliccaddystabledebubuntu ${UBUNTU_CODENAME} main  etcaptsources.list.dcaddy-stable.list
apt update
apt install -y caddy  true
systemctl enable --now caddy  true

#### 5) Install Prowlarr (latest linux-core-x64 release) ####
echo
echo === Prowlarr installation ===
PROWLARR_USER=prowlarr
PROWLARR_HOME=optprowlarr
PROWLARR_DATA=varlibprowlarr

if ! id -u $PROWLARR_USER devnull 2&1; then
  useradd --system --home-dir $PROWLARR_DATA --shell usrsbinnologin $PROWLARR_USER  true
fi

mkdir -p $PROWLARR_HOME $PROWLARR_DATA
chown -R $PROWLARR_USER$PROWLARR_USER $PROWLARR_HOME $PROWLARR_DATA

PROWLARR_URL=$(curl -s httpsapi.github.comreposProwlarrProwlarrreleaseslatest 
   jq -r '.assets[]  select(.name  test(linux-core-x64.tar.gz$))  .browser_download_url'  head -n1)

if [ -z $PROWLARR_URL ]  [ $PROWLARR_URL = null ]; then
  echo Could not determine Prowlarr download URL. Please check GitHub releases manually.
else
  echo Downloading Prowlarr from $PROWLARR_URL
  tmpfile=$(mktemp)
  curl -L $PROWLARR_URL -o $tmpfile
  tar -xzf $tmpfile -C $PROWLARR_HOME --strip-components=1
  rm -f $tmpfile
  chown -R $PROWLARR_USER$PROWLARR_USER $PROWLARR_HOME $PROWLARR_DATA

  # detect run method
  if [ -x $PROWLARR_HOMEProwlarr ]; then
    PROWLARR_EXEC=$PROWLARR_HOMEProwlarr
  elif [ -f $PROWLARR_HOMEProwlarr.dll ]; then
    PROWLARR_EXEC=usrbindotnet $PROWLARR_HOMEProwlarr.dll
  else
    echo Warning could not find Prowlarr executable in $PROWLARR_HOME
    PROWLARR_EXEC=
  fi

  if [ -n $PROWLARR_EXEC ]; then
    cat  etcsystemdsystemprowlarr.service EOF
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
    systemctl enable --now prowlarr  true
  fi
fi

echo
echo === Summary of services ===
systemctl is-active --quiet prowlarr && echo prowlarr active  echo prowlarr not active  check logs
systemctl is-active --quiet qbittorrent-nox && echo qbittorrent-nox active (webui port 8083)  echo qbittorrent-nox not active  check logs
systemctl is-active --quiet caddy && echo caddy active  echo caddy not active  check logs

echo
echo Done. Check logs with
echo   journalctl -u qbittorrent-nox -f
echo   journalctl -u prowlarr -f
echo   journalctl -u caddy -f

echo
echo Notes
echo  - qBittorrent web UI is configured on port 8083 in the systemd unit.
echo  - Both qBittorrent and Prowlarr systemd units use Restart=always and have rate-limits disabled (StartLimitIntervalSec=0  StartLimitBurst=0) to keep them restarting on failure.
echo  - If a service continually crashes on start it will still be restarted by systemd indefinitely; in that case inspect logs to fix root cause instead of relying on restart behavior.
