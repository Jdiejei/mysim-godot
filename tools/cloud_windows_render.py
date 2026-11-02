"""Actual exported Windows renderer smoke; evidence stays inside encrypted build package.
This checks a rendered player, not full race/physics or manual visual acceptance.
"""
import json,os,pathlib,subprocess
root=pathlib.Path.cwd();exe=root/'build-cloud/MySim.exe'
evidence=root/'build-cloud/render-evidence';evidence.mkdir(parents=True,exist_ok=True)
rows=[]
for view,values in [('car',{'MYSIM_CARVIEW':'16','MYSIM_FITVIEW':'1'}),('race',{'MYSIM_TRACK':'24','MYSIM_CAR':'16','MYSIM_RIVALS':'5','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_SECONDS':'8'})]:
 for attempt in (1,2):
  shot=evidence/f'{view}-{attempt}.png';log=evidence/f'{view}-{attempt}.log'
  env=dict(os.environ,**values,MYSIM_SHOT=str(shot))
  timed_out=False
  with log.open('w') as output:
   try:
    process=subprocess.run([str(exe),'--rendering-method','gl_compatibility','--resolution','960x540','--fixed-fps','60','--audio-driver','Dummy','--log-file',str(evidence/f'{view}-{attempt}-engine.log')],env=env,stdout=output,stderr=subprocess.STDOUT,timeout=600,creationflags=subprocess.CREATE_NO_WINDOW)
    code=process.returncode
   except subprocess.TimeoutExpired:code=-1;timed_out=True
  from PIL import Image,ImageStat
  image_ok=False
  if shot.exists():
   with Image.open(shot) as image:
    image_ok=image.size==(960,540) and max(ImageStat.Stat(image.convert('RGB')).stddev)>2
  ok=code==0 and not timed_out and image_ok
  rows.append(dict(view=view,attempt=attempt,exit=code,timedOut=timed_out,imagePresent=shot.exists(),nonuniformImage=image_ok,ok=ok))
  (root/'windows-render-results.json').write_text(json.dumps(dict(cases=rows,headless=False,manualVisualAcceptance=False,scope='Exported Windows OpenGL player smoke only; actual images encrypted, race legality/native physics not certified'),indent=2))
  if ok:break
 else:raise RuntimeError(f'Exported Windows {view} render failed after one retry')
