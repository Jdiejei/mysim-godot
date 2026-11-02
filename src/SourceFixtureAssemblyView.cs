using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Encrypted source-assembly evidence only. Copies retain source geometry/UV/materials.
public partial class SourceFixtureAssemblyView : Node3D {
 public List<Node3D> Layers;public int TrackIndex;public string Destination;
 public override async void _Ready(){try{
  var assemblies=SourceNightFixtureAudit.BuildIllustrativeAssemblies(TrackIndex,this);if(assemblies.Count==0)throw new Exception("No grouped source fixture assemblies to render");
  foreach(var layer in Layers)layer.Visible=false;
  AddChild(new WorldEnvironment{Environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new Color(.10f,.12f,.16f),AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=Colors.White,AmbientLightEnergy=.65f}});
  AddChild(new DirectionalLight3D{RotationDegrees=new Vector3(-40,-35,0),LightEnergy=1.4f});
  var camera=new Camera3D{Current=true,Projection=Camera3D.ProjectionType.Orthogonal,Near=.05f,Far=300};AddChild(camera);
  var records=new List<object>();string folder=System.IO.Path.GetDirectoryName(Destination);System.IO.Directory.CreateDirectory(folder);
  for(int index=0;index<assemblies.Count;index++){
   foreach(var item in assemblies)item.Visible=false;var assembly=assemblies[index];assembly.Visible=true;
   Aabb bounds=default;bool have=false;foreach(var node in assembly.FindChildren("*","MeshInstance3D",true,false)){var mesh=(MeshInstance3D)node;var box=mesh.Transform*mesh.GetAabb();bounds=have?bounds.Merge(box):box;have=true;}
   if(!have)throw new Exception("Empty source assembly");float height=bounds.Size.Y;
   for(int detail=0;detail<2;detail++){
    var target=detail==0?bounds.GetCenter():new Vector3(bounds.GetCenter().X,bounds.End.Y-2,bounds.GetCenter().Z);camera.Size=detail==0?Mathf.Max(height*1.2f,10):10;camera.Position=target+new Vector3(1,.35f,1).Normalized()*Mathf.Max(height*1.8f,20);camera.LookAt(target);
    for(int frame=0;frame<8;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
    string image=System.IO.Path.Combine(folder,$"fixture-track-{TrackIndex}-assembly-{index}-view-{detail}.png");using var pixels=GetViewport().GetTexture().GetImage();if(pixels.SavePng(image)!=Error.Ok)throw new Exception("Fixture image save failed");records.Add(new{index,detail,image,source=assembly.GetMeta("source_shaft").AsString(),component=assembly.GetMeta("source_component").AsInt32(),validated=false});
   }
  }
  System.IO.File.WriteAllText(Destination,System.Text.Json.JsonSerializer.Serialize(new{track=TrackIndex,captures=records,scope="Illustrative grouped source geometry; no lamp placement, source assembly membership or in-game lighting acceptance"}));GD.Print("COMPLETE labelled source fixture assembly captures");CleanExit.Request(this);
 }catch(Exception error){GD.PushError(error.ToString());CleanExit.Request(this,1);}}
}
}
