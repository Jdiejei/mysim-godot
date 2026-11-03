using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Unaccepted W10/SF90 visual steering rig candidate. Derive the kingpin from intact source wishbone
// outboard vertex clusters; never alter source buffers/UVs or native contact hubs/radii.
public static class SourceKingpinVisualCandidate {
 internal static Vector3 Joint(Node3D car,string role){
  var inverse=car.GlobalTransform.AffineInverse();var points=new List<Vector3>();float outer=0;
  foreach(var child in car.FindChildren("*","MeshInstance3D",true,false)){var mesh=(MeshInstance3D)child;if(!mesh.IsVisibleInTree()||mesh.Mesh==null||!mesh.Name.ToString().StartsWith(role,StringComparison.Ordinal))continue;var transform=inverse*mesh.GlobalTransform;for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){using var arrays=mesh.Mesh.SurfaceGetArrays(surface);foreach(var vertex in arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array()){var point=transform*vertex;points.Add(point);outer=Mathf.Max(outer,Mathf.Abs(point.X));}}}
  if(points.Count==0||outer<.2f)throw new InvalidOperationException("Source kingpin joint role missing:"+role);var joint=Vector3.Zero;int samples=0;foreach(var point in points)if(Mathf.Abs(point.X)>=outer-.015f){joint+=point;samples++;}return joint/samples;
 }
 public static void ReviewJoints(Node3D car,Node3D[] wheels,int index){
  if(System.Environment.GetEnvironmentVariable("MYSIM_SOURCE_JOINT_REVIEW")!="1")return;
  if(index!=16)throw new Exception("Joint review requires Formula16");
  foreach(var node in car.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)node;string name=mesh.Name.ToString();bool lower=name.StartsWith("LOD_A_STRUT_LOWER_FRONT_LEFT"),upper=name.StartsWith("LOD_A_STRUT_UPPER_FRONT_LEFT");
   if(!lower&&!upper&&!wheels[0].IsAncestorOf(mesh)){mesh.Visible=false;continue;}
   if(lower||upper)mesh.MaterialOverride=new StandardMaterial3D{AlbedoColor=lower?new Color(.1f,.7f,.8f):new Color(.9f,.6f,.1f),Roughness=.7f,CullMode=BaseMaterial3D.CullModeEnum.Disabled};
   else if(mesh.Name=="RebuiltRubberTyre")mesh.MaterialOverride=new StandardMaterial3D{AlbedoColor=new Color(.2f,.25f,.3f,.18f),Transparency=BaseMaterial3D.TransparencyEnum.Alpha,Roughness=.8f,CullMode=BaseMaterial3D.CullModeEnum.Disabled};
  }
  foreach(bool upper in new[]{false,true}){
   var point=Joint(car,"LOD_A_STRUT_"+(upper?"UPPER":"LOWER")+"_FRONT_LEFT");
   car.AddChild(new MeshInstance3D{Name=upper?"ProposedUpperJoint":"ProposedLowerJoint",Position=point,Mesh=new SphereMesh{Radius=.018f,Height=.036f},MaterialOverride=new StandardMaterial3D{AlbedoColor=upper?new Color(1,.2f,.1f):new Color(.1f,1,.2f),ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded}});
  }
  GD.Print("PASS isolated intact source wishbones and proposed joints; upperarm gold/redmarker, lowerarm cyan/greenmarker; not accepted kinematics");
 }
 public static void Apply(Node3D pivot,int wheel,float steering,float spinAngle){
  if(System.Environment.GetEnvironmentVariable("MYSIM_SOURCE_KINGPIN_VISUAL")!="1"||wheel>=2)return;
  var car=pivot.GetParent<Node3D>();if(car.Name.ToString()!="2019_f1_mercedes_w10"&&car.Name.ToString()!="2019_f1_ferrari_sf90")return;
  if(!pivot.HasMeta("source_kingpin_lower")){
   if(System.Environment.GetEnvironmentVariable("MYSIM_SOURCE_AXLE_FIT")!="1")throw new InvalidOperationException("Source kingpin visual candidate requires measured source axle candidate");
   string side=wheel==0?"LEFT":"RIGHT";Vector3 lower=Joint(car,"LOD_A_STRUT_LOWER_FRONT_"+side),upper=Joint(car,"LOD_A_STRUT_UPPER_FRONT_"+side);var axis=upper-lower;float length=axis.Length();if(length<.05f||length>.6f||axis.Y/length<.5f)throw new InvalidOperationException("Source kingpin joint clusters do not establish valid steering axis");
   var hub=pivot.Position;float lateral=Mathf.Abs(hub.X)-Mathf.Abs((upper.X+lower.X)*.5f);if(lateral<.02f||lateral>.4f||Mathf.Abs(hub.Z-(lower.Z+upper.Z)*.5f)>.35f)throw new InvalidOperationException("Source kingpin is inconsistent with fitted source wheel hub");
   pivot.SetMeta("source_kingpin_lower",lower);pivot.SetMeta("source_kingpin_upper",upper);pivot.SetMeta("source_kingpin_axis",axis/length);pivot.SetMeta("source_kingpin_rest_hub",hub);
   GD.Print("SOURCE_KINGPIN_VISUAL car=",car.Name," wheel=",wheel," lower=",lower," upper=",upper," jointClusterBand=.015m; native physics unchanged");
  }
  var rest=pivot.GetMeta("source_kingpin_rest_hub").AsVector3();var lowerJoint=pivot.GetMeta("source_kingpin_lower").AsVector3();var kingpin=pivot.GetMeta("source_kingpin_axis").AsVector3();float baseYaw=pivot.GetMeta("steer_base_yaw").AsSingle();
  var turn=new Basis(kingpin,steering);var desiredBasis=turn*new Basis(Vector3.Up,baseYaw);var desiredOffset=lowerJoint+turn*(rest-lowerJoint)-rest;var inverseCurrent=pivot.Basis.Inverse();var correction=new Transform3D(inverseCurrent*desiredBasis,inverseCurrent*desiredOffset);
  pivot.GetNode<Node3D>("Spin").Transform=correction*new Transform3D(new Basis(Vector3.Right,spinAngle),Vector3.Zero);pivot.GetNode<Node3D>("BrakeAssembly").Transform=correction;
 }
}
}
