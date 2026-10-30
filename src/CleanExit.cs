using Godot;
namespace MySim {
// Automated scene probes release the complete scene and pending managed resource wrappers
// before engine shutdown. Exit codes and all earlier diagnostics remain unchanged.
public partial class CleanExit : Node {
 static bool requested;int code;
 public static void Request(Node caller,int exitCode=0){if(requested)return;requested=true;caller.SetProcess(false);caller.SetPhysicsProcess(false);var exit=new CleanExit {code=exitCode};caller.GetTree().Root.CallDeferred(Node.MethodName.AddChild,exit);}
 public override async void _Ready(){
  var tree=GetTree();await ToSignal(tree,SceneTree.SignalName.ProcessFrame);
  foreach(var child in tree.Root.GetChildren())if(child!=this)child.QueueFree();
  await ToSignal(tree,SceneTree.SignalName.ProcessFrame);await ToSignal(tree,SceneTree.SignalName.ProcessFrame);
  System.GC.Collect();System.GC.WaitForPendingFinalizers();System.GC.Collect();tree.Quit(code);
 }
}
}
