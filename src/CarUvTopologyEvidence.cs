using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Private source-coordinate evidence only. Kept in a nested directory excluded from public runtime artifacts.
public static class CarUvTopologyEvidence {
 public static void Write(Node3D car,int index,string assemblyPath){
  if(System.Environment.GetEnvironmentVariable("MYSIM_FORMULA_UV_TOPOLOGY")!="1")return;
  if(index!=16)throw new Exception("UV topology evidence is scoped to Formula16");
  var rows=new List<object>();var inverse=car.GlobalTransform.AffineInverse();
  foreach(var node in car.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)node;string name=mesh.Name.ToString();if(!mesh.IsVisibleInTree()||mesh.Mesh==null||!(name=="LOD_A_BODY_mm_ext"||name=="LOD_A_FRONTBUMPER_mm_ext"||name=="LOD_A_REARBUMPER_mm_ext"))continue;
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
    var material=mesh.GetActiveMaterial(surface) as ShaderMaterial;if(material==null||!material.HasMeta("source_material_guid")||material.GetMeta("source_material_guid").AsString()!="449e4db78ecc6e21c9bd11a7fbf05975")throw new Exception("UV topology source assignment mismatch");
    using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var points=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var uv=arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array();var indices=arrays[(int)Mesh.ArrayType.Index].AsInt32Array();if(points.Length!=uv.Length||indices.Length%3!=0)throw new Exception("UV topology buffers incomplete");
    var vertices=new List<float[]>();foreach(var point in points){var v=inverse*mesh.GlobalTransform*point;vertices.Add(new[]{v.X,v.Y,v.Z});}var coords=new List<float[]>();foreach(var v in uv)coords.Add(new[]{v.X,v.Y});
    rows.Add(new{name,surface,texture=material.GetShaderParameter("base").As<Texture2D>()?.ResourcePath,vertices,uv=coords,indices});
   }
  }
  if(rows.Count!=3)throw new Exception("Require all three Formula16 source panels");
  string folder=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assemblyPath),"private-uv-topology");System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"panels.json"),System.Text.Json.JsonSerializer.Serialize(new{car=index,rows,scope="Private source mesh/UV correspondence; diagnostic only, originals unchanged"}));GD.Print("FORMULA_UV_TOPOLOGY private panels=3");
 }
}
}
