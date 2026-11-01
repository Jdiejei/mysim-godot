using Godot;
namespace MySim {
// Automated scene probes release the complete scene and pending managed resource wrappers
// before engine shutdown. Exit codes and all earlier diagnostics remain unchanged.
public partial class CleanExit : Node {
 static bool requested;int code;
 public static void Request(Node caller,int exitCode=0){if(requested)return;requested=true;caller.GetViewport().Disable3D=true;GD.Print("CLEAN_EXIT requested code=",exitCode);caller.SetProcess(false);caller.SetPhysicsProcess(false);var exit=new CleanExit {code=exitCode};caller.GetTree().Root.CallDeferred(Node.MethodName.AddChild,exit);}
 public override async void _Ready(){
  var tree=GetTree();await ToSignal(tree,SceneTree.SignalName.ProcessFrame);
  GD.Print("CLEAN_EXIT freeing scene");var children=tree.Root.GetChildren();foreach(var child in children)if(child!=this)child.QueueFree();children.Clear();
  await ToSignal(tree,SceneTree.SignalName.ProcessFrame);await ToSignal(tree,SceneTree.SignalName.ProcessFrame);
  GD.Print("CLEAN_EXIT collecting resources");System.GC.Collect();GD.Print("CLEAN_EXIT awaiting finalizers");System.GC.WaitForPendingFinalizers();System.GC.Collect();GD.Print("CLEAN_EXIT quit code=",code);tree.Quit(code);
 }
}
}
