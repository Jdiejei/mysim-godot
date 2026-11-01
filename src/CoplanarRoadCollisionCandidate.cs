using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// Explicit generated-collision candidate. No source resource is edited. Only convex, hole-free,
// edge-connected planar islands may become slabs; all rejected faces remain exact source triangles.
public static class CoplanarRoadCollisionCandidate {
 const float PlaneTolerance=.0001f,Thickness=.05f;
 public sealed class Result {
  public StaticBody3D Body;public int InputTriangles,SlabTriangles,Slabs,RetainedTriangles,RejectedIslands;public float MaximumPlaneError;public readonly List<object> Islands=new();
  public object Evidence()=>new{InputTriangles,SlabTriangles,Slabs,RetainedTriangles,RejectedIslands,MaximumPlaneError,Islands,planeTolerance=PlaneTolerance,thickness=Thickness,scope="Opt-in generated collision only: convex hole-free connected planar islands retain original top vertices; slab adds volume strictly below support surface. Actual seam, boundary, underside and race contact verification required."};
 }
 readonly struct Edge : IEquatable<Edge> {
  public readonly Vector3 a,b;public Edge(Vector3 x,Vector3 y){bool first=x.X<y.X||x.X==y.X&&(x.Y<y.Y||x.Y==y.Y&&x.Z<=y.Z);a=first?x:y;b=first?y:x;}
  public bool Equals(Edge other)=>a==other.a&&b==other.b;public override bool Equals(object o)=>o is Edge e&&Equals(e);public override int GetHashCode()=>HashCode.Combine(a,b);
 }
 static float Cross(Vector2 o,Vector2 a,Vector2 b)=>(a-o).Cross(b-o);
 static List<Vector2> Hull(List<Vector2> points){points.Sort((a,b)=>a.X!=b.X?a.X.CompareTo(b.X):a.Y.CompareTo(b.Y));var hull=new List<Vector2>();foreach(var p in points){while(hull.Count>=2&&Cross(hull[^2],hull[^1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}int lower=hull.Count;for(int i=points.Count-2;i>=0;i--){var p=points[i];while(hull.Count>lower&&Cross(hull[^2],hull[^1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}if(hull.Count>0)hull.RemoveAt(hull.Count-1);return hull;}
 public static Result Build(Vector3[] source){
  if(source==null||source.Length%3!=0)throw new ArgumentException("Road triangle list incomplete");int count=source.Length/3;var result=new Result{Body=new StaticBody3D{Name="DriveSurface",CollisionLayer=1,CollisionMask=4},InputTriangles=count};
  var normals=new Vector3[count];var adjacent=new List<int>[count];var edgeFaces=new Dictionary<Edge,List<int>>();for(int i=0;i<count;i++){adjacent[i]=new();var a=source[3*i];var b=source[3*i+1];var c=source[3*i+2];if(!a.IsFinite()||!b.IsFinite()||!c.IsFinite())throw new Exception("Nonfinite road source");var normal=(b-a).Cross(c-a);normals[i]=normal.LengthSquared()>.0000000001f?normal.Normalized():Vector3.Zero;if(normals[i].Y<0)normals[i]=-normals[i];foreach(var edge in new[]{new Edge(a,b),new Edge(b,c),new Edge(c,a)}){if(!edgeFaces.TryGetValue(edge,out var faces)){faces=new();edgeFaces[edge]=faces;}faces.Add(i);}}
  foreach(var faces in edgeFaces.Values)if(faces.Count==2){int a=faces[0],b=faces[1];if(normals[a].Y<.8f||normals[b].Y<.8f||normals[a].Dot(normals[b])<.999999f)continue;bool planar=true;for(int j=0;j<3;j++)if(Mathf.Abs(normals[a].Dot(source[3*b+j]-source[3*a]))>PlaneTolerance)planar=false;if(planar){adjacent[a].Add(b);adjacent[b].Add(a);}}
  var visited=new bool[count];var retained=new List<Vector3>();for(int seed=0;seed<count;seed++){
   if(visited[seed])continue;var island=new List<int>();var todo=new Stack<int>();todo.Push(seed);visited[seed]=true;while(todo.Count>0){int face=todo.Pop();island.Add(face);foreach(int next in adjacent[face])if(!visited[next]){visited[next]=true;todo.Push(next);}}
   var unique=new HashSet<Vector3>();float sourceArea=0,maxPlaneError=0;var normal=normals[seed];var origin=source[3*seed];foreach(int face in island){var a=source[3*face];var b=source[3*face+1];var c=source[3*face+2];sourceArea+=Mathf.Abs(new Vector2(b.X-a.X,b.Z-a.Z).Cross(new Vector2(c.X-a.X,c.Z-a.Z)))*.5f;foreach(var p in new[]{a,b,c}){unique.Add(p);maxPlaneError=Mathf.Max(maxPlaneError,Mathf.Abs(normal.Dot(p-origin)));}}
   var projected=new List<Vector2>();foreach(var p in unique)projected.Add(new Vector2(p.X-origin.X,p.Z-origin.Z));var boundary=Hull(projected);float hullArea=0;for(int i=0;i<boundary.Count;i++)hullArea+=boundary[i].Cross(boundary[(i+1)%boundary.Count]);hullArea=Mathf.Abs(hullArea)*.5f;
   // Area equality alone is insufficient for arbitrary overlapping inputs: also require each
   // non-boundary edge exactly twice and one closed boundary loop, rejecting nonmanifold seams.
   var localEdges=new Dictionary<Edge,int>();foreach(int face in island){var a=source[3*face];var b=source[3*face+1];var c=source[3*face+2];foreach(var edge in new[]{new Edge(a,b),new Edge(b,c),new Edge(c,a)}){localEdges.TryGetValue(edge,out int n);localEdges[edge]=n+1;}}
   bool manifold=true;int boundaryEdges=0;foreach(int n in localEdges.Values){if(n==1)boundaryEdges++;else if(n!=2)manifold=false;}
   var boundaryGraph=new Dictionary<Vector3,List<Vector3>>();foreach(var entry in localEdges)if(entry.Value==1){foreach(var pair in new[]{(entry.Key.a,entry.Key.b),(entry.Key.b,entry.Key.a)}){if(!boundaryGraph.TryGetValue(pair.Item1,out var neighbours)){neighbours=new();boundaryGraph[pair.Item1]=neighbours;}neighbours.Add(pair.Item2);}}
   foreach(var neighbours in boundaryGraph.Values)if(neighbours.Count!=2)manifold=false;
   var reached=new HashSet<Vector3>();if(boundaryGraph.Count>0){var stack=new Stack<Vector3>();foreach(var point in boundaryGraph.Keys){stack.Push(point);break;}while(stack.Count>0){var point=stack.Pop();if(!reached.Add(point))continue;foreach(var neighbour in boundaryGraph[point])stack.Push(neighbour);}}if(reached.Count!=boundaryGraph.Count)manifold=false;
   bool eligible=island.Count>=2&&normal.Y>=.8f&&maxPlaneError<=PlaneTolerance&&hullArea>.001f&&Mathf.Abs(hullArea-sourceArea)<=Mathf.Max(.0001f,hullArea*.00001f)&&manifold&&boundaryEdges>=3;
   result.Islands.Add(new{sourceTriangles=island.Count,uniqueVertices=unique.Count,sourceProjectedArea=sourceArea,convexBoundaryProjectedArea=hullArea,areaDifference=Mathf.Abs(hullArea-sourceArea),maximumPlaneError=maxPlaneError,boundaryEdges,boundaryVertices=boundaryGraph.Count,singleManifoldLoop=manifold,eligible,edgeCaveat="Finite slab has vertical boundary faces below the original top edge; transitions to retained/noncoplanar triangles are not accepted for production and require seam-contact replay."});
   if(!eligible){result.RejectedIslands++;foreach(int face in island)for(int j=0;j<3;j++)retained.Add(source[3*face+j]);continue;}
   var centre=Vector3.Zero;foreach(var p in unique)centre+=p;centre/=unique.Count;var points=new List<Vector3>();foreach(var p in unique){points.Add(p-centre);points.Add(p-centre-Vector3.Up*Thickness);}
   var shape=new ConvexPolygonShape3D{Margin=0,Points=points.ToArray()};result.Body.AddChild(new CollisionShape3D{Name="PlanarRoadSlab_"+result.Slabs,Position=centre,Shape=shape});result.Slabs++;result.SlabTriangles+=island.Count;result.MaximumPlaneError=Mathf.Max(result.MaximumPlaneError,maxPlaneError);
  }
  result.RetainedTriangles=retained.Count/3;if(retained.Count>0)result.Body.AddChild(new CollisionShape3D{Name="UnchangedNonplanarRoad",Shape=new ConcavePolygonShape3D{Data=retained.ToArray(),BackfaceCollision=true}});
  if(result.SlabTriangles+result.RetainedTriangles!=result.InputTriangles)throw new Exception("Road candidate triangle accounting failed");return result;
 }
}
}
