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
 public static int Build(Node3D track) {
  int count=0;
  foreach(var node in track.FindChildren("*","MeshInstance3D",true,false)) {
   var mesh=(MeshInstance3D)node;if(!IsSolid(mesh))continue;
   mesh.CreateTrimeshCollision();count++;
   foreach(var child in mesh.GetChildren())if(child is StaticBody3D body){body.CollisionLayer=2;body.CollisionMask=0;foreach(var shapeNode in body.GetChildren())if(shapeNode is CollisionShape3D cs&&cs.Shape is ConcavePolygonShape3D shape)shape.BackfaceCollision=true;}
  }
  GD.Print("WALLS physical meshes=",count);return count;
 }
}
}
