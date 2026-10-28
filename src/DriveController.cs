using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Player drive loop: recovered Race simulation steps the car, the model is placed on the drive-surface mesh by raycast,
// chase camera 7.2 m back / 2.65 m up with speed lookahead (revised Unity values), no artificial shake.
public partial class DriveController : Node3D {
 public Race Race; public Vehicle Player; public Node3D Car; public Camera3D Cam; public bool AutoDrive; public float AutoSeconds = 8; public Hud Hud; public Vector3 CarSize = new Vector3(2, 1.2f, 4.7f); public int TrackIndex; public Node3D[] WheelPivots; float spinAngle; public int Laps = 3; public bool AiTest; string lastSt = ""; float finT;
 int CamMode = int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_CAM") ?? "0"); bool cWas; bool tabWas; ulong pausedAt; float t, camYaw; int shownMode = -1; bool camInit;
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
 Vector3 ObstructCam(Vector3 from, Vector3 want) { var q = PhysicsRayQueryParameters3D.Create(from, want); q.CollisionMask = 2; q.HitBackFaces = true; var hit = GetWorld3D().DirectSpaceState.IntersectRay(q); if (hit.Count == 0) return want; var p = (Vector3)hit["position"]; var n = (Vector3)hit["normal"]; var dir = (want - from).Normalized(); float dist = Mathf.Max(0.8f, from.DistanceTo(p) - 0.35f); return from + dir * dist; }
 static float DeadZone(float v, float dz) { float a = Mathf.Abs(v); if (a <= dz) return 0; return Mathf.Sign(v) * (a - dz) / (1f - dz); }
 static Vector3 ToWorld(Vec p, float y) => new Vector3(p.x, y, -p.z);
 public override void _PhysicsProcess(double delta) {
  bool tabNow = Input.IsKeyPressed(Key.Tab) || Input.IsKeyPressed(Key.Escape); if (tabNow && !tabWas) Race.paused = !Race.paused; tabWas = tabNow; if (Hud != null) Hud.Paused = Race.paused; if (System.Environment.GetEnvironmentVariable("MYSIM_PAUSETEST") != null) { if (!Race.paused && t > 2f && pausedAt == 0) { Race.paused = true; pausedAt = Time.GetTicksMsec(); } if (Race.paused && Time.GetTicksMsec() - pausedAt > 1500) { GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "pause.png"); GetTree().Quit(); } } if (Race.paused) return; bool escWas = false;
  t += (float)delta;
  float thr = Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1 : 0, brk = Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1 : 0;
  float steer = (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1 : 0) - (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1 : 0);
  var pads = Input.GetConnectedJoypads(); if (pads.Count > 0) { int pid = pads[0]; float ax = DeadZone(Input.GetJoyAxis(pid, JoyAxis.LeftX), 0.08f), rt = Input.GetJoyAxis(pid, JoyAxis.TriggerRight), lt = Input.GetJoyAxis(pid, JoyAxis.TriggerLeft); if (Mathf.Abs(ax) > 0) steer = ax; thr = Mathf.Max(thr, DeadZone(rt, 0.04f)); brk = Mathf.Max(brk, DeadZone(lt, 0.04f)); if (Input.IsJoyButtonPressed(pid, JoyButton.A)) Player.pitRequested = true; }
  if (AutoDrive && !AiTest) { thr = 1; brk = 0; steer = 0; }
  if (AiTest) { Player.human = false; Player.pitRequested = System.Environment.GetEnvironmentVariable("MYSIM_NOPIT") == null; var st = (Player.pitActive ? "A" : "-") + (Player.servicing ? "S" : "-") + (Player.pitDone ? "D" : "-"); if (st != lastSt) { GD.Print("PITSTATE t=", t.ToString("0.0"), " ", st, " progress=", Player.progress.ToString("0.000"), " speed=", Player.speed.ToString("0.0"), " lap=", Player.completedLaps, " wear=", Player.Wear.ToString("0.00")); lastSt = st; } }
  if (Input.IsKeyPressed(Key.P)) Player.pitRequested = true;
  Player.input = new DriverInput(thr, brk, steer) { handbrake = Input.IsKeyPressed(Key.Space) };
  var startPos = Player.position; Race.Advance((float)delta); ResolveWalls(startPos);
  var space = GetWorld3D().DirectSpaceState; var w = ToWorld(Player.position, 0);
  var hit = space.IntersectRay(PhysicsRayQueryParameters3D.Create(new Vector3(w.X, w.Y + 60, w.Z), new Vector3(w.X, w.Y - 200, w.Z)));
  float y = hit.Count > 0 ? ((Vector3)hit["position"]).Y : Car.Position.Y; Vector3 n = hit.Count > 0 ? (Vector3)hit["normal"] : Vector3.Up;
  float h = Player.heading; var fwd = new Vector3(Mathf.Sin(h), 0, -Mathf.Cos(h));
  Car.Position = new Vector3(w.X, y + 0.05f, w.Z);
  var f2 = (fwd - n * fwd.Dot(n)).Normalized(); var carX = n.Cross(f2).Normalized(); Car.Basis = new Basis(carX, n, f2); /* proper rotation (det +1): nose (+Z local) = travel direction. The old basis was left-handed and Orthonormalized() left it mirrored, so the nose faced the camera. */
  if (WheelPivots != null) { spinAngle += Player.speed * (float)delta / 0.34f; for (int wi = 0; wi < 4; wi++) { var sp = WheelPivots[wi].GetNode<Node3D>("Spin"); float sgn = WheelPivots[wi].Position.X < 0 ? -1f : 1f; sp.Rotation = new Vector3(spinAngle * sgn, 0, 0); if (wi < 2) { var rot = WheelPivots[wi].Position.X < 0 ? Mathf.Pi : 0f; WheelPivots[wi].Rotation = new Vector3(0, rot - Player.steering, 0); } } }
  // chase camera
  bool cNow = Input.IsKeyPressed(Key.C) || (Input.GetConnectedJoypads().Count > 0 && Input.IsJoyButtonPressed(Input.GetConnectedJoypads()[0], JoyButton.Y)); if (cNow && !cWas) CamMode = (CamMode + 1) % 4; cWas = cNow; if (CamMode != shownMode) { shownMode = CamMode; foreach (var mn in Car.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)mn; if (!mi.HasMeta("grp")) continue; string gp = mi.GetMeta("grp").AsString(); mi.Visible = gp == "int" ? CamMode == 2 : CamMode != 2; } }
  float speed = Player.speed; float look = Mathf.Clamp(speed / 20f, 0, 1) * 4f + 3f;
  if (CamMode == 0 || CamMode == 1) {
   float len = Mathf.Max(CarSize.Z, 3.5f), hgt = Mathf.Max(CarSize.Y, 1.0f); float back = CamMode == 0 ? len * 1.3f : len * 2.1f, up = CamMode == 0 ? hgt * 1.35f + 0.25f : hgt * 2.4f; var target = ObstructCam(Car.Position + Vector3.Up * (hgt * 0.8f), Car.Position - fwd * back + Vector3.Up * up);
   if (!camInit) { Cam.Position = target; camInit = true; } Cam.Position = Cam.Position.Lerp(target, 1f - Mathf.Exp(-6f * (float)delta)); Cam.LookAt(Car.Position + fwd * look + Vector3.Up * 1.0f); Cam.Fov = Mathf.Lerp(Cam.Fov, 58f + Mathf.Clamp(speed / 80f, 0, 1) * 6f, 1f - Mathf.Exp(-3f * (float)delta));
  } else if (CamMode == 2) { Cam.Position = Car.GlobalTransform * new Vector3(-0.35f, 0.88f, 0.05f); Cam.GlobalTransform = new Transform3D(Car.GlobalTransform.Basis.Rotated(Car.GlobalTransform.Basis.Y.Normalized(), Mathf.Pi), Cam.Position); Cam.Fov = 75f; camInit = false; }
  else { Cam.Position = Car.GlobalTransform * new Vector3(0, 0.62f, 1.9f); Cam.GlobalTransform = new Transform3D(Car.GlobalTransform.Basis.Rotated(Car.GlobalTransform.Basis.Y.Normalized(), Mathf.Pi), Cam.Position); Cam.Fov = 70f; camInit = false; }
  if (AiTest && Player.finished) { finT += (float)delta; if (finT > 1.5f) { GD.Print("FINISHED t=", t.ToString("0.0"), " laps=", Player.completedLaps, " time=", Player.finishTime.ToString("0.00"), " best=", Player.bestLap.ToString("0.00"), " wallHits=", WallHits, " damage=", Player.damage.ToString("0.00")); GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "finish.png"); GetTree().Quit(); } }
  if (AutoDrive && !(AiTest && AutoSeconds > 250) && t >= AutoSeconds) { GD.Print("DRIVETEST t=", t, " speed=", Player.speed, " progress=", Player.progress, " lateral=", Player.lateral, " lap=", Player.completedLaps, " wallHits=", WallHits, " onSurface=", hit.Count > 0, " pos=", Car.Position); GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "drive.png"); GetTree().Quit(); }
 }
}}




















