import os,pathlib,csv,json,collections,subprocess,tarfile,io,hashlib,time,traceback
from cryptography.hazmat.primitives.ciphers import Cipher,algorithms,modes
ROOT=pathlib.Path(__file__).resolve().parents[1];BASE=pathlib.Path('D:/MySim-transport/repair-20261029');BASE.mkdir(parents=True,exist_ok=True)
REPO='instagramshashank8-bot/mysim-godot';TAG='repair-inputs-20261029';STATE=ROOT/'outputs/repair-20261029/cloud-intake-state.json'
def cmd(args,**kw):return subprocess.run(args,check=True,capture_output=True,text=True,**kw)
env=dict(os.environ);env['GH_TOKEN']=cmd(['gh','auth','token','-u','instagramshashank8-bot']).stdout.strip()
keyfile=BASE/'intake.key'
if not keyfile.exists():keyfile.write_bytes(os.urandom(32))
key=keyfile.read_bytes();cmd(['gh','secret','set','MYSIM_INTAKE_KEY','--repo',REPO],input=key.hex(),env=env)
for tag in [TAG,'repair-review-20261029']:
 exists=subprocess.run(['gh','release','view',tag,'--repo',REPO],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL).returncode==0
 if not exists:cmd(['gh','release','create',tag,'--repo',REPO,'--title',tag,'--prerelease','--latest=false','--notes','Encrypted project compute inputs/review outputs. The key is stored only as an Actions secret and locally outside the repository.'],env=env)
planfile=BASE/'plan.json'
if planfile.exists():parts=json.loads(planfile.read_text())
else:
 grouped=collections.OrderedDict()
 for r in csv.DictReader(open('D:/MySim-processed/final/manifest/selected_candidates.csv',encoding='utf-8-sig',newline='')):
  path=r['zip_path'];entry=grouped.setdefault(path,{'path':path,'member':'archives/'+r['account']+'/'+r['zip'],'candidates':[]})
  entry['candidates'].append({k:r[k] for k in ['procedure_id','mesh','category','blend','blend_sha256','source','source_sha256','quality_accepted','review_images','vertices','faces','uv_layers','uv0_max_error','uv1_max_error','fidelity_within_tolerance']})
 priorities={'car':0,'vehicle':1,'ground':2,'barrier':3,'architecture':4}
 groups=sorted(grouped.values(),key=lambda a:min(priorities.get(r['category'],5) for r in a['candidates']))
 parts=[];bucket=[];size=0
 def flush(kind):
  global bucket,size
  if bucket:parts.append({'kind':kind,'name':f'{kind}-part-{sum(p["kind"]==kind for p in parts):05}.enc','entries':bucket,'bytes':size,'status':'pending'});bucket=[];size=0
 for e in groups:
  n=pathlib.Path(e['path']).stat().st_size
  if size+n>512*1024*1024:flush('mesh')
  bucket.append(e);size+=n
 flush('mesh')
 for p in sorted((ROOT/'assets').rglob('*')):
  if not p.is_file() or p.suffix=='.import':continue
  n=p.stat().st_size
  if size+n>512*1024*1024:flush('assets')
  bucket.append({'path':str(p),'member':p.relative_to(ROOT).as_posix()});size+=n
 flush('assets');planfile.write_text(json.dumps(parts))
def save():
 planfile.write_text(json.dumps(parts));summary={'state':'transporting','parts':len(parts),'uploaded':sum(p['status']=='uploaded' for p in parts),'failed':sum(p['status']=='failed' for p in parts),'dispatched':sum(bool(p.get('dispatched')) for p in parts),'updated':time.time(),'plan':str(planfile),'repo':REPO,'branch':'codex/repair-20261029'};STATE.write_text(json.dumps(summary,indent=2))
class EncryptWriter:
 def __init__(self,path):
  self.f=open(path,'wb');nonce=os.urandom(12);self.f.write(b'MYSIMENC1'+nonce);self.c=Cipher(algorithms.AES(key),modes.GCM(nonce)).encryptor()
 def write(self,data):self.f.write(self.c.update(data));return len(data)
 def close(self):self.f.write(self.c.finalize());self.f.write(self.c.tag);self.f.close()
