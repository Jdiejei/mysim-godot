using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Shared ground + wall contact logic used by both the playable DriveController and the headless CrashBatch.
public static class Contact {
 public static Vector3 ToWorld(Vec p, float y) => new Vector3(p.x, y, -p.z);
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
 static readonly System.Collections.Generic.Dictionary<Vehicle, Vec> lastGood = new System.Collections.Generic.Dictionary<Vehicle, Vec>();
 public static void MarkGood(Vehicle v, bool groundNear) { if (groundNear) lastGood[v] = v.position; }
 public static bool Contain(Vehicle v, Vec start, bool groundNear, float limit) {
  if (groundNear && Mathf.Abs(v.lateral) <= limit) return false;
  if (groundNear == false && Mathf.Abs(v.lateral) <= 6f) return false; // holes inside the road are not walls
  v.position = lastGood.TryGetValue(v, out var g0) ? g0 : start; v.velocity = new Vec(-v.velocity.x * 0.3f, -v.velocity.z * 0.3f); v.yawRate = 0; v.speed = Mathf.Sqrt(v.velocity.x * v.velocity.x + v.velocity.z * v.velocity.z); v.damage = Mathf.Min(1f, v.damage + 0.01f); return true;
 }
 // Box sweep against barrier colliders (layer 2). Returns true if a wall was hit. carY = current car height.
 public static bool Walls(PhysicsDirectSpaceState3D space, Vehicle v, Vec start, float carY, Vector3 boxSize) {
  if (v.servicing) return false; float dx = v.position.x - start.x, dz = v.position.z - start.z; float dist = Mathf.Sqrt(dx * dx + dz * dz); if (dist < 0.0001f) return false;
  var s3 = ToWorld(start, carY + 0.65f); var motion = new Vector3(dx, 0, -dz);
  var q = new PhysicsShapeQueryParameters3D { Shape = new BoxShape3D { Size = boxSize }, Transform = new Transform3D(new Basis(Vector3.Up, -v.heading), s3), Motion = motion, CollisionMask = 2 };
  var r = space.CastMotion(q); float safe = (float)r[0], unsafeF = (float)r[1]; if (safe >= 1f) return false;
  q.Transform = new Transform3D(q.Transform.Basis, s3 + motion * unsafeF); var info = space.GetRestInfo(q); Vector3 nrm = info.Count > 0 ? (Vector3)info["normal"] : -motion.Normalized(); nrm.Y = 0; nrm = nrm.Normalized(); if (nrm.Dot(motion) > 0) nrm = -nrm;
  float back = Mathf.Max(0, safe * dist - 0.1f); float ux = dx / dist, uz = dz / dist; v.position = new Vec(start.x + ux * back, start.z + uz * back);
  Vec n = new Vec(nrm.X, -nrm.Z); float into = v.velocity.x * n.x + v.velocity.z * n.z; bool hit = false;
  if (into < 0) { v.velocity = new Vec(v.velocity.x - n.x * into * 1.1f, v.velocity.z - n.z * into * 1.1f); v.damage = Mathf.Min(1f, v.damage + Mathf.Abs(into) * 0.002f); hit = true;
   // Glancing response: rotate the car toward the wall tangent so it slides along the wall instead of staying pinned nose-first under throttle.
   float fx = Mathf.Sin(v.heading), fz = Mathf.Cos(v.heading); float tx = -n.z, tz = n.x; if (tx * fx + tz * fz < 0) { tx = -tx; tz = -tz; } float target = Mathf.Atan2(tx, tz); float dh = MathUtil.Wrap(target - v.heading); v.heading = MathUtil.Wrap(v.heading + dh * 0.35f); v.yawRate *= 0.5f; v.position = new Vec(v.position.x + n.x * 0.05f, v.position.z + n.z * 0.05f); float tvx = tx * (v.velocity.x * tx + v.velocity.z * tz), tvz = tz * (v.velocity.x * tx + v.velocity.z * tz); v.velocity = new Vec(tvx, tvz); }
  v.speed = Mathf.Sqrt(v.velocity.x * v.velocity.x + v.velocity.z * v.velocity.z); return hit;
 }
}}

