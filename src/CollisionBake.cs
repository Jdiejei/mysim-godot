using Godot;
using System.IO;
namespace MySim {
// Bakes barrier triangles (world space, after the track import scale/mirror) into a compact binary: int32 triCount, then triCount*9 float32.
public static class CollisionBake {
 public static int Write(Node3D track, string path) {
  var tris = new System.Collections.Generic.List<Vector3>();var support=new System.Collections.Generic.List<Vector3>();
  foreach (var node in track.FindChildren("*", "MeshInstance3D", true, false)) {
   var mi = (MeshInstance3D)node; if (!WallBuilder.IsSolid(mi)) continue; var xf = mi.GlobalTransform;
   var wallFaces=WallBuilder.WallFaces(mi,out var deck);
   foreach(var vertex in wallFaces)tris.Add(xf*vertex);
   foreach(var vertex in deck)support.Add(xf*vertex);
  }
  WriteFaces(ProjectSettings.GlobalizePath(path),tris);WriteFaces(ProjectSettings.GlobalizePath(path)+".support",support);
  return tris.Count / 3;
 }
 static void WriteFaces(string path,System.Collections.Generic.List<Vector3> vertices){using var fs=new System.IO.FileStream(path,FileMode.Create);using var bw=new BinaryWriter(fs);bw.Write(vertices.Count/3);foreach(var v in vertices){bw.Write(v.X);bw.Write(v.Y);bw.Write(-v.Z);}}
 static Vector3[] ReadFaces(string path){using var fs=new System.IO.FileStream(path,FileMode.Open);using var br=new BinaryReader(fs);int n=br.ReadInt32();if(n<0||fs.Length!=4L+n*36L)throw new InvalidDataException("Invalid collision triangle count");var pts=new Vector3[n*3];for(int i=0;i<pts.Length;i++)pts[i]=new Vector3(br.ReadSingle(),br.ReadSingle(),-br.ReadSingle());return pts;}
 public static StaticBody3D Read(string path) {
  var body=new StaticBody3D {Name="BakedWalls",CollisionLayer=2,CollisionMask=0};body.AddChild(new CollisionShape3D {Shape=new ConcavePolygonShape3D {Data=ReadFaces(path),BackfaceCollision=true}});
  if(System.IO.File.Exists(path+".support")){var deck=ReadFaces(path+".support");if(deck.Length>0){var support=new StaticBody3D {Name="BakedBridgeSupport",CollisionLayer=1,CollisionMask=0};support.AddChild(new CollisionShape3D {Shape=new ConcavePolygonShape3D {Data=deck,BackfaceCollision=true}});body.AddChild(support);}}
  return body;
 }
}
}
