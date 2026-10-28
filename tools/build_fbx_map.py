import json,shutil,os
mats=json.load(open('assets/BH/materials.json')); match=json.load(open('assets/BH/texture_match.json'))
fbx=[l.rstrip('\r\n') for l in open('assets/BH/fbx_materials.txt',encoding='utf-8-sig') if l.strip()]
out={};os.makedirs('assets/BH/textures',exist_ok=True)
for f in fbx:
  best=None
  for u,t in match.items():
    if t and (u==f or u.startswith(f+'_') or u.startswith(f)): best=t;break
  col=None
  for u,m in mats.items():
    if u.startswith(f) and m.get('color'): col=m['color'];break
  rel=None
  if best:
    rel='textures/'+os.path.basename(best)
    if not os.path.exists('assets/BH/'+rel): shutil.copy(best,'assets/BH/'+rel)
  out[f]={'tex':rel,'color':col}
json.dump(out,open('assets/BH/fbx_material_map.json','w'))
print(len(fbx),'fbx mats;',sum(1 for v in out.values() if v['tex']),'textured;',sum(1 for v in out.values() if v['color'] and not v['tex']),'colour-only')
