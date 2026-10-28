using Godot;
namespace MySim {
// Persistent settings (user://settings.cfg): last track/car/laps/camera, audio mute (default muted as requested).
public static class Settings {
 const string Path = "user://settings.cfg"; static ConfigFile cf = new ConfigFile(); static bool loaded;
 static void Load() { if (loaded) return; loaded = true; cf.Load(Path); }
 public static T Get<[MustBeVariant] T>(string key, T def) { Load(); return cf.GetValue("game", key, Variant.From(def)).As<T>(); }
 public static void Set<[MustBeVariant] T>(string key, T v) { Load(); cf.SetValue("game", key, Variant.From(v)); cf.Save(Path); }
 public static void ApplyAudio() { bool mute = Get("mute", true); AudioServer.SetBusMute(0, mute); }
}}
