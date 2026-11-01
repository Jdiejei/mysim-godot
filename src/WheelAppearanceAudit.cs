using Godot;
using System.Collections.Generic;
using System.Text.Json;
namespace MySim {
// Diagnostic evidence from the actual assembled wheel, without modifying materials or geometry.
public static class WheelAppearanceAudit {
 public static void Write(Node3D[] wheels,string destination){
  var rows=new List<object>();
  for(int wheel=0;wheel<wheels.Length;wheel++){
   var nodes=wheels[wheel].FindChildren("*","MeshInstance3D",true,false);
   foreach(var node in nodes){var mesh=(MeshInstance3D)node;if(mesh.Mesh==null)continue;
    var surfaces=new List<object>();
    for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
     using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var normals=(Vector3[])arrays[(int)Mesh.ArrayType.Normal]??System.Array.Empty<Vector3>();int invalid=0;float minimum=1e9f,maximum=0;
     foreach(var normal in normals){float length=normal.Length();if(!normal.IsFinite()||length<.5f||length>1.5f)invalid++;minimum=Mathf.Min(minimum,length);maximum=Mathf.Max(maximum,length);}
     var material=mesh.GetActiveMaterial(surface);var standard=material as StandardMaterial3D;
     surfaces.Add(new {surface,normalCount=normals.Length,invalidNormals=invalid,minimumNormalLength=minimum,maximumNormalLength=maximum,materialClass=material?.GetClass().ToString(),colour=standard?.AlbedoColor.ToString(),roughness=standard?.Roughness,shading=standard?.ShadingMode.ToString(),texture=standard?.AlbedoTexture?.ResourcePath});
    }
    var bounds=mesh.Transform*mesh.GetAabb();rows.Add(new {wheel,node=mesh.GetPath().ToString(),visible=mesh.IsVisibleInTree(),boundsCentre=bounds.GetCenter().ToString(),boundsSize=bounds.Size.ToString(),surfaces});
   }nodes.Clear();
  }
  System.IO.File.WriteAllText(destination,JsonSerializer.Serialize(new {rows,scope="Actual assembled wheel material/normal evidence; not visual or physics acceptance"},new JsonSerializerOptions {WriteIndented=true}));
 }
}
}

