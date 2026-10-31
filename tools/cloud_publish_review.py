"""Publish encrypted evidence into bounded release batches, preserving older releases."""
import datetime,json,os,pathlib,subprocess,sys
file=pathlib.Path(sys.argv[1]);run=int(os.environ['GITHUB_RUN_ID']);repo=os.environ['GITHUB_REPOSITORY']
tag='repair-review-'+datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%d')+'-'+str(run//1000000)
def gh(*args):return subprocess.run(['gh',*args],capture_output=True,text=True)
if gh('release','view',tag,'--repo',repo).returncode:
 created=gh('release','create',tag,'--repo',repo,'--prerelease','--title','MySim encrypted review evidence '+tag,'--notes','Encrypted review evidence only; not a completed game release.')
 if created.returncode and gh('release','view',tag,'--repo',repo).returncode:raise RuntimeError(created.stderr)
r=gh('release','upload',tag,str(file),'--repo',repo)
if r.returncode:raise RuntimeError(r.stderr)
pathlib.Path('outputs/cloud-game/evidence-publication.json').write_text(json.dumps({'release':tag,'asset':file.name,'encrypted':True,'run':run},indent=2))
print('Encrypted evidence published',tag,file.name,flush=True)
