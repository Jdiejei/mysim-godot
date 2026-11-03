import os,pathlib,subprocess,json,sys,time
r=pathlib.Path.cwd();out=r/'outputs/cloud-game';out.mkdir(parents=True,exist_ok=True);g=os.environ['GODOT_BIN'];shard=int(os.environ.get('SHARD','0'));rows=[]
def run(label,values,args=None,timeout=300):
 env={k:v for k,v in os.environ.items() if not k.startswith('MYSIM_')};env.update(values);env.setdefault('MYSIM_INTERLAGOS_SPIN_DIAGONAL',os.environ.get('INTERLAGOS_SPIN_DIAGONAL','0'));env.setdefault('MYSIM_CONTACT_CAPTURE_AFTER',os.environ.get('CONTACT_CAPTURE_AFTER','0'));env.setdefault('MYSIM_NATIVE_COMBINED_ASSIST',os.environ.get('NATIVE_COMBINED_ASSIST','0'));env.setdefault('MYSIM_INTERLAGOS_SEAM_WELD',os.environ.get('INTERLAGOS_SEAM_WELD','0'));env.setdefault('MYSIM_CHIRON_RECONSTRUCTED_WING',os.environ.get('MYSIM_CHIRON_RECONSTRUCTED_WING','0'));env.setdefault('MYSIM_SOURCE_AXLE_FIT',os.environ.get('SOURCE_AXLE_FIT','0'));env.setdefault('MYSIM_COPLANAR_ROAD_CANDIDATE',os.environ.get('ROAD_COPLANAR_CANDIDATE','0'));env.setdefault('MYSIM_NATIVE_PHYSICS_HZ',os.environ.get('NATIVE_PHYSICS_HZ','120'));env.setdefault('MYSIM_NATIVE_ROTATIONAL_TYRES',os.environ.get('NATIVE_ROTATIONAL_TYRES','0'));env.setdefault('MYSIM_NATIVE_DAMAGE',os.environ.get('NATIVE_DAMAGE','0'));env.setdefault('MYSIM_NATIVE_CCD',os.environ.get('NATIVE_CCD','1'));env.setdefault('MYSIM_NATIVE_RIDE_HEIGHT',os.environ.get('NATIVE_RIDE_HEIGHT','0'));env.setdefault('MYSIM_SOURCE_RUNOFF_SUPPORT',os.environ.get('SOURCE_RUNOFF_SUPPORT','0'));env.setdefault('MYSIM_VERTICAL_ROAD_PLAN',os.environ.get('NATIVE_VERTICAL_ROAD_PLAN','0'));env['MYSIM_NATIVE_FACE_EVIDENCE']='1' if os.environ.get('NATIVE_GEOMETRY_RECOVERY')=='1' else '0';env['MYSIM_BINDING_AUDIT']=str(out/label);log=out/(label+'.log')
 for attempt in (1,2):
  env['MYSIM_PROBE_ATTEMPT']=str(attempt)
  shot=pathlib.Path(values['MYSIM_SHOT']).resolve() if values.get('MYSIM_SHOT') else None
  if shot is not None:
   if not shot.is_relative_to(out):raise ValueError('Screenshot must stay in this run output directory')
   if shot.exists():shot.rename(shot.with_name(shot.stem+f'-previous-attempt{attempt}'+shot.suffix))
  attempt_args=list(args or [])
  if '--write-movie' in attempt_args:
   movie_index=attempt_args.index('--write-movie')+1;movie=pathlib.Path(attempt_args[movie_index]);attempt_args[movie_index]=str(movie.with_name(movie.stem+f'-attempt-{attempt}'+movie.suffix))
  result_image=pathlib.Path(values['MYSIM_NATIVE_RESULT_CAPTURE']) if values.get('MYSIM_NATIVE_RESULT_CAPTURE') else None
  if result_image is not None and result_image.exists():result_image.rename(result_image.with_name(result_image.stem+f'-previous-attempt{attempt}'+result_image.suffix))
  started=time.monotonic();timed_out=False
  with (out/(label+f'-{attempt}.log')).open('w') as f:
   try:p=subprocess.run([g,'--path',str(r),'--resolution','640x360' if values.get('MYSIM_NATIVE_RESULT_CAPTURE') else '960x540','--no-window','--fixed-fps',values.get('MYSIM_NATIVE_RECORD_FPS','60'),'--audio-driver','Dummy']+attempt_args,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=timeout);code=p.returncode
   except subprocess.TimeoutExpired:code=-1;timed_out=True
  # Keep each failed/successful attempt's actual report, not only the last retry's file.
  for key,value in values.items():
   if key.endswith('_REPORT') and value:
    report=pathlib.Path(value).resolve()
    if report.is_relative_to(out) and report.is_file():
     import shutil
     shutil.copyfile(report,report.with_name(report.stem+f'-attempt-{attempt}'+report.suffix))
     replay=report.with_name(report.stem+'-preimpact-replay.json')
     if replay.is_file():shutil.copyfile(replay,replay.with_name(replay.stem+f'-attempt-{attempt}'+replay.suffix))
  text=(out/(label+f'-{attempt}.log')).read_text(errors='replace')
  (out/(label+f'-{attempt}-process.json')).write_text(json.dumps({'returncode':code,'timed_out':timed_out,'elapsed_seconds':time.monotonic()-started,'timeout_seconds':timeout,'screenshot_exists':shot.exists() if shot else None,'error_lines':[line for line in text.splitlines() if 'ERROR:' in line]},indent=2))
  if code==0 and 'ERROR:' not in text:return text
 raise RuntimeError(label+' failed; see saved logs')
def recorded_native_race(label,values):
 from cloud_native_recording import record
 return record(run,out,label,values,os.environ.get('NATIVE_VISUAL_RENDERER','gl_compatibility'))
