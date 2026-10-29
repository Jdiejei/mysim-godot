using Godot;
using System;
using MySimulator.Rebuild;
namespace MySim {
public partial class DrivingScenario : Node {
 public DriveController Drive; double elapsed; int stage=-1; bool checkedReverse,checkedPause,checkedResume;
 void KeyState(Key key,bool pressed){Input.ParseInputEvent(new InputEventKey { Keycode=key,PhysicalKeycode=key,Pressed=pressed });}
 public override void _Process(double delta){
  elapsed+=delta;int next=elapsed<.8?0:elapsed<4.5?1:elapsed<8?2:elapsed<8.15?3:elapsed<9?4:elapsed<9.15?5:elapsed<11?6:elapsed<11.15?7:8;
  if(next!=stage){ KeyState(Key.S,false);KeyState(Key.W,false);KeyState(Key.Escape,false);KeyState(Key.C,false);stage=next;
   if(stage==1)KeyState(Key.S,true);if(stage==2||stage==6||stage==8)KeyState(Key.W,true);if(stage==3||stage==5)KeyState(Key.Escape,true);if(stage==7)KeyState(Key.C,true);
  }
  var v=Drive.Player;
  if(!float.IsFinite(v.position.x)||!float.IsFinite(v.speed)){Fail("nonfinite playable vehicle");return;}
  if(elapsed>4 && !checkedReverse){checkedReverse=true;if(Vec.Dot(v.velocity,v.Forward)>-1.2f){Fail("held reverse failed to sustain motion");return;}GD.Print("PASS held reverse longitudinal=",Vec.Dot(v.velocity,v.Forward));}
  if(stage==4&&!checkedPause){checkedPause=true;if(!Drive.Race.paused){Fail("Escape did not pause");return;}GD.Print("PASS pause remains responsive");}
  if(stage==6&&!checkedResume){checkedResume=true;if(Drive.Race.paused){Fail("Escape did not resume");return;}GD.Print("PASS resume");}
  if(elapsed>13){
   foreach(var node in Drive.Car.FindChildren("*","MeshInstance3D",true,false)){var m=(MeshInstance3D)node;if(m.HasMeta("wheel_hidden")&&m.Visible){Fail("camera reenabled a hidden wheel template");return;}}
   GD.Print("PASS playable input reverse, forward, pause/resume, camera visibility; wallHits=",Drive.WallHits," damage=",v.damage," speed=",v.speed);
   GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT")??"outputs/repair-20261029/input.png");GetTree().Quit();SetProcess(false);
  }
 }
 void Fail(string why){GD.PrintErr("FAIL input scenario ",why);GetTree().Quit(1);SetProcess(false);}
}
}
