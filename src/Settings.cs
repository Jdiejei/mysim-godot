using Godot;
namespace MySim {
// Persistent settings (user://settings.cfg): last track/car/laps/camera, audio mute (default muted as requested).
public static class Settings {
 const string P = "user://settings.cfg"; const int SchemaVersion = 2; static ConfigFile cf = new ConfigFile(); static bool loaded;
 static void Load() { if (loaded) return; loaded = true;
  if (cf.Load(P) != Error.Ok) { cf = new ConfigFile(); return; } // Unloadable file: fresh defaults; nothing was recoverable from it.
  // Valid file WITHOUT schema_version = valid legacy state: PRESERVED in place (values and types untouched); the marker is stamped on the next Save. Never erase valid legacy settings merely because the marker is missing.
  if (cf.GetValue("game", "schema_version", 0).As<int>() == 0) cf.SetValue("game", "schema_version", SchemaVersion);
 }
 public static T Get<[MustBeVariant] T>(string key, T def) { Load(); return cf.GetValue("game", key, Variant.From(def)).As<T>(); }
 public static void Set<[MustBeVariant] T>(string key, T v) { Load(); cf.SetValue("game", key, Variant.From(v)); cf.SetValue("game", "schema_version", SchemaVersion);
  // Crash safety: the previous file is NEVER overwritten except by the atomic rename that fully replaces it. On ANY temp-save or rename failure the old file stays intact, the failure is reported and the sibling tmp copy is kept for manual recovery.
  string live = ProjectSettings.GlobalizePath(P);
  string tmp = live + ".tmp"; // Absolute same-directory sibling of the live file: the rename lands on the same volume and stays atomic, never a cross-volume copy.
  Error tmpSave = cf.Save(tmp);
  if (tmpSave != Error.Ok) { GD.PushError("SETTINGS_SAVE tmp=" + tmpSave + " old file kept, nothing replaced"); return; }
  using var d = Godot.DirAccess.Open(System.IO.Path.GetDirectoryName(live));
  if (d == null) { GD.PushError("SETTINGS_SAVE dir_open_failed old file kept; recovery copy=" + tmp); return; }
  Error rename = d.Rename(tmp, live);
  if (rename != Error.Ok) { GD.PushError("SETTINGS_SAVE rename=" + rename + " old file kept; recovery copy=" + tmp); return; }
 }
 public static void ApplyAudio() { bool mute = Get("mute", true); AudioServer.SetBusMute(0, mute); }
 // Desktop defaults to physical inputs; an explicit saved choice also wins on touch hardware.
 public static bool ScreenControlsEnabled {
  get { string forced=System.Environment.GetEnvironmentVariable("MYSIM_TOUCH");return forced=="1"?true:forced=="0"?false:Get("screen_controls",DisplayServer.IsTouchscreenAvailable()); }
 }
}}
