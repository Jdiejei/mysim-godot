using System;using System.Collections.Generic;using System.IO;using System.Linq;using MySimulator.Rebuild;
// AI lap matrix: every (track, car) pair runs a headless Race for up to 400 simulated seconds; checks finiteness, progress and lap completion.
class P{static int Main(string[] a){int shard=int.Parse(a[0]),shards=int.Parse(a[1]);int tracks=SourceRoutes.Names.Length,cars=CarCatalog.Count;int total=0,fail=0,pass=0;var lines=new List<string>();
 for(int t=0;t<tracks;t++)for(int c=0;c<cars;c++){if((t*cars+c)%shards!=shard)continue;total++;string why=null;float lapTime=-1,prog=0;
  try{var r=new Race(t,1,0f);var v=r.Add("AI",false,Compound.Medium);v.carModel=c;r.countdown=0;r.started=true;v.Place(r.track,0.988f,0f);float dt=1f/30f;float t0=0;
   for(int i=0;i<12000&&!v.finished;i++){r.Step(dt);if(!float.IsFinite(v.position.x)||!float.IsFinite(v.position.z)||!float.IsFinite(v.speed)){why="non-finite state";break;}if(v.speed>130){why="speed>130";break;}}
   prog=v.progress;if(why==null&&!v.finished)why="lap not finished in 400s progress="+v.progress.ToString("0.00");lapTime=v.finishTime;}
  catch(Exception e){why="exception "+e.GetType().Name+": "+e.Message;}
  if(why==null)pass++;else fail++;lines.Add($"{{\"track\":{t},\"car\":{c},\"ok\":{(why==null?"true":"false")},\"lap\":{lapTime:0.0},\"why\":\"{why}\"}}");}
 File.WriteAllLines($"laps-{shard}.jsonl",lines);Console.WriteLine($"shard {shard}/{shards}: {total} runs, {pass} pass, {fail} fail");foreach(var l in lines.Where(x=>x.Contains("\"ok\":false")).Take(15))Console.WriteLine(l);return 0;}}
