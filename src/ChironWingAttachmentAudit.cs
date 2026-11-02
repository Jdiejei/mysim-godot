using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MySim {
// Read-only source attachment evidence. Socket candidates are labelled hypotheses, never mounted parts.
// All geometry-derived output must remain in encrypted cloud review artifacts.
public static class ChironWingAttachmentAudit {
 readonly struct Edge:IEquatable<Edge>{public readonly Vector3 A,B;public Edge(Vector3 a,Vector3 b){bool first=a.X<b.X||a.X==b.X&&(a.Y<b.Y||a.Y==b.Y&&a.Z<=b.Z);A=first?a:b;B=first?b:a;}public bool Equals(Edge e)=>A==e.A&&B==e.B;public override bool Equals(object o)=>o is Edge e&&Equals(e);public override int GetHashCode()=>HashCode.Combine(A,B);}
 static float[] A(Vector3 p)=>new[]{p.X,p.Y,p.Z};
 static void Marker(Node3D car,Vector3 point,string label,Color colour){
  var group=new Node3D{Name="AttachmentAudit_"+label,Position=point};car.AddChild(group);group.AddChild(new MeshInstance3D{Name="AuditMarker",Mesh=new SphereMesh{Radius=.018f,Height=.036f,RadialSegments=12,Rings=6},MaterialOverride=new StandardMaterial3D{AlbedoColor=colour,ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded}});group.AddChild(new Label3D{Text=label,Position=Vector3.Up*.045f,FontSize=32,PixelSize=.0015f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=colour,NoDepthTest=true});
 }
 public static void Write(Node3D car,int index,string path,bool showLabels=true){
  if(index!=20||car.Name.ToString()!="2017_bugatti_chiron")throw new InvalidOperationException("Wing socket audit requires actual car20 Chiron source");
  var inverse=car.GlobalTransform.AffineInverse();var groups=new SortedDictionary<string,List<Vector3>>();
  foreach(var child in car.FindChildren("*","MeshInstance3D",true,false)){
   var mesh=(MeshInstance3D)child;if(!mesh.IsVisibleInTree()||mesh.Mesh==null)continue;string name=mesh.Name.ToString();int suffix=name.IndexOf("_mm_",StringComparison.Ordinal);string role=suffix>=0?name.Substring(0,suffix):name;if(role!="LOD_A_BODY"&&role!="LOD_A_BOOT"&&!role.StartsWith("LOD_A_WING",StringComparison.Ordinal))continue;if(!groups.TryGetValue(role,out var list)){list=new();groups[role]=list;}var transform=inverse*mesh.GlobalTransform;foreach(var vertex in mesh.Mesh.GetFaces())list.Add(transform*vertex);
  }
  foreach(string role in new[]{"LOD_A_BODY","LOD_A_BOOT","LOD_A_WING","LOD_A_WING_STRUT","LOD_A_WING_STRUT_REAR"})if(!groups.ContainsKey(role))throw new Exception("Required source attachment role missing:"+role);
  var parts=new List<object>();var loops=new List<object>();var extremities=new List<object>();int marker=0;
  foreach(var group in groups){
   var points=group.Value;var bounds=new Aabb(points[0],Vector3.Zero);var unique=new HashSet<Vector3>();var edges=new Dictionary<Edge,int>();for(int i=0;i+2<points.Count;i+=3){for(int j=0;j<3;j++){var p=points[i+j];unique.Add(p);bounds=bounds.Expand(p);var edge=new Edge(p,points[i+(j+1)%3]);if(edge.A==edge.B)continue;edges.TryGetValue(edge,out int count);edges[edge]=count+1;}}
   int nonmanifold=0,boundaryCount=0;var graph=new Dictionary<Vector3,List<Vector3>>();foreach(var entry in edges){if(entry.Value>2)nonmanifold++;if(entry.Value!=1)continue;boundaryCount++;foreach(var pair in new[]{(entry.Key.A,entry.Key.B),(entry.Key.B,entry.Key.A)}){if(!graph.TryGetValue(pair.Item1,out var neighbours)){neighbours=new();graph[pair.Item1]=neighbours;}neighbours.Add(pair.Item2);}}
   parts.Add(new{role=group.Key,triangles=points.Count/3,uniqueVertices=unique.Count,boundsMin=A(bounds.Position),boundsMax=A(bounds.End),centre=A(bounds.GetCenter()),boundaryEdges=boundaryCount,nonmanifoldEdges=nonmanifold});
   if(group.Key.StartsWith("LOD_A_WING",StringComparison.Ordinal)){
    // Six measured extreme bands expose intact part tips, without guessing which end is a mount.
    for(int axis=0;axis<3;axis++)foreach(bool maximum in new[]{false,true}){float extreme=maximum?bounds.End[axis]:bounds.Position[axis];var average=Vector3.Zero;int count=0;foreach(var p in unique)if(Mathf.Abs(p[axis]-extreme)<=.01f){average+=p;count++;}if(count==0)continue;average/=count;string label="W"+marker++;extremities.Add(new{label,role=group.Key,axis,maximum,bandMetres=.01f,samples=count,centroid=A(average),isVerifiedMount=false});if(showLabels)Marker(car,average,label,new Color(1,.55f,.1f));}
   }
   if(group.Key!="LOD_A_BODY"&&group.Key!="LOD_A_BOOT")continue;
   var seen=new HashSet<Vector3>();foreach(var first in graph.Keys){if(!seen.Add(first))continue;var pending=new Stack<Vector3>();pending.Push(first);var component=new List<Vector3>();bool closed=true;while(pending.Count>0){var point=pending.Pop();component.Add(point);if(graph[point].Count!=2)closed=false;foreach(var neighbour in graph[point])if(seen.Add(neighbour))pending.Push(neighbour);}var box=new Aabb(component[0],Vector3.Zero);var centre=Vector3.Zero;foreach(var p in component){box=box.Expand(p);centre+=p;}centre/=component.Count;
    // Keep all numeric topology evidence. Label only plausible rear/deck-sized components to avoid a sea of labels.
    bool rearDeck=centre.Z<-.5f&&centre.Y>.55f&&box.Size.X<.6f&&box.Size.Z<.6f&&component.Count>=3;string label=rearDeck?"S"+marker++:null;loops.Add(new{label,role=group.Key,vertices=component.Count,closedManifoldLoop=closed,boundsMin=A(box.Position),boundsMax=A(box.End),centroid=A(centre),rearDeckCandidate=rearDeck,isVerifiedSocket=false});if(showLabels&&rearDeck)Marker(car,centre,label,new Color(.1f,1,.5f));
   }
  }
  var folder=System.IO.Path.GetDirectoryName(path);if(!string.IsNullOrEmpty(folder))System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllText(path,JsonSerializer.Serialize(new{car=index,parts,wingExtremeBands=extremities,bodyBootBoundaryComponents=loops,markersShown=showLabels,scope="Read-only intact source geometry audit, not a reconstructed or recovered wing pose. OrangeW labels=measured part extremity bands; greenS=rear/deck boundary hypotheses only, NOT verified mounting sockets. Source transforms/geometry/UV/colliders untouched. Mesh seams/material boundaries may produce false socket candidates; manual full-car and close-up review required."}));GD.Print("PASS read-only Chiron wing attachment audit; no mounting pose accepted");
 }
}
}
