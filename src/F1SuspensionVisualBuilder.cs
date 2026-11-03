using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MySim {
// Explicit replacement asset candidate. Source wishbones stay intact and hidden;
// the new arm endpoints are solved from source body anchors and authored upright.
public static class F1SuspensionVisualBuilder {
 static Vector3[] Anchors(Node3D car,string role){
  var points=new List<Vector3>();float inner=float.PositiveInfinity;var inverse=car.GlobalTransform.AffineInverse();
  foreach(var node in car.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)node;if(!mesh.IsVisibleInTree()||mesh.Mesh==null||!mesh.Name.ToString().StartsWith(role,StringComparison.Ordinal))continue;
   var transform=inverse*mesh.GlobalTransform;for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){using var arrays=mesh.Mesh.SurfaceGetArrays(surface);foreach(var vertex in arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array()){var point=transform*vertex;points.Add(point);inner=Mathf.Min(inner,Mathf.Abs(point.X));}}
  }
  var candidates=points.FindAll(v=>Mathf.Abs(v.X)<=inner+.015f);float low=float.PositiveInfinity,high=float.NegativeInfinity;foreach(var v in candidates){low=Mathf.Min(low,v.Z);high=Mathf.Max(high,v.Z);}
  if(candidates.Count<2||high-low<.08f)throw new Exception("Source wishbone inboard anchors unresolved: "+role);
  var front=Vector3.Zero;var rear=Vector3.Zero;int fronts=0,rears=0;foreach(var v in candidates){if(v.Z<=(low+high)*.5f){rear+=v;rears++;}else{front+=v;fronts++;}}
  if(fronts==0||rears==0)throw new Exception("Wishbone source anchor clusters empty");return new[]{rear/rears,front/fronts};
 }
 static ArrayMesh Arm(Vector3 a,Vector3 b,float width,float thickness){
  var axis=(b-a).Normalized();var side=axis.Cross(Vector3.Up).Normalized();if(side.LengthSquared()<.5f)throw new Exception("Invalid visual wishbone span");var up=side.Cross(axis).Normalized();using var tool=new SurfaceTool();tool.Begin(Mesh.PrimitiveType.Triangles);
  void Vertex(int end,int ring){float angle=Mathf.Tau*ring/24;var normal=(side*Mathf.Cos(angle)/width+up*Mathf.Sin(angle)/thickness).Normalized();var point=(end==0?a:b)+side*(width*Mathf.Cos(angle))+up*(thickness*Mathf.Sin(angle));tool.SetNormal(normal);tool.SetUV(new Vector2(end,ring/24f));tool.AddVertex(point);}
  for(int j=0;j<24;j++){Vertex(0,j);Vertex(1,j);Vertex(0,j+1);Vertex(0,j+1);Vertex(1,j);Vertex(1,j+1);}tool.Index();return tool.Commit();
 }
 public static void Apply(Node3D car,Node3D[] wheels,int index){
  if(System.Environment.GetEnvironmentVariable("MYSIM_REBUILT_F1_SUSPENSION")!="1")return;
  if(index!=16||System.Environment.GetEnvironmentVariable("MYSIM_SOURCE_AXLE_FIT")!="1")throw new Exception("Rebuilt suspension requires measured Formula16 axle fit");
  if(car.HasMeta("rebuilt_f1_suspension"))return;
  using var document=JsonDocument.Parse(FileAccess.GetFileAsString("res://data/f1_suspension_visual_profiles.json"));var cfg=document.RootElement.GetProperty("cars").GetProperty(index.ToString());float caster=cfg.GetProperty("casterDegrees").GetSingle(),width=cfg.GetProperty("armHalfWidth").GetSingle(),thickness=cfg.GetProperty("armHalfThickness").GetSingle(),jointRadius=cfg.GetProperty("jointRadius").GetSingle();
  if(!float.IsFinite(caster)||Mathf.Abs(caster)>15||width<.005f||width>.04f||thickness<.003f||thickness>.015f||jointRadius<.005f||jointRadius>.03f)throw new Exception("Invalid authored suspension visual profile");
  var root=new Node3D{Name="RebuiltFrontDoubleWishbones"};car.AddChild(root);var carbon=new StandardMaterial3D{AlbedoColor=new Color(.035f,.04f,.045f),Roughness=.48f,Metallic=.1f,CullMode=BaseMaterial3D.CullModeEnum.Disabled};var metal=new StandardMaterial3D{AlbedoColor=new Color(.2f,.22f,.24f),Roughness=.35f,Metallic=.8f};
  for(int wheel=0;wheel<2;wheel++){
   string side=wheel==0?"LEFT":"RIGHT",lowerRole="LOD_A_STRUT_LOWER_FRONT_"+side,upperRole="LOD_A_STRUT_UPPER_FRONT_"+side;
   var lower=SourceKingpinVisualCandidate.Joint(car,lowerRole);var sourceUpper=SourceKingpinVisualCandidate.Joint(car,upperRole);var upper=new Vector3(sourceUpper.X,sourceUpper.Y,lower.Z+Mathf.Tan(Mathf.DegToRad(caster))*(sourceUpper.Y-lower.Y));var axis=(upper-lower).Normalized();if(axis.Y<.9f)throw new Exception("Authored upright inclination is outside diagnostic design bound");
   foreach(bool high in new[]{false,true}){string role=high?upperRole:lowerRole;var joint=high?upper:lower;var anchors=Anchors(car,role);
    for(int arm=0;arm<2;arm++)root.AddChild(new MeshInstance3D{Name=side+(high?"Upper":"Lower")+"Arm"+arm,Mesh=Arm(anchors[arm],joint,width,thickness),MaterialOverride=carbon});
    root.AddChild(new MeshInstance3D{Name=side+(high?"Upper":"Lower")+"BallJoint",Position=joint,Mesh=new SphereMesh{Radius=jointRadius,Height=jointRadius*2},MaterialOverride=metal});
    foreach(var node in car.FindChildren("*","MeshInstance3D",true,false)){var mesh=(MeshInstance3D)node;if(mesh.Name.ToString().StartsWith(role,StringComparison.Ordinal)){mesh.Visible=false;mesh.SetMeta("replaced_wishbone_source",true);}}
   }
   wheels[wheel].SetMeta("source_kingpin_lower",lower);wheels[wheel].SetMeta("source_kingpin_upper",upper);wheels[wheel].SetMeta("source_kingpin_axis",axis);wheels[wheel].SetMeta("source_kingpin_rest_hub",wheels[wheel].Position);wheels[wheel].SetMeta("kingpin_recipe","Authored caster6deg, source joint heights/lateral positions and body anchors; native contact rig unverified");
  }
  car.SetMeta("rebuilt_f1_suspension",true);GD.Print("REBUILT_F1_SUSPENSION car=",index," authoredCaster=",caster," source anchors retained; original meshes/UV unchanged");
 }
}
}
