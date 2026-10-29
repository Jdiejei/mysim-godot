using System;using MySimulator.Rebuild;
class RepairChecks {
 static int count;static void Check(bool ok,string why){if(!ok)throw new Exception(why);count++;}
 static void Main(){
 foreach(float dt in new[]{.02f,1f/120}) {var c=new Circuit(0);var v=new Vehicle();v.Place(c,0,0);v.input=new DriverInput(1,0,0);for(int i=0;i<(int)(3.2f/dt);i++)v.Step(c,dt,0,true,i*dt,1);Console.WriteLine($"PROBE dt={dt} speed={v.speed} lat={v.lateral} long={v.dbgLong} force={v.dbgDriveF}");}
 var ped=new PedalController();DriverInput input=default;for(int i=0;i<40;i++)input=ped.Update(0,1,0,false,0,1f/120,true);Check(input.reverse,"reverse didn't engage");
 input=ped.Update(0,1,0,false,-6,1f/120,true);Check(input.reverse,"reverse disengaged at speed");input=ped.Update(1,0,0,false,-6,1f/120,true);Check(!input.reverse&&input.throttle==0&&input.brake==1,"forward pedal must brake reverse first");input=ped.Update(1,0,0,false,0,1f/120,true);Check(!input.reverse&&input.throttle==1,"forward didn't engage after stopping");
 input=ped.Update(0,1,0,false,15,1,true);Check(!input.reverse,"braking at speed selected reverse");
 for(int ac=0;ac<CarCatalog.Count;ac++)for(int bc=0;bc<CarCatalog.Count;bc++){
  var r=new Race(0,1,0);var a=r.Add("a",true,Compound.Medium);var b=r.Add("b",true,Compound.Medium);a.carModel=ac;b.carModel=bc;a.heading=0;b.heading=MathUtil.Pi;
  a.position=new Vec(0,0);b.position=new Vec(0,(VehicleDimensions.Length[ac]+VehicleDimensions.Length[bc])*.5f-.1f);a.velocity=new Vec(0,30);b.velocity=new Vec(0,-15);
  float ma=RecoveredVehicleProfiles.Mass[ac],mb=RecoveredVehicleProfiles.Mass[bc];float energy=ma*900+mb*225,momentum=ma*30-mb*15;
  r.Collisions();Check(ma*a.speed*a.speed+mb*b.speed*b.speed<=energy+2,"contact added energy");Check(Math.Abs(ma*a.velocity.z+mb*b.velocity.z-momentum)<.2f,"contact lost momentum");Check(a.damage>0&&b.damage>0,"impact damage missing");
  Check(Math.Abs(b.position.z-a.position.z)>=(VehicleDimensions.Length[ac]+VehicleDimensions.Length[bc])*.5f-.001f,"footprints remain overlapped");
 }
 // A lateral gap larger than the body widths must not cause phantom contact.
 {var r=new Race(0,1,0);var a=r.Add("a",true,Compound.Medium);var b=r.Add("b",true,Compound.Medium);a.heading=0;b.heading=0;a.position=new Vec(0,0);b.position=new Vec((VehicleDimensions.Width[a.carModel]+VehicleDimensions.Width[b.carModel])*.5f+.1f,0);var old=b.position;r.Collisions();Check(b.position.x==old.x,"phantom side collision");}
 {var r=new Race(0,1,0);var a=r.Add("ahead",true,Compound.Medium);var b=r.Add("behind",true,Compound.Medium);a.progress=.1f;a.nextGate=1;b.progress=.98f;b.nextGate=1;Check(r.Standings()[0]==a,"start-line ranking wrapped incorrectly");b.totalDistance=99999;Check(r.Standings()[0]==a,"distance travelled incorrectly grants race position");}
 Console.WriteLine($"PASS repair checks {count} assertions");
 }
}
