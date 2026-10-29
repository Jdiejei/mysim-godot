using Godot;
namespace MySim {
public static class ControlBindings {
 public static readonly string[] Actions={"Throttle","Brake / reverse","Steer left","Steer right","Handbrake","Pit request","Camera","Mute"};
 public static readonly Key[] Defaults={Key.W,Key.S,Key.A,Key.D,Key.Space,Key.P,Key.C,Key.M};
 public static Key Get(int action)=>(Key)Settings.Get("key_"+action,(long)Defaults[action]);
 public static bool Held(int action){bool key=Input.IsPhysicalKeyPressed(Get(action));return key||(action==0&&Input.IsKeyPressed(Key.Up))||(action==1&&Input.IsKeyPressed(Key.Down))||(action==2&&Input.IsKeyPressed(Key.Left))||(action==3&&Input.IsKeyPressed(Key.Right));}
 public static int Conflict(int action,Key key){for(int i=0;i<Actions.Length;i++)if(i!=action&&Get(i)==key)return i;return -1;}
 public static void Reset(){for(int i=0;i<Actions.Length;i++)Settings.Set("key_"+i,(long)Defaults[i]);}
}
public partial class ControlsPanel : CanvasLayer {
 int waiting=-1;Button[] buttons;Label message;
 public override void _Ready(){
  Layer=50;var background=new ColorRect {Color=new Color(.025f,.035f,.05f,.98f),AnchorRight=1,AnchorBottom=1};AddChild(background);
  var title=new Label {Text="DRIVING CONTROLS",Position=new Vector2(80,48)};title.AddThemeFontSizeOverride("font_size",32);AddChild(title);
  buttons=new Button[ControlBindings.Actions.Length];
  for(int i=0;i<buttons.Length;i++){int action=i;float y=112+i*52;AddChild(new Label {Text=ControlBindings.Actions[i],Position=new Vector2(80,y+8)});var button=new Button {Position=new Vector2(420,y),Size=new Vector2(250,42)};button.Pressed+=()=>{waiting=action;message.Text="Press a key. Escape cancels; pause keys stay reserved.";Refresh();};buttons[i]=button;AddChild(button);}
  message=new Label {Text="Arrow keys also drive. Esc / Tab pause. Controller: stick, RT/LT, A pit, Y camera, Start pause.",Position=new Vector2(80,546)};AddChild(message);
  var reset=new Button {Text="RESTORE DEFAULTS",Position=new Vector2(80,610),Size=new Vector2(250,48)};reset.Pressed+=()=>{waiting=-1;ControlBindings.Reset();Refresh();message.Text="Default controls restored.";};AddChild(reset);
  var close=new Button {Text="BACK",Position=new Vector2(420,610),Size=new Vector2(250,48)};close.Pressed+=()=>QueueFree();AddChild(close);Refresh();close.GrabFocus();
 }
 void Refresh(){for(int i=0;i<buttons.Length;i++)buttons[i].Text=waiting==i?"PRESS A KEY…":OS.GetKeycodeString(ControlBindings.Get(i));}
 public override void _Input(InputEvent input){
  if(input is not InputEventKey key||!key.Pressed||key.Echo)return;
  if(key.Keycode==Key.Escape){if(waiting<0)QueueFree();else{waiting=-1;Refresh();message.Text="Binding cancelled.";}GetViewport().SetInputAsHandled();return;}
  if(waiting<0)return;GetViewport().SetInputAsHandled();Key physical=key.PhysicalKeycode!=Key.None?key.PhysicalKeycode:key.Keycode;
  if(physical==Key.Tab){message.Text="Tab is reserved for pause.";return;}
  int conflict=ControlBindings.Conflict(waiting,physical);if(conflict>=0){message.Text="Already assigned to "+ControlBindings.Actions[conflict]+". Choose another key.";return;}
  Settings.Set("key_"+waiting,(long)physical);waiting=-1;Refresh();message.Text="Binding saved.";
 }
}
}
