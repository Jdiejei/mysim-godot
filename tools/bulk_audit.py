"""Bulk texture audit: finds every unique ground-type texture, scores it for baked streaks/skid marks,
and writes labelled contact sheets so everything can be reviewed in a handful of images.
Usage: python tools/bulk_audit.py   (writes audit/ground_XX.png and audit/ground_scores.csv)"""
import os, sys, csv, hashlib
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
root = 'assets'; out = 'audit'; os.makedirs(out, exist_ok=True)
seen = {}; rows = []
for dp, dn, fn in os.walk(root):
    for f in fn:
        l = f.lower()
        if not l.endswith('.png') or not (l.startswith(('gnd_', 'overlay_road', 'mat_road', 'road_')) ): continue
        p = os.path.join(dp, f)
        try:
            h = hashlib.md5(open(p, 'rb').read(200000)).hexdigest()
        except Exception: continue
        if h in seen: continue
        seen[h] = p
for h, p in seen.items():
    try:
        im = Image.open(p).convert('RGB'); a = np.asarray(im.resize((256, 256)), dtype=np.float32).mean(axis=2)
    except Exception: continue
    lo = np.asarray(Image.fromarray(a.astype(np.uint8)).filter(ImageFilter.GaussianBlur(10)), dtype=np.float32)
    # streak score: strong large-scale dark lines = low-pass residual relative to mean, anisotropy of gradients
    gx = np.abs(np.diff(lo, axis=1)).mean(); gy = np.abs(np.diff(lo, axis=0)).mean()
    contrast = lo.std() / max(1.0, lo.mean())
    rows.append((os.path.basename(p), p, round(float(contrast), 4), round(float(max(gx, gy) / max(0.01, min(gx, gy))), 2), round(float(a.mean()), 1)))
rows.sort(key=lambda r: -r[2])
with open(os.path.join(out, 'ground_scores.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['name', 'path', 'lowfreq_contrast', 'anisotropy', 'mean']); w.writerows(rows)
tile, cols, per = 224, 6, 24
for k in range(0, len(rows), per):
    chunk = rows[k:k + per]; rws = (len(chunk) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * tile, rws * (tile + 18)), (20, 20, 20)); d = ImageDraw.Draw(sheet)
    for i, r in enumerate(chunk):
        try: im = Image.open(r[1]).convert('RGB').resize((tile, tile))
        except Exception: continue
        x, y = (i % cols) * tile, (i // cols) * (tile + 18); sheet.paste(im, (x, y + 18))
        d.text((x + 2, y + 2), f'{r[0][:30]} c={r[2]}', fill=(255, 255, 0))
    sheet.save(os.path.join(out, f'ground_{k // per:02d}.png'))
print('unique ground textures', len(rows), 'sheets', (len(rows) + per - 1) // per)