if os.environ.get('REPLAY_CAPTURE_RUN'):
 capture_run=os.environ['REPLAY_CAPTURE_RUN'];assert capture_run.isdigit(),'Invalid capture run ID'
 supported_damage=capture_run=='41579600177'
 speculative_zero=os.environ.get('REPLAY_SPECULATIVE_ZERO')=='1'
 if speculative_zero:assert supported_damage and os.environ.get('REPLAY_LEGACY_EDGES','0')=='0','Speculative-distance ablation requires exact measured-support capture only'
 early_both_spin=capture_run in ('41574231413','41579600177')
 later_spin=capture_run=='41568745467'
 spin_replay=capture_run in ('41513037207','41568745467','41574231413','41579600177')
 repaired_seam=capture_run=='41467648085'
 source_plane_only=os.environ.get('REPLAY_SOURCE_PLANE_ONLY')=='1' and not repaired_seam
 if repaired_seam:assert shard==4 and os.environ.get('REPLAY_LEGACY_EDGES','0')=='0','Repaired seam requires exact shard/settings'
 if source_plane_only:assert capture_run=='41450839854' and shard==4 and os.environ.get('REPLAY_LEGACY_EDGES','0')=='0','Fleet replay requires exact existing shard4 capture and no settings ablation'
 destination=out/'replay-input';subprocess.run(['gh','run','download',capture_run,'--repo','instagramshashank8-bot/mysim-godot','-n',f'game-runtime-{shard}-attempt-1','-D',str(destination)],check=True)
 candidates=list(destination.rglob('*-preimpact-replay-attempt-1.json'))
 selection='explicit first-attempt copy'
 if not candidates:
  # A report-serialization failure can leave the standalone capture without the attempt copy.
  # Preserve and label that provenance; it is not asserted to be the first attempt.
  candidates=list(destination.rglob('*-preimpact-replay.json'));selection='standalone final preserved capture; attempt identity unproven'
 assert len(candidates)==1,'Require one unambiguous preserved damaging-contact capture'
 (out/'replay-input-provenance.json').write_text(json.dumps({'run':capture_run,'artifactShard':shard,'selection':selection,'path':str(candidates[0])},indent=2))
 capture=candidates[0];data=json.loads(capture.read_text());
 if spin_replay:assert shard==4 and data['car']==13 and data['configuration']['MYSIM_TRACK']=='4' and data['configuration']['MYSIM_INTERLAGOS_SEAM_WELD']=='2' and data['captureTrigger'].startswith('Opt-in bounded DriveSurface spin diagnostic'),'Require exact undamaged spin capture'
 if early_both_spin:assert data['configuration']['MYSIM_INTERLAGOS_SPIN_DIAGONAL']=='2' and data['configuration']['MYSIM_CONTACT_CAPTURE_AFTER']=='55','Require exact early mode2 capture'
 if supported_damage:assert data['bodyConfiguration']['DamageEnabled'] and data['configuration']['MYSIM_NATIVE_RIDE_HEIGHT']=='1' and abs(data['bodyConfiguration']['SuspensionPreload']-.0040999986)<.000001,'Require exact measured-support damage-enabled capture'
 if later_spin:assert data['configuration']['MYSIM_INTERLAGOS_SPIN_DIAGONAL']=='1' and data['configuration']['MYSIM_CONTACT_CAPTURE_AFTER']=='245','Require exact later diagonal candidate capture'
 if repaired_seam:assert data['car']==13 and data['configuration']['MYSIM_TRACK']=='4' and 'movedOccurrences=7 degenerateFacesRemoved=1' in capture.with_name('mounted-native-lap-track-04-car-13-1.log').read_text(),'Require actual repaired-seam capture'
 if source_plane_only:assert data['car']==17 and data['configuration']['MYSIM_TRACK']=='4' and data['configuration']['MYSIM_SOURCE_AXLE_FIT']=='0' and data['configuration']['MYSIM_COPLANAR_ROAD_CANDIDATE']=='0','Wrong fleet source configuration'
 assert data['schema']==2 and len(data['priorStates'])>=100,'Require damaging-event bounded capture, not initial settling'
 report=out/'reduced-contact-replay.json';text=run('reduced-contact-replay',{'MYSIM_REPLAY_SPECULATIVE_ZERO':'1' if speculative_zero else '0','MYSIM_REPLAY_EARLY_BOTH_SPIN':'1' if early_both_spin else '0','MYSIM_REPLAY_LATER_SPIN':'1' if later_spin else '0','MYSIM_INTERLAGOS_SPIN_DIAGONAL':'2' if early_both_spin else '1' if later_spin else '0','MYSIM_REPLAY_UNDAMAGED_SPIN':'1' if spin_replay else '0','MYSIM_REPLAY_REPAIRED_SEAM':'1' if repaired_seam else '0','MYSIM_INTERLAGOS_SEAM_WELD':'2' if spin_replay else '1' if repaired_seam else '0','MYSIM_REDUCED_CONTACT_CAPTURE':str(capture.resolve()),'MYSIM_REDUCED_CONTACT_REPORT':str(report),'MYSIM_REPLAY_INTERNAL_EDGE_ABLATION':os.environ.get('REPLAY_LEGACY_EDGES','0'),'MYSIM_REPLAY_SOURCE_PLANE_ONLY':'1' if source_plane_only else '0'},['--headless'],timeout=180)
 result=json.loads(report.read_text());ablation=os.environ.get('REPLAY_LEGACY_EDGES')=='1' or speculative_zero
 assert (result['controlledSettingsAblation'] and result['controlledChanges']==1 and not result['settingsMatched'] if ablation else result['settingsMatched']) and len(result['trials'])==(2 if early_both_spin else 3 if spin_replay else 3 if repaired_seam else 4 if source_plane_only else 2 if ablation else 5) and 'COMPLETE reduced contact experiment' in text
 if source_plane_only:assert result['sourcePlaneOnly'] and result['sourceTopology'] and result['enhancedInternalEdgeRemoval']=='false' and result['car']==17 and result['track']==4,'Fleet replay topology/settings evidence missing'
 if ablation:assert result['sharpEdgeControl']['ok'] and 'PASS actual source-car perpendicular sharp-edge blockage/CCD controls' in text
 print('COMPLETE isolated comparison; reproduction remains explicitly unproven, no gameplay repair accepted',flush=True);sys.exit(0)
if os.environ.get('WALL_GEOMETRY_DIAGNOSTIC')=='1':
 results=[]
 for metres in (False,True):
  label='wall-geometry-'+('metres' if metres else 'source-units');report=out/(label+'.json');env={k:v for k,v in os.environ.items() if not k.startswith('MYSIM_')};env.update(MYSIM_WALL_GEOMETRY_CHECKS=str(report),MYSIM_METRE_WALL_COLLIDERS='1' if metres else '0')
  with (out/(label+'.log')).open('w') as stream:p=subprocess.run([g,'--headless','--path',str(r),'--fixed-fps','60'],env=env,stdout=stream,stderr=subprocess.STDOUT,timeout=600)
  log=(out/(label+'.log')).read_text(errors='replace');errors=[line for line in log.splitlines() if 'ERROR:' in line];data=json.loads(report.read_text()) if report.exists() else None
  results.append({'metres':metres,'code':p.returncode,'errors':errors,'result':data});(out/'wall-geometry-comparison.json').write_text(json.dumps(results,indent=2))
 assert results[0]['result'] is not None,'Source diagnostic did not produce geometry evidence'
 assert results[1]['code']==0 and not results[1]['errors'] and results[1]['result']['all'],'Metre representation remains unverified; keep original failure evidence'
 print('PASS exact source-sign metre-representation/two-sided contacts; production unchanged',flush=True);sys.exit(0)
if os.environ.get('NATIVE_AI_DIAGNOSTIC')=='1':
 label='native-ai-diagnostic-track-22-car-16';report=out/(label+'.json');error=None
 try:text=run(label,{'MYSIM_TRACK':'22','MYSIM_CAR':'16','MYSIM_NATIVE_GAMEPLAY':'1','MYSIM_COMPONENT_MOUNTS':'1','MYSIM_REQUIRE_NATIVE_CAR_HULLS':'1','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_LAPS':'1','MYSIM_SECONDS':'180','MYSIM_RIVALS':'0','MYSIM_PRECISE_AI':'3','MYSIM_ERS_AUTO':'0','MYSIM_NATIVE_GAMEPLAY_REPORT':str(report)},['--headless'],timeout=600)
 except RuntimeError as e:error=e
 if report.exists():subprocess.run([sys.executable,'tools/cloud_plot_native_route.py',str(report)],check=True)
 if error:raise error
 assert json.loads(report.read_text())['ok'],'Diagnostic does not waive legal-lap/no-rescue gates'
 sys.exit(0)
if os.environ.get('NATIVE_RETIREMENT')=='1':
 track=(0,43)[shard];car=(0,16)[shard];label=f'empty-fuel-retirement-track-{track:02}-car-{car:02}'
 text=run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':str(car),'MYSIM_NATIVE_GAMEPLAY':'1','MYSIM_COMPONENT_MOUNTS':'1','MYSIM_REQUIRE_NATIVE_CAR_HULLS':'1','MYSIM_RETIREMENT_CHECKS':'1','MYSIM_START_FUEL':'0','MYSIM_RIVALS':'0'},['--headless'],timeout=180)
 assert 'PASS actual empty-fuel native retirement and DNF results' in text and 'PASS retirement to fresh restart' in text and 'PASS retirement restart to main menu' in text
 assert 'NATIVE_SOURCE_BODY_COLLIDERS' in text and 'NATIVE_COLLISION_FIXTURE_PROXY' not in text
 print('PASS actual native empty-fuel retirement/results/restart/menu; no completed-lap substitution',flush=True);sys.exit(0)
if os.environ.get('NATIVE_MOUNTED_LAPS')=='1':
 cars=[int(x) for x in os.environ.get('NATIVE_CARS','0,16').split(',')]
 assert cars and len(set(cars))==len(cars) and all(0<=car<24 for car in cars),'Invalid requested car coverage'
 (out/'mounted-native-requested-coverage.json').write_text(json.dumps({'cars':cars,'tracks':list(range(shard,60,12)),'shard':shard},indent=2))
 recovery=[x for x in json.loads(pathlib.Path(os.environ.get('GEOMETRY_CASE_MANIFEST','data/native_geometry_recovery.json')).read_text())['cases'] if x['track']%12==shard] if os.environ.get('NATIVE_GEOMETRY_RECOVERY')=='1' else None
 for track in (sorted({x['track'] for x in recovery}) if recovery is not None else range(shard,60,12)):
  for car in ([x['car'] for x in recovery if x['track']==track] if recovery is not None else cars):
   label=f'mounted-native-lap-track-{track:02}-car-{car:02}';report=out/(label+'.json')
   values={'MYSIM_TRACK':str(track),'MYSIM_CAR':str(car),'MYSIM_NATIVE_GAMEPLAY':'1','MYSIM_COMPONENT_MOUNTS':'1','MYSIM_REQUIRE_NATIVE_CAR_HULLS':'1','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_LAPS':'1','MYSIM_SECONDS':'1200','MYSIM_RIVALS':'0','MYSIM_PRECISE_AI':os.environ.get('NATIVE_AI_MODE','3') if os.environ.get('NATIVE_AI_ENABLED')=='1' else '3','MYSIM_NATIVE_GAMEPLAY_CHECKS':'1','MYSIM_NATIVE_GAMEPLAY_REPORT':str(report),'MYSIM_COMPONENT_MOUNT_REPORT':str(out/(label+'-mounts.json'))}
   try:
    text=recorded_native_race(label,values) if os.environ.get('NATIVE_RECORD_GAMEPLAY')=='1' else run(label,values,['--headless'],timeout=1800);result=json.loads(report.read_text());ok=result['ok'] and 'PASS native force-driven valid lap and results UI' in text and 'NATIVE_SOURCE_BODY_COLLIDERS' in text and 'NATIVE_COLLISION_FIXTURE_PROXY' not in text and 'RECONSTRUCTED_COMPONENT_MOUNTS' in text
    if os.environ.get('INTERLAGOS_SEAM_WELD') in ('1','2'):ok=ok and 'INTERLAGOS_MEASURED_SEAM candidate' in text
    if os.environ.get('INTERLAGOS_SEAM_WELD')=='2':ok=ok and 'INTERLAGOS_SECOND_MEASURED_SEAM candidate' in text
    if os.environ.get('INTERLAGOS_SPIN_DIAGONAL') in ('1','2'):ok=ok and 'INTERLAGOS_SPIN_DIAGONAL candidate' in text
    if os.environ.get('INTERLAGOS_SPIN_DIAGONAL')=='2':ok=ok and 'INTERLAGOS_LATER_SPIN_DIAGONAL candidate' in text
    row={'track':track,'car':car,'ok':bool(ok),'result':result}
   except RuntimeError as e:row={'track':track,'car':car,'ok':False,'error':str(e)}
   rows.append(row);(out/'mounted-native-layout-results.json').write_text(json.dumps(rows,indent=2))
 assert len(rows)==(len(recovery) if recovery is not None else 5*len(cars)) and all(x['ok'] for x in rows),'See preserved exact layout/car failures; no rescue, legal-lap, UI or hull gates weakened'
 print('PASS native mounted hull layout shard; full traffic/weather/art/Windows acceptance separate',flush=True);sys.exit(0)
