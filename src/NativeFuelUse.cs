using System;
using System.Text.Json;
using MySimulator.Rebuild;
namespace MySim {
public static class NativeFuelUse {
 static readonly Lazy<(float Energy,float Efficiency,float Idle)> profile=new(()=>{
  using var data=JsonDocument.Parse(Godot.FileAccess.GetFileAsString("res://data/native_fuel_profile.json"));var p=data.RootElement;float energy=p.GetProperty("joulesPerLitre").GetSingle(),eff=p.GetProperty("wheelEfficiency").GetSingle(),idle=p.GetProperty("idleLitresPerSecond").GetSingle();if(!float.IsFinite(energy)||energy<=0||!float.IsFinite(eff)||eff<=0||eff>1||!float.IsFinite(idle)||idle<0)throw new Exception("Invalid native fuel energy profile");return(energy,eff,idle);
 });
 public static void Consume(Vehicle car,float positiveCombustionWheelWork,float dt){if(!float.IsFinite(positiveCombustionWheelWork)||positiveCombustionWheelWork<0||!float.IsFinite(dt)||dt<0)throw new ArgumentOutOfRangeException("Native fuel work/time");if(car.fuel<=0){car.fuel=0;return;}var p=profile.Value;car.fuel=Math.Max(0,car.fuel-positiveCombustionWheelWork/(p.Energy*p.Efficiency)-p.Idle*dt);}
}
}
