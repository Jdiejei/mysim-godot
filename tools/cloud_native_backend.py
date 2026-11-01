"""CI-only physics backend experiment. Source project defaults are never changed by this tool's authoring commit."""
import os,pathlib,json,re
backend=os.environ.get('NATIVE_PHYSICS_BACKEND','source');assert backend in ('source','jolt')
distance=os.environ.get('NATIVE_SPECULATIVE_DISTANCE','0.02');assert distance in ('0.02','0.005')
penetration=os.environ.get('NATIVE_CCD_PENETRATION','0.01');assert penetration in ('0.01','0.1','0.25')
face_evidence=os.environ.get('NATIVE_ROAD_FACE_EVIDENCE','0')=='1'
p=pathlib.Path('project.godot')
if backend=='jolt':
 text=p.read_text();settings='3d/physics_engine="Jolt Physics"\njolt_physics_3d/simulation/continuous_cd_movement_threshold=0.05\njolt_physics_3d/simulation/continuous_cd_max_penetration=0.01\njolt_physics_3d/simulation/penetration_slop=0.005\n'
 settings=settings.replace('continuous_cd_max_penetration=0.01',f'continuous_cd_max_penetration={penetration}')
 settings+=f'jolt_physics_3d/simulation/speculative_contact_distance={distance}\n'
 if face_evidence:settings+='jolt_physics_3d/queries/enable_ray_cast_face_index=true\n'
 if '[physics]' in text:text=text.replace('[physics]','[physics]\n'+settings,1)
 else:text+='\n[physics]\n'+settings
 p.write_text(text)
out=pathlib.Path('outputs/cloud-game');out.mkdir(parents=True,exist_ok=True);(out/'physics-backend-experiment.json').write_text(json.dumps({'backend':backend,'roadRayFaceIndex':face_evidence and backend=='jolt','speculativeContactDistance':float(distance),'ccdMaxPenetrationFraction':float(penetration),'ccdMovementThresholdFraction':0.05,'scope':'Cloud-only comparison; no production engine promotion'},indent=2))