if os.environ.get('NATIVE_AI_LAP')=='1':
 track=(0,43)[shard];car=(0,16)[shard];label=f'native-ai-lap-track-{track:02}-car-{car:02}';report=out/(label+'.json')
 values={'MYSIM_TRACK':str(track),'MYSIM_CAR':str(car),'MYSIM_NATIVE_GAMEPLAY':'1','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_LAPS':'1','MYSIM_SECONDS':'480','MYSIM_RIVALS':'0','MYSIM_PRECISE_AI':'3','MYSIM_NATIVE_GAMEPLAY_REPORT':str(report)}
 if os.environ.get('NATIVE_HULL_GAMEPLAY')=='1':values['MYSIM_REQUIRE_NATIVE_CAR_HULLS']='1'
 if os.environ.get('NATIVE_PIT_SERVICE')=='1':values.update(MYSIM_NATIVE_PIT_CHECKS='1',MYSIM_COMPONENT_MOUNTS='1',MYSIM_SECONDS='1200',MYSIM_LAPS='2')
 if os.environ.get('NATIVE_TRAFFIC_LAP')=='1':values.update(MYSIM_NATIVE_TRAFFIC='1',MYSIM_RIVALS='1',MYSIM_NATIVE_GAMEPLAY_CHECKS='1',MYSIM_COMPONENT_MOUNTS='1',MYSIM_LAPS='2')
 text=run(label,values,['--headless'],timeout=1800 if os.environ.get('NATIVE_PIT_SERVICE')=='1' else 1200)
 if os.environ.get('NATIVE_HULL_GAMEPLAY')=='1':assert 'NATIVE_SOURCE_BODY_COLLIDERS' in text and 'NATIVE_COLLISION_FIXTURE_PROXY' not in text
 assert json.loads(report.read_text())['ok'] and 'PASS native force-driven valid lap and results UI' in text
 print('COMPLETE genuine solo native AI lap/results; full hull/traffic/game gates separate',flush=True);sys.exit(0)
if os.environ.get('NATIVE_HULL_CONTACT')=='1':
 cars=[shard*2,shard*2+1] if os.environ.get('NATIVE_HULL_MATRIX')=='1' else [(0,16)[shard]]
 recovery=json.loads(pathlib.Path('data/native_hull_recovery.json').read_text())['cases'] if os.environ.get('NATIVE_HULL_RECOVERY')=='1' else None
 if recovery:cars=sorted(set(x['car'] for x in recovery))
 failures=[]
 for car in cars:
  label=f'native-hull-contact-car-{car:02}';values={'MYSIM_NATIVE_HULL_CONTACT_CHECKS':str(car),'MYSIM_REQUIRE_NATIVE_CAR_HULLS':'1'}
  if os.environ.get('NATIVE_HULL_MATRIX')=='1':values.update(MYSIM_NATIVE_HULL_CONTACT_SPEEDS='20,60,120',MYSIM_COMPONENT_MOUNTS='1')
  if recovery:
   values['MYSIM_COMPONENT_MOUNTS']='1'
   cases=[x for x in recovery if x['car']==car];assert len(set(x['hz'] for x in cases))==1
   values.update(MYSIM_NATIVE_HULL_CONTACT_SPEEDS=','.join(map(str,sorted(set(x['speed'] for x in cases)))),MYSIM_NATIVE_HULL_CONTACT_CASES=','.join(str(x['speed'])+(':'+('front' if x['front'] else 'rear')) for x in cases),MYSIM_NATIVE_CONTACT_HZ=str(cases[0]['hz']))
  text='';error=None
  try:text=run(label,values,['--headless'],timeout=360)
  except RuntimeError as e:error=str(e)
  path=out/'native-hull-contact-results.json';result=json.loads(path.read_text()) if path.exists() else {}
  if path.exists():path.rename(out/(label+'.json'))
  expected_hz=int(values.get('MYSIM_NATIVE_CONTACT_HZ',os.environ.get('NATIVE_PHYSICS_HZ','120')))
  if error or result.get('physicsHz')!=expected_hz or not result.get('all') or 'PASS recovered compound hull bumper coverage' not in text:failures.append({'car':car,'error':error or 'Collision gates failed'})
  (out/'hull-case-failures.json').write_text(json.dumps(failures,indent=2))
 assert not failures, 'See preserved per-car results; all selected cars were attempted'
 print('COMPLETE recovered compound body wall impacts; wheel and full-game gates remain open',flush=True);sys.exit(0)
if os.environ.get('NATIVE_GAMEPLAY')=='1':
 track=(0,43)[shard];car=(0,16)[shard];label=f'native-gameplay-track-{track:02}-car-{car:02}'
 values={'MYSIM_TRACK':str(track),'MYSIM_CAR':str(car),'MYSIM_NATIVE_GAMEPLAY':'1','MYSIM_NATIVE_GAMEPLAY_CHECKS':'1','MYSIM_NATIVE_GAMEPLAY_REPORT':str(out/(label+'.json')),'MYSIM_AUTODRIVE':'1','MYSIM_SECONDS':'11','MYSIM_RIVALS':'0'}
 if os.environ.get('NATIVE_HULL_GAMEPLAY')=='1':values['MYSIM_REQUIRE_NATIVE_CAR_HULLS']='1'
 if os.environ.get('NATIVE_COMPONENT_MOUNTS')=='1':values.update(MYSIM_COMPONENT_MOUNTS='1',MYSIM_COMPONENT_MOUNT_REPORT=str(out/(label+'-component-mounts.json')))
 visual=os.environ.get('NATIVE_MOUNTED_VISUAL')=='1'
 if visual:values.update(MYSIM_COMPONENT_MOUNTS='1',MYSIM_DEFER_NATIVE_RENDER='1',MYSIM_SHOT=str(out/(label+'.png')),MYSIM_COMPONENT_MOUNT_REPORT=str(out/(label+'-component-mounts.json')))
 renderer=os.environ.get('NATIVE_VISUAL_RENDERER','gl_compatibility');assert renderer in ('gl_compatibility','mobile')
 if visual and renderer=='mobile':values['LP_NUM_THREADS']='0' # Mesa worker-lifetime isolation; actual rasterization and acceptance gates unchanged
 render_args=['--rendering-method',renderer,'--rendering-driver','vulkan' if renderer=='mobile' else 'opengl3'] if visual else ['--headless']
 (out/(label+'-render-request.json')).write_text(json.dumps({'visual':visual,'requestedRenderer':renderer if visual else 'headless','mesaRasterThreads':values.get('LP_NUM_THREADS','default'),'arguments':render_args,'scope':'Requested backend; verify actual engine startup log before visual acceptance'},indent=2))
 text=run(label,values,render_args,timeout=360)
 if visual:assert (out/(label+'.png')).exists() and 'PASS actual integrated native scene capture' in text
 if os.environ.get('NATIVE_HULL_GAMEPLAY')=='1':assert 'NATIVE_SOURCE_BODY_COLLIDERS' in text and 'NATIVE_COLLISION_FIXTURE_PROXY' not in text
 assert json.loads((out/(label+'.json')).read_text())['ok']
 assert 'PASS integrated native pause, clock, resume, pit options and four camera modes' in text
 assert 'RACE_READY' in text and 'PASS native integrated race HUD, chassis acceleration and braking' in text
 print('COMPLETE opt-in integrated native race flow; no full-game or rival acceptance',flush=True);sys.exit(0)
