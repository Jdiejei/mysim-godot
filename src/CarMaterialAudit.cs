using Godot;
using System.Collections.Generic;
namespace MySim {
// Evidence only: report the materials actually used on visible assembled car surfaces.
public static class CarMaterialAudit {
 public static List<object> Describe(Node3D car) {
  var rows=new List<object>();
  foreach(var node in car.FindChildren("*","MeshInstance3D",true,false)) {
   var mesh=(MeshInstance3D)node;if(!mesh.IsVisibleInTree()||mesh.Mesh==null)continue;
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++) {
    var material=mesh.GetActiveMaterial(surface);var shader=material as ShaderMaterial;var standard=material as StandardMaterial3D;
    var arrays=mesh.Mesh.SurfaceGetArrays(surface);var uv=(Vector2[])arrays[(int)Mesh.ArrayType.TexUV];
    Vector2 minimum=new(float.PositiveInfinity,float.PositiveInfinity),maximum=new(float.NegativeInfinity,float.NegativeInfinity);
    if(uv!=null)foreach(var point in uv){minimum=minimum.Min(point);maximum=maximum.Max(point);}
    string texture=standard?.AlbedoTexture?.ResourcePath,mask=null;string scale=standard?.Uv1Scale.ToString(),offset=standard?.Uv1Offset.ToString();
    if(shader!=null){texture=shader.GetShaderParameter("base").As<Texture2D>()?.ResourcePath;mask=shader.GetShaderParameter("mask").As<Texture2D>()?.ResourcePath;scale=shader.GetShaderParameter("uv_scale").ToString();offset=shader.GetShaderParameter("uv_offset").ToString();}
    rows.Add(new {renderer=mesh.Name.ToString(),path=car.GetPathTo(mesh).ToString(),surface,sourceMaterial=mesh.Mesh.SurfaceGetMaterial(surface)?.ResourceName,sourceGuid=material!=null&&material.HasMeta("source_material_guid")?material.GetMeta("source_material_guid").AsString():null,type=material?.GetType().Name,texture,mask,uvScale=scale,uvOffset=offset,uvCount=uv?.Length??0,uvMinimum=uv!=null&&uv.Length>0?minimum.ToString():null,uvMaximum=uv!=null&&uv.Length>0?maximum.ToString():null});
   }
  }
  return rows;
 }
}
}
