using Godot;
using System;
namespace MySim {
// Actual empty-fuel chassis -> retirement -> results -> fresh restart -> menu.
public partial class RaceRetirementScenario : Node {
 public Func<DriveController> GetDrive;public Func<bool> ResultsVisible,MenuVisible;public Action Restart,Menu;int stage;double elapsed;DriveController first;
 public override void _Process(double delta){try{elapsed+=delta;if(elapsed>40)throw new Exception("Empty-fuel race flow timed out");var drive=GetDrive();
  if(stage==0&&drive is NativeRaceController native&&native.Chassis!=null){if(drive.Player.fuel!=0)throw new Exception("Retirement fixture did not start with an empty tank");first=drive;stage=1;}
  if(stage==1&&ResultsVisible()){if(!first.Player.retired||first.Player.finished||first.Player.completedLaps!=0||first.Player.retirementReason!="OUT OF FUEL")throw new Exception("Retirement results invented a finish or lost its reason");GD.Print("PASS actual empty-fuel native retirement and DNF results");stage=2;Restart();return;}
  if(stage==2&&drive!=null&&drive!=first){if(drive.Player.retired||drive.Player.finished||ResultsVisible())throw new Exception("Restart retained retired race state");GD.Print("PASS retirement to fresh restart");stage=3;Menu();return;}
  if(stage==3&&MenuVisible()){GD.Print("PASS retirement restart to main menu");stage=4;CleanExit.Request(this);}
 }catch(Exception e){SetProcess(false);GD.PushError(e.ToString());CleanExit.Request(this,1);}}
}
}
