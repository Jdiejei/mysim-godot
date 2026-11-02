import re,json,os
g=json.load(open('guid2path.json')); mats={}; out={}
def mat_info(gd):
  if gd in mats: return
  p=g.get(gd); info={'path':p}; mats[gd]=info
  if not p or not os.path.exists(p): return
  t=open(p,errors='ignore').read()
  m=re.search(r'm_Name: (.*)',t); info['name']=m.group(1).strip() if m else ''
  m=re.search(r'm_Shader: \{[^}]*guid: ([0-9a-f]{32})',t); info['shader']=m.group(1) if m else None
  texs={}
  for mm in re.finditer(r'- (_\w+):\s+m_Texture: \{fileID: \d+, guid: ([0-9a-f]{32})',t):
    if mm.group(2) in g: texs[mm.group(1)]=g[mm.group(2)]
  info['texs']=texs
  # Preserve Unity texture-coordinate transforms. Older exports silently discarded these.
  transforms={}
  for block in re.finditer(r'- (_\w+):\s*\n(.*?)(?=\n\s*- _\w+:|\n\s*m_Ints:|\n\s*m_Floats:|\n\s*m_Colors:|\Z)',t,re.S):
    body=block.group(2)
    if 'm_Texture:' not in body: continue
    scale=re.search(r'm_Scale: \{x: ([\d.eE+-]+), y: ([\d.eE+-]+)\}',body)
    offset=re.search(r'm_Offset: \{x: ([\d.eE+-]+), y: ([\d.eE+-]+)\}',body)
    if scale and offset: transforms[block.group(1)]={'scale':[float(v) for v in scale.groups()],'offset':[float(v) for v in offset.groups()]}
  info['texTransforms']=transforms
  info['textureTransformSource']='Actual source Unity m_TextureEnvs m_Scale/m_Offset; absent entries are unknown, not assumed identity'
  for key in ('_BaseMap','_MainTex'):
    if key in texs: info['tex']=texs[key];break
  cols={}
  for mm in re.finditer(r'- (_\w+): \{r: ([\d.e-]+), g: ([\d.e-]+), b: ([\d.e-]+), a: ([\d.e-]+)\}',t): cols[mm.group(1)]=[float(x) for x in mm.groups()[1:]]
  info['colors']=cols
  fl={}
  for mm in re.finditer(r'- (_\w+): ([\d.e-]+)\n',t): fl[mm.group(1)]=float(mm.group(2))
  info['floats']=fl
  m=re.search(r'm_CustomRenderQueue: (-?\d+)',t); info['queue']=int(m.group(1)) if m else -1
for i in range(24):
  t=open(f'assets/Resources/RealRacing/Car{i}.prefab',errors='ignore').read()
  docs=re.split(r'\n--- !u!(\d+) &(\d+)\n',t); names={}; rend={}
  for k in range(1,len(docs)-2,3):
    cls,fid,body=docs[k],docs[k+1],docs[k+2]
    if cls=='1': names[fid]=re.search(r'm_Name: (.*)',body).group(1).strip()
    elif cls=='23' or cls=='137':
      go=re.search(r'm_GameObject: \{fileID: (\d+)\}',body).group(1)
      rend.setdefault(go,[]).append(re.findall(r'- \{fileID: \d+, guid: ([0-9a-f]{32}), type: 2\}',body))
  byname={}
  for go,ls in rend.items():
    for l in ls:
      byname.setdefault(names.get(go,''),[]).append(l)
      for gd in l: mat_info(gd)
  out[str(i)]=byname
json.dump({'cars':out,'materials':mats},open('assets/car_map.json','w'))
print('cars',len(out),'materials',len(mats),'with tex',sum(1 for m in mats.values() if m.get('tex')))
ks={}
for m in mats.values():
  for k in m.get('texs',{}): ks[k]=ks.get(k,0)+1
print(ks)
