import os,re,json,sys
U='D:/MySim-recovered/full-game-build/project/Assets/'
R=U+'RealRacing/'
P='assets/Resources/RealRacing/'
guid2path={}
def scan(base):
  for d,_,fs in os.walk(base):
    for f in fs:
      if f.endswith('.meta'):
        try: t=open(os.path.join(d,f),errors='ignore').read(600)
        except: continue
        m=re.search(r'guid: ([0-9a-f]{32})',t)
        if m: guid2path[m.group(1)]=(os.path.join(d,f)[:-5]).replace(chr(92),'/')
for sub in ('Source','TrackMaterials','Materials','RecoveredNativeMaterials','RecoveredNativeTextures'):
  scan(R+sub); print('scanned',sub,len(guid2path),flush=True)
json.dump(guid2path,open('guid2path.json','w'))
tl=json.load(open('assets/RealRacing/TrackList.json'))['tracks']
variants=sorted({t['baseTrackIndex'] for t in tl}); mats={}; out={}
def mat_info(g):
  if g in mats: return
  p=guid2path.get(g); info={'path':p}; mats[g]=info
  if not p or not os.path.exists(p): return
  t=open(p,errors='ignore').read()
  m=re.search(r'm_Name: (.*)',t); info['name']=m.group(1).strip() if m else ''
  m=re.search(r'm_Shader: \{[^}]*guid: ([0-9a-f]{32})',t); info['shader']=m.group(1) if m else None
  for key in ('_BaseMap','_MainTex'):
    m=re.search(r'- '+key+r':\s+m_Texture: \{fileID: \d+, guid: ([0-9a-f]{32})',t)
    if m and m.group(1) in guid2path: info['tex']=guid2path[m.group(1)]; break
  m=re.search(r'- _BaseColor: \{r: ([\d.e-]+), g: ([\d.e-]+), b: ([\d.e-]+), a: ([\d.e-]+)',t)
  if m: info['color']=[float(x) for x in m.groups()]
  m=re.search(r'- _Cutoff: ([\d.e-]+)',t); info['cutoff']=float(m.group(1)) if m else 0
  m=re.search(r'- _AlphaClip: (\d)',t); info['alphaClip']=bool(m and m.group(1)=='1')
  m=re.search(r'- _Surface: (\d)',t); info['transparent']=bool(m and m.group(1)=='1')
  m=re.search(r'm_CustomRenderQueue: (-?\d+)',t); info['queue']=int(m.group(1)) if m else -1
for v in variants:
  f=P+f'Track{v}.prefab'
  if not os.path.exists(f): print('no prefab',f); continue
  txt=open(f,errors='ignore').read()
  docs=re.split(r'\n--- !u!(\d+) &(\d+)\n',txt); names={}; rend={}
  for i in range(1,len(docs)-2,3):
    cls,fid,body=docs[i],docs[i+1],docs[i+2]
    if cls=='1':
      m=re.search(r'm_Name: (.*)',body); names[fid]=m.group(1).strip() if m else ''
    elif cls=='23':
      go=re.search(r'm_GameObject: \{fileID: (\d+)\}',body).group(1)
      mm=re.findall(r'- \{fileID: \d+, guid: ([0-9a-f]{32}), type: 2\}',body)
      rend.setdefault(go,[]).append(mm)
  byname={}
  for go,lists in rend.items():
    for l in lists:
      byname.setdefault(names.get(go,''),[]).append(l)
      for g in l: mat_info(g)
  out[str(v)]=byname; print('track',v,len(byname),'names',flush=True)
json.dump({'tracks':out,'materials':mats},open('assets/unity_map.json','w'))
have=sum(1 for m in mats.values() if m.get('tex')); print('materials',len(mats),'with texture',have)
print('ALLDONE')