def reconcile_reviews():
 runs=json.loads(cmd(['gh','api',f'repos/{REPO}/actions/workflows/mesh-review.yml/runs?per_page=100'],env=env).stdout)['workflow_runs']
 active=sum(r['status']!='completed' for r in runs)
 byname={}
 legacy=sorted([r for r in runs if r.get('display_title')=='mesh-and-texture-review'],key=lambda r:r['created_at'])
 for run in runs:
  name=run.get('display_title','').removeprefix('review ')
  if name not in byname:byname[name]=run
 for index,run in enumerate(legacy):byname.setdefault(f'mesh-part-{index:05}.enc',run)
 for part in parts:
  run=byname.get(part['name'])
  if run:
   old_heads={'7d3b1fd5e62c46b6a925d5596a758f083b3d658a','6c24491','563f447'}
   obsolete=part['kind']=='mesh' and any(run['head_sha'].startswith(h) for h in old_heads)
   if obsolete and run['status']=='completed' and not part.get('compatibility_upgrade'):
    part.setdefault('review_failures',[]).append({'run':run['id'],'reason':'Old Blender cannot read the 4.5 candidate libraries; fixed loader generation'})
    cmd(['gh','workflow','run','mesh-review.yml','--repo',REPO,'--ref','codex/repair-20261029','-f','part='+part['name'],'-f','kind='+part['kind']],env=env);part['compatibility_upgrade']=True;part['review_status']='upgrade_queued';active+=1;continue
   part['run_id']=run['id'];part['review_status']=run['status'];part['review_conclusion']=run['conclusion']
   if run['status']=='completed' and run['conclusion'] not in ['success','cancelled']:
    if not part.get('review_retry'):
     part.setdefault('review_failures',[]).append({'run':run['id'],'conclusion':run['conclusion']});part['review_retry']=1
     cmd(['gh','run','rerun',str(run['id']),'--failed','--repo',REPO],env=env);part['review_status']='retry_requested';active+=1
    elif run.get('run_attempt',1)>=2:part['review_status']='failed_after_retry'
    else:part['review_status']='retry_requested'
  if part['status']=='uploaded' and not part.get('dispatched') and active<8:
   cmd(['gh','workflow','run','mesh-review.yml','--repo',REPO,'--ref','codex/repair-20261029','-f','part='+part['name'],'-f','kind='+part['kind']],env=env);part['dispatched']=True;active+=1
 save()

# Upload existing archives and source assets only; geometry/render work runs exclusively on GitHub.
parts.sort(key=lambda p: (0 if p["name"]=="mesh-part-00000.enc" else 1 if p["kind"]=="assets" else 2, p["name"]))
for part in parts:
 if part['status']=='uploaded' or (part['status']=='failed' and part.get('attempt',0)>=2):continue
 for attempt in range(part.get('attempt',0)+1,3):
  try:
   part['attempt']=attempt;part['status']='preparing';save();dest=BASE/part['name'];w=EncryptWriter(dest)
   try:
    with tarfile.open(fileobj=w,mode='w|') as t:
     manifest={'kind':part['kind'],'archives':[{k:e[k] for k in ['member','candidates']} for e in part['entries']] if part['kind']=='mesh' else []}
     data=json.dumps(manifest).encode();info=tarfile.TarInfo('manifest.json');info.size=len(data);t.addfile(info,io.BytesIO(data))
     for e in part['entries']:t.add(e['path'],arcname=e['member'],recursive=False)
   finally:w.close()
   digest=hashlib.file_digest(dest.open('rb'),'sha256').hexdigest();part['sha256']=digest;part['encrypted_bytes']=dest.stat().st_size;part['status']='uploading';save()
   cmd(['gh','release','upload',TAG,str(dest),'--repo',REPO,'--clobber'],env=env)
   assets=json.loads(cmd(['gh','release','view',TAG,'--repo',REPO,'--json','assets'],env=env).stdout)['assets'];remote=next(a for a in assets if a['name']==part['name'])
   if remote['size']!=dest.stat().st_size:raise ValueError('remote size mismatch')
   if remote.get('digest') and remote['digest']!='sha256:'+digest:raise ValueError('remote digest mismatch')
   part['status']='uploaded';part['asset_url']=remote['url'];save();dest.unlink();print('uploaded',part['name'],flush=True);break
  except Exception as e:
   part.setdefault('failures',[]).append({'attempt':attempt,'time':time.time(),'error':str(e)});part['error']=str(e);part['status']='failed';save();print('failed',part['name'],'attempt',attempt,type(e).__name__,flush=True)
 try:reconcile_reviews()
 except Exception as e:print('review dispatch pending',type(e).__name__,flush=True)
 save()
save()

while True:
 try:
  reconcile_reviews()
  terminal=sum(p.get('review_status') in ['completed','failed_after_retry'] for p in parts if p['status']=='uploaded')
  snapshot=json.loads(STATE.read_text());snapshot['state']='review_outputs_ready' if terminal==sum(p['status']=='uploaded' for p in parts) else 'cloud_reviews_pending';snapshot['reviews_terminal']=terminal;snapshot['manual_reviewed']=0;STATE.write_text(json.dumps(snapshot,indent=2))
  if snapshot['state']=='review_outputs_ready':break
 except Exception as e:print('review monitor',type(e).__name__,flush=True)
 time.sleep(60)