if os.environ.get('NATIVE_SUSPENSION_TRACK')=='1':
 track=(0,43)[shard];car=(0,16)[shard];label=f'native-suspension-track-{track:02}-car-{car:02}';metadata=out/(label+'.json')
 steer=os.environ.get('NATIVE_SUSPENSION_STEER')=='1';values={'MYSIM_TRACK':str(track),'MYSIM_CAR':str(car),'MYSIM_NATIVE_TRACK_TRIAL':str(metadata)}
 if steer:values.update(MYSIM_NATIVE_TRACK_STEER='right' if shard==0 else 'left',MYSIM_SHOT=str(out/(label+'.png')))
 capture_args=['--rendering-method','gl_compatibility','--rendering-driver','opengl3'] if os.environ.get('NATIVE_CAPTURE_COMPATIBILITY')=='1' else []
 text=run(label,values,capture_args if steer else ['--headless'],timeout=360)
 if steer:assert (out/(label+'.png')).exists()
 assert metadata.exists() and json.loads(metadata.read_text())['ok'] and 'PASS force-driven native track trial' in text
 print('COMPLETE isolated native real-track force trial; full-game acceptance separate',flush=True);sys.exit(0)
if os.environ.get('CAR_PAINT_CANDIDATE')=='1':
 # Source-extrema kingpin41581350664 is rejected for excessive caster/tilt.
 # Rebuild arms between source body anchors and explicit authored upright.
 for travel in (-.04,.04):
  for steer in (-28,0,28):
   for wheel in (0,1):
    label=f'formula-suspension-travel-{travel}-steer-{steer}-wheel-{wheel}';shot=out/(label+'.png');fit=out/(label+'-fit.json')
    text=run(label,{'MYSIM_WHEEL_FIT_TRAVEL':str(travel),'MYSIM_CARVIEW':'16','MYSIM_SOURCE_AXLE_FIT':'1','MYSIM_SOURCE_AXLE_REPORT':str(out/(label+'-axle.json')),'MYSIM_SOURCE_KINGPIN_VISUAL':'1','MYSIM_REBUILT_F1_SUSPENSION':'1','MYSIM_WHEEL_CLOSEUP':str(wheel),'MYSIM_WHEEL_OBLIQUE':'1','MYSIM_WHEEL_FIT_STEER':str(steer),'MYSIM_WHEEL_FIT_AUDIT':str(fit),'MYSIM_COMPONENT_MOUNTS':'1','MYSIM_REBUILT_RIMS':'1','MYSIM_REBUILT_TYRES':'1','MYSIM_REBUILT_BRAKES':'1','MYSIM_CAR_UV_SCOPE':'misc','MYSIM_CAR_UV':'v','MYSIM_CAR_ASSEMBLY_AUDIT':str(out/(label+'-assembly.json')),'MYSIM_SHOT':str(shot)})
    assert shot.exists() and fit.exists() and 'PASS complete visible wheel framing' in text and 'REBUILT_F1_SUSPENSION car=16' in text and 'PASS repeated component setup preserves fitted assembly car=16' in text
 print('COMPLETE authored suspension visual candidate; native contacts and full swept clearance remain unverified',flush=True);sys.exit(0)

if os.environ.get('CAR_UV_CHANNEL_DIAGNOSTIC')=='1':
 transpose=os.environ.get('FORMULA_ATLAS_ORIENTATION')=='1'
 for channel in (('0',) if transpose or os.environ.get('CAR_UV_STATS_ONLY')=='1' else ('0','1')):
  for mode in (('both',) if os.environ.get('CAR_UV_STATS_ONLY')=='1' else ('both','none','u','v')):
   label=('car16-transpose' if transpose else 'car16-channel')+f'-{channel}-{mode}';shot=out/(label+'.png')
   values={'MYSIM_CARVIEW':'16','MYSIM_FITVIEW':'1','MYSIM_CAMPOS':'4,2.5,4','MYSIM_CAR_UV_CHANNEL':channel,'MYSIM_CAR_UV':mode,'MYSIM_CAR_UV_SCOPE':'livery','MYSIM_CAR_ASSEMBLY_AUDIT':str(out/(label+'-assembly.json')),'MYSIM_SHOT':str(shot)}
   if os.environ.get('CAR_UV_STATS_ONLY')=='1':values['MYSIM_FORMULA_UV_TOPOLOGY']='1'
   if transpose:values['MYSIM_FORMULA_ATLAS_TRANSPOSE']='1'
   text=run(label,values);assert shot.exists() and 'PASS complete visible car framing' in text
 if os.environ.get('CAR_UV_STATS_ONLY')=='1':
  from cloud_formula_uv_topology import render
  render(r,out)
  # Verify production exact3panel repairs without global UV diagnostic overrides.
  for view,camera in enumerate(('4,2.5,4','4,2.5,-4','-4,2.5,-4','-4,2.5,4')):
   label=f'car16-panel-repair-view-{view}';shot=out/(label+'.png')
   text=run(label,{'MYSIM_CARVIEW':'16','MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_CAR_ASSEMBLY_AUDIT':str(out/(label+'-assembly.json')),'MYSIM_SHOT':str(shot)})
   assert shot.exists() and 'PASS complete visible car framing' in text and 'CARMAP_SOURCE_ROLE_ALIAS LOD_A_WHEEL_mm -> LOD_A_WHEEL_mm_wheel' in text and 'SURFACE_UV_REPAIR 16/LOD_A_BODY_mm_ext' in text and 'SURFACE_UV_REPAIR 16/LOD_A_FRONTBUMPER_mm_ext' in text and 'SURFACE_UV_REPAIR 16/LOD_A_REARBUMPER_mm_ext' in text
 print('COMPLETE isolated source UV channel comparison; production unchanged',flush=True);sys.exit(0)
if os.environ.get('F1_WHEEL_DIAGNOSTIC')=='1':
 for contrast in ('1.14','1.0'):
  label='wheel-grade-'+contrast;shot=out/(label+'.png')
  text=run(label,{'MYSIM_CARVIEW':'15','MYSIM_REBUILT_RIMS':'1','MYSIM_REBUILT_TYRES':'1','MYSIM_WHEEL_CLOSEUP':'0','MYSIM_WHEEL_OBLIQUE':'1','MYSIM_COMPOUND':'3','MYSIM_CONTRAST':contrast,'MYSIM_WHEEL_APPEARANCE_AUDIT':str(out/(label+'-appearance.json')),'MYSIM_SHOT':str(shot)})
  assert shot.exists() and 'PASS complete visible wheel framing' in text
 print('COMPLETE isolated wheel colour-grade comparison; no visual acceptance',flush=True);sys.exit(0)
if os.environ.get('NIGHT_FIXTURE_AUDIT')=='1':
 track=(25,28)[shard];report=out/f'source-night-fixtures-{track}.json'
 text=run(f'source-night-fixtures-{track}',{'MYSIM_TRACK':str(track),'MYSIM_AUTODRIVE':'1','MYSIM_NIGHT_FIXTURE_AUDIT':str(report),'MYSIM_NIGHT_FIXTURE_VIEW':str(out/f'source-fixture-captures-{track}.json'),'MYSIM_DISABLE_REVIEWED_FIXTURES':'1'},timeout=300)
 captures=json.loads((out/f'source-fixture-captures-{track}.json').read_text());assert len(captures['captures'])>=4 and all(pathlib.Path(x['image']).exists() for x in captures['captures'])
 data=json.loads(report.read_text());assert data['track']==track and data['sourceMeshes'] and data['candidateAttachments'] and 'SOURCE_NIGHT_FIXTURE_AUDIT' in text
 assert all(not row['validated'] for row in data['candidateAttachments']),'Source extraction is not placement validation'
 print('COMPLETE source fixture component audit; all attachments remain unvalidated',flush=True);sys.exit(0)
if os.environ.get('NIGHT_CUTOUT_REVIEW')=='1':
 # Prior matched alpha/shadow modes failed to remove the observed grid.
 for mode in ('baseline','opaque_metal_candidate'):
  label='night25-cutout-'+mode;shot=out/(label+'.png')
  text=run(label,{'MYSIM_TRACK':'25','MYSIM_CAR':'16','MYSIM_TOD':'3','MYSIM_WET':'0','MYSIM_TRACK_FLOODLIGHTS':'1','MYSIM_TRACK_CUTOUT_DIAGNOSTIC':mode,'MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'5','MYSIM_SHOT':str(shot)})
  assert shot.exists() and 'TRACK_CUTOUT_DIAGNOSTIC' in text
 print('COMPLETE matched cutout isolation, no alpha or shadow policy promoted',flush=True);sys.exit(0)
if os.environ.get('F1_PAINT_REVIEW')=='1':
 car=15+shard
 for candidate in (0,1):
  for view,camera in enumerate(('4,2.5,4','-4,2.5,-4','0,4,1')):
   label=f'f1-paint-car-{car}-candidate-{candidate}-view-{view}';shot=out/(label+'.png')
   text=run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_SOURCE_AXLE_FIT':'1','MYSIM_CAR_PAINT_CANDIDATE':str(candidate),'MYSIM_REBUILT_RIMS':'1','MYSIM_REBUILT_TYRES':'1','MYSIM_REBUILT_BRAKES':'1','MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)})
   assert shot.exists() and 'PASS complete visible car framing' in text
 print('COMPLETE four-car original MySim paint candidates; source atlases retained, no art acceptance',flush=True);sys.exit(0)
