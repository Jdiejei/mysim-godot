using System;
namespace MySimulator.Rebuild {
// Source-part identity locates damage independently of deceleration direction.
// Repeated manifold points must not multiply a region's share of one event.
public static class ImpactDamageLocalization {
 public static int Region(string sourcePart,int fallback){
  string name=(sourcePart??"").ToUpperInvariant();
  if(name.Contains("FRONTBUMPER"))return 0;
  if(name.Contains("REARBUMPER"))return 1;
  if(name.Contains("DOOR_LEFT"))return 2;
  if(name.Contains("DOOR_RIGHT"))return 3;
  return fallback>=0&&fallback<7?fallback:6;
 }
 public static float[] Shares(string[] parts,int fallback){
  var result=new float[7];int distinct=0;
  foreach(string part in parts){int region=Region(part,fallback);if(result[region]==0){result[region]=1;distinct++;}}
  if(distinct==0){result[Region(null,fallback)]=1;distinct=1;}
  for(int i=0;i<result.Length;i++)result[i]/=distinct;
  return result;
 }
}
}
