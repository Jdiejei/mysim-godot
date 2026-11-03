"""Runner-only inspection of preserved encrypted media; no game/assets rerun."""
import os,pathlib,json,subprocess,tempfile,hashlib,re
run=os.environ['SOURCE_RECORDING_RUN'];assert re.fullmatch(r'[0-9]+',run)
repo=os.environ['GITHUB_REPOSITORY'];out=pathlib.Path('outputs/recording-samples');out.mkdir(parents=True,exist_ok=True)
source=json.loads(subprocess.check_output(['gh','run','view',run,'-R',repo,'--json','conclusion,headSha']))
assert source['conclusion']=='success','Source recording run must have passed'
with tempfile.TemporaryDirectory() as scratch:
 root=pathlib.Path(scratch)
 subprocess.run(['gh','run','download',run,'-R',repo,'-n','encrypted-review-0-attempt-1','-D',str(root/'download')],check=True)
 envelopes=list((root/'download').rglob('*.enc'));assert len(envelopes)==1
 subprocess.run(['python','tools/cloud_decrypt.py',str(envelopes[0]),str(root/'plain')],check=True)
 videos=list((root/'plain').rglob('*.mp4'));assert len(videos)==1,'Select a single preserved race'
 video=videos[0];digest=hashlib.sha256(video.read_bytes()).hexdigest()
 records=[json.loads(p.read_text()) for p in (root/'plain').rglob('*-recording.json')]
 assert any(x['sha256']==digest for r in records for x in r['recordings']),'Video hash differs from original engine evidence'
 samples=root/'samples';samples.mkdir()
 subprocess.run(['ffmpeg','-v','error','-i',str(video),'-vf','fps=1','-frames:v','300',str(samples/'second-%03d.jpg')],check=True)
 count=len(list(samples.glob('*.jpg')));assert count>5
 meta={'sourceRun':int(run),'sourceHead':source['headSha'],'videoSha256':digest,'sampledFrames':count,'sampling':'1fps, maximum300frames; original movie preserved. Stills are not full motion acceptance.'}
 (samples/'source.json').write_text(json.dumps(meta,indent=2));(out/'source.json').write_text(json.dumps(meta,indent=2))
 subprocess.run(['python','tools/cloud_encrypt_output.py',str(samples),str(out/('recording-'+run+'.enc'))],check=True)
