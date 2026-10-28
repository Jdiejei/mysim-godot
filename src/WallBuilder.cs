using Godot;
using System.Text.RegularExpressions;
namespace MySim {
// Solid barriers from the original render meshes (armco, walls, fences, tyre walls) on collision layer 2, driven by box sweep like the Unity ResolveWalls.
public static class WallBuilder {
 static readonly Regex Solid = new Regex("armco|barrier|wall|fence|tyre|tire|concrete|guardrail|bollard", RegexOptions.IgnoreCase);
 public static bool IsSolid(string materialName) => Solid.IsMatch(materialName);
 public static int Build(Node3D track) {
  int n = 0;
  foreach (var node in track.FindChildren("*", "MeshInstance3D", true, false)) {
   var m = (MeshInstance3D)node; bool solid = false;
   for (int s = 0; s < m.Mesh.GetSurfaceCount() && !solid; s++) if (Solid.IsMatch(m.Mesh.SurfaceGetMaterial(s)?.ResourceName ?? "")) solid = true;
   if (!solid) continue;
   m.CreateTrimeshCollision(); n++;
   foreach (var ch in m.GetChildren()) if (ch is StaticBody3D sb) { sb.CollisionLayer = 2; sb.CollisionMask = 0; foreach (var cs in sb.GetChildren()) if (cs is CollisionShape3D c3 && c3.Shape is ConcavePolygonShape3D cp) cp.BackfaceCollision = true; }
  }
  GD.Print("WALLS solid meshes=", n); return n;
 }
}}

