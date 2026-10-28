using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Player drive loop: recovered Race simulation steps the car, the model is placed on the drive-surface mesh by raycast,
// chase camera 7.2 m back / 2.65 m up with speed lookahead (revised Unity values), no artificial shake.
public partial class DriveController : Node3D {
 public Race Race; public Vehicle Player; public Node3D Car; public Camera3D Cam; public bool AutoDrive; public float AutoSeconds = 8;
 float t, camYaw; bool camInit;
 public override void _Ready() {
  Race = new Race(0, 3, 0f); Player = Race.Add("PLAYER", true, Compound.Medium); Race.countdown = 0; Race.started = true; Player.Place(Race.track, 0.988f, 0f);
 }
 static Vector3 ToWorld(Vec p, float y) => new Vector3(p.x, y, -p.z);
 public override void _PhysicsProcess(double delta) {
  t += (float)delta;
  float thr = Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1 : 0, brk = Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1 : 0;
  float steer = (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1 : 0) - (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1 : 0);
  if (AutoDrive) { thr = 1; brk = 0; steer = 0; }
  Player.input = new DriverInput(thr, brk, steer) { handbrake = Input.IsKeyPressed(Key.Space) };
  Race.Step((float)delta);
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
  if (AutoDrive && t >= AutoSeconds) { GD.Print("DRIVETEST t=", t, " speed=", Player.speed, " progress=", Player.progress, " lateral=", Player.lateral, " lap=", Player.completedLaps, " onSurface=", hit.Count > 0, " pos=", Car.Position); GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "drive.png"); GetTree().Quit(); }
 }
}}

