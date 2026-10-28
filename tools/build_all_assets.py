import os,re,json,shutil,sys
U='D:/MySim-recovered/full-game-build/project/Assets/'
R=U+'RealRacing/'; TEX=R+'RecoveredNativeTextures/'
out='assets/'
maps=sorted({t['map'] for t in json.load(open(out+'RealRacing/TrackList.json'))['tracks']})
# drive surfaces
os.makedirs(out+'Resources',exist_ok=True)
for f in os.listdir(U+'Resources/DriveSurfaces'):
  if f.endswith('.txt'): shutil.copy(U+'Resources/DriveSurfaces/'+f,out+'Resources/'+f)
def index(sub):
  idx={}
  for d,_,fs in os.walk(TEX+sub):
    for f in fs:
      if f.endswith('.png'):
        s=f[:-4].lower()
        if s.endswith(('_norm','_nrm','_mask','_spec')): continue
        idx.setdefault(s,os.path.join(d,f))
  return idx
common=index('common'); log=[]
for mp in maps:
  idx=dict(common); idx.update(index(mp))
  mdir=R+'TrackMaterials/'+mp
  if not os.path.isdir(mdir): log.append(f'{mp}: no TrackMaterials');continue
  m={};n=0
  for f in os.listdir(mdir):
    if not f.endswith('.mat'): continue
    name=re.search(r'm_Name: (.*)',open(mdir+'/'+f,errors='ignore').read()).group(1).strip()
    base=re.sub(r'_(lm|\d{2}_\d{3})$','',name.lower()); parts=re.split(r'[-_]',base); found=None
    for i in range(len(parts)):
      cand='_'.join(parts[i:])
      for c in (cand,'gnd_'+cand):
        if c in idx: found=idx[c];break
      if found:break
    if found: m[name]=found
  od=out+mp+'/'; os.makedirs(od+'textures',exist_ok=True); res={}
  for k,t in m.items():
    rel=os.path.basename(t); d=od+'textures/'+rel
    if not os.path.exists(d): shutil.copy(t,d)
    res[k]=rel
  json.dump(res,open(od+'tex_by_material.json','w'))
  log.append(f'{mp}: {len(res)} matched of {len(os.listdir(mdir))//2} materials, {len(os.listdir(od+"textures"))} textures')
  print(log[-1],flush=True)
print('ALLDONE')
