using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MySim {
public partial class WallGeometryScenario : Node3D {
 public override async void _Ready(){try{
  var table=Json.ParseString(FileAccess.GetFileAsString("res://assets/RealRacing/TrackList.json")).AsGodotDictionary()["tracks"].AsGodotArray();var track=table[49].AsGodotDictionary();
  var source=GD.Load<PackedScene>("res://assets/RealRacing/Source/"+track["map"].AsString()+"/"+track["scene"].AsString()).Instantiate<Node3D>();source.Scale=Vector3.One*100;AddChild(source);
  foreach(var variation in track["variationPaths"].AsGodotArray()){string resource="res://"+variation.AsString().Replace("Assets/RealRacing/Source/","assets/RealRacing/Source/");if(EmptyVariations.Paths.Contains(resource)||!ResourceLoader.Exists(resource))continue;var layer=GD.Load<PackedScene>(resource).Instantiate<Node3D>();layer.Scale=Vector3.One*100;AddChild(layer);}
  foreach(var child in GetChildren())if(child is Node3D layer){WindingFixer.Fix(layer,out _);GroundFixer.Fix(layer);}
  var rows=new List<object>();var suspects=new List<(MeshInstance3D Mesh,Vector3 Centre,Vector3 Normal)>();
  foreach(var node in FindChildren("*","MeshInstance3D",true,false)){var mesh=(MeshInstance3D)node;if(!mesh.Name.ToString().Contains("prop_turn_sign_a_35_mm5",StringComparison.OrdinalIgnoreCase))continue;
   var faces=WallBuilder.WallFaces(mesh,out var support);float localMin=float.PositiveInfinity,worldMin=float.PositiveInfinity,maxWorldArea=0;int zeroArea=0;Vector3 probeCentre=Vector3.Zero,probeNormal=Vector3.Zero;
   for(int i=0;i<faces.Length;i+=3){var local=(faces[i+1]-faces[i]).Cross(faces[i+2]-faces[i]);var a=mesh.GlobalTransform*faces[i];var b=mesh.GlobalTransform*faces[i+1];var c=mesh.GlobalTransform*faces[i+2];var world=(b-a).Cross(c-a);localMin=Math.Min(localMin,local.Length());worldMin=Math.Min(worldMin,world.Length());if(world.LengthSquared()==0)zeroArea++;if(world.Length()>maxWorldArea){maxWorldArea=world.Length();probeCentre=(a+b+c)/3;probeNormal=world.Normalized();}}
   rows.Add(new {name=mesh.Name.ToString(),vertices=faces.Length,zeroArea,localMinDoubleArea=float.IsFinite(localMin)?localMin:0,worldMinDoubleArea=float.IsFinite(worldMin)?worldMin:0,maxWorldDoubleArea=maxWorldArea,basisScale=new[]{mesh.GlobalBasis.Scale.X,mesh.GlobalBasis.Scale.Y,mesh.GlobalBasis.Scale.Z}});suspects.Add((mesh,probeCentre,probeNormal));
  }
  if(rows.Count==0)throw new Exception("Original rejected sign mesh not found in diagnostic source");
  foreach(var child in GetChildren())if(child is Node3D layer)WallBuilder.Build(layer);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  var probes=new List<object>();bool all=true;foreach(var suspect in suspects){bool both=true;for(int side=-1;side<=1;side+=2){bool hitTarget=false;if(suspect.Normal.LengthSquared()>0){using var query=PhysicsRayQueryParameters3D.Create(suspect.Centre+suspect.Normal*(side*.2f),suspect.Centre-suspect.Normal*(side*.2f),2);query.HitBackFaces=true;var hit=GetWorld3D().DirectSpaceState.IntersectRay(query);hitTarget=hit.Count>0&&(hit["collider"].AsGodotObject() as Node)?.Name.ToString()==suspect.Mesh.Name.ToString()+"_col";hit.Clear();}both&=hitTarget;probes.Add(new {name=suspect.Mesh.Name.ToString(),side,hitTarget});}all&=both;}
  string path=System.Environment.GetEnvironmentVariable("MYSIM_WALL_GEOMETRY_CHECKS");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));System.IO.File.WriteAllText(path,JsonSerializer.Serialize(new {all,metreRepresentation=System.Environment.GetEnvironmentVariable("MYSIM_METRE_WALL_COLLIDERS")=="1",rows,probes,scope="Rejected track49 source sign geometry and real two-sided ray contacts only; no whole-track/race acceptance"},new JsonSerializerOptions {WriteIndented=true}));CleanExit.Request(this,0);
 }catch(Exception e){GD.PushError(e.ToString());CleanExit.Request(this,1);}}
}
}

