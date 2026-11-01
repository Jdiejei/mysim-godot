using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
public partial class Main {
 async void SourceTyreView(string asset){
  if(asset!="WetTyre"&&asset!="IntermediateTyre"&&asset!="DeepWetTyre")throw new InvalidOperationException("Unknown recovered tyre source");
  string path="res://assets/Resources/RealRacing/"+asset+".fbx";
  var source=GD.Load<PackedScene>(path).Instantiate<Node3D>();source.Scale=Vector3.One*100f;AddChild(source);
  Aabb bounds=default;bool first=true;var rows=new List<object>();
  foreach(var child in source.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)child;if(mesh.Mesh==null||!mesh.IsVisibleInTree())continue;var box=mesh.GlobalTransform*mesh.GetAabb();bounds=first?box:bounds.Merge(box);first=false;
   rows.Add(new {name=mesh.Name.ToString(),centre=new[]{box.GetCenter().X,box.GetCenter().Y,box.GetCenter().Z},size=new[]{box.Size.X,box.Size.Y,box.Size.Z},surfaces=mesh.Mesh.GetSurfaceCount()});
   // Geometry comparison neutral rubber only; original resources remain unchanged.
   if(mesh.Name.ToString().Contains("TyreBody"))mesh.MaterialOverride=new StandardMaterial3D{AlbedoColor=new Color(.18f,.19f,.20f),Roughness=.85f,MetallicSpecular=.2f,CullMode=BaseMaterial3D.CullModeEnum.Disabled};
  }
  if(first)throw new InvalidOperationException("Recovered tyre source has no visible mesh");
  source.Position-=new Vector3(bounds.GetCenter().X,bounds.Position.Y,bounds.GetCenter().Z);var target=new Vector3(0,bounds.Size.Y*.5f,0);
  AddChild(new MeshInstance3D{Mesh=new PlaneMesh{Size=new Vector2(8,8)},MaterialOverride=new StandardMaterial3D{AlbedoColor=new Color(.3f,.3f,.32f)}});
  int angle=int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_TYRE_REVIEW_ANGLE")??"0");var offset=angle switch{0=>new Vector3(2,.2f,.2f),1=>new Vector3(1.7f,.9f,1.5f),_=>new Vector3(-1.7f,.7f,-1.5f)};
  var camera=new Camera3D{Current=true,Fov=38,Position=target+offset};AddChild(camera);camera.LookAt(target);FitCarView(camera,source,target,"recovered tyre");
  if(System.Environment.GetEnvironmentVariable("MYSIM_SOURCE_TYRE_REPORT") is string report)System.IO.File.WriteAllText(report,System.Text.Json.JsonSerializer.Serialize(new {asset,path,prefabRootScale=100,scope="Recovered geometry, neutral tyre-body material for comparison only; original UVs/meshes unmodified; no integration or compound-physics acceptance",rows}));
  for(int i=0;i<25;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT"));GD.Print("PASS recovered tyre source capture");GetTree().Quit();
 }
}
}
