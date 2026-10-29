"""Bulk de-streak: replaces baked tyre-skid streaks in road/asphalt textures with clean surface that keeps the
texture's own average colour and gets its fine grain from the clean 'noskids' asphalt. Originals go to audit/orig/.
Usage: python tools/bulk_declean.py [--dry]"""
import os, re, sys, shutil, hashlib
import numpy as np
from PIL import Image, ImageFilter
root = 'assets'; bak = 'audit/orig'; dry = '--dry' in sys.argv
pat = re.compile(r'^(gnd_asphalt_0\d|gnd_asphalt_dusty_0\d|road_asphalt_\d+|road_main_[a-z]|road_airport_\d+|gnd_road_[a-z_]*dark[a-z_]*|gnd_road_\d+)\.png$', re.I)
clean = None
for dp, dn, fn in os.walk(root):
    if 'gnd_asphalt_noskids_01.png' in fn: clean = os.path.join(dp, 'gnd_asphalt_noskids_01.png'); break
g = Image.open(clean).convert('RGB'); ga = np.asarray(g, dtype=np.float32)
hp = ga - np.asarray(g.filter(ImageFilter.GaussianBlur(6)), dtype=np.float32)  # fine grain only
done = 0; skipped = 0
for dp, dn, fn in os.walk(root):
    for f in fn:
        if not pat.match(f): continue
        p = os.path.join(dp, f)
        try: im = Image.open(p)
        except Exception: continue
        rgb = im.convert('RGB'); a = np.asarray(rgb, dtype=np.float32)
        mean = a.reshape(-1, 3).mean(axis=0)
        if a.mean() > 150 or (a.max(axis=2) - a.min(axis=2)).mean() > 30: skipped += 1; continue  # not asphalt-like
        h, w = a.shape[:2]
        grain = np.asarray(Image.fromarray(np.clip(hp + 128, 0, 255).astype(np.uint8)).resize((w, h), Image.LANCZOS), dtype=np.float32) - 128
        # keep only very-low-frequency tone from the original (patchiness), drop its lines
        low = np.asarray(rgb.filter(ImageFilter.GaussianBlur(max(w, h) / 6.0)), dtype=np.float32)
        out = np.clip(0.55 * low + 0.45 * mean + grain * 1.0, 0, 255).astype(np.uint8)
        res = Image.fromarray(out)
        if im.mode == 'RGBA': res.putalpha(im.getchannel('A'))
        if not dry:
            b = os.path.join(bak, p.replace(os.sep, '__'))
            os.makedirs(bak, exist_ok=True)
            if not os.path.exists(b): shutil.copyfile(p, b)
            res.save(p)
        done += 1
print('declean', 'DRY' if dry else 'APPLIED', 'files', done, 'skipped(non-asphalt)', skipped)
