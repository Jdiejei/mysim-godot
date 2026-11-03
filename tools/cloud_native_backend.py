"""CI-only physics backend experiment. Source project defaults are never changed by this tool's authoring commit."""
import os,pathlib,json,re,subprocess,tempfile,hashlib
backend=os.environ.get('NATIVE_PHYSICS_BACKEND','source');assert backend in ('source','jolt')
distance=os.environ.get('NATIVE_SPECULATIVE_DISTANCE','0.02');assert distance in ('0.02','0.005','0')
if distance=='0':assert backend=='jolt' and os.environ.get('NATIVE_REPLAY_CAPTURE_RUN')=='41579600177','Zero speculative distance is restricted to the exact isolated captured-support comparison'
penetration=os.environ.get('NATIVE_CCD_PENETRATION','0.01');assert penetration in ('0.01','0.1','0.25')
face_evidence=os.environ.get('NATIVE_ROAD_FACE_EVIDENCE','0')=='1'
legacy_edges=os.environ.get('NATIVE_LEGACY_INTERNAL_EDGES','0')=='1'
edge_control=None
if legacy_edges:
 assert backend=='jolt','Edge algorithm comparison requires Jolt'
 if not os.environ.get('NATIVE_REPLAY_CAPTURE_RUN'):
  control_run=os.environ.get('NATIVE_EDGE_CONTROL_RUN','');assert control_run.isdigit(),'Gameplay comparison requires a completed sharp-edge control run'
  repo='instagramshashank8-bot/mysim-godot'
  status=json.loads(subprocess.check_output(['gh','run','view',control_run,'-R',repo,'--json','status,conclusion,headSha']))
  assert status['status']=='completed' and status['conclusion']=='success','Sharp-edge control is not successful and terminal'
  with tempfile.TemporaryDirectory(prefix='mysim-edge-control-') as folder:
   subprocess.run(['gh','run','download',control_run,'-R',repo,'-n','game-runtime-8-attempt-1','-D',folder],check=True)
   reports=list(pathlib.Path(folder).rglob('reduced-contact-replay.json'));assert len(reports)==1,'Require one exact control report'
   data=json.loads(reports[0].read_text());control=data.get('sharpEdgeControl',{})
   assert data.get('controlledSettingsAblation') and data.get('controlledChanges')==1 and data.get('enhancedInternalEdgeRemoval')=='false' and not data.get('settingsMatched'),'Wrong edge ablation provenance'
   assert control.get('ok') and control.get('car')==13 and sorted(row['speed'] for row in control['rows'])==[20,60,120],'Missing actual source-car corner speeds'
   assert all(row['ok'] and row['blocked'] and row['Finite'] and row['ccd'] and row['XWallContact'] and row['ZWallContact'] and row['MaximumPenetration']<=.05 for row in control['rows']),'Corner control failed'
   source=next(t for t in data['trials'] if t['kind']=='source-patch')['samples'][0];plane=next(t for t in data['trials'] if t['kind']=='matched-plane')['samples'][0]
   assert all(abs(a-b)<=.0001 for key in ['linearVelocity','angularVelocity'] for a,b in zip(source[key],plane[key])),'Source contact response does not match plane control'
   edge_control={'run':control_run,'headSha':status['headSha'],'sha256':hashlib.sha256(reports[0].read_bytes()).hexdigest(),'verified':True}

p=pathlib.Path('project.godot')
if backend=='jolt':
 text=p.read_text();settings='3d/physics_engine="Jolt Physics"\njolt_physics_3d/simulation/continuous_cd_movement_threshold=0.05\njolt_physics_3d/simulation/continuous_cd_max_penetration=0.01\njolt_physics_3d/simulation/penetration_slop=0.005\n'
 settings=settings.replace('continuous_cd_max_penetration=0.01',f'continuous_cd_max_penetration={penetration}')
 settings+=f'jolt_physics_3d/simulation/speculative_contact_distance={distance}\n'
 if legacy_edges:settings+='jolt_physics_3d/simulation/use_enhanced_internal_edge_removal=false\n'
 if face_evidence:settings+='jolt_physics_3d/queries/enable_ray_cast_face_index=true\n'
 if '[physics]' in text:text=text.replace('[physics]','[physics]\n'+settings,1)
 else:text+='\n[physics]\n'+settings
 p.write_text(text)
out=pathlib.Path('outputs/cloud-game');out.mkdir(parents=True,exist_ok=True);(out/'physics-backend-experiment.json').write_text(json.dumps({'backend':backend,'controlledLegacyInternalEdges':legacy_edges,'sharpEdgeGameplayGate':edge_control,'roadRayFaceIndex':face_evidence and backend=='jolt','speculativeContactDistance':float(distance),'ccdMaxPenetrationFraction':float(penetration),'ccdMovementThresholdFraction':0.05,'scope':'Cloud-only comparison; no production engine promotion'},indent=2))
