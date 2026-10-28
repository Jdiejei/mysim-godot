using Godot;
namespace MySim {
// On-screen racing controls (touch / mouse): steer left/right on the left, throttle/brake on the right. Shown on touch devices or when MYSIM_TOUCH is set.
public partial class TouchControls : CanvasLayer {
 public static float Steer, Throttle, Brake; public static bool Active;
 public override void _Ready() {
  Layer = 5; Active = DisplayServer.IsTouchscreenAvailable() || System.Environment.GetEnvironmentVariable("MYSIM_TOUCH") != null; if (!Active) return;
  var root = new Control { AnchorRight = 1, AnchorBottom = 1, MouseFilter = Control.MouseFilterEnum.Ignore }; AddChild(root);
  Make(root, "<", new Vector2(40, 520), new Vector2(150, 150), v => Steer = v ? -1 : (Steer < 0 ? 0 : Steer));
  Make(root, ">", new Vector2(210, 520), new Vector2(150, 150), v => Steer = v ? 1 : (Steer > 0 ? 0 : Steer));
  Make(root, "GAS", new Vector2(1090, 440), new Vector2(160, 230), v => Throttle = v ? 1 : 0);
  Make(root, "BRAKE", new Vector2(930, 520), new Vector2(140, 150), v => Brake = v ? 1 : 0);
 }
 static void Make(Control parent, string text, Vector2 pos, Vector2 size, System.Action<bool> set) {
  var b = new Button { Text = text, Position = pos, Size = size, Modulate = new Color(1, 1, 1, 0.55f), FocusMode = Control.FocusModeEnum.None }; b.AddThemeFontSizeOverride("font_size", 36);
  b.ButtonDown += () => set(true); b.ButtonUp += () => set(false); parent.AddChild(b);
 }
}}
