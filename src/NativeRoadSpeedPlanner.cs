using Godot;
using MySimulator.Rebuild;
using System;
namespace MySim {
// Opt-in native AI speed planning from real ground heights. Commands pedals; never moves the body.
public sealed class NativeRoadSpeedPlanner {
 float remaining,limit=float.PositiveInfinity,curvature,worstOffset,worstAhead;int samples;
 public object Evidence=>new {speedLimit=float.IsFinite(limit)?(float?)limit:null,maximumVerticalCurvature=curvature,worstOffset,worstAhead,samples,scope="Actual support-height preview; provisional load targets, not real-world calibration"};
 public void Step(Vehicle vehicle,NativeSuspensionVehicle body,Circuit track,float dt){
  if(System.Environment.GetEnvironmentVariable("MYSIM_VERTICAL_ROAD_PLAN")!="1"||track==null||vehicle.human||vehicle.pitActive||vehicle.finished||vehicle.retired)return;
  remaining-=dt;
  if(remaining<=0){remaining=.1f;limit=float.PositiveInfinity;curvature=0;worstOffset=0;worstAhead=0;samples=0;
   using var profile=System.Text.Json.JsonDocument.Parse(FileAccess.GetFileAsString("res://data/native_road_planning.json"));var cfg=profile.RootElement;
   float spacing=cfg.GetProperty("spacingMetres").GetSingle(),distance=cfg.GetProperty("previewMetres").GetSingle(),decel=cfg.GetProperty("planningDeceleration").GetSingle(),crest=cfg.GetProperty("crestGravityFraction").GetSingle(),dip=cfg.GetProperty("dipExtraGravityFraction").GetSingle();
   if(spacing<1||spacing>10||distance<spacing*3||distance>200||decel<=0||crest<=0||dip<=0)throw new Exception("Invalid vertical road planning profile");
   var space=body.GetWorld3D().DirectSpaceState;float halfTrack=0;if(body.RecoveredWheelCentres!=null)foreach(var hub in body.RecoveredWheelCentres)halfTrack=Mathf.Max(halfTrack,Mathf.Abs(hub.X));
   foreach(float offset in new[]{-halfTrack,0f,halfTrack}){float previousY=body.GlobalPosition.Y,previousSlope=0;bool haveSlope=false;
   for(float ahead=0;ahead<=distance;ahead+=spacing){var pos=track.Lane(vehicle.progress+ahead/track.length,offset);var ground=Contact.Ground(space,new Vector3(pos.x,previousY+2,pos.z),previousY+2);
    if(!ground.found||Mathf.Abs(ground.rawY-previousY)>spacing)break;samples++;
    if(ahead>0){float slope=Mathf.Atan((ground.rawY-previousY)/spacing);
     if(haveSlope){float bend=(slope-previousSlope)/spacing;curvature=Mathf.Max(curvature,Mathf.Abs(bend));float target=Mathf.Sqrt(9.81f*(bend<0?crest:dip)/Mathf.Max(.0001f,Mathf.Abs(bend)));float buffer=RecoveredGeometry.Wheelbase[vehicle.carModel]*2+vehicle.speed*.2f;float safe=Mathf.Sqrt(target*target+2*decel*Mathf.Max(0,ahead-spacing-buffer));if(safe<limit){limit=safe;worstOffset=offset;worstAhead=ahead;}}
     previousSlope=slope;haveSlope=true;
    }previousY=ground.rawY;
   }}
  }
  if(vehicle.speed>limit){var input=vehicle.input;input.throttle=0;input.brake=Mathf.Max(input.brake,Mathf.Clamp((vehicle.speed-limit)*.22f,0,1));vehicle.input=input;}
 }
}
}
