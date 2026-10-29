using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Player drive loop: recovered Race simulation steps the car, the model is placed on the drive-surface mesh by raycast,
// chase camera 7.2 m back / 2.65 m up with speed lookahead (revised Unity values), no artificial shake.
public partial class DriveController : Node3D {
 public Race Race; public Vehicle Player; public Node3D Car; public Camera3D Cam; public bool AutoDrive; public float AutoSeconds = 8; public Hud Hud; float? lastGroundY, prevY; float groundSmooth; bool groundInit; float minY = 1e9f, maxY = -1e9f, maxDy; float steerCmd; public Vector3 CarSize = new Vector3(2, 1.2f, 4.7f); public int TrackIndex; public Node3D[] WheelPivots; float spinAngle; public int Laps = 3; public bool AiTest; string lastSt = ""; float finT;
 int CamMode = int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_CAM") ?? "0"); bool cWas; bool tabWas; ulong pausedAt; float t, camYaw; int shownMode = -1; bool camInit;
 public override void _Ready() {
  Race = new Race(TrackIndex, Laps, 0f); Player = Race.Add("PLAYER", true, Compound.Medium); Race.countdown = 0; Race.started = true; Player.Place(Race.track, 0.988f, 0f); CallDeferred(nameof(BindHud));
 }
 void BindHud() { if (Hud != null) { Hud.Race = Race; Hud.Player = Player; } }
 void ResolveWalls(Vec start) { if (Contact.Walls(GetWorld3D().DirectSpaceState, Player, start, Car.Position.Y, new Vector3(1.96f, 1.0f, 4.06f))) WallHits++; }
 public int WallHits; int lastTrace = -1; bool floatScanned;
 Vector3 ObstructCam(Vector3 from, Vector3 want) { var q = PhysicsRayQueryParameters3D.Create(from, want); q.CollisionMask = 2; q.HitBackFaces = true; var hit = GetWorld3D().DirectSpaceState.IntersectRay(q); if (hit.Count == 0) return want; var p = (Vector3)hit["position"]; var n = (Vector3)hit["normal"]; var dir = (want - from).Normalized(); float dist = Mathf.Max(0.8f, from.DistanceTo(p) - 0.35f); return from + dir * dist; }
float lastLift; Node3D crew; Node3D[] crewFig; Vector3[] crewHome;
 void BuildCrew() {
  crew = new Node3D { Visible = false }; AddChild(crew); var inv = Car.GlobalTransform.Basis.Orthonormalized().Transposed(); crewFig = new Node3D[6]; crewHome = new Vector3[6];
  var suit = new StandardMaterial3D { AlbedoColor = new Color(0.78f, 0.12f, 0.1f), Roughness = 0.9f }; var skin = new StandardMaterial3D { AlbedoColor = new Color(0.8f, 0.62f, 0.5f), Roughness = 0.9f }; var helm = new StandardMaterial3D { AlbedoColor = new Color(0.95f, 0.95f, 0.95f), Roughness = 0.4f };
  for (int i = 0; i < 6; i++) {
   var fig = new Node3D(); var body = new MeshInstance3D { Mesh = new CapsuleMesh { Radius = 0.17f, Height = 1.0f }, MaterialOverride = suit, Position = new Vector3(0, 0.5f, 0) }; var head = new MeshInstance3D { Mesh = new SphereMesh { Radius = 0.11f, Height = 0.22f }, MaterialOverride = helm, Position = new Vector3(0, 1.12f, 0) };
   fig.AddChild(body); fig.AddChild(head); crew.AddChild(fig); crewFig[i] = fig;
   Vector3 home;
   if (i < 4 && WheelPivots != null) { var wp = inv * (WheelPivots[i].GlobalPosition - Car.GlobalPosition); float side = wp.X < 0 ? -1f : 1f; home = new Vector3(wp.X + side * 0.55f, 0, wp.Z); }
   else if (i == 4) home = new Vector3(1.9f, 0, CarSize.Z * 0.5f + 0.7f); else home = new Vector3(0, 0, CarSize.Z * 0.5f + 0.7f);
   crewHome[i] = home; fig.Position = home;
  }
 }
 void UpdateCrew(float e) {
  if (WheelPivots == null || Car == null) return; if (crew == null) BuildCrew(); crew.Visible = e >= 0 && e < 7.3f; if (!crew.Visible) return; crew.GlobalTransform = new Transform3D(Car.GlobalTransform.Basis.Orthonormalized(), new Vector3(Car.GlobalPosition.X, Car.GlobalPosition.Y - lastLift, Car.GlobalPosition.Z));
  for (int i = 0; i < 6; i++) {
   float lean = 0, off = 1.0f + i * 0.6f, on = 3.4f + i * 0.6f;
   if (i < 4) { if (e > off - 0.5f && e < on + 0.7f) lean = 0.45f; float side = WheelPivots[i].Position.X < 0 ? -1f : 1f; crewFig[i].Rotation = new Vector3(0, 0, side * lean); crewFig[i].Position = crewHome[i] + new Vector3(-side * lean * 0.25f, 0, 0); }
   else { lean = (i == 4 ? (e < 0.8f || e > 6.4f ? 0.5f : 0f) : (e < 7.3f ? 0.0f : 0f)); crewFig[i].Rotation = new Vector3(lean, 0, 0); }
  }
 }
 float PitSequence(float e) {
  if (WheelPivots == null) return 0; if (e < 0) { foreach (var wp in WheelPivots) wp.Visible = true; return 0; }
  float lift = e < 0.8f ? 0.14f * (e / 0.8f) : e < 6.4f ? 0.14f : 0.14f * (1f - Mathf.Clamp((e - 6.4f) / 0.8f, 0, 1));
  // wheels come off one by one 1.0-3.4 s (front-left, front-right, rear-left, rear-right) and go back on 3.4-6.0 s in the same order
  for (int i = 0; i < 4; i++) { float off = 1.0f + i * 0.6f, on = 3.4f + i * 0.6f; WheelPivots[i].Visible = !(e >= off && e < on); }
  return lift;
 }
 static float DeadZone(float v, float dz) { float a = Mathf.Abs(v); if (a <= dz) return 0; return Mathf.Sign(v) * (a - dz) / (1f - dz); }
 static Vector3 ToWorld(Vec p, float y) => new Vector3(p.x, y, p.z);
 public override void _PhysicsProcess(double delta) {
  bool tabNow = Input.IsKeyPressed(Key.Tab) || Input.IsKeyPressed(Key.Escape); if (tabNow && !tabWas) Race.paused = !Race.paused; tabWas = tabNow; if (Hud != null) Hud.Paused = Race.paused; if (System.Environment.GetEnvironmentVariable("MYSIM_PAUSETEST") != null) { if (!Race.paused && t > 2f && pausedAt == 0) { Race.paused = true; pausedAt = Time.GetTicksMsec(); } if (Race.paused && Time.GetTicksMsec() - pausedAt > 1500) { GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "pause.png"); GetTree().Quit(); } } if (Race.paused) return; bool escWas = false;
  t += (float)delta;
  float thr = Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1 : 0, brk = Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1 : 0;
  float steer = (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1 : 0) - (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1 : 0);
  var pads = Input.GetConnectedJoypads(); if (pads.Count > 0) { int pid = pads[0]; float ax = DeadZone(Input.GetJoyAxis(pid, JoyAxis.LeftX), 0.08f), rt = Input.GetJoyAxis(pid, JoyAxis.TriggerRight), lt = Input.GetJoyAxis(pid, JoyAxis.TriggerLeft); if (Mathf.Abs(ax) > 0) steer = ax; thr = Mathf.Max(thr, DeadZone(rt, 0.04f)); brk = Mathf.Max(brk, DeadZone(lt, 0.04f)); if (Input.IsJoyButtonPressed(pid, JoyButton.A)) Player.pitRequested = true; }
  if (AutoDrive && !AiTest) { thr = 1; brk = 0; steer = float.Parse(System.Environment.GetEnvironmentVariable("MYSIM_STEERBIAS") ?? "0", System.Globalization.CultureInfo.InvariantCulture); }
  if (AiTest) { Player.human = false; Player.pitRequested = System.Environment.GetEnvironmentVariable("MYSIM_NOPIT") == null; var st = (Player.pitActive ? "A" : "-") + (Player.servicing ? "S" : "-") + (Player.pitDone ? "D" : "-"); if (st != lastSt) { GD.Print("PITSTATE t=", t.ToString("0.0"), " ", st, " progress=", Player.progress.ToString("0.000"), " speed=", Player.speed.ToString("0.0"), " lap=", Player.completedLaps, " wear=", Player.Wear.ToString("0.00")); lastSt = st; } }
  if (TouchControls.Active) { if (TouchControls.Throttle > 0) thr = 1; if (TouchControls.Brake > 0) brk = 1; if (TouchControls.Steer != 0) steer = TouchControls.Steer; }
  if (Input.IsKeyPressed(Key.P)) Player.pitRequested = true;
  // Analog-style steering: keyboard digital input ramps up/down and the available lock shrinks with speed (no instant full-lock snap-spins).
  bool padSteer = Input.GetConnectedJoypads().Count > 0 && Mathf.Abs(Input.GetJoyAxis(Input.GetConnectedJoypads()[0], JoyAxis.LeftX)) > 0.08f;
  if (!padSteer && !AutoDrive) { float rate = Mathf.Abs(steer) > 0.01f ? 2.6f : 5.0f; if (Mathf.Sign(steer) != Mathf.Sign(steerCmd) && Mathf.Abs(steer) > 0.01f) rate = 5.0f; steerCmd = Mathf.MoveToward(steerCmd, steer, rate * (float)delta); float lim = Mathf.Clamp(1.0f - Player.speed / 120f, 0.35f, 1f); steer = steerCmd * lim; } else steerCmd = steer;
  Player.input = new DriverInput(thr, brk, steer) { handbrake = Input.IsKeyPressed(Key.Space) };
  var startPos = Player.position; Race.Advance((float)delta); ResolveWalls(startPos);
  var space = GetWorld3D().DirectSpaceState; var w = ToWorld(Player.position, 0);
  // Ground = drive-surface layer only (mask 1), continuous with the previous height. The old ray started at y=60 with the default (all-layer) mask, so it landed on barrier tops, bridges and stacked decks (car "flew").
  float refY = lastGroundY.HasValue ? lastGroundY.Value : Car.Position.Y;
  var gr = Contact.Ground(space, w, refY, 0.6f); Contact.MarkGood(Player, gr.near); if (Contact.Contain(Player, startPos, gr.near, 30f)) { WallHits++; w = ToWorld(Player.position, 0); gr = Contact.Ground(space, w, refY, 0.6f); }
  var hit = gr.found ? new Godot.Collections.Dictionary() { { "p", 1 } } : new Godot.Collections.Dictionary();
  float y = gr.y; lastGroundY = y; if (y < minY) minY = y; if (y > maxY) maxY = y; if (prevY.HasValue) maxDy = Mathf.Max(maxDy, Mathf.Abs(y - prevY.Value)); prevY = y; Vector3 n = gr.normal;
  float h = Player.heading; var fwd = new Vector3(Mathf.Sin(h), 0, Mathf.Cos(h));
  // Four-wheel contact: sample the drive surface under each wheel, put the chassis on the average height and pitch/roll it from the wheel heights (no floating, no wheels in the ground).
  Vector3 carPos = new Vector3(w.X, y + 0.05f, w.Z);
  if (WheelPivots != null && WheelPivots.Length == 4) {
   var rotOnly = Car.Basis; var cp = new Vector3[4]; float avg = 0; int ok = 0;
   for (int wi = 0; wi < 4; wi++) { var off = rotOnly * WheelPivots[wi].Position; var wp = new Vector3(w.X + off.X, 0, w.Z + off.Z); var gi = Contact.Ground(space, wp, refY, 0.6f); cp[wi] = new Vector3(wp.X, gi.found ? gi.y : y, wp.Z); avg += cp[wi].Y; if (gi.found) ok++; }
   avg /= 4f; var front = (cp[0] + cp[1]) * 0.5f - (cp[2] + cp[3]) * 0.5f; var side = (cp[1] + cp[3]) * 0.5f - (cp[0] + cp[2]) * 0.5f;
   float tgtY = ok > 0 ? avg : y; groundSmooth = groundInit ? Mathf.Lerp(groundSmooth, tgtY, 1f - Mathf.Exp(-40f * (float)delta)) : tgtY; groundInit = true;
   carPos = new Vector3(w.X, groundSmooth, w.Z);
   var nn = front.Cross(side).Normalized(); if (nn.Y < 0) nn = -nn; if (ok == 4 && nn.Y > 0.7f) n = nn;
  }
  // Pit stop sequence (service time 7.5 s): jack up, remove and refit each wheel in turn, lower, release. Driven by the real service timer.
  float lift = PitSequence(Player.servicing ? 7.5f - Player.serviceRemaining : -1f); carPos += new Vector3(0, lift, 0); lastLift = lift; UpdateCrew(Player.servicing ? 7.5f - Player.serviceRemaining : -1f);
  Car.Position = carPos;
  var f2 = (fwd - n * fwd.Dot(n)).Normalized(); var carX = n.Cross(f2).Normalized(); Car.Basis = new Basis(carX, n, f2); /* proper rotation (det +1): nose (+Z local) = travel direction. The old basis was left-handed and Orthonormalized() left it mirrored, so the nose faced the camera. */
  if (WheelPivots != null) { spinAngle += Player.speed * (float)delta / 0.34f; for (int wi = 0; wi < 4; wi++) { var sp = WheelPivots[wi].GetNode<Node3D>("Spin"); float sgn = WheelPivots[wi].Position.X < 0 ? -1f : 1f; sp.Rotation = new Vector3(spinAngle * sgn, 0, 0); if (wi < 2) { var rot = WheelPivots[wi].Position.X < 0 ? Mathf.Pi : 0f; WheelPivots[wi].Rotation = new Vector3(0, rot + Player.steering, 0); } } }
  // chase camera
  bool cNow = Input.IsKeyPressed(Key.C) || (Input.GetConnectedJoypads().Count > 0 && Input.IsJoyButtonPressed(Input.GetConnectedJoypads()[0], JoyButton.Y)); if (cNow && !cWas) CamMode = (CamMode + 1) % 4; cWas = cNow; if (CamMode != shownMode) { shownMode = CamMode; foreach (var mn in Car.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)mn; if (!mi.HasMeta("grp")) continue; string gp = mi.GetMeta("grp").AsString(); mi.Visible = gp == "int" ? CamMode == 2 : CamMode != 2; } }
  if (System.Environment.GetEnvironmentVariable("MYSIM_CLOSE") != null) { var sideDir = new Vector3(fwd.Z, 0, -fwd.X); Cam.Position = Car.Position + sideDir * 4.2f + Vector3.Up * 0.45f - fwd * 0.8f; Cam.LookAt(Car.Position + Vector3.Up * 0.35f); Cam.Fov = 45f; if (AutoDrive && t >= AutoSeconds) { GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "close.png"); GetTree().Quit(); } return; }
  float speed = Player.speed; float look = Mathf.Clamp(speed / 20f, 0, 1) * 4f + 3f;
  if (CamMode == 0 || CamMode == 1) {
   float len = Mathf.Max(CarSize.Z, 3.5f), hgt = Mathf.Max(CarSize.Y, 1.0f); float back = CamMode == 0 ? len * 0.95f : len * 1.8f, up = CamMode == 0 ? hgt * 1.15f + 0.2f : hgt * 2.2f; var target = ObstructCam(Car.Position + Vector3.Up * (hgt * 0.8f), Car.Position - fwd * back + Vector3.Up * up);
   if (!camInit) { Cam.Position = target; camInit = true; } Cam.Position = Cam.Position.Lerp(target, 1f - Mathf.Exp(-6f * (float)delta)); Cam.LookAt(Car.Position + fwd * look + Vector3.Up * 1.0f); Cam.Fov = Mathf.Lerp(Cam.Fov, 46f + Mathf.Clamp(speed / 80f, 0, 1) * 6f, 1f - Mathf.Exp(-3f * (float)delta));
  } else if (CamMode == 2) { Cam.Position = Car.GlobalTransform * new Vector3(-0.35f, 0.88f, 0.05f); Cam.GlobalTransform = new Transform3D(Car.GlobalTransform.Basis.Rotated(Car.GlobalTransform.Basis.Y.Normalized(), Mathf.Pi), Cam.Position); Cam.Fov = 75f; camInit = false; }
  else { Cam.Position = Car.GlobalTransform * new Vector3(0, 0.62f, 1.9f); Cam.GlobalTransform = new Transform3D(Car.GlobalTransform.Basis.Rotated(Car.GlobalTransform.Basis.Y.Normalized(), Mathf.Pi), Cam.Position); Cam.Fov = 70f; camInit = false; }
  if (AiTest && Player.finished) { finT += (float)delta; if (finT > 1.5f) { GD.Print("FINISHED t=", t.ToString("0.0"), " laps=", Player.completedLaps, " time=", Player.finishTime.ToString("0.00"), " best=", Player.bestLap.ToString("0.00"), " wallHits=", WallHits, " damage=", Player.damage.ToString("0.00")); GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "finish.png"); GetTree().Quit(); } }
  if (!floatScanned && t > 0.6f && System.Environment.GetEnvironmentVariable("MYSIM_FINDFLOAT") != null) { floatScanned = true; var sp = GetWorld3D().DirectSpaceState; int rep = 0; foreach (var fn in GetTree().Root.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)fn; string nn = mi.Name.ToString().ToLowerInvariant(); if (!(nn.Contains("tree") || nn.Contains("bush") || nn.Contains("org_"))) continue; var ab = mi.GlobalTransform * mi.GetAabb(); var c = ab.GetCenter(); var rq = PhysicsRayQueryParameters3D.Create(new Vector3(c.X, c.Y + 300f, c.Z), new Vector3(c.X, c.Y - 300f, c.Z), 1); var rr = sp.IntersectRay(rq); if (rr.Count == 0) continue; float gy = ((Vector3)rr["position"]).Y; float gap = ab.Position.Y - gy; if (gap > 2.5f && rep++ < 12) GD.Print("FLOAT ", mi.Name, " gap=", gap.ToString("0.0"), " center=", c, " size=", ab.Size); } }
  if (AiTest && System.Environment.GetEnvironmentVariable("MYSIM_TRACE") != null && (int)(t / 4f) != lastTrace) { lastTrace = (int)(t / 4f); GD.Print("TR t=", t.ToString("0"), " p=", Player.progress.ToString("0.000"), " v=", Player.speed.ToString("0.0"), " lat=", Player.lateral.ToString("0.0"), " hits=", WallHits, " dmg=", Player.damage.ToString("0.00")); }
  if (AutoDrive && !(AiTest && AutoSeconds > 250) && t >= AutoSeconds) { GD.Print("YRANGE min=", minY.ToString("0.00"), " max=", maxY.ToString("0.00"), " maxStepDy=", maxDy.ToString("0.00")); GD.Print("DRIVETEST t=", t, " speed=", Player.speed, " progress=", Player.progress, " lateral=", Player.lateral, " lap=", Player.completedLaps, " wallHits=", WallHits, " onSurface=", hit.Count > 0, " pos=", Car.Position); GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "drive.png"); GetTree().Quit(); }
 }
}}






























