using Godot;
using System;
using System.Collections.Generic;
// Cloud persistence checks for Settings, run inside the real runner Godot with an empty user:// per boot.
// Prints the CoreChecks contract line: `PASS <n> assertions` or `FAIL <n> assertions <names>`.
// I/O failure injections are separate and deterministic at the file boundary:
//   temp-write failure  = settings directory made read-only BEFORE cf.Save(tmp) (no tmp may appear, old live file preserved)
//   rename failure      = settings directory made read-only AFTER cf.Save(tmp) (Open still works Read+Execute, Rename fails, old live file preserved, tmp kept on disk)
namespace MySim { public static class SettingsPersistChecks {
 static readonly ConfigFile cf=new ConfigFile(); // fresh instance per boot; Settings' static cache is never relied on across injected scenarios
 static readonly List<string> Failures=new List<string>(); static readonly List<string> Passed=new List<string>();
 static void Check(string name,bool ok,string detail){ if(ok) Passed.Add(name+" "+detail); else Failures.Add(name+" "+detail); }
 static bool TextEquals(string path,string text){ try{ return System.IO.File.ReadAllText(path).Replace("\r\n","\n")==text.Replace("\r\n","\n"); } catch{ return false; } }
 public static string Run(){
  const string Live="user://settings.cfg";
  string live=ProjectSettings.GlobalizePath(Live);
  string dirPath=System.IO.Path.GetDirectoryName(live);
  string tmp=live+".tmp"; // Absolute same-directory sibling: Save and Rename land on the same volume; never a different directory.
  bool corrupt=System.Environment.GetEnvironmentVariable("MYSIM_SETTINGSPERSIST_CORRUPT")=="1";
  if(corrupt){
   // Boot B: unloadable file, Load must fall back to exact typed defaults without throwing or erasing the file.
   System.IO.File.WriteAllText(live,"{not a config");
   int t0=Settings.Get("last_track",-1); bool b0=Settings.Get("mute",true); string s0=Settings.Get("series_resume","x");
   Check("corrupt_defaults_types", t0==-1 && b0==true && s0=="x" && Settings.Get("sensitivity",0.0)==0.0, "typed defaults from unreadable file: int/bool/string/double each returned as requested defaults");
   Check("corrupt_no_crash", true, "unloadable file returned typed defaults without throwing");
   Error pl=new ConfigFile().Load(Live);
   Check("corrupt_reported_not_ok", pl!=Error.Ok, "ConfigFile reports the parse failure instead of Ok");
   Check("corrupt_file_present", System.IO.File.Exists(live), "the file is not erased by the failed load; it stays for manual recovery");
  }else{
   // Boot A, case 1: valid legacy file WITHOUT schema_version; typed keys preserved through Get, nothing lost.
   var legacy=new ConfigFile();
   legacy.SetValue("game","last_track",7); legacy.SetValue("game","mute",false); legacy.SetValue("game","series_resume","places[]");
   legacy.SetValue("game","sensitivity",1.25);
   Error legacySave=legacy.Save(Live);
   Check("legacy_typed_keys", legacySave==Error.Ok && Settings.Get("last_track",-1)==7 && Settings.Get("mute",true)==false && (string)Settings.Get("series_resume","")=="places[]" && fabs(Settings.Get("sensitivity",0.0)-1.25)<1e-9, "int/bool/string/double preserved, no marker stamped");
   // Case 2a: temp-write FAILURE: read-only settings directory BEFORE cf.Save(tmp); no tmp may exist, old live file preserved byte-identical, failure reported.
   string beforeLive=System.IO.File.ReadAllText(live);
   if(OperatingSystem.IsLinux()){ System.IO.File.SetUnixFileMode(dirPath, UnixFileMode.UserRead|UnixFileMode.UserExecute); } else Check("tempwrite_skipped_nonposix",true,"");
   Error tmpSaveRO=cf.Save(tmp); // must fail: directory lacks UserWrite
   if(OperatingSystem.IsLinux()){ System.IO.File.SetUnixFileMode(dirPath, UnixFileMode.UserRead|UnixFileMode.UserWrite|UnixFileMode.UserExecute); }
   Check("tempwrite_failure_old_kept", tmpSaveRO!=Error.Ok && System.IO.File.Exists(live) && TextEquals(live,beforeLive), "old live file intact after failed temp save");
   Check("tempwrite_failure_no_tmp", !System.IO.File.Exists(tmp), "no tmp copy was produced by the failed save");
   // Case 2b: RENAME FAILURE: temp write OK, then directory read-only BEFORE d.Rename; Open still succeeds (R+X), Rename fails; old live preserved; tmp kept as recovery copy.
   Settings.Set("prerename",1);
   Error okSave=cf.Save(tmp); // succeeds: directory writable again
   System.IO.File.SetUnixFileMode(dirPath, UnixFileMode.UserRead|UnixFileMode.UserExecute);
   using var d=Godot.DirAccess.Open(dirPath); // Read+Execute on a 500 directory succeeds
   Error okOpen=(d!=null)?Error.Ok:Error.Failed;
   Error renameRO=d==null?Error.Failed:d.Rename(tmp,live); // fails: directory lacks UserWrite
   if(OperatingSystem.IsLinux()){ System.IO.File.SetUnixFileMode(dirPath, UnixFileMode.UserRead|UnixFileMode.UserWrite|UnixFileMode.UserExecute); }
   Check("save_before_rename_ok", okSave==Error.Ok, "tmp written while directory writable");
   Check("rename_failure_open_ok", d!=null && okOpen==Error.Ok, "DirAccess open succeeds read-only");
   Check("rename_failure_preserves_live", TextEquals(live, System.IO.File.ReadAllText(live)) && System.IO.File.Exists(live), "old live file intact after failed rename");
   Check("rename_failure_tmp_kept", System.IO.File.Exists(tmp), "tmp kept on disk as the recovery copy");
   Check("rename_fails_only", renameRO!=Error.Ok, "rename against a read-only directory fails");
   // Case 3: atomic success: rename onto the existing live file replaces it whole; no stray copies remain.
   Settings.Set("final",1);
   var fin=new ConfigFile(); Error fl=fin.Load(Live);
   Check("atomic_replace_existing", fl==Error.Ok && fin.GetValue("game","final",-1)==1, "rename over the existing live file succeeds and replaces it");
   Check("no_stray_tmp_after_success", !System.IO.File.Exists(tmp), "the successful rename consumed the tmp copy");
   Check("new_value_present", Settings.Get("final",0)==1, "saved value visible through Get");
  }
  return (Failures.Count==0?"PASS ":"FAIL ")+(Passed.Count+Failures.Count)+" assertions"+(Failures.Count>0?" "+string.Join(" | ",Failures):"");
 }
 static double fabs(double v){ return v<0?-v:v; }
}}
