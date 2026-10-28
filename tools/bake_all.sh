#!/bin/bash
cd /c/Users/shash/Documents/MySimGodot
G="$(cat .godotpath | tr -d '\r')"
for t in $(cat colbake/firsts.txt); do
  MAP=$(python -c "import json;print(json.load(open('assets/RealRacing/TrackList.json'))['tracks'][$t]['map'])")
  MYSIM_BAKECOL=$t MYSIM_BAKEOUT="C:\Users\shash\Documents\MySimGodot\colbake\col_$MAP.bin" timeout 900 "$G" --headless --path . 2>&1 | grep -E "BAKECOL|SCRIPT ERROR" >> colbake/bake.log
done
echo BAKEDONE >> colbake/bake.log
