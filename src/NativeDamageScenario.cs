using Godot;
using MySimulator.Rebuild;
using System;
using System.Collections.Generic;
namespace MySim {
public partial class NativeDamageScenario : Node3D {
 sealed class Trial {public string Kind;public NativeSuspensionVehicle Body;public Vehicle Car;public int MaximumContacts;}
 readonly List<Trial> trials=new();float elapsed;bool launched,ended;
 public override void _Ready(){NativePhysicsClock.Configure();var floor=new StaticBody3D {CollisionLayer=1,CollisionMask=4,Position=new Vector3(2150,-.5f,200)};floor.AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=new Vector3(4400,1,1000)}});AddChild(floor);
  for(int car=0;car<24;car++)foreach(string kind in new[]{"coast","brake","low","medium","high","rest-contact"}){float x=trials.Count*30;bool wall=kind!="coast"&&kind!="brake";if(wall){var barrier=new StaticBody3D {CollisionLayer=2,CollisionMask=4,Position=new Vector3(x,2,6)};barrier.AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=new Vector3(15,4,.2f)}});AddChild(barrier);}float z=kind=="rest-contact"?5.9f-(RecoveredGeometry.Wheelbase[car]+.5f)*.5f:0;var body=new NativeSuspensionVehicle {CarIndex=car,DamageEnabled=true,Position=new Vector3(x,.8f,z)};AddChild(body);trials.Add(new Trial {Kind=kind,Body=body,Car=new Vehicle {carModel=car,human=true,externalPhysics=true,ersAuto=false}});}
 }
 public override void _PhysicsProcess(double delta){if(ended)return;try{elapsed+=(float)delta;if(!launched&&elapsed>=3){foreach(var t in trials)t.Body.SeedRollingVelocity(new Vector3(0,0,t.Kind=="rest-contact"?0:t.Kind=="low"?5:t.Kind=="high"?60:20));launched=true;}if(!launched)return;
  foreach(var t in trials){var b=t.Body;var v=t.Car;if(!b.GlobalPosition.IsFinite()||!b.LinearVelocity.IsFinite())throw new Exception("Nonfinite damage body");v.speed=b.LinearVelocity.Length();v.input=t.Kind=="brake"||t.Kind=="rest-contact"?new DriverInput(0,1,0):new DriverInput(0,0,0);b.BindRaceControls(v,(float)delta);t.MaximumContacts=Math.Max(t.MaximumContacts,b.ChassisContacts);}
  if(elapsed<9)return;var rows=new List<object>();bool all=trials.Count==144;for(int i=0;i<trials.Count;i++){var t=trials[i];bool ok=t.Body.DamageEnabled&&float.IsFinite(t.Car.damage)&&t.Car.damage>=0&&t.Car.damage<=1;
   if(t.Kind=="coast"||t.Kind=="brake"||t.Kind=="rest-contact")ok&=t.Car.damage==0&&t.Body.Impacts.Events==0&&!t.Car.retired&&(t.Kind!="rest-contact"||t.MaximumContacts>0);
   else if(t.Kind=="low")ok&=t.Car.damage>0&&t.Car.damage<.3f&&t.Car.damageRegions[0]>0;
   else if(t.Kind=="medium")ok&=t.Car.damage>trials[i-1].Car.damage&&t.Car.damageRegions[0]>0;
   else ok&=t.Car.damage>=.999f&&t.Car.retired&&!t.Car.finished&&t.Car.retirementReason=="CRASH DAMAGE";
   all&=ok;rows.Add(new {car=t.Car.carModel,trial=t.Kind,ok,damage=t.Car.damage,regions=(float[])t.Car.damageRegions.Clone(),events=t.Body.Impacts.Events,maximumDeltaV=t.Body.Impacts.MaximumDeltaV,maximumContacts=t.MaximumContacts,retired=t.Car.retired,reason=t.Car.retirementReason});}
  string path=System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_DAMAGE_CHECKS");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));System.IO.File.WriteAllText(path,JsonSerializerCompat(new {all,physicsHz=Engine.PhysicsTicksPerSecond,rows,scope="Actual native collision damage gameplay,24cars proxy-hull fixtures; not source-body crash calibration, deformation, audio or structural certification"}));ended=true;GD.Print(all?"PASS native collision damage, contact stability and crash retirement":"FAIL native damage or collision stability");CleanExit.Request(this,all?0:1);
 }catch(Exception e){ended=true;GD.PushError(e.ToString());CleanExit.Request(this,1);}}
 static string JsonSerializerCompat(object value)=>System.Text.Json.JsonSerializer.Serialize(value,new System.Text.Json.JsonSerializerOptions {WriteIndented=true});
}
}
