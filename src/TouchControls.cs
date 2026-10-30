using Godot;
namespace MySim {
// Mouse/touch controls are visible by default and can be disabled in Settings.
public partial class TouchControls : CanvasLayer {
 public DriveController Drive;
 public static float Steer,Throttle,Brake;public static bool Active;static TouchControls current;bool leftHeld,rightHeld;Window inputWindow;Button pauseButton;
 public override void _Ready(){
  Layer=5;current=this;Clear();Active=Settings.Get("screen_controls",true)||DisplayServer.IsTouchscreenAvailable()||System.Environment.GetEnvironmentVariable("MYSIM_TOUCH")!=null;
  inputWindow=GetWindow();inputWindow.FocusExited+=Clear;
  if(!Active)return;
  var root=new Control { AnchorRight=1,AnchorBottom=1,MouseFilter=Control.MouseFilterEnum.Ignore };AddChild(root);
  Make(root,"LEFT",new Vector2(32,562),new Vector2(130,118),v=>{leftHeld=v;UpdateSteer();});
  Make(root,"RIGHT",new Vector2(174,562),new Vector2(130,118),v=>{rightHeld=v;UpdateSteer();});
  Make(root,"GAS",new Vector2(1100,500),new Vector2(148,180),v=>Throttle=v?1:0);
  Make(root,"BRAKE",new Vector2(940,562),new Vector2(148,118),v=>Brake=v?1:0);
  Action(root,"PIT",new Vector2(24,80),()=>{if(Drive?.Player!=null)Drive.Player.pitRequested=true;});
  Action(root,"CAMERA",new Vector2(132,80),()=>Drive?.CycleCamera());
  pauseButton=Action(root,"PAUSE",new Vector2(264,80),()=>{if(Drive?.Race!=null){Drive.Race.paused=!Drive.Race.paused;Clear();}});
 }
 public override void _Process(double delta){if(pauseButton!=null&&Drive?.Race!=null)pauseButton.Text=Drive.Race.paused?"RESUME":"PAUSE";}
 void UpdateSteer()=>Steer=(rightHeld?1:0)-(leftHeld?1:0);
 void Clear(){leftHeld=rightHeld=false;if(current==this)Steer=Throttle=Brake=0;}
 public override void _ExitTree(){if(IsInstanceValid(inputWindow))inputWindow.FocusExited-=Clear;Clear();if(current==this){current=null;Active=false;}}
 static void Make(Control parent,string text,Vector2 pos,Vector2 size,System.Action<bool> set){
  bool right=text=="GAS"||text=="BRAKE";float left=pos.X-(right?1280:0),top=pos.Y-720;var button=new Button {Name=text,Text=text,AnchorLeft=right?1:0,AnchorRight=right?1:0,AnchorTop=1,AnchorBottom=1,OffsetLeft=left,OffsetRight=left+size.X,OffsetTop=top,OffsetBottom=top+size.Y,FocusMode=Control.FocusModeEnum.None};button.AddThemeFontSizeOverride("font_size",22);
  Style(button);button.ButtonDown+=()=>set(true);button.ButtonUp+=()=>set(false);parent.AddChild(button);
 }
 static void Style(Button button){
  button.AddThemeColorOverride("font_color",Colors.White);button.AddThemeColorOverride("font_hover_color",Colors.White);button.AddThemeColorOverride("font_pressed_color",Colors.White);
  foreach(string state in new[]{"normal","hover","pressed"})button.AddThemeStyleboxOverride(state,new StyleBoxFlat {BgColor=state=="pressed"?new Color(.06f,.35f,.42f,.98f):new Color(.035f,.05f,.07f,.92f),BorderColor=new Color(.45f,.62f,.7f,.9f),BorderWidthLeft=1,BorderWidthRight=1,BorderWidthTop=1,BorderWidthBottom=1,CornerRadiusTopLeft=8,CornerRadiusTopRight=8,CornerRadiusBottomLeft=8,CornerRadiusBottomRight=8});
 }
 static Button Action(Control parent,string text,Vector2 pos,System.Action action){var button=new Button {Name=text,Text=text,Position=pos,Size=new Vector2(100,40),FocusMode=Control.FocusModeEnum.None};Style(button);button.Pressed+=action;parent.AddChild(button);return button;}
}
}
