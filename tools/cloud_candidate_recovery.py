"""Resume only helper-selection failures after the fixed-loader pilot passes. Metadata/transport only."""
import json,os,pathlib,re,subprocess,sys,time,msvcrt,traceback
root=pathlib.Path(__file__).resolve().parents[1];os.chdir(root)
base=root/'outputs/repair-20261029';lock=(base/'candidate-recovery.lock').open('a+b');lock.write(b'0');lock.flush();lock.seek(0)
try:msvcrt.locking(lock.fileno(),msvcrt.LK_NBLCK,1)
except OSError:sys.exit(0)
sys.stdout=(base/'candidate-recovery.log').open('a',buffering=1);sys.stderr=sys.stdout
(base/'candidate-recovery.pid').write_text(str(os.getpid()))
env=dict(os.environ);repo='instagramshashank8-bot/mysim-godot';statepath=base/'candidate-recovery.json'
def call(args):return subprocess.run(args,env=env,check=True,capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW).stdout
env['GH_TOKEN']=call(['gh','auth','token','-u','instagramshashank8-bot']).strip()
state=json.loads(statepath.read_text()) if statepath.exists() else {'pilot':40929682955,'pilot_retry':False,'parts':{},'phase':'pilot'}
def save():
 temp=statepath.with_suffix('.writing');temp.write_text(json.dumps(state,indent=2));os.replace(temp,statepath)
def view(run):return json.loads(call(['gh','run','view',str(run),'--repo',repo,'--json','status,conclusion,headSha']))
def capacity():
 used=0
 for status in ('queued','in_progress'):
  data=json.loads(call(['gh','api',f'repos/{repo}/actions/runs?status={status}&per_page=100']))
  for run in data['workflow_runs']:
   jobs=json.loads(call(['gh','api',f'repos/{repo}/actions/runs/{run["id"]}/jobs?per_page=100']))['jobs']
   used+=sum(j['status']!='completed' for j in jobs)
 return max(0,20-used)
save()
while True:
 try:
  if state['phase']=='pilot':
   pilot=view(state['pilot'])
   if pilot['status']=='completed':
    if pilot['conclusion']=='success':state['phase']='classify';save()
    elif not state['pilot_retry']:
     state['pilot_retry']=True;save();call(['gh','run','rerun',str(state['pilot']),'--failed','--repo',repo])
    else:state['phase']='pilot_failed_requires_fix';save();break
  if state['phase']=='classify':
   plan=json.loads(pathlib.Path('D:/MySim-transport/repair-20261029/plan.json').read_text())
   for part in plan:
    if part.get('review_status')!='failed_after_retry' or part['name'] in state['parts']:continue
    dest=base/'cloud'/'recovery-diagnostics'/str(part['run_id']);dest.mkdir(parents=True,exist_ok=True)
    call(['gh','run','download',str(part['run_id']),'--repo',repo,'--dir',str(dest)])
    bad=[]
    for file in dest.rglob('geometry.json'):bad += [r for r in json.loads(file.read_text()) if not r['ok']]
    helper_only=bool(bad) and all('ambiguous candidate objects:' in r.get('error','') and '__program_' in r['error'] and '__LOD' in r['error'] for r in bad)
    state['parts'][part['name']]={'original_run':part['run_id'],'failure_count':len(bad),'status':'ready' if helper_only else 'requires_other_fix'};save()
   state['phase']='recover';save()
  if state['phase']=='recover':
   budget=capacity()
   for name,part in state['parts'].items():
    if part['status']=='ready' and budget>0:
     part['status']='dispatching';save()
     output=call(['gh','workflow','run','mesh-review.yml','--repo',repo,'--ref','codex/repair-20261029','-f','part='+name,'-f','kind=mesh'])
     match=re.search(r'/runs/(\d+)',output)
     if not match:raise RuntimeError('Dispatch outcome needs manual reconciliation: '+name)
     part.update(run_id=int(match[1]),status='queued',retry=False);save();budget-=1
    if part.get('run_id') and part['status'] not in ('success','failed_after_retry'):
     run=view(part['run_id']);part['result']=run
     if run['status']=='completed':
      if run['conclusion']=='success':part['status']='success'
      elif not part['retry']:
       part['retry']=True;save();call(['gh','run','rerun',str(part['run_id']),'--failed','--repo',repo]);part['status']='retry_requested'
      else:part['status']='failed_after_retry'
     save()
   if all(p['status'] in ('success','failed_after_retry','requires_other_fix') for p in state['parts'].values()):state['phase']='terminal_review_required';save();break
  print('RECOVERY',state['phase'],time.time(),flush=True)
 except Exception:traceback.print_exc()
 time.sleep(60)
