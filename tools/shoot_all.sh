#!/bin/bash
cd /c/Users/shash/Documents/MySimGodot
G="$(cat .godotpath | tr -d '\r')"
: > shots/log.txt
for t in $(seq 0 59); do
  MYSIM_TRACK=$t MYSIM_CAR=$((t % 24)) MYSIM_AUTODRIVE=1 MYSIM_SECONDS=5 MYSIM_SHOT="C:\Users\shash\Documents\MySimGodot\shots\t$t.png" timeout 240 "$G" --path . --resolution 960x540 --no-window 2>&1 | grep -E "TRACKBIND bound|WALLS|DRIVETEST|DRIVESURFACE|SCRIPT ERROR|ERROR: (Cannot|Failed|Condition)" | sed "s/^/t$t: /" >> shots/log.txt
  [ -f shots/t$t.png ] || echo "t$t: NO SHOT" >> shots/log.txt
done
echo SHOOTDONE >> shots/log.txt
