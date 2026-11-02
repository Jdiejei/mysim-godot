using Godot;
namespace MySim {
// Opt-in visual-only near-field rain. Does not alter water, grip, drying or rain-rate simulation.
public partial class RainVisibilityCandidate : Node3D {
 public int Amount;Node3D observer;float height;
 public override void _Ready(){
  observer=GetParent<Node3D>();TopLevel=true;
  using var document=System.Text.Json.JsonDocument.Parse(FileAccess.GetFileAsString("res://data/rain_visibility_candidate.json"));var profile=document.RootElement;float V(string key)=>profile.GetProperty(key).GetSingle();height=V("spawnHeight");var weather=Conditions.RainVisual;float W(string key)=>weather.GetProperty(key).GetSingle();var wind=weather.GetProperty("wind");var direction=new Vector3(wind[0].GetSingle(),-W("fallSpeedMin"),wind[2].GetSingle()).Normalized();
  var material=new ShaderMaterial{Shader=new Shader{Code=@"shader_type spatial;
render_mode blend_mix, depth_draw_never, cull_disabled;
uniform vec4 tint : source_color = vec4(.55,.62,.70,.22);
void vertex(){
 vec3 facing=CAMERA_POSITION_WORLD-MODEL_MATRIX[3].xyz;facing.y=0.0;
 facing=normalize(facing+vec3(0.00001,0.0,0.0));vec3 right=cross(vec3(0.0,1.0,0.0),facing);
 MODELVIEW_MATRIX=VIEW_MATRIX*mat4(vec4(right,0.0),vec4(0.0,1.0,0.0,0.0),vec4(facing,0.0),MODEL_MATRIX[3]);
}
void fragment(){
 float edge=1.0-smoothstep(.10,.50,abs(UV.x-.5));
 float tip=smoothstep(0.0,.15,UV.y)*(1.0-smoothstep(.75,1.0,UV.y));
 ALBEDO=tint.rgb;ALPHA=tint.a*edge*tip;ROUGHNESS=1.0;SPECULAR=0.0;
}"}};var tint=weather.GetProperty("tint");material.SetShaderParameter("tint",new Color(tint[0].GetSingle(),tint[1].GetSingle(),tint[2].GetSingle(),W("opacity")));
  var process=new ParticleProcessMaterial{EmissionShape=ParticleProcessMaterial.EmissionShapeEnum.Box,EmissionBoxExtents=new Vector3(V("spawnHalfWidth"),.2f,V("spawnHalfWidth")),Direction=direction,Spread=2,InitialVelocityMin=W("fallSpeedMin"),InitialVelocityMax=W("fallSpeedMax"),Gravity=new Vector3(0,-6,0)};
  AddChild(new GpuParticles3D{Amount=Amount,Lifetime=W("lifetime"),ProcessMaterial=process,DrawPass1=new QuadMesh{Size=new Vector2(V("streakWidth"),V("streakLength")),Material=material},LocalCoords=false,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off,VisibilityAabb=new Aabb(new Vector3(-40,-30,-40),new Vector3(80,60,80))});Follow();
  GD.Print("RAIN_VISIBILITY_CANDIDATE amount=",Amount," halfWidth=",V("spawnHalfWidth")," streakWidth=",V("streakWidth")," streakLength=",V("streakLength")," worldAligned=true physicalWeatherUnchanged=true");
 }
 void Follow(){GlobalTransform=new Transform3D(Basis.Identity,observer.GlobalPosition+Vector3.Up*height);}
 public override void _Process(double delta){if(IsInstanceValid(observer))Follow();}
}
}
