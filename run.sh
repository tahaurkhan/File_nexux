#!/bin/bash
set -e
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

echo "========================================="
echo "   Building and Launching FileNexus UI   "
echo "========================================="

dotnet run --project src/FileNexus.UI/FileNexus.UI.csproj
