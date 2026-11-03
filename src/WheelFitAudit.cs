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
   rows.Add(new {wheel=wheel.Name.ToString(),hub=new[]{wheel.Position.X,wheel.Position.Y,wheel.Position.Z},visualHubInCarFrame=new[]{(car.GlobalTransform.AffineInverse()*spin.GlobalPosition).X,(car.GlobalTransform.AffineInverse()*spin.GlobalPosition).Y,(car.GlobalTransform.AffineInverse()*spin.GlobalPosition).Z},kingpinRecipe=wheel.HasMeta("kingpin_recipe")?wheel.GetMeta("kingpin_recipe").AsString():"Source extrema-cluster candidate; joint identity unproven",sourceKingpin=wheel.HasMeta("source_kingpin_axis")?new{lower=wheel.GetMeta("source_kingpin_lower").AsVector3().ToString(),upper=wheel.GetMeta("source_kingpin_upper").AsVector3().ToString(),axis=wheel.GetMeta("source_kingpin_axis").AsVector3().ToString(),scope="Visual-only candidate steering axis; native tyre contact hub/radius unchanged"}:null,yaw=wheel.Rotation.Y,radius,width=2*halfWidth,tyreCentreOffset=new[]{centre.X,centre.Y,centre.Z},dryRubberPenetrationSamples=parts});
  }
  System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(new {car=index,scope="Body and fixed hub-mounted source-component triangle samples against actual generated dry tyre cross-section, excluding rotating wheel meshes, hub void and rounded-sidewall gaps; positive samples only, not exhaustive triangle or swept-motion certification",rows},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));GD.Print("PASS wheel fit evidence");
 }
}
}
