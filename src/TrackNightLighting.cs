using Godot;
using MySimulator.Rebuild;
using System.Collections.Generic;
namespace MySim {
// Opt-in art candidate. Source geometry/materials are untouched; no lights are invented in daylight.
// Mounts require actual drive-surface support and clearance from every route segment.
public partial class TrackNightLighting : Node3D {
 public int TrackIndex;
 readonly List<(Vector3 mount,Vector3 aim)> lamps=new();
 readonly List<SpotLight3D> pool=new();
 bool built;float updateAge;
 static float Read(System.Text.Json.JsonElement p,string key)=>p.GetProperty(key).GetSingle();
 public override void _PhysicsProcess(double delta) {
  if(!built){built=true;Build();}
  updateAge+=(float)delta;if(updateAge<.2f)return;updateAge=0;
  var camera=GetViewport().GetCamera3D();if(camera==null)return;
  var order=new List<int>();for(int i=0;i<lamps.Count;i++)order.Add(i);
  order.Sort((a,b)=>camera.GlobalPosition.DistanceSquaredTo(lamps[a].mount).CompareTo(camera.GlobalPosition.DistanceSquaredTo(lamps[b].mount)));
  for(int i=0;i<pool.Count;i++){var light=pool[i];light.Visible=i<order.Count&&camera.GlobalPosition.DistanceTo(lamps[order[i]].mount)<180f;if(!light.Visible)continue;var station=lamps[order[i]];light.GlobalPosition=station.mount;light.LookAt(station.aim,Vector3.Up);}
 }
 void Build(){
  using var document=System.Text.Json.JsonDocument.Parse(FileAccess.GetFileAsString("res://data/track_floodlights.json"));var p=document.RootElement;
  float spacing=Read(p,"spacing"),height=Read(p,"height"),offset=Read(p,"routeOffset"),range=Read(p,"range");
  if(spacing<20||height<8||offset<9||range<height)throw new System.InvalidOperationException("Invalid floodlight placement profile");
  var route=new Circuit(TrackIndex);var space=GetWorld3D().DirectSpaceState;float walked=0,next=0;int rejected=0;
  var steel=new StandardMaterial3D{AlbedoColor=new Color(.16f,.18f,.20f),Metallic=.7f,Roughness=.5f};
  var emitter=new StandardMaterial3D{AlbedoColor=new Color(.78f,.85f,.92f),EmissionEnabled=true,Emission=new Color(.78f,.85f,.92f),EmissionEnergyMultiplier=2f,Roughness=.4f};
  for(int segment=0;segment<route.points.Length;segment++){
   var a=route.points[segment];var b=route.points[(segment+1)%route.points.Length];float length=(b-a).Magnitude;if(length<.001f)continue;
   while(next<walked+length){float t=(next-walked)/length;next+=spacing;var centre=a+(b-a)*t;var right=(b-a).Normalized.Right;
    var centreGround=Contact.Ground(space,new Vector3(centre.x,0,centre.z),0);if(!centreGround.found){rejected+=2;continue;}
    foreach(float side in new[]{-1f,1f}){
     var pos=centre+right*(offset*side);route.Find(pos,out float lateral);if(Mathf.Abs(lateral)<route.width*.5f+2f){rejected++;continue;}
     var ground=Contact.Ground(space,new Vector3(pos.x,centreGround.rawY,pos.z),centreGround.rawY);
     if(!ground.found||Mathf.Abs(ground.rawY-centreGround.rawY)>3f||ground.normal.Y<.85f){rejected++;continue;}
     var bottom=new Vector3(pos.x,ground.rawY,pos.z);var top=bottom+Vector3.Up*height;
     using var clearanceShape=new CylinderShape3D{Height=height+.4f,Radius=.7f};
     using var check=new PhysicsShapeQueryParameters3D{Shape=clearanceShape,Transform=new Transform3D(Basis.Identity,bottom+Vector3.Up*(height*.5f+.2f)),CollisionMask=2,Margin=.05f};
     if(space.IntersectShape(check,1).Count>0){rejected++;continue;}
     var mast=new StaticBody3D{Name="FloodlightMast_"+lamps.Count,CollisionLayer=2,CollisionMask=0,Position=bottom};AddChild(mast);
     mast.AddChild(new CollisionShape3D{Position=Vector3.Up*(height*.5f),Shape=new CylinderShape3D{Height=height,Radius=.13f}});
     mast.AddChild(new MeshInstance3D{Position=Vector3.Up*(height*.5f),Mesh=new CylinderMesh{Height=height,TopRadius=.09f,BottomRadius=.13f,RadialSegments=8},MaterialOverride=steel});
     var head=new MeshInstance3D{Position=Vector3.Up*height,Mesh=new BoxMesh{Size=new Vector3(1.1f,.18f,.65f)},MaterialOverride=steel};mast.AddChild(head);
     mast.AddChild(new MeshInstance3D{Position=Vector3.Up*(height-.10f),Mesh=new BoxMesh{Size=new Vector3(.95f,.025f,.5f)},MaterialOverride=emitter});
     lamps.Add((top-Vector3.Up*.15f,new Vector3(centre.x,centreGround.rawY,centre.z)));
    }
   }walked+=length;
  }
  int budget=p.GetProperty("activeLights").GetInt32();if(budget<2||budget>12)throw new System.InvalidOperationException("Floodlight pool must remain bounded");
  for(int i=0;i<budget;i++){var light=new SpotLight3D{Visible=false,SpotRange=range,SpotAngle=Read(p,"coneAngle"),SpotAttenuation=1f,LightEnergy=Read(p,"energy"),LightColor=new Color(.82f,.89f,1f),ShadowEnabled=i<2,ShadowBias=.03f,ShadowNormalBias=1f,DistanceFadeEnabled=true,DistanceFadeBegin=110f,DistanceFadeLength=45f};AddChild(light);pool.Add(light);}
  GD.Print("TRACK_FLOODLIGHTS candidate track=",TrackIndex," supportedMounts=",lamps.Count," rejected=",rejected," activeBudget=",pool.Count);
  if(System.Environment.GetEnvironmentVariable("MYSIM_TRACK_LIGHT_AUDIT") is string path){var folder=System.IO.Path.GetDirectoryName(path);if(!string.IsNullOrEmpty(folder))System.IO.Directory.CreateDirectory(folder);var stations=new List<object>();foreach(var lamp in lamps)stations.Add(new{mount=new[]{lamp.mount.X,lamp.mount.Y,lamp.mount.Z},aim=new[]{lamp.aim.X,lamp.aim.Y,lamp.aim.Z}});System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(new{track=TrackIndex,candidate=true,supportedMounts=lamps.Count,rejected,activeBudget=pool.Count,stations,scope="Ray-supported procedural candidate; requires full track night/shadow/clearance review before promotion"}));}
 }
}
}
