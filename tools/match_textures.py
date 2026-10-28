import os,json,re,sys,shutil
T='D:/MySim-recovered/full-game-build/project/Assets/RealRacing/RecoveredNativeTextures/'
track=sys.argv[1]
idx={}
for sub in ('common',track):
  for d,_,fs in os.walk(T+sub):
    for f in fs:
      if f.endswith('.png') and 'lightmap' not in d.lower() and 'shadowmap' not in d.lower():
        s=f[:-4].lower()
        if s.endswith('_norm') or s.endswith('_nrm') or s.endswith('_mask') or s.endswith('_spec'): continue
        idx.setdefault(s,os.path.join(d,f))
mats=json.load(open('assets/BH/materials.json'))
res={};hit=0
for name in mats:
  base=name.lower()
  # strip trailing lightmap/variant suffixes
  base=re.sub(r'_(lm|\d{2}_\d{3})$','',base)
  parts=re.split(r'[-_]',base)
  found=None
  for i in range(len(parts)):
    cand='_'.join(parts[i:])
    for c in (cand,'gnd_'+cand):
      if c in idx: found=idx[c];break
    if found:break
  res[name]=found
  if found:hit+=1
json.dump(res,open('assets/BH/texture_match.json','w'))
print(hit,'of',len(mats),'matched by name; index',len(idx))
