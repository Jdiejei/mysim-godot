"""Collect completed native-game evidence only; never dispatch/retry or alter running jobs."""
import os,sys,json,pathlib,subprocess,tempfile,concurrent.futures,re,time,msvcrt
root=pathlib.Path(__file__).resolve().parents[1];os.chdir(root);out=root/'outputs/repair-20261029';base=pathlib.Path('D:/MySim-transport/repair-20261029/native-evidence');base.mkdir(parents=True,exist_ok=True)
lock=(base/'collector.lock').open('a+b');lock.write(b'0');lock.flush();lock.seek(0)
try:msvcrt.locking(lock.fileno(),msvcrt.LK_NBLCK,1)
except OSError:sys.exit(0)
sys.stdout=(out/'native-collector.log').open('a',buffering=1);sys.stderr=sys.stdout;(out/'native-collector.pid').write_text(str(os.getpid()))
fleet=[('instagramshashank8-bot/mysim-godot',41012651158,[16]),('instagramshashank8-bot/mysim-godot',41014089393,list(range(17,24))),('Jdiejei/mysim-godot',41014039754,list(range(8))),('blackwingftc-lang/mysim-godot',41014052630,list(range(8,16)))]
def call(args):return subprocess.run(args,check=True,capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW).stdout
def retrieve(task):
 repo,run,artifact,cars=task;safe=re.sub('[^a-zA-Z0-9_-]','_',artifact['name']);dest=base/repo.replace('/','_')/str(run)/safe
 marker=dest/'download-complete'
 if not marker.exists():
  dest.parent.mkdir(parents=True,exist_ok=True)
  with tempfile.TemporaryDirectory(dir=base) as temp:
   if not pathlib.Path(temp).resolve().is_relative_to(base.resolve()):raise ValueError('Invalid evidence directory')
   call(['gh','run','download',str(run),'--repo',repo,'--name',artifact['name'],'--dir',temp])
   if dest.exists():raise RuntimeError('Incomplete evidence requires reconciliation: '+str(dest))
   os.rename(temp,dest)
  marker.write_text(str(artifact['id']))
 found=[]
 for file in dest.rglob('native-race-status.json'):
  for row in json.loads(file.read_text()):
   car=row.get('car',cars[0] if len(cars)==1 else None);track=row['track']
   if car not in cars or not 0<=track<60:raise ValueError('Unexpected case in campaign evidence')
   found.append(dict(row,car=car,repo=repo,run_id=run,evidence=str(dest)))
 return found
while True:
 try:
  tasks=[];runs=[]
  for repo,run,cars in fleet:
   status=json.loads(call(['gh','run','view',str(run),'--repo',repo,'--json','status,conclusion,headSha,url']));runs.append(dict(status,repo=repo,run_id=run,cars=cars))
   artifacts=json.loads(call(['gh','api',f'repos/{repo}/actions/runs/{run}/artifacts?per_page=100']))['artifacts']
   tasks += [(repo,run,a,cars) for a in artifacts if a['name'].startswith('game-runtime-') and not a['expired']]
  cases={};errors=[]
  with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
   futures=[pool.submit(retrieve,t) for t in tasks]
   for f in futures:
    try:
     for row in f.result():
      key=f"{row['car']:02}-{row['track']:02}"
      if key in cases:raise ValueError('Duplicate native case: '+key)
      cases[key]=row
    except Exception as e:errors.append(str(e))
  passed=sum(bool(r['ok']) for r in cases.values());failed=len(cases)-passed
  report={'scope':'native full-scene physics only; no final art or game acceptance','expected':1440,'passed':passed,'failed':failed,'not_reported':1440-len(cases),'all_campaigns_terminal':all(r['status']=='completed' for r in runs),'runs':runs,'cases':cases,'errors':errors,'updated':time.time()}
  path=out/'NATIVE_CASE_PROGRESS.json';tmp=path.with_suffix('.writing');tmp.write_text(json.dumps(report,indent=2));os.replace(tmp,path);print('NATIVE_PROGRESS',passed,failed,report['not_reported'],flush=True)
  if report['all_campaigns_terminal']:break
 except Exception as e:print('COLLECTOR_ERROR',repr(e),flush=True)
 time.sleep(60)
