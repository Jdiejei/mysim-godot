using Godot;
using System;
using MySimulator.Rebuild;
namespace MySim {
public partial class GridLightsScenario : Node {
 public override async void _Ready(){Hud hud=null;try{
  var race=new Race(0,3,0);hud=new Hud{Race=race};var root=new Control{AnchorRight=1,AnchorBottom=1};AddChild(root);var lights=new RaceStartLights{Hud=hud};root.AddChild(lights);
  void Check(bool yes,string reason){if(!yes)throw new Exception(reason);}
  async System.Threading.Tasks.Task Frame(){await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
  await Frame();Check(lights.Visible&&lights.LitCount==1,"Initial red light absent");
  float frozen=race.countdown;race.paused=true;race.Advance(.25f);Check(race.countdown==frozen&&race.clock==0,"Paused countdown advanced");race.paused=false;
  for(int stage=1;stage<=5;stage++){
   if(stage>1)for(int step=0;step<5;step++)race.Advance(.21f);
   await Frame();Check(!race.started&&race.clock==0,"Race started before final red light");Check(lights.LitCount==stage,"Red light sequence skipped a stage");Check(lights.GetGlobalRect().Size.X>=250,"Start lights collapsed");
  }
  string shot=System.Environment.GetEnvironmentVariable("MYSIM_SHOT");if(shot!=null){using var image=GetViewport().GetTexture().GetImage();image.SavePng(shot);}
  for(int step=0;step<5;step++)race.Advance(.25f);await Frame();Check(race.started&&race.clock>0&&!lights.Visible&&lights.LitCount==0,"Lights did not extinguish with race start");
  GD.Print("PASS actual five grid lights, pause and lights-out; UI fixture only, native grid motion separate");CleanExit.Request(this);
 }catch(Exception error){GD.PushError(error.ToString());CleanExit.Request(this,1);}finally{hud?.Free();}}
}
}
