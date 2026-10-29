using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Shared ground + wall contact logic used by both the playable DriveController and the headless CrashBatch.
public static class Contact {
 public static Vector3 ToWorld(Vec p, float y) => new Vector3(p.x, y, p.z);
 public struct GroundResult { public bool found, near; public float y, rawY; public Vector3 normal; public bool clamped; }
 // Ground = drive-surface layer only (mask 1), continuous with the previous height (+-dyMax per call).
 public static GroundResult Ground(PhysicsDirectSpaceState3D space, Vector3 w, float refY, float dyMax = 0.6f) {
  var g = new GroundResult { y = refY, rawY = refY, normal = Vector3.Up };
  var rq = PhysicsRayQueryParameters3D.Create(new Vector3(w.X, refY + 2.5f, w.Z), new Vector3(w.X, refY - 6f, w.Z)); rq.CollisionMask = 1; rq.HitBackFaces = true;
  var hit = space.IntersectRay(rq); bool nearHit = hit.Count > 0;
  if (hit.Count == 0) { var rq2 = PhysicsRayQueryParameters3D.Create(new Vector3(w.X, refY + 40f, w.Z), new Vector3(w.X, refY - 80f, w.Z)); rq2.CollisionMask = 1; rq2.HitBackFaces = true; hit = space.IntersectRay(rq2); }
  if (hit.Count == 0) return g;
  g.found = true; g.near = nearHit; g.rawY = ((Vector3)hit["position"]).Y; g.normal = (Vector3)hit["normal"]; if (g.normal.Y < 0) g.normal = -g.normal;
  g.y = Mathf.Clamp(g.rawY, refY - dyMax, refY + dyMax); g.clamped = Mathf.Abs(g.rawY - refY) > dyMax; return g;
 }
 // Invisible containment: if the car leaves the drive-surface mesh (no ground under it) or goes further than limit metres from the centreline, put it back and bounce it. Barrier coverage gaps otherwise let it escape.
 sealed class GroundHistory { public Vec Position; }
 static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Vehicle, GroundHistory> lastGood = new();
 public static void MarkGood(Vehicle v, bool groundNear) { if (groundNear) lastGood.GetOrCreateValue(v).Position = v.position; }
 public static bool Contain(Vehicle v, Vec start, bool groundNear, float limit) {
  if (v.pitActive || v.servicing) return false; // pit lane may lie off the drive-surface mesh; the pit sequence is scripted
  if (groundNear && Mathf.Abs(v.lateral) <= limit) return false;
  if (groundNear == false && Mathf.Abs(v.lateral) <= 6f) return false; // holes inside the road are not walls
  v.position = lastGood.TryGetValue(v, out var g0) ? g0.Position : start; v.velocity = new Vec(-v.velocity.x * 0.3f, -v.velocity.z * 0.3f); v.yawRate = 0; v.speed = Mathf.Sqrt(v.velocity.x * v.velocity.x + v.velocity.z * v.velocity.z); v.damage = Mathf.Min(1f, v.damage + 0.01f); return true;
 }
 // Box sweep against barrier colliders (layer 2). Returns true if a wall was hit. carY = current car height.
 public static bool Walls(PhysicsDirectSpaceState3D space, Vehicle v, Vec start, float carY, Vector3 boxSize) {
  if (v.servicing || v.pitActive) return false;
  var motion = ToWorld(v.position - start, 0); var s3 = ToWorld(start, carY + boxSize.Y * .5f);
  var q = new PhysicsShapeQueryParameters3D { Shape = new BoxShape3D { Size=boxSize }, Transform = new Transform3D(new Basis(Vector3.Up,v.heading),s3), CollisionMask=2, Margin=.01f };
  bool touched=false;
  // CastMotion ignores shapes already overlapping at the start. Resolve those explicitly, even at rest.
  var rest=space.GetRestInfo(q);
  if(rest.Count>0) {
   var n=(Vector3)rest["normal"];n.Y=0;n=n.Normalized();var pts=space.CollideShape(q,16);float depth=0;
   for(int i=0;i+1<pts.Count;i+=2) depth=Mathf.Max(depth,Mathf.Abs((pts[i]-pts[i+1]).Dot(n)));
   if(n.LengthSquared()>.1f) {s3+=n*(depth+.02f);start=new Vec(s3.X,s3.Z);Respond(v,n);touched=true;}
  }
  float dist=motion.Length();if(dist<.0001f){v.position=start;return touched;}
  q.Transform=new Transform3D(q.Transform.Basis,s3);q.Motion=motion;
  var cast=space.CastMotion(q);float safe=cast[0],unsafeF=cast[1];
  if(safe>=1f){if(touched)v.position=start+new Vec(motion.X,motion.Z);v.lastIntegrated=v.position;return touched;}
  q.Transform=new Transform3D(q.Transform.Basis,s3+motion*unsafeF);q.Motion=Vector3.Zero;
  var info=space.GetRestInfo(q);Vector3 normal=info.Count>0?(Vector3)info["normal"]:-motion.Normalized();normal.Y=0;normal=normal.Normalized();if(normal.Dot(motion)>0)normal=-normal;
  var stop=s3+motion*(Mathf.Max(0,safe*dist-.02f)/dist);v.position=new Vec(stop.X,stop.Z);
  Respond(v,normal);return true;
 }
 static void Respond(Vehicle v,Vector3 normal) {
  Vec n=new Vec(normal.X,normal.Z);float into=Vec.Dot(v.velocity,n);
  if(into<0){v.velocity-=n*(into*1.1f);v.damage=Mathf.Min(1,v.damage+Mathf.Abs(into)*.002f);v.yawRate*=.5f;}
  // Preserve heading and tangential velocity instead of snapping the car toward an arbitrary wall tangent.
  v.speed=v.velocity.Magnitude;v.lastIntegrated=v.position;
 }
}}

