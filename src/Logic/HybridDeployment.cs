using System;
namespace MySimulator.Rebuild {
 // Overtake/ERS: hybrid cars harvest under braking and deploy extra power, automatically or on demand.
 public static class HybridDeployment {
  public const float DeployKw=120f,FullDeploySeconds=33f,HarvestPerBrakeSecond=.045f;
  public static bool IsHybrid(int model){return (model>=15&&model<=19)||model==21;}
  public static bool Deploying(Vehicle v,DriverInput input){
   if(!IsHybrid(v.carModel)||v.ersCharge<=.001f||input.reverse||input.brake>.05f||input.throttle<.6f||v.speed<8||v.pitActive)return false;
   return v.ersAuto?(input.throttle>.95f&&v.speed>20&&v.ersCharge>.12f):v.ersOvertake;
  }
  // Returns extra longitudinal force (N) and updates the battery state.
  public static float Step(Vehicle v,DriverInput input,float dt){
   if(!IsHybrid(v.carModel)||dt<=0){v.ersActive=false;return 0;}
   if(input.brake>.05f&&v.speed>5)v.ersCharge=Math.Min(1,v.ersCharge+input.brake*HarvestPerBrakeSecond*dt);
   v.ersActive=Deploying(v,input);if(!v.ersActive)return 0;
   v.ersCharge=Math.Max(0,v.ersCharge-dt/FullDeploySeconds);
   return DeployKw*1000f/Math.Max(v.speed,12f);
  }
 }
}
