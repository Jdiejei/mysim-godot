@echo off
rem Starts MySim on Silverstone with the Ferrari F1 and 5 AI rivals, 3 laps, in a normal window.
rem Close the game window yourself when done (Esc pauses, Esc again/menu button returns to the menu).
cd /d "%~dp0"
set MYSIM_TRACK=24
set MYSIM_CAR=16
set MYSIM_LAPS=3
set MYSIM_RIVALS=5
start "" "C:\Users\shash\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe" --path . --resolution 1600x900 --windowed
