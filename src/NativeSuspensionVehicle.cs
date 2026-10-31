using Godot;
using MySimulator.Rebuild;
using System;
namespace MySim {
// Experimental force-driven native rigid body. No pose snapping, height clamping or heading assignment.
// Not production-integrated until suspension, tyre/contact and real-track gates pass.
public partial class NativeSuspensionVehicle : RigidBody3D {
 Vehicle raceVehicle;float emptyStoppedTime;public bool DamageEnabled;public readonly NativeImpactDamage Impacts=new();
 public void BindRaceControls(Vehicle vehicle,float dt){raceVehicle=vehicle;Controls=vehicle.servicing||vehicle.retired?new DriverInput(0,1,0):vehicle.input;TractionControl=vehicle.traction;BrakeStabilityAssist=vehicle.abs;if(vehicle.servicing||vehicle.retired){HybridDriveForce=0;vehicle.ersActive=false;}else HybridDriveForce=HybridDeployment.Step(vehicle,vehicle.input,dt)*vehicle.input.throttle;}
 public bool RotationalTyres;public readonly float[] WheelOmega=new float[4],WheelRotation=new float[4],LongitudinalSlip=new float[4],LateralSlipAngle=new float[4];
 public void SeedRollingVelocity(Vector3 velocity){Impacts.ResetHistory();LinearVelocity=velocity;float speed=velocity.Dot(GlobalBasis.Z);for(int i=0;i<4;i++){WheelOmega[i]=speed/radii[i];LongitudinalSlip[i]=0;}}
 void FreeWheel(int i,DriverInput input,float engine,float dt){if(!RotationalTyres)return;float brake=(input.reverse?0:input.brake)*brakeDecel*Mass*(i<2?.29f:.21f)+(input.handbrake&&i>=2?Mass*1.5f:0);float inertia=1.6f*radii[i]*radii[i]/(.33f*.33f);RotationalTyreContact.Airborne(ref WheelOmega[i],radii[i],inertia,i>=2?engine*.5f:0,brake,dt);WheelRotation[i]=Mathf.Wrap(WheelRotation[i]+WheelOmega[i]*dt,-Mathf.Pi,Mathf.Pi);}
 public int CarIndex;public float HybridDriveForce;public DriverInput Controls;public bool TractionControl=true,BrakeStabilityAssist=true;public float Water;public Compound Compound=Compound.Medium;
 public readonly System.Collections.Generic.List<(CollisionShape3D Shape,int Hub)> MountedShapes=new();
 public override void _PhysicsProcess(double delta){foreach(var entry in MountedShapes){int hub=entry.Hub;var centre=RecoveredWheelCentres[hub];var rotation=new Basis(Vector3.Up,hub<2?WheelAngle:0);var offset=Vector3.Up*(RestLength-StaticSag-SpringLength[hub]);entry.Shape.Transform=new Transform3D(rotation,centre+offset-rotation*centre);}}
 public string[] ContactParts=Array.Empty<string>();public int ChassisContacts;public readonly Vector3[] ContactForces=new Vector3[4],ContactNormals=new Vector3[4];
 public readonly bool[] Grounded=new bool[4];public readonly float[] Loads=new float[4];public readonly float[] SpringLength=new float[4];
 public Vector3[] RecoveredWheelCentres;public float[] RecoveredWheelRadii;
 public float WheelRadius=.33f,RestLength=.18f,StaticSag=.07f,DampingRatio=.7f;public int Contacts;public float WheelAngle;
 public float TyreLoadRatio {get{float sum=0;foreach(float load in Loads)sum+=load;return sum/(Mass*9.81f);}}
 public Tyre[] Tyres=>tyres;
 Vector3[] mounts=new Vector3[4];float[] radii=new float[4];Tyre[] tyres={new Tyre(),new Tyre(),new Tyre(),new Tyre()};float power,drag,brakeDecel;
 public override void _Ready(){
  DamageEnabled|=System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_DAMAGE")=="1";RotationalTyres=System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_ROTATIONAL_TYRES")=="1";CarIndex=Math.Clamp(CarIndex,0,23);NativeCarColliders.ConfigureWheels(this,CarIndex);Mass=RecoveredVehicleProfiles.Mass[CarIndex];CanSleep=false;ContinuousCd=true;MaxContactsReported=16;LinearDamp=0;AngularDamp=.15f;CollisionLayer=4;CollisionMask=7;
  CenterOfMassMode=CenterOfMassModeEnum.Custom;CenterOfMass=new Vector3(0,-.3f,0);
  float wheelbase=RecoveredGeometry.Wheelbase[CarIndex],track=VehicleDimensions.Width[CarIndex]*.8f;
  if((RecoveredWheelCentres==null)!=(RecoveredWheelRadii==null)||RecoveredWheelCentres!=null&&(RecoveredWheelCentres.Length!=4||RecoveredWheelRadii.Length!=4))throw new InvalidOperationException("Recovered wheel contact configuration incomplete");
  for(int i=0;i<4;i++){radii[i]=RecoveredWheelRadii==null?WheelRadius:RecoveredWheelRadii[i];if(!float.IsFinite(radii[i])||radii[i]<.15f||radii[i]>.6f)throw new InvalidOperationException("Recovered wheel radius invalid");mounts[i]=RecoveredWheelCentres==null?new Vector3(i%2==0?-track*.5f:track*.5f,-.32f+RestLength-StaticSag,i<2?wheelbase*.5f:-wheelbase*.5f):RecoveredWheelCentres[i]+Vector3.Up*(RestLength-StaticSag);tyres[i].Replace(Compound);}
  if(!NativeCarColliders.Install(this,CarIndex)){AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=new Vector3(track,.5f,wheelbase+.5f)}});GD.Print("NATIVE_COLLISION_FIXTURE_PROXY car=",CarIndex);}
  float vmax=RecoveredVehicleProfiles.Speed[CarIndex]*.44704f;power=Mass*26.82f*26.82f/(2*RecoveredVehicleProfiles.Acceleration[CarIndex])*1.35f;drag=power/(vmax*vmax*vmax);brakeDecel=26.82f*26.82f/(2*RecoveredVehicleProfiles.Brakes[CarIndex]*.3048f);
 }
 public override void _IntegrateForces(PhysicsDirectBodyState3D state){
  if(DamageEnabled)Impacts.Observe(state,raceVehicle,CarIndex);var input=Controls.Safe();var pose=state.Transform;var up=pose.Basis.Y.Normalized();var forward=pose.Basis.Z.Normalized();float longitudinal=state.LinearVelocity.Dot(forward),speed=state.LinearVelocity.Length();
  WheelAngle=Mathf.MoveToward(WheelAngle,Dynamics.SteeringTarget(input.steer,longitudinal),3.2f*state.Step);
  float gravity=state.TotalGravity.Length(),spring=Mass*gravity/(4*StaticSag),damper=2*DampingRatio*Mathf.Sqrt(spring*Mass*.25f);
  float motorSpeed=RotationalTyres?Mathf.Max(Mathf.Abs(longitudinal),Mathf.Abs((WheelOmega[2]*radii[2]+WheelOmega[3]*radii[3])*.5f)):Mathf.Abs(longitudinal);
  float engine=input.reverse?-Mass*3.5f*(input.throttle+input.brake)*.5f:Mathf.Min(power/Mathf.Max(motorSpeed,6),Mass*gravity*2.2f)*input.throttle;
  if(raceVehicle!=null&&(raceVehicle.fuel<=0||raceVehicle.retired)){engine=0;if(raceVehicle.retired)HybridDriveForce=0;}if(DamageEnabled&&raceVehicle!=null)engine*=NativeImpactDamage.PowerFactor(raceVehicle);float combustionForce=engine;
  if(!input.reverse)engine+=Mathf.Max(0,HybridDriveForce);float combustionShare=Mathf.Abs(engine)>.001f?Mathf.Abs(combustionForce/engine):0,combustionWork=0;
  Contacts=0;ChassisContacts=state.GetContactCount();ContactParts=new string[ChassisContacts];for(int c=0;c<ChassisContacts;c++){int shape=state.GetContactLocalShape(c);uint owner=ShapeFindOwner(shape);ContactParts[c]=(ShapeOwnerGetOwner(owner) as Node)?.Name.ToString()??"UnknownBodyShape";}
  for(int i=0;i<4;i++){
   Grounded[i]=false;Loads[i]=0;ContactForces[i]=Vector3.Zero;ContactNormals[i]=Vector3.Zero;SpringLength[i]=RestLength+.15f;var mount=pose*mounts[i];
   using var query=PhysicsRayQueryParameters3D.Create(mount,mount-up*(radii[i]+RestLength+.15f),1);
   var hit=state.GetSpaceState().IntersectRay(query);if(hit.Count==0){FreeWheel(i,input,engine,state.Step);continue;}
   var point=(Vector3)hit["position"];var normal=((Vector3)hit["normal"]).Normalized();ContactNormals[i]=normal;hit.Clear();
   float length=Mathf.Max(0,mount.DistanceTo(point)-radii[i]);SpringLength[i]=length;
   var arm=point-pose.Origin;var velocity=state.GetVelocityAtLocalPosition(arm);
   float load=Mathf.Clamp(spring*(RestLength-length)-damper*velocity.Dot(normal),0,Mass*gravity);if(load<=0){FreeWheel(i,input,engine,state.Step);continue;}
   Grounded[i]=true;Loads[i]=load;Contacts++;state.ApplyForce(normal*load,arm);
   var rolling=(i<2?forward.Rotated(up,WheelAngle):forward);rolling=(rolling-normal*rolling.Dot(normal)).Normalized();var right=rolling.Cross(normal).Normalized();
   float u=velocity.Dot(rolling),side=velocity.Dot(right),mu=tyres[i].Grip(Water)*1.18f*Dynamics.GripMul(CarIndex),capacity=mu*load;
   float service=input.reverse?0:input.brake*brakeDecel*Mass*(i<2?.29f:.21f);float handbrake=input.handbrake&&i>=2?Mass*1.5f:0;if(!RotationalTyres)service+=handbrake;
   // A brake impulse cannot reverse a wheel's supported share of body momentum.
   if(!RotationalTyres)service=Mathf.Min(service,Mathf.Abs(u)*Mass/(4*Mathf.Max(state.Step,.00001f)));
   float alpha=Mathf.Atan2(side,Mathf.Max(Mathf.Abs(u),1));float lateralDemand=-capacity*(float)Math.Tanh(alpha/.12f);
   bool reserveLateral=(TractionControl&&i>=2&&engine>0&&input.brake<.01f&&!input.handbrake)||(BrakeStabilityAssist&&input.brake>.01f&&!input.reverse&&!(input.handbrake&&i>=2));
   float drive=i>=2?engine*.5f:0;
   // Torque management preserves the grip requested by measured lateral slip instead of
   // spending the whole rear friction circle on drive and allowing power oversteer.
   float longitudinalBudget=reserveLateral?Mathf.Sqrt(Mathf.Max(0,capacity*capacity-lateralDemand*lateralDemand)):capacity;
   float fx=Mathf.Clamp(drive-Mathf.Sign(u)*service,-longitudinalBudget,longitudinalBudget);float remaining=Mathf.Sqrt(Mathf.Max(0,capacity*capacity-fx*fx));
   float fy=reserveLateral?lateralDemand:-remaining*(float)Math.Tanh(alpha/.12f);
   if(RotationalTyres){float inertia=1.6f*radii[i]*radii[i]/(.33f*.33f);RotationalTyreContact.Step(ref WheelOmega[i],radii[i],inertia,load,mu,u,side,drive,service,handbrake,TractionControl&&i>=2,BrakeStabilityAssist,state.Step,out fx,out fy,out LongitudinalSlip[i],out LateralSlipAngle[i]);alpha=LateralSlipAngle[i];WheelRotation[i]=Mathf.Wrap(WheelRotation[i]+WheelOmega[i]*state.Step,-Mathf.Pi,Mathf.Pi);}
   // Passive contact damping must not reverse a patch's lateral momentum in one solver step.
   // Include angular inertia at the contact lever, then share the budget over all four patches.
   var lateralLever=(arm-state.CenterOfMass).Cross(right);float inverseEffectiveMass=state.InverseMass+lateralLever.Dot(state.InverseInertiaTensor*lateralLever);
   float lateralStop=Mathf.Abs(side)/(4*Mathf.Max(state.Step,.00001f)*Mathf.Max(inverseEffectiveMass,.0000001f));fy=Mathf.Clamp(fy,-lateralStop,lateralStop);
   if(i>=2)combustionWork+=Mathf.Max(0,Mathf.Sign(engine)*fx)*Mathf.Abs(u)*state.Step*combustionShare;
   float roll=Mathf.Min(load*.014f,Mathf.Abs(u)*Mass/(4*Mathf.Max(state.Step,.00001f)));
   var tangentialForce=rolling*(fx-Mathf.Sign(u)*roll)+right*fy;ContactForces[i]=normal*load+tangentialForce;state.ApplyForce(tangentialForce,arm);
   tyres[i].Step(state.Step,Mathf.Abs(u),Mathf.Sqrt(alpha*alpha+(RotationalTyres?.25f*LongitudinalSlip[i]*LongitudinalSlip[i]:0)),load/Mathf.Max(1,Mass*gravity*.25f),Water);
  }
  if(raceVehicle!=null){NativeFuelUse.Consume(raceVehicle,combustionWork,state.Step);if(!raceVehicle.finished&&!raceVehicle.retired&&!raceVehicle.servicing&&raceVehicle.fuel<=0&&state.LinearVelocity.Length()<.5f){emptyStoppedTime+=state.Step;if(emptyStoppedTime>=2){raceVehicle.retired=true;raceVehicle.retirementReason="OUT OF FUEL";}}else emptyStoppedTime=0;}
  float damageDrag=DamageEnabled&&raceVehicle!=null?NativeImpactDamage.DragFactor(raceVehicle):1,damageAero=DamageEnabled&&raceVehicle!=null?NativeImpactDamage.AeroFactor(raceVehicle):1;state.ApplyCentralForce(-state.LinearVelocity*speed*drag*damageDrag-up*(Dynamics.DownK(CarIndex)*speed*speed*damageAero));
 }
}
}
