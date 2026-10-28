using System;using System.IO;using System.Text.Json;
namespace MySimulator.Rebuild {
// Host AI planning policy; never changes human tyre forces or recovered tuning values. Ported off UnityEngine.
public static class AiRaceCalibration {
 public class Entry{public int track{get;set;}public int car{get;set;}public float scale{get;set;}=1;}
 public class Table{public Entry[] entries{get;set;}}
 public static string DataDir=Path.Combine(AppContext.BaseDirectory,"data");
 static float[,] scales;
 static float Clamp(float v,float lo,float hi)=>v<lo?lo:v>hi?hi:v;
 static void Load(){scales=new float[SourceRoutes.Names.Length,CarCatalog.Count];for(int t=0;t<scales.GetLength(0);t++)for(int c=0;c<scales.GetLength(1);c++)scales[t,c]=1;
  string path=Path.Combine(DataDir,"ai-calibration.json");if(!File.Exists(path))return;
  try{var data=JsonSerializer.Deserialize<Table>(File.ReadAllText(path));foreach(var e in data.entries)if(e.track>=0&&e.track<scales.GetLength(0)&&e.car>=0&&e.car<scales.GetLength(1)&&!float.IsNaN(e.scale)&&!float.IsInfinity(e.scale))scales[e.track,e.car]=Clamp(e.scale,.35f,1);}catch(Exception e){Console.Error.WriteLine("AI calibration unavailable: "+e.Message);}}
 public static float Scale(int car,int track){if(scales==null)Load();return scales[(int)Clamp(track,0,scales.GetLength(0)-1),(int)Clamp(car,0,scales.GetLength(1)-1)];}
}}
