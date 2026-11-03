using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
// Settings persistence fixture. ONE scenario per fresh Godot process (MYSIM_SETTINGSPERSIST_SCENARIO):
//   legacy    - versionless typed file preserved; marker stamped only by the first Set; atomic replace works
//   foreign   - future marker (99) KEPT through saves (never rewritten to 2); values intact
//   corrupt   - unloadable bytes: typed defaults, file preserved, Load reports failure
//   malformed - wrong-typed values (string where int expected, string schema marker): no crash, typed defaults, disk untouched by pure Get
//   failures  - REAL I/O faults through the production Set path: temp-save failure (read-only dir BEFORE save: no tmp, live intact)
//               and rename failure (seam swaps the rename target to an impossible path AFTER a successful tmp write: live intact, tmp recovery copy kept)
// Output: structured JSON {passed,failed,failures[],cases[]}; HasFailures drives the process exit code.
namespace MySim { public static class SettingsPersistChecks {
 static readonly List<string> Failures=new List<string>(); static readonly List<string> Passed=new List<string>();
 static void Check(string name,bool ok,string detail){ if(ok) Passed.Add(name+": "+detail); else Failures.Add(name+": "+detail); }
 static string Read(string p){ try{ return System.IO.File.ReadAllText(p).Replace("\r\n","\n"); } catch{ return ""; } }
 static bool FileEquals(string p,string text){ return Read(p)==text; }
 public static bool HasFailures=>Failures.Count>0; public static int PassedCount=>Passed.Count; public static int FailedCount=>Failures.Count;
 static int Int(ConfigFile f,string key,int def){ Variant v=f.GetValue("game",key,def); try{ return v.VariantType==Variant.Type.Int?v.As<int>():def; } catch{ return def; } }
 static string Str(ConfigFile f,string key){ return f.GetValue("game",key,"").AsString(); }
 public static string Run(){
  const string Live="user://settings.cfg";
  string live=ProjectSettings.GlobalizePath(Live), tmp=live+".tmp";
  string scenario=System.Environment.GetEnvironmentVariable("MYSIM_SETTINGSPERSIST_SCENARIO") ?? "legacy";
  if(scenario=="legacy"){
   var seed=new ConfigFile();
   seed.SetValue("game","last_track",7); seed.SetValue("game","mute",false); seed.SetValue("game","series_resume","places[]"); seed.SetValue("game","sensitivity",1.25);
   Check("legacy_seed_save",seed.Save(Live)==Error.Ok,"versionless typed file seeded");
   Check("legacy_readback",Settings.Get("last_track",-1)==7&&Settings.Get("mute",true)==false&&Settings.Get("series_resume","")=="places[]"&&Settings.Get("sensitivity",0.0)==1.25,"int/bool/string/double preserved from the versionless file");
   Check("legacy_get_did_not_write_disk",Read(Live).IndexOf("schema_version",System.StringComparison.Ordinal)<0,"pure Get left the file without the marker");
   Check("legacy_set_stamps",Settings.Set("probe",42),"first Set succeeded");
   var after=new ConfigFile();
   Check("legacy_marker_and_values_persisted",after.Load(Live)==Error.Ok&&Int(after,"schema_version",0)==Settings.SchemaVersion&&Int(after,"last_track",0)==7&&after.GetValue("game","series_resume","").AsString()=="places[]","marker stamped; prior values intact");
   Check("atomic_replace_existing",Settings.Set("final",1)&&new ConfigFile().Load(Live)==Error.Ok&&Int(new ConfigFile(),"final",0)==1&&!System.IO.File.Exists(tmp),"atomic replacement of the live file by rename; tmp consumed");
  }else if(scenario=="foreign"){
   var seed=new ConfigFile();
   seed.SetValue("game","schema_version",99); seed.SetValue("game","last_track",3); seed.SetValue("game","mute",false);
   Check("foreign_seed_save",seed.Save(Live)==Error.Ok,"future marker seeded");
   Check("foreign_readback",Settings.Get("last_track",-1)==3&&Settings.Get("mute",true)==false,"values readable from the future-schema file");
   Check("foreign_set_succeeds",Settings.Set("extra",1),"save works under a future marker");
   var after=new ConfigFile();
   Check("foreign_marker_kept",after.Load(Live)==Error.Ok&&Int(after,"schema_version",0)==99&&Int(after,"extra",0)==1&&Int(after,"last_track",0)==3,"future marker NOT rewritten to 2; values stayed");
  }else if(scenario=="corrupt"){
   System.IO.File.WriteAllText(live,"{not a config");
   Check("corrupt_defaults_typed",Settings.Get("last_track",-1)==-1&&Settings.Get("mute",true)==true&&Settings.Get("series_resume","x")=="x"&&Settings.Get("sensitivity",0.0)==0.0,"loaded nothing; typed defaults exactly as requested");
   Check("corrupt_no_crash",true,"unreadable file did not throw");
   Check("corrupt_reported_not_ok",new ConfigFile().Load(Live)!=Error.Ok,"parser reports the failure, not success");
   Check("corrupt_file_intact",Read(Live)=="{not a config","disk untouched by the failed load");
  }else if(scenario=="malformed"){
   var seed=new ConfigFile();
   seed.SetValue("game","schema_version","abc"); seed.SetValue("game","last_track","abc"); seed.SetValue("game","mute","not-a-bool"); seed.SetValue("game","sensitivity","fast");
   Check("malformed_seed_save",seed.Save(Live)==Error.Ok,"wrong-typed values seeded");
   Check("malformed_defaults_typed",Settings.Get("last_track",-1)==-1&&Settings.Get("mute",true)==true&&Settings.Get("sensitivity",0.0)==0.0&&Settings.Get("schema_version",0)==0,"every wrong type falls back to its typed default without throwing");
   Check("malformed_disk_untouched_by_get",Read(Live).IndexOf("schema_version",System.StringComparison.Ordinal)>=0&&Read(Live).IndexOf("last_track",System.StringComparison.Ordinal)>0,"pure Get did not rewrite or 'repair' the file");
   System.Environment.SetEnvironmentVariable("MYSIM_PERSIST_FAULT","rename");
   bool setOk=Settings.Set("recovery-test",1);
   System.Environment.SetEnvironmentVariable("MYSIM_PERSIST_FAULT",null);
   Check("malformed_set_attempts_clean_path",setOk,"Set with an invalid marker still executes its save path and reports success/failure truthfully");
  }else if(scenario=="failures"){
   Check("baseline_saved",Settings.Set("baseline",5),"good save establishes the live file");
   string before=Read(Live); lastGoodLive=before;
   if(OperatingSystem.IsLinux()){ System.IO.File.SetUnixFileMode(System.IO.Path.GetDirectoryName(live)!,System.IO.UnixFileMode.UserRead|System.IO.UnixFileMode.UserExecute); }
   bool tempOk=Settings.Set("temp-fault",6);
   if(OperatingSystem.IsLinux()){ System.IO.File.SetUnixFileMode(System.IO.Path.GetDirectoryName(live)!,System.IO.UnixFileMode.UserRead|System.IO.UnixFileMode.UserWrite|System.IO.UnixFileMode.UserExecute); }
   Check("temp_failure_rejected",!tempOk,"Save reported failure at the temp-write stage");
   Check("temp_no_tmp",!System.IO.File.Exists(tmp),"no half-written tmp exists after the failed temp save");
   Check("temp_live_kept",FileEquals(Live,beforeLiveVal),"older live file untouched");
   System.Environment.SetEnvironmentVariable("MYSIM_PERSIST_FAULT","rename");
   bool renOk=Settings.Set("rename-fault",7);
   System.Environment.SetEnvironmentVariable("MYSIM_PERSIST_FAULT",null);
   Check("rename_fault_rejected",!renOk,"Save reported failure at the rename stage (tmp had been written successfully first)");
   Check("rename_tmp_recovery_kept",System.IO.File.Exists(tmp),"recovery copy kept on disk");
   Check("rename_live_kept",FileEquals(Live,beforeLiveVal),"older live file untouched by the failed rename");
   System.IO.File.Delete(tmp);
   Check("atomic_success_after_faults",Settings.Set("final",9),"recovery save succeeded after both fault classes");
  }else Check("unknown_scenario",false,"scenario="+scenario);
  return JsonSerializer.Serialize(new{scenario,passed=Passed.Count,failed=Failures.Count,failures=Failures.ToArray(),cases=Passed.ToArray()},new JsonSerializerOptions{WriteIndented=true});
 }
 static string beforeLiveVal=>lastGoodLive;
 static string lastGoodLive="";
 public static void WriteReport(string path,string contents){ System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!); System.IO.File.WriteAllText(path,contents); }
}}

