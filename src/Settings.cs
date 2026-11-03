using Godot;
namespace MySim {
// Persistent settings (user://settings.cfg): last track/car/laps/camera, audio mute (default muted as requested).
public static class Settings {
 const string P = "user://settings.cfg"; public const int SchemaVersion = 2; static ConfigFile cf = new ConfigFile(); static bool loaded;
 static int SafeInt(Variant v) { try { return v.VariantType == Variant.Type.Int ? v.As<int>() : 0; } catch { return 0; } } // Malformed marker/values never crash: non-int or unreadable → treated as absent/legacy, disk untouched.
 static void Load() { if (loaded) return; loaded = true;
  if (cf.Load(P) != Error.Ok) { cf = new ConfigFile(); return; } // Unloadable file: fresh defaults in memory; nothing was recoverable from it; disk untouched until the next successful save.
  // Valid file WITHOUT an int schema_version = valid legacy state: PRESERVED in place; the marker is stamped in memory only and persisted by the next Set. Never erase valid legacy settings merely because the marker is missing; a FUTURE marker (>0) is kept as-is.
  if (SafeInt(cf.GetValue("game", "schema_version", 0)) == 0) cf.SetValue("game", "schema_version", SchemaVersion);
 }
 public static T Get<[MustBeVariant] T>(string key, T def) { Load(); Variant v = cf.GetValue("game", key, Variant.From(def)); try { return v.As<T>(); } catch { return def; } } // Malformed stored type: typed default returned, file never rewritten.
 public static bool Set<[MustBeVariant] T>(string key, T v) { Load(); cf.SetValue("game", key, Variant.From(v)); if (SafeInt(cf.GetValue("game", "schema_version", 0)) == 0) cf.SetValue("game", "schema_version", SchemaVersion);
  // Crash safety: the previous file is NEVER overwritten except by the atomic rename that fully replaces it. On ANY temp-save or rename failure the old file stays intact, the failure is reported and the sibling tmp copy is kept for manual recovery.
  string live = ProjectSettings.GlobalizePath(P);
  string tmp = live + ".tmp"; // Absolute same-directory sibling of the live file: the rename lands on the same volume and stays atomic, never a cross-volume copy.
  Error tmpSave = cf.Save(tmp);
  if (tmpSave != Error.Ok) { GD.PushError("SETTINGS_SAVE tmp=" + tmpSave + " old file kept, nothing replaced"); return false; }
  using var d = Godot.DirAccess.Open(System.IO.Path.GetDirectoryName(live));
  if (d == null) { GD.PushError("SETTINGS_SAVE dir_open_failed old file kept; recovery copy=" + tmp); return false; }
  string target = System.Environment.GetEnvironmentVariable("MYSIM_PERSIST_FAULT") == "rename" // Test-only deterministic fault seam: real failed rename syscall, env-gated, default absent in normal play.
   ? System.IO.Path.Combine(System.IO.Path.GetDirectoryName(live)!, "no-such-fault-dir", "blocked") : live;
  Error rename = d.Rename(tmp, target);
  if (rename != Error.Ok) { GD.PushError("SETTINGS_SAVE rename=" + rename + " old file kept; recovery copy=" + tmp); return false; }
  return true;
 }
 public static void ApplyAudio() { bool mute = Get("mute", true); AudioServer.SetBusMute(0, mute); }
 // Desktop defaults to physical inputs; an explicit saved choice also wins on touch hardware.
 public static bool ScreenControlsEnabled {
  get { string forced=System.Environment.GetEnvironmentVariable("MYSIM_TOUCH");return forced=="1"?true:forced=="0"?false:Get("screen_controls",DisplayServer.IsTouchscreenAvailable()); }
 }
}}
