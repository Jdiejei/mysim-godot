using Godot;
using MySimulator.Rebuild;
using System;
using System.Collections.Generic;
namespace MySim {
// Actual force-driven bodies use the same race-control bridge as player and AI.
public partial class NativeHybridScenario : Node3D {
 sealed class Trial {public string Name;public NativeSuspensionVehicle Body;public Vehicle Vehicle;public float MaximumForce,Speed;public bool Settled;}
 readonly List<Trial> trials=new();float elapsed;bool launched,ended;
 public override void _Ready(){NativePhysicsClock.Configure();var floor=new StaticBody3D {CollisionLayer=1,CollisionMask=4,Position=new Vector3(60,-.5f,500)};floor.AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=new Vector3(220,1,2000)}});AddChild(floor);
  foreach(string name in new[]{"off","automatic","manual","nonhybrid","brake-harvest"}){int car=name=="nonhybrid"?0:16;var body=new NativeSuspensionVehicle {CarIndex=car,Position=new Vector3(trials.Count*30,.8f,0)};AddChild(body);var vehicle=new Vehicle {carModel=car,human=true,externalPhysics=true,ersAuto=name=="automatic",ersOvertake=name=="manual"||name=="nonhybrid",ersCharge=name=="brake-harvest"?.5f:1};trials.Add(new Trial {Name=name,Body=body,Vehicle=vehicle});}
 }
 public override void _PhysicsProcess(double delta){if(ended)return;try{elapsed+=(float)delta;if(!launched&&elapsed>=3){foreach(var t in trials){t.Settled=t.Body.Contacts==4&&Mathf.Abs(t.Body.LinearVelocity.Y)<.15f;t.Body.LinearVelocity=new Vector3(0,0,25);}launched=true;}
  if(!launched)return;foreach(var t in trials){var b=t.Body;var v=t.Vehicle;if(!b.GlobalPosition.IsFinite()||!b.LinearVelocity.IsFinite())throw new Exception("Nonfinite hybrid body");v.speed=b.LinearVelocity.Length();v.input=t.Name=="brake-harvest"?new DriverInput(0,1,0):new DriverInput(1,0,0);b.BindRaceControls(v,(float)delta);t.MaximumForce=Mathf.Max(t.MaximumForce,b.HybridDriveForce);t.Speed=v.speed;}
  if(elapsed<9)return;var baseline=trials[0];bool all=true;var rows=new List<object>();foreach(var t in trials){bool boosted=t.Name=="automatic"||t.Name=="manual";bool ok=t.Settled&&(boosted?t.Speed>baseline.Speed+.05f&&t.MaximumForce>0&&t.Vehicle.ersCharge<.99f:t.Name=="brake-harvest"?t.Speed<.5f&&t.Vehicle.ersCharge>.5f:t.MaximumForce==0&&Mathf.Abs(t.Vehicle.ersCharge-1)<.00001f);all&=ok;rows.Add(new {trial=t.Name,ok,t.Settled,speed=t.Speed,baselineSpeed=baseline.Speed,maximumHybridForce=t.MaximumForce,charge=t.Vehicle.ersCharge,contacts=t.Body.Contacts});}
  string path=System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_HYBRID_CHECKS");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(new {all,physicsHz=Engine.PhysicsTicksPerSecond,rows,scope="Actual native force boost/manual/automatic/battery-state integration; simplified existing harvest model, not energy-conservation or real-world calibration acceptance"},new System.Text.Json.JsonSerializerOptions {WriteIndented=true}));ended=true;GD.Print(all?"PASS native ERS acceleration and battery-state bridge":"FAIL native ERS acceleration or battery-state bridge");CleanExit.Request(this,all?0:1);
 }catch(Exception e){ended=true;GD.PushError(e.ToString());CleanExit.Request(this,1);}}
}
}
