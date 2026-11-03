using Godot;
using System.Collections.Generic;
namespace MySim {
// Read-only actual rendered rubber support relative to the solver's current contact plane.
public static class LoadedWheelEvidence {
 public static object Capture(Node3D[] wheels,NativeSuspensionVehicle body){
  if(System.Environment.GetEnvironmentVariable("MYSIM_LOADED_WHEEL_EVIDENCE")!="1")return null;
  var rows=new List<object>();
  for(int i=0;i<4;i++){
   var normal=body.ContactNormals[i];bool supported=body.Grounded[i]&&normal.LengthSquared()>.5f;
   float minimum=float.PositiveInfinity;int count=0;
   foreach(var node in wheels[i].FindChildren("RebuiltRubberTyre","MeshInstance3D",true,false)){
    var mesh=(MeshInstance3D)node;if(!mesh.IsVisibleInTree()||mesh.Mesh==null)continue;
    for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++){
     using var arrays=mesh.Mesh.SurfaceGetArrays(surface);
     foreach(var point in arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array()){
      count++;if(supported)minimum=Mathf.Min(minimum,(mesh.GlobalTransform*point-body.ContactPoints[i]).Dot(normal));
     }
    }
   }
   rows.Add(new{wheel=i,supported,rubberVertices=count,minimumContactPlaneClearance=supported&&count>0?(float?)minimum:null,load=body.Loads[i],springLength=body.SpringLength[i],slip=body.LongitudinalSlip[i],slipAngle=body.LateralSlipAngle[i],temperature=body.Tyres[i].temperature,grip=body.Tyres[i].Grip(body.Water),water=body.Water,compound=(int)body.Tyres[i].compound});
  }
  return new{wheels=rows,scope="Actual transformed generated-rubber vertices against current native contact tangent plane; not curved-road intersections, swept clearance or suspension certification"};
 }
}
