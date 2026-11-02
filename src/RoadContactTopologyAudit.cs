using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MySim {
// Numeric source-feature evidence only. No source vertices exported and no topology modified.
public static class RoadContactTopologyAudit {
 readonly struct Edge:IEquatable<Edge>{public readonly Vector3 A,B;public Edge(Vector3 a,Vector3 b){bool first=a.X<b.X||a.X==b.X&&(a.Y<b.Y||a.Y==b.Y&&a.Z<=b.Z);A=first?a:b;B=first?b:a;}public bool Equals(Edge e)=>A==e.A&&B==e.B;public override bool Equals(object o)=>o is Edge e&&Equals(e);public override int GetHashCode()=>HashCode.Combine(A,B);}
 static float[] A(Vector3 p)=>new[]{p.X,p.Y,p.Z};static Vector3 V(JsonElement p)=>new(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle());
 static float Distance(Vector3 p,Edge e,out float fraction){var direction=e.B-e.A;fraction=Mathf.Clamp((p-e.A).Dot(direction)/Mathf.Max(direction.LengthSquared(),1e-12f),0,1);return p.DistanceTo(e.A+direction*fraction);}
 public static object Build(Vector3[] source,Transform3D transform,JsonElement witnesses){
  var edges=new Dictionary<Edge,List<int>>();var vertices=new HashSet<Vector3>();var normals=new List<Vector3>();int degenerates=0;
  for(int i=0;i+2<source.Length;i+=3){var a=transform*source[i];var b=transform*source[i+1];var c=transform*source[i+2];vertices.Add(a);vertices.Add(b);vertices.Add(c);var cross=(b-a).Cross(c-a);if(cross.LengthSquared()<1e-12f)degenerates++;var normal=cross.LengthSquared()>1e-12f?cross.Normalized():Vector3.Zero;normals.Add(normal.Y<0?-normal:normal);foreach(var edge in new[]{new Edge(a,b),new Edge(b,c),new Edge(c,a)}){if(!edges.TryGetValue(edge,out var list)){list=new();edges[edge]=list;}list.Add(i/3);}}
  int boundary=0,nonmanifold=0;foreach(var list in edges.Values){if(list.Count==1)boundary++;if(list.Count>2)nonmanifold++;}var rows=new List<object>();var seen=new HashSet<Vector3>();
  foreach(var witness in witnesses.EnumerateArray()){
   var point=V(witness.GetProperty("colliderWitness"));var contactNormal=V(witness.GetProperty("normal"));if(contactNormal.Y>=.95f||!seen.Add(point))continue;var nearest=new List<(Edge edge,float distance,float fraction)>();foreach(var edge in edges.Keys){float distance=Distance(point,edge,out float fraction);if(distance<=.25f)nearest.Add((edge,distance,fraction));}nearest.Sort((a,b)=>a.distance.CompareTo(b.distance));var features=new List<object>();
   for(int i=0;i<Math.Min(10,nearest.Count);i++){var candidate=nearest[i];var faces=edges[candidate.edge];var faceNormals=new List<float[]>();float maximumAngle=0;foreach(int f in faces){faceNormals.Add(A(normals[f]));foreach(int g in faces)maximumAngle=Mathf.Max(maximumAngle,Mathf.Acos(Mathf.Clamp(normals[f].Dot(normals[g]),-1,1)));}int exactTJunctions=0,nearTJunctions=0;float minimumInteriorDistance=float.PositiveInfinity;
    foreach(var vertex in vertices){if(vertex==candidate.edge.A||vertex==candidate.edge.B)continue;float distance=Distance(vertex,candidate.edge,out float fraction);if(fraction<=.0001f||fraction>=.9999f)continue;if(distance<=.001f){nearTJunctions++;minimumInteriorDistance=Mathf.Min(minimumInteriorDistance,distance);if(distance<=.0001f)exactTJunctions++;}}
    var adjacentGeometry=new List<object>();foreach(int face in faces){var a=transform*source[face*3];var b=transform*source[face*3+1];var c=transform*source[face*3+2];float twiceArea=(b-a).Cross(c-a).Length();float longest=Mathf.Max(a.DistanceTo(b),Mathf.Max(b.DistanceTo(c),c.DistanceTo(a)));var bounds=new Aabb(a,Vector3.Zero).Expand(b).Expand(c);adjacentGeometry.Add(new{triangle=face,area=twiceArea*.5f,longestEdge=longest,minimumAltitude=twiceArea/Mathf.Max(longest,1e-12f),verticalSpan=bounds.Size.Y,sourceNormal=A(normals[face]),scope="Read-only dimensions distinguish thin vertical seam strips from real walls; no automatic removal permitted"});}
    features.Add(new{distanceFromColliderWitness=candidate.distance,witnessEdgeFraction=candidate.fraction,edgeLength=candidate.edge.A.DistanceTo(candidate.edge.B),exactIncidence=faces.Count,sourceTriangleIndices=faces,adjacentGeometry,upwardFaceNormals=faceNormals,maximumAdjacentNormalAngleRadians=maximumAngle,interiorVertexCandidatesWithin01mm=exactTJunctions,interiorVertexCandidatesWithin1mm=nearTJunctions,minimumInteriorVertexDistance=float.IsFinite(minimumInteriorDistance)?(float?)minimumInteriorDistance:null});
   }
   rows.Add(new{colliderWitness=A(point),capturedContactNormal=A(contactNormal),nearestSourceEdges=features,scope="Nearest source edge geometry is not proof of the collision subtriangle selected by Jolt. T-junction candidates are distance tests, not verified connectivity defects."});
  }
  return new{sourceTriangles=source.Length/3,sourceUniqueVertices=vertices.Count,sourceBoundaryEdges=boundary,sourceNonmanifoldEdges=nonmanifold,sourceDegenerateTriangles=degenerates,obliqueWitnesses=rows,scope="Read-only whole source mesh exact-edge incidence and local T-junction candidate audit; no welding, source edits or raw vertices exported."};
 }
}
}
