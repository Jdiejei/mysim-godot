using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Read-only assembly evidence; never hides, moves, rewrites UVs or replaces a source part.
public static class CarAssemblyAudit {
 static object UVStats(Godot.Collections.Array arrays,Mesh.ArrayType channel){
  var value=arrays[(int)channel];if(value.VariantType==Variant.Type.Nil)return new {present=false,count=0};
  var points=value.AsVector2Array();int invalid=0;var minimum=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var maximum=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
  foreach(var point in points){if(!float.IsFinite(point.X)||!float.IsFinite(point.Y)){invalid++;continue;}minimum=minimum.Min(point);maximum=maximum.Max(point);}
  return new {present=points.Length>0,count=points.Length,invalid,minimum=points.Length>invalid?new[]{minimum.X,minimum.Y}:null,maximum=points.Length>invalid?new[]{maximum.X,maximum.Y}:null};
 }
 public static void Write(Node3D car,int index,string path){
  RecoveredComponentMounts.VerifyRepeatApply(car,index);
  CarUvTopologyEvidence.Write(car,index,path);
  var rows=new List<object>();var children=car.FindChildren("*","MeshInstance3D",true,false);
  foreach(var child in children){var mesh=(MeshInstance3D)child;if(mesh.Mesh==null)continue;var box=(car.GlobalTransform.AffineInverse()*mesh.GlobalTransform)*mesh.GetAabb();var materials=new List<object>();
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var material=mesh.GetActiveMaterial(surface);var shader=material as ShaderMaterial;var texture=shader?.GetShaderParameter("base").As<Texture2D>();materials.Add(new {surface,uv0=UVStats(arrays,Mesh.ArrayType.TexUV),uv2=UVStats(arrays,Mesh.ArrayType.TexUV2),uvScale=shader?.GetShaderParameter("uv_scale").AsVector2().ToString(),uvOffset=shader?.GetShaderParameter("uv_offset").AsVector2().ToString(),source=material!=null&&material.HasMeta("source_material_guid")?material.GetMeta("source_material_guid").AsString():null,texture=texture?.ResourcePath,alphaMode=shader?.GetShaderParameter("alpha_mode").AsDouble(),materialType=material?.GetType().Name});}
   var blendShapes=new List<object>();if(mesh.Mesh is ArrayMesh am)for(int i=0;i<am.GetBlendShapeCount();i++)blendShapes.Add(new {name=am.GetBlendShapeName(i).ToString(),value=mesh.GetBlendShapeValue(i)});
   rows.Add(new {name=mesh.Name.ToString(),path=car.GetPathTo(mesh).ToString(),visible=mesh.IsVisibleInTree(),localPosition=new[]{mesh.Position.X,mesh.Position.Y,mesh.Position.Z},centre=new[]{box.GetCenter().X,box.GetCenter().Y,box.GetCenter().Z},size=new[]{box.Size.X,box.Size.Y,box.Size.Z},materials,blendShapes});
  }children.Clear();System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(new {car=index,scope="Read-only imported assembly and material evidence; not geometry acceptance",rows,visibleMaterials=CarMaterialAudit.Describe(car)},new System.Text.Json.JsonSerializerOptions {WriteIndented=true}));GD.Print("PASS read-only car assembly evidence ",rows.Count);
 }
}
}
