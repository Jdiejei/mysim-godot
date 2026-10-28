using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Player drive loop: recovered Race simulation steps the car, the model is placed on the drive-surface mesh by raycast,
// chase camera 7.2 m back / 2.65 m up with speed lookahead (revised Unity values), no artificial shake.
public partial class DriveController : Node3D {
 public Race Race; public Vehicle Player; public Node3D Car; public Camera3D Cam; public bool AutoDrive; public float AutoSeconds = 8; public Hud Hud; public int TrackIndex; public int Laps = 3; public bool AiTest; string lastSt = ""; float finT;
 float t, camYaw; bool camInit;
 public override void _Ready() {
  Race = new Race(TrackIndex, Laps, 0f); Player = Race.Add("PLAYER", true, Compound.Medium); Race.countdown = 0; Race.started = true; Player.Place(Race.track, 0.988f, 0f); CallDeferred(nameof(BindHud));
 }
 void BindHud() { if (Hud != null) { Hud.Race = Race; Hud.Player = Player; } }
 void ResolveWalls(Vec start) {
  if (Player.servicing) return; float dx = Player.position.x - start.x, dz = Player.position.z - start.z; float dist = Mathf.Sqrt(dx * dx + dz * dz); if (dist < 0.0001f) return;
  var space = GetWorld3D().DirectSpaceState; var s3 = ToWorld(start, Car.Position.Y + 0.65f); var motion = new Vector3(dx, 0, -dz);
  var q = new PhysicsShapeQueryParameters3D { Shape = new BoxShape3D { Size = new Vector3(1.96f, 1.0f, 4.06f) }, Transform = new Transform3D(new Basis(Vector3.Up, -Player.heading), s3), Motion = motion, CollisionMask = 2 };
  var r = space.CastMotion(q); float safe = (float)r[0], unsafeF = (float)r[1]; if (safe >= 1f) return;
  q.Transform = new Transform3D(q.Transform.Basis, s3 + motion * unsafeF); var info = space.GetRestInfo(q); Vector3 nrm = info.Count > 0 ? (Vector3)info["normal"] : -motion.Normalized(); nrm.Y = 0; nrm = nrm.Normalized(); if (nrm.Dot(motion) > 0) nrm = -nrm;
  float back = Mathf.Max(0, safe * dist - 0.1f); float ux = dx / dist, uz = dz / dist; Player.position = new Vec(start.x + ux * back, start.z + uz * back);
  Vec n = new Vec(nrm.X, -nrm.Z); float into = Player.velocity.x * n.x + Player.velocity.z * n.z; if (into < 0) { Player.velocity = new Vec(Player.velocity.x - n.x * into * 1.1f, Player.velocity.z - n.z * into * 1.1f); Player.damage = Mathf.Min(1f, Player.damage + Mathf.Abs(into) * 0.002f); WallHits++; }
  Player.speed = Mathf.Sqrt(Player.velocity.x * Player.velocity.x + Player.velocity.z * Player.velocity.z);
 }
 public int WallHits;
 static Vector3 ToWorld(Vec p, float y) => new Vector3(p.x, y, -p.z);
 public override void _PhysicsProcess(double delta) {
  t += (float)delta;
  float thr = Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1 : 0, brk = Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1 : 0;
  float steer = (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1 : 0) - (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1 : 0);
  if (AutoDrive && !AiTest) { thr = 1; brk = 0; steer = 0; }
  if (AiTest) { Player.human = false; Player.pitRequested = System.Environment.GetEnvironmentVariable("MYSIM_NOPIT") == null; var st = (Player.pitActive ? "A" : "-") + (Player.servicing ? "S" : "-") + (Player.pitDone ? "D" : "-"); if (st != lastSt) { GD.Print("PITSTATE t=", t.ToString("0.0"), " ", st, " progress=", Player.progress.ToString("0.000"), " speed=", Player.speed.ToString("0.0"), " lap=", Player.completedLaps, " wear=", Player.Wear.ToString("0.00")); lastSt = st; } }
  if (Input.IsKeyPressed(Key.P)) Player.pitRequested = true;
  Player.input = new DriverInput(thr, brk, steer) { handbrake = Input.IsKeyPressed(Key.Space) };
  var startPos = Player.position; Race.Step((float)delta); ResolveWalls(startPos);
  var space = GetWorld3D().DirectSpaceState; var w = ToWorld(Player.position, 0);
  var hit = space.IntersectRay(PhysicsRayQueryParameters3D.Create(new Vector3(w.X, w.Y + 60, w.Z), new Vector3(w.X, w.Y - 200, w.Z)));
  float y = hit.Count > 0 ? ((Vector3)hit["position"]).Y : Car.Position.Y; Vector3 n = hit.Count > 0 ? (Vector3)hit["normal"] : Vector3.Up;
  float h = Player.heading; var fwd = new Vector3(Mathf.Sin(h), 0, -Mathf.Cos(h));
  Car.Position = new Vector3(w.X, y + 0.05f, w.Z);
  var right = fwd.Cross(n).Normalized(); var f2 = n.Cross(right).Normalized(); Car.Basis = new Basis(-right, n, -f2).Orthonormalized();
  // chase camera
  float speed = Player.speed; float look = Mathf.Clamp(speed / 20f, 0, 1) * 4f + 3f;
  var target = Car.Position - fwd * 7.2f + Vector3.Up * 2.65f;
  if (!camInit) { Cam.Position = target; camInit = true; }
  Cam.Position = Cam.Position.Lerp(target, 1f - Mathf.Exp(-6f * (float)delta));
  Cam.LookAt(Car.Position + fwd * look + Vector3.Up * 1.0f);
  if (AiTest && Player.finished) { finT += (float)delta; if (finT > 1.5f) { GD.Print("FINISHED t=", t.ToString("0.0"), " laps=", Player.completedLaps, " time=", Player.finishTime.ToString("0.00"), " best=", Player.bestLap.ToString("0.00"), " wallHits=", WallHits, " damage=", Player.damage.ToString("0.00")); GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "finish.png"); GetTree().Quit(); } }
  if (AutoDrive && !(AiTest && AutoSeconds > 250) && t >= AutoSeconds) { GD.Print("DRIVETEST t=", t, " speed=", Player.speed, " progress=", Player.progress, " lateral=", Player.lateral, " lap=", Player.completedLaps, " wallHits=", WallHits, " onSurface=", hit.Count > 0, " pos=", Car.Position); GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "drive.png"); GetTree().Quit(); }
 }
}}









