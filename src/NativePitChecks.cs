using Godot;
using MySimulator.Rebuild;
using System;
namespace MySim {
// Observe an actual driven pit entry/service/exit; never move the car or advance a service clock.
public sealed class NativePitChecks {
 public bool Entered,Serviced,TyresChanged,Repaired,Released,Exited,Blocked;float blockedTime;readonly System.Collections.Generic.List<object> corridor=new();float maximumElapsed;Vector3 boxPose;bool haveBox;
 public bool Completed=>Entered&&Serviced&&TyresChanged&&Repaired&&Released&&Exited;
 public object Evidence=>new {Completed,Entered,Serviced,TyresChanged,Repaired,Released,Exited,Blocked,corridor,maximumElapsed,scope="Actual force-driven pit approach, service freeze, tyre/repair completion and exit; crew art/audio/unsafe traffic release acceptance separate"};
 public void Request(Vehicle v){v.pitRequested=true;v.nextCompound=Compound.Hard;v.repairRequested=true;v.damage=.25f;v.damageRegions[0]=.25f;}
 public void Observe(NativeRaceController drive){var v=drive.Player;var body=drive.Chassis;Entered|=v.pitActive;
  if(v.pitActive&&!v.servicing&&!v.pitDone&&v.input.throttle>.5f&&body.LinearVelocity.Length()<.2f&&body.ChassisContacts>0)blockedTime+=1f/Engine.PhysicsTicksPerSecond;else blockedTime=0;
  if(!Blocked&&blockedTime>3){Blocked=true;var route=drive.Race.track;float distance;int nearest=(int)route.FindPit(v.position,out distance);for(int index=Math.Max(0,nearest-4);index<Math.Min(route.PitRoute.Length-1,nearest+12);index++){
    var point=route.PitPoint(index);var next=route.PitPoint(index+1);var direction=(next-point).Normalized;var right=direction.Right;
    for(int offset=-4;offset<=4;offset++){var a=point+right*offset;var b=next+right*offset;var ground=Contact.Ground(drive.GetWorld3D().DirectSpaceState,new Vector3(a.x,body.GlobalPosition.Y,a.z),body.GlobalPosition.Y);bool blocked=false;string collider=null;
     if(ground.near)for(int edge=-1;edge<=1;edge++){using var query=PhysicsRayQueryParameters3D.Create(new Vector3(a.x+right.x*edge*.95f,ground.rawY+.35f,a.z+right.z*edge*.95f),new Vector3(b.x+right.x*edge*.95f,ground.rawY+.35f,b.z+right.z*edge*.95f),2);query.HitBackFaces=true;var hit=drive.GetWorld3D().DirectSpaceState.IntersectRay(query);if(hit.Count>0){blocked=true;collider=(hit["collider"].AsGodotObject() as Node)?.GetPath().ToString();}hit.Clear();}
     corridor.Add(new {index,offset,groundNear=ground.near,blocked,collider});
    }
   }}
  if(v.servicing){Serviced=true;if(!haveBox){boxPose=body.GlobalPosition;haveBox=true;}if(!body.Freeze||body.GlobalPosition.DistanceTo(boxPose)>.003f)throw new Exception("Native chassis moved during pit service");maximumElapsed=Math.Max(maximumElapsed,v.ServiceElapsed);}
  if(Serviced&&v.tyresReplaced){bool all=true;foreach(var tyre in v.tyres)all&=tyre.compound==Compound.Hard;foreach(var wheel in drive.WheelPivots)all&=wheel.GetMeta("displayed_compound").AsInt32()==(int)Compound.Hard;if(!all)throw new Exception("Native pit tyre physics and visible compounds disagree");TyresChanged=true;}
  if(Serviced&&v.bodyRepaired){if(v.damage!=0)throw new Exception("Native pit bodywork repair did not clear damage");foreach(float region in v.damageRegions)if(region!=0)throw new Exception("Native pit repair left damaged region");Repaired=true;}
  if(Serviced&&!v.servicing){if(body.Freeze||!TyresChanged||!Repaired||maximumElapsed<PitServiceTimeline.Duration+PitServiceTimeline.RepairDuration-.02f)throw new Exception("Native pit released before completed service or stayed frozen");Released=true;}
  if(Released&&!v.pitActive)Exited=true;
 }
}
}
