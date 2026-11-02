"""Cloud-only native recording; preserve each actual engine attempt as encrypted evidence."""
import pathlib,subprocess,json,hashlib

def record(run,out,label,values,renderer):
 movie=pathlib.Path('/tmp')/(label+'.avi')
 values={**values,'MYSIM_NATIVE_RESULT_CAPTURE':str(out/(label+'-results.png')),'MYSIM_DEFER_NATIVE_RENDER':'0','MYSIM_NATIVE_RECORD_FPS':'15'}
 try:
  text=run(label,values,['--rendering-method',renderer,'--write-movie',str(movie)],timeout=1800)
  assert pathlib.Path(values['MYSIM_NATIVE_RESULT_CAPTURE']).exists(),'Missing actual results image'
  return text
 finally:
  recordings=[]
  for original in sorted(movie.parent.glob(movie.stem+'-attempt-*.avi')):
   if original.stat().st_size==0:continue
   video=out/(original.stem+'.mp4')
   with (out/(original.stem+'-encode.log')).open('w') as log:
    subprocess.run(['ffmpeg','-y','-i',str(original),'-c:v','libx264','-preset','fast','-crf','20','-pix_fmt','yuv420p','-c:a','aac','-b:a','96k',str(video)],stdout=log,stderr=subprocess.STDOUT,check=True)
    subprocess.run(['ffmpeg','-y','-i',str(video),'-vf','fps=1/30',str(out/(original.stem+'-frame-%03d.jpg'))],stdout=log,stderr=subprocess.STDOUT,check=True)
   probe=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_format','-show_streams','-of','json',str(video)]))
   assert any(stream.get('codec_type')=='video' for stream in probe['streams']) and float(probe['format']['duration'])>5,'Missing actual gameplay video'
   recordings.append({'video':str(video),'sha256':hashlib.sha256(video.read_bytes()).hexdigest(),'metadata':probe})
  (out/(label+'-recording.json')).write_text(json.dumps({'recordings':recordings,'scope':'Actual rendered native lap at640x360/15fps; physics remains240Hz with16 steps per recorded frame, all engine attempts preserved,30second frames and actual results image. No final graphics/Windows acceptance implied.'},indent=2))
  if not recordings:raise RuntimeError('Native recording produced no actual movie')
