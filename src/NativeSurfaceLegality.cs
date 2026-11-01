using Godot;
namespace MySim {
public static class NativeSurfaceLegality {
 // Suspension unloading is not a track-limits violation. Require a real nearby
 // drive mesh below the chassis and the existing route bounds; never move the car.
 public static bool IsLegalWithWheelSupport(PhysicsDirectSpaceState3D space,Vector3 chassis,bool insideRoute,bool[] grounded){
  if(!insideRoute)return false;var ground=Contact.Ground(space,chassis,chassis.Y,float.MaxValue);
  if(ground.found)return ground.near&&ground.rawY<=chassis.Y;
  // An actual four-wheel support footprint can span a hole in the centre ray.
  // All four support flags come from native wheel rays against drive-surface layer1.
  return grounded!=null&&grounded.Length==4&&grounded[0]&&grounded[1]&&grounded[2]&&grounded[3];
 }
 public static bool IsLegal(PhysicsDirectSpaceState3D space,Vector3 chassis,bool insideRoute){if(!insideRoute)return false;var ground=Contact.Ground(space,chassis,chassis.Y,float.MaxValue);return ground.near&&ground.rawY<=chassis.Y;}
}
}
