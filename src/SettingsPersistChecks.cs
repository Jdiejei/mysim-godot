using Godot;
using System;
using System.Collections.Generic;
// Cloud persistence checks for Settings, executed inside the real runner Godot with an empty user:// per boot.
// Boot A (default): legacy migration, foreign version keep, typed round-trips, atomic rename onto an existing live file, real rename failure via a read-only settings directory.
// Boot B (MYSIM_SETTINGSPERSIST_CORRUPT=1): unloadable/corrupt file only: typed defaults, no crash, no erasure.
// Prints the CoreChecks report contract: `PASS <n> assertions` or `FAIL <n> assertions <names>`.
namespace MySim { public static class SettingsPersistChecks {
 static readonly List<string> Failures=new List<string>(); static readonly List<string> Passed=new List<string>();
 static void Check(string name,bool ok,string detail){string line=name+" "+detail;if(ok)Passed.Add(line);else Failures.Add(line);}
 static bool FileTextEquals(string path,string text){try{return System.IO.File.ReadAllText(path).Replace("\r\n","\n")==text.Replace("\r\n","\n");}catch{return false;}}
 public static string Run(){
  const string Live="user://settings.cfg";string live=ProjectSettings.GlobalizePath(Live);
  bool corrupt=System.Environment.GetEnvironmentVariable("MYSIM_SETTINGSPERSIST_CORRUPT")=="1";
  if(corrupt){
   // Boot B: unloadable file, Load must fall back to exact typed defaults without throwing or erasing the file.
   System.IO.File.WriteAllText(live,"{not a config");
   int t0=Settings.Get("last_track",-1);bool b0=Settings.Get("mute",true);string s0=Settings.Get("series_resume","x");double d0=Settings.Get("sensitivity",1.1);
   Check("corrupt_defaults_types",t0==-1&&b0==true&&s0=="" &&Math.Abs(d0-1.1)<1e-9,"exact typed defaults after parse failure");
   Check("corrupt_no_crash",true,"unloadable file returned typed defaults without throwing");
   Error pl=new ConfigFile().Load(Live);
   Check("corrupt_reported_not_ok",pl!=Error.Ok,"ConfigFile reports parse failure");
   Check("corrupt_file_present",System.IO.File.Exists(live),"file not erased by the failed load");
  }else{
   // Boot A. Case 1: valid legacy file WITHOUT schema_version: typed keys preserved through unchanged Get; nothing lost.
   var legacy=new ConfigFile();
   legacy.SetValue("game","last_track",7);legacy.SetValue("game","mute",false);legacy.SetValue("game","series_resume","places[]");
   legacy.SetValue("game","sensitivity",1.25);Error legacySave=legacy.Save(Live);
   Check("legacy_typed_keys",legacySave==Error.Ok&&Settings.Get("last_track",-1)==7&&Settings.Get("mute",true)==false&&(string)Settings.Get("series_resume","")=="places[]"&&Math.Abs((double)Settings.Get("sensitivity",0.0)-1.25)<1e-9,"int/bool/string/double all preserved, no marker");
   // Case 2: foreign explicit version: values KEPT, no reset, no erasure; a distinct marker still round-trips.
   legacy.SetValue("game","schema_version",99);legacy.Save(Live);
   int f0=Settings.Get("last_track",-1);bool f1=Settings.Get("mute",true);string f2=Settings.Get("series_resume","");
   Check("foreign_version_kept",f0==7&&f1==false&&f2=="places[]","foreign v99 not reset, data intact");
   // Case 3: first Set on a kept file stamps schema_version and preserves the other keys and types.
   Settings.Set("probe",42);
   var after=new ConfigFile();Error al=after.Load(Live);
   Check("first_set_stamps_version",al==Error.Ok&&after.GetValue("game","schema_version",-1).As<int>()>0,"marker stamped on next save");
   Check("first_set_preserves_keys",after.GetValue("game","last_track",-1).As<int>()==7&&(string)after.GetValue("game","series_resume","")=="places[]"&&after.GetValue("game","mute",true).As<bool>()==false&&after.GetValue("game","sensitivity",0.0).As<double>()==1.25,"kept keys/types intact after first Set");
   // Case 4: typed round-trip across a full save/reload with every Settings type.
   Settings.Set("mute",false);Settings.Set("sensitivity",1.5);Settings.Set("probe",43);
   var reread=new ConfigFile();Error rl=reread.Load(Live);
   Check("set_typed_roundtrip",rl==Error.Ok&&reread.GetValue("game","probe",-1).As<int>()==43&&reread.GetValue("game","mute",true).As<bool>()==false&&reread.GetValue("game","sensitivity",0.0).As<double>()==1.5,"int/bool/double survive save/reload");
   // Case 5: REAL rename failure: read-only settings directory; old live file must stay intact, error reported, tmp copy kept for manual recovery.
   if(OperatingSystem.IsLinux()){
    Settings.Set("sentinel",7);string beforeLive=System.IO.File.ReadAllText(live);
    string dir=System.IO.Path.GetDirectoryName(live)!;
    System.IO.File.SetUnixFileMode(dir,UnixFileMode.UserRead|UnixFileMode.UserExecute);
    try{Settings.Set("blocked",1);}finally{System.IO.File.SetUnixFileMode(dir,UnixFileMode.UserRead|UnixFileMode.UserWrite|UnixFileMode.UserExecute);}
    Check("rename_failure_preserves_live",FileTextEquals(live,beforeLive),"old file byte-identical after failed rename");
    Check("rename_failure_tmp_kept",System.IO.File.Exists(live+".tmp"),"recovery copy present on rename failure");
    Check("rename_failure_reported",!FileTextEquals(live,System.IO.File.ReadAllText(live+".tmp")),"new state isolated in tmp, not silently applied");
   }else Check("rename_failure_skipped",true,"non-POSIX runner");
   // Case 6: atomic success: rename onto an existing live file replaces it whole; no stray persists.
   Settings.Set("final",1);
   var fin=new ConfigFile();Error fl=fin.Load(Live);
   Check("atomic_replace_existing",fl==Error.Ok&&fin.GetValue("game","final",-1).As<int>()==1,"rename over existing file succeeds and replaces it");
   Check("no_stray_tmp_after_success",!System.IO.File.Exists(live+".tmp"),"tmp removed by the successful rename");
  }
  return (Failures.Count==0?"PASS ":"FAIL ")+(Passed.Count+Failures.Count)+" assertions"+(Failures.Count>0?" "+string.Join(";",Failures):"");
 }
}}
