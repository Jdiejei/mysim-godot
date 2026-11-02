using Godot;
namespace MySim {
// Opt-in visual-only near-field rain. Does not alter water, grip, drying or rain-rate simulation.
public partial class RainVisibilityCandidate : Node3D {
 public int Amount;Node3D observer;float height,halfWidth,fallSpeed,age;Vector3 previousObserver;bool sampled,predict;readonly System.Collections.Generic.List<object> samples=new();string audit;
 public override void _Ready(){
  observer=GetParent<Node3D>();TopLevel=true;
  using var document=System.Text.Json.JsonDocument.Parse(FileAccess.GetFileAsString("res://data/rain_visibility_candidate.json"));var profile=document.RootElement;float V(string key)=>profile.GetProperty(key).GetSingle();height=V("spawnHeight");halfWidth=V("spawnHalfWidth");var weather=Conditions.RainVisual;float W(string key)=>weather.GetProperty(key).GetSingle();var wind=weather.GetProperty("wind");var direction=new Vector3(wind[0].GetSingle(),-W("fallSpeedMin"),wind[2].GetSingle()).Normalized();fallSpeed=(W("fallSpeedMin")+W("fallSpeedMax"))*.5f*-direction.Y;predict=System.Environment.GetEnvironmentVariable("MYSIM_RAIN_PREDICT_SPAWN")=="1";audit=System.Environment.GetEnvironmentVariable("MYSIM_RAIN_PREDICT_AUDIT");
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
  // World-space historical drops can trail the predicted emitter by up to twice the bounded
  // 50m offset. This wider culling box changes visibility bounds only, never particle motion.
  var bounds=predict?new Aabb(new Vector3(-140,-40,-140),new Vector3(280,80,280)):new Aabb(new Vector3(-40,-30,-40),new Vector3(80,60,80));
  AddChild(new GpuParticles3D{Amount=Amount,Lifetime=W("lifetime"),ProcessMaterial=process,DrawPass1=new QuadMesh{Size=new Vector2(V("streakWidth"),V("streakLength")),Material=material},LocalCoords=false,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off,VisibilityAabb=bounds});Follow(0);
  GD.Print("RAIN_VISIBILITY_CANDIDATE amount=",Amount," halfWidth=",V("spawnHalfWidth")," streakWidth=",V("streakWidth")," streakLength=",V("streakLength")," worldAligned=true physicalWeatherUnchanged=true");
 }
 void Follow(float seconds){var position=observer.GlobalPosition;var velocity=sampled&&seconds>.00001f?(position-previousObserver)/seconds:Vector3.Zero;previousObserver=position;sampled=true;velocity.Y=0;
  // Solve height = v*t + .5*g*t² using unchanged downward particle gravity (6m/s²).
  float travel=(Mathf.Sqrt(fallSpeed*fallSpeed+12f*height)-fallSpeed)/6f;var requested=velocity*travel;var offset=predict?requested.LimitLength(50f):Vector3.Zero;GlobalTransform=new Transform3D(Basis.Identity,position+Vector3.Up*height+offset);
  age+=seconds;if(audit!=null&&samples.Count<120&&(samples.Count==0||age>=.25f)){age=0;var error=requested-offset;samples.Add(new{elapsedWallMs=Time.GetTicksMsec(),observedHorizontalSpeed=velocity.Length(),velocityX=velocity.X,velocityZ=velocity.Z,spawnX=GlobalPosition.X,spawnY=GlobalPosition.Y,spawnZ=GlobalPosition.Z,observerX=position.X,observerY=position.Y,observerZ=position.Z,travelTime=travel,requestedOffset=requested.Length(),appliedOffset=offset.Length(),predictionClamped=requested.Length()>50f,eyeHeightCoverageMarginX=halfWidth-Mathf.Abs(error.X),eyeHeightCoverageMarginZ=halfWidth-Mathf.Abs(error.Z)});var folder=System.IO.Path.GetDirectoryName(audit);if(!string.IsNullOrEmpty(folder))System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllText(audit,System.Text.Json.JsonSerializer.Serialize(new{predict,offsetLimit=50,halfWidth,particleMotionUnchanged=true,samples,scope="Measured observer motion with constant-velocity/mean-fall-time prediction. Margins are analytical footprint estimates, not GPU particle counts or validated screen coverage"}));}
 }
 public override void _Process(double delta){if(IsInstanceValid(observer))Follow((float)delta);}
}
}
