using Godot;
using System.Collections.Generic;
using System.Globalization;
namespace MySim {
// Recovered RR3 drive-surface collision mesh (OBJ text: v/f, 1-based). Becomes the only ground the car drives on.
public static class DriveSurfaceLoader {
 public static StaticBody3D Load(string resPath, uint layer = 1) {
  var verts = new List<Vector3>(); var tris = new List<Vector3>();
  using var f = FileAccess.Open(resPath, FileAccess.ModeFlags.Read);
  while (!f.EofReached()) {
   string l = f.GetLine(); if (l.Length < 2) continue;
   if (l[0] == 'v' && l[1] == ' ') { var p = l.Substring(2).Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries); verts.Add(new Vector3(P(p[0]), P(p[1]), -P(p[2]))); }
   else if (l[0] == 'f' && l[1] == ' ') { var p = l.Substring(2).Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries); tris.Add(verts[int.Parse(p[0]) - 1]); tris.Add(verts[int.Parse(p[1]) - 1]); tris.Add(verts[int.Parse(p[2]) - 1]); }
  }
  var shape = new ConcavePolygonShape3D { BackfaceCollision = true }; shape.Data = tris.ToArray();
  var body = new StaticBody3D { Name = "DriveSurface", CollisionLayer = layer }; body.AddChild(new CollisionShape3D { Shape = shape });
  GD.Print("DRIVESURFACE verts=", verts.Count, " tris=", tris.Count / 3);
  return body;
 }
 static float P(string s) => float.Parse(s, CultureInfo.InvariantCulture);
}}
