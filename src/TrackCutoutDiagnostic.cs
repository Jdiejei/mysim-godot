using Godot;
using System.Collections.Generic;
namespace MySim {
// Controlled isolation of the observed night fence/foliage speckle. Never changes source assets.
// Each mode changes one cause only; a matching baseline is required before choosing a fix.
public static class TrackCutoutDiagnostic {
 public static void Apply(Node3D track){
  string mode=System.Environment.GetEnvironmentVariable("MYSIM_TRACK_CUTOUT_DIAGNOSTIC");
  if(string.IsNullOrEmpty(mode))return;
  if(mode!="baseline"&&mode!="coverage_off"&&mode!="shadows_off"&&mode!="unshaded"&&mode!="specular_off"&&mode!="opaque_metal_candidate"&&mode!="filtered_metal_candidate"&&mode!="identity"&&mode!="fence_blend_candidate"&&mode!="fence_hash_candidate")throw new System.ArgumentException("Unknown track cutout diagnostic: "+mode);
  var rows=new List<object>();
  if(mode=="filtered_metal_candidate"&&System.Environment.GetEnvironmentVariable("MYSIM_BINDING_AUDIT") is string evidence){
   System.IO.Directory.CreateDirectory(evidence);
   var texture=GD.Load<Texture2D>("res://assets/RealRacing/RecoveredNativeTextures/common/mat/metal/mat_metal_scafolding_d.png");
   using var original=texture.GetImage();using var alpha=Image.CreateEmpty(original.GetWidth(),original.GetHeight(),false,Image.Format.Rgba8);
   var histogram=new int[256];
   for(int y=0;y<original.GetHeight();y++)for(int x=0;x<original.GetWidth();x++){
    float a=original.GetPixel(x,y).A;histogram[Mathf.Clamp(Mathf.RoundToInt(a*255),0,255)]++;alpha.SetPixel(x,y,new Color(a,a,a,1));
   }
   alpha.SavePng(System.IO.Path.Combine(evidence,"grandstand-scaffold-d-source-alpha.png"));
   System.IO.File.WriteAllText(System.IO.Path.Combine(evidence,"grandstand-scaffold-d-source-alpha.json"),System.Text.Json.JsonSerializer.Serialize(new{width=original.GetWidth(),height=original.GetHeight(),histogram,scope="Actual imported source alpha channel; no source mutation or inferred authoring semantics"}));
  }
  foreach(var node in track.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)node;if(mesh.Mesh==null||!mesh.IsVisibleInTree())continue;
   bool cutout=false;
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
    if(mesh.GetActiveMaterial(surface) is not StandardMaterial3D source||source.Transparency!=BaseMaterial3D.TransparencyEnum.AlphaScissor)continue;
    cutout=true;var material=source;
    string textureName=System.IO.Path.GetFileName(source.AlbedoTexture?.ResourcePath??"");bool opaqueMetal=mode=="opaque_metal_candidate"&&(textureName=="mat_fence_metal_catch_01.png"||textureName=="mat_metal_scafolding_g.png");
    bool filteredMetal=mode=="filtered_metal_candidate"&&textureName=="mat_metal_scafolding_d.png";
    bool blendFence=mode=="fence_blend_candidate"&&(textureName=="mat_fence_wire_04.png"||textureName=="mat_fence_small_wire_01.png"||textureName=="mat_fence_metal_catch_01.png");
    bool hashFence=mode=="fence_hash_candidate"&&(textureName=="mat_fence_wire_04.png"||textureName=="mat_fence_small_wire_01.png"||textureName=="mat_fence_metal_catch_01.png");
    if(mode=="identity"||mode=="coverage_off"||mode=="unshaded"||mode=="specular_off"||opaqueMetal||filteredMetal||blendFence||hashFence){
     material=(StandardMaterial3D)source.Duplicate();
     if(mode=="identity"){string path=source.AlbedoTexture?.ResourcePath??"";material.Transparency=BaseMaterial3D.TransparencyEnum.Disabled;material.AlbedoTexture=null;material.ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded;material.AlbedoColor=path.Contains("/crowd/")?Colors.Red:path.Contains("/org/")?Colors.Green:path.Contains("/fence/")?Colors.Blue:path.Contains("scafolding")?Colors.Yellow:Colors.Magenta;}
     if(opaqueMetal)material.Transparency=BaseMaterial3D.TransparencyEnum.Disabled;
     // Retain filtered fractional source alpha instead of binary thresholding mip levels.
     // Exact reference-track wire textures only: preserve holes, geometry, UVs and source colour.
     if(filteredMetal)material.Transparency=BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
     // True fractional blending: unlike depth prepass, no binary alpha depth write.
     // Diagnostic only; inspect ordering and holes before any production adoption.
     if(blendFence){material.Transparency=BaseMaterial3D.TransparencyEnum.Alpha;material.AlphaAntialiasingMode=BaseMaterial3D.AlphaAntiAliasing.Off;}
     // Hash dither keeps binary depth writes (no order/holes) but replaces hard mip-threshold flicker with a stochastic screen-door cutout.
     if(hashFence){material.Transparency=BaseMaterial3D.TransparencyEnum.AlphaScissor;material.AlphaAntialiasingMode=BaseMaterial3D.AlphaAntiAliasing.AlphaToCoverageAndToOne;}
     if(mode=="coverage_off")material.AlphaAntialiasingMode=BaseMaterial3D.AlphaAntiAliasing.Off;
     if(mode=="specular_off"){material.Metallic=0;material.MetallicSpecular=0;material.ClearcoatEnabled=false;}
     if(mode=="unshaded")material.ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded;
     mesh.SetSurfaceOverrideMaterial(surface,material);
    }
    rows.Add(new{renderer=mesh.GetPath().ToString(),surface,worldOrigin=new[]{mesh.GlobalPosition.X,mesh.GlobalPosition.Y,mesh.GlobalPosition.Z},boundsSize=new[]{mesh.GetAabb().Size.X,mesh.GetAabb().Size.Y,mesh.GetAabb().Size.Z},texture=source.AlbedoTexture?.ResourcePath,sourceTransparency=source.Transparency.ToString(),actualTransparency=material.Transparency.ToString(),opaqueMetal,sourceAlphaAA=source.AlphaAntialiasingMode.ToString(),actualAlphaAA=material.AlphaAntialiasingMode.ToString(),threshold=material.AlphaScissorThreshold,edge=material.AlphaAntialiasingEdge,filter=material.TextureFilter.ToString(),cull=material.CullMode.ToString(),uvScale=new[]{material.Uv1Scale.X,material.Uv1Scale.Y},uvOffset=new[]{material.Uv1Offset.X,material.Uv1Offset.Y},sourceShadow=mesh.CastShadow.ToString(),actualShadow=mode=="shadows_off"?GeometryInstance3D.ShadowCastingSetting.Off.ToString():mesh.CastShadow.ToString(),sourceMetallic=source.Metallic,actualMetallic=material.Metallic,sourceSpecular=source.MetallicSpecular,actualSpecular=material.MetallicSpecular,actualClearcoat=material.ClearcoatEnabled,emissionEnabled=material.EmissionEnabled,shading=material.ShadingMode.ToString()});
   }
   if(cutout&&mode=="shadows_off")mesh.CastShadow=GeometryInstance3D.ShadowCastingSetting.Off;
  }
  GD.Print("TRACK_CUTOUT_DIAGNOSTIC mode=",mode," surfaces=",rows.Count);
  if(System.Environment.GetEnvironmentVariable("MYSIM_BINDING_AUDIT") is string dir){System.IO.Directory.CreateDirectory(dir);string name=System.Text.RegularExpressions.Regex.Replace(track.Name.ToString(),"[^A-Za-z0-9_-]","_");System.IO.File.WriteAllText(System.IO.Path.Combine(dir,"cutout-"+name+"-"+mode+".json"),System.Text.Json.JsonSerializer.Serialize(new{mode,layer=track.Name.ToString(),surfaces=rows,scope="One-variable diagnostic; no geometry/UV/source-texture/collision changes; not a promoted rendering fix"}));}
 }
}
}
