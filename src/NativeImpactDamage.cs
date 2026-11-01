using Godot;
using MySimulator.Rebuild;
using System;
using System.Text.Json;
namespace MySim {
// Measure actual body velocity changes; do not sum Jolt's estimated manifold impulses.
// https://docs.godotengine.org/en/stable/tutorials/physics/using_jolt_physics.html#contact-impulses
public sealed class NativeImpactDamage {
 sealed class Profile {public Profile(){}public float minimumDeltaV {get;set;}public float fullDamageDeltaV {get;set;}public float eventCooldownSeconds {get;set;}public float angularRadiusWheelbaseFraction {get;set;}public float frontRearAeroLoss {get;set;}public float rearPowerLoss {get;set;}public float dragIncrease {get;set;}}
 static readonly Lazy<Profile> profile=new(()=>{var p=JsonSerializer.Deserialize<Profile>(FileAccess.GetFileAsString("res://data/native_damage_profile.json"));if(p.minimumDeltaV<=0||p.fullDamageDeltaV<=p.minimumDeltaV||p.eventCooldownSeconds<=0)throw new Exception("Invalid native damage profile");return p;});
 public readonly System.Collections.Generic.List<object> Evidence=new();
 Vector3 previousVelocity,previousAngular;bool valid;float cooldown;public int Events {get;private set;}public float MaximumDeltaV {get;private set;}
 public void ResetHistory(){valid=false;cooldown=0;}
 public void Observe(PhysicsDirectBodyState3D state,Vehicle vehicle,int car,NativeSuspensionVehicle body=null){var delta=state.LinearVelocity-previousVelocity;var angular=state.AngularVelocity-previousAngular;bool hadHistory=valid;previousVelocity=state.LinearVelocity;previousAngular=state.AngularVelocity;valid=true;cooldown=Mathf.Max(0,cooldown-state.Step);if(!hadHistory||vehicle==null||vehicle.retired||state.GetContactCount()==0)return;
  var p=profile.Value;float radius=RecoveredGeometry.Wheelbase[car]*p.angularRadiusWheelbaseFraction;float equivalent=Mathf.Sqrt(delta.LengthSquared()+angular.LengthSquared()*radius*radius);MaximumDeltaV=Mathf.Max(MaximumDeltaV,equivalent);if(cooldown>0||equivalent<=p.minimumDeltaV)return;
  float addition=Mathf.Clamp((equivalent*equivalent-p.minimumDeltaV*p.minimumDeltaV)/(p.fullDamageDeltaV*p.fullDamageDeltaV-p.minimumDeltaV*p.minimumDeltaV),0,1);vehicle.damage=Mathf.Clamp(vehicle.damage+addition,0,1);
  // Directional regions: front/rear/left/right/underfloor/upper/general. Remaining source slots are preserved.
  var local=state.Transform.Basis.Transposed()*(-delta);int region=6;if(local.LengthSquared()>.001f){var a=local.Abs();region=a.Y>a.X&&a.Y>a.Z?(local.Y<0?4:5):a.Z>a.X?(local.Z>0?0:1):(local.X<0?2:3);}vehicle.damageRegions[region]=Mathf.Clamp(vehicle.damageRegions[region]+addition,0,1);Events++;cooldown=p.eventCooldownSeconds;
  if(Evidence.Count<256){var contacts=new System.Collections.Generic.List<object>();for(int i=0;i<state.GetContactCount();i++){var normal=state.GetContactLocalNormal(i);var point=state.GetContactLocalPosition(i);var collider=state.GetContactColliderObject(i) as Node;var colliderVelocity=state.GetContactColliderVelocityAtPosition(i);contacts.Add(new {collider=collider?.GetPath().ToString(),localShape=state.GetContactLocalShape(i),rawLocalNormal=new[]{normal.X,normal.Y,normal.Z},rawLocalPosition=new[]{point.X,point.Y,point.Z},colliderVelocity=new[]{colliderVelocity.X,colliderVelocity.Y,colliderVelocity.Z}});}Evidence.Add(new {underfloorAtImpact=body?.UnderfloorEvidence(state.Transform),raceElapsed=vehicle.lapElapsed,position=new[]{state.Transform.Origin.X,state.Transform.Origin.Y,state.Transform.Origin.Z},deltaVelocity=new[]{delta.X,delta.Y,delta.Z},deltaAngular=new[]{angular.X,angular.Y,angular.Z},equivalentDeltaV=equivalent,addition,region,contacts});}
  if(vehicle.damage>=.999f){vehicle.retired=true;vehicle.retirementReason="CRASH DAMAGE";vehicle.ersActive=false;}
 }
 public static float PowerFactor(Vehicle car)=>Mathf.Clamp(1-car.damageRegions[1]*profile.Value.rearPowerLoss,0,1);
 public static float AeroFactor(Vehicle car)=>Mathf.Clamp(1-(car.damageRegions[0]+car.damageRegions[1])*profile.Value.frontRearAeroLoss,.1f,1);
 public static float DragFactor(Vehicle car)=>1+car.damage*profile.Value.dragIncrease;
}
}