if os.environ.get('RECOVERED_WET_REVIEW')=='1':
 for candidate in (0,1):
  for wheel in (0,2):
   label=f'recovered-wet-candidate-{candidate}-wheel-{wheel}';shot=out/(label+'.png')
   text=run(label,{'MYSIM_CARVIEW':'15','MYSIM_REBUILT_RIMS':'1','MYSIM_REBUILT_TYRES':'1','MYSIM_REBUILT_BRAKES':'1','MYSIM_WHEEL_LETTERING':'1','MYSIM_RECOVERED_WET_TYRE':str(candidate),'MYSIM_WHEEL_CLOSEUP':str(wheel),'MYSIM_WHEEL_OBLIQUE':'1','MYSIM_COMPOUND':'3','MYSIM_SHOT':str(shot)})
   assert shot.exists() and 'PASS complete visible wheel framing' in text
   if candidate:assert 'RECOVERED_WET_TYRE sourceRadius=' in text
 print('COMPLETE recovered wet source fit comparison; no production promotion',flush=True);sys.exit(0)
if os.environ.get('SOURCE_TYRE_REVIEW')=='1':
 for asset in ('WetTyre','IntermediateTyre','DeepWetTyre'):
  for angle in range(3):
   label=f'source-tyre-{asset}-angle-{angle}';shot=out/(label+'.png');metadata=out/(label+'.json')
   text=run(label,{'MYSIM_SOURCE_TYRE_VIEW':asset,'MYSIM_TYRE_REVIEW_ANGLE':str(angle),'MYSIM_SOURCE_TYRE_REPORT':str(metadata),'MYSIM_SHOT':str(shot)})
   assert shot.exists() and metadata.exists() and 'PASS recovered tyre source capture' in text
 print('COMPLETE recovered tyre geometry comparison; no integration or compound acceptance',flush=True);sys.exit(0)
if os.environ.get('NIGHT_LIGHT_REVIEW')=='1' or os.environ.get('NIGHT_RAIN_SHUTDOWN')=='1' or os.environ.get('SOURCE_FIXTURE_LIGHT_REVIEW')=='1' or os.environ.get('SOURCE_FIXTURE_PROBE')=='1':
 fixture_probe=os.environ.get('SOURCE_FIXTURE_PROBE')=='1'
 source_fixtures=os.environ.get('SOURCE_FIXTURE_LIGHT_REVIEW')=='1' or fixture_probe
 reconstructed_lenses=source_fixtures and os.environ.get('RECONSTRUCTED_FIXTURE_LENS_REVIEW')=='1'
 shutdown_case=os.environ.get('NIGHT_RAIN_SHUTDOWN')=='1'
 track=(25,28)[shard] if source_fixtures else (28 if shutdown_case else (0,25,28)[shard])
 weather_profiles=json.loads((r/'data/weather_profiles.json').read_text())['profiles']
 rainy=next(i for i,profile in enumerate(weather_profiles) if profile['rainAmount']>0 and profile['initialWater']>0)
 (out/f'night-weather-coverage-{track}.json').write_text(json.dumps({'track':track,'profiles':[{'index':i,**weather_profiles[i]} for i in (0,rainy)],'scope':'Explicit dry and actual rainy/wet profiles; former profile1 was overcast with zero water/rain, not wet evidence'},indent=2))
 for wet in ((0,) if fixture_probe else ((rainy,) if shutdown_case else (0,rainy))):
  for candidate in ((1,) if shutdown_case or fixture_probe else (0,1)):
   label=f'night-light-track-{track}-wet-{wet}-candidate-{candidate}'+('-reconstructed-lens' if reconstructed_lenses else '');shot=out/(label+'.png')
   values={'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_TOD':'3','MYSIM_WET':str(wet),'MYSIM_TRACK_FLOODLIGHTS':str(candidate),'MYSIM_TRACK_LIGHT_AUDIT':str(out/(label+'-mounts.json')),'MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'5','MYSIM_SHOT':str(shot)}
   if source_fixtures:
    values['MYSIM_TRACK_FLOODLIGHTS']='0';values['MYSIM_SOURCE_FIXTURE_LIGHTS']=str(candidate);values['MYSIM_SOURCE_FIXTURE_LIGHT_AUDIT']=str(out/(label+'-source-fixtures.json'))
    values['MYSIM_RECONSTRUCTED_FIXTURE_LENS']='1' if reconstructed_lenses and candidate else '0'
   text=run(label,values,timeout=600 if shutdown_case or source_fixtures else 300)
   assert shot.exists()
   if wet==rainy:assert 'RAIN_VOLUME_READY' in text,'Actual rain component did not initialize'
   if candidate:assert ('SOURCE_FIXTURE_LIGHTS candidate' if source_fixtures else 'TRACK_FLOODLIGHTS candidate') in text
   if reconstructed_lenses and candidate:
    fixture_report=out/(label+'-source-fixtures.json');assert fixture_report.exists(),'Measured lens candidate did not publish actual evidence'
    fixture_data=json.loads(fixture_report.read_text());emitters=fixture_data['sourceFixtures'];assert all(lamp.get('reconstructed') is True for lamp in emitters),'Reconstructed candidate silently used legacy emitter placement'
    (out/(label+'-lens-outcome.json')).write_text(json.dumps({'track':track,'weatherProfile':wet,'reconstructedEmitters':len(emitters),'acceptedComponentCount':sum(attempt.get('accepted') is True for attempt in fixture_data.get('attachmentAttempts',[])),'allGatesRetained':True,'manualVisualAcceptance':False,'scope':'Actual zero emitters is a rejected lighting outcome, not working-night or circuit-coverage acceptance'},indent=2))
 print('COMPLETE candidate/baseline night lighting; manual art and collision review required',flush=True);sys.exit(0)
if os.environ.get('CHIRON_ATLAS_REVIEW')=='1':
 assert shard in (0,1)
 for mode in ('none','u','v','both'):
  for view,camera in [('front','4,2,6'),('rear','-4,2,-6')]:
   label=f'chiron-atlas-transpose-{shard}-{mode}-{view}';shot=out/(label+'.png')
   values={'MYSIM_CARVIEW':'20','MYSIM_CHIRON_ATLAS_TRANSPOSE':str(shard),'MYSIM_CAR_UV':mode,'MYSIM_CAR_UV_SCOPE':'livery','MYSIM_CAR_ASSEMBLY_AUDIT':str(out/(label+'-assembly.json')),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)}
   text=run(label,values);assert shot.exists()
   if shard:assert 'CHIRON_ATLAS_TRANSPOSE' in text,'Guarded transpose did not apply'
 print('COMPLETE eight dihedral Chiron exterior mappings; manual reference review required',flush=True);sys.exit(0)
