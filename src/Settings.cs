using Godot;
namespace MySim {
// Persistent settings (user://settings.cfg): last track/car/laps/camera, audio mute (default muted as requested).
public static class Settings {
 const string Path = "user://settings.cfg"; const int SchemaVersion = 2; static ConfigFile cf = new ConfigFile(); static bool loaded;
 static void Load() { if (loaded) return; loaded = true;
  if (cf.Load(Path) == Error.Ok) {
   int v = cf.GetValue("game", "schema_version", SchemaVersion).As<int>();
   if (v != SchemaVersion) { cf = new ConfigFile(); GD.PushWarning("SETTINGS_SCHEMA_MISMATCH dropped v" + v); } // Stale/corrupt state must not leak across sessions.
  }
 }
 public static T Get<[MustBeVariant] T>(string key, T def) { Load(); return cf.GetValue("game", key, Variant.From(def)).As<T>(); }
 public static void Set<[MustBeVariant] T>(string key, T v) { Load(); cf.SetValue("game", key, Variant.From(v)); cf.SetValue("game", "schema_version", SchemaVersion);
  string tmp = Path + ".tmp"; if (cf.Save(tmp) != Error.Ok || Godot.DirAccess.Rename(tmp, Path) != Error.Ok) cf.Save(Path); // Crash mid-write must not corrupt settings; rename is atomic on the same volume.
 }
 public static void ApplyAudio() { bool mute = Get("mute", true); AudioServer.SetBusMute(0, mute); }
 // Desktop defaults to physical inputs; an explicit saved choice also wins on touch hardware.
 public static bool ScreenControlsEnabled {
  get { string forced=System.Environment.GetEnvironmentVariable("MYSIM_TOUCH");return forced=="1"?true:forced=="0"?false:Get("screen_controls",DisplayServer.IsTouchscreenAvailable()); }
 }
}}
