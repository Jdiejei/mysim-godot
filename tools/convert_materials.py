import os,re,json,sys,shutil
R='D:/MySim-recovered/full-game-build/project/Assets/RealRacing/'
track=sys.argv[1]; out=sys.argv[2]
guid={}
for base in (R+'RecoveredNativeTextures', R+'RecoveredNativeMaterials', R+'Source/'+track, R+'TrackMaterials/'+track, R+'../Resources/RealRacing', R+'../ThirdParty'):
  for d,_,fs in os.walk(base):
    for f in fs:
      if f.endswith('.meta'):
        m=re.search(r'guid: ([0-9a-f]{32})',open(os.path.join(d,f),errors='ignore').read())
        if m: guid[m.group(1)]=os.path.join(d,f)[:-5]
mats={}
mdir=R+'TrackMaterials/'+track
for f in os.listdir(mdir):
  if not f.endswith('.mat'): continue
  t=open(mdir+'/'+f,errors='ignore').read()
  name=re.search(r'm_Name: (.*)',t).group(1).strip()
  tex=None
  for m in re.finditer(r'- (_BaseMap|_MainTex):\s+m_Texture: \{fileID: \d+, guid: ([0-9a-f]{32})',t):
    if m.group(2) in guid: tex=guid[m.group(2)];break
  col=re.search(r'- _BaseColor: \{r: ([\d.e-]+), g: ([\d.e-]+), b: ([\d.e-]+), a: ([\d.e-]+)',t)
  cut='_Cutoff' in t and re.search(r'- _AlphaClip: 1',t) is not None
  rel=None
  if tex and os.path.exists(tex):
    rel='textures/'+os.path.basename(tex)
    os.makedirs(out+'/textures',exist_ok=True); shutil.copy(tex,out+'/'+rel)
  mats[name]={'tex':rel,'color':[float(x) for x in col.groups()] if col else None,'alphaClip':bool(cut)}
json.dump(mats,open(out+'/materials.json','w'))
print(len(mats),'materials',sum(1 for v in mats.values() if v['tex']),'with textures; guid map',len(guid))
