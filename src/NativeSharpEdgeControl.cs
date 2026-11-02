using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
namespace MySim {
// Negative control for the edge-algorithm comparison. Fixture gravity is zero; source hulls,
// margins, mass and custom COM are retained. Does not change game or solver settings.
public static partial class NativeSharpEdgeControl {
 static Vector3 V(JsonElement p)=>new(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle());
 static float[] A(Vector3 p)=>new[]{p.X,p.Y,p.Z};
 public partial class Probe : RigidBody3D {
  public int Steps,ContactSamples;public bool Finite=true;public float MaximumPenetration;public float MaximumOriginAdvance=float.NegativeInfinity;public readonly List<object> Witnesses=new();public readonly List<CollisionShape3D> Hulls=new();public Vector3 LastVelocity;public bool XWallContact,ZWallContact;
  public override void _IntegrateForces(PhysicsDirectBodyState3D state){
   Steps++;LastVelocity=state.LinearVelocity;Finite&=state.Transform.Origin.IsFinite()&&state.LinearVelocity.IsFinite()&&state.AngularVelocity.IsFinite();MaximumOriginAdvance=Mathf.Max(MaximumOriginAdvance,Mathf.Max(state.Transform.Origin.X,state.Transform.Origin.Z));
   foreach(var shape in Hulls)foreach(var vertex in ((ConvexPolygonShape3D)shape.Shape).Points){var p=state.Transform*shape.Transform*vertex;MaximumPenetration=Mathf.Max(MaximumPenetration,Mathf.Max(p.X,p.Z));}
   for(int i=0;i<state.GetContactCount();i++){
    var collider=state.GetContactColliderObject(i) as Node;var normal=state.GetContactLocalNormal(i);ContactSamples++;if(collider?.Name.ToString()=="SharpWallX"&&Mathf.Abs(normal.X)>.7f)XWallContact=true;if(collider?.Name.ToString()=="SharpWallZ"&&Mathf.Abs(normal.Z)>.7f)ZWallContact=true;
    if(Witnesses.Count<32)Witnesses.Add(new{step=Steps,collider=collider?.Name.ToString(),normal=A(normal),bodyWitness=A(state.GetContactLocalPosition(i)),colliderWitness=A(state.GetContactColliderPosition(i)),velocity=A(state.LinearVelocity),angularVelocity=A(state.AngularVelocity)});
   }
  }
 }
 public static async Task<object> Run(Node3D parent,JsonElement bodyConfig,int car){
  if(car!=13)throw new Exception("Sharp-edge control currently scoped to actual captured car13");if(Engine.PhysicsTicksPerSecond!=240)throw new Exception("Sharp-edge control requires matching240Hz clock");if(!bodyConfig.GetProperty("ContinuousCd").GetBoolean())throw new Exception("Captured CCD must remain enabled");
  var rows=new List<object>();foreach(float speed in new[]{20f,60f,120f}){
   var fixture=new Node3D{Name="SharpCornerControl"};parent.AddChild(fixture);
   try{
    // Perpendicular solid barriers meet without a ground surface. Their inner faces are X=0/Z=0.
    var wallX=new StaticBody3D{Name="SharpWallX",CollisionLayer=2,CollisionMask=4,Position=new Vector3(.15f,100,0)};wallX.AddChild(new CollisionShape3D{Shape=new BoxShape3D{Size=new Vector3(.3f,12,40)}});fixture.AddChild(wallX);
    var wallZ=new StaticBody3D{Name="SharpWallZ",CollisionLayer=2,CollisionMask=4,Position=new Vector3(0,100,.15f)};wallZ.AddChild(new CollisionShape3D{Shape=new BoxShape3D{Size=new Vector3(40,12,.3f)}});fixture.AddChild(wallZ);
    var body=new Probe{Name="ActualSourceCar13",Mass=bodyConfig.GetProperty("Mass").GetSingle(),CenterOfMassMode=RigidBody3D.CenterOfMassModeEnum.Custom,CenterOfMass=V(bodyConfig.GetProperty("centerOfMass")),GravityScale=0,LinearDamp=bodyConfig.GetProperty("LinearDamp").GetSingle(),AngularDamp=bodyConfig.GetProperty("AngularDamp").GetSingle(),CanSleep=false,ContinuousCd=true,MaxContactsReported=32,CollisionLayer=4,CollisionMask=2,Position=new Vector3(-8,100,-8),Rotation=new Vector3(0,Mathf.Pi*.25f,0)};
    string path="res://assets/generated_native_car_hulls/car-13.json";using var manifest=JsonDocument.Parse(FileAccess.GetFileAsString(path));var files=manifest.RootElement.GetProperty("files");var names=manifest.RootElement.GetProperty("partNames");
    for(int i=0;i<files.GetArrayLength();i++){string resource=files[i].GetString();if(!resource.StartsWith("res://assets/generated_native_car_hulls/",StringComparison.Ordinal)||resource.Contains(".."))throw new Exception("Invalid source hull reference");var shape=GD.Load<ConvexPolygonShape3D>(resource);if(shape==null||shape.Points.Length<4)throw new Exception("Source hull missing");var node=new CollisionShape3D{Name="SourceBodyHull_"+names[i].GetString(),Shape=shape};body.AddChild(node);body.Hulls.Add(node);}
    if(body.Hulls.Count==0)throw new Exception("Empty actual source hull compound");fixture.AddChild(body);var direction=new Vector3(1,0,1).Normalized();body.LinearVelocity=direction*speed;body.AngularVelocity=Vector3.Zero;
    int requiredSteps=240;while(body.Steps<requiredSteps)await parent.ToSignal(parent.GetTree(),SceneTree.SignalName.PhysicsFrame);
    bool blocked=body.GlobalPosition.X<0&&body.GlobalPosition.Z<0&&body.MaximumOriginAdvance<0&&body.LastVelocity.Dot(direction)<speed*.25f;
    bool ok=body.ContinuousCd&&body.Finite&&body.ContactSamples>0&&body.XWallContact&&body.ZWallContact&&body.MaximumPenetration<=.05f&&blocked;
    rows.Add(new{speed,ok,physicsHz=Engine.PhysicsTicksPerSecond,fixtureGravityScale=0,mass=body.Mass,customCOM=A(body.CenterOfMass),sourceHullCount=body.Hulls.Count,ccd=body.ContinuousCd,body.ContactSamples,body.XWallContact,body.ZWallContact,body.MaximumPenetration,body.MaximumOriginAdvance,finalPosition=A(body.GlobalPosition),finalVelocity=A(body.LastVelocity),blocked,body.Finite,witnesses=body.Witnesses.ToArray()});
    if(!ok)throw new Exception("Sharp-edge source-car control failed at"+speed+"m/s: "+JsonSerializer.Serialize(rows));
   }finally{fixture.QueueFree();await parent.ToSignal(parent.GetTree(),SceneTree.SignalName.PhysicsFrame);await parent.ToSignal(parent.GetTree(),SceneTree.SignalName.PhysicsFrame);}
  }
  GD.Print("PASS actual source-car perpendicular sharp-edge blockage/CCD controls");return new{ok=true,car,rows,scope="Actual baked compound hulls against perpendicular0.3m barriers at20/60/120m/s,240Hz,fixture gravity0. Requires finite state, both wall normals/contact, <=50mm maximum source-vertex penetration and continued blockage. Not suspension, mesh-track or full-game acceptance."};
 }
}
}
