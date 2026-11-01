using Godot;
using MySimulator.Rebuild;
using System;
namespace MySim {
// Observe an actual driven pit entry/service/exit; never move the car or advance a service clock.
public sealed class NativePitChecks {
 public bool Entered,Serviced,TyresChanged,Repaired,Released,Exited,Blocked;float blockedTime;readonly System.Collections.Generic.List<object> corridor=new();float maximumElapsed;Vector3 boxPose;bool haveBox;
 public bool Completed=>Entered&&Serviced&&TyresChanged&&Repaired&&Released&&Exited;
 public object Evidence=>new {Completed,Entered,Serviced,TyresChanged,Repaired,Released,Exited,Blocked,corridor,maximumElapsed,scope="Actual force-driven pit approach, service freeze, tyre/repair completion and exit; crew art/audio/unsafe traffic release acceptance separate"};
 // Diagnostic only: translate the actual intact compound along each candidate segment.
 // Rotation between segments and full suspension travel still require driven validation.
 static object SweepSourceHulls(NativeRaceController drive,Vec a,Vec b,bool supported,float groundY){
  if(!supported)return new {tested=false,reason="Missing start support"};
  var body=drive.Chassis;var space=drive.GetWorld3D().DirectSpaceState;
  var home=Contact.Ground(space,body.GlobalPosition,body.GlobalPosition.Y);
  var end=Contact.Ground(space,new Vector3(b.x,groundY+1,b.z),groundY+1);
  if(!home.found||!end.found)return new {tested=false,reason="Missing reference or end support"};
  float height=body.GlobalPosition.Y-home.rawY;var delta=b-a;
  var basis=new Basis(Vector3.Up,Mathf.Atan2(delta.x,delta.z));
  var pose=new Transform3D(basis,new Vector3(a.x,groundY+height,a.z));
  var motion=new Vector3(delta.x,end.rawY-groundY,delta.z);
  int count=0,overlapParts=0;float safe=1;string limitingPart=null;
  var children=body.GetChildren();
  foreach(var node in children){if(node is not CollisionShape3D shape||shape.Disabled||shape.Shape is not ConvexPolygonShape3D)continue;
   count++;using var query=new PhysicsShapeQueryParameters3D {Shape=shape.Shape,Transform=pose*shape.Transform,CollisionMask=2,Margin=.005f};
   var hits=space.IntersectShape(query,1);bool overlaps=hits.Count>0;hits.Clear();
   if(overlaps){overlapParts++;safe=0;limitingPart=shape.Name.ToString();continue;}
   query.Motion=motion;var cast=space.CastMotion(query);
   if(cast[0]<safe){safe=cast[0];limitingPart=shape.Name.ToString();}
  }
  children.Clear();return new {tested=count>0,parts=count,overlapParts,safeFraction=safe,limitingPart,referenceHeight=height,scope="Actual source convex shapes translated against wall layer; segment rotations and dynamic tyre support require driven validation"};
 }
 public void Request(Vehicle v){v.pitRequested=true;v.nextCompound=Compound.Hard;v.repairRequested=true;v.damage=.25f;v.damageRegions[0]=.25f;}
 public void Observe(NativeRaceController drive){var v=drive.Player;var body=drive.Chassis;Entered|=v.pitActive;
  if(v.pitActive&&!v.servicing&&!v.pitDone&&v.input.throttle>.5f&&body.LinearVelocity.Length()<.2f&&body.ChassisContacts>0)blockedTime+=1f/Engine.PhysicsTicksPerSecond;else blockedTime=0;
  if(!Blocked&&blockedTime>3){Blocked=true;var surfaces=new System.Collections.Generic.List<(string Name,Aabb Bounds,MeshInstance3D Mesh)>();var sceneMeshes=drive.GetTree().CurrentScene.FindChildren("*","MeshInstance3D",true,false);foreach(var node in sceneMeshes){var mesh=(MeshInstance3D)node;if(mesh.Mesh==null||!mesh.IsVisibleInTree())continue;string name=mesh.Name.ToString();if(!(name.StartsWith("gnd_",StringComparison.OrdinalIgnoreCase)||name.StartsWith("road",StringComparison.OrdinalIgnoreCase)||name.Contains("pit",StringComparison.OrdinalIgnoreCase)))continue;surfaces.Add((mesh.GetPath().ToString(),mesh.GlobalTransform*mesh.GetAabb(),mesh));}sceneMeshes.Clear();var worldFaces=new System.Collections.Generic.Dictionary<string,Vector3[]>();var route=drive.Race.track;float distance;int nearest=(int)route.FindPit(v.position,out distance);for(int index=Math.Max(0,nearest-4);index<Math.Min(route.PitRoute.Length-1,route.PitServiceIndex+6);index++){
    var point=route.PitPoint(index);var next=route.PitPoint(index+1);var direction=(next-point).Normalized;var right=direction.Right;
    for(int offset=-4;offset<=4;offset++){var a=point+right*offset;var b=next+right*offset;var ground=Contact.Ground(drive.GetWorld3D().DirectSpaceState,new Vector3(a.x,body.GlobalPosition.Y,a.z),body.GlobalPosition.Y);bool blocked=false;string collider=null;
     if(ground.found)for(int edge=-1;edge<=1;edge++){using var query=PhysicsRayQueryParameters3D.Create(new Vector3(a.x+right.x*edge*.95f,ground.rawY+.35f,a.z+right.z*edge*.95f),new Vector3(b.x+right.x*edge*.95f,ground.rawY+.35f,b.z+right.z*edge*.95f),2);query.HitBackFaces=true;var hit=drive.GetWorld3D().DirectSpaceState.IntersectRay(query);if(hit.Count>0){blocked=true;collider=(hit["collider"].AsGodotObject() as Node)?.GetPath().ToString();}hit.Clear();}
     var visibleSupport=new System.Collections.Generic.List<object>();if(!ground.found)foreach(var surface in surfaces){var box=surface.Bounds;if(a.x>=box.Position.X&&a.x<=box.End.X&&a.z>=box.Position.Z&&a.z<=box.End.Z){if(!worldFaces.TryGetValue(surface.Name,out var faces)){faces=surface.Mesh.Mesh.GetFaces();for(int f=0;f<faces.Length;f++)faces[f]=surface.Mesh.GlobalTransform*faces[f];worldFaces[surface.Name]=faces;}int hits=0;float nearestY=float.PositiveInfinity,nearestDelta=float.PositiveInfinity;for(int f=0;f+2<faces.Length;f+=3){var p0=faces[f];var p1=faces[f+1];var p2=faces[f+2];var u=new Vector2(p1.X-p0.X,p1.Z-p0.Z);var w=new Vector2(p2.X-p0.X,p2.Z-p0.Z);var q=new Vector2(a.x-p0.X,a.z-p0.Z);float denominator=u.X*w.Y-u.Y*w.X;if(Mathf.Abs(denominator)<.000001f)continue;float weightB=(q.X*w.Y-q.Y*w.X)/denominator,t=(u.X*q.Y-u.Y*q.X)/denominator;if(weightB<0||t<0||weightB+t>1)continue;var normal=(p1-p0).Cross(p2-p0).Normalized();if(Mathf.Abs(normal.Y)<.5f)continue;float y=p0.Y+weightB*(p1.Y-p0.Y)+t*(p2.Y-p0.Y);hits++;float dy=Mathf.Abs(y-(body.GlobalPosition.Y-.65f));if(dy<nearestDelta){nearestDelta=dy;nearestY=y;}}visibleSupport.Add(new {name=surface.Name,minY=box.Position.Y,maxY=box.End.Y,walkableTriangleHits=hits,nearestY=float.IsFinite(nearestY)?(float?)nearestY:null});}}
     var hullSweep=SweepSourceHulls(drive,a,b,ground.found,ground.rawY);
     corridor.Add(new {index,offset,hullSweep,visibleSupport,x=a.x,z=a.z,groundFound=ground.found,groundNear=ground.near,groundY=ground.rawY,groundClamped=ground.clamped,blocked,collider});
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