if os.environ.get('CHIRON_WING_REBUILD')=='1':
 for angle,position in [('front','4,2,6'),('rear','-4,2,-6'),('side','7,1.5,0'),('overhead','0,8,0.01')]:
  label='chiron-reconstructed-'+angle;shot=out/(label+'.png');report=out/(label+'-mount.json')
  text=run(label,{'MYSIM_CARVIEW':'20','MYSIM_CHIRON_RECONSTRUCTED_WING':'1','MYSIM_CHIRON_WING_MOUNT_REPORT':str(report),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':position,'MYSIM_SHOT':str(shot)})
  assert shot.exists() and report.exists() and 'CHIRON_RECONSTRUCTED_WING_KIT' in text
  evidence=json.loads(report.read_text());assert evidence['car']==20 and evidence['selected']['minimumSourceVertexDeckGap']>=-.003
 print('COMPLETE reconstructed Chiron kit render and fresh hull evidence; no original pose or full-game acceptance',flush=True);sys.exit(0)
if os.environ.get('FIXTURE_LENS_EXIT')=='1':
 track=(25,28)[shard];label=f'fixture-lens-exit-{track}';destination=out/label
 text=run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_TOD':'3','MYSIM_WET':'0','MYSIM_FIXTURE_LENS_EXIT':str(destination)},timeout=600)
 assert 'PASS actual fixture head UV' in text and destination.with_suffix('.json').exists()
 assert len(list(out.glob(label+'-head-*-side-*.png')))==(2 if track==25 else 4)
 print('COMPLETE exact source lens-head diagnosis; no placement accepted',flush=True);sys.exit(0)
if os.environ.get('CHIRON_WING_AUDIT')=='1':
 focused=os.environ.get('CHIRON_WING_FOCUS')=='1'
 for angle,position in ([('rear','-4,2,-6'),('side','7,1.5,0'),('overhead','0,8,0.01')] if focused else [('front','4,2,6'),('rear','-4,2,-6'),('side','7,1.5,0'),('overhead','0,8,0.01')]):
  label='chiron-wing-'+angle;shot=out/(label+'.png');metadata=out/(label+'.json')
  text=run(label,{'MYSIM_CARVIEW':'20','MYSIM_CHIRON_WING_AUDIT':str(metadata),'MYSIM_CHIRON_WING_FOCUS':'1' if focused else '0','MYSIM_FITVIEW':'1','MYSIM_CAMPOS':position,'MYSIM_SHOT':str(shot)})
  assert shot.exists() and metadata.exists() and ('PASS focused Chiron' if focused else 'PASS read-only Chiron') in text
 print('COMPLETE Chiron source attachment evidence; sockets/mounts NOT verified',flush=True);sys.exit(0)
if os.environ.get('RAIN_MOTION_REVIEW')=='1':
 from cloud_rain_motion import review
 review(run,out,shard);sys.exit(0)
if os.environ.get('SOURCE_KINGPIN_REVIEW')=='1':
 for candidate in (0,1):
  for steer in (-23,-12,0,12,23):
   label=f'w10-kingpin-{candidate}-steer-{steer}';shot=out/(label+'.png');metadata=out/(label+'.json')
   values={'MYSIM_CARVIEW':'15','MYSIM_REBUILT_RIMS':'1','MYSIM_REBUILT_TYRES':'1','MYSIM_SOURCE_AXLE_FIT':'1','MYSIM_SOURCE_KINGPIN_VISUAL':str(candidate),'MYSIM_WHEEL_FIT_STEER':str(steer),'MYSIM_WHEEL_FIT_AUDIT':str(metadata),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':'3.5,4.5,5','MYSIM_SHOT':str(shot)}
   text=run(label,values)
   assert shot.exists() and metadata.exists() and 'PASS wheel fit evidence' in text
   if candidate:assert 'SOURCE_KINGPIN_VISUAL' in text,'Source joint rig did not initialize'
 print('COMPLETE W10 matched visual steering rig evidence; native hub unchanged, not production acceptance',flush=True);sys.exit(0)
if os.environ.get('WHEEL_FIT_REVIEW')=='1':
 car=15+shard
 for steer in (-23,0,23):
  label=f'wheel-fit-car-{car}-steer-{steer}';shot=out/(label+'.png');metadata=out/(label+'.json')
  text=run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_REBUILT_RIMS':'1','MYSIM_REBUILT_TYRES':'1','MYSIM_WHEEL_FIT_STEER':str(steer),'MYSIM_SOURCE_AXLE_FIT':os.environ.get('SOURCE_AXLE_REVIEW','0'),'MYSIM_SOURCE_AXLE_REPORT':str(out/(label+'-source-axle.json')),'MYSIM_WHEEL_FIT_AUDIT':str(metadata),'MYSIM_CAR_ASSEMBLY_AUDIT':str(out/(label+'-assembly.json')),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':'3.5,4.5,5','MYSIM_SHOT':str(shot)})
  assert shot.exists() and metadata.exists() and 'PASS wheel fit evidence' in text
 print('COMPLETE wheel fit diagnostic; overlap findings require repair, not acceptance',flush=True);sys.exit(0)
if os.environ.get('REBUILT_WHEEL_REVIEW')=='1':
 custom=os.environ.get('F1_WHEEL_REVIEW')=='1';car=15+shard if custom else (1,6,16)[shard]
 for compound in range(4):
  for wheel in ((0,2) if custom else (0,)):
   label=f'rebuilt-wheel-car-{car:02}-compound-{compound}-wheel-{wheel}';shot=out/(label+'.png')
   text=run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_REBUILT_RIMS':'1','MYSIM_REBUILT_TYRES':'1','MYSIM_REBUILT_BRAKES':'1' if custom else '0','MYSIM_WHEEL_LETTERING':'1' if custom else '0','MYSIM_WHEEL_CLOSEUP':str(wheel),'MYSIM_WHEEL_OBLIQUE':'1' if custom else '0','MYSIM_WHEEL_APPEARANCE_AUDIT':str(out/(label+'-appearance.json')),'MYSIM_COMPOUND':str(compound),'MYSIM_SHOT':str(shot)})
   assert shot.exists() and 'PASS complete visible wheel framing' in text
 print('COMPLETE replacement wheel/compound closeups; manual acceptance required',flush=True);sys.exit(0)
if os.environ.get('CAR_ASSEMBLY_AUDIT')=='1':
 car=(1,6,16)[shard]
 for view,camera in enumerate(('4,2.5,4',) if os.environ.get('NEUTRAL_CAR_REVIEW')=='1' or os.environ.get('REBUILT_BODY_PAINT')=='1' else ('4,2.5,4','-4,2.5,-4')):
  label=f'assembly-audit-car-{car:02}-view-{view}';shot=out/(label+'.png');metadata=out/(label+'.json')
  text=run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_CAR_ASSEMBLY_AUDIT':str(metadata),'MYSIM_NEUTRAL_CAR_MATERIAL':os.environ.get('NEUTRAL_CAR_REVIEW','0'),'MYSIM_REBUILT_BODY_PAINT':os.environ.get('REBUILT_BODY_PAINT','0'),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)})
  assert shot.exists() and metadata.exists() and 'PASS read-only car assembly evidence' in text
 print('COMPLETE read-only assembly diagnostics; visual acceptance remains separate',flush=True);sys.exit(0)
if os.environ.get('REBUILT_RIM_REVIEW')=='1':
 car=(1,6,16)[shard]
 for view,camera in ((1,'0,1.1,-6'),): # New close-up coverage only; completed whole-car views are retained.
  label=f'rebuilt-rim-car-{car:02}-view-{view}';shot=out/(label+'.png')
  values={'MYSIM_CARVIEW':str(car),'MYSIM_REBUILT_RIMS':'1','MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)}
  if view==1:values['MYSIM_WHEEL_CLOSEUP']='0'
  text=run(label,values)
  assert shot.exists() and ('PASS complete visible wheel framing' if view==1 else 'PASS complete visible car framing') in text
 print('COMPLETE rebuilt rim candidate views; manual acceptance required',flush=True);sys.exit(0)
if os.environ.get('ALPHA_POLICY_REVIEW')=='1':
 track=(0,5)[shard]
 for source_policy in (0,2):
  label=f'alpha-track-{track:02}-source-policy-{source_policy}';shot=out/(label+'.png')
  run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_TOD':'0','MYSIM_WET':'0','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'3','MYSIM_SOURCE_ALPHA_POLICY':str(source_policy),'MYSIM_SHOT':str(shot)})
  assert shot.exists()
 print('COMPLETE alpha policy comparison; no production change accepted',flush=True);sys.exit(0)
if os.environ.get('TRACK_SCENE_AUDIT')=='1':
 tracks=[t for t in range(shard,60,12) if t not in (0,5)] if os.environ.get('TRACK_SCENE_ALL')=='1' else [(0,5)[shard]]
 results=[]
 for track in tracks:
  label=f'track-scene-audit-{track:02}';error=None
  try:
   text=run(label,{'MYSIM_TRACK':str(track),'MYSIM_TRACK_SCENE_AUDIT':str(out/(label+'.json'))},['--headless'])
   assert 'PASS read-only track scene audit' in text
  except (RuntimeError,AssertionError) as exc:error=str(exc)
  results.append({'track':track,'ok':error is None,'error':error})
  (out/'track-scene-case-results.json').write_text(json.dumps(results,indent=2))
 assert results and all(x['ok'] for x in results),'See exact preserved per-track audit failures'
 print('COMPLETE scene metadata audit; visual and duplicate acceptance separate',flush=True);sys.exit(0)
