using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Minimal in-game HUD: speed km/h, gear, rpm, lap/target, lap time, tyre wear. Real UI nodes (not a camera render).
public partial class Hud : CanvasLayer {
 public Race Race; public Vehicle Player; Label speed, info, gear;
 public override void _Ready() {
  var root = new Control { AnchorRight = 1, AnchorBottom = 1, MouseFilter = Control.MouseFilterEnum.Ignore }; AddChild(root);
  speed = Make(root, 64, new Vector2(-300, -140), true); gear = Make(root, 40, new Vector2(-300, -210), true); info = Make(root, 22, new Vector2(24, 20), false);
 }
 static Label Make(Control p, int size, Vector2 off, bool bottomRight) {
  var l = new Label(); l.AddThemeFontSizeOverride("font_size", size); l.AddThemeColorOverride("font_color", Colors.White); l.AddThemeColorOverride("font_outline_color", Colors.Black); l.AddThemeConstantOverride("outline_size", 6);
  if (bottomRight) { l.AnchorLeft = l.AnchorRight = 1; l.AnchorTop = l.AnchorBottom = 1; l.OffsetLeft = off.X; l.OffsetTop = off.Y; l.OffsetRight = -24; l.OffsetBottom = off.Y + 70; l.HorizontalAlignment = HorizontalAlignment.Right; }
  else { l.Position = off; }
  p.AddChild(l); return l;
 }
 public override void _Process(double delta) {
  if (Player == null) return;
  speed.Text = ((int)(Player.speed * 3.6f)) + " km/h"; gear.Text = "GEAR " + Player.gear;
  info.Text = "LAP " + (Player.completedLaps + 1) + "/" + Race.targetLaps + "   LAP TIME " + Player.lapElapsed.ToString("0.0") + "s   BEST " + (Player.bestLap > 0 ? Player.bestLap.ToString("0.00") : "--") + "\nTYRES " + (int)(100 - Player.Wear * 100) + "%   PIT " + (Player.servicing ? "SERVICING " + Player.serviceRemaining.ToString("0.0") + "s" : Player.pitActive ? "IN LANE" : Player.pitRequested ? "REQUESTED (P)" : "press P") + "   FUEL " + (int)Player.fuel + " L   DAMAGE " + (int)(Player.damage * 100) + "%";
 }
}}

