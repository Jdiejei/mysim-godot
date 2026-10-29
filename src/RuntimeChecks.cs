using Godot;
using System;
using System.Collections.Generic;
using MySimulator.Rebuild;
namespace MySim {
public partial class RuntimeChecks : Node3D {
 int count; void Check(bool ok,string reason) { if(!ok) throw new Exception(reason); count++; }
 public override async void _Ready() {
  try {
   var wall=new StaticBody3D { CollisionLayer=2, CollisionMask=0 };
   wall.AddChild(new CollisionShape3D { Shape=new BoxShape3D { Size=new Vector3(30,4,.2f) }, Position=new Vector3(0,1,0) }); AddChild(wall);
   var floor=new StaticBody3D { CollisionLayer=1, CollisionMask=0 };
   floor.AddChild(new CollisionShape3D { Shape=new BoxShape3D { Size=new Vector3(100,.2f,100) }, Position=new Vector3(0,-.1f,0) });AddChild(floor);
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
   var g=Contact.Ground(space,new Vector3(4,0,-4),0);Check(g.near&&Math.Abs(g.y)<.01f,"ground height incorrect");
   var contained=new Vehicle {position=new Vec(0,-4),velocity=new Vec(10,0),speed=10,lateral=0}; Contact.MarkGood(contained,true);
   contained.position=new Vec(40,-4);contained.lateral=40;Check(Contact.Contain(contained,new Vec(39,-4),true,30),"containment missed");Check(contained.position.x==0,"containment did not restore last valid point");
   if (System.Environment.GetEnvironmentVariable("MYSIM_CONTACT_ONLY") != null) { GD.Print("PASS contact checks ",count," assertions"); GetTree().Quit(); return; }
   var rows=new List<object>();
   for(int ci=0;ci<CarCatalog.Count;ci++) {
    var car=CarLoader.Load(CarCatalog.Folders[ci]);AddChild(car);CarLoader.ApplyExact(car,ci);var wheels=CarLoader.AttachWheels(car,ci);
    var dims=CarLoader.MeasureSize(car);var wr=new List<object>();
    for(int wi=0;wi<4;wi++) {
     var spin=wheels[wi].GetNode<Node3D>("Spin"); var bound=new Aabb();bool first=true;
     foreach(var node in spin.GetChildren()) if(node is MeshInstance3D mesh && mesh.Visible) {
      var ab=mesh.Transform*mesh.GetAabb();bound=first?ab:bound.Merge(ab);first=false;
      var sc=mesh.Transform.Basis.Scale;Check(float.IsFinite(sc.X)&&Math.Abs(sc.X)>0,"invalid wheel transform");
     }
     wr.Add(new { wheel=wi, x=bound.Size.X,y=bound.Size.Y,z=bound.Size.Z,center=bound.GetCenter().ToString() });
     Check(Math.Abs(bound.GetCenter().X)<.08f,"wheel assembly offset from hub car "+ci);Check(!first,"missing wheel geometry car "+ci);Check(bound.Size.Y>.3f&&bound.Size.Y<1.5f,"wheel diameter invalid car "+ci);
     Check(Math.Abs(bound.Size.Y-bound.Size.Z)<.18f,"wheel radial bounds not circular car "+ci+" wheel "+wi+" bounds="+bound);
    }
    rows.Add(new {car=ci,name=CarCatalog.Names[ci],width=dims.X,height=dims.Y,length=dims.Z,wheels=wr});car.Free();
   }
   System.IO.File.WriteAllText(System.Environment.GetEnvironmentVariable("MYSIM_GEOMETRYOUT")??"outputs/repair-20261029/geometry.json",System.Text.Json.JsonSerializer.Serialize(rows,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
   GD.Print("PASS runtime contact and geometry ",count," assertions");GetTree().Quit();
  } catch(Exception e) { GD.PrintErr("FAIL runtime checks ",e);GetTree().Quit(1); }
 }
}}
