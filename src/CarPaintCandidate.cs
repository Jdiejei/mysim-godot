using Godot;
namespace MySim {
// Opt-in art candidate. Original geometry, UVs and material assignments remain recoverable.
public static class CarPaintCandidate {
 static readonly Shader shader=new(){Code=@"shader_type spatial;
render_mode cull_disabled;
uniform mat4 part_to_car;
uniform vec3 primary_color : source_color = vec3(0.55,0.012,0.018);
uniform vec3 accent_color : source_color = vec3(0.96,0.95,0.90);
uniform float paint_metallic=0.02;
uniform float paint_roughness=0.38;
uniform float coat_weight=0.55;
uniform float coat_roughness=0.23;
uniform vec3 bounds_min;
uniform vec3 bounds_size;
varying vec3 car_position;
void vertex(){car_position=(part_to_car*vec4(VERTEX,1.0)).xyz;}
void fragment(){
 vec3 p=(car_position-bounds_min)/max(bounds_size,vec3(0.001));
 float carbon=1.0-smoothstep(0.18,0.25,p.y);
 float centre=1.0-smoothstep(0.025,0.040,abs(p.x-0.5));
 float nose=smoothstep(0.55,0.72,p.z);
 float stripe=centre*nose;
 float side_band=smoothstep(0.36,0.38,p.y)*(1.0-smoothstep(0.40,0.42,p.y));
 float side=smoothstep(0.22,0.34,abs(p.x-0.5));
 vec3 paint=mix(primary_color,accent_color,max(stripe,side_band*side));
 ALBEDO=mix(paint,vec3(0.018,0.021,0.026),carbon);
 METALLIC=mix(paint_metallic,0.02,carbon);ROUGHNESS=mix(paint_roughness,0.57,carbon);
 SPECULAR=0.5;CLEARCOAT=coat_weight*(1.0-carbon);CLEARCOAT_ROUGHNESS=coat_roughness;
}"};
 public static void Apply(Node3D root,int car){
  if(System.Environment.GetEnvironmentVariable("MYSIM_CAR_PAINT_CANDIDATE")!="1"||car<15||car>18)return;
  using var profileDocument=System.Text.Json.JsonDocument.Parse(FileAccess.GetFileAsString("res://data/f1_paint_profiles.json"));var profile=profileDocument.RootElement.GetProperty("cars").GetProperty(car.ToString());
  Color Paint(string field){var rgb=profile.GetProperty(field);return new Color(rgb[0].GetSingle(),rgb[1].GetSingle(),rgb[2].GetSingle());}
  var nodes=root.FindChildren("*","MeshInstance3D",true,false);var inverse=root.GlobalTransform.AffineInverse();Aabb bounds=default;bool first=true;
  foreach(var node in nodes){var mesh=(MeshInstance3D)node;if(!mesh.IsVisibleInTree()||!mesh.Name.ToString().EndsWith("_mm_ext"))continue;var box=(inverse*mesh.GlobalTransform)*mesh.GetAabb();bounds=first?box:bounds.Merge(box);first=false;}
  if(first)throw new System.Exception("Paint candidate has no visible source livery geometry");
  foreach(var node in nodes){var mesh=(MeshInstance3D)node;if(!mesh.IsVisibleInTree()||!mesh.Name.ToString().EndsWith("_mm_ext"))continue;var material=new ShaderMaterial{Shader=shader};material.SetShaderParameter("primary_color",Paint("primary"));material.SetShaderParameter("paint_metallic",profile.GetProperty("metallic").GetSingle());material.SetShaderParameter("paint_roughness",profile.GetProperty("roughness").GetSingle());material.SetShaderParameter("coat_weight",profile.GetProperty("clearcoat").GetSingle());material.SetShaderParameter("coat_roughness",profile.GetProperty("clearcoatRoughness").GetSingle());material.SetShaderParameter("accent_color",Paint("accent"));material.SetShaderParameter("part_to_car",inverse*mesh.GlobalTransform);material.SetShaderParameter("bounds_min",bounds.Position);material.SetShaderParameter("bounds_size",bounds.Size);for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++)mesh.SetSurfaceOverrideMaterial(surface,material);}
  nodes.Clear();GD.Print("CAR_PAINT_CANDIDATE source geometry and UVs preserved; art acceptance pending");
 }
}
}