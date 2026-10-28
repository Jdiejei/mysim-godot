#!/bin/bash
cd /c/Users/shash/Documents/MySimGodot
G="$(cat .godotpath | tr -d '\r')"
: > carshots/log.txt
for c in $(seq 0 23); do
  MYSIM_CARVIEW=$c MYSIM_SHOT="C:\Users\shash\Documents\MySimGodot\carshots\c$c.png" timeout 200 "$G" --path . --resolution 800x450 --position -4000,0 2>&1 | grep -E "SCRIPT ERROR|CARMAP|WHEELS" | sed "s/^/c$c: /" >> carshots/log.txt
done
echo CARSDONE >> carshots/log.txt
