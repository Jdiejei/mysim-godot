"""Summarize terminal native race artifacts on GitHub; never rerun simulation."""
import json,os,pathlib,subprocess,tempfile,hashlib
repo=os.environ['GITHUB_REPOSITORY'];run=int(os.environ['SOURCE_RUN']);cars=[int(x) for x in os.environ['EXPECTED_CARS'].split(',')]
assert cars and len(set(cars))==len(cars) and all(0<=c<24 for c in cars)
def gh(*args):return subprocess.check_output(['gh',*args],text=True)
state=json.loads(gh('run','view',str(run),'--repo',repo,'--json','status,conclusion,headSha'))
assert state['status']=='completed','Source campaign still live; adopt it without duplication'
rows=[];shards=[]
with tempfile.TemporaryDirectory() as temp:
 root=pathlib.Path(temp)
 for shard in range(12):
  dest=root/str(shard)
  subprocess.run(['gh','run','download',str(run),'--repo',repo,'-n',f'game-runtime-{shard}-attempt-1','-D',str(dest)],check=True)
  files=list(dest.rglob('mounted-native-layout-results.json'));assert len(files)==1
  source=files[0];cases=json.loads(source.read_text());shards.append({'shard':shard,'cases':len(cases),'sha256':hashlib.sha256(source.read_bytes()).hexdigest()})
  for case in cases:
   t,c=case['track'],case['car'];assert t%12==shard and c in cars
   report=source.parent/f'mounted-native-lap-track-{t:02}-car-{c:02}.json'
   result=case.get('result') or (json.loads(report.read_text()) if report.exists() else {})
   strict=bool(case.get('ok') and result.get('ok') and result.get('laps',0)>=1 and result.get('bestLap',0)>0 and result.get('resultVisible') and result.get('ruleResets')==0 and result.get('damage')==0 and result.get('impactEvents')==0 and result.get('actualPoseDistance',0)>=result.get('requiredDistance',float('inf')) and result.get('continuousCcd') and result.get('rotationalTyres') and result.get('damageEnabled'))
   rows.append({'track':t,'car':c,'reportedOk':case.get('ok'),'strictPass':strict,**{k:result.get(k) for k in ('laps','bestLap','damage','impactEvents','ruleResets','resultVisible','actualPoseDistance','requiredDistance')},'error':case.get('error')})
  # Large telemetry remains in original artifacts, not duplicated across shards locally.
  import shutil;shutil.rmtree(dest)
expected={(t,c) for t in range(60) for c in cars};actual=[(x['track'],x['car']) for x in rows]
assert len(actual)==len(set(actual)) and set(actual)==expected,'Missing/duplicate campaign coverage'
out={'run':run,'source':state,'total':len(rows),'passed':sum(x['strictPass'] for x in rows),'coverageComplete':True,'shards':shards,'rows':rows,'scope':'Actual dry solo race artifacts only; failures preserved. No art, traffic, weather, human driving or whole-game acceptance.'}
pathlib.Path('campaign-summary.json').write_text(json.dumps(out,indent=2));print('Actual coverage',len(rows),'strict passes',out['passed'])