if os.environ.get('CONTROLS_GAMEPLAY')=='1':
 label='controls-gameplay-crisp';shot=out/(label+'.png')
 text=run(label,{'MYSIM_TRACK':'0','MYSIM_CAR':'16','MYSIM_TOD':'0','MYSIM_WET':'0','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'4','MYSIM_CAMERA_CYCLE_CHECK':'1','MYSIM_CAMERA_FRAME_CHECK':'1','MYSIM_CAM':'0','MYSIM_TOUCH':'1','MYSIM_SHOT':str(shot)})
 assert shot.exists() and 'PASS full camera cycle restores all four wheels' in text and 'PASS complete gameplay car framing' in text
 print('COMPLETE crisp controls in real track scene; visual review separate',flush=True);sys.exit(0)
if os.environ.get('GROUND_REVIEW')=='1':
 track=(0,5)[shard]
 for authored in (0,1):
  label=f'ground-track-{track:02}-authored-{authored}';shot=out/(label+'.png')
  run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_TOD':'0','MYSIM_WET':'0','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'3','MYSIM_AUTHORED_GROUND':str(authored),'MYSIM_SHOT':str(shot)})
  assert shot.exists()
 print('COMPLETE matched authored/current surface comparison; no production acceptance',flush=True);sys.exit(0)
if os.environ.get('RAIN_FOCUS')=='1':
 for car in (6,16):
  for tod,wet in ((0,2),(3,0),(3,3)):
   label=f'rain-focus-track-00-car-{car:02}-tod-{tod}-wet-{wet}';shot=out/(label+'.png')
   text=run(label,{'MYSIM_CAMERA_CYCLE_CHECK':'1','MYSIM_CAM':'0','MYSIM_TRACK':'0','MYSIM_CAR':str(car),'MYSIM_TOD':str(tod),'MYSIM_WET':str(wet),'MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'8','MYSIM_SHOT':str(shot)})
   assert 'PASS full camera cycle restores all four wheels' in text
   assert shot.exists()
 print('COMPLETE first-track rain/light comparison; no full weather or track acceptance',flush=True);sys.exit(0)
if os.environ.get('REAR_PANEL_UV')=='1':
 for view,camera in enumerate(('4,1,0','0,1.15,-6')):
  for node in ('baseline','LOD_A_BODY_mm_ext','LOD_A_BODY_mm_misc','LOD_A_REARBUMPER_mm_ext','LOD_A_REARBUMPER_mm_misc'):
   label=f'rear-panel-{view}-{node}';shot=out/(label+'.png');values={'MYSIM_CARVIEW':'6','MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)}
   if node!='baseline':values.update(MYSIM_CAR_UV='v',MYSIM_CAR_UV_NODE=node)
   text=run(label,values);assert shot.exists() and 'PASS complete visible car framing' in text
 print('COMPLETE isolated rear panel material comparison; no production repair accepted',flush=True);sys.exit(0)
if os.environ.get('LIVERY_UV')=='1':
 for car in (0,6,12,18):
  for view,camera in enumerate(('4,1,0','0,1.15,-6')):
   for mode in ('baseline','none','u','v'):
    label=f'livery-car-{car:02}-view-{view}-{mode}';shot=out/(label+'.png');values={'MYSIM_CARVIEW':str(car),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)}
    if mode!='baseline':values.update(MYSIM_CAR_UV=mode,MYSIM_CAR_UV_SCOPE='livery')
    text=run(label,values);assert shot.exists() and 'PASS complete visible car framing' in text
 print('COMPLETE isolated livery UV comparison; production unchanged',flush=True);sys.exit(0)
if os.environ.get('GUI_REVIEW')=='1':
 for resolution in ('640x360','960x540','1600x900'):
  label='results-'+resolution;shot=out/(label+'.png')
  text=run(label,{'MYSIM_RESULTSCHECKS':'1','MYSIM_TOUCH':'1','MYSIM_SHOT':str(shot)},['--resolution',resolution]);assert 'PASS actual results layout' in text and shot.exists()
 for resolution in ('960x540','1280x720','1600x900'):
  label='touch-'+resolution;shot=out/(label+'.png')
  text=run(label,{'MYSIM_TOUCHCHECKS':'1','MYSIM_TOUCH':'1','MYSIM_SHOT':str(shot)},['--resolution',resolution]);assert 'PASS independent touch fingers and focus clearing' in text
 for resolution in ('960x540','1280x720','1600x900'):
  label='settings-'+resolution;shot=out/(label+'.png')
  text=run(label,{'MYSIM_SETTINGSCHECKS':'1','MYSIM_SHOT':str(shot)},['--resolution',resolution]);assert 'PASS race settings real controls' in text and shot.exists()
 for resolution in ('960x540','1280x720','1600x900'):
  label='navigation-'+resolution;shot=out/(label+'.png')
  text=run(label,{'MYSIM_NAVCHECKS':'1','MYSIM_SHOT':str(shot)},['--resolution',resolution]);assert 'PASS mode-first navigation routing' in text
 print('COMPLETE actual settings signals and resolution captures; manual layout review separate',flush=True);sys.exit(0)
if os.environ.get('WEATHER_REVIEW')=='1':
 for track in range(shard,60,6):
  for tod,wet in ((0,0),(3,0),(0,2),(3,3),(0,4)):
   label=f'weather-track-{track:02}-tod-{tod}-wet-{wet}';shot=out/(label+'.png')
   run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_TOD':str(tod),'MYSIM_WET':str(wet),'MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'3','MYSIM_SHOT':str(shot)})
   assert shot.exists(),label+' missing render evidence'
 print('COMPLETE track lighting/weather captures; manual acceptance separate',flush=True);sys.exit(0)
if os.environ.get('ASSEMBLY_REVIEW')=='1':
 for car in range(shard,24,6):
  for view,camera in enumerate(('0,1.15,6','4,1,0','0,1.15,-6')):
   label=f'assembled-car-{car:02}-view-{view}';shot=out/(label+'.png')
   text=run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)});assert shot.exists() and 'CARMAP car=' in text
 print('COMPLETE assembled front/side/rear evidence; manual approval separate',flush=True);sys.exit(0)
if os.environ.get('WHEEL_COMPOUNDS')=='1':
 for car in (1,6,16):
  for compound in range(4):
   label=f'wheel-compound-car-{car:02}-{compound}';shot=out/(label+'.png')
   run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_FITVIEW':'1','MYSIM_COMPOUND':str(compound),'MYSIM_CAMPOS':'4,1,0','MYSIM_SHOT':str(shot)});assert shot.exists()
 print('COMPLETE assembled wheel/compound captures; visual acceptance pending',flush=True);sys.exit(0)
if os.environ.get('CAR_PANEL_UV')=='1':
 for view,camera in enumerate(('5.2,1.8,5.6','5.2,1.5,-5.6')):
  for mode in ('baseline','hood'):
   label=f'panel-car-06-{view}-{mode}';shot=out/(label+'.png');values={'MYSIM_CARVIEW':'6','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot),'MYSIM_DISABLE_SURFACE_UV_REPAIRS':'1'}
   if mode=='hood':values.update(MYSIM_CAR_UV='v',MYSIM_CAR_UV_NODE='LOD_A_HOOD_mm_misc')
   run(label,values);assert shot.exists()
 print('COMPLETE single-panel UV comparison; production unchanged',flush=True);sys.exit(0)
if os.environ.get('CAR_UV_ROLES')=='1':
 for car in (1,6,16):
  for scope in ('baseline','misc','nonlivery'):
   label=f'uv-roles-car-{car:02}-{scope}';shot=out/(label+'.png');values={'MYSIM_CARVIEW':str(car),'MYSIM_CAMPOS':'5.2,1.8,5.6','MYSIM_SHOT':str(shot)}
   if scope!='baseline':values.update(MYSIM_CAR_UV='v',MYSIM_CAR_UV_SCOPE=scope)
   run(label,values);assert shot.exists()
 print('COMPLETE material-role UV comparison; production UV remains unchanged',flush=True);sys.exit(0)
if os.environ.get('CONTACT_TRACE')=='1' or os.environ.get('PRECISE_AI')=='1':
 import re
 for track in (43,15):
  label=f'trace-track-{track:02}'
  try:
   text=run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_LAPS':'1','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'1000','MYSIM_CONTACT_TRACE':str(out/(label+'-contacts.jsonl')),'MYSIM_PRECISE_AI':os.environ.get('AI_MODE','1') if os.environ.get('PRECISE_AI')=='1' else '0'},['--headless'],timeout=1800)
   finish=re.search(r'FINISHED.*best=([0-9.]+).*wallHits=([0-9]+).*damage=([0-9.]+)',text);rows.append({'track':track,'ok':bool(finish) and float(finish[1])>0 and int(finish[2])==0 and float(finish[3])==0,'finish':finish[0] if finish else None})
  except RuntimeError as error:rows.append({'track':track,'ok':False,'error':str(error)})
  (out/'contact-trace-status.json').write_text(json.dumps(rows,indent=2))
 assert all(row['ok'] for row in rows)
 print('PASS targeted native scene regression',flush=True);sys.exit(0)
if os.environ.get('WHEEL_ORIENTATION')=='1':
 for car in (1,6,16):
  for mode in ('legacy','source'):
   label=f'wheel-facing-car-{car:02}-{mode}';shot=out/(label+'.png');run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_CAMPOS':'6,1.2,0','MYSIM_WHEEL_MIRROR':mode,'MYSIM_SHOT':str(shot)});assert shot.exists()
 print('COMPLETE source/legacy wheel-facing comparison; production choice pending',flush=True);sys.exit(0)
