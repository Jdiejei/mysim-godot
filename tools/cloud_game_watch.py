import os,pathlib,json,time,subprocess,re,msvcrt,sys,traceback
root=pathlib.Path(__file__).resolve().parents[1];os.chdir(root);base=pathlib.Path('D:/MySim-transport/repair-20261029');lock=(base/'game-watch.lock').open('a+b');lock.write(b'0');lock.flush();lock.seek(0)
try:msvcrt.locking(lock.fileno(),msvcrt.LK_NBLCK,1)
except OSError:sys.exit(0)
log=(root/'outputs/repair-20261029/cloud-game-watch.log').open('a',encoding='utf-8',buffering=1);sys.stdout=log;sys.stderr=log
(root/'outputs/repair-20261029/cloud-game-watch.pid').write_text(str(os.getpid()));repo='instagramshashank8-bot/mysim-godot';env=dict(os.environ)
def call(args):return subprocess.run(args,env=env,capture_output=True,text=True,check=True,creationflags=subprocess.CREATE_NO_WINDOW).stdout
env['GH_TOKEN']=call(['gh','auth','token','-u','instagramshashank8-bot']).strip();statefile=root/'outputs/repair-20261029/cloud-game-state.json';jobs={}
if statefile.exists():jobs=json.loads(statefile.read_text()).get('jobs',{})
while True:
 try:
  parts=json.loads((base/'plan.json').read_text());assets=[p for p in parts if p['kind']=='assets'];ready=bool(assets) and all(p['status']=='uploaded' for p in assets)
  if ready:
   for workflow in ['game-review.yml','windows-build.yml']:
    job=jobs.get(workflow)
    if not job:
     existing=json.loads(call(['gh','run','list','--repo',repo,'--workflow',workflow,'--branch','codex/repair-20261029','--limit','1','--json','databaseId,status,url']))
     if existing:job=existing[0]
     else:
      result=call(['gh','workflow','run',workflow,'--repo',repo,'--ref','codex/repair-20261029']);match=re.search(r'/runs/(\d+)',result)
      if not match:raise RuntimeError('Dispatch succeeded but no run URL; reconcile before retry')
      job={'databaseId':int(match[1]),'url':match[0],'status':'queued'}
     jobs[workflow]=job
    run=json.loads(call(['gh','run','view',str(job['databaseId']),'--repo',repo,'--json','status,conclusion,url,headSha']))
    job.update(run)
    if run['status']=='completed' and run['conclusion']=='failure' and not job.get('retried'):
     call(['gh','run','rerun',str(job['databaseId']),'--failed','--repo',repo]);job['retried']=True;job['status']='retry_requested'
  preview=jobs.get('game-review.yml',{})
  if preview.get('conclusion')=='success' and 'full-races' not in jobs:
   result=call(['gh','workflow','run','game-review.yml','--repo',repo,'--ref','codex/repair-20261029','-f','depth=full_races']);match=re.search(r'/runs/(\d+)',result)
   if not match:raise RuntimeError('Full-race dispatch requires run reconciliation')
   jobs['full-races']={'databaseId':int(match[1]),'status':'queued'}
  if 'full-races' in jobs:
   job=jobs['full-races'];run=json.loads(call(['gh','run','view',str(job['databaseId']),'--repo',repo,'--json','status,conclusion,url,headSha']));job.update(run)
   if run['status']=='completed' and run['conclusion']=='failure' and not job.get('retried'):
    call(['gh','run','rerun',str(job['databaseId']),'--failed','--repo',repo]);job['retried']=True;job['status']='retry_requested'
  state={'assets_uploaded':sum(p['status']=='uploaded' for p in assets),'assets_total':len(assets),'ready':ready,'jobs':jobs,'updated':time.time()};statefile.write_text(json.dumps(state,indent=2))
  if jobs and all(j.get('status')=='completed' for j in jobs.values()):print('GAME_WORKFLOWS_TERMINAL',time.time());break
 except Exception:traceback.print_exc()
 time.sleep(60)
