@echo off
setlocal
cd /d "%~dp0"
rem Open the actual main menu. The Silverstone launcher is a direct-race diagnostic shortcut.
set MYSIM_TRACK=
set MYSIM_CAR=
set MYSIM_AI=
set MYSIM_AUTODRIVE=
set MYSIM_FLOWCHECKS=
set MYSIM_RUNTIME_CHECKS=
set MYSIM_INPUTCHECKS=
for /f "usebackq delims=" %%G in (".godotpath") do start "" "%%G" --path . --resolution 1600x900 --windowed
endlocal
