using Godot;
using System.Collections.Generic;
namespace MySim {
// Loads a recovered car FBX set, keeps LOD_A only, binds PNG textures by material suffix (mm_ext uses the livery).
public static class CarLoader {
 public static Node3D Load(string carId) {
  string dir = "res://assets/RealRacing/Source/" + carId + "/";
  var root = new Node3D { Name = carId };
  foreach (var part in DirAccess.GetFilesAt(dir)) {
   if (!part.EndsWith(".fbx")) continue;
   var inst = GD.Load<PackedScene>(dir + part).Instantiate<Node3D>(); inst.Scale = Vector3.One * 100f; root.AddChild(inst);
  }
  var cache = new Dictionary<string, StandardMaterial3D>();
  foreach (var n in root.FindChildren("*", "MeshInstance3D", true, false)) {
   var m = (MeshInstance3D)n;
   if (!m.Name.ToString().StartsWith("LOD_A")) { m.Visible = false; continue; }
   for (int s = 0; s < m.Mesh.GetSurfaceCount(); s++) {
    string mn = m.Mesh.SurfaceGetMaterial(s)?.ResourceName ?? ""; int i = mn.LastIndexOf("_mm_"); if (i < 0) continue; string suf = mn.Substring(i + 4);
    if (!cache.TryGetValue(suf, out var mat)) { mat = Build(dir, carId, suf); cache[suf] = mat; }
    m.SetSurfaceOverrideMaterial(s, mat);
   }
  }
  return root;
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
