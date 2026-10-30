using Godot;
using System.Text.RegularExpressions;
namespace MySim {
// One collision-role policy shared by live scenes and collision baking.
public static class WallBuilder {
 static readonly Regex Barrier=new("armco|barrier|wall|fence|guardrail|bollard|tyrewall|tirewall",RegexOptions.IgnoreCase);
 static readonly Regex Structure=new("^(arc_|prop_|veh_)|building|bridge|pillar|grandstand|garage|pitwall",RegexOptions.IgnoreCase);
 static readonly Regex Decoration=new("overlay|decal|skid|dust|shadow|sky|cloud|glow|crowd|spectator|^org_|tree|bush|grass|leaf",RegexOptions.IgnoreCase);
 public static bool IsSolid(string name)=>Barrier.IsMatch(name);
 public static bool IsSolid(MeshInstance3D mesh) {
  string name=mesh.Name.ToString();
  if(!mesh.IsVisibleInTree()||mesh.Mesh==null||Decoration.IsMatch(name))return false;
  if(name.Contains("flag",System.StringComparison.OrdinalIgnoreCase)&&!name.Contains("pole",System.StringComparison.OrdinalIgnoreCase))return false;
  if(Barrier.IsMatch(name))return true;
  if(name.StartsWith("gnd_",System.StringComparison.OrdinalIgnoreCase)||name.StartsWith("road",System.StringComparison.OrdinalIgnoreCase))return false;
  if(Structure.IsMatch(name))return true;
  for(int i=0;i<mesh.Mesh.GetSurfaceCount();i++)if(Barrier.IsMatch(mesh.Mesh.SurfaceGetMaterial(i)?.ResourceName??""))return true;
  return false;
 }
 // Keep every source triangle: upward bridge deck faces support the car on layer1;
 // sides and downward soffits remain walls on layer2. Never exclude the whole bridge RID.
 public static Vector3[] WallFaces(MeshInstance3D mesh,out Vector3[] support) {
  var walls=new System.Collections.Generic.List<Vector3>();var floor=new System.Collections.Generic.List<Vector3>();
  bool bridge=mesh.Name.ToString().Contains("bridge",System.StringComparison.OrdinalIgnoreCase);
  var xf=mesh.GlobalTransform;
  var faces=mesh.Mesh.GetFaces();
  for(int i=0;i+2<faces.Length;i+=3){
   var a=xf*faces[i];var b=xf*faces[i+1];var c=xf*faces[i+2];
   // Godot front faces are clockwise, so the geometric cross points inward.
   var outward=-(b-a).Cross(c-a).Normalized();var target=bridge&&outward.Y>.98f?floor:walls;
   target.Add(faces[i]);target.Add(faces[i+1]);target.Add(faces[i+2]);
  }
  support=floor.ToArray();return walls.ToArray();
 }
 static void AddFaces(MeshInstance3D mesh,Vector3[] faces,uint layer,string name){
  if(faces.Length==0)return;var shape=new ConcavePolygonShape3D {Data=faces,BackfaceCollision=true};
  var body=new StaticBody3D {Name=name,CollisionLayer=layer,CollisionMask=0};body.AddChild(new CollisionShape3D {Shape=shape});mesh.AddChild(body);
 }
 public static int Build(Node3D track) {
  int count=0,supportTriangles=0;
  foreach(var node in track.FindChildren("*","MeshInstance3D",true,false)) {
   var mesh=(MeshInstance3D)node;if(!IsSolid(mesh))continue;
   var walls=WallFaces(mesh,out var support);AddFaces(mesh,walls,2,mesh.Name+"_col");AddFaces(mesh,support,1,mesh.Name+"_support");count++;supportTriangles+=support.Length/3;
  }
  GD.Print("WALLS physical meshes=",count," bridge support triangles=",supportTriangles);return count;
 }
}
}
