using Godot;
using System;
using System.Collections.Generic;
namespace MySim {
// New reconstructed lens art, not a restored source material. Only a measured open planar rim
// can be filled. Caller supplies one exact reflector component, never a whole batched pole.
public static class MeasuredFixtureLens {
 static string K(Vector3 p)=>BitConverter.SingleToInt32Bits(p.X)+":"+BitConverter.SingleToInt32Bits(p.Y)+":"+BitConverter.SingleToInt32Bits(p.Z);
 static float[] V(Vector3 p)=>new[]{p.X,p.Y,p.Z};
 public static bool TryCreate(Vector3[] worldTriangles,Vector3 roadTarget,PhysicsDirectSpaceState3D space,Node3D parent,float sourceMargin,out Vector3 emitter,out Vector3 normal,out object evidence){
  emitter=normal=Vector3.Zero;evidence=new{accepted=false,reason="no-measured-open-rim"};
  if(System.Environment.GetEnvironmentVariable("MYSIM_RECONSTRUCTED_FIXTURE_LENS")!="1")return false;
  var vertices=new Dictionary<string,Vector3>();var edges=new Dictionary<string,(string a,string b,int count)>();
  for(int i=0;i+2<worldTriangles.Length;i+=3)for(int corner=0;corner<3;corner++){var a=worldTriangles[i+corner];var b=worldTriangles[i+(corner+1)%3];string ka=K(a),kb=K(b);vertices[ka]=a;vertices[kb]=b;if(ka==kb)continue;string edge=StringComparer.Ordinal.Compare(ka,kb)<0?ka+"|"+kb:kb+"|"+ka;if(edges.TryGetValue(edge,out var item))edges[edge]=(item.a,item.b,item.count+1);else edges[edge]=(ka,kb,1);}
  var adjacent=new Dictionary<string,List<string>>();foreach(var edge in edges.Values)if(edge.count==1){if(!adjacent.ContainsKey(edge.a))adjacent[edge.a]=new();if(!adjacent.ContainsKey(edge.b))adjacent[edge.b]=new();adjacent[edge.a].Add(edge.b);adjacent[edge.b].Add(edge.a);}
  foreach(var neighbours in adjacent.Values)if(neighbours.Count!=2){evidence=new{accepted=false,reason="nonmanifold-or-branched-rim"};return false;}
  var remaining=new SortedSet<string>(adjacent.Keys,StringComparer.Ordinal);var loops=new List<List<Vector3>>();while(remaining.Count>0){string start=remaining.Min,at=start,previous=null;var loop=new List<Vector3>();do{loop.Add(vertices[at]);remaining.Remove(at);var neighbours=adjacent[at];string next=neighbours[0]==previous?neighbours[1]:neighbours[0];previous=at;at=next;if(loop.Count>256){evidence=new{accepted=false,reason="rim-over-256-vertices"};return false;}}while(at!=start);loops.Add(loop);}
  loops.Sort((a,b)=>Perimeter(b).CompareTo(Perimeter(a)));
  foreach(var loop in loops){if(loop.Count<3)continue;var centre=Vector3.Zero;foreach(var point in loop)centre+=point;centre/=loop.Count;var n=Vector3.Zero;for(int i=0;i<loop.Count;i++)n+=(loop[i]-centre).Cross(loop[(i+1)%loop.Count]-centre);if(n.LengthSquared()<1e-10f)continue;n=n.Normalized();if(n.Dot(roadTarget-centre)<0)n=-n;
   float planarError=0,radius=0;bool convex=true;for(int i=0;i<loop.Count;i++){planarError=Mathf.Max(planarError,Mathf.Abs((loop[i]-centre).Dot(n)));radius=Mathf.Max(radius,loop[i].DistanceTo(centre));var a=loop[i]-centre;var b=loop[(i+1)%loop.Count]-centre;if(a.Cross(b).Dot(n)<-1e-5f)convex=false;}
   // A backwards ordered loop has the same valid geometry; reverse once then check convexity.
   if(!convex){loop.Reverse();convex=true;for(int i=0;i<loop.Count;i++)if((loop[i]-centre).Cross(loop[(i+1)%loop.Count]-centre).Dot(n)<-1e-5f)convex=false;}
   if(!convex||planarError>.02f||radius<.12f||radius>1.5f||n.Dot((roadTarget-centre).Normalized())<.2f)continue;
   var candidate=centre+n*(sourceMargin+.10f);using var sphere=new PhysicsShapeQueryParameters3D{Shape=new SphereShape3D{Radius=.08f},Transform=new Transform3D(Basis.Identity,candidate),CollisionMask=2};var overlaps=space.IntersectShape(sphere,1);using var query=PhysicsRayQueryParameters3D.Create(candidate,roadTarget);query.CollisionMask=2;query.HitBackFaces=true;var hit=space.IntersectRay(query);if(overlaps.Count>0||hit.Count>0){evidence=new{accepted=false,reason="measured-aperture-still-obstructed",rimVertices=loop.Count,centre=V(centre),normal=V(n),planarError,radius,sourceMargin,sphereBlocked=overlaps.Count>0,lineBlocked=hit.Count>0,blocker=hit.Count>0?(hit["collider"].AsGodotObject() as Node)?.GetPath().ToString():null};continue;}
   using var tool=new SurfaceTool();tool.Begin(Mesh.PrimitiveType.Triangles);tool.SetMaterial(new StandardMaterial3D{AlbedoColor=new Color(.75f,.79f,.82f),Roughness=.25f,EmissionEnabled=true,Emission=new Color(.75f,.79f,.82f),EmissionEnergyMultiplier=1.5f,CullMode=BaseMaterial3D.CullModeEnum.Disabled});for(int i=0;i<loop.Count;i++){tool.SetNormal(n);tool.AddVertex(parent.ToLocal(centre+n*.002f));tool.AddVertex(parent.ToLocal(loop[(i+1)%loop.Count]+n*.002f));tool.AddVertex(parent.ToLocal(loop[i]+n*.002f));}parent.AddChild(new MeshInstance3D{Name="ReconstructedMeasuredLens",Mesh=tool.Commit(),CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});emitter=candidate;normal=n;evidence=new{accepted=true,productionValidated=false,newReconstructedGeometry=true,rimVertices=loop.Count,centre=V(centre),normal=V(n),planarError,radius,sourceMargin,emitter=V(candidate),scope="Measured open planar reflector rim filled by new lens art; full existing-world clearance/LOS retained; requires actual night rendering"};return true;
  }return false;
 }
 static float Perimeter(List<Vector3> loop){float total=0;for(int i=0;i<loop.Count;i++)total+=loop[i].DistanceTo(loop[(i+1)%loop.Count]);return total;}
}
}
