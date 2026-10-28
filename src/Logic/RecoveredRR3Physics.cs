using System;
namespace MySimulator.Rebuild {
// Translated from libRealRacing3.so ELF 0x1586e9c; assembly and bounded reconstruction retained.
// Inputs come from RR3 suspension data + vehicle mass. Current game profiles are locally tuned inputs.
public static class RecoveredRR3Physics {
 public struct Inertia {public float x,y,z;}
 public static Inertia BoxInertia(float massKg,float width,float length,float height){float w=(int)(width*128)*2/256f,l=(int)(length*128)*2/256f,h=(int)(height*256)/256f;float factor=massKg/12f;return new Inertia{x=factor*(l*l+h*h),y=factor*(w*w+l*l),z=factor*(w*w+h*h)};}
 public struct SuspensionSetup {public float stiffness,compressionDamping,reboundDamping,bumpTravel;}
 public static SuspensionSetup Suspension(float massKg,float gravity,float staticDeflection,float compressionRatio,float reboundRatio){
  if(massKg<=0||gravity<=0||staticDeflection<=0)throw new ArgumentOutOfRangeException("Suspension inputs must be positive");
  float cornerMass=massKg*.25f;float stiffness=cornerMass*gravity/staticDeflection;
  return new SuspensionSetup{stiffness=stiffness,compressionDamping=2*compressionRatio*(float)Math.Sqrt(cornerMass*stiffness),reboundDamping=2*reboundRatio*(float)Math.Sqrt(cornerMass*stiffness),bumpTravel=cornerMass*.13f*gravity/stiffness};
 }
 // The native calculation squares the normalized speed BEFORE evaluating smoothstep.
 // Speed units / per-car threshold and boost values must come from the native car profile.
 // This curve is retained for recovery, and is not enabled with guessed native speed units.
 public static float NativeAeroBoost(int nativeSpeed,float speedMin,float speedMax,float boostMin,float boostMax){
  if(speedMax<=speedMin)return 0;
  float speed=unchecked(nativeSpeed*559)*(1f/65536f);float t=Math.Max(0,Math.Min(1,(speed-speedMin)/(speedMax-speedMin)));t*=t;
  return boostMin+(t*t*3-t*t*(t+t))*(boostMax-boostMin);
 }
}
}
