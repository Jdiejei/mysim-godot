import json,pathlib,subprocess,concurrent.futures,sys,time
rows=json.loads(pathlib.Path('data/review_campaign_20261030.json').read_text());base=pathlib.Path('/tmp/intake-verification');base.mkdir(exist_ok=True)
def check(row):
 result=dict(row,ok=False);dest=base/row['part'];dest.mkdir(exist_ok=True)
 for attempt in (1,2):
  p=subprocess.run(['gh','run','download',str(row['run_id']),'--repo','instagramshashank8-bot/mysim-godot','--name','summary-'+row['part'],'--dir',str(dest)],capture_output=True,text=True)
  if p.returncode==0:break
  if attempt==2:result['error']=p.stderr[-1000:];return result
  time.sleep(2)
 try:
  s=json.loads((dest/'summary.json').read_text());result['summary']=s
  if row['kind']=='mesh':
   geometry=json.loads((dest/'geometry.json').read_text());result['missing_texture_objects']=sum(bool(x.get('missing_textures')) for x in geometry)
   result['ok']=s['requested']==s['converted']==row['expected'] and s['intake_failures']==0 and len(geometry)==row['expected'] and all(x['ok'] for x in geometry)
  else:result['ok']=s['assets']==row['expected']
 except Exception as e:result['error']=str(e)
 return result
with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool:results=list(pool.map(check,rows))
report={'scope':'complete intake/conversion verification; no art or production acceptance','parts':len(rows),'passed':sum(x['ok'] for x in results),'expected_meshes':sum(x['expected'] for x in rows if x['kind']=='mesh'),'manual_accepted':0,'results':results}
out=pathlib.Path('outputs/cloud-intake-verification');out.mkdir(parents=True,exist_ok=True);(out/'report.json').write_text(json.dumps(report,indent=2));print(json.dumps({k:v for k,v in report.items() if k!='results'}));sys.exit(0 if all(x['ok'] for x in results) else 1)
