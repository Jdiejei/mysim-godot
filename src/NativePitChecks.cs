using Godot;
using MySimulator.Rebuild;
using System;
namespace MySim {
// Observe an actual driven pit entry/service/exit; never move the car or advance a service clock.
public sealed class NativePitChecks {
 public bool Entered,Serviced,TyresChanged,Repaired,Released,Exited;float maximumElapsed;Vector3 boxPose;bool haveBox;
 public bool Completed=>Entered&&Serviced&&TyresChanged&&Repaired&&Released&&Exited;
 public object Evidence=>new {Completed,Entered,Serviced,TyresChanged,Repaired,Released,Exited,maximumElapsed,scope="Actual force-driven pit approach, service freeze, tyre/repair completion and exit; crew art/audio/unsafe traffic release acceptance separate"};
 public void Request(Vehicle v){v.pitRequested=true;v.nextCompound=Compound.Hard;v.repairRequested=true;v.damage=.25f;v.damageRegions[0]=.25f;}
 public void Observe(NativeRaceController drive){var v=drive.Player;var body=drive.Chassis;Entered|=v.pitActive;
  if(v.servicing){Serviced=true;if(!haveBox){boxPose=body.GlobalPosition;haveBox=true;}if(!body.Freeze||body.GlobalPosition.DistanceTo(boxPose)>.003f)throw new Exception("Native chassis moved during pit service");maximumElapsed=Math.Max(maximumElapsed,v.ServiceElapsed);}
  if(Serviced&&v.tyresReplaced){bool all=true;foreach(var tyre in v.tyres)all&=tyre.compound==Compound.Hard;foreach(var wheel in drive.WheelPivots)all&=wheel.GetMeta("displayed_compound").AsInt32()==(int)Compound.Hard;if(!all)throw new Exception("Native pit tyre physics and visible compounds disagree");TyresChanged=true;}
  if(Serviced&&v.bodyRepaired){if(v.damage!=0)throw new Exception("Native pit bodywork repair did not clear damage");foreach(float region in v.damageRegions)if(region!=0)throw new Exception("Native pit repair left damaged region");Repaired=true;}
  if(Serviced&&!v.servicing){if(body.Freeze||!TyresChanged||!Repaired||maximumElapsed<PitServiceTimeline.Duration+PitServiceTimeline.RepairDuration-.02f)throw new Exception("Native pit released before completed service or stayed frozen");Released=true;}
  if(Released&&!v.pitActive)Exited=true;
 }
}
}
