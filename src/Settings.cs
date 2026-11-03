using Godot;
namespace MySim {
// Persistent settings (user://settings.cfg): last track/car/laps/camera, audio mute (default muted as requested).
public static class Settings {
 const string Path = "user://settings.cfg"; const int SchemaVersion = 2; static ConfigFile cf = new ConfigFile(); static bool loaded;
 static void Load() { if (loaded) return; loaded = true;
  if (cf.Load(Path) != Error.Ok) { cf = new ConfigFile(); return; } // Unloadable file: fresh defaults; nothing was recoverable.
  // Missing schema_version = valid legacy file: PRESERVE all values in place (migration, not reset); stamp the version marker on next Save.
  // Explicit foreign version present: keep data too; type-safe defaults apply only where individual keys are unreadable. Never erase valid legacy settings merely because the marker is missing.
  int v = cf.GetValue("game", "schema_version", 0).As<int>();
  if (v != 0 && v != SchemaVersion) GD.PushWarning("SETTINGS_SCHEMA foreign v" + v + " KEPT; values preserved for migration");
 }
 public static T Get<[MustBeVariant] T>(string key, T def) { Load(); return cf.GetValue("game", key, Variant.From(def)).As<T>(); }
 public static void Set<[MustBeVariant] T>(string key, T v) { Load(); cf.SetValue("game", key, Variant.From(v)); cf.SetValue("game", "schema_version", SchemaVersion);
  string tmp = Path + ".tmp"; // Same directory as Path, so the rename lands on the same volume and stays atomic.
  if (cf.Save(tmp) != Error.Ok) { cf.Save(Path); return; } // Fall back to the direct write only if even the temp write failed.
  using var dir = Godot.DirAccess.Open(ProjectSettings.GlobalizePath(Path).GetBaseDir());
  if (dir == null || dir.Rename(tmp, Path) != Error.Ok) cf.Save(Path); // Crash mid-write must not corrupt settings; rename is atomic on the same volume, overwrites the existing file.
 }
 public static void ApplyAudio() { bool mute = Get("mute", true); AudioServer.SetBusMute(0, mute); }
 // Desktop defaults to physical inputs; an explicit saved choice also wins on touch hardware.
 public static bool ScreenControlsEnabled {
  get { string forced=System.Environment.GetEnvironmentVariable("MYSIM_TOUCH");return forced=="1"?true:forced=="0"?false:Get("screen_controls",DisplayServer.IsTouchscreenAvailable()); }
 }
}}
