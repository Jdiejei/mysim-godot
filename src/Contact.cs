using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Shared ground + wall contact logic used by both the playable DriveController and the headless CrashBatch.
public static class Contact {
 static void Trace(Vehicle vehicle,string phase,Vector3 corrected,Vector3 normal,Godot.Collections.Dictionary info=null){
  string file=System.Environment.GetEnvironmentVariable("MYSIM_CONTACT_TRACE");if(string.IsNullOrEmpty(file))return;string collider=null;string bounds=null;var rawNormal=Vector3.Zero;var point=Vector3.Zero;if(info!=null){if(info.ContainsKey("normal"))rawNormal=(Vector3)info["normal"];if(info.ContainsKey("point"))point=(Vector3)info["point"];}
  if(info!=null&&info.ContainsKey("collider_id")){var node=GodotObject.InstanceFromId((ulong)info["collider_id"].AsInt64()) as Node;if(GodotObject.IsInstanceValid(node)){collider=node.GetPath().ToString();if(node.GetParent() is MeshInstance3D mesh)bounds=(mesh.GlobalTransform*mesh.GetAabb()).ToString();}}
  var row=new {phase,lapElapsed=vehicle.lapElapsed,attempt=System.Environment.GetEnvironmentVariable("MYSIM_PROBE_ATTEMPT"),car=vehicle.carModel,position=new[]{vehicle.position.x,vehicle.position.z},corrected=new[]{corrected.X,corrected.Y,corrected.Z},normal=new[]{normal.X,normal.Y,normal.Z},speed=vehicle.speed,lateral=vehicle.lateral,damage=vehicle.damage,collider,bounds,rawNormal=new[]{rawNormal.X,rawNormal.Y,rawNormal.Z},contactPoint=new[]{point.X,point.Y,point.Z}};System.IO.File.AppendAllText(file,System.Text.Json.JsonSerializer.Serialize(row)+"\n");
 }
 public static Vector3 BodySize(int car)=>BodySize(new Vector3(VehicleDimensions.Width[car],VehicleDimensions.Height[car],VehicleDimensions.Length[car]));
 public static Vector3 BodySize(Vector3 measured){if(!measured.IsFinite()||measured.X<=0||measured.Y<=0||measured.Z<=0)throw new System.ArgumentException("Invalid measured collision dimensions");return measured;}
 public static Vector3 ToWorld(Vec p, float y) => new Vector3(p.x, y, p.z);
 public struct GroundResult { public bool found, near; public float y, rawY; public Vector3 normal; public bool clamped; }
 // Ground = drive-surface layer only (mask 1), continuous with the previous height (+-dyMax per call).
 public static GroundResult Ground(PhysicsDirectSpaceState3D space, Vector3 w, float refY, float dyMax = 0.6f) {
  var g = new GroundResult { y = refY, rawY = refY, normal = Vector3.Up };
  using var rq = PhysicsRayQueryParameters3D.Create(new Vector3(w.X, refY + 2.5f, w.Z), new Vector3(w.X, refY - 6f, w.Z)); rq.CollisionMask = 1; rq.HitBackFaces = true;
  var hit = space.IntersectRay(rq); bool nearHit = hit.Count > 0;
  if (hit.Count == 0) { using var rq2 = PhysicsRayQueryParameters3D.Create(new Vector3(w.X, refY + 40f, w.Z), new Vector3(w.X, refY - 80f, w.Z)); rq2.CollisionMask = 1; rq2.HitBackFaces = true; hit = space.IntersectRay(rq2); }
  if (hit.Count == 0) return g;
  g.found = true; g.near = nearHit; g.rawY = ((Vector3)hit["position"]).Y; g.normal = (Vector3)hit["normal"]; if (g.normal.Y < 0) g.normal = -g.normal;
  g.y = Mathf.Clamp(g.rawY, refY - dyMax, refY + dyMax); g.clamped = Mathf.Abs(g.rawY - refY) > dyMax; return g;
 }
 // Invisible containment: if the car leaves the drive-surface mesh (no ground under it) or goes further than limit metres from the centreline, put it back and bounce it. Barrier coverage gaps otherwise let it escape.
 sealed class GroundHistory { public Vec Position; }
 static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Vehicle, GroundHistory> lastGood = new();
 public static void MarkGood(Vehicle v, bool groundNear) { if (groundNear) lastGood.GetOrCreateValue(v).Position = v.position; }
 public static bool Contain(Vehicle v, Vec start, bool groundNear, float limit,Circuit circuit=null) {
  if(circuit!=null)v.progress=circuit.Find(v.position,out v.lateral);
  if (v.pitActive || v.servicing) return false; // pit lane may lie off the drive-surface mesh; the pit sequence is scripted
  if (groundNear && Mathf.Abs(v.lateral) <= limit) return false;
  if (groundNear == false && Mathf.Abs(v.lateral) <= 6f) return false; // holes inside the road are not walls
  v.position = lastGood.TryGetValue(v, out var g0) ? g0.Position : start; v.velocity = new Vec(-v.velocity.x * 0.3f, -v.velocity.z * 0.3f); v.yawRate = 0; v.speed = Mathf.Sqrt(v.velocity.x * v.velocity.x + v.velocity.z * v.velocity.z); v.damage = Mathf.Min(1f, v.damage + 0.01f);Trace(v,"containment",ToWorld(v.position,0),Vector3.Zero);v.lastIntegrated=v.position;if(circuit!=null)v.progress=circuit.Find(v.position,out v.lateral);return true;
 }
 // Box sweep against barrier colliders (layer 2). Returns true if a wall was hit. carY = current car height.
 // A deeply embedded planar box can receive a vertical minimum-separation normal. Search only
 // horizontal, bounded exits using the real shape; never lift the car or invent a wall normal.
 static Vector3 PlanarExit(PhysicsDirectSpaceState3D space,PhysicsShapeQueryParameters3D query,Vector3 origin,Vector3 size){
  var original=query.Transform;Vector3 best=Vector3.Zero;float bestDistance=float.PositiveInfinity,limit=Mathf.Max(size.X,size.Z)+.5f;
  for(int direction=0;direction<16;direction++){
   float angle=direction*Mathf.Tau/16;var axis=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));float blocked=0;
   for(float distance=.1f;distance<=limit*2;distance*=2){
    float probe=Mathf.Min(distance,limit);if(probe>=bestDistance)break;query.Transform=new Transform3D(original.Basis,origin+axis*probe);
    if(space.IntersectShape(query,1).Count==0){float clear=probe;for(int refine=0;refine<8;refine++){float mid=(blocked+clear)*.5f;query.Transform=new Transform3D(original.Basis,origin+axis*mid);if(space.IntersectShape(query,1).Count==0)clear=mid;else blocked=mid;}float gap=clear+.02f;query.Transform=new Transform3D(original.Basis,origin+axis*gap);if(space.IntersectShape(query,1).Count==0&&gap<bestDistance){bestDistance=gap;best=axis*gap;}break;}
    blocked=probe;if(probe>=limit)break;
   }
  }
  query.Transform=original;return best;
 }
 public static bool Walls(PhysicsDirectSpaceState3D space, Vehicle v, Vec start, float carY, Vector3 boxSize, Vector3 localCenter=default) {
  if (v.servicing || v.pitActive) return false;
  var motion = ToWorld(v.position - start, 0);var basis=new Basis(Vector3.Up,v.heading);var offset=basis*new Vector3(localCenter.X,0,localCenter.Z); var s3 = ToWorld(start, carY + (localCenter.Y>0?localCenter.Y:boxSize.Y*.5f))+offset;
  // Wheel/ground support is handled on layer1. Keep the chassis sweep slightly above
  // its contact patch, preserving the measured roof and every vertical side face.
  float clearance=Mathf.Min(.04f,boxSize.Y*.1f);var chassisSize=boxSize;chassisSize.Y-=clearance;s3.Y+=clearance*.5f;
  using var chassisShape = new BoxShape3D { Size=chassisSize };
  using var q = new PhysicsShapeQueryParameters3D { Shape = chassisShape, Transform = new Transform3D(new Basis(Vector3.Up,v.heading),s3), CollisionMask=2, Margin=.01f };
  bool touched=false;
  // CastMotion ignores shapes already overlapping at the start. Resolve those explicitly, even at rest.
  for(int iteration=0;iteration<8;iteration++) {
   q.Transform=new Transform3D(q.Transform.Basis,s3);var rest=space.GetRestInfo(q);
   if(rest.Count==0){
    // Concave surfaces can overlap a convex box without supplying a rest normal.
    // Require the exact overlap query before using the same bounded planar separation.
    var overlaps=space.IntersectShape(q,1);if(overlaps.Count==0)break;
    var exit=PlanarExit(space,q,s3,boxSize);if(exit.LengthSquared()<.0001f)break;
    s3+=exit;start=new Vec(s3.X-offset.X,s3.Z-offset.Z);Respond(v,exit.Normalized());Trace(v,"overlap_without_rest",s3,exit.Normalized(),overlaps[0]);touched=true;continue;
   }
   var n=(Vector3)rest["normal"];n.Y=0;n=n.Normalized();if(n.LengthSquared()<=.1f){var exit=PlanarExit(space,q,s3,boxSize);if(exit.LengthSquared()<.0001f)break;s3+=exit;start=new Vec(s3.X-offset.X,s3.Z-offset.Z);Respond(v,exit.Normalized());Trace(v,"deep_overlap",s3,exit.Normalized(),rest);touched=true;continue;}
   var pts=space.CollideShape(q,16);float depth=0;for(int i=0;i+1<pts.Count;i+=2)depth=Mathf.Max(depth,Mathf.Abs((pts[i]-pts[i+1]).Dot(n)));
   s3+=n*(depth+.02f);start=new Vec(s3.X-offset.X,s3.Z-offset.Z);Respond(v,n);Trace(v,"initial_overlap",s3,n,rest);touched=true;
  }
  float dist=motion.Length();if(dist<.0001f){v.position=start;v.lastIntegrated=v.position;return touched;}
  q.Transform=new Transform3D(q.Transform.Basis,s3);q.Motion=motion;
  var cast=space.CastMotion(q);float safe=cast[0],unsafeF=cast[1];
  if(safe>=1f){if(touched)v.position=start+new Vec(motion.X,motion.Z);v.lastIntegrated=v.position;return touched;}
  q.Transform=new Transform3D(q.Transform.Basis,s3+motion*unsafeF);q.Motion=Vector3.Zero;
  var info=space.GetRestInfo(q);Vector3 normal=info.Count>0?(Vector3)info["normal"]:-motion.Normalized();normal.Y=0;normal=normal.Normalized();if(normal.Dot(motion)>0)normal=-normal;
  var stop=s3+motion*(Mathf.Max(0,safe*dist-.02f)/dist);v.position=new Vec(stop.X-offset.X,stop.Z-offset.Z);
  Respond(v,normal);Trace(v,"sweep",stop,normal,info);return true;
 }
 static void Respond(Vehicle v,Vector3 normal) {
  Vec n=new Vec(normal.X,normal.Z);float into=Vec.Dot(v.velocity,n);
  if(into<0){v.velocity-=n*(into*1.1f);v.damage=Mathf.Min(1,v.damage+Mathf.Abs(into)*.002f);v.yawRate*=.5f;}
  // Preserve heading and tangential velocity instead of snapping the car toward an arbitrary wall tangent.
  v.speed=v.velocity.Magnitude;v.lastIntegrated=v.position;
 }
}}

