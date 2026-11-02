using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Explicit source-specific collision candidate. Original files/visuals remain unchanged.
public static class MeasuredInterlagosSeam {
 public static Vector3[] Apply(Vector3[] source,string path){
  string mode=System.Environment.GetEnvironmentVariable("MYSIM_INTERLAGOS_SEAM_WELD");
  if(mode!="1"&&mode!="2")return source;
  if(path!="res://assets/Resources/brazil_circuit.txt")return source;
  if(source.Length!=25798*3)throw new Exception("Measured Interlagos seam source triangle count changed");
  int at=10946*3;var triangle=new[]{source[at],source[at+1],source[at+2]};int left=0,right=1;float distance=triangle[0].DistanceTo(triangle[1]);for(int a=0;a<3;a++)for(int b=a+1;b<3;b++)if(triangle[a].DistanceTo(triangle[b])<distance){left=a;right=b;distance=triangle[a].DistanceTo(triangle[b]);}
  float area=(triangle[1]-triangle[0]).Cross(triangle[2]-triangle[0]).Length()*.5f;
  if(Mathf.Abs(distance-.0009765625f)>.000001f||Mathf.Abs(area-.008282375f)>.00001f||Mathf.Abs((triangle[1]-triangle[0]).Cross(triangle[2]-triangle[0]).Normalized().Y)>.001f)throw new Exception("Measured Interlagos seam identity differs");
  var midpoint=(triangle[left]+triangle[right])*.5f;Vector3 Weld(Vector3 p)=>p==triangle[left]||p==triangle[right]?midpoint:p;
  var result=new List<Vector3>();int moved=0,removed=0;for(int i=0;i<source.Length;i+=3){var a=Weld(source[i]);var b=Weld(source[i+1]);var c=Weld(source[i+2]);if(a!=source[i])moved++;if(b!=source[i+1])moved++;if(c!=source[i+2])moved++;if(i/3==10946){if(a!=b&&b!=c&&a!=c)throw new Exception("Measured seam triangle did not collapse");removed++;continue;}result.Add(a);result.Add(b);result.Add(c);}
  if(removed!=1||moved!=7)throw new Exception("Measured seam weld unexpected counts removed="+removed+" moved="+moved);
  GD.Print("INTERLAGOS_MEASURED_SEAM candidate movedOccurrences=",moved," degenerateFacesRemoved=",removed," maxDisplacement=",distance*.5f," originalTriangles=",source.Length/3," outputTriangles=",result.Count/3);
  if(mode=="2"){
   var first=result.ToArray();const int face=10950;var v=new[]{first[face*3],first[face*3+1],first[face*3+2]};int l=0,r=1;float d=v[0].DistanceTo(v[1]);for(int x=0;x<3;x++)for(int y=x+1;y<3;y++)if(v[x].DistanceTo(v[y])<d){l=x;r=y;d=v[x].DistanceTo(v[y]);}
   var cross=(v[1]-v[0]).Cross(v[2]-v[0]);if(first.Length!=25797*3||Mathf.Abs(d-.0009765625f)>.000001f||Mathf.Abs(cross.Length()*.5f-.008289062f)>.00001f||Mathf.Abs(cross.Normalized().Y)>.001f)throw new Exception("Second measured Interlagos seam identity differs");
   var mid=(v[l]+v[r])*.5f;Vector3 W(Vector3 q)=>q==v[l]||q==v[r]?mid:q;result.Clear();int secondMoved=0;
   for(int i=0;i<first.Length;i+=3){var a=W(first[i]);var b=W(first[i+1]);var c=W(first[i+2]);if(a!=first[i])secondMoved++;if(b!=first[i+1])secondMoved++;if(c!=first[i+2])secondMoved++;if(i/3==face){if(a!=b&&b!=c&&a!=c)throw new Exception("Second measured seam did not collapse");continue;}result.Add(a);result.Add(b);result.Add(c);}
   if(secondMoved<3||result.Count!=25796*3)throw new Exception("Second measured seam output differs");
   GD.Print("INTERLAGOS_SECOND_MEASURED_SEAM candidate movedOccurrences=",secondMoved," degenerateFacesRemoved=1 maxDisplacement=",d*.5f," originalTriangles=",first.Length/3," outputTriangles=",result.Count/3);
  }
  return result.ToArray();
 }
}
}
