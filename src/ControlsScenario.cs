using Godot;
using System;
namespace MySim {
// Exercises the real controls dialog through button signals and input events; restores every original binding.
public partial class ControlsScenario : Node {
 void Check(bool value,string why){if(!value)throw new Exception(why);}
 void KeyEvent(Key key){Input.ParseInputEvent(new InputEventKey {Keycode=key,PhysicalKeycode=key,Pressed=true});Input.ParseInputEvent(new InputEventKey {Keycode=key,PhysicalKeycode=key,Pressed=false});}
 public override async void _Ready(){
  var original=new Key[ControlBindings.Actions.Length];for(int i=0;i<original.Length;i++)original[i]=ControlBindings.Get(i);
  try {
   ControlBindings.Reset();var panel=new ControlsPanel();GetTree().Root.AddChild(panel);
   await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
   var binding=panel.GetNode<Button>("Binding0");binding.EmitSignal(BaseButton.SignalName.Pressed);KeyEvent(Key.K);
   Check(ControlBindings.Get(0)==Key.K,"new throttle key not stored");
   Input.ParseInputEvent(new InputEventKey {Keycode=Key.K,PhysicalKeycode=Key.K,Pressed=true});Check(DisplayServer.GetName()=="headless" ? !ControlBindings.Held(0) : ControlBindings.Held(0),"rebound throttle or inactive-window input guard failed");KeyEvent(Key.K);
   binding.EmitSignal(BaseButton.SignalName.Pressed);KeyEvent(Key.S);Check(ControlBindings.Get(0)==Key.K,"conflicting brake key accepted");
   KeyEvent(Key.Escape);Check(IsInstanceValid(panel)&&!panel.IsQueuedForDeletion(),"cancel closed the entire dialog");
   binding.EmitSignal(BaseButton.SignalName.Pressed);KeyEvent(Key.Tab);Check(ControlBindings.Get(0)==Key.K,"reserved pause key accepted");KeyEvent(Key.Escape);
   panel.GetNode<Button>("RestoreDefaults").EmitSignal(BaseButton.SignalName.Pressed);Check(ControlBindings.Get(0)==Key.W,"reset did not restore controls");
   if(DisplayServer.GetName()!="headless")GetViewport().GetTexture().GetImage().SavePng(System.Environment.GetEnvironmentVariable("MYSIM_SHOT")??"outputs/cloud-game/controls.png");
   KeyEvent(Key.Escape);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);Check(!IsInstanceValid(panel),"back did not close controls");
   GD.Print("PASS controls dialog rebinding, persistence, physical input, conflict rejection, reserved pause, cancel, defaults and back");GetTree().Quit();
  }catch(Exception e){GD.PrintErr("FAIL controls dialog ",e);GetTree().Quit(1);}
  finally {for(int i=0;i<original.Length;i++)Settings.Set("key_"+i,(long)original[i]);}
 }
}
}
