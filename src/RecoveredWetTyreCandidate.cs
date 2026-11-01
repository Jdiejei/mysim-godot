using Godot;
using System;
namespace MySim {
// Explicit visual comparison only. Native contacts/radii and original source resources are unchanged.
public static class RecoveredWetTyreCandidate {
 public static bool Apply(Node3D spin,MeshInstance3D generated,Compound compound){
  var candidate=spin.GetNodeOrNull<Node3D>("RecoveredWetTyreCandidate");bool enabled=System.Environment.GetEnvironmentVariable("MYSIM_RECOVERED_WET_TYRE")=="1"&&compound==Compound.Wet;
  if(enabled&&candidate==null){
   candidate=GD.Load<PackedScene>("res://assets/Resources/RealRacing/WetTyre.fbx").Instantiate<Node3D>();candidate.Name="RecoveredWetTyreCandidate";spin.AddChild(candidate);
   var inverse=candidate.GlobalTransform.AffineInverse();Aabb bounds=default;bool first=true;
   foreach(var node in candidate.FindChildren("*","MeshInstance3D",true,false)){var part=(MeshInstance3D)node;if(part.Mesh==null||!part.Name.ToString().Contains("TyreBody"))continue;var box=(inverse*part.GlobalTransform)*part.GetAabb();bounds=first?box:bounds.Merge(box);first=false;}
   if(first)throw new InvalidOperationException("Recovered wet tyre has no rubber body");
   float radius=generated.GetMeta("radius").AsSingle(),width=generated.GetMeta("width").AsSingle(),sourceRadius=Mathf.Max(bounds.Size.Y,bounds.Size.Z)*.5f;
   if(sourceRadius<.2f||sourceRadius>.5f||bounds.Size.X<.15f||bounds.Size.X>.5f)throw new InvalidOperationException("Recovered wet tyre import units invalid");
   var scale=new Vector3(width/bounds.Size.X,radius/sourceRadius,radius/sourceRadius);candidate.Transform=new Transform3D(Basis.FromScale(scale),-scale*bounds.GetCenter());
   foreach(var node in candidate.FindChildren("*","MeshInstance3D",true,false)){var part=(MeshInstance3D)node;bool rubber=part.Name.ToString().Contains("TyreBody");part.MaterialOverride=new StandardMaterial3D{AlbedoColor=rubber?new Color(.18f,.19f,.20f):TyreAppearance.Colour(Compound.Wet),Roughness=rubber?.88f:.85f,MetallicSpecular=.15f};}
   GD.Print("RECOVERED_WET_TYRE sourceRadius=",sourceRadius," targetRadius=",radius," sourceWidth=",bounds.Size.X," targetWidth=",width," visual-only fitted source geometry; native radius unchanged");
  }
  if(candidate!=null)candidate.Visible=enabled;if(generated!=null)generated.Visible=!enabled;return enabled;
 }
}
}
