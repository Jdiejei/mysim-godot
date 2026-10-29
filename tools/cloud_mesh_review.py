import pathlib,json,zipfile,hashlib,subprocess,sys,os,collections
from PIL import Image,ImageDraw
root=pathlib.Path(sys.argv[1]).resolve();out=pathlib.Path(sys.argv[2]).resolve();out.mkdir(parents=True,exist_ok=True);manifest=json.loads((root/'manifest.json').read_text());requests=[];fail=[];images=[]
for archive in manifest['archives']:
 p=root/archive['member'];folder=root/'unpacked'/p.stem;folder.mkdir(parents=True,exist_ok=True)
 try:
  with zipfile.ZipFile(p) as z:
   names={pathlib.PurePosixPath(n).name:n for n in z.namelist() if not n.endswith('/')}
   for r in archive['candidates']:
    try:
     data=z.read(names[r['blend']]);digest=hashlib.sha256(data).hexdigest()
     if digest!=r['blend_sha256']:raise ValueError('blend checksum mismatch')
     blend=folder/r['blend'];blend.write_bytes(data)
     requests.append(dict(r,blend_path=str(blend),glb_path=str(out/'glb'/(r['procedure_id']+'.glb'))))
     for name in r['review_images'].split(';'):
      if name in names:
       dst=out/'review'/name;dst.parent.mkdir(exist_ok=True);dst.write_bytes(z.read(names[name]));images.append((r['procedure_id'],dst))
    except Exception as e:fail.append({'id':r['procedure_id'],'error':str(e)})
 except Exception as e:fail.append({'archive':str(p),'error':str(e)})
(root/'requests.json').write_text(json.dumps(requests));code=subprocess.run(['blender','--background','--disable-autoexec','--python','tools/cloud_blend_review.py','--',str(root/'requests.json'),str(out/'geometry.json')]).returncode
if code:raise RuntimeError('Blender process failed: '+str(code))
rows=json.loads((out/'geometry.json').read_text());(out/'intake-failures.json').write_text(json.dumps(fail,indent=2))
# Every tile retains the procedure ID. Creating a sheet does not mark its meshes as manually reviewed.
for start in range(0,len(images),100):
 sheet=Image.new('RGB',(160*10,140*10));d=ImageDraw.Draw(sheet)
 for k,(pid,p) in enumerate(images[start:start+100]):
  im=Image.open(p).convert('RGB');im.thumbnail((160,120));x=k%10*160;y=k//10*140;sheet.paste(im,(x,y));d.text((x+2,y+121),pid[:24],fill='white')
 folder=out/'sheets';folder.mkdir(exist_ok=True);sheet.save(folder/f'sheet-{start//100:04}.jpg',quality=85)
(out/'summary.json').write_text(json.dumps({'requested':sum(len(a['candidates']) for a in manifest['archives']),'converted':sum(x['ok'] for x in rows),'intake_failures':len(fail),'manual_reviewed':0,'accepted':0},indent=2))

if fail or any(not r["ok"] for r in rows):sys.exit(1)