if os.environ.get('NATIVE_RACES')=='1' or os.environ.get('NATIVE_FOCUS')=='1':
 import re
 cars=[int(x) for x in os.environ.get('NATIVE_CARS','16').split(',')];assert len(set(cars))==len(cars) and all(0<=car<24 for car in cars)
 caseSet=os.environ.get('NATIVE_CASE_SET','shutdown');assert caseSet in ('shutdown','bridge','monaco_ai3','ai4_regression','braking_monaco')
 focusPath={'bridge':'data/native_bridge_regression.json','shutdown':'data/native_shutdown_recovery.json','monaco_ai3':'data/native_monaco_ai3.json','ai4_regression':'data/native_ai4_regression.json','braking_monaco':'data/native_braking_monaco.json'}[caseSet]
 focus=json.loads(pathlib.Path(focusPath).read_text())['cases'] if os.environ.get('NATIVE_FOCUS')=='1' else None
 for track in (sorted(set(x['track'] for x in focus)) if focus else range(shard,60,12)):
  for car in ([x['car'] for x in focus if x['track']==track] if focus else cars):
   label=f'native-car-{car:02}-track-{track:02}'
   try:
    text=run(label,{'MYSIM_TRACK':str(track),'MYSIM_CAR':str(car),'MYSIM_LAPS':'1','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'1000','MYSIM_PRECISE_AI':'4' if focus and caseSet=='ai4_regression' else '3' if focus and caseSet=='monaco_ai3' else os.environ.get('NATIVE_AI_MODE','0'),'MYSIM_CONTACT_TRACE':str(out/(label+'-contacts.jsonl')) if focus else ''},['--headless'],timeout=1800)
    finished=re.search(r'FINISHED.*best=([0-9.]+).*wallHits=([0-9]+).*damage=([0-9.]+)',text)
    ok=bool(finished) and float(finished[1])>0 and int(finished[2])==0 and float(finished[3])==0
    row={'track':track,'car':car,'ok':ok,'finish':finished[0] if finished else None}
   except RuntimeError as e:row={'track':track,'car':car,'ok':False,'error':str(e)}
   row['scope']='native full-scene physics lap; no visual performance acceptance';rows.append(row);(out/'native-race-status.json').write_text(json.dumps(rows,indent=2))
 assert all(x['ok'] for x in rows)
 print('PASS native full-scene race shard (visual gates separate)',flush=True);sys.exit(0)
if os.environ.get('CAR_UV_REVIEW')=='1':
 for car in (1,6,16):
  for mode in (('both',) if os.environ.get('CAR_UV_STATS_ONLY')=='1' else ('both','none','u','v')):
   label=f'uv-car-{car:02}-{mode}';shot=out/(label+'.png');run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_CAMPOS':'5.2,1.8,5.6','MYSIM_CAR_UV':mode,'MYSIM_SHOT':str(shot)});assert shot.exists()
 print('COMPLETE controlled car UV comparisons; production UV unchanged',flush=True);sys.exit(0)
if os.environ.get('INPUT_PHYSICS')=='1':
 text=run('input-physics',{'MYSIM_TRACK':'24','MYSIM_CAR':'16','MYSIM_RIVALS':'0','MYSIM_INPUTCHECKS':'1','MYSIM_TOUCH':'1'},['--headless'],timeout=300)
 assert 'PASS playable input' in text and 'SCREENSHOT_SKIPPED headless input logic only' in text
 print('PASS native game input/physics (headless; visual timeout remains unresolved)',flush=True);sys.exit(0)
if os.environ.get('CAR_MATERIAL_REVIEW')=='1':
 for car in (1,6,16):
  for view,camera in enumerate(('5.2,1.8,5.6','6,1.2,0','5.2,1.5,-5.6')):
   label=f'wheel-material-car-{car:02}-view-{view}';shot=out/(label+'.png');run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_WHEELDBG':'1','MYSIM_SHOT':str(shot)});assert shot.exists()
 print('COMPLETE authored wheel material capture; visual acceptance pending',flush=True);sys.exit(0)
if os.environ.get('COLLISION_PROBE')=='1':
 # Reproduce a heavily flagged layout with fresh source collision, not the obsolete campaign bins.
 track=48;track_list=json.loads((r/'assets/RealRacing/TrackList.json').read_text())['tracks'];map_name=track_list[track]['map']
 folder=out/'fresh-collision';folder.mkdir(exist_ok=True);baked=folder/('col_'+map_name+'.bin')
 text=run('collision-bake',{'MYSIM_BAKECOL':str(track),'MYSIM_BAKEOUT':str(baked)},['--headless'],timeout=600);assert 'BAKECOL map=' in text and baked.exists()
 import hashlib,struct,collections
 data=baked.read_bytes();triangles=struct.unpack('<i',data[:4])[0];assert triangles>0 and len(data)==4+triangles*36
 report={'track':track,'triangles':triangles,'collision_sha256':hashlib.sha256(data).hexdigest(),'scope':'fresh live/baked diagnostic comparison, not game acceptance','results':{}}
 for mode in ('live','baked'):
  result=out/('crash-'+mode+'.jsonl');values={'MYSIM_CRASHMAP':str(track),'MYSIM_CARS':'16','MYSIM_CRASHPTS':'8','MYSIM_ONLYFLAGGED':'1','MYSIM_CRASHOUT':str(result)}
  if mode=='baked':values['MYSIM_COLDIR']=str(folder)
  text=run('collision-'+mode,values,['--headless'],timeout=1800);assert 'CRASHBATCH track=' in text and result.exists()
  records=[json.loads(line) for line in result.read_text().splitlines()];summaries=[x for x in records if x.get('summary')];assert len(summaries)==1 and summaries[0]['crashes']>0
  flags=collections.Counter(flag for row in records if not row.get('summary') for flag in row['flags']);report['results'][mode]={'summary':summaries[0],'flags':dict(flags)}
  (out/'collision-comparison.json').write_text(json.dumps(report,indent=2))
 # Flags remain visible and require diagnosis; running a diagnostic is never quality acceptance.
 assert report['results']['live']['summary']['crashes']==report['results']['baked']['summary']['crashes']
 print('COMPLETE collision diagnostics; inspect flags and live/baked parity',flush=True);sys.exit(0)
if os.environ.get('SURFACE_REVIEW')=='1':
 for track in (5,23,59):
  for authored,shadows in ((False,True),(True,True),(False,False),(True,False)):
   label=f'surface-{track:02}-authored-{int(authored)}-shadows-{int(shadows)}';shot=out/(label+'.png')
   values={'MYSIM_TRACK':str(track),'MYSIM_CAR':'16','MYSIM_AI':'1','MYSIM_AUTODRIVE':'1','MYSIM_NOPIT':'1','MYSIM_RIVALS':'0','MYSIM_SECONDS':'2','MYSIM_AUTHORED_GROUND':str(int(authored)),'MYSIM_SHOT':str(shot)}
   if not shadows:values['MYSIM_NOSHADOW']='1'
   text=run(label,values,timeout=300);assert shot.exists() and 'onSurface=True' in text
 print('PASS surface comparison capture (visual acceptance pending)',flush=True);sys.exit(0)
if shard==0:
 text=run('geometry',{'MYSIM_RUNTIME_CHECKS':'1','MYSIM_GEOMETRYOUT':str(out/'geometry.json')},['--headless']);assert 'PASS runtime' in text
if os.environ.get('GEOMETRY_ONLY')=='1':
 print('PASS isolated full-car geometry validation',flush=True);sys.exit(0)
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
  label=f'car-{car:02}-view-{shard}';shot=out/(label+'.png');run(label,{'MYSIM_CARVIEW':str(car),'MYSIM_FITVIEW':'1','MYSIM_CAMPOS':camera,'MYSIM_SHOT':str(shot)});assert shot.exists()
if shard==0 and os.environ.get('FULL_RACES')!='1':
 text=run('controls-dialog',{'MYSIM_CONTROLCHECKS':'1','MYSIM_SHOT':str(out/'controls-dialog.png')});assert 'PASS controls dialog' in text
 for resolution in ('960x540','1600x900'):
  label='menu-'+resolution
  text=run(label,{'MYSIM_MENUSHOT':str(out/(label+'.png'))},['--resolution',resolution]);assert (out/(label+'.png')).exists()
 text=run('input-controls',{'MYSIM_TRACK':'24','MYSIM_CAR':'16','MYSIM_RIVALS':'0','MYSIM_INPUTCHECKS':'1','MYSIM_TOUCH':'1','MYSIM_SHOT':str(out/'input-controls.png')});assert 'PASS playable input' in text
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
