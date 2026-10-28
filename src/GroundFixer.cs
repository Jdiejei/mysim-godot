using Godot;
namespace MySim {
// Source ground meshes arrive with downward normals/reversed winding after the mirror; rebuild them facing up.
public static class GroundFixer {
 public static int Fix(Node3D track) {
  int fixedCount = 0;
  foreach (var node in track.FindChildren("*", "MeshInstance3D", true, false)) {
   var mi = (MeshInstance3D)node; string nm = mi.Name.ToString().ToLowerInvariant(); if (!(nm.StartsWith("gnd_") || nm.StartsWith("road") || nm.StartsWith("overlay"))) continue;
   var src = mi.Mesh; var res = new ArrayMesh(); bool any = false;
   for (int s = 0; s < src.GetSurfaceCount(); s++) {
    var arr = src.SurfaceGetArrays(s); var nrm = (Vector3[])arr[(int)Mesh.ArrayType.Normal]; var idx = (int[])arr[(int)Mesh.ArrayType.Index]; var vtx = (Vector3[])arr[(int)Mesh.ArrayType.Vertex];
    double sum = 0; if (nrm != null) foreach (var n in nrm) sum += n.Y;
    float scaleSign = -1f; // mirrored by the z flip; compare in world space using geometric normal of first triangles
    double geo = 0; if (idx != null && vtx != null) for (int i = 0; i + 2 < idx.Length; i += 3) { var a = vtx[idx[i]]; var b = vtx[idx[i + 1]]; var c = vtx[idx[i + 2]]; geo += ((b - a).Cross(c - a)).Y; }
    bool flip = (nrm != null && sum < 0) || (nrm == null && geo > 0);
    if (flip && nrm != null) { for (int i = 0; i < nrm.Length; i++) nrm[i] = -nrm[i]; arr[(int)Mesh.ArrayType.Normal] = nrm; if (idx != null) { for (int i = 0; i + 2 < idx.Length; i += 3) { int t = idx[i + 1]; idx[i + 1] = idx[i + 2]; idx[i + 2] = t; } arr[(int)Mesh.ArrayType.Index] = idx; } any = true; }
    res.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr); res.SurfaceSetMaterial(s, src.SurfaceGetMaterial(s));
    _ = scaleSign;
   }
   if (any) { mi.Mesh = res; fixedCount++; }
  }
  GD.Print("GROUNDFIX meshes flipped=", fixedCount); return fixedCount;
 }
}}

