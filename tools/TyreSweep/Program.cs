using System;
using System.IO;
using System.Globalization;
using System.Text.Json;
using MySimulator.Rebuild;
CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
Directory.CreateDirectory("outputs/tyre-sweep");
(float Slip,float Force,float Omega,float Ratio) Measure(float radius,float load,float forward,float angle,Compound compound,float water,string mode,bool tc,bool abs){
 var tyre=new Tyre {compound=compound,temperature=compound==Compound.Wet?65:88,wear=0};float mu=tyre.Grip(water)*1.18f,capacity=mu*load,inertia=1.6f*radius*radius/(.33f*.33f),omega=forward/radius,slip=0,force=0,ratio=0;int samples=0;
 for(int step=0;step<720;step++){RotationalTyreContact.Step(ref omega,radius,inertia,load,mu,forward,(float)Math.Tan(angle)*Math.Max(Math.Abs(forward),2),mode=="drive"?capacity*2:0,mode=="brake"?capacity*2:0,mode=="handbrake"?capacity*2:0,tc,abs,1f/240,out float fx,out float fy,out float kappa,out float alpha);
  if(!float.IsFinite(omega)||!float.IsFinite(fx)||!float.IsFinite(fy)||!float.IsFinite(kappa))throw new Exception("Nonfinite tyre sweep result");ratio=Math.Max(ratio,(float)Math.Sqrt(fx*fx+fy*fy)/capacity);if(ratio>1.0001f)throw new Exception("Combined tyre force exceeded friction circle");if(step>=480){samples++;slip+=Math.Abs(kappa);force+=Math.Abs(fx);}
 }
 return(slip/samples,force/samples,omega,ratio);
}
int count=0;float maxRatio=0;
using(var csv=new StreamWriter("outputs/tyre-sweep/constant-speed-contact.csv")){csv.WriteLine("radius_m,normal_load_N,forward_mps,slip_angle_rad,compound,water,mode,tc,abs,mean_abs_kappa,mean_abs_force_N,final_omega_rad_s,max_friction_circle_ratio");
 foreach(float radius in new[]{.25f,.33f,.4f})foreach(float load in new[]{500f,2000f,5000f})foreach(float speed in new[]{0f,5f,20f,50f})foreach(float angle in new[]{0f,.06f,.15f})foreach(Compound compound in Enum.GetValues<Compound>())foreach(float water in new[]{0f,.5f,1f})foreach(string mode in new[]{"coast","drive","brake","handbrake"})foreach(bool tc in new[]{false,true})foreach(bool abs in new[]{false,true}){var value=Measure(radius,load,speed,angle,compound,water,mode,tc,abs);maxRatio=Math.Max(maxRatio,value.Ratio);csv.WriteLine($"{radius:R},{load:R},{speed:R},{angle:R},{compound},{water:R},{mode},{tc},{abs},{value.Slip:R},{value.Force:R},{value.Omega:R},{value.Ratio:R}");count++;}
}
var driveOff=Measure(.33f,2000,20,0,Compound.Medium,0,"drive",false,false);var driveOn=Measure(.33f,2000,20,0,Compound.Medium,0,"drive",true,false);var brakeOff=Measure(.33f,2000,20,0,Compound.Medium,0,"brake",false,false);var brakeOn=Measure(.33f,2000,20,0,Compound.Medium,0,"brake",false,true);var handOff=Measure(.33f,2000,20,0,Compound.Medium,0,"handbrake",false,false);var handOn=Measure(.33f,2000,20,0,Compound.Medium,0,"handbrake",false,true);
bool all=count==20736&&driveOn.Slip<driveOff.Slip*.5f&&brakeOn.Slip<brakeOff.Slip*.6f&&brakeOn.Force>brakeOff.Force*1.01f&&handOn.Slip>.8f&&Math.Abs(handOn.Slip-handOff.Slip)<.000001f;
File.WriteAllText("outputs/tyre-sweep/summary.json",JsonSerializer.Serialize(new {all,cases=count,maxRatio,driveOff,driveOn,brakeOff,brakeOn,handOff,handOn,scope="Controlled constant-speed, fixed-temperature/wear native contact-model bench; each parameter explicit inCSV. Our model only, not Monoposto recovered physics or complete3D vehicle/real-world calibration."},new JsonSerializerOptions {IncludeFields=true,WriteIndented=true}));
Console.WriteLine(all?"PASS controlled tyre sweep, TC/ABS slip response and handbrake independence":"FAIL controlled tyre assist or contact invariants");return all?0:1;
