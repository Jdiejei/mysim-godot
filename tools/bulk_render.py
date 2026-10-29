"""Bulk render + review: renders every track (default: Ferrari F1, AI drive, chase camera) windowless and builds
one mosaic of all 60 plus labelled 4-up sheets. Usage: python tools/bulk_render.py [car] [seconds]"""
import os, subprocess, sys
sys.path.insert(0, os.path.dirname(__file__))
from hidden import run_hidden, sys, json, time
from PIL import Image, ImageDraw
car = sys.argv[1] if len(sys.argv) > 1 else '16'; secs = sys.argv[2] if len(sys.argv) > 2 else '10'
G = open('.godotpath').read().strip(); os.makedirs('bulk', exist_ok=True)
names = [x['name'] for x in json.load(open('assets/RealRacing/TrackList.json', encoding='utf-8-sig'))['tracks']]
for t in range(60):
    out = os.path.abspath(f'bulk/t{t}.png')
    env = dict(os.environ, MYSIM_TRACK=str(t), MYSIM_CAR=car, MYSIM_AI='1', MYSIM_AUTODRIVE='1', MYSIM_NOPIT='1', MYSIM_RIVALS='0', MYSIM_SECONDS=secs, MYSIM_SHOT=out)
    run_hidden([G, '--path', '.', '--resolution', '960x540', '--no-window', '--audio-driver', 'Dummy'], env=env, timeout=400)
tile = (384, 216); sheet = Image.new('RGB', (tile[0] * 10, tile[1] * 6)); d = ImageDraw.Draw(sheet)
for t in range(60):
    f = f'bulk/t{t}.png'
    if os.path.exists(f):
        im = Image.open(f).convert('RGB').resize(tile); x, y = (t % 10) * tile[0], (t // 10) * tile[1]; sheet.paste(im, (x, y)); d.rectangle((x, y + tile[1] - 14, x + tile[0], y + tile[1]), fill=(0, 0, 0)); d.text((x + 3, y + tile[1] - 13), f'{t} {names[t][:32]}', fill=(255, 255, 255))
sheet.save('bulk/mosaic_all60.png'); print('done')
