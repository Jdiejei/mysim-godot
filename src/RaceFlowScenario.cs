using Godot;
using System;
namespace MySim {
public partial class RaceFlowScenario : Node {
 public Func<DriveController> GetDrive; public Func<bool> ResultsVisible,MenuVisible;public Action Restart,Menu;
 int stage;DriveController first;double settle;
 public override void _Process(double delta){
  var d=GetDrive();
  if(stage==0&&d?.Race!=null){first=d;stage=1;}
  if(stage==1&&ResultsVisible()){
   settle+=delta;if(settle<.6)return;
   if(!first.Player.finished||first.Player.bestLap<=0){Fail("results without a valid completed lap");return;}
   GD.Print("PASS race to results lap=",first.Player.bestLap," wallHits=",first.WallHits," damage=",first.Player.damage);
   GetViewport().GetTexture().GetImage().SavePng("outputs/repair-20261029/results.png");stage=2;Restart();return;
  }
  if(stage==2&&d?.Race!=null&&d!=first){
   if(ResultsVisible()||d.Player.completedLaps!=0){Fail("restart kept old race state");return;}GD.Print("PASS results to fresh restart");stage=3;Menu();return;
  }
  if(stage==3&&MenuVisible()){GD.Print("PASS restart to main menu");stage=4;GetTree().Quit();}
 }
 void Fail(string reason){GD.PrintErr("FAIL race flow ",reason);stage=4;GetTree().Quit(1);}
}
}
