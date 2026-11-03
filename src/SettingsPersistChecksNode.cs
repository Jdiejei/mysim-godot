using Godot;
// Godot-side runner node for the Settings persistence fixture: the ONLY thing this boot does.
// Runs the real Settings runtime against isolated per-case files under user://, prints a structured
// JSON report and exits non-zero when any assertion fails (consumed by cloud_game_audit.py).
namespace MySim { public sealed partial class SettingsPersistChecksNode : Node {
 public override void _Ready(){
  string report=SettingsPersistChecks.Run();
  GD.Print("SETTINGSPERSIST ",report);
  SettingsPersistChecks.WriteReport(System.Environment.GetEnvironmentVariable("MYSIM_SETTINGS_REPORT")??"data/settings-persist/report.json");
  GetTree().Quit(ChecksHasFailures()?1:0);
 }
 bool ChecksHasFailures(){return SettingsPersistChecks.HasFailures;}
}}
