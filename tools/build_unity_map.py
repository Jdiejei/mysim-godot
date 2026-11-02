import os,re,json,sys,pathlib
U='D:/MySim-recovered/full-game-build/project/Assets/'
R=U+'RealRacing/'
P='assets/Resources/RealRacing/'
if not pathlib.Path(R).is_dir():
 raise FileNotFoundError('Recovered source material root unavailable; existing unity_map.json must be preserved: '+R)
guid2path={}
def scan(base):
  for d,_,fs in os.walk(base):
    for f in fs:
      if f.endswith('.meta'):
        try: t=open(os.path.join(d,f),errors='ignore').read(600)
        except: continue
        m=re.search(r'guid: ([0-9a-f]{32})',t)
        if m: guid2path[m.group(1)]=(os.path.join(d,f)[:-5]).replace(chr(92),'/')
for sub in ('Source','TrackMaterials','Materials','Shaders','RecoveredNativeMaterials','RecoveredNativeTextures'):
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
  # Keep custom shader inputs even when they are not URP's _BaseMap/_Surface.
  # Preservation is evidence, never permission to invent a runtime blend mode.
  info['textureEnvironments']={}
  for match in re.finditer(r'- (\w+):\s*\n\s*m_Texture: \{([^}]*)\}\s*\n\s*m_Scale: \{x: ([^,]+), y: ([^}]+)\}\s*\n\s*m_Offset: \{x: ([^,]+), y: ([^}]+)\}',t):
   key,reference,sx,sy,ox,oy=match.groups();guid=re.search(r'guid: ([0-9a-f]{32})',reference)
   info['textureEnvironments'][key]={'guid':guid.group(1) if guid else None,'path':guid2path.get(guid.group(1)) if guid else None,'scale':[float(sx),float(sy)],'offset':[float(ox),float(oy)]}
  info['savedFloats']={key:float(value) for key,value in re.findall(r'^\s*- (\w+): ([-+\d.eE]+)\s*$',t,re.M)}
  info['savedColors']={key:[float(r),float(g),float(b),float(a)] for key,r,g,b,a in re.findall(r'- (\w+): \{r: ([-+\d.eE]+), g: ([-+\d.eE]+), b: ([-+\d.eE]+), a: ([-+\d.eE]+)\}',t)}
  shader_path=guid2path.get(info['shader'])
  if shader_path and os.path.isfile(shader_path):
   import hashlib
   shader_source=pathlib.Path(shader_path).read_text(errors='replace')
   info['customShaderEvidence']={'path':shader_path,'sha256':hashlib.sha256(pathlib.Path(shader_path).read_bytes()).hexdigest(),'blendDirectives':re.findall(r'^\s*Blend\s+([^\r\n]+)',shader_source,re.M),'zWriteDirectives':re.findall(r'^\s*ZWrite\s+([^\r\n]+)',shader_source,re.M),'runtimeReconstructionVerified':False}
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
if not out or not mats:raise RuntimeError('Incomplete source mapping; refusing to replace existing unity_map.json')
missing=[guid for guid,material in mats.items() if not material.get('path') or not pathlib.Path(material['path']).is_file()]
if missing:raise RuntimeError(f'{len(missing)} referenced source materials unavailable; preserve existing map until complete inputs are restored')
target=pathlib.Path('assets/unity_map.json');temporary=target.with_suffix('.json.pending')
temporary.write_text(json.dumps({'tracks':out,'materials':mats}),encoding='utf-8');os.replace(temporary,target)
have=sum(1 for m in mats.values() if m.get('tex')); print('materials',len(mats),'with texture',have)
print('ALLDONE')
