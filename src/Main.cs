using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Game shell: menu (track/car/laps) -> race -> results -> restart/menu. Env MYSIM_TRACK skips the menu for automated tests.
public partial class Main : Node3D {
 Node3D world; CanvasLayer ui; DriveController drive; int lastTrack, lastCar, lastLaps = 3; bool resultsShown;
 public override void _Ready() {
  var env = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Sky, AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new Color(0.62f, 0.66f, 0.72f), AmbientLightEnergy = 1.0f, TonemapMode = Godot.Environment.ToneMapper.Aces, TonemapExposure = 1.0f, TonemapWhite = 6f, FogEnabled = true, FogLightColor = new Color(0.63f, 0.70f, 0.76f), FogDensity = 0.00025f, FogSkyAffect = 0.4f, AdjustmentEnabled = true, AdjustmentContrast = 1.12f, AdjustmentSaturation = 1.1f };
  env.Sky = new Sky { SkyMaterial = new ProceduralSkyMaterial() }; AddChild(new WorldEnvironment { Environment = env });
  AddChild(new DirectionalLight3D { RotationDegrees = new Vector3(-48, -35, 0), LightEnergy = 1.7f, ShadowEnabled = true,
   DirectionalShadowMode = DirectionalLight3D.ShadowMode.Parallel4Splits, DirectionalShadowMaxDistance = 150, DirectionalShadowBlendSplits = true, ShadowBias = 0.03f, ShadowNormalBias = 1.0f, ShadowBlur = 1.0f });
  if (System.Environment.GetEnvironmentVariable("MYSIM_AUDITCARS") != null) { for (int ci = 0; ci < CarCatalog.Count; ci++) { var cn = CarLoader.Load(CarCatalog.Folders[ci]); AddChild(cn); Aabb bb = new Aabb(); bool first = true; foreach (var n in cn.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)n; if (!mi.Visible) continue; var b2 = mi.GlobalTransform * mi.GetAabb(); if (first) { bb = b2; first = false; } else bb = bb.Merge(b2); } if (ci == int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_AUDITMESH") ?? "-1")) foreach (var n in cn.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)n; if (!mi.Visible) continue; var b3 = mi.GlobalTransform * mi.GetAabb(); GD.Print("AUDITMESH ", mi.Name, " c=", (b3.Position + b3.Size / 2).ToString("0.0"), " s=", b3.Size.ToString("0.0")); }
   GD.Print("CARAUDIT ", ci, " ", CarCatalog.Folders[ci], " L=", bb.Size.Z.ToString("0.00"), " W=", bb.Size.X.ToString("0.00"), " H=", bb.Size.Y.ToString("0.00"), " wheelbase=", RecoveredGeometry.Wheelbase[ci].ToString("0.00"), " mass=", RecoveredVehicleProfiles.Mass[ci].ToString("0")); cn.QueueFree(); } GetTree().Quit(); return; }
  var cv = System.Environment.GetEnvironmentVariable("MYSIM_CARVIEW"); if (cv != null) { CarView(int.Parse(cv)); return; }
  Settings.ApplyAudio(); lastTrack = Settings.Get("track", 0); lastCar = Settings.Get("car", 0); lastLaps = Settings.Get("laps", 3);
  var t = System.Environment.GetEnvironmentVariable("MYSIM_TRACK");
  if (t != null) StartRace(int.Parse(t), int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_CAR") ?? "8"), int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_LAPS") ?? "3")); else { ShowMenu(); var ms = System.Environment.GetEnvironmentVariable("MYSIM_MENUSHOT"); if (ms != null) MenuShot(ms); }
 }
 async void MenuShot(string path) { for (int i = 0; i < 20; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); GetViewport().GetTexture().GetImage().SavePng(path); GetTree().Quit(); }
 async void CarView(int ci) {
  var floor = new MeshInstance3D { Mesh = new PlaneMesh { Size = new Vector2(60, 60) }, MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.3f, 0.3f, 0.32f) } }; AddChild(floor);
  var car = CarLoader.Load(CarCatalog.Folders[ci]); AddChild(car); CarLoader.ApplyExact(car, ci); CarLoader.AttachWheels(car, ci); car.Position = new Vector3(0, 0.0f, 0);
  var cam = new Camera3D { Fov = 40, Current = true, Position = new Vector3(5.2f, 1.8f, 5.6f) }; AddChild(cam); cam.LookAt(new Vector3(0, 0.6f, 0));
  for (int i = 0; i < 25; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
  GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT") ?? "carview.png"); GetTree().Quit();
 }
 void ClearUi() { ui?.QueueFree(); ui = null; }
 void ShowMenu() {
  EndRace(); ClearUi(); ui = new CanvasLayer(); AddChild(ui); resultsShown = false;
  var bg = new ColorRect { Color = new Color(0.04f, 0.05f, 0.07f), AnchorRight = 1, AnchorBottom = 1 }; ui.AddChild(bg);
  var title = new Label { Text = "MYSIM", Position = new Vector2(48, 24) }; title.AddThemeFontSizeOverride("font_size", 56); ui.AddChild(title);
  var tl = Json.ParseString(FileAccess.GetFileAsString("res://assets/RealRacing/TrackList.json")).AsGodotDictionary()["tracks"].AsGodotArray();
  var tracks = new ItemList { Position = new Vector2(48, 110), Size = new Vector2(520, 520) }; foreach (var x in tl) tracks.AddItem(x.AsGodotDictionary()["name"].AsString()); tracks.Select(lastTrack); ui.AddChild(tracks);
  var cars = new ItemList { Position = new Vector2(600, 110), Size = new Vector2(420, 520) }; foreach (var n in CarCatalog.Names) cars.AddItem(n); cars.Select(lastCar); ui.AddChild(cars);
  var lapsLbl = new Label { Text = "LAPS", Position = new Vector2(1060, 110) }; ui.AddChild(lapsLbl);
  var laps = new SpinBox { MinValue = 1, MaxValue = 20, Value = lastLaps, Position = new Vector2(1060, 140) }; ui.AddChild(laps);
  var hint = new Label { Text = "Pick track and car, then START.  Drive: WASD/arrows, Space handbrake, P pit, Esc menu", Position = new Vector2(48, 650) }; ui.AddChild(hint);
  var start = new Button { Text = "START RACE", Position = new Vector2(1060, 200), Size = new Vector2(180, 56) };
  start.Pressed += () => { var ts = tracks.GetSelectedItems(); var cs = cars.GetSelectedItems(); StartRace(ts.Length > 0 ? ts[0] : 0, cs.Length > 0 ? cs[0] : 0, (int)laps.Value); };
  ui.AddChild(start); tracks.ItemActivated += i => start.EmitSignal(BaseButton.SignalName.Pressed); cars.ItemActivated += i => start.EmitSignal(BaseButton.SignalName.Pressed);
 }
 async void DebugRoad(Node3D track) {
  foreach (var n in track.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)n; mi.CreateTrimeshCollision(); foreach (var ch in mi.GetChildren()) if (ch is StaticBody3D sb) { sb.CollisionLayer = 8; sb.CollisionMask = 0; sb.SetMeta("mesh", mi.GetPath().ToString()); foreach (var cs in sb.GetChildren()) if (cs is CollisionShape3D c3 && c3.Shape is ConcavePolygonShape3D cp) cp.BackfaceCollision = true; } }
  await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
  var space = GetWorld3D().DirectSpaceState; foreach (var p in new[] { new Vector3(-266.6f, 0, -266.1f), new Vector3(-247.1f, 0, -291.4f), new Vector3(-30f, 0, -460f) }) { var q = PhysicsRayQueryParameters3D.Create(new Vector3(p.X, 100, p.Z), new Vector3(p.X, -100, p.Z)); q.CollisionMask = 8; q.HitBackFaces = true; var r = space.IntersectRay(q); if (r.Count == 0) { GD.Print("ROADRAY none at ", p); continue; } var body = (Node)r["collider"]; var mi = GetNode<MeshInstance3D>(body.GetMeta("mesh").AsString()); string mats = ""; for (int s = 0; s < mi.Mesh.GetSurfaceCount(); s++) mats += (mi.Mesh.SurfaceGetMaterial(s)?.ResourceName ?? "null") + "|"; GD.Print("ROADRAY ", p, " hit y=", ((Vector3)r["position"]).Y.ToString("0.00"), " mesh=", mi.Name, " mats=", mats, " active=", mi.GetActiveMaterial(0) is StandardMaterial3D sm ? (sm.AlbedoTexture != null ? sm.AlbedoTexture.ResourcePath : "color") : "none"); }
 }
 void EndRace() { world?.QueueFree(); world = null; drive = null; }
 void StartRace(int trackIdx, int carIdx, int laps) {
  EndRace(); ClearUi(); lastTrack = trackIdx; lastCar = carIdx; lastLaps = laps; resultsShown = false; Settings.Set("track", trackIdx); Settings.Set("car", carIdx); Settings.Set("laps", laps);
  world = new Node3D { Name = "World" }; AddChild(world);
  var tl = Json.ParseString(FileAccess.GetFileAsString("res://assets/RealRacing/TrackList.json")).AsGodotDictionary()["tracks"].AsGodotArray()[trackIdx].AsGodotDictionary();
  string mp = tl["map"].AsString(), lay = tl["layout"].AsString(), scn = tl["scene"].AsString();
  var track = GD.Load<PackedScene>("res://assets/RealRacing/Source/" + mp + "/" + scn).Instantiate<Node3D>(); track.Scale = new Vector3(100f, 100f, -100f); world.AddChild(track);
  var layers = new System.Collections.Generic.List<Node3D> { track }; foreach (var vp in tl["variationPaths"].AsGodotArray()) { string rp = "res://" + vp.AsString().Replace("Assets/RealRacing/Source/", "assets/RealRacing/Source/"); if (!ResourceLoader.Exists(rp)) { GD.Print("VARIATION missing ", rp); continue; } var vps = GD.Load<PackedScene>(rp); if (vps == null) { GD.Print("VARIATION import failed ", rp); continue; } var vn = vps.Instantiate<Node3D>(); vn.Scale = new Vector3(100f, 100f, -100f); world.AddChild(vn); layers.Add(vn); GD.Print("VARIATION loaded ", rp); }
  var car = CarLoader.Load(CarCatalog.Folders[carIdx]); world.AddChild(car); CarLoader.ApplyExact(car, carIdx); var wheelPivots = CarLoader.AttachWheels(car, carIdx);
  int baseVar = (int)tl["baseTrackIndex"]; foreach (var L in layers) { GroundFixer.Fix(L); WallBuilder.Build(L); UnityMaterialBinder.Apply(L, baseVar, out int utot); TrackMaterialBinder.Apply(L, "res://assets/" + mp + "/"); }
  SkyDome.Add(world, mp);
  string ds = "res://assets/Resources/" + mp + "_" + lay + ".txt"; world.AddChild(DriveSurfaceLoader.Load(FileAccess.FileExists(ds) ? ds : "res://assets/Resources/" + mp + "_processed.txt"));
  if (System.Environment.GetEnvironmentVariable("MYSIM_DEBUGROAD") != null) DebugRoad(track);
  var cam = new Camera3D { Fov = 63, Current = true, Far = 4000 }; world.AddChild(cam);
  drive = new DriveController { Car = car, WheelPivots = wheelPivots, TrackIndex = trackIdx, Laps = laps, Cam = cam, AutoDrive = System.Environment.GetEnvironmentVariable("MYSIM_AUTODRIVE") != null, AiTest = System.Environment.GetEnvironmentVariable("MYSIM_AI") != null, AutoSeconds = float.Parse(System.Environment.GetEnvironmentVariable("MYSIM_SECONDS") ?? "8") };
  var hud = new Hud(); drive.Hud = hud; world.AddChild(drive); world.AddChild(hud);
 }
 public override void _Process(double delta) {
  if (Input.IsKeyPressed(Key.Escape) && drive != null) ShowMenu();
  if (drive != null && !resultsShown && drive.Race != null && drive.Player != null && drive.Player.finished) ShowResults();
  if (resultsShown && Input.IsKeyPressed(Key.Enter)) StartRace(lastTrack, lastCar, lastLaps);
 }
 void ShowResults() {
  resultsShown = true; var layer = new CanvasLayer(); world.AddChild(layer);
  var p = drive.Player; var panel = new ColorRect { Color = new Color(0, 0, 0, 0.7f), AnchorRight = 1, AnchorBottom = 1 }; layer.AddChild(panel);
  var l = new Label { Position = new Vector2(420, 200) }; l.AddThemeFontSizeOverride("font_size", 36);
  l.Text = "RESULTS\n\nFINISHED  " + p.completedLaps + " laps\nTIME  " + p.finishTime.ToString("0.00") + " s\nBEST LAP  " + p.bestLap.ToString("0.00") + " s\nPENALTY  " + p.penalty.ToString("0.0") + " s\nDAMAGE  " + (int)(p.damage * 100) + "%\n\nENTER restart    ESC menu"; layer.AddChild(l);
 }
}}


















