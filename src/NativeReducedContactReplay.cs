using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MySim {
// Isolated source-contact experiment, not a gameplay replacement. No source vertices leave this process.
public partial class NativeReducedContactReplay : Node3D {
 public string CapturePath,OutputPath;
 static Vector3 V(JsonElement p)=>new(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle());
 static Transform3D T(JsonElement p){var b=p.GetProperty("basisColumns");return new Transform3D(new Basis(V(b[0]),V(b[1]),V(b[2])),V(p.GetProperty("origin")));}
 static float[] A(Vector3 p)=>new[]{p.X,p.Y,p.Z};
 static object Pose(Transform3D p)=>new{origin=A(p.Origin),basisColumns=new[]{A(p.Basis.X),A(p.Basis.Y),A(p.Basis.Z)}};
 readonly List<object> trials=new();
 sealed partial class ReplayBody : RigidBody3D {
  public readonly List<object> Samples=new();public int Steps;public Vector3 FirstPosition,FirstVelocity;public int MaximumContacts;public readonly List<Vector3> Positions=new(),Velocities=new();
  public override void _IntegrateForces(PhysicsDirectBodyState3D state){
   Steps++;Positions.Add(state.Transform.Origin);Velocities.Add(state.LinearVelocity);if(Steps==1){FirstPosition=state.Transform.Origin;FirstVelocity=state.LinearVelocity;}var contacts=new List<object>();
   for(int i=0;i<state.GetContactCount();i++){var local=state.GetContactLocalShape(i);var owner=ShapeOwnerGetOwner(ShapeFindOwner(local)) as CollisionShape3D;contacts.Add(new{localShape=local,localShapeName=owner?.Name.ToString(),bodyWitness=A(state.GetContactLocalPosition(i)),colliderWitness=A(state.GetContactColliderPosition(i)),normal=A(state.GetContactLocalNormal(i)),reportedPointVelocity=A(state.GetContactLocalVelocityAtPosition(i))});}
   MaximumContacts=Math.Max(MaximumContacts,contacts.Count);if(Samples.Count<8)Samples.Add(new{step=Steps,transform=Pose(state.Transform),linearVelocity=A(state.LinearVelocity),angularVelocity=A(state.AngularVelocity),contacts});
  }
 }
 public override async void _Ready(){try{
  if(string.IsNullOrWhiteSpace(CapturePath)||string.IsNullOrWhiteSpace(OutputPath))throw new Exception("Replay requires capture and output paths");
  using var capture=JsonDocument.Parse(System.IO.File.ReadAllText(CapturePath));var root=capture.RootElement;var prior=root.GetProperty("priorStates");if(prior.GetArrayLength()<2)throw new Exception("Replay requires prior integration states");
  var seed=prior[prior.GetArrayLength()-1];var target=root.GetProperty("firstContactState");var witness=root.GetProperty("witnesses")[0];var config=root.GetProperty("bodyConfiguration");int car=root.GetProperty("car").GetInt32();
  var mismatches=new List<string>();foreach(var setting in root.GetProperty("settings").EnumerateObject()){string current=ProjectSettings.GetSetting(setting.Name).ToString();if(current!=setting.Value.GetString())mismatches.Add(setting.Name+": captured="+setting.Value.GetString()+" current="+current);}
  if(mismatches.Count>0)throw new Exception("Replay settings differ: "+string.Join(";",mismatches));
  float capturedStep=seed.GetProperty("step").GetSingle();if(Mathf.Abs(capturedStep-1f/Engine.PhysicsTicksPerSecond)>.000001f)throw new Exception("Replay physics clock mismatch");
  var configuration=root.GetProperty("configuration");int track=int.Parse(configuration.GetProperty("MYSIM_TRACK").GetString());
  var tracks=Json.ParseString(FileAccess.GetFileAsString("res://assets/RealRacing/TrackList.json")).AsGodotDictionary()["tracks"].AsGodotArray();var descriptor=tracks[track].AsGodotDictionary();string map=descriptor["map"].AsString(),layout=descriptor["layout"].AsString();
  string road="res://assets/Resources/"+map+"_"+layout+".txt";if(!FileAccess.FileExists(road))road="res://assets/Resources/"+map+"_processed.txt";
  var originalRoad=DriveSurfaceLoader.Load(road);var originalShape=originalRoad.GetChild<CollisionShape3D>(0);var data=((ConcavePolygonShape3D)originalShape.Shape).Data;var roadTransform=T(witness.GetProperty("colliderTransform"))*T(witness.GetProperty("colliderShapeTransform"));var contact=V(witness.GetProperty("colliderWitness"));
  const float patchHalfSize=12f;var patch=new List<Vector3>();
  for(int i=0;i+2<data.Length;i+=3){var a=roadTransform*data[i];var b=roadTransform*data[i+1];var c=roadTransform*data[i+2];var box=new Aabb(a,Vector3.Zero).Expand(b).Expand(c);if(box.Position.X>contact.X+patchHalfSize||box.End.X<contact.X-patchHalfSize||box.Position.Z>contact.Z+patchHalfSize||box.End.Z<contact.Z-patchHalfSize)continue;patch.Add(a);patch.Add(b);patch.Add(c);}
  originalRoad.Free();if(patch.Count<3)throw new Exception("Source road patch is empty");
  // Resolve matched plane from actual source triangle beneath collider witness. No slope is invented.
  Vector3 planeNormal=Vector3.Zero;float best=float.PositiveInfinity;
  for(int i=0;i+2<patch.Count;i+=3){var a=patch[i];var b=patch[i+1];var c=patch[i+2];var normal=(b-a).Cross(c-a);if(normal.LengthSquared()<.00000001f)continue;normal=normal.Normalized();float distance=Mathf.Abs(normal.Dot(contact-a));if(distance>=best)continue;float denominator=(b.Z-c.Z)*(a.X-c.X)+(c.X-b.X)*(a.Z-c.Z);if(Mathf.Abs(denominator)<.00000001f)continue;float u=((b.Z-c.Z)*(contact.X-c.X)+(c.X-b.X)*(contact.Z-c.Z))/denominator;float v=((c.Z-a.Z)*(contact.X-c.X)+(a.X-c.X)*(contact.Z-c.Z))/denominator;if(u<-.001f||v<-.001f||1-u-v<-.001f)continue;best=distance;planeNormal=normal.Y<0?-normal:normal;}
  if(best>.01f||planeNormal.Y<.5f)throw new Exception("Cannot resolve a valid matched road plane");
  foreach(string kind in new[]{"source-patch","matched-plane"}){
   var trial=new Node3D{Name=kind};AddChild(trial);var floor=new StaticBody3D{Name="DriveSurface",CollisionLayer=1,CollisionMask=4};trial.AddChild(floor);
   if(kind=="source-patch")floor.AddChild(new CollisionShape3D{Shape=new ConcavePolygonShape3D{Data=patch.ToArray(),BackfaceCollision=true}});
   else floor.AddChild(new CollisionShape3D{Shape=new WorldBoundaryShape3D{Plane=new Plane(planeNormal,planeNormal.Dot(contact))}});
   var body=new ReplayBody{Name="ActualSourceCar",Mass=config.GetProperty("Mass").GetSingle(),CenterOfMassMode=RigidBody3D.CenterOfMassModeEnum.Custom,CenterOfMass=V(config.GetProperty("centerOfMass")),GravityScale=config.GetProperty("GravityScale").GetSingle(),LinearDamp=config.GetProperty("LinearDamp").GetSingle(),AngularDamp=config.GetProperty("AngularDamp").GetSingle(),ContinuousCd=config.GetProperty("ContinuousCd").GetBoolean(),CanSleep=false,MaxContactsReported=16,CollisionLayer=4,CollisionMask=1,Transform=T(seed.GetProperty("transform"))};
   // Preserve complete baked body compound rather than changing its mass/inertia to a single bumper.
   string manifest="res://assets/generated_native_car_hulls/car-"+car.ToString("00")+".json";using var hulls=JsonDocument.Parse(FileAccess.GetFileAsString(manifest));var files=hulls.RootElement.GetProperty("files");var names=hulls.RootElement.GetProperty("partNames");
   for(int i=0;i<files.GetArrayLength();i++){string resource=files[i].GetString();if(!resource.StartsWith("res://assets/generated_native_car_hulls/")||resource.Contains(".."))throw new Exception("Invalid hull reference");body.AddChild(new CollisionShape3D{Name="SourceBodyHull_"+names[i].GetString(),Shape=GD.Load<ConvexPolygonShape3D>(resource)});}
   int hitShape=witness.GetProperty("localShape").GetInt32();if(hitShape<0||hitShape>=body.GetChildCount())throw new Exception("Captured source shape index cannot be resolved");body.GetChild<CollisionShape3D>(hitShape).Transform=T(witness.GetProperty("localShapeTransform"));
   trial.AddChild(body);body.LinearVelocity=V(seed.GetProperty("linearVelocity"));body.AngularVelocity=V(seed.GetProperty("angularVelocity"));
   while(body.Steps<4)await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   var seedPosition=V(seed.GetProperty("transform").GetProperty("origin"));var expectedPosition=V(target.GetProperty("transform").GetProperty("origin"));var expectedVelocity=V(target.GetProperty("linearVelocity"));
   float closestPositionError=float.PositiveInfinity,associatedVelocityError=float.PositiveInfinity;for(int i=0;i<body.Positions.Count;i++){float error=body.Positions[i].DistanceTo(expectedPosition);if(error<closestPositionError){closestPositionError=error;associatedVelocityError=body.Velocities[i].DistanceTo(expectedVelocity);}}
   trials.Add(new{kind,patchTriangles=patch.Count/3,planeNormal=A(planeNormal),sourceRoad=road,sourceHullManifest=manifest,seedPose=Pose(T(seed.GetProperty("transform"))),seedLinearVelocity=A(V(seed.GetProperty("linearVelocity"))),seedAngularVelocity=A(V(seed.GetProperty("angularVelocity"))),firstEntryPositionError=body.FirstPosition.DistanceTo(seedPosition),body.MaximumContacts,samples=body.Samples.ToArray(),recordedNextContactPosition=A(expectedPosition),recordedNextContactVelocity=A(expectedVelocity),closestRecordedPositionError=closestPositionError,associatedRecordedVelocityError=associatedVelocityError,reproductionEstablished=false});
   trial.QueueFree();await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  }
  var folder=System.IO.Path.GetDirectoryName(OutputPath);if(!string.IsNullOrEmpty(folder))System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllText(OutputPath,JsonSerializer.Serialize(new{car,track,capture=CapturePath,settingsMatched=true,trials,scope="Reduced free-body source-patch/plane comparison only. Complete baked body compound retained, but mounted-part poses other than struck shape, gameplay tyre/suspension/aero forces and contact-cache history are not replayed. Recorded precontact trajectory has NOT been matched; this fixture cannot prove root cause or full-game repair. No raw source vertices included."}));GD.Print("COMPLETE reduced contact experiment; reproduction unproven");CleanExit.Request(this);
 }catch(Exception error){GD.PushError(error.ToString());CleanExit.Request(this,1);}}
}
}
