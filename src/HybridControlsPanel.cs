using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Battery controls bind to the same race vehicle consumed by the physical chassis.
public partial class HybridControlsPanel : PanelContainer {
 public Vehicle Player;public Race Race;Button automatic,overtake;Label charge;bool held,modeWas;
 public override void _Ready(){Name="HybridControls";Theme=MySimTheme.Build();AnchorLeft=AnchorRight=.5f;AnchorTop=AnchorBottom=1;OffsetLeft=-180;OffsetRight=180;OffsetTop=-76;OffsetBottom=-18;
  var row=new HBoxContainer();row.AddThemeConstantOverride("separation",8);AddChild(row);charge=new Label {CustomMinimumSize=new Vector2(92,44),VerticalAlignment=VerticalAlignment.Center};row.AddChild(charge);
  automatic=new Button {Name="Auto",Text="AUTO",ToggleMode=true,CustomMinimumSize=new Vector2(92,44)};automatic.Toggled+=value=>{if(Player!=null){Player.ersAuto=value;Player.ersOvertake=false;}held=false;};row.AddChild(automatic);
  overtake=new Button {Name="Overtake",Text="OVERTAKE",CustomMinimumSize=new Vector2(130,44)};overtake.ButtonDown+=()=>{held=true;if(Player!=null&&!Player.ersAuto&&Race?.Running==true&&!Player.servicing&&!Player.finished&&!Player.retired)Player.ersOvertake=true;};overtake.ButtonUp+=()=>{held=false;if(Player!=null)Player.ersOvertake=false;};row.AddChild(overtake);
 }
 public override void _Process(double delta){bool hybrid=Player!=null&&HybridDeployment.IsHybrid(Player.carModel);Visible=hybrid;if(!hybrid)return;bool active=Race!=null&&!Race.paused&&!Player.finished&&!Player.retired&&!Player.servicing&&DisplayServer.WindowIsFocused();
  bool mode=ControlBindings.Held(8);if(active&&mode&&!modeWas)Player.ersAuto=!Player.ersAuto;modeWas=mode;
  automatic.SetPressedNoSignal(Player.ersAuto);automatic.Disabled=!active;overtake.Disabled=!active||Player.ersAuto||Player.ersCharge<=.001f;
  if(!active)held=false;bool pad=false;var pads=Input.GetConnectedJoypads();if(pads.Count>0)pad=Input.IsJoyButtonPressed(pads[0],JoyButton.RightShoulder);
  Player.ersOvertake=active&&!Player.ersAuto&&(held||ControlBindings.Held(9)||pad);charge.Text="ERS "+(int)(Player.ersCharge*100)+"%";overtake.Text=Player.ersActive?"DEPLOYING":"OVERTAKE";
 }
 public override void _ExitTree(){if(Player!=null)Player.ersOvertake=false;}
}
}
