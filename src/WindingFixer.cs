using Godot;
namespace MySim {
// Godot front faces are clockwise: cross(b-a,c-a) points OPPOSITE the stored outward normal.
// Reverse only surfaces whose averaged cross/normal dot is positive. Preserve correct imported winding.
public static class WindingFixer {
 public static int Fix(Node3D root, out int surfacesChecked) {
  int flipped = 0; surfacesChecked = 0;
  foreach (var node in root.FindChildren("*", "MeshInstance3D", true, false)) {
   var mi = (MeshInstance3D)node; var src = mi.Mesh; if (src == null) continue; ArrayMesh res = null;
   for (int s = 0; s < src.GetSurfaceCount(); s++) {
    var arr = src.SurfaceGetArrays(s); var vtx = (Vector3[])arr[(int)Mesh.ArrayType.Vertex]; var nrm = (Vector3[])arr[(int)Mesh.ArrayType.Normal]; var idx = (int[])arr[(int)Mesh.ArrayType.Index]; surfacesChecked++;
    bool flip = false;
    if (vtx != null && nrm != null && idx != null && idx.Length >= 3) { double agree = 0; int step = System.Math.Max(3, (idx.Length / 3 / 200) * 3); for (int i = 0; i + 2 < idx.Length; i += step) { var a = vtx[idx[i]]; var b = vtx[idx[i + 1]]; var c = vtx[idx[i + 2]]; var g = (b - a).Cross(c - a); if (g.LengthSquared() < 1e-12f) continue; g = g.Normalized(); agree += g.Dot(nrm[idx[i]] + nrm[idx[i + 1]] + nrm[idx[i + 2]]); } flip = agree > 0; }
    if (flip) { for (int i = 0; i + 2 < idx.Length; i += 3) { int t = idx[i + 1]; idx[i + 1] = idx[i + 2]; idx[i + 2] = t; } arr[(int)Mesh.ArrayType.Index] = idx; }
    if (flip && res == null) { res = new ArrayMesh(); for (int k = 0; k < s; k++) { res.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, src.SurfaceGetArrays(k)); res.SurfaceSetMaterial(k, src.SurfaceGetMaterial(k)); } }
    if (res != null) { res.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr); res.SurfaceSetMaterial(res.GetSurfaceCount() - 1, src.SurfaceGetMaterial(s)); }
    if (flip) flipped++;
   }
   if (res != null) mi.Mesh = res;
  }
  GD.Print("WINDING surfaces=", surfacesChecked, " flipped=", flipped); return flipped;
 }
}}
