using Godot;
using System.Collections.Generic;
namespace MySim {
// Binds the original recovered textures to the track's FBX materials (name -> texture via recovered Unity material names).
public static class TrackMaterialBinder {
 public static void Apply(Node3D track, string dir) {
  var map = new Dictionary<string, string>();
  using (var f = FileAccess.Open(dir + "tex_by_material.json", FileAccess.ModeFlags.Read)) { var d = Json.ParseString(f.GetAsText()).AsGodotDictionary(); foreach (var k in d.Keys) map[k.AsString()] = d[k].AsString(); }
  var cache = new Dictionary<string, StandardMaterial3D>(); int bound = 0, total = 0;
  foreach (var n in track.FindChildren("*", "MeshInstance3D", true, false)) {
   var m = (MeshInstance3D)n;
   for (int s = 0; s < m.Mesh.GetSurfaceCount(); s++) {
    total++; string name = m.Mesh.SurfaceGetMaterial(s)?.ResourceName ?? ""; if (name == "") continue;
    if (!cache.TryGetValue(name, out var mat)) {
     string tex = null; if (map.TryGetValue(name, out var t0)) tex = t0; else foreach (var kv in map) if (kv.Key.StartsWith(name + "_") || kv.Key.StartsWith(name)) { tex = kv.Value; break; }
     if (tex != null && ResourceLoader.Exists(dir + "textures/" + tex)) {
      mat = new StandardMaterial3D { AlbedoTexture = GD.Load<Texture2D>(dir + "textures/" + tex), Roughness = 0.9f, TextureFilter = BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic };
      string ln = name.ToLowerInvariant(); if (ln.Contains("overlay") || ln.Contains("alpha") || ln.Contains("fence") || ln.Contains("crowd")) mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
     }
     cache[name] = mat;
    }
    if (mat != null) { m.SetSurfaceOverrideMaterial(s, mat); bound++; }
   }
  }
  GD.Print("TRACKBIND bound=", bound, " of ", total, " surfaces; distinct textured materials=", cache.Count);
 }
}}
