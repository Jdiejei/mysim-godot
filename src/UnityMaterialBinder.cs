using Godot;
using System.Collections.Generic;
namespace MySim {
// Exact Unity material assignment: prefab renderer (by node name) -> .mat guid -> texture/colour/alpha settings, parsed offline into assets/unity_map.json.
public static class UnityMaterialBinder {
 static Godot.Collections.Dictionary map;
 static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();
 static readonly Shader GroundShader = new Shader { Code = "shader_type spatial; render_mode cull_disabled; uniform sampler2D tex : source_color, repeat_enable, filter_linear_mipmap_anisotropic; uniform vec4 tint : source_color = vec4(1.0); void fragment() { ALBEDO = texture(tex, UV).rgb * tint.rgb; ROUGHNESS = 1.0; SPECULAR = 0.04; NORMAL = normalize((VIEW_MATRIX * vec4(0.0, 1.0, 0.0, 0.0)).xyz); }" };
 static string Res(string p) { if (p == null) return null; int i = p.IndexOf("Assets/RealRacing/"); return i < 0 ? null : "res://assets/" + p.Substring(i + "Assets/".Length); }
 static string Strip(string n) { int i = n.LastIndexOf('_'); return n; }
 public static int Apply(Node3D layer, int variant, out int total) {
  total = 0; int bound = 0;
  if (map == null) { if (!FileAccess.FileExists("res://assets/unity_map.json")) { GD.Print("UMAP file missing, using name matching"); return 0; } using var f = FileAccess.Open("res://assets/unity_map.json", FileAccess.ModeFlags.Read); map = Json.ParseString(f.GetAsText()).AsGodotDictionary(); }
  var tracks = map["tracks"].AsGodotDictionary(); string vk = variant.ToString(); if (!tracks.ContainsKey(vk)) { GD.Print("UMAP no variant ", vk); return 0; }
  var byName = tracks[vk].AsGodotDictionary(); var mats = map["materials"].AsGodotDictionary(); var used = new Dictionary<string, int>();
  foreach (var node in layer.FindChildren("*", "MeshInstance3D", true, false)) {
   var mi = (MeshInstance3D)node; string nm = mi.Name.ToString(); Godot.Collections.Array lists = null;
   if (byName.ContainsKey(nm)) lists = byName[nm].AsGodotArray(); else { int k = nm.LastIndexOf('_'); string alt = nm; while (alt.Length > 0 && char.IsDigit(alt[alt.Length - 1])) alt = alt.Substring(0, alt.Length - 1); if (byName.ContainsKey(alt)) lists = byName[alt].AsGodotArray(); }
   if (lists == null || lists.Count == 0) { total += mi.Mesh.GetSurfaceCount(); continue; }
   used.TryGetValue(nm, out int ui); used[nm] = ui + 1; var guids = lists[ui % lists.Count].AsGodotArray();
   bool ground = nm.ToLowerInvariant().StartsWith("gnd_") || nm.ToLowerInvariant().StartsWith("road");
   for (int s = 0; s < mi.Mesh.GetSurfaceCount(); s++) {
    total++; if (guids.Count == 0) continue; string g = guids[s < guids.Count ? s : guids.Count - 1].AsString(); string key = g + (ground ? "|g" : "");
    if (!cache.TryGetValue(key, out var mat)) {
     mat = null; if (mats.ContainsKey(g)) { var mi2 = mats[g].AsGodotDictionary(); string tp = mi2.ContainsKey("tex") ? Res(mi2["tex"].AsString()) : null; Texture2D tex = tp != null && ResourceLoader.Exists(tp) ? GD.Load<Texture2D>(tp) : null;
      Color col = Colors.White; if (mi2.ContainsKey("color")) { var c = mi2["color"].AsGodotArray(); col = new Color((float)c[0], (float)c[1], (float)c[2], (float)c[3]); }
      if (col.R + col.G + col.B < 0.06f) col = new Color(1, 1, 1, col.A < 0.01f ? 1f : col.A); bool clip = mi2.ContainsKey("alphaClip") && mi2["alphaClip"].AsBool(); bool trans = mi2.ContainsKey("transparent") && mi2["transparent"].AsBool(); float cut = mi2.ContainsKey("cutoff") ? (float)mi2["cutoff"].AsDouble() : 0.5f;
      if (tex != null || col != Colors.White) {
       if (ground && tex != null) { var sm = new ShaderMaterial { Shader = GroundShader }; sm.SetShaderParameter("tex", tex); sm.SetShaderParameter("tint", new Color(col.R, col.G, col.B)); mat = sm; }
       else { var st = new StandardMaterial3D { AlbedoTexture = tex, AlbedoColor = col, Roughness = 1f, MetallicSpecular = 0.12f, TextureFilter = BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic }; if (clip) { st.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor; st.AlphaScissorThreshold = cut > 0 ? cut : 0.5f; st.CullMode = BaseMaterial3D.CullModeEnum.Disabled; } else if (trans) st.Transparency = BaseMaterial3D.TransparencyEnum.Alpha; mat = st; }
      } }
     cache[key] = mat;
    }
    if (mat != null) { mi.SetSurfaceOverrideMaterial(s, mat); bound++; }
   }
  }
  GD.Print("UMAP variant ", variant, " bound=", bound, " of ", total, " surfaces");
  return bound;
 }
}}


