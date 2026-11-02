using Godot;
using MySimulator.Rebuild;
using System;
namespace MySim {
public partial class Main {
 async void CheckResultsLayout(){
  try{
   world=new Node3D();AddChild(world);var race=new Race(0,3,0);var player=race.Add("YOU",true,Compound.Medium);
   player.finished=true;player.completedLaps=3;player.finishTime=240;player.bestLap=78;
   for(int i=0;i<19;i++){var rival=race.Add("RIVAL "+i,false,Compound.Medium);rival.finished=true;rival.finishTime=241+i;}
   drive=new DriveController{Race=race,Player=player};var fixtureDriver=drive;var hud=new Hud{Race=race,Player=player};drive.Hud=hud;world.AddChild(hud);var touch=new TouchControls{Drive=drive};world.AddChild(touch);
   ShowResults();for(int i=0;i<5;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
   if(hud.Visible||touch.Visible||TouchControls.Active)throw new Exception("Driving overlay remains active over results");
   Button home=null,again=null;ScrollContainer scroll=null;
   foreach(var node in world.FindChildren("*","",true,false)){if(node is Button b){if(b.Text=="MAIN MENU")home=b;if(b.Text=="RACE AGAIN")again=b;}if(node is ScrollContainer s)scroll=s;}
   if(home==null||again==null||scroll==null)throw new Exception("Missing results actions/scroll");
   var screen=GetViewport().GetVisibleRect();foreach(var button in new[]{home,again})if(!screen.Encloses(button.GetGlobalRect()))throw new Exception("Results action outside viewport");
   if(home.GetGlobalRect().Intersects(again.GetGlobalRect()))throw new Exception("Results actions overlap");
   if(scroll.GetGlobalRect().Intersects(home.GetGlobalRect())||scroll.GetGlobalRect().Intersects(again.GetGlobalRect()))throw new Exception("Results content overlaps actions");
   scroll.ScrollVertical=10000;await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);if(scroll.ScrollVertical<=0)throw new Exception("20-car standings cannot scroll");
   if(DisplayServer.GetName()!="headless"){await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT"));}
   home.EmitSignal(BaseButton.SignalName.Pressed);for(int i=0;i<3;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
   if(resultsShown||drive!=null||ui==null)throw new Exception("Results main-menu action failed");
   fixtureDriver.Free();GD.Print("PASS actual results layout, scroll, overlay isolation and main-menu action; fixture race data, not driven race");CleanExit.Request(this);
  }catch(Exception error){GD.PushError(error.ToString());CleanExit.Request(this,1);}
 }
}
}
