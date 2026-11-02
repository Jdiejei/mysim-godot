using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
namespace MySim {
// Read-only world-space geometry evidence. Duplicate candidates are never hidden/deleted.
public static class TrackSceneAudit {
 internal static object UvSummary(Vector2[] uv,Vector3[] points,int[] indices,Transform3D transform){
  if(uv==null||uv.Length!=points.Length)return new{present=false,count=uv?.Length??0};
  var low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);int nonfinite=0,collapsed=0;double area=0,uvArea=0;
  foreach(var p in uv){if(!p.IsFinite()){nonfinite++;continue;}low=new Vector2(Mathf.Min(low.X,p.X),Mathf.Min(low.Y,p.Y));high=new Vector2(Mathf.Max(high.X,p.X),Mathf.Max(high.Y,p.Y));}
  int count=indices?.Length??points.Length;for(int i=0;i+2<count;i+=3){int a=indices==null?i:indices[i],b=indices==null?i+1:indices[i+1],c=indices==null?i+2:indices[i+2];if(!uv[a].IsFinite()||!uv[b].IsFinite()||!uv[c].IsFinite())continue;float ua=Mathf.Abs((uv[b]-uv[a]).Cross(uv[c]-uv[a]))*.5f;float wa=((transform*points[b]-transform*points[a]).Cross(transform*points[c]-transform*points[a])).Length()*.5f;if(wa<=1e-10f)continue;if(ua<=1e-12f)collapsed++;else{area+=wa;uvArea+=ua;}}
  return new{present=true,count=uv.Length,nonfinite,minimum=nonfinite==uv.Length?null:new[]{low.X,low.Y},maximum=nonfinite==uv.Length?null:new[]{high.X,high.Y},collapsedTriangles=collapsed,worldArea=area,uvArea,metresPerUv=uvArea>0?(double?)Math.Sqrt(area/uvArea):null,scope="Read-only channel density; tiling intent and texture orientation require authored material and rendered verification"};
 }
 public static void Write(List<Node3D> layers,int track,string destination){
  var groups=new Dictionary<string,List<object>>();var lamps=new List<object>();int meshes=0,hidden=0,hiddenPhysical=0;var objects=new List<object>();
  foreach(var layer in layers){var children=layer.FindChildren("*","MeshInstance3D",true,false);foreach(var node in children){
   var mesh=(MeshInstance3D)node;if(mesh.Mesh==null)continue;if(mesh.IsVisibleInTree())meshes++;else hidden++;
   using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);int vertices=0,indices=0;var materials=new List<object>();
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
    using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var points=(Vector3[])arrays[(int)Mesh.ArrayType.Vertex];var triangles=(int[])arrays[(int)Mesh.ArrayType.Index];vertices+=points.Length;indices+=triangles?.Length??0;
    using var bytes=new MemoryStream();using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true)){
     writer.Write(points.Length);foreach(var point in points){var world=mesh.GlobalTransform*point;writer.Write((float)Math.Round(world.X,4));writer.Write((float)Math.Round(world.Y,4));writer.Write((float)Math.Round(world.Z,4));}
     writer.Write(triangles?.Length??0);if(triangles!=null)foreach(int index in triangles)writer.Write(index);
    }hash.AppendData(bytes.ToArray());var material=mesh.GetActiveMaterial(surface);object appearance=null;
    if(material is StandardMaterial3D standard)appearance=new {texture=standard.AlbedoTexture?.ResourcePath,color=new[]{standard.AlbedoColor.R,standard.AlbedoColor.G,standard.AlbedoColor.B,standard.AlbedoColor.A},transparency=standard.Transparency.ToString(),cull=standard.CullMode.ToString(),roughness=standard.Roughness,uvScale=new[]{standard.Uv1Scale.X,standard.Uv1Scale.Y},uvOffset=new[]{standard.Uv1Offset.X,standard.Uv1Offset.Y}};
    else if(material is ShaderMaterial shader)appearance=new {shader=shader.Shader?.ResourcePath,texture=shader.GetShaderParameter("tex").AsGodotObject() is Texture2D texture?texture.ResourcePath:null};
    materials.Add(new {surface,name=material?.ResourceName,type=material?.GetClass(),appearance,uv0=UvSummary((Vector2[])arrays[(int)Mesh.ArrayType.TexUV],points,triangles,mesh.GlobalTransform),uv1=UvSummary((Vector2[])arrays[(int)Mesh.ArrayType.TexUV2],points,triangles,mesh.GlobalTransform)});
   }
   string key=Convert.ToHexString(hash.GetHashAndReset());var bounds=mesh.GlobalTransform*mesh.GetAabb();
   int wallBodies=0;var meshChildren=mesh.GetChildren();foreach(var child in meshChildren)if(child is StaticBody3D body&&(body.CollisionLayer&2)!=0)wallBodies++;meshChildren.Clear();
   if(wallBodies>0&&!mesh.IsVisibleInTree())hiddenPhysical++;
   var record=new {wallBodies,placeholderCollisionRetained=mesh.HasMeta("placeholder_branding_visible_collision"),path=mesh.GetPath().ToString(),name=mesh.Name.ToString(),visible=mesh.Visible,visibleInTree=mesh.IsVisibleInTree(),castShadow=mesh.CastShadow.ToString(),vertices,indices,materials,centre=new[]{bounds.GetCenter().X,bounds.GetCenter().Y,bounds.GetCenter().Z},size=new[]{bounds.Size.X,bounds.Size.Y,bounds.Size.Z}};
   objects.Add(record);
   if(!groups.ContainsKey(key))groups[key]=new List<object>();groups[key].Add(record);
   string label=mesh.Name.ToString().ToLowerInvariant();if(label.Contains("lamp")||label.Contains("flood")||label.Contains("light"))lamps.Add(record);
  }
  children.Clear();
  }
  var duplicates=new List<object>();foreach(var pair in groups)if(pair.Value.Count>1)duplicates.Add(new {worldGeometryHash=pair.Key,instances=pair.Value});
  Directory.CreateDirectory(Path.GetDirectoryName(destination));System.IO.File.WriteAllText(destination,System.Text.Json.JsonSerializer.Serialize(new {track,visibleMeshes=meshes,hiddenMeshes=hidden,hiddenPhysicalMeshes=hiddenPhysical,objects,duplicateGroups=duplicates,lightingNameCandidates=lamps,scope="Read-only geometry candidates quantized0.1mm; intentional overlays/LOD require manual review. No automatic hiding, deduplication or collision change."},new System.Text.Json.JsonSerializerOptions {WriteIndented=true}));
  if(hiddenPhysical>0)throw new InvalidOperationException("Prepared track retains invisible physical wall renderers: "+hiddenPhysical);
  GD.Print("PASS read-only track scene audit meshes=",meshes," duplicateGroups=",duplicates.Count," lightingCandidates=",lamps.Count);
 }
}
}
