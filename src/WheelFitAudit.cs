using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Read-only fit evidence: actual body triangle samples in each steered wheel frame.
// A detected penetration is actionable; no detected sample is NOT a clearance certificate.
public static class WheelFitAudit {
 static List<Vector2> DrySection(Node3D spin){
  var mesh=spin.GetNodeOrNull<MeshInstance3D>("RebuiltRubberTyre");if(mesh==null||mesh.GetMeta("wet").AsBool())throw new InvalidOperationException("Exact wheel fit requires generated dry tyre geometry");
  var ordered=new SortedDictionary<float,Vector2>();using var arrays=mesh.Mesh.SurfaceGetArrays(0);var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var uv=arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array();
  for(int i=0;i<vertices.Length;i++)if(Mathf.Abs(uv[i].X)<.000001f){var point=mesh.Transform*vertices[i];ordered[uv[i].Y]=new Vector2(point.X,Mathf.Sqrt(point.Y*point.Y+point.Z*point.Z));}
  if(ordered.Count<8)throw new InvalidOperationException("Dry tyre cross-section extraction failed");return new List<Vector2>(ordered.Values);
 }
 static float MaterialDepth(Vector3 point,List<Vector2> section){
  var p=new Vector2(point.X,Mathf.Sqrt(point.Y*point.Y+point.Z*point.Z));bool inside=false;float distance=float.PositiveInfinity;
  for(int i=0,j=section.Count-1;i<section.Count;j=i++){
   var a=section[i];var b=section[j];if((a.Y>p.Y)!=(b.Y>p.Y)&&p.X<(b.X-a.X)*(p.Y-a.Y)/(b.Y-a.Y)+a.X)inside=!inside;
   var edge=b-a;float t=Mathf.Clamp((p-a).Dot(edge)/Mathf.Max(edge.LengthSquared(),.00000001f),0,1);distance=Mathf.Min(distance,p.DistanceTo(a+edge*t));
  }return inside?distance:-distance;
 }
 public static void Write(Node3D car,Node3D[] wheels,int index,string path){
  var rows=new List<object>();
  foreach(var wheel in wheels){
   float radius=wheel.GetMeta("radius").AsSingle();var spin=wheel.GetNode<Node3D>("Spin");Aabb? tyre=null;
   foreach(var child in spin.GetChildren())if(child is MeshInstance3D part&&part.Visible&&part.Mesh!=null){string n=part.Name.ToString().ToUpperInvariant();if(!n.Contains("TYRE")&&!n.Contains("TIRE"))continue;var box=part.Transform*part.GetAabb();tyre=tyre.HasValue?tyre.Value.Merge(box):box;}
   if(!tyre.HasValue)throw new InvalidOperationException("Wheel fit audit has no visible tyre");
   var bounds=tyre.Value;var centre=bounds.GetCenter();float halfWidth=bounds.Size.X*.5f;var parts=new List<object>();var section=DrySection(spin);
   foreach(var child in car.FindChildren("*","MeshInstance3D",true,false)){
    var mesh=(MeshInstance3D)child;string name=mesh.Name.ToString().ToUpperInvariant();bool rebuiltWishbone=mesh.GetParent()?.Name.ToString()=="RebuiltFrontDoubleWishbones";if(!mesh.IsVisibleInTree()||mesh.Mesh==null||(!name.StartsWith("LOD_A_")&&!rebuiltWishbone))continue;// Rubber/rim meshes rotate with Spin and are the reference envelope.
    // Fixed hub-mounted guards/calipers must still be tested: skipping the
    // entire hub silently omitted exactly the parts that can cut into tyres.
    bool wheelPart=false;foreach(var hub in wheels)if(hub.GetNode<Node3D>("Spin").IsAncestorOf(mesh)){wheelPart=true;break;}if(wheelPart)continue;
    var transform=spin.GlobalTransform.AffineInverse()*mesh.GlobalTransform;int samples=0,inside=0;float deepest=0;
    void Probe(Vector3 p){samples++;float depth=MaterialDepth(p,section);if(depth>.001f){inside++;deepest=Mathf.Max(deepest,depth);}}
    for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
     using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var indices=arrays[(int)Mesh.ArrayType.Index].VariantType==Variant.Type.Nil?Array.Empty<int>():arrays[(int)Mesh.ArrayType.Index].AsInt32Array();int count=indices.Length>0?indices.Length:vertices.Length;
     for(int t=0;t+2<count;t+=3){var a=transform*vertices[indices.Length>0?indices[t]:t];var b=transform*vertices[indices.Length>0?indices[t+1]:t+1];var c=transform*vertices[indices.Length>0?indices[t+2]:t+2];for(int u=0;u<=4;u++)for(int v=0;v<=4-u;v++)Probe(a+(b-a)*(u/4f)+(c-a)*(v/4f));}
    }
    if(inside>0)parts.Add(new {part=mesh.Name.ToString(),samples,inside,deepest});
   }
   float minimumWorldY=float.PositiveInfinity,maximumWorldY=float.NegativeInfinity;int rubberVertices=0;
   foreach(var child in spin.GetChildren())if(child is MeshInstance3D rubber&&rubber.IsVisibleInTree()&&rubber.Name=="RebuiltRubberTyre"){
    for(int surface=0;surface<rubber.Mesh.GetSurfaceCount();surface++){using var arrays=rubber.Mesh.SurfaceGetArrays(surface);foreach(var vertex in arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array()){var world=rubber.GlobalTransform*vertex;minimumWorldY=Mathf.Min(minimumWorldY,world.Y);maximumWorldY=Mathf.Max(maximumWorldY,world.Y);rubberVertices++;}}
   }
   if(rubberVertices==0||!float.IsFinite(minimumWorldY))throw new InvalidOperationException("Actual rubber support extrema missing");
   rows.Add(new {rubberSupport=new{minimumWorldY,maximumWorldY,rubberVertices,scope="Actual transformed rubber vertices relative to preview floorY0; not loaded gameplay contact"},wheel=wheel.Name.ToString(),hub=new[]{wheel.Position.X,wheel.Position.Y,wheel.Position.Z},visualHubInCarFrame=new[]{(car.GlobalTransform.AffineInverse()*spin.GlobalPosition).X,(car.GlobalTransform.AffineInverse()*spin.GlobalPosition).Y,(car.GlobalTransform.AffineInverse()*spin.GlobalPosition).Z},kingpinRecipe=wheel.HasMeta("kingpin_recipe")?wheel.GetMeta("kingpin_recipe").AsString():"Source extrema-cluster candidate; joint identity unproven",sourceKingpin=wheel.HasMeta("source_kingpin_axis")?new{lower=wheel.GetMeta("source_kingpin_lower").AsVector3().ToString(),upper=wheel.GetMeta("source_kingpin_upper").AsVector3().ToString(),axis=wheel.GetMeta("source_kingpin_axis").AsVector3().ToString(),scope="Visual-only candidate steering axis; native tyre contact hub/radius unchanged"}:null,yaw=wheel.Rotation.Y,radius,width=2*halfWidth,tyreCentreOffset=new[]{centre.X,centre.Y,centre.Z},dryRubberPenetrationSamples=parts});
  }
  object sweep=System.Environment.GetEnvironmentVariable("MYSIM_WHEEL_CLEARANCE_SWEEP")=="1"?Sweep(car,wheels,index):null;
  System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(new {car=index,scope="Body and fixed hub-mounted source-component triangle samples against actual generated dry tyre cross-section, excluding rotating wheel meshes, hub void and rounded-sidewall gaps; positive samples only, not exhaustive triangle or swept-motion certification",rows,sweep},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));GD.Print("PASS wheel fit evidence");
 }
 static float[] SweepValues(string name,string fallback,float limit){
  string text=System.Environment.GetEnvironmentVariable(name)??fallback;var parts=text.Split(',');if(parts.Length==0||parts.Length>9)throw new InvalidOperationException("Clearance sweep grid size invalid: "+name);
  var values=new float[parts.Length];for(int i=0;i<parts.Length;i++){if(!float.TryParse(parts[i],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out values[i])||!float.IsFinite(values[i])||Mathf.Abs(values[i])>limit)throw new InvalidOperationException("Clearance sweep value out of bound: "+name+"="+parts[i]);}return values;
 }
 // Stationary body/wing/brake sample cloud in car space, captured once and reused for every pose.
 // Same mesh filter and barycentric sampling as Write; rotating wheel meshes stay excluded.
 static System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string,System.Collections.Generic.List<Vector3>>> BodySampleCloud(Node3D car,Node3D[] wheels){
  var inverse=car.GlobalTransform.AffineInverse();var cloud=new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string,System.Collections.Generic.List<Vector3>>>();
  foreach(var node in car.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)node;string name=mesh.Name.ToString().ToUpperInvariant();bool rebuiltWishbone=mesh.GetParent()?.Name.ToString()=="RebuiltFrontDoubleWishbones";
   if(!mesh.IsVisibleInTree()||mesh.Mesh==null||(!name.StartsWith("LOD_A_")&&!rebuiltWishbone))continue;
   bool wheelPart=false;foreach(var hub in wheels)if(hub.GetNode<Node3D>("Spin").IsAncestorOf(mesh)){wheelPart=true;break;}if(wheelPart)continue;
   var toCar=inverse*mesh.GlobalTransform;var samples=new System.Collections.Generic.List<Vector3>();
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
    using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var indices=arrays[(int)Mesh.ArrayType.Index].VariantType==Variant.Type.Nil?Array.Empty<int>():arrays[(int)Mesh.ArrayType.Index].AsInt32Array();int count=indices.Length>0?indices.Length:vertices.Length;
    for(int t=0;t+2<count;t+=3){var a=toCar*vertices[indices.Length>0?indices[t]:t];var b=toCar*vertices[indices.Length>0?indices[t+1]:t+1];var c=toCar*vertices[indices.Length>0?indices[t+2]:t+2];for(int u=0;u<=4;u++)for(int v=0;v<=4-u;v++)samples.Add(a+(b-a)*(u/4f)+(c-a)*(v/4f));}
   }
   if(samples.Count>0)cloud.Add(new System.Collections.Generic.KeyValuePair<string,System.Collections.Generic.List<Vector3>>(mesh.Name.ToString(),samples));
  }
  return cloud;
 }
 // Opt-in swept-clearance fixture (MYSIM_WHEEL_CLEARANCE_SWEEP=1): every pose steers through the real
 // WheelAnimation path (identical to gameplay and to MYSIM_WHEEL_FIT_STEER) and translates the hubs
 // within the established evidence-travel bound, then probes the static body cloud against all four
 // dry tyre cross-sections. Reported extremes are findings, not a swept-clearance certificate; the
 // pre-sweep pose is restored and the restoration is verified so later audits see pristine state.
 static object Sweep(Node3D car,Node3D[] wheels,int index){
  float[] steers=SweepValues("MYSIM_WHEEL_CLEARANCE_STEERS","-28,-14,0,14,28",45f);
  float[] travels=SweepValues("MYSIM_WHEEL_CLEARANCE_TRAVELS","-0.08,-0.04,0.04,0.08",0.08f);
  var sections=new System.Collections.Generic.List<System.Collections.Generic.List<Vector2>>();var rest=new Vector3[wheels.Length];
  var snapshots=new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<Node3D,Transform3D>>();
  for(int i=0;i<wheels.Length;i++){rest[i]=wheels[i].Position;sections.Add(DrySection(wheels[i].GetNode<Node3D>("Spin")));snapshots.Add(new System.Collections.Generic.KeyValuePair<Node3D,Transform3D>(wheels[i],wheels[i].Transform));foreach(var child in wheels[i].GetChildren())if(child is Node3D node)snapshots.Add(new System.Collections.Generic.KeyValuePair<Node3D,Transform3D>(node,node.Transform));}
  var cloud=BodySampleCloud(car,wheels);if(cloud.Count==0)throw new InvalidOperationException("Clearance sweep found no stationary body geometry");var carTransform=car.GlobalTransform;var rows=new System.Collections.Generic.List<object>();var modes=new[]{"front_both","rear_both","front_roll","rear_roll"};int poses=0;
  var plan=new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string,float>>();foreach(float travel in travels)if(travel==0f)plan.Add(new System.Collections.Generic.KeyValuePair<string,float>("rest",0f));foreach(var mode in modes)foreach(float travel in travels)if(travel!=0f)plan.Add(new System.Collections.Generic.KeyValuePair<string,float>(mode,travel));
  try{
   foreach(float steer in steers)foreach(var step in plan){
    for(int i=0;i<wheels.Length;i++)wheels[i].Position=rest[i];
    WheelAnimation.Update(wheels,0,Mathf.DegToRad(steer));
    if(step.Key=="front_both"){wheels[0].Position+=Vector3.Up*step.Value;wheels[1].Position+=Vector3.Up*step.Value;}
    else if(step.Key=="rear_both"){wheels[2].Position+=Vector3.Up*step.Value;wheels[3].Position+=Vector3.Up*step.Value;}
    else if(step.Key=="front_roll"){wheels[0].Position+=Vector3.Up*step.Value;wheels[1].Position-=Vector3.Up*step.Value;}
    else if(step.Key=="rear_roll"){wheels[2].Position+=Vector3.Up*step.Value;wheels[3].Position-=Vector3.Up*step.Value;}
    var wheelRows=new System.Collections.Generic.List<object>();
    for(int w=0;w<wheels.Length;w++){
     var spin=wheels[w].GetNode<Node3D>("Spin");var toSpin=spin.GlobalTransform.AffineInverse();var section=sections[w];
     float worst=float.NegativeInfinity;var offenders=new System.Collections.Generic.List<object>();int samplesInspected=0;
     foreach(var part in cloud){float deepest=float.NegativeInfinity;foreach(var point in part.Value){float depth=MaterialDepth(toSpin*(carTransform*point),section);if(depth>deepest)deepest=depth;}samplesInspected+=part.Value.Count;if(deepest>0f)offenders.Add(new{part=part.Key,deepest});if(deepest>worst)worst=deepest;}
     wheelRows.Add(new{wheel=wheels[w].Name.ToString(),hub=new[]{wheels[w].Position.X,wheels[w].Position.Y,wheels[w].Position.Z},clearance=-worst,penetratingParts=offenders.Count,offenders,samplesInspected});
    }
    rows.Add(new{steerDegrees=steer,mode=step.Key,travel=step.Value,wheels=wheelRows});poses++;
   }
  }
  finally{
   for(int i=0;i<wheels.Length;i++)wheels[i].Position=rest[i];
   WheelAnimation.Update(wheels,0,0f);
   foreach(var snapshot in snapshots)if(!snapshot.Key.Transform.IsEqualApprox(snapshot.Value))snapshot.Key.Transform=snapshot.Value;
   foreach(var snapshot in snapshots)if(!snapshot.Key.Transform.IsEqualApprox(snapshot.Value))throw new InvalidOperationException("Clearance sweep failed to restore pose: "+snapshot.Key.Name);
  }
  GD.Print("WHEEL_CLEARANCE_SWEEP car=",index," poses=",poses," bodyParts=",cloud.Count," steers=",steers.Length," travelSteps=",plan.Count," pose restored; evidence only, not a clearance certificate");
  return new{car=index,steers,travels,modes,poses,bodyParts=cloud.Count,rows,scope="Steered and travelled clearance probe of the static body/wing/brake sample cloud against dry tyre cross-sections through the real steering path; positive penetrations are findings, not acceptance, and hub translation is an evidence pose, not a suspension solution"};
 }
}
}
