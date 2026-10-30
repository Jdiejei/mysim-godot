using Godot;
using System;
using System.Collections.Generic;
using MySimulator.Rebuild;
namespace MySim {
public partial class RuntimeChecks : Node3D {
 int count; void Check(bool ok,string reason) { if(!ok) throw new Exception(reason); count++; }
 public override async void _Ready() {
  try {
   var windingRoot=new Node3D();AddChild(windingRoot);var plane=new PlaneMesh();var good=new MeshInstance3D {Mesh=plane};windingRoot.AddChild(good);Check(WindingFixer.Fix(windingRoot,out _)==0,"healthy Godot clockwise plane was reversed");
   var planeArrays=plane.SurfaceGetArrays(0);var indices=(int[])planeArrays[(int)Mesh.ArrayType.Index];for(int i=0;i<indices.Length;i+=3){int tmp=indices[i+1];indices[i+1]=indices[i+2];indices[i+2]=tmp;}planeArrays[(int)Mesh.ArrayType.Index]=indices;var reversed=new ArrayMesh();reversed.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,planeArrays);good.Mesh=reversed;
   Check(WindingFixer.Fix(windingRoot,out _)==1,"reversed plane was not repaired");Check(WindingFixer.Fix(windingRoot,out _)==0,"winding repair is not idempotent");windingRoot.QueueFree();
   var wall=new StaticBody3D { CollisionLayer=2, CollisionMask=0 };
   wall.AddChild(new CollisionShape3D { Shape=new BoxShape3D { Size=new Vector3(30,4,.2f) }, Position=new Vector3(0,1,0) }); AddChild(wall);
   var beam=new StaticBody3D {CollisionLayer=2,CollisionMask=0};beam.AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=new Vector3(3,.2f,.3f)},Position=new Vector3(-20,1.8f,-6)});AddChild(beam);
   var corner=new StaticBody3D {CollisionLayer=2,CollisionMask=0};corner.AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=new Vector3(8,3,.2f)},Position=new Vector3(36,1,-6)});corner.AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=new Vector3(.2f,3,8)},Position=new Vector3(38,1,-8)});AddChild(corner);
   var floor=new StaticBody3D { CollisionLayer=1, CollisionMask=0 };
   floor.AddChild(new CollisionShape3D { Shape=new BoxShape3D { Size=new Vector3(100,.2f,100) }, Position=new Vector3(0,-.1f,0) });AddChild(floor);
   var objects=new Node3D();AddChild(objects);
   var building=new MeshInstance3D {Name="arc_brick_test",Mesh=new BoxMesh {Size=new Vector3(3,3,3)},Position=new Vector3(20,1.5f,-6)};objects.AddChild(building);
   var road=new MeshInstance3D {Name="gnd_concrete_test",Mesh=new PlaneMesh()};objects.AddChild(road);
   var decal=new MeshInstance3D {Name="overlay_wall_scuff",Mesh=new PlaneMesh()};objects.AddChild(decal);
   Check(WallBuilder.IsSolid(building),"architecture lacks a solid role");Check(!WallBuilder.IsSolid(road),"road became a phantom wall");Check(!WallBuilder.IsSolid(decal),"decal became a phantom wall");Check(WallBuilder.Build(objects)==1,"physical role coverage wrong");
   for(int i=0;i<3;i++) await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   var space=GetWorld3D().DirectSpaceState;
   foreach(float length in new[]{4.06f,5.55f}) foreach(float angle in new[]{0f,.35f,-.35f}) {
    var v=new Vehicle { heading=angle, position=new Vec(0,8),velocity=new Vec(0,80),speed=80 };
    float before=v.speed;Check(Contact.Walls(space,v,new Vec(0,-8),0,new Vector3(2.2f,1,length)),"swept wall missed");
    Check(v.position.z < -length*.4f,"car tunneled through wall");Check(v.speed<=before+.001f,"wall added energy");
   }
   foreach(float speed in new[]{0f,10f,-10f}) {
    var v=new Vehicle { position=new Vec(0,-.5f),velocity=new Vec(0,speed),speed=Math.Abs(speed) };
    Check(Contact.Walls(space,v,new Vec(0,-.5f),0,new Vector3(2,1,4)),"initial overlap ignored");Check(v.position.z < -2f,"initial overlap not cleared");Check(v.speed<=Math.Abs(speed)+.01f,"overlap added energy");
   }
   var offsetHit=new Vehicle {position=new Vec(0,8),velocity=new Vec(0,80),speed=80};Check(Contact.Walls(space,offsetHit,new Vec(0,-8),0,new Vector3(2,1,4),new Vector3(0,.5f,1)),"offset body missed wall");Check(offsetHit.position.z < -3f,"visual nose crossed wall despite collision");
   var buildingHit=new Vehicle {position=new Vec(20,0),velocity=new Vec(0,80),speed=80};Check(Contact.Walls(space,buildingHit,new Vec(20,-14),0,new Vector3(2,1,4)),"drive-through architecture");Check(buildingHit.position.z < -9f,"car entered building footprint");
   var tall=new Vehicle {position=new Vec(-20,0),velocity=new Vec(0,30),speed=30};Check(Contact.Walls(space,tall,new Vec(-20,-14),0,Contact.BodySize(new Vector3(2,1.85f,4))),"tall car roof clipped overhead beam");Check(tall.position.z<-8,"tall car passed through overhead obstruction");
   var low=new Vehicle {position=new Vec(-20,0),velocity=new Vec(0,30),speed=30};Check(!Contact.Walls(space,low,new Vec(-20,-14),0,Contact.BodySize(new Vector3(2,1.3f,4))),"short car blocked by clear overhead beam");
   var wedged=new Vehicle {position=new Vec(37.5f,-6.5f),velocity=new Vec(),speed=0};Check(Contact.Walls(space,wedged,wedged.position,0,new Vector3(2,1,4)),"corner overlap ignored");
   var clearance=new PhysicsShapeQueryParameters3D {Shape=new BoxShape3D {Size=new Vector3(2,1,4)},Transform=new Transform3D(Basis.Identity,new Vector3(wedged.position.x,.5f,wedged.position.z)),CollisionMask=2,Margin=.01f};Check(space.IntersectShape(clearance).Count==0,"initial overlap remains against second corner wall");Check((wedged.lastIntegrated-wedged.position).Magnitude<.001f,"stationary corner recovery leaves stale integration pose");
   var g=Contact.Ground(space,new Vector3(4,0,-4),0);Check(g.near&&Math.Abs(g.y)<.01f,"ground height incorrect");
   var contained=new Vehicle {position=new Vec(0,-4),velocity=new Vec(10,0),speed=10,lateral=0}; Contact.MarkGood(contained,true);
   contained.position=new Vec(40,-4);contained.lateral=40;Check(Contact.Contain(contained,new Vec(39,-4),true,30),"containment missed");Check(contained.position.x==0,"containment did not restore last valid point");
   Check((contained.lastIntegrated-contained.position).Magnitude<.001f,"containment leaves stale integration pose");
   var testCircuit=new Circuit(0);var stale=new Vehicle {position=testCircuit.Point(.2f),lateral=40};Check(!Contact.Contain(stale,stale.position,true,30f,testCircuit),"stale pre-contact lateral coordinate caused false containment");Check(Math.Abs(stale.lateral)<.01f,"containment did not project corrected position");
   if (System.Environment.GetEnvironmentVariable("MYSIM_CONTACT_ONLY") != null) { GD.Print("PASS contact checks ",count," assertions"); GetTree().Quit(); return; }
   var rows=new List<object>();
   for(int ci=0;ci<CarCatalog.Count;ci++) {
    var car=CarLoader.Load(CarCatalog.Folders[ci]);AddChild(car);CarLoader.ApplyExact(car,ci);var wheels=CarLoader.AttachWheels(car,ci);
    var dims=CarLoader.MeasureSize(car);var wr=new List<object>();var partAudit=new List<object>();
    for(int wi=0;wi<4;wi++) {
     var spin=wheels[wi].GetNode<Node3D>("Spin"); var bound=new Aabb();bool first=true;
     foreach(var node in spin.GetChildren()) if(node is MeshInstance3D mesh && mesh.Visible) {
      var ab=mesh.Transform*mesh.GetAabb();bound=first?ab:bound.Merge(ab);first=false;
      var sc=mesh.Transform.Basis.Scale;Check(float.IsFinite(sc.X)&&Math.Abs(sc.X)>0,"invalid wheel transform");
     }
     GD.Print("WHEEL_GEOMETRY car=",ci," wheel=",wi," center=",bound.GetCenter()," size=",bound.Size);
     foreach(var child in spin.GetChildren())if(child is MeshInstance3D part&&part.Visible){var bounds=part.Transform*part.GetAabb();var materials=new List<object>();for(int si=0;si<part.Mesh.GetSurfaceCount();si++){var material=part.GetActiveMaterial(si);materials.Add(new {surface=si,type=material?.GetType().Name,texture=(material as StandardMaterial3D)?.AlbedoTexture?.ResourcePath});}partAudit.Add(new {wheel=wi,name=part.Name.ToString(),center=bounds.GetCenter().ToString(),size=bounds.Size.ToString(),materials});}
     wr.Add(new { wheel=wi, x=bound.Size.X,y=bound.Size.Y,z=bound.Size.Z,center=bound.GetCenter().ToString() });
     Check(Math.Abs(bound.GetCenter().X)<.08f,"wheel assembly offset from hub car "+ci);Check(!first,"missing wheel geometry car "+ci);Check(bound.Size.Y>.3f&&bound.Size.Y<1.5f,"wheel diameter invalid car "+ci);
     Check(Math.Abs(bound.Size.Y-bound.Size.Z)<.18f,"wheel radial bounds not circular car "+ci+" wheel "+wi+" bounds="+bound);
    }
    rows.Add(new {car=ci,name=CarCatalog.Names[ci],width=dims.X,height=dims.Y,length=dims.Z,wheels=wr,parts=partAudit});car.Free();
   }
   System.IO.File.WriteAllText(System.Environment.GetEnvironmentVariable("MYSIM_GEOMETRYOUT")??"outputs/repair-20261029/geometry.json",System.Text.Json.JsonSerializer.Serialize(rows,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
   GD.Print("PASS runtime contact and geometry ",count," assertions");GetTree().Quit();
  } catch(Exception e) { GD.PrintErr("FAIL runtime checks ",e);GetTree().Quit(1); }
 }
}}
