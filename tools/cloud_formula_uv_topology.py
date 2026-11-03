"""Runner-only labelled atlas correspondence sheets; private source buffers remain encrypted."""
import json,pathlib,math
from PIL import Image,ImageDraw

def render(root,out):
 data=json.loads((out/'private-uv-topology/panels.json').read_text());assert data['car']==16 and len(data['rows'])==3
 rows=[]
 for panel in data['rows']:
  source=panel['texture'];assert source.startswith('res://assets/RealRacing/Source/2019_f1_ferrari_sf90/livery/')
  texture=root/source.removeprefix('res://');base=Image.open(texture).convert('RGB');base.thumbnail((1024,1024));w,h=base.size
  image=Image.new('RGB',(w*2,h+48),(25,25,25));draw=ImageDraw.Draw(image)
  for mode in range(2):
   image.paste(base,(mode*w,48));draw.text((mode*w+8,8),panel['name']+(' current(-u,-v)' if mode==0 else ' source(u,v)'),fill='white')
   for i in range(0,len(panel['indices']),3):
    ids=panel['indices'][i:i+3];uv=[panel['uv'][k] for k in ids];sign=-1 if mode==0 else 1
    coords=[(mode*w+(sign*u%1)*(w-1),48+(1-sign*v%1)*(h-1)) for u,v in uv]
    area=abs((uv[1][0]-uv[0][0])*(uv[2][1]-uv[0][1])-(uv[1][1]-uv[0][1])*(uv[2][0]-uv[0][0]))
    draw.line(coords+[coords[0]],fill=(255,80,80) if area<1e-10 else (60,220,245),width=1)
  path=out/(panel['name']+'-uv-correspondence.png');image.save(path);rows.append({'panel':panel['name'],'triangles':len(panel['indices'])//3,'image':path.name})
 (out/'formula-uv-topology-summary.json').write_text(json.dumps({'panels':rows,'scope':'Labelled correspondence only; seams crossing wrap are displayed as wrapped edges; not accepted UV repair'},indent=2))
