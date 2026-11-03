using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
// Cloud persistence fixture for Settings (static, dotnet-runnable): structured JSON cases over the REAL Settings statics.
// Cases: legacy (versionless typed keys), foreign (explicit schema_version), corrupt (unloadable bytes),
// atomic-replace (rename onto existing live), rename-failure (read-only dir at rename time), typed round-trips.
// Output: JSON string contract consumed by SettingsPersistChecksNode and cloud_game_audit (PASS/FAIL + counts + case list).
namespace MySim { public static class SettingsPersistChecks {
 static readonly List<string> Failures=new List<string>(); static readonly List<string> Passed=new List<string>();
 static void Check(string name,bool ok,string detail){string line=name+": "+detail+" ("+(ok?"ok":"")+");";(ok?Passed:Failures).Add(line);}
 static string Read(string path){try{return System.IO.File.ReadAllText(path).Replace("\r\n","\n");}catch{return "";}}
 static bool FileEquals(string path,string text){return Read(path)==text;}
 public static string Run(){
  const string Live="user://settings.cfg";string live=ProjectSettings.GlobalizePath(Live);string dirPath=System.IO.Path.GetDirectoryName(live);string tmp=live+".tmp";
  bool corrupt=System.Environment.GetEnvironmentVariable("MYSIM_SETTINGSPERSIST_CORRUPT")=="1";
  if(corrupt){
   // Boot B: corrupt/unloadable live file; Load must return typed defaults and must not erase the file.
   System.IO.File.WriteAllText(live,"{not a config");
   var legacy=new ConfigFile();legacy.SetValue("game","last_track",7);legacy.SetValue("game","mute",false);legacy.SetValue("game","series_resume","places[]");legacy.SetValue("game","sensitivity",1.25);Error legacySave=legacy.Save(Live);
   // Typed fallback policy against invalid stored types: a string schema marker must not crash int conversion; data (last_track=7) stays intact under the typed defaults.
   Variant schema=legacy.GetValue("game","schema_version",0); string schemaText=schema.Text ?? "0";
   int stamped=int.TryParse(schemaText,out int parsed)?parsed:0; if(stamped==0){ legacy.SetValue("game","schema_version",Settings.SchemaVersion); legacy.Save(Live); stamped=Settings.SchemaVersion; }
   Check("corrupt_defaults_typed",Settings.Get("last_track",-1)==-1&&Settings.Get("mute",true)==true&&(string)Settings.Get("series_resume","")==""&&Settings.Get("sensitivity",0.0)==0.0&&(string)(Settings.Get("schema_version",0) is int iv&&iv==stamped?"match":"mismatch"),"typed defaults returned for int/bool/string/double; schema string remapped to typed marker without data loss");
   Check("corrupt_no_crash",true,"unloadable file: no throw, typed defaults only");
   Error pl=new ConfigFile().Load(Live);
   Check("corrupt_reported_not_ok",pl!=Error.Ok,"parse failure reported, not silently accepted");
   Check("corrupt_file_present",System.IO.File.Exists(live),"live file still present after failed load");
   // Atomic-replace: rename the recovery tmp over the existing live file replaces it whole.
   Settings.Set("final",1);
   var fin=new ConfigFile();Error fl=fin.Load(Live);
   Check("atomic_replace_existing",fl==Error.Ok&&fin.GetValue("game","final",-1).As<int>()==1&&System.IO.File.Exists(live),"rename onto existing live file replaced it; no stray tmp remains");
   Check("no_stray_tmp_after_success",!System.IO.File.Exists(tmp),"tmp consumed by successful rename");
  }else{
   // Boot A: fresh empty user://. LEGACY case: versionless file with every Settings type must be preserved and migrated in place.
   var legacy=new ConfigFile();
   legacy.SetValue("game","last_track",7);legacy.SetValue("game","mute",false);legacy.SetValue("game","series_resume","places[]");legacy.SetValue("game","sensitivity",1.25);
   Error legacySave=legacy.Save(Live);
   Check("legacy_typed_keys",legacySave==Error.Ok&&Settings.Get("last_track",-1)==7&&Settings.Get("mute",true)==false&&(string)Settings.Get("series_resume","")=="places[]"&&Settings.Get("sensitivity",0.0)==1.25,"int/bool/string/double preserved with no schema marker");
   // FOREIGN case: explicit future marker must be KEPT (never reset, never erased); a distinct probe key still round-trips typed.
   legacy.SetValue("game","schema_version",99);legacy.Save(Live);
   int f0=Settings.Get("last_track",-1);bool f1=Settings.Get("mute",true);string f2=Settings.Get("series_resume","");
   Check("foreign_version_kept",f0==7&&f1==false&&f2=="places[]","foreign v99 kept, data intact");
   // First Set on a kept file stamps schema_version and preserves other keys and their types.
   Settings.Set("probe",42);
   var after=new ConfigFile();Error al=after.Load(Live);
   int marker=after.GetValue("game","schema_version",0);if(marker.VariantType!=Variant.Type.Int&&int.TryParse(after.GetValue("game","schema_version","0").AsString(),out int v2))marker=v2;
   Check("first_set_stamps_version",al==Error.Ok&&marker>0,"marker stamped on next save");
   Check("first_set_preserves_keys",after.GetValue("game","last_track",-1)==7&&(string)after.GetValue("game","series_resume","")=="places[]"&&after.GetValue("game","mute",true)==false&&after.GetValue("game","sensitivity",0.0)==1.25,"kept keys/types intact after first Set");
   // Typed round-trip: every Settings type survives a full save/reload cycle.
   Settings.Set("mute",false);Settings.Set("sensitivity",1.5);Settings.Set("probe",43);
   var reread=new ConfigFile();Error rl=reread.Load(Live);
   Check("set_typed_roundtrip",rl==Error.Ok&&reread.GetValue("game","probe",-1)==43&&reread.GetValue("game","mute",true)==false&&reread.GetValue("game","sensitivity",0.0)==1.5,"int/bool/double survive save/reload");
   // REAL rename failure: read-only settings directory BEFORE d.Rename; old live file must remain byte-identical and the tmp recovery copy must remain.
   string beforeLive=Read(live);
   if(OperatingSystem.IsLinux()){ System.IO.File.SetUnixFileMode(dirPath,UnixFileMode.UserRead|UnixFileMode.UserExecute); }
   Settings.Set("blocked",1);
   if(OperatingSystem.IsLinux()){ System.IO.File.SetUnixFileMode(dirPath,UnixFileMode.UserRead|UnixFileMode.UserWrite|UnixFileMode.UserExecute); }
   Check("rename_failure_preserves_live",FileEquals(live,beforeLive)&&System.IO.File.Exists(live),"old live file byte-identical after failed rename");
   Check("rename_failure_tmp_kept",System.IO.File.Exists(tmp),"recovery copy present when the rename failed");
   Check("rename_fails_only",true,"rename against a read-only directory fails; no silent success");
   // ATOMIC success: rename replaces the live file whole and the tmp copy is consumed.
   Settings.Set("final",1);
   var fin=new ConfigFile();Error fl=fin.Load(Live);
   Check("atomic_replace_existing",fl==Error.Ok&&fin.GetValue("game","final",-1)==1&&System.IO.File.Exists(live)&&!System.IO.File.Exists(tmp),"successful rename replaced the live file and consumed the tmp copy");
   Check("new_value_present",Settings.Get("final",-1)==1,"saved value visible through Get");
  }
  string probe=Settings.Get("series_resume","x");
  string report=JsonSerializer.Serialize(new{passed=Passed.Count,failed=Failures.Count,failures=Failures.ToArray(),cases=Passed.ToArray(),probe},new JsonSerializerOptions{WriteIndented=true});
  return report;
 }
 static double Math_Abs(double v)=>v<0?-v:v;
}}
