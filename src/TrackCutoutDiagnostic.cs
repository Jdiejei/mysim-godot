using Godot;
using System.Collections.Generic;
namespace MySim {
// Controlled isolation of the observed night fence/foliage speckle. Never changes source assets.
// Each mode changes one cause only; a matching baseline is required before choosing a fix.
public static class TrackCutoutDiagnostic {
 public static void Apply(Node3D track){
  string mode=System.Environment.GetEnvironmentVariable("MYSIM_TRACK_CUTOUT_DIAGNOSTIC");
  if(string.IsNullOrEmpty(mode))return;
  if(mode!="baseline"&&mode!="coverage_off"&&mode!="shadows_off"&&mode!="unshaded")throw new System.ArgumentException("Unknown track cutout diagnostic: "+mode);
  var rows=new List<object>();
  foreach(var node in track.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)node;if(mesh.Mesh==null||!mesh.IsVisibleInTree())continue;
   bool cutout=false;
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
    if(mesh.GetActiveMaterial(surface) is not StandardMaterial3D source||source.Transparency!=BaseMaterial3D.TransparencyEnum.AlphaScissor)continue;
    cutout=true;var material=source;
    if(mode=="coverage_off"||mode=="unshaded"){
     material=(StandardMaterial3D)source.Duplicate();
     if(mode=="coverage_off")material.AlphaAntialiasingMode=BaseMaterial3D.AlphaAntiAliasing.Off;
     if(mode=="unshaded")material.ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded;
     mesh.SetSurfaceOverrideMaterial(surface,material);
    }
    rows.Add(new{renderer=mesh.GetPath().ToString(),surface,texture=source.AlbedoTexture?.ResourcePath,sourceAlphaAA=source.AlphaAntialiasingMode.ToString(),actualAlphaAA=material.AlphaAntialiasingMode.ToString(),threshold=material.AlphaScissorThreshold,edge=material.AlphaAntialiasingEdge,filter=material.TextureFilter.ToString(),cull=material.CullMode.ToString(),uvScale=new[]{material.Uv1Scale.X,material.Uv1Scale.Y},uvOffset=new[]{material.Uv1Offset.X,material.Uv1Offset.Y},sourceShadow=mesh.CastShadow.ToString(),actualShadow=mode=="shadows_off"?GeometryInstance3D.ShadowCastingSetting.Off.ToString():mesh.CastShadow.ToString(),emissionEnabled=material.EmissionEnabled,shading=material.ShadingMode.ToString()});
   }
   if(cutout&&mode=="shadows_off")mesh.CastShadow=GeometryInstance3D.ShadowCastingSetting.Off;
  }
  GD.Print("TRACK_CUTOUT_DIAGNOSTIC mode=",mode," surfaces=",rows.Count);
  if(System.Environment.GetEnvironmentVariable("MYSIM_BINDING_AUDIT") is string dir){System.IO.Directory.CreateDirectory(dir);string name=System.Text.RegularExpressions.Regex.Replace(track.Name.ToString(),"[^A-Za-z0-9_-]","_");System.IO.File.WriteAllText(System.IO.Path.Combine(dir,"cutout-"+name+"-"+mode+".json"),System.Text.Json.JsonSerializer.Serialize(new{mode,layer=track.Name.ToString(),surfaces=rows,scope="One-variable diagnostic; no geometry/UV/source-texture/collision changes; not a promoted rendering fix"}));}
 }
}
}
