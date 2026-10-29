using Godot;
using System.IO;
namespace MySim {
// Bakes barrier triangles (world space, after the track import scale/mirror) into a compact binary: int32 triCount, then triCount*9 float32.
public static class CollisionBake {
 public static int Write(Node3D track, string path) {
  var tris = new System.Collections.Generic.List<Vector3>();
  foreach (var node in track.FindChildren("*", "MeshInstance3D", true, false)) {
   var mi = (MeshInstance3D)node; if (!WallBuilder.IsSolid(mi)) continue; var xf = mi.GlobalTransform;
   for (int s = 0; s < mi.Mesh.GetSurfaceCount(); s++) { var arr = mi.Mesh.SurfaceGetArrays(s); var vtx = (Vector3[])arr[(int)Mesh.ArrayType.Vertex]; var idx = (int[])arr[(int)Mesh.ArrayType.Index]; if (vtx == null || idx == null) continue; for (int i = 0; i < idx.Length; i++) tris.Add(xf * vtx[idx[i]]); }
  }
  using (var fs = new System.IO.FileStream(ProjectSettings.GlobalizePath(path), FileMode.Create)) using (var bw = new BinaryWriter(fs)) { bw.Write(tris.Count / 3); foreach (var v in tris) { bw.Write(v.X); bw.Write(v.Y); bw.Write(-v.Z); } /* bins stay in raw (mirrored) coordinates */ }
  return tris.Count / 3;
 }
 public static StaticBody3D Read(string path) {
  using var fs = new System.IO.FileStream(path, FileMode.Open); using var br = new BinaryReader(fs); int n = br.ReadInt32(); var pts = new Vector3[n * 3]; for (int i = 0; i < pts.Length; i++) { float bx = br.ReadSingle(), by = br.ReadSingle(), bz = br.ReadSingle(); pts[i] = new Vector3(bx, by, -bz); }
  var shape = new ConcavePolygonShape3D { BackfaceCollision = true }; shape.Data = pts; var body = new StaticBody3D { Name = "BakedWalls", CollisionLayer = 2, CollisionMask = 0 }; body.AddChild(new CollisionShape3D { Shape = shape }); return body;
 }
}}
