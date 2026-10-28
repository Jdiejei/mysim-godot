#!/bin/bash
cd /c/Users/shash/Documents/MySimGodot
G="$(cat .godotpath | tr -d '\r')"
export PATH="$PATH:/c/Program Files/dotnet"
rm -rf build/*
timeout 3400 "$G" --headless --path . --export-release "Windows Desktop" build/MySim.exe > export5.log 2>&1
echo EXPORTDONE >> export5.log
