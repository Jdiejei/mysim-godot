import os,pathlib,subprocess,json,sys
r=pathlib.Path.cwd();out=r/'outputs/cloud-game';out.mkdir(parents=True,exist_ok=True);g=os.environ['GODOT_BIN'];shard=int(os.environ.get('SHARD','0'));rows=[]
def run(label,values,args=None,timeout=120):
 env=dict(os.environ,**values,MYSIM_BINDING_AUDIT=str(out/label));log=out/(label+'.log')
 for attempt in (1,2):
  with (out/(label+f'-{attempt}.log')).open('w') as f:
   try:p=subprocess.run([g,'--path',str(r),'--resolution','960x540','--no-window','--fixed-fps','60','--audio-driver','Dummy']+(args or []),env=env,stdout=f,stderr=subprocess.STDOUT,timeout=timeout);code=p.returncode
   except subprocess.TimeoutExpired:code=-1
  text=(out/(label+f'-{attempt}.log')).read_text(errors='replace')
  if code==0 and 'ERROR:' not in text:return text
 raise RuntimeError(label+' failed; see saved logs')
if shard==0:
 text=run('geometry',{'MYSIM_RUNTIME_CHECKS':'1','MYSIM_GEOMETRYOUT':str(out/'geometry.json')},['--headless']);assert 'PASS runtime' in text
for t in range(shard,60,6):
 label=f'track-{t:02}';shot=out/(label+'.png');full=os.environ.get('FULL_RACES')=='1'
 text=run(label,{'MYSIM_TRACK':str(t),'MYSIM_CAR':'16','MYSIM_LAPS':'1','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'1000' if full else '2','MYSIM_SHOT':str(shot)},timeout=1800 if full else 120)
 import re
 finished=re.search(r'FINISHED.*best=([0-9.]+).*wallHits=([0-9]+).*damage=([0-9.]+)',text)
 ok=shot.exists() and (bool(finished) and float(finished[1])>0 and int(finished[2])==0 and float(finished[3])==0 if full else 'onSurface=True' in text)
 rows.append({'track':t,'scope':'full live-collision lap' if full else 'startup render probe','ok':ok,'finish':finished[0] if finished else None});(out/'track-status.json').write_text(json.dumps(rows,indent=2))

if shard<3 and os.environ.get('FULL_RACES')!='1':
 camera=['5.2,1.8,5.6','6,1.2,0','5.2,1.5,-5.6'][shard]
 for car in range(24):
  label=f'car-{car:02}-view-{shard}';shot=out/(label+'.png');run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)});assert shot.exists()
if shard==0 and os.environ.get('FULL_RACES')!='1':
 for track in (0,4,24):
  for mode in ('0','1'):
   label=f'ground-{track:02}-authored-{mode}';run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'8','MYSIM_AUTHORED_GROUND':mode,'MYSIM_SHOT':str(out/(label+'.png'))})
 for bias in ('0.2','1.0'):
  label='shadow-normal-bias-'+bias;run(label,{'MYSIM_TRACK':'4','MYSIM_CAR':'16','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'2','MYSIM_SHADOW_NORMAL_BIAS':bias,'MYSIM_SHOT':str(out/(label+'.png'))})
 text=run('race-flow',{'MYSIM_TRACK':'24','MYSIM_CAR':'16','MYSIM_LAPS':'1','MYSIM_RIVALS':'5','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_SECONDS':'1000','MYSIM_FLOWCHECKS':'1'},timeout=1200);assert 'PASS restart to main menu' in text
 import shutil
 result=r/'outputs/repair-20261029/results.png'
 if result.exists():shutil.copyfile(result,out/'results.png')
from PIL import Image,ImageDraw
files=sorted(out.glob('*.png'));sheet=Image.new('RGB',(320*5,200*((len(files)+4)//5)));d=ImageDraw.Draw(sheet)
for i,p in enumerate(files):
 im=Image.open(p).convert('RGB');im.thumbnail((320,180));x=i%5*320;y=i//5*200;sheet.paste(im,(x,y));d.text((x+3,y+182),p.stem,fill='white')
sheet.save(out/'mosaic.jpg',quality=90)
if not all(x['ok'] for x in rows):sys.exit(1)
