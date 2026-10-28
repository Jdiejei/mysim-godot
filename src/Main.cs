using Godot;
namespace MySim {
// Bootstrap scene: configured sun shadows (4 cascades, 150 m, tuned bias), chase camera target, optional car.
public partial class Main : Node3D {
 public override void _Ready() {
  var env = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Sky, AmbientLightSource = Godot.Environment.AmbientSource.Sky, TonemapMode = Godot.Environment.ToneMapper.Filmic };
  var sky = new Sky { SkyMaterial = new ProceduralSkyMaterial() }; env.Sky = sky;
  AddChild(new WorldEnvironment { Environment = env });
  var sun = new DirectionalLight3D { RotationDegrees = new Vector3(-48, -35, 0), LightEnergy = 1.3f, ShadowEnabled = true,
   DirectionalShadowMode = DirectionalLight3D.ShadowMode.Parallel4Splits, DirectionalShadowMaxDistance = 150, DirectionalShadowBlendSplits = true,
   ShadowBias = 0.03f, ShadowNormalBias = 1.0f, ShadowBlur = 1.0f };
  AddChild(sun);
  var ground = new MeshInstance3D { Mesh = new PlaneMesh { Size = new Vector2(400, 400) }, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off, MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.25f, 0.27f, 0.27f) } };
  var trackScene = GD.Load<PackedScene>("res://assets/RealRacing/Source/Brands_Hatch/processed/high/hatch.fbx"); var track = trackScene.Instantiate<Node3D>(); track.Scale = new Vector3(100f, 100f, -100f); AddChild(track); Aabb tb = new Aabb(); int tm = 0; foreach (var n in track.FindChildren("*", "MeshInstance3D", true, false)) { var m = (MeshInstance3D)n; tb = tb.Merge(m.GlobalTransform * m.GetAabb()); tm++; } var names = new System.Collections.Generic.SortedSet<string>(); foreach (var n in track.FindChildren("*", "MeshInstance3D", true, false)) { var m = (MeshInstance3D)n; for (int s = 0; s < m.Mesh.GetSurfaceCount(); s++) names.Add(m.Mesh.SurfaceGetMaterial(s)?.ResourceName ?? "null"); } GD.Print("TRACKMAT count=", names.Count, " ", string.Join(",", names));
  GD.Print("TRACK meshes=", tm, " pos=", tb.Position, " size=", tb.Size);
  var car = CarLoader.Load("2015_mclaren_650s_gt3"); car.Visible = false; AddChild(car);
  AddChild(DriveSurfaceLoader.Load("res://assets/Resources/Brands_Hatch_processed.txt"));
  var cam0 = new Camera3D { Fov = 63, Current = true, Far = 4000 }; AddChild(cam0);
  var drive = new DriveController { Car = car, AutoDrive = System.Environment.GetEnvironmentVariable("MYSIM_AUTODRIVE") != null }; car.Visible = true; AddChild(drive); drive.Cam = cam0;
  
  
 }
 async void Spawn(Node3D car, Node3D track) {
  await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
  var space = GetWorld3D().DirectSpaceState; var pts = new[] { new Vector2(-247.0625f, 291.4375f), new Vector2(-256.875f, 278.6875f), new Vector2(-266.625f, 266.125f), new Vector2(-276.375f, 253.375f), new Vector2(-285.9375f, 240.5f) }; Vector2 a = pts[0], b = pts[1];
  foreach (float zs in new[] { -1f }) { for (int k = 0; k < 4; k++) { a = pts[k]; b = pts[k + 1]; var rr = space.IntersectRay(PhysicsRayQueryParameters3D.Create(new Vector3(a.X, 300, a.Y * zs), new Vector3(a.X, -300, a.Y * zs))); if (rr.Count > 0) break; } var r = space.IntersectRay(PhysicsRayQueryParameters3D.Create(new Vector3(a.X, 300, a.Y * zs), new Vector3(a.X, -300, a.Y * zs))); GD.Print("SPAWN zsign=", zs, " hit=", r.Count > 0 ? ((Vector3)r["position"]).ToString() : "none");
   if (r.Count == 0) continue; var p = (Vector3)r["position"]; car.Position = p + Vector3.Up * 0.3f; var d = new Vector3(b.X - a.X, 0, (b.Y - a.Y) * zs); car.LookAt(car.Position - d, Vector3.Up); car.Visible = true;
   var cam = GetViewport().GetCamera3D(); cam.Position = car.Position + d.Normalized() * -7.2f + Vector3.Up * 2.65f; cam.LookAt(car.Position + Vector3.Up * 1f); break; }
 }
 async void CaptureAfter(string path) { for (int i = 0; i < 40; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); GetViewport().GetTexture().GetImage().SavePng(path); GetTree().Quit(); }
}}

















