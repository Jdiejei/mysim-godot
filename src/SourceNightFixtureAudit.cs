using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
namespace MySim {
// Read-only cloud diagnostic. Candidate points are real triangle centroids, never batched AABB centres.
// No source buffers are serialized and no candidate is certified as an installed/usable light.
public static class SourceNightFixtureAudit {
 sealed class Triangle {public Vector3 A,B,C;public int Surface,Index;public Vector3 Centre=>(A+B+C)/3f;public Vector3 Normal=>-(B-A).Cross(C-A).Normalized();}
 sealed class Component {public string Node,Hash;public int Id;public Aabb Bounds;public List<Triangle> Faces=new();public bool Shaft;public MeshInstance3D Source;public string Family;}
 sealed class Assembly {public Component Shaft;public List<Component> Members=new();public List<object> Ambiguous=new();}
 static readonly Dictionary<int,List<Assembly>> reviewAssemblies=new();
 static string Family(string name){name=System.Text.RegularExpressions.Regex.Replace(name.ToLowerInvariant(),@"_mm\d+$","");return System.Text.RegularExpressions.Regex.Replace(name,@"\d+$","");}
 static float[] V(Vector3 v)=>new[]{v.X,v.Y,v.Z};
 static string Key(Vector3 v)=>BitConverter.SingleToInt32Bits(v.X)+":"+BitConverter.SingleToInt32Bits(v.Y)+":"+BitConverter.SingleToInt32Bits(v.Z);
 static bool Included(string name,int track){name=name.ToLowerInvariant();return track==25&&(name.Contains("singapore_floodlights")||name.Contains("singapore_lightpole")||name.Contains("singapore_bridgelights")||name.Contains("singapore_grandstand_lighting"))||track==28&&name.Contains("yasmarina_floodlight_pole");}
 static string Hash(List<Triangle> faces){using var buffer=new MemoryStream();using(var writer=new BinaryWriter(buffer,System.Text.Encoding.UTF8,true))foreach(var face in faces){writer.Write(face.Surface);writer.Write(face.Index);foreach(var p in new[]{face.A,face.B,face.C}){writer.Write(p.X);writer.Write(p.Y);writer.Write(p.Z);}}return Convert.ToHexString(SHA256.HashData(buffer.ToArray()));}
 public static void Write(List<Node3D> layers,int track,string destination,Node3D debugParent=null){
  if(track!=25&&track!=28)throw new ArgumentOutOfRangeException(nameof(track),"Fixture source allowlist currently covers25/28 only");
  var components=new List<Component>();var meshes=new List<object>();
  foreach(var layer in layers){var nodes=layer.FindChildren("*","MeshInstance3D",true,false);foreach(var node in nodes){
   var mesh=(MeshInstance3D)node;if(mesh.Mesh==null||!mesh.IsVisibleInTree()||!Included(mesh.Name.ToString(),track))continue;
   var faces=new List<Triangle>();var parent=new List<int>();var owners=new Dictionary<string,int>();int degenerate=0;
   int Root(int x){while(parent[x]!=x){parent[x]=parent[parent[x]];x=parent[x];}return x;}
   void Join(int a,int b){a=Root(a);b=Root(b);if(a!=b)parent[b]=a;}
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
    if((mesh.Mesh as ArrayMesh ?? throw new NotSupportedException("Fixture surface requires ArrayMesh primitive metadata")).SurfaceGetPrimitiveType(surface)!=Mesh.PrimitiveType.Triangles)continue;
    using var arrays=mesh.Mesh.SurfaceGetArrays(surface);var points=(Vector3[])arrays[(int)Mesh.ArrayType.Vertex];var indices=(int[])arrays[(int)Mesh.ArrayType.Index];int count=indices?.Length>0?indices.Length:points.Length;
    for(int n=0;n+2<count;n+=3){int ia=indices?.Length>0?indices[n]:n,ib=indices?.Length>0?indices[n+1]:n+1,ic=indices?.Length>0?indices[n+2]:n+2;if(ia<0||ib<0||ic<0||ia>=points.Length||ib>=points.Length||ic>=points.Length)throw new InvalidOperationException("Source fixture triangle index outside source buffer");
     var local=new[]{points[ia],points[ib],points[ic]};foreach(var p in local)if(!p.IsFinite())throw new InvalidOperationException("Nonfinite source fixture vertex");
     int id=faces.Count;parent.Add(id);var face=new Triangle{A=mesh.GlobalTransform*local[0],B=mesh.GlobalTransform*local[1],C=mesh.GlobalTransform*local[2],Surface=surface,Index=n/3};faces.Add(face);if((face.B-face.A).Cross(face.C-face.A).LengthSquared()<1e-16f)degenerate++;
     // Exact coordinate sharing reconnects UV/material seam duplicates without an epsilon weld.
     foreach(var p in local){string key=Key(p);if(owners.TryGetValue(key,out int owner))Join(id,owner);else owners[key]=id;}
    }
   }
   var groups=new SortedDictionary<int,List<Triangle>>();for(int i=0;i<faces.Count;i++){int root=Root(i);if(!groups.ContainsKey(root))groups[root]=new();groups[root].Add(faces[i]);}
   int componentId=0;foreach(var group in groups.Values){var bounds=new Aabb(group[0].A,Vector3.Zero);foreach(var face in group){bounds=bounds.Expand(face.A);bounds=bounds.Expand(face.B);bounds=bounds.Expand(face.C);}var size=bounds.Size;float span=Mathf.Max(size.X,size.Z);components.Add(new Component{Node=mesh.GetPath().ToString(),Id=componentId++,Hash=Hash(group),Bounds=bounds,Faces=group,Source=mesh,Family=Family(mesh.Name.ToString()),Shaft=size.Y>=12f&&span<=2f&&size.Y/Mathf.Max(.01f,span)>=10f});}
   meshes.Add(new{node=mesh.GetPath().ToString(),name=mesh.Name.ToString(),triangles=faces.Count,exactConnectedComponents=groups.Count,degenerateTriangles=degenerate,worldTopologyHash=Hash(faces)});
  }nodes.Clear();}
  var assemblies=new List<Assembly>();foreach(var component in components)if(component.Shaft){var assembly=new Assembly{Shaft=component};assembly.Members.Add(component);assemblies.Add(assembly);}
  var unresolved=new List<object>();
  foreach(var component in components){if(component.Shaft)continue;var options=new List<(Assembly assembly,float distance)>();var centre=component.Bounds.GetCenter();
   foreach(var assembly in assemblies){var shaft=assembly.Shaft;if(shaft.Family!=component.Family)continue;float top=shaft.Bounds.End.Y;if(component.Bounds.Position.Y<top-6f||component.Bounds.End.Y>top+1f)continue;var d=centre-shaft.Bounds.GetCenter();float distance=Mathf.Sqrt(d.X*d.X+d.Z*d.Z);if(distance<=4f)options.Add((assembly,distance));}
   options.Sort((a,b)=>a.distance.CompareTo(b.distance));
   if(options.Count>0&&(options.Count==1||options[1].distance-options[0].distance>.5f)){options[0].assembly.Members.Add(component);}
   else {var record=new{node=component.Node,component=component.Id,hash=component.Hash,status=options.Count==0?"unresolved-no-tall-shaft-headzone":"ambiguous-nearest-shaft",candidateShafts=options.ConvertAll(o=>new{node=o.assembly.Shaft.Node,component=o.assembly.Shaft.Id,distance=o.distance})};unresolved.Add(record);foreach(var option in options)option.assembly.Ambiguous.Add(record);}
  }
  assemblies.Sort((a,b)=>{int compare=StringComparer.Ordinal.Compare(a.Shaft.Node,b.Shaft.Node);return compare!=0?compare:a.Shaft.Id.CompareTo(b.Shaft.Id);});reviewAssemblies[track]=assemblies;
  var assemblyRows=new List<object>();foreach(var assembly in assemblies){var members=new List<object>();foreach(var member in assembly.Members)members.Add(new{node=member.Node,component=member.Id,hash=member.Hash,triangles=member.Faces.Count});assemblyRows.Add(new{shaftNode=assembly.Shaft.Node,shaftComponent=assembly.Shaft.Id,shaftHash=assembly.Shaft.Hash,family=assembly.Shaft.Family,headZoneMin=assembly.Shaft.Bounds.End.Y-6f,headZoneMax=assembly.Shaft.Bounds.End.Y+1f,members,ambiguous=assembly.Ambiguous,validated=false,status=assembly.Members.Count>1?"grouped-head-bank-unvalidated":"unresolved-shaft-without-head-bank"});}
  var rows=new List<object>();var anchors=new List<object>();int marker=0;
  foreach(var component in components){var bounds=component.Bounds;float top=bounds.End.Y;Triangle candidate=null;float best=float.NegativeInfinity;
   foreach(var face in component.Faces){if((face.B-face.A).Cross(face.C-face.A).LengthSquared()<1e-16f)continue;float score=face.Centre.Y;if(face.Normal.Y<-.25f&&score>top-2f&&score>best){candidate=face;best=score;}}
   bool downward=candidate!=null;if(candidate==null)foreach(var face in component.Faces)if(face.Centre.Y>best&&(face.B-face.A).Cross(face.C-face.A).LengthSquared()>=1e-16f){candidate=face;best=face.Centre.Y;}
   var associations=new List<object>();if(candidate!=null)foreach(var shaft in components){if(!shaft.Shaft||ReferenceEquals(shaft,component))continue;var s=shaft.Bounds;var point=candidate.Centre;float dx=Mathf.Max(s.Position.X-point.X,Mathf.Max(0,point.X-s.End.X)),dz=Mathf.Max(s.Position.Z-point.Z,Mathf.Max(0,point.Z-s.End.Z));float horizontal=Mathf.Sqrt(dx*dx+dz*dz);float heightDelta=point.Y-s.End.Y;if(horizontal<=3f&&Mathf.Abs(heightDelta)<=3f)associations.Add(new{node=shaft.Node,component=shaft.Id,hash=shaft.Hash,horizontalDistance=horizontal,headToShaftTop=heightDelta});}
   bool batched=Mathf.Max(bounds.Size.X,bounds.Size.Z)>8f;string status=batched?"unresolved-connected-batch":component.Shaft?"shaft-component-not-head":associations.Count==1?"single-shaft-association-unvalidated":associations.Count>1?"ambiguous-shaft-association":"unresolved-no-shaft-association";
   rows.Add(new{node=component.Node,component=component.Id,hash=component.Hash,triangles=component.Faces.Count,boundsMin=V(bounds.Position),boundsMax=V(bounds.End),size=V(bounds.Size),shaftCandidate=component.Shaft,status});
   if(candidate==null)continue;var anchor=candidate.Centre;anchors.Add(new{node=component.Node,component=component.Id,componentHash=component.Hash,surface=candidate.Surface,triangle=candidate.Index,attachment=V(anchor),normal=V(candidate.Normal),downwardFace=downward,status,validated=false,shaftAssociations=associations,required="Housing/component review, source attachment, surrounding clearance, unobstructed ground targets and full-route illumination coverage"});
   if(debugParent!=null&&!component.Shaft&&!batched){var material=new StandardMaterial3D{AlbedoColor=new Color(1,.2f,.6f),ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};debugParent.AddChild(new MeshInstance3D{Name="FixtureCandidate_"+marker++,Position=debugParent.ToLocal(anchor),Mesh=new SphereMesh{Radius=.18f,Height=.36f},MaterialOverride=material,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});}
  }
  string folder=Path.GetDirectoryName(destination);if(!string.IsNullOrEmpty(folder))Directory.CreateDirectory(folder);System.IO.File.WriteAllText(destination,System.Text.Json.JsonSerializer.Serialize(new{track,sourceMeshes=meshes,components=rows,candidateAttachments=anchors,assemblies=assemblyRows,unresolvedComponents=unresolved,markers=marker,connectivity="Shared exact local positions, across surfaces in each source mesh; no epsilon weld",scope="Read-only source fixture diagnostic. Numeric hashes/counts/bounds/triangle-centroid candidates only; no raw vertex buffers and no validated lamp placement"},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
  GD.Print("SOURCE_NIGHT_FIXTURE_AUDIT track=",track," meshes=",meshes.Count," components=",components.Count," candidates=",anchors.Count," markers=",marker);
 }
 // Call after Write while original source nodes remain alive. Render copies stay in memory only.
 // The return list contains separately framed assemblies; no source mesh/material resource is edited.
 public static List<Node3D> BuildIllustrativeAssemblies(int track,Node3D parent,int maximum=3){
  if(!reviewAssemblies.TryGetValue(track,out var available))throw new InvalidOperationException("Run fixture Write before building review assemblies");
  var chosen=new List<Assembly>();foreach(string target in track==25?new[]{"floodlights_001","floodlights_002"}:new[]{"pole_short002","pole_double","pole_short006"})foreach(var assembly in available)if(assembly.Shaft.Node.EndsWith(target,StringComparison.Ordinal)&&assembly.Members.Count>1&&!chosen.Contains(assembly)){chosen.Add(assembly);break;}
  foreach(var assembly in available)if(chosen.Count<maximum&&assembly.Members.Count>1&&!chosen.Contains(assembly))chosen.Add(assembly);
  var result=new List<Node3D>();foreach(var assembly in chosen){if(result.Count>=Math.Min(3,maximum))break;var root=new Node3D{Name="SourceFixtureAssembly_"+result.Count};parent.AddChild(root);var centre=assembly.Shaft.Bounds.GetCenter();
   foreach(var component in assembly.Members){var surfaces=new Dictionary<int,List<Triangle>>();foreach(var face in component.Faces){if(!surfaces.ContainsKey(face.Surface))surfaces[face.Surface]=new();surfaces[face.Surface].Add(face);}foreach(var pair in surfaces){using var arrays=component.Source.Mesh.SurfaceGetArrays(pair.Key);var indices=(int[])arrays[(int)Mesh.ArrayType.Index];var normals=(Vector3[])arrays[(int)Mesh.ArrayType.Normal];var uv=(Vector2[])arrays[(int)Mesh.ArrayType.TexUV];var uv2=(Vector2[])arrays[(int)Mesh.ArrayType.TexUV2];var colours=(Color[])arrays[(int)Mesh.ArrayType.Color];var surface=new SurfaceTool();surface.Begin(Mesh.PrimitiveType.Triangles);surface.SetMaterial(component.Source.GetActiveMaterial(pair.Key));foreach(var face in pair.Value){var points=new[]{face.A,face.B,face.C};for(int corner=0;corner<3;corner++){int at=face.Index*3+corner,index=indices?.Length>0?indices[at]:at;if(normals?.Length>index)surface.SetNormal((component.Source.GlobalBasis.Inverse().Transposed()*normals[index]).Normalized());if(uv?.Length>index)surface.SetUV(uv[index]);if(uv2?.Length>index)surface.SetUV2(uv2[index]);if(colours?.Length>index)surface.SetColor(colours[index]);surface.AddVertex(points[corner]-centre);}}root.AddChild(new MeshInstance3D{Name="Component_"+component.Id+"_Surface_"+pair.Key,Mesh=surface.Commit()});surface.Dispose();}}
   root.AddChild(new Label3D{Text=assembly.Shaft.Source.Name+" / shaft "+assembly.Shaft.Id+"\n"+assembly.Members.Count+" source components / UNVALIDATED",Position=new Vector3(0,assembly.Shaft.Bounds.Size.Y*.5f+1f,0),Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,FontSize=48,PixelSize=.012f,NoDepthTest=true});root.SetMeta("source_shaft",assembly.Shaft.Node);root.SetMeta("source_component",assembly.Shaft.Id);root.SetMeta("validated",false);result.Add(root);
  }return result;
 }
}
}
