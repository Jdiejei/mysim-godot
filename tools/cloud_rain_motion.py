"""Matched short moving rain evidence; all engine attempts preserved, no quality acceptance."""
import pathlib,subprocess,json,hashlib,os

def review(run,out,shard):
 tod=(0,3)[shard];prediction=os.environ.get('RAIN_PREDICTION_REVIEW')=='1'
 for candidate in (0,1):
  label=f'rain-motion-tod-{tod}-candidate-{candidate}'
  if prediction:label=f'rain-prediction-tod-{tod}-candidate-{candidate}'
  movie=pathlib.Path('/tmp')/(label+'.avi')
  values={'MYSIM_TRACK':'0','MYSIM_CAR':'16','MYSIM_TOD':str(tod),'MYSIM_WET':'2','MYSIM_RAIN_VISIBILITY_CANDIDATE':str(candidate),'MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'12','MYSIM_SHOT':str(out/(label+'.png')),'MYSIM_NATIVE_RECORD_FPS':'15'}
  if prediction:
   values['MYSIM_RAIN_VISIBILITY_CANDIDATE']='1';values['MYSIM_RAIN_PREDICT_SPAWN']=str(candidate);values['MYSIM_RAIN_PREDICT_AUDIT']=str(out/(label+'-prediction.json'))
  try:
   text=run(label,values,['--rendering-method','gl_compatibility','--write-movie',str(movie)],timeout=900)
   assert ('RAIN_VISIBILITY_CANDIDATE' if candidate or prediction else 'RAIN_VOLUME_READY') in text
   if prediction:assert pathlib.Path(values['MYSIM_RAIN_PREDICT_AUDIT']).exists(),'Missing actual observer motion audit'
  finally:
   rows=[]
   for original in sorted(movie.parent.glob(movie.stem+'-attempt-*.avi')):
    if not original.stat().st_size:continue
    target=out/(original.stem+'.mp4')
    with (out/(original.stem+'-encode.log')).open('w') as log:
     subprocess.run(['ffmpeg','-y','-i',str(original),'-c:v','libx264','-crf','20','-pix_fmt','yuv420p',str(target)],stdout=log,stderr=subprocess.STDOUT,check=True)
     subprocess.run(['ffmpeg','-y','-i',str(target),'-vf','fps=1/3',str(out/(original.stem+'-frame-%03d.jpg'))],stdout=log,stderr=subprocess.STDOUT,check=True)
    metadata=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_format','-show_streams','-of','json',str(target)]))
    rows.append({'video':target.name,'sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'metadata':metadata})
   (out/(label+'-recording.json')).write_text(json.dumps({'recordings':rows,'scope':'Matched moving baseline/candidate day/night rain, software15fps capture; not performance, spray, physics or final quality acceptance'},indent=2))
   assert rows,'No recorded rain evidence'
