#!/usr/bin/env bash
set -e

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# Let Playwright know where to find its driver
export PLAYWRIGHT_DRIVER_SEARCH_PATH="$SCRIPT_DIR"

# Run the Playwright driver DLL with dotnet
exec dotnet "$SCRIPT_DIR/Microsoft.Playwright.dll" "$@"