#!/bin/bash
cd /c/Users/shash/Documents/MySimGodot
cmd //c "rmdir assets" 2>&1
cmd //c "robocopy D:\MySim-godot-assets\assets assets /E /NFL /NDL /NJH /NP /MT:8" > move_back.log 2>&1
echo COPYDONE >> move_back.log
G="$(cat .godotpath | tr -d '\r')"
export PATH="$PATH:/c/Program Files/dotnet"
rm -rf build/*
timeout 3400 "$G" --headless --path . --export-release "Windows Desktop" build/MySim.exe > export4.log 2>&1
echo EXPORTDONE >> export4.log
