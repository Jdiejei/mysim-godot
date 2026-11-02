using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Explicit source-specific collision candidate. Original files/visuals remain unchanged.
public static class MeasuredInterlagosSeam {
 public static Vector3[] Apply(Vector3[] source,string path){
  if(System.Environment.GetEnvironmentVariable("MYSIM_INTERLAGOS_SEAM_WELD")!="1")return source;
  if(path!="res://assets/Resources/brazil_circuit.txt")return source;
  if(source.Length!=25798*3)throw new Exception("Measured Interlagos seam source triangle count changed");
  int at=10946*3;var triangle=new[]{source[at],source[at+1],source[at+2]};int left=0,right=1;float distance=triangle[0].DistanceTo(triangle[1]);for(int a=0;a<3;a++)for(int b=a+1;b<3;b++)if(triangle[a].DistanceTo(triangle[b])<distance){left=a;right=b;distance=triangle[a].DistanceTo(triangle[b]);}
  float area=(triangle[1]-triangle[0]).Cross(triangle[2]-triangle[0]).Length()*.5f;
  if(Mathf.Abs(distance-.0009765625f)>.000001f||Mathf.Abs(area-.008282375f)>.00001f||Mathf.Abs((triangle[1]-triangle[0]).Cross(triangle[2]-triangle[0]).Normalized().Y)>.001f)throw new Exception("Measured Interlagos seam identity differs");
  var midpoint=(triangle[left]+triangle[right])*.5f;Vector3 Weld(Vector3 p)=>p==triangle[left]||p==triangle[right]?midpoint:p;
  var result=new List<Vector3>();int moved=0,removed=0;for(int i=0;i<source.Length;i+=3){var a=Weld(source[i]);var b=Weld(source[i+1]);var c=Weld(source[i+2]);if(a!=source[i])moved++;if(b!=source[i+1])moved++;if(c!=source[i+2])moved++;if(a==b||b==c||a==c){removed++;continue;}result.Add(a);result.Add(b);result.Add(c);}
  if(removed<1||moved<3)throw new Exception("Measured seam weld did not affect expected connected faces");
  GD.Print("INTERLAGOS_MEASURED_SEAM candidate movedOccurrences=",moved," degenerateFacesRemoved=",removed," maxDisplacement=",distance*.5f," originalTriangles=",source.Length/3," outputTriangles=",result.Count/3);
  return result.ToArray();
 }
}
}
