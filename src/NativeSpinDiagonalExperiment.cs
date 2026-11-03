using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Measured alternate diagonal: diagnostic plus explicit opt-in full-race candidate. Defaults preserve source.
public static class NativeSpinDiagonalExperiment {
 public static Vector3[] ApplyCandidate(Vector3[] source,string resource){
  if(System.Environment.GetEnvironmentVariable("MYSIM_INTERLAGOS_SPIN_DIAGONAL")!="1")return source;
  if(resource!="res://assets/Resources/brazil_circuit.txt"||System.Environment.GetEnvironmentVariable("MYSIM_INTERLAGOS_SEAM_WELD")!="2")throw new Exception("Spin diagonal candidate requires exact mode2 Interlagos source");
  var result=Build(source,Transform3D.Identity,out var evidence);
  GD.Print("INTERLAGOS_SPIN_DIAGONAL candidate ",System.Text.Json.JsonSerializer.Serialize(evidence));return result;
 }
 static double Cross(Vector3 a,Vector3 b,Vector3 c)=>(double)(b.X-a.X)*(c.Z-a.Z)-(double)(b.Z-a.Z)*(c.X-a.X);
 static double Height(Vector3 a,Vector3 b,Vector3 c,Vector3 p){double area=Cross(a,b,c);if(Math.Abs(area)<1e-9)throw new Exception("Degenerate projected spin face");double u=Cross(p,b,c)/area,v=Cross(a,p,c)/area;return u*a.Y+v*b.Y+(1-u-v)*c.Y;}
 public static Vector3[] Build(Vector3[] source,Transform3D transform,out object evidence){
  if(source.Length!=25796*3)throw new Exception("Exact mode2 spin road required");
  var a=new[]{transform*source[3354*3],transform*source[3354*3+1],transform*source[3354*3+2]};
  var b=new[]{transform*source[3355*3],transform*source[3355*3+1],transform*source[3355*3+2]};
  var shared=new List<Vector3>();Vector3 left=default,right=default;int uniqueA=0,uniqueB=0;
  foreach(var p in a){if(Array.IndexOf(b,p)>=0)shared.Add(p);else{left=p;uniqueA++;}}
  foreach(var p in b)if(Array.IndexOf(a,p)<0){right=p;uniqueB++;}
  if(shared.Count!=2||uniqueA!=1||uniqueB!=1||Math.Abs(shared[0].DistanceTo(shared[1])-13.650729f)>.001f)throw new Exception("Measured spin diagonal identity changed");
  var s=shared[0];var t=shared[1];double side1=Cross(s,t,left),side2=Cross(s,t,right),other1=Cross(left,right,s),other2=Cross(left,right,t);
  if(side1*side2>=0||other1*other2>=0)throw new Exception("Spin quad not strictly convex");
  // The two piecewise planar surfaces agree on their perimeter. Their maximum height
  // difference occurs at the intersection of old/new diagonals (all other overlay vertices agree).
  double denominator=Cross(s,t,right)-Cross(s,t,left);double fraction=-side1/denominator;
  var intersection=left.Lerp(right,(float)fraction);
  double error=Math.Abs(Height(a[0],a[1],a[2],intersection)-Height(left,right,s,intersection));
  if(!double.IsFinite(error)||error>.01)throw new Exception("Spin alternate diagonal exceeds diagnostic 10mm support bound");
  var first=new[]{left,right,s};var second=new[]{right,left,t};
  double sign=Cross(a[0],a[1],a[2]);if(sign*Cross(b[0],b[1],b[2])<=0)throw new Exception("Source spin winding differs");
  if(sign*Cross(first[0],first[1],first[2])<0)Array.Reverse(first);
  if(sign*Cross(second[0],second[1],second[2])<0)Array.Reverse(second);
  double areaBefore=Math.Abs(sign)+Math.Abs(Cross(b[0],b[1],b[2]));double areaAfter=Math.Abs(Cross(first[0],first[1],first[2]))+Math.Abs(Cross(second[0],second[1],second[2]));
  if(Math.Abs(areaAfter-areaBefore)>.0001)throw new Exception("Spin quad projected area changed");
  var output=(Vector3[])source.Clone();for(int j=0;j<3;j++){output[3354*3+j]=transform.AffineInverse()*first[j];output[3355*3+j]=transform.AffineInverse()*second[j];}
  evidence=new{sourceFaces=new[]{3354,3355},sourceVerticesRetained=4,boundaryPreserved=true,trianglesPreserved=true,maximumSupportHeightDifferenceMetres=error,projectedAreaError=Math.Abs(areaAfter-areaBefore)*.5,scope="Alternate-diagonal isolated hypothesis only; nonzero surface change explicitly bounded, not gameplay repair"};return output;
 }
}
}
