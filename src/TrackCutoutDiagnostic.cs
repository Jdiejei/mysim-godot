using Godot;
using System.Collections.Generic;
namespace MySim {
// Controlled isolation of the observed night fence/foliage speckle. Never changes source assets.
// Each mode changes one cause only; a matching baseline is required before choosing a fix.
public static class TrackCutoutDiagnostic {
 public static void Apply(Node3D track){
  string mode=System.Environment.GetEnvironmentVariable("MYSIM_TRACK_CUTOUT_DIAGNOSTIC");
  if(string.IsNullOrEmpty(mode))return;
  if(mode!="baseline"&&mode!="coverage_off"&&mode!="shadows_off"&&mode!="unshaded"&&mode!="specular_off"&&mode!="opaque_metal_candidate"&&mode!="filtered_metal_candidate")throw new System.ArgumentException("Unknown track cutout diagnostic: "+mode);
  var rows=new List<object>();
  foreach(var node in track.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)node;if(mesh.Mesh==null||!mesh.IsVisibleInTree())continue;
   bool cutout=false;
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
    if(mesh.GetActiveMaterial(surface) is not StandardMaterial3D source||source.Transparency!=BaseMaterial3D.TransparencyEnum.AlphaScissor)continue;
    cutout=true;var material=source;
    string textureName=System.IO.Path.GetFileName(source.AlbedoTexture?.ResourcePath??"");bool opaqueMetal=mode=="opaque_metal_candidate"&&(textureName=="mat_fence_metal_catch_01.png"||textureName=="mat_metal_scafolding_g.png");
    bool filteredMetal=mode=="filtered_metal_candidate"&&(textureName=="mat_fence_metal_catch_01.png"||textureName=="mat_metal_scafolding_g.png");
    if(mode=="coverage_off"||mode=="unshaded"||mode=="specular_off"||opaqueMetal||filteredMetal){
     material=(StandardMaterial3D)source.Duplicate();
     if(opaqueMetal)material.Transparency=BaseMaterial3D.TransparencyEnum.Disabled;
     // Retain filtered fractional source alpha instead of binary thresholding mip levels.
     // Exact two-texture experiment only: preserve holes, geometry, UVs and source colour.
     if(filteredMetal)material.Transparency=BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
     if(mode=="coverage_off")material.AlphaAntialiasingMode=BaseMaterial3D.AlphaAntiAliasing.Off;
     if(mode=="specular_off"){material.Metallic=0;material.MetallicSpecular=0;material.ClearcoatEnabled=false;}
     if(mode=="unshaded")material.ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded;
     mesh.SetSurfaceOverrideMaterial(surface,material);
    }
    rows.Add(new{renderer=mesh.GetPath().ToString(),surface,texture=source.AlbedoTexture?.ResourcePath,sourceTransparency=source.Transparency.ToString(),actualTransparency=material.Transparency.ToString(),opaqueMetal,sourceAlphaAA=source.AlphaAntialiasingMode.ToString(),actualAlphaAA=material.AlphaAntialiasingMode.ToString(),threshold=material.AlphaScissorThreshold,edge=material.AlphaAntialiasingEdge,filter=material.TextureFilter.ToString(),cull=material.CullMode.ToString(),uvScale=new[]{material.Uv1Scale.X,material.Uv1Scale.Y},uvOffset=new[]{material.Uv1Offset.X,material.Uv1Offset.Y},sourceShadow=mesh.CastShadow.ToString(),actualShadow=mode=="shadows_off"?GeometryInstance3D.ShadowCastingSetting.Off.ToString():mesh.CastShadow.ToString(),sourceMetallic=source.Metallic,actualMetallic=material.Metallic,sourceSpecular=source.MetallicSpecular,actualSpecular=material.MetallicSpecular,actualClearcoat=material.ClearcoatEnabled,emissionEnabled=material.EmissionEnabled,shading=material.ShadingMode.ToString()});
   }
   if(cutout&&mode=="shadows_off")mesh.CastShadow=GeometryInstance3D.ShadowCastingSetting.Off;
  }
  GD.Print("TRACK_CUTOUT_DIAGNOSTIC mode=",mode," surfaces=",rows.Count);
  if(System.Environment.GetEnvironmentVariable("MYSIM_BINDING_AUDIT") is string dir){System.IO.Directory.CreateDirectory(dir);string name=System.Text.RegularExpressions.Regex.Replace(track.Name.ToString(),"[^A-Za-z0-9_-]","_");System.IO.File.WriteAllText(System.IO.Path.Combine(dir,"cutout-"+name+"-"+mode+".json"),System.Text.Json.JsonSerializer.Serialize(new{mode,layer=track.Name.ToString(),surfaces=rows,scope="One-variable diagnostic; no geometry/UV/source-texture/collision changes; not a promoted rendering fix"}));}
 }
}
}
