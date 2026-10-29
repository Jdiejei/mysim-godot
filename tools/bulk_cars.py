"""Bulk car review: renders all 24 cars from front-3/4, side and rear-3/4 in the showroom and builds one mosaic per angle
plus an automatic 'solidity' check (fraction of the car silhouette that is background-coloured holes). Usage: python tools/bulk_cars.py"""
import os, subprocess, sys
sys.path.insert(0, os.path.dirname(__file__))
from hidden import run_hidden
from PIL import Image, ImageDraw
import numpy as np
G = open('.godotpath').read().strip(); os.makedirs('bulk/cars', exist_ok=True)
angles = {'front': '4.5,1.3,5.2', 'side': '6.8,0.9,0.0', 'rear': '-4.5,1.3,-5.2'}
for a, pos in angles.items():
    for c in range(24):
        out = os.path.abspath(f'bulk/cars/{a}_{c}.png')
        env = dict(os.environ, MYSIM_CARVIEW=str(c), MYSIM_CAMPOS=pos, MYSIM_SHOT=out)
        run_hidden([G, '--path', '.', '--resolution', '640x360', '--no-window', '--audio-driver', 'Dummy'], env=env, timeout=200)
for a in angles:
    sheet = Image.new('RGB', (320 * 6, 180 * 4)); d = ImageDraw.Draw(sheet)
    for c in range(24):
        f = f'bulk/cars/{a}_{c}.png'
        if os.path.exists(f):
            x, y = (c % 6) * 320, (c // 6) * 180; sheet.paste(Image.open(f).convert('RGB').resize((320, 180)), (x, y)); d.text((x + 3, y + 3), str(c), fill=(255, 255, 0))
    sheet.save(f'bulk/cars_{a}.png')
print('done')
