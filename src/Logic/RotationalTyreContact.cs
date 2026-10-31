using System;
namespace MySimulator.Rebuild {
// Wheel angular momentum coupled implicitly to the existing combined-slip tyre law.
// The body still receives forces at its real 3D contact; no chassis pose is assigned.
public static class RotationalTyreContact {
 public static void Step(ref float omega,float radius,float inertia,float normalLoad,float mu,float forward,float side,float driveForce,float brakeForce,float handbrakeForce,bool tc,bool abs,float dt,out float fx,out float fy,out float kappa,out float alpha){
  float reference=Math.Max(Math.Abs(forward),2);kappa=(omega*radius-forward)/reference;alpha=(float)Math.Atan2(side,reference);
  float drive=driveForce*radius;if(tc&&drive>0)drive*=MathUtil.Clamp(1-(kappa-.10f)*3,.05f,1);
  float brake=brakeForce*radius;if(abs&&brakeForce>0)brake*=MathUtil.Clamp(1-(-kappa-.13f)/.08f,.15f,1);brake+=handbrakeForce*radius;
  Dynamics.ContactTyreForces(normalLoad,mu,kappa,alpha,out float oldForce,out _);Dynamics.ContactTyreForces(normalLoad,mu,kappa+.02f,alpha,out float sampledForce,out _);
  float slope=Math.Max(normalLoad*mu*.05f,(sampledForce-oldForce)/.02f),denominator=1+dt*radius*radius*slope/(inertia*reference);
  float free=omega+dt/inertia*(drive-radius*oldForce)/denominator;omega=Math.Sign(free)*Math.Max(0,Math.Abs(free)-dt/inertia*brake/denominator);
  kappa=(omega*radius-forward)/reference;Dynamics.ContactTyreForces(normalLoad,mu,kappa,alpha,out fx,out fy);
 }
 public static void Airborne(ref float omega,float radius,float inertia,float driveForce,float brakeForce,float dt){float free=omega+driveForce*radius/inertia*dt;omega=Math.Sign(free)*Math.Max(0,Math.Abs(free)-brakeForce*radius/inertia*dt);}
}
}
