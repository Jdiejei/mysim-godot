using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Explicit missing dihedral-coordinate experiment, not an accepted atlas repair.
// Exact Chiron source GUID+texture guard; no mesh UV or original shared shader mutation.
public static class CarAtlasTransposeDiagnostic {
 static readonly Dictionary<string,Shader> sharedShaders=new(StringComparer.Ordinal);
 public static Material Apply(Material material,int car,string renderer,string guid){
  if(System.Environment.GetEnvironmentVariable("MYSIM_CHIRON_ATLAS_TRANSPOSE")!="1"||car!=20||!renderer.EndsWith("_mm_ext",StringComparison.Ordinal))return material;
  const string expectedGuid="6cbf28e8030d8558fb2926454456387e",expectedTexture="res://assets/RealRacing/Source/2017_bugatti_chiron/livery/2017_bugatti_chiron_ext_blue.png";
  if(guid!=expectedGuid||material is not ShaderMaterial original||original.GetShaderParameter("base").As<Texture2D>()?.ResourcePath!=expectedTexture)throw new InvalidOperationException("Chiron transpose diagnostic source material mismatch:"+renderer);
  const string before="vec2 uv = mix(UV, UV2, use_uv2) * uv_scale + uv_offset;",after="vec2 uv = mix(UV, UV2, use_uv2).yx * uv_scale + uv_offset;";
  if(!original.Shader.Code.Contains(before,StringComparison.Ordinal))throw new Exception("Chiron diagnostic shader equation changed");
  string code=original.Shader.Code.Replace(before,after,StringComparison.Ordinal);
  if(!sharedShaders.TryGetValue(code,out var shader)){shader=new Shader{Code=code};sharedShaders.Add(code,shader);}
  var candidate=(ShaderMaterial)original.Duplicate();candidate.Shader=shader;foreach(var uniform in original.Shader.GetShaderUniformList()){string name=uniform.AsGodotDictionary()["name"].AsString();candidate.SetShaderParameter(name,original.GetShaderParameter(name));}candidate.SetMeta("diagnostic_chiron_uv_transpose",true);GD.Print("CHIRON_ATLAS_TRANSPOSE diagnostic renderer=",renderer," exactGUID=",guid," sharedShaderVariants=",sharedShaders.Count," originalUVbuffersunchanged");return candidate;
 }
}
}
