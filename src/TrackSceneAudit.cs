using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
namespace MySim {
// Read-only world-space geometry evidence. Duplicate candidates are never hidden/deleted.
public static class TrackSceneAudit {
 public static void Write(List<Node3D> layers,int track,string destination){
  var groups=new Dictionary<string,List<object>>();var lamps=new List<object>();int meshes=0,hidden=0;var objects=new List<object>();
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
    materials.Add(new {surface,name=material?.ResourceName,type=material?.GetClass(),appearance});
   }
   string key=Convert.ToHexString(hash.GetHashAndReset());var bounds=mesh.GlobalTransform*mesh.GetAabb();
   var record=new {path=mesh.GetPath().ToString(),name=mesh.Name.ToString(),visible=mesh.Visible,visibleInTree=mesh.IsVisibleInTree(),castShadow=mesh.CastShadow.ToString(),vertices,indices,materials,centre=new[]{bounds.GetCenter().X,bounds.GetCenter().Y,bounds.GetCenter().Z},size=new[]{bounds.Size.X,bounds.Size.Y,bounds.Size.Z}};
   objects.Add(record);
   if(!groups.ContainsKey(key))groups[key]=new List<object>();groups[key].Add(record);
   string label=mesh.Name.ToString().ToLowerInvariant();if(label.Contains("lamp")||label.Contains("flood")||label.Contains("light"))lamps.Add(record);
  }
  children.Clear();
  }
  var duplicates=new List<object>();foreach(var pair in groups)if(pair.Value.Count>1)duplicates.Add(new {worldGeometryHash=pair.Key,instances=pair.Value});
  Directory.CreateDirectory(Path.GetDirectoryName(destination));System.IO.File.WriteAllText(destination,System.Text.Json.JsonSerializer.Serialize(new {track,visibleMeshes=meshes,hiddenMeshes=hidden,objects,duplicateGroups=duplicates,lightingNameCandidates=lamps,scope="Read-only geometry candidates quantized0.1mm; intentional overlays/LOD require manual review. No automatic hiding, deduplication or collision change."},new System.Text.Json.JsonSerializerOptions {WriteIndented=true}));
  GD.Print("PASS read-only track scene audit meshes=",meshes," duplicateGroups=",duplicates.Count," lightingCandidates=",lamps.Count);
 }
}
}
