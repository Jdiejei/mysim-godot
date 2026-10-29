using Godot;
using System.Collections.Generic;
namespace MySim {
// Loads a recovered car FBX set, keeps LOD_A only, binds PNG textures by material suffix (mm_ext uses the livery).
public static class CarLoader {
 public static Node3D Load(string carId) {
  string dir = "res://assets/RealRacing/Source/" + carId + "/";
  var root = new Node3D { Name = carId };
  foreach (var part in Assets.Files(dir)) {
   if (!part.EndsWith(".fbx")) continue;
   var inst = GD.Load<PackedScene>(dir + part).Instantiate<Node3D>(); inst.Scale = Vector3.One * 100f; root.AddChild(inst);
  }
  var cache = new Dictionary<string, StandardMaterial3D>();
  foreach (var n in root.FindChildren("*", "MeshInstance3D", true, false)) {
   var m = (MeshInstance3D)n;
   string up = m.Name.ToString().ToUpperInvariant(); bool isInt = up.StartsWith("LOD_A_INT_") && !System.Text.RegularExpressions.Regex.IsMatch(up, "DAMAGE|BROKEN|CRACKED|SHATTERED"); if (!up.StartsWith("LOD_A") || System.Text.RegularExpressions.Regex.IsMatch(up, "DAMAGE|BROKEN|CRACKED|SHATTERED|BONNETCAM|_DEBRIS|SCRATCH|LOD_A_LIT_|WHEEL_BLUR") || (up.StartsWith("LOD_A_INT_") && !isInt)) { m.Visible = false; continue; } m.SetMeta("grp", isInt ? "int" : "ext"); if (isInt) m.Visible = false;
   for (int s = 0; s < m.Mesh.GetSurfaceCount(); s++) {
    string mn = m.Mesh.SurfaceGetMaterial(s)?.ResourceName ?? ""; int i = mn.LastIndexOf("_mm_"); if (i < 0) continue; string suf = mn.Substring(i + 4);
    if (!cache.TryGetValue(suf, out var mat)) { mat = Build(dir, carId, suf); cache[suf] = mat; }
    m.SetSurfaceOverrideMaterial(s, mat);
   }
  }
  return root;
 }
static Godot.Collections.Dictionary carMap;
 static readonly Shader CarShader = new Shader { Code = "shader_type spatial; render_mode cull_disabled; uniform sampler2D base : source_color, repeat_enable, filter_linear_mipmap_anisotropic; uniform sampler2D mask : hint_default_white; uniform vec3 ambient_tint = vec3(1.0); uniform float use_mask = 0.0; uniform float alpha_mode = 0.0; void fragment() { vec2 uv = vec2(1.0 - UV.x, 1.0 - UV.y); vec4 b = texture(base, uv); vec4 m = texture(mask, uv); float mk = mix(0.5, m.r, use_mask); ALBEDO = b.rgb * ambient_tint; ROUGHNESS = mix(0.65, 0.22, mk); SPECULAR = mix(0.35, 0.9, mk); METALLIC = 0.0; if (alpha_mode > 0.5) { ALPHA = b.a; ALPHA_SCISSOR_THRESHOLD = 0.5; } }" };
 static string Res(string p) { if (p == null) return null; int i = p.IndexOf("Assets/RealRacing/"); return i < 0 ? null : "res://assets/" + p.Substring(i + "Assets/".Length); }
 // Exact Unity assignment from Car{n}.prefab: renderer name -> material guids -> textures (base + mask), colours and alpha mode.
 public static int ApplyExact(Node3D root, int carIdx) {
  if (carMap == null) { if (!FileAccess.FileExists("res://assets/car_map.json")) return 0; carMap = Json.ParseString(FileAccess.GetFileAsString("res://assets/car_map.json")).AsGodotDictionary(); }
  var byName = carMap["cars"].AsGodotDictionary()[carIdx.ToString()].AsGodotDictionary(); var mats = carMap["materials"].AsGodotDictionary(); var cache = new Dictionary<string, Material>(); int bound = 0;
  foreach (var node in root.FindChildren("*", "MeshInstance3D", true, false)) {
   var mi = (MeshInstance3D)node; string nm = mi.Name.ToString(); if (!byName.ContainsKey(nm)) continue; var lists = byName[nm].AsGodotArray(); if (lists.Count == 0) continue; var guids = lists[0].AsGodotArray();
   for (int s = 0; s < mi.Mesh.GetSurfaceCount() && guids.Count > 0; s++) {
    string g = guids[s < guids.Count ? s : guids.Count - 1].AsString(); if (!cache.TryGetValue(g, out var mat)) {
     mat = null; if (mats.ContainsKey(g)) { var mi2 = mats[g].AsGodotDictionary(); var texs = mi2.ContainsKey("texs") ? mi2["texs"].AsGodotDictionary() : null; string bp = texs != null && texs.ContainsKey("_BaseMap") ? Res(texs["_BaseMap"].AsString()) : (texs != null && texs.ContainsKey("_MainTex") ? Res(texs["_MainTex"].AsString()) : null);
      if (bp == null) bp = FallbackTex(carIdx, nm); if (bp != null && ResourceLoader.Exists(bp)) { var sm = new ShaderMaterial { Shader = CarShader }; sm.SetShaderParameter("base", GD.Load<Texture2D>(bp)); string mp = texs.ContainsKey("_MaskMap") ? Res(texs["_MaskMap"].AsString()) : null; if (mp != null && ResourceLoader.Exists(mp)) sm.SetShaderParameter("mask", GD.Load<Texture2D>(mp));
       var fl = mi2.ContainsKey("floats") ? mi2["floats"].AsGodotDictionary() : null; if (fl != null && fl.ContainsKey("_UseMask")) sm.SetShaderParameter("use_mask", (float)fl["_UseMask"].AsDouble());
       var cols = mi2.ContainsKey("colors") ? mi2["colors"].AsGodotDictionary() : null; if (cols != null && cols.ContainsKey("_AmbientColour")) { var a = cols["_AmbientColour"].AsGodotArray(); sm.SetShaderParameter("ambient_tint", new Vector3((float)a[0], (float)a[1], (float)a[2]) * 1.15f); }
       string ln = nm.ToLowerInvariant(); long q = mi2.ContainsKey("queue") ? (long)mi2["queue"].AsInt64() : -1; if (ln.Contains("badge") || ln.Contains("scratch") || ln.Contains("light") || ln.Contains("glow") || q > 2450) sm.SetShaderParameter("alpha_mode", 1f); mat = sm; } }
     cache[g] = mat; }
    if (mat != null) { mi.SetSurfaceOverrideMaterial(s, mat); bound++; }
   }
  }
  GD.Print("CARMAP car=", carIdx, " bound=", bound); return bound;
 }
 public static Vector3 MeasureSize(Node3D car) { Aabb bb = new Aabb(); bool first = true; foreach (var n in car.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)n; if (!mi.Visible || !mi.HasMeta("grp") || mi.GetMeta("grp").AsString() != "ext") continue; var b = mi.GlobalTransform * mi.GetAabb(); if (first) { bb = b; first = false; } else bb = bb.Merge(b); } return bb.Size; }
 public static void ForceUnshaded(Node3D car) { foreach (var n in car.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)n; for (int s = 0; s < mi.Mesh.GetSurfaceCount(); s++) { var ov = mi.GetSurfaceOverrideMaterial(s) as ShaderMaterial; var tex = ov?.GetShaderParameter("base").As<Texture2D>(); if (tex != null) mi.SetSurfaceOverrideMaterial(s, new StandardMaterial3D { ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, AlbedoTexture = tex, CullMode = BaseMaterial3D.CullModeEnum.Disabled, Uv1Scale = new Vector3(1, -1, 1), Uv1Offset = new Vector3(0, 1, 0) }); } } }
 // Some Unity car materials have an empty _BaseMap (assigned at runtime in Unity): fall back to the car's own livery/slot texture by node suffix.
 static string FallbackTex(int carIdx, string nodeName) {
  string folder = MySimulator.Rebuild.CarCatalog.Folders[carIdx]; string dir = "res://assets/RealRacing/Source/" + folder + "/"; int k = nodeName.LastIndexOf("_mm_"); if (k < 0) return null; string suf = nodeName.Substring(k + 4);
  if (suf == "ext") { foreach (var f in Assets.Files(dir + "livery/")) if (f.EndsWith(".png")) return dir + "livery/" + f; return null; }
  string p = dir + folder + "_" + suf + ".png"; return ResourceLoader.Exists(p) ? p : null;
 }
 public static Node3D[] AttachWheels(Node3D car, int carIdx) {
  var pts = Json.ParseString(FileAccess.GetFileAsString("res://assets/car_pivots.json")).AsGodotDictionary()[carIdx.ToString()].AsGodotArray();
  var wheelMeshes = new System.Collections.Generic.List<MeshInstance3D>();
  foreach (var n in car.FindChildren("*", "MeshInstance3D", true, false)) { var mi = (MeshInstance3D)n; string up = mi.Name.ToString().ToUpperInvariant(); if (System.Text.RegularExpressions.Regex.IsMatch(up, "^LOD_A_(WHEEL|TYRE|TIRE|CALIPER|BRAKE_DISC|DISC|HUB|ROTOR)") && !up.Contains("BLUR") && !up.Contains("GUARD")) wheelMeshes.Add(mi); }
  if (System.Environment.GetEnvironmentVariable("MYSIM_WHEELDBG") != null) foreach (var wm in wheelMeshes) for (int si = 0; si < wm.Mesh.GetSurfaceCount(); si++) { var am = wm.GetActiveMaterial(si); string ds = am is StandardMaterial3D sm0 ? "std tex=" + (sm0.AlbedoTexture != null ? sm0.AlbedoTexture.ResourcePath : "none") + " col=" + sm0.AlbedoColor + " met=" + sm0.Metallic + " rough=" + sm0.Roughness + " unsh=" + sm0.ShadingMode : (am == null ? "null" : am.GetType().Name + (am is ShaderMaterial shm ? " shader" : "")); GD.Print("WHEELPART ", wm.Name, " s", si, " ", ds); }
  var pivots = new Node3D[4]; var inv = car.GlobalTransform.AffineInverse();
  for (int i = 0; i < 4; i++) {
   var p = pts[i].AsGodotArray(); var pivot = new Node3D { Name = "WheelPivot" + i, Position = new Vector3((float)p[0], (float)p[1], (float)p[2]) }; car.AddChild(pivot);
   var spin = new Node3D { Name = "Spin" }; pivot.AddChild(spin); if ((float)p[0] < 0) pivot.RotationDegrees = new Vector3(0, 180, 0);
   bool rearAxle = (float)p[2] < 0; bool anyRear = false; foreach (var w0 in wheelMeshes) if (w0.Name.ToString().ToUpperInvariant().Contains("_REAR")) anyRear = true;
   foreach (var w in wheelMeshes) { bool isRearMesh = w.Name.ToString().ToUpperInvariant().Contains("_REAR"); if (anyRear && isRearMesh != rearAxle) continue; var d = (MeshInstance3D)w.Duplicate(); spin.AddChild(d); d.Transform = inv * w.GlobalTransform; d.Visible = true; }
   pivots[i] = pivot;
  }
  foreach (var w in wheelMeshes) w.Visible = false;
  GD.Print("WHEELS car=", carIdx, " parts=", wheelMeshes.Count);
  return pivots;
 }
 static StandardMaterial3D Build(string dir, string car, string suf) {
  var mat = new StandardMaterial3D { Roughness = 0.45f, Metallic = 0.2f };
  string tex = dir + car + "_" + suf + ".png"; if (suf == "ext") tex = dir + "livery/" + car + "_ext_59.png";
  if (ResourceLoader.Exists(tex)) mat.AlbedoTexture = GD.Load<Texture2D>(tex); else mat.AlbedoColor = new Color(0.12f, 0.12f, 0.13f);
  if (suf == "lights" || suf == "badges" || suf == "scratches") mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
  if (suf == "lights") { mat.EmissionEnabled = false; }
  return mat;
 }
}}











