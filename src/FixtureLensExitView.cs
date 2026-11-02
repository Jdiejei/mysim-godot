using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
public partial class FixtureLensExitView : Node3D {
 public List<Node3D> Layers;public int TrackIndex;public string Destination;bool started;
 public override void _PhysicsProcess(double delta){if(started)return;started=true;Capture();}
 async void Capture(){try{
  var views=FixtureLensExitDiagnostic.Write(Layers,TrackIndex,Destination+".json",this);if(views.Count==0)throw new Exception("No exact fixture head views");
  foreach(var sibling in GetParent().GetChildren())if(sibling is Node3D original&&original!=this)original.Visible=false;
  var environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new Color(.1f,.12f,.16f),AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=Colors.White,AmbientLightEnergy=.65f};
  AddChild(new DirectionalLight3D{RotationDegrees=new Vector3(-40,-35,0),LightEnergy=1.4f});var camera=new Camera3D{Current=true,Projection=Camera3D.ProjectionType.Orthogonal,Size=11,Near=.05f,Far=100,Environment=environment};AddChild(camera);
  for(int i=0;i<views.Count;i++){foreach(var view in views)view.Visible=false;views[i].Visible=true;Aabb bounds=default;bool have=false;foreach(var child in views[i].FindChildren("*","MeshInstance3D",true,false)){var mesh=(MeshInstance3D)child;var box=mesh.Transform*mesh.GetAabb();bounds=have?bounds.Merge(box):box;have=true;}if(!have)throw new Exception("Empty fixture head geometry");var centre=bounds.GetCenter();camera.Size=Mathf.Max(3,Mathf.Max(bounds.Size.X,Mathf.Max(bounds.Size.Y,bounds.Size.Z))*1.7f);foreach(int side in new[]{-1,1}){camera.Position=centre+new Vector3(7,side*5,7);camera.LookAt(centre);for(int frame=0;frame<4;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);using var image=GetViewport().GetTexture().GetImage();if(image.SavePng(Destination+$"-head-{i}-side-{side}.png")!=Error.Ok)throw new Exception("Fixture head capture failed");}}
  GD.Print("PASS actual fixture head UV and exit-ray diagnostic captures; no lens accepted");CleanExit.Request(this);
 }catch(Exception error){GD.PushError(error.ToString());CleanExit.Request(this,1);}}
}
}
