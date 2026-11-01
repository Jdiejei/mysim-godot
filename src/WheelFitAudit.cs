using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Read-only fit evidence: actual wing triangle samples in each steered wheel frame.
// A detected penetration is actionable; no detected sample is NOT a clearance certificate.
public static class WheelFitAudit {
 public static void Write(Node3D car,Node3D[] wheels,int index,string path){
  var rows=new List<object>();
  foreach(var wheel in wheels){
   float radius=wheel.GetMeta("radius").AsSingle();var spin=wheel.GetNode<Node3D>("Spin");Aabb? tyre=null;
   foreach(var child in spin.GetChildren())if(child is MeshInstance3D part&&part.Visible&&part.Mesh!=null){string n=part.Name.ToString().ToUpperInvariant();if(!n.Contains("TYRE")&&!n.Contains("TIRE"))continue;var box=part.Transform*part.GetAabb();tyre=tyre.HasValue?tyre.Value.Merge(box):box;}
   if(!tyre.HasValue)throw new InvalidOperationException("Wheel fit audit has no visible tyre");
   var bounds=tyre.Value;var centre=bounds.GetCenter();float halfWidth=bounds.Size.X*.5f;var parts=new List<object>();
   foreach(var child in car.FindChildren("*","MeshInstance3D",true,false)){
    var mesh=(MeshInstance3D)child;string name=mesh.Name.ToString().ToUpperInvariant();if(!mesh.IsVisibleInTree()||mesh.Mesh==null||!name.Contains("WING"))continue;
    var transform=wheel.GlobalTransform.AffineInverse()*mesh.GlobalTransform;int samples=0,inside=0;float deepest=0;
    void Probe(Vector3 p){p-=centre;samples++;float depth=Mathf.Min(halfWidth-Mathf.Abs(p.X),radius-Mathf.Sqrt(p.Y*p.Y+p.Z*p.Z));if(depth>.001f){inside++;deepest=Mathf.Max(deepest,depth);}}
    for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
     using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var indices=arrays[(int)Mesh.ArrayType.Index].VariantType==Variant.Type.Nil?Array.Empty<int>():arrays[(int)Mesh.ArrayType.Index].AsInt32Array();int count=indices.Length>0?indices.Length:vertices.Length;
     for(int t=0;t+2<count;t+=3){var a=transform*vertices[indices.Length>0?indices[t]:t];var b=transform*vertices[indices.Length>0?indices[t+1]:t+1];var c=transform*vertices[indices.Length>0?indices[t+2]:t+2];for(int u=0;u<=4;u++)for(int v=0;v<=4-u;v++)Probe(a+(b-a)*(u/4f)+(c-a)*(v/4f));}
    }
    if(inside>0)parts.Add(new {part=mesh.Name.ToString(),samples,inside,deepest});
   }
   rows.Add(new {wheel=wheel.Name.ToString(),hub=new[]{wheel.Position.X,wheel.Position.Y,wheel.Position.Z},yaw=wheel.Rotation.Y,radius,width=2*halfWidth,tyreCentreOffset=new[]{centre.X,centre.Y,centre.Z},wingPenetrations=parts});
  }
  System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(new {car=index,scope="Actual triangle samples against measured cylindrical tyre envelope; positive detections only, not exhaustive triangle collision certification",rows},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));GD.Print("PASS wheel fit evidence");
 }
}
}
