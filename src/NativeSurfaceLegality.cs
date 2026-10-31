using Godot;
namespace MySim {
public static class NativeSurfaceLegality {
 // Suspension unloading is not a track-limits violation. Require a real nearby
 // drive mesh below the chassis and the existing route bounds; never move the car.
 public static bool IsLegal(PhysicsDirectSpaceState3D space,Vector3 chassis,bool insideRoute){if(!insideRoute)return false;var ground=Contact.Ground(space,chassis,chassis.Y,float.MaxValue);return ground.near&&ground.rawY<=chassis.Y;}
}
}
