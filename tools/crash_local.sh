#!/bin/bash
cd /c/Users/shash/Documents/MySimGodot
G="$(cat .godotpath | tr -d '\r')"
mkdir -p crashmap
for t in $(seq 0 59); do
  MYSIM_CRASHMAP=$t MYSIM_CRASHPTS=24 MYSIM_CRASHOUT="C:\Users\shash\Documents\MySimGodot\crashmap\crash_$t.jsonl" timeout 900 "$G" --headless --path . 2>&1 | grep -E "CRASHBATCH|SCRIPT ERROR" >> crashmap/campaign.log
done
echo CAMPAIGNDONE >> crashmap/campaign.log
