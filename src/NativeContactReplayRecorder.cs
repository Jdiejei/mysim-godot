using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MySim {
// Diagnostic-only bounded history. Never changes body state, forces, contacts or damage.
// Resource paths reference the separately encrypted original hull/road inputs; no vertices are published here.
public sealed class NativeContactReplayRecorder {
 const int Capacity=128;
 readonly Queue<object> history=new();bool captured;long integration;double elapsed;
 static float[] V(Vector3 p)=>new[]{p.X,p.Y,p.Z};
 static object T(Transform3D t)=>new {origin=V(t.Origin),basisColumns=new[]{V(t.Basis.X),V(t.Basis.Y),V(t.Basis.Z)}};
 public void Observe(NativeSuspensionVehicle body,PhysicsDirectBodyState3D state){
  if(captured||System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_FACE_EVIDENCE")!="1")return;
  integration++;elapsed+=state.Step;
  var snapshot=new {integration,elapsed,step=state.Step,transform=T(state.Transform),linearVelocity=V(state.LinearVelocity),angularVelocity=V(state.AngularVelocity),centerOfMass=V(state.CenterOfMass),inverseMass=state.InverseMass,inverseInertia=V(state.InverseInertia),inverseInertiaTensorColumns=new[]{V(state.InverseInertiaTensor.X),V(state.InverseInertiaTensor.Y),V(state.InverseInertiaTensor.Z)},totalGravity=V(state.TotalGravity),totalLinearDamp=state.TotalLinearDamp,totalAngularDamp=state.TotalAngularDamp,controls=new{body.Controls.throttle,body.Controls.brake,body.Controls.steer,body.Controls.reverse,body.Controls.handbrake},body.WheelAngle,wheelOmega=(float[])body.WheelOmega.Clone(),springLengths=(float[])body.SpringLength.Clone(),priorIntegrationContactForces=Array.ConvertAll(body.ContactForces,V),body.Contacts,chassisContacts=state.GetContactCount()};
  var witnesses=new List<object>();
  for(int i=0;i<state.GetContactCount();i++){
   if(state.GetContactColliderObject(i) is not CollisionObject3D collider||collider.Name.ToString()!="DriveSurface")continue;
   int local=state.GetContactLocalShape(i),remote=state.GetContactColliderShape(i);var localShape=body.ShapeOwnerGetOwner(body.ShapeFindOwner(local)) as CollisionShape3D;var remoteShape=collider.ShapeOwnerGetOwner(collider.ShapeFindOwner(remote)) as CollisionShape3D;
   witnesses.Add(new {contact=i,localShape=local,localShapeName=localShape?.Name.ToString(),localShapeTransform=localShape!=null?T(localShape.Transform):null,localShapeResource=localShape?.Shape?.ResourcePath,collider=collider.GetPath().ToString(),colliderShape=remote,colliderTransform=T(collider.GlobalTransform),colliderShapeTransform=remoteShape!=null?T(remoteShape.Transform):null,colliderShapeResource=remoteShape?.Shape?.ResourcePath,bodyWitness=V(state.GetContactLocalPosition(i)),colliderWitness=V(state.GetContactColliderPosition(i)),normal=V(state.GetContactLocalNormal(i)),reportedBodyPointVelocity=V(state.GetContactLocalVelocityAtPosition(i)),reportedColliderPointVelocity=V(state.GetContactColliderVelocityAtPosition(i))});
  }
  bool spinDiagnostic=double.TryParse(System.Environment.GetEnvironmentVariable("MYSIM_CONTACT_CAPTURE_AFTER"),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out double after)&&double.IsFinite(after)&&after>0&&elapsed>=after&&state.LinearVelocity.Length()>10&&Mathf.Abs(state.AngularVelocity.Y)>.5f;
  if(witnesses.Count==0||(body.Impacts.Evidence.Count==0&&!spinDiagnostic)){history.Enqueue(snapshot);if(history.Count>Capacity)history.Dequeue();return;}
  captured=true;
  var settings=new Dictionary<string,object>();foreach(string key in new[]{"physics/3d/physics_engine","physics/common/physics_ticks_per_second","physics/jolt_physics_3d/simulation/continuous_cd_movement_threshold","physics/jolt_physics_3d/simulation/continuous_cd_max_penetration","physics/jolt_physics_3d/simulation/penetration_slop","physics/jolt_physics_3d/simulation/speculative_contact_distance","physics/jolt_physics_3d/simulation/use_enhanced_internal_edge_removal","physics/jolt_physics_3d/simulation/body_pair_contact_cache_enabled","physics/jolt_physics_3d/collisions/active_edge_threshold","physics/jolt_physics_3d/collisions/collision_margin_fraction","physics/jolt_physics_3d/queries/enable_ray_cast_face_index"})settings[key]=ProjectSettings.GetSetting(key).ToString();
  var configuration=new Dictionary<string,string>();foreach(string key in new[]{"MYSIM_CAR","MYSIM_TRACK","MYSIM_PRECISE_AI","MYSIM_NATIVE_PHYSICS_HZ","MYSIM_NATIVE_CCD","MYSIM_NATIVE_RIDE_HEIGHT","MYSIM_VERTICAL_ROAD_PLAN","MYSIM_NATIVE_ROTATIONAL_TYRES","MYSIM_COMPONENT_MOUNTS","MYSIM_COPLANAR_ROAD_CANDIDATE","MYSIM_SOURCE_AXLE_FIT","MYSIM_CONTACT_CAPTURE_AFTER","MYSIM_NATIVE_COMBINED_ASSIST","MYSIM_INTERLAGOS_SPIN_DIAGONAL","MYSIM_INTERLAGOS_SEAM_WELD"})configuration[key]=System.Environment.GetEnvironmentVariable(key);
  string report=System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_GAMEPLAY_REPORT");string path=report!=null?System.IO.Path.ChangeExtension(report,null)+"-preimpact-replay.json":"outputs/cloud-game/native-preimpact-replay-car-"+body.CarIndex+".json";
  var folder=System.IO.Path.GetDirectoryName(path);if(!string.IsNullOrEmpty(folder))System.IO.Directory.CreateDirectory(folder);
  System.IO.File.WriteAllText(path,JsonSerializer.Serialize(new{schema=2,captureTrigger=spinDiagnostic?"Opt-in bounded DriveSurface spin diagnostic after requested time; not a damage event":"First actual damage event with DriveSurface contact; initial settling excluded without changing damage gates",car=body.CarIndex,historyCapacity=Capacity,priorStates=history.ToArray(),firstContactState=snapshot,witnesses,settings,configuration,bodyConfiguration=new{body.Mass,centerOfMass=V(body.CenterOfMass),body.ContinuousCd,body.CollisionLayer,body.CollisionMask,body.GravityScale,body.LinearDamp,body.AngularDamp,body.RestLength,body.StaticSag,body.SuspensionPreload,body.DampingRatio,body.RotationalTyres,body.DamageEnabled,recoveredWheelCentres=body.RecoveredWheelCentres!=null?Array.ConvertAll(body.RecoveredWheelCentres,V):null,recoveredWheelRadii=body.RecoveredWheelRadii},sourceInputs=new{hullManifest="res://assets/generated_native_car_hulls/car-"+body.CarIndex.ToString("00")+".json",roadSource="Use the unchanged selected track drive-surface input and collider transform from this capture; geometry remains in encrypted source transport"},scope="Frozen first actual DriveSurface contact with preceding integration states. Frames occur at _IntegrateForces entry; cached contact witnesses may predate current pose. Prior force samples are labelled and do not include engine solver impulses. Inputs support reduced replay, not full deterministic game replay or proof of repair."}));
  GD.Print("NATIVE_PREIMPACT_REPLAY captured priorStates=",history.Count," integration=",integration," car=",body.CarIndex);history.Clear();
 }
}
}
