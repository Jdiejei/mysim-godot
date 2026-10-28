using System;
using System.Collections.Generic;
namespace MySimulator.Rebuild
{
    [Serializable] public struct Vec
    {
        public float x,z;
        public Vec(float x,float z){this.x=x;this.z=z;}
        public float Magnitude {get{return (float)Math.Sqrt(x*x+z*z);}}
        public Vec Normalized {get{float m=Magnitude;return m>0.0001f?this/m:new Vec(0,1);}}
        public static Vec operator +(Vec a,Vec b){return new Vec(a.x+b.x,a.z+b.z);}
        public static Vec operator -(Vec a,Vec b){return new Vec(a.x-b.x,a.z-b.z);}
        public static Vec operator *(Vec a,float b){return new Vec(a.x*b,a.z*b);}
        public static Vec operator /(Vec a,float b){return new Vec(a.x/b,a.z/b);}
        public static float Dot(Vec a,Vec b){return a.x*b.x+a.z*b.z;}
        public Vec Right {get{return new Vec(z,-x);}}
    }
    public static class MathUtil
    {
        public const float Pi=(float)Math.PI;
        public static float Clamp(float v,float lo,float hi){return Math.Max(lo,Math.Min(hi,v));}
        public static float Lerp(float a,float b,float t){return a+(b-a)*Clamp(t,0,1);}
        public static float Wrap(float a){while(a>Pi)a-=2*Pi;while(a<-Pi)a+=2*Pi;return a;}
        public static float Wrap01(float a){return a-(float)Math.Floor(a);}
        public static bool Finite(float a){return !float.IsNaN(a)&&!float.IsInfinity(a);}
    }
    public enum Compound {Soft,Medium,Hard,Wet}
    [Serializable] public class Tyre
    {
        public Compound compound=Compound.Medium;
        public float wear,temperature=72;
        public float Grip(float wetness)
        {
            float baseGrip=compound==Compound.Soft?1.16f:compound==Compound.Hard?.98f:compound==Compound.Wet?1.03f:1.07f;
            float optimal=compound==Compound.Wet?65:88;
            float thermal=MathUtil.Clamp(1-Math.Abs(temperature-optimal)*.004f,.56f,1);
            float condition=1-.42f*wear-.25f*MathUtil.Clamp((wear-.75f)*4,0,1);
            float weather=compound==Compound.Wet?MathUtil.Lerp(.75f,1,wetness):MathUtil.Lerp(1,.38f,wetness);
            return baseGrip*thermal*condition*weather;
        }
        public void Step(float dt,float speed,float slip,float load,float wetness,float wearScale=1)
        {
            float cooling=(temperature-(22+wetness*8))*(.005f+speed*.00035f);
            float heating=Math.Abs(slip)*speed*.16f+speed*.017f;
            temperature=MathUtil.Clamp(temperature+(heating-cooling)*dt,20,170);
            float durability=compound==Compound.Soft?1.6f:compound==Compound.Hard?.65f:1;
            if(compound==Compound.Wet&&wetness<.2f)durability*=2.2f;
            wear=MathUtil.Clamp(wear+dt*durability*wearScale*(speed*.000015f+Math.Abs(slip)*speed*.00010f)*MathUtil.Clamp(load,.5f,2),0,1);
        }
        public void Replace(Compound next){compound=next;wear=0;temperature=next==Compound.Wet?60:72;}
    }
    [Serializable] public struct DriverInput
    {
        public float throttle,brake,steer;
        public bool handbrake,reverse;
        public DriverInput(float throttle,float brake,float steer){this.throttle=throttle;this.brake=brake;this.steer=steer;handbrake=false;reverse=false;}
        public DriverInput Safe(){return new DriverInput(MathUtil.Finite(throttle)?MathUtil.Clamp(throttle,0,1):0,MathUtil.Finite(brake)?MathUtil.Clamp(brake,0,1):0,MathUtil.Finite(steer)?MathUtil.Clamp(steer,-1,1):0){handbrake=handbrake,reverse=reverse};}
    }
    public class Circuit
    {
        public readonly Vec[] points;
        public readonly float[] cumulative;
        public readonly float length,width;
        public readonly int variant;
        public readonly string name;
        public Circuit(int variant)
        {
            this.variant=Math.Max(0,Math.Min(SourceRoutes.Points.Length-1,variant));name=SourceRoutes.Names[this.variant];
            width=12;points=(Vec[])SourceRoutes.Points[this.variant].Clone();cumulative=new float[points.Length+1];
            for(int i=1;i<points.Length;i++)cumulative[i]=cumulative[i-1]+(points[i]-points[i-1]).Magnitude;
            cumulative[points.Length]=cumulative[points.Length-1]+(points[0]-points[points.Length-1]).Magnitude;length=cumulative[points.Length];
        }
        public Vec Point(float progress)
        {
            float f=MathUtil.Wrap01(progress)*points.Length;int i=(int)f;
            return points[i]+(points[(i+1)%points.Length]-points[i])*(f-i);
        }
        public Vec Direction(float progress){return (Point(progress+.002f)-Point(progress-.002f)).Normalized;}
        public Vec Lane(float progress,float offset){return Point(progress)+Direction(progress).Right*offset;}
        public float Heading(float p){Vec d=Direction(p);return (float)Math.Atan2(d.x,d.z);}
        public float Find(Vec p,out float lateral)
        {
            float best=float.MaxValue,progress=0;lateral=0;
            for(int i=0;i<points.Length;i++)
            {
                Vec a=points[i],d=points[(i+1)%points.Length]-a;
                float t=MathUtil.Clamp(Vec.Dot(p-a,d)/Vec.Dot(d,d),0,1);Vec q=a+d*t;
                float dist=Vec.Dot(p-q,p-q);
                if(dist<best){best=dist;progress=(i+t)/points.Length;lateral=Vec.Dot(p-q,d.Normalized.Right);}
            }
            return progress;
        }
        public Vec[] PitRoute {get{return SourcePitRoutes.Points[variant];}}
        public int PitServiceIndex {get{return SourcePitRoutes.ServiceIndex[variant];}}
        public Vec PitBox {get{return PitRoute[PitServiceIndex];}}
        public Vec PitPoint(float index){index=MathUtil.Clamp(index,0,PitRoute.Length-1);int i=(int)index;return PitRoute[i]+(PitRoute[Math.Min(i+1,PitRoute.Length-1)]-PitRoute[i])*(index-i);}
        public float FindPit(Vec p,out float distance){float best=float.MaxValue,index=0;for(int i=0;i<PitRoute.Length-1;i++){Vec a=PitRoute[i],d=PitRoute[i+1]-a;float t=MathUtil.Clamp(Vec.Dot(p-a,d)/Math.Max(.001f,Vec.Dot(d,d)),0,1);float sq=Vec.Dot(p-a-d*t,p-a-d*t);if(sq<best){best=sq;index=i+t;}}distance=(float)Math.Sqrt(best);return index;}
        public bool InPit(float p,float lateral){float distance;FindPit(Lane(p,lateral),out distance);return Math.Abs(lateral)>width*.5f&&distance<5;}
        public bool OnRoad(float p,float lateral){return Math.Abs(lateral)<width*.5f||InPit(p,lateral);}
    }
    [Serializable] public class Vehicle
    {
        public int id;
        public string driver="Driver";
        public bool human;
        public bool hasBodyPose;public float bodyHeight,bodyQx,bodyQy,bodyQz,bodyQw=1;
        public Vec position,velocity;
        public Vec surfaceGravity;public float sgx,sgz;
        public float heading,yawRate,steering,speed,rpm=1000,damage,fuel=45,progress,lateral,totalDistance;
        public int carModel,gear=1,completedLaps,nextGate=1;
        public float lastLap,bestLap,lapElapsed,finishTime=-1,penalty;
        public float[] damageRegions=new float[15];
        public Tyre[] tyres={new Tyre(),new Tyre(),new Tyre(),new Tyre()};
        public bool pitRequested,pitActive,servicing,pitDone,finished,retired;
        public float serviceRemaining,stuckTime;
        public Compound nextCompound=Compound.Medium;
        public DriverInput input;
        public bool abs=true,traction=true,steerAssist=false;public float ersCharge=1;public bool ersAuto=true,ersOvertake,ersActive;public float currentMaxSteer=.5f;Vec lastIntegrated;bool hasIntegrated;
        public bool externalPhysics,externalResetRequested,physicalHandling,surfaceKnown,onDrivingSurface;
        public float previousProgress,offTrackTime,stationaryTime;
        public bool lapInvalid,lapCutPenaltyApplied,pitSpeedPenaltyApplied;public float pitOverspeedTime,recoveryRemaining;
        public const float PitSpeedLimit=16.666667f;
        public const float CornerLateralAccel=10.5f,TopAiSpeed=75f,PitLaneAiSpeed=15.4f;
        public Vec Forward {get{return new Vec((float)Math.Sin(heading),(float)Math.Cos(heading));}}
        public Vec Right {get{return Forward.Right;}}
        public float Wear {get{return (tyres[0].wear+tyres[1].wear+tyres[2].wear+tyres[3].wear)*.25f;}}
        public void Place(Circuit circuit,float p,float offset)
        {
            hasBodyPose=false;externalResetRequested=externalPhysics;position=circuit.Lane(p,offset);heading=circuit.Heading(p);velocity=new Vec();yawRate=0;speed=0;progress=previousProgress=p;nextGate=1;lapElapsed=0;
        }
        public void ReplaceTyres(){foreach(Tyre t in tyres)t.Replace(nextCompound);damage*=.2f;}
        public void Step(Circuit circuit,float dt,float wetness,bool running,float time,int laps,bool serviceAllowed=true,bool releaseAllowed=true)
        {
            recoveryRemaining=Math.Max(0,recoveryRemaining-dt);input=input.Safe();float oldProgress=progress;progress=circuit.Find(position,out lateral);
            if(running&&!finished&&!retired&&pitRequested&&!pitDone&&(position-circuit.PitRoute[0]).Magnitude<10)pitActive=true;
            if(serviceAllowed&&running&&!finished&&!retired&&pitActive&&!pitDone&&!servicing&&(position-circuit.PitBox).Magnitude<1.2f&&speed<1.2f)
            {servicing=true;serviceRemaining=7.5f;position=circuit.PitBox;Vec pitDirection=(circuit.PitPoint(circuit.PitServiceIndex+2)-circuit.PitPoint(circuit.PitServiceIndex-2)).Normalized;heading=(float)Math.Atan2(pitDirection.x,pitDirection.z);velocity=new Vec();yawRate=0;}
            if(!running){velocity=new Vec();yawRate=0;return;}
            if(servicing)
            {
                serviceRemaining=Math.Max(0,serviceRemaining-dt);velocity=new Vec();speed=0;
                if(serviceRemaining<=0&&releaseAllowed){ReplaceTyres();servicing=false;pitDone=true;pitRequested=false;}
                if(running&&!finished&&!retired)lapElapsed+=dt;return;
            }
            float pitDistance;float pitIndex=circuit.FindPit(position,out pitDistance);
            if(pitDone&&pitIndex>circuit.PitRoute.Length-4&&pitDistance<5){pitActive=false;pitDone=false;pitSpeedPenaltyApplied=false;pitOverspeedTime=0;}
            if(!running){velocity=new Vec();yawRate=0;return;}
            if(!finished&&!retired)lapElapsed+=dt;
            if(externalPhysics){ApplyRaceRules(circuit,dt,time,laps,previousProgress);previousProgress=progress;totalDistance+=speed*dt;return;}
            // MySim driving model (replaces the recovered native bridge as the default). RR3-style: speed-sensitive steering
            // that targets the tyre limit, friction-circle tyres, downforce, ABS/TC, grass only slows the car.
            int m=Math.Max(0,Math.Min(RecoveredVehicleProfiles.Count-1,carModel));
            float mass=RecoveredVehicleProfiles.Mass[m],vmax=RecoveredVehicleProfiles.Speed[m]*.44704f,t60=RecoveredVehicleProfiles.Acceleration[m],stop60=RecoveredVehicleProfiles.Brakes[m]*.3048f,df=RecoveredVehicleProfiles.Downforce[m];
            float wheelbase=RecoveredGeometry.Wheelbase[Math.Max(0,Math.Min(RecoveredGeometry.Wheelbase.Length-1,carModel))],frontArm=wheelbase*.48f,rearArm=wheelbase*.52f;
            // walls/containment moved the car after last step: remove velocity into the obstacle and scrub speed (no "pinned at 150 km/h")
            if(hasIntegrated){Vec push=position-lastIntegrated;float pm=push.Magnitude;if(pm>.002f){Vec n=push*(1/pm);float vn=Vec.Dot(velocity,n);if(vn<0)velocity=velocity-n*vn*1.3f;velocity=velocity*(1-Math.Min(.5f,pm*.8f));}}
            float longitudinal=Vec.Dot(velocity,Forward),side=Vec.Dot(velocity,Right);speed=velocity.Magnitude;float v=Math.Max(Math.Abs(longitudinal),.1f);
            float power=mass*26.82f*26.82f/(2*t60)*1.35f,dragK=power/(vmax*vmax*vmax);
            float mu=(tyres[0].Grip(wetness)+tyres[1].Grip(wetness)+tyres[2].Grip(wetness)+tyres[3].Grip(wetness))*.25f*1.18f;
            bool road=circuit.OnRoad(progress,lateral)||(externalPhysics?onDrivingSurface:false);if(!road)mu*=.62f;
            float downforce=(df-.9f)*.55f*speed*speed;float fz=mass*9.81f+downforce;float gripMax=mu*fz;
            // steering: port of RR3 rr3_steering_limit + rr3_steering_update (0x0168ed1c). Angle unit 1/8192 rad.
            // limit blends angleHigh->angleLow between lo/hi speed thresholds, scaled up from standstill; target=input*330/256 clamped to limit;
            // current moves 4.375 units/ms toward target and snaps when it would overshoot.
            float lo=8f,hi=45f,angleHigh=.55f,angleLow=.085f;float frac=MathUtil.Clamp((hi-v)/(hi-lo),0,1);float lockRad=frac*(angleHigh-angleLow)+angleLow;
            float launch=1f;float maxSteer=Math.Max(lockRad*launch,1f/8192f);
            currentMaxSteer=maxSteer;float target=MathUtil.Clamp(input.steer*330f/256f*maxSteer,-maxSteer,maxSteer);
            float step=3.2f*dt;float prev=steering;steering+=MathUtil.Clamp(target-steering,-step,step);if((target-steering)*(target-prev)<0)steering=target;
            // tyres: saturating lateral force per axle, front/rear load split, friction circle with drive/brake
            float frontSlip=steering-(float)Math.Atan2(side+frontArm*yawRate,v),rearSlip=-(float)Math.Atan2(side-rearArm*yawRate,v);
            float fzF=fz*rearArm/wheelbase,fzR=fz*frontArm/wheelbase;const float peak=.13f;
            float brakeDecel=Math.Min(26.82f*26.82f/(2*stop60),mu*fz/mass);float brakeForce=input.brake*brakeDecel*mass*(road?1:.7f);
            float driveForce=input.throttle*Math.Min(power/Math.Max(v,6),mu*fzR*1.15f);
            driveForce+=HybridDeployment.Step(this,input,dt)*input.throttle;
            if(input.handbrake){brakeForce+=mass*3;}
            float rearLong=driveForce+brakeForce*.42f,frontLong=brakeForce*.58f;
            float latF=muCurve(frontSlip/peak)*mu*fzF,latR=muCurve(rearSlip/peak)*mu*fzR;
            latF=CircleClamp(latF,frontLong,mu*fzF);latR=CircleClamp(latR,rearLong,mu*fzR*(input.handbrake?.45f:1));
            if(traction){float room=(float)Math.Sqrt(Math.Max(0,mu*fzR*mu*fzR-latR*latR));driveForce=Math.Min(driveForce,room*1.05f);}
            float resistance=dragK*longitudinal*Math.Abs(longitudinal)+mass*9.81f*(road?.014f:.09f)*Math.Sign(longitudinal);
            float accel=(driveForce-brakeForce*Math.Sign(longitudinal)-resistance)/mass;
            // Body-frame rotation couples both velocity axes; omitting this term creates energy in turns.
            accel+=side*yawRate;
            float newLong;
            if(input.reverse){newLong=longitudinal>1?longitudinal-brakeDecel*dt:MathUtil.Clamp(longitudinal-(input.throttle*6+input.brake*6)*dt,-9,0);}
            else newLong=Math.Max(0,longitudinal+accel*dt);
            float iz=mass*wheelbase*wheelbase*.28f;
            float yawAccel=(latF*frontArm*(float)Math.Cos(steering)-latR*rearArm)/iz;
            float dynYaw=yawRate+yawAccel*dt;dynYaw*=1-dt*(steerAssist?1.6f:1.1f);
            float kinYaw=newLong/wheelbase*(float)Math.Tan(steering);
            {float gripYaw=gripMax/mass/Math.Max(v,3);float kin=MathUtil.Clamp(kinYaw,-gripYaw*1.05f,gripYaw*1.05f);yawRate=MathUtil.Lerp(kin,dynYaw,MathUtil.Clamp((speed-3)/9,0,1));}
            if(steerAssist){float maxYaw=gripMax/mass/Math.Max(v,3)*1.08f;yawRate=MathUtil.Clamp(yawRate,-maxYaw,maxYaw);}
            float newSide=side+((latF*(float)Math.Cos(steering)+latR)/mass-longitudinal*yawRate)*dt;newSide*=1-dt*(speed<5?8:.6f);
            heading=MathUtil.Wrap(heading+yawRate*dt);
            velocity=Forward*newLong+Right*newSide;velocity+=surfaceGravity*dt;
            position+=velocity*dt;speed=velocity.Magnitude;totalDistance+=speed*dt;lastIntegrated=position;hasIntegrated=true;
            int gearCount=RecoveredVehicleProfiles.GearData[m].Length;float ratio=vmax/gearCount;gear=newLong<-.1f?-1:Math.Max(1,Math.Min(gearCount,1+(int)(Math.Abs(newLong)/(ratio*.92f))));
            float inGear=(Math.Abs(newLong)-(gear-1)*ratio*.92f)/(ratio*.92f);rpm=MathUtil.Clamp(gear<0?2500:3600+inGear*4600,900,RecoveredVehicleProfiles.Redline[m]);
            float frontSlipAbs=Math.Abs(frontSlip),rearSlipAbs=Math.Abs(rearSlip);
            float brakeSlip=abs?.015f:input.brake*.4f;
            for(int i=0;i<4;i++)tyres[i].Step(dt,speed,(i<2?frontSlipAbs:rearSlipAbs)+brakeSlip+(traction?0:input.throttle*.15f),1,wetness);
            progress=circuit.Find(position,out lateral);
            ApplyRaceRules(circuit,dt,time,laps,oldProgress);

        }
        // Fastest speed now that still lets the car slow to every upcoming corner's limit (curvature from 16 m chords).
        public static float SpeedAhead(Circuit track,float progress,float lateral,float decel){float best=200,step=16f/track.length;
            for(int k=0;k<22;k++){float p=progress+k*step;float h0=track.Heading(p),h1=track.Heading(p+step),h2=track.Heading(p+2*step);float curv=Math.Abs(MathUtil.Wrap(h2-h0))/32f;float vc=(float)Math.Sqrt(lateral/Math.Max(.0012f,curv));float dist=Math.Max(0,k*16f-6);best=Math.Min(best,(float)Math.Sqrt(vc*vc+2*decel*dist));}
            return best;}
        static float muCurve(float x){return (float)Math.Tanh(x)*(1-.08f*Math.Min(1,Math.Max(0,Math.Abs(x)-1.5f)));}
        static float CircleClamp(float lat,float lon,float max){float t=lat*lat+lon*lon;if(t<=max*max)return lat;float room=Math.Max(0,max*max-lon*lon);return Math.Sign(lat)*(float)Math.Sqrt(room);}
        // Shared by both the Unity physics bridge and the standalone simulation.
        void ApplyRaceRules(Circuit circuit,float dt,float time,int laps,float before)
        {
            if(finished||retired)return;
            float pitDistance;circuit.FindPit(position,out pitDistance);
            bool pitLane=pitActive&&pitDistance<5;
            bool onRoad=(externalPhysics&&surfaceKnown?onDrivingSurface:Math.Abs(lateral)<=circuit.width*.5f)||pitLane;
            if(!onRoad){offTrackTime+=dt;lapInvalid=true;}
            else offTrackTime=0;
            if(pitLane&&speed>PitSpeedLimit+.5f){pitOverspeedTime+=dt;if(pitOverspeedTime>=1&&!pitSpeedPenaltyApplied&&!finished)pitSpeedPenaltyApplied=true;}
            else pitOverspeedTime=0;
            // A checkpoint crossed off the legal driving surface cannot validate a lap.
            if(onRoad)RegisterProgress(before,progress,time,laps);
            stationaryTime=speed<1?stationaryTime+dt:0;
        }
        public void RegisterProgress(float before,float after,float raceTime,int laps)
        {
            if(finished||retired)return;
            if(before>.9f&&after<.1f&&nextGate!=4){lapElapsed=0;lapInvalid=false;lapCutPenaltyApplied=false;nextGate=1;}
            float delta=after-before;if(delta<-.5f)delta+=1;if(delta>.5f)delta-=1;
            if(delta<0||delta>.12f)return;
            if(nextGate<=3&&before<nextGate*.25f&&after>=nextGate*.25f)nextGate++;
            if(before>.9f&&after<.1f&&nextGate==4)
            {
                completedLaps++;lastLap=lapElapsed;if(!lapInvalid&&(bestLap==0||lastLap<bestLap))bestLap=lastLap;lapElapsed=0;nextGate=1;lapInvalid=false;lapCutPenaltyApplied=false;
                if(completedLaps>=laps&&!finished){finished=true;finishTime=raceTime;}
            }
        }
        static float PhysicalSteeringLimit(float speed){float[] speeds={0,15,40,70},factors={1,.6f,.28f,.16f};int i=0;while(i<2&&speed>speeds[i+1])i++;float t=MathUtil.Clamp((speed-speeds[i])/(speeds[i+1]-speeds[i]),0,1);t=t*t*(3-2*t);return (32*MathUtil.Pi/180)*MathUtil.Lerp(factors[i],factors[i+1],t);}
        public DriverInput Autopilot(Circuit track,float pace=1,List<Vehicle> traffic=null)
        {
            float look=MathUtil.Clamp(4+speed*.5f,4,40),offset=0;
            Vehicle obstruction=null;float aheadDistance=float.MaxValue;
            if(traffic!=null&&!pitActive)foreach(var other in traffic)
            {
                if(other.id==id||other.servicing)continue;float along=MathUtil.Wrap01(other.progress-progress)*track.length;if(along>Math.Max(18,speed*1.5f))continue;Vec gap=other.position-position;float ahead=Vec.Dot(gap,Forward);
                if(ahead>0&&ahead<Math.Max(18,speed*1.5f)&&ahead<aheadDistance&&Math.Abs(Vec.Dot(gap,Right))<2.3f){obstruction=other;aheadDistance=ahead;}
            }
            if(obstruction!=null)offset=MathUtil.Clamp(offset+(id%2==0?-2.0f:2.0f),-track.width*.5f+2,track.width*.5f-2);
            float pitDistance,pitIndex=track.FindPit(position,out pitDistance);Vec target=pitActive?track.PitPoint(Math.Min(!pitDone?track.PitServiceIndex:track.PitRoute.Length-1,pitIndex+MathUtil.Clamp(2+speed*.13f,2,4))):track.Lane(progress+look/track.length,offset);Vec diff=target-position;
            float localX=Vec.Dot(diff,Right),d=diff.Magnitude;
            float desiredSteer=(float)Math.Atan2(2*RecoveredGeometry.Wheelbase[Math.Max(0,Math.Min(RecoveredGeometry.Wheelbase.Length-1,carModel))]*localX,Math.Max(1,d*d));
            float steer=desiredSteer/((externalPhysics||physicalHandling)?PhysicalSteeringLimit(speed):Math.Max(.05f,currentMaxSteer));
            float turn=Math.Abs(MathUtil.Wrap(track.Heading(progress+.04f)-track.Heading(progress)))/(.04f*track.length);
            for(int j=0;j<6;j++){float p=progress+j*.012f;turn=Math.Max(turn,Math.Abs(MathUtil.Wrap(track.Heading(p+.012f)-track.Heading(p)))/(.012f*track.length));}
            float desired=MathUtil.Clamp(SpeedAhead(track,progress,CornerLateralAccel*Math.Min(pace,.75f),7f),10,TopAiSpeed);
            desired=Math.Max(4,desired*AiRaceCalibration.Scale(carModel,track.variant));
            // Limit speed for the turn actually requested, including recovery from lateral error.
            float wheelbase=RecoveredGeometry.Wheelbase[Math.Max(0,Math.Min(RecoveredGeometry.Wheelbase.Length-1,carModel))];
            float curvature=Math.Abs((float)Math.Tan(desiredSteer))/Math.Max(1,wheelbase);
            desired=Math.Min(desired,(float)Math.Sqrt(CornerLateralAccel*Math.Min(pace,.75f)/Math.Max(.001f,curvature)));
            if(Vec.Dot(diff,Forward)<0){desired=Math.Min(desired,4);steer=localX>=0?1:-1;}
            if(pitActive){desired=Math.Min(desired,PitLaneAiSpeed);Vec before=track.PitPoint(pitIndex),ahead=track.PitPoint(pitIndex+2),later=track.PitPoint(pitIndex+4);Vec aDir=(ahead-before).Normalized,bDir=(later-ahead).Normalized;float corner=Math.Abs(MathUtil.Wrap((float)Math.Atan2(aDir.x,aDir.z)-(float)Math.Atan2(bDir.x,bDir.z)));desired=Math.Min(desired,MathUtil.Lerp(PitLaneAiSpeed,4,corner));if(!pitDone&&pitIndex>track.PitServiceIndex-12){float remaining=(position-track.PitBox).Magnitude;desired=Math.Min(desired,(float)Math.Sqrt(Math.Max(0,remaining-.25f)*2.5f));if(remaining<1.1f)desired=0;}}
            if(obstruction!=null&&aheadDistance<8)desired=Math.Min(desired,Math.Max(0,obstruction.speed-2));
            if(!pitActive&&!finished&&stationaryTime>3){recoveryRemaining=2.5f;stationaryTime=0;}
            if(recoveryRemaining>0)return new DriverInput(.4f,0,-MathUtil.Clamp(steer,-1,1)){reverse=true};
            if(finished)desired=18;if(retired)return new DriverInput(0,1,0);
            float error=desired-speed;
            return new DriverInput(MathUtil.Clamp(error*.38f,0,1),MathUtil.Clamp(-error*.22f,0,1),MathUtil.Clamp(steer,-1,1));
        }
    }
    public class Race
    {
        public readonly Circuit track;
        public readonly List<Vehicle> cars=new List<Vehicle>();
        public int targetLaps=3;
        public float clock,countdown=5,wetness,pace=.9f;
        public bool started,paused;
        public bool Classified {get{if(cars.Count==0)return false;foreach(var v in cars)if(!v.finished&&!v.retired)return false;return true;}}
        public bool AllFinished {get{if(cars.Count==0)return false;foreach(var v in cars)if(!v.finished)return false;return true;}}
        public bool Running {get{return started&&!paused;}}
        public Race(int circuit,int laps,float wetness){track=new Circuit(circuit);targetLaps=Math.Max(1,laps);this.wetness=wetness;}
        public Vehicle Add(string name,bool human,Compound compound)
        {
            Vehicle v=new Vehicle{id=cars.Count,carModel=cars.Count%CarCatalog.Count,driver=name,human=human};
            foreach(Tyre tyre in v.tyres)tyre.Replace(compound);
            float p=MathUtil.Wrap01(.988f-cars.Count*.008f);v.Place(track,p,(cars.Count%2==0?-2.4f:2.4f));cars.Add(v);return v;
        }
        public void Step(float dt)
        {
            if(paused||!MathUtil.Finite(dt)||dt<=0)return;dt=Math.Min(dt,.1f);
            if(!started){countdown-=dt;if(countdown<=0)started=true;}
            if(started)clock+=dt;
            Vehicle occupied=null;foreach(var candidate in cars)if(candidate.servicing){occupied=candidate;break;}
            foreach(Vehicle v in cars)
            {
                if(!v.human||v.pitActive||v.finished||v.retired)v.input=v.Autopilot(track,pace*MathUtil.Lerp(1,.7f,wetness),cars);
                if(!v.human&&v.Wear>.55f&&!v.pitRequested){v.pitRequested=true;v.nextCompound=wetness>.5f?Compound.Wet:Compound.Hard;}
                bool boxAvailable=occupied==null||occupied==v;
                if(!boxAvailable&&v.pitActive&&!v.pitDone&&(v.position-track.PitBox).Magnitude<8)v.input=new DriverInput(0,1,0);
                bool safeRelease=true;if(v.servicing)foreach(var other in cars){if(other==v)continue;Vec gap=other.position-v.position;float ahead=Vec.Dot(gap,v.Forward);if(ahead>0&&ahead<6&&Math.Abs(Vec.Dot(gap,v.Right))<2.5f){safeRelease=false;break;}}
                v.Step(track,dt,wetness,started,clock,targetLaps,boxAvailable,safeRelease);if(started&&!v.servicing&&!v.finished&&v.speed<1.5f&&Math.Abs(v.lateral)>track.width*.5f){v.stuckTime+=dt;if(v.stuckTime>3f){v.stuckTime=0;Reset(v);}}else v.stuckTime=0;
                if(v.servicing)occupied=v;else if(occupied==v)occupied=null;
            }
            if(started)Collisions();
            float firstFinish=float.MaxValue;foreach(var v in cars)if(v.finished)firstFinish=Math.Min(firstFinish,v.finishTime);
            if(firstFinish<float.MaxValue&&clock-firstFinish>120)foreach(var v in cars)if(!v.finished)v.retired=true;
        }
        public void Collisions()
        {
            for(int i=0;i<cars.Count;i++)for(int j=i+1;j<cars.Count;j++)
            {
                Vehicle a=cars[i],b=cars[j];if(a.externalPhysics||b.externalPhysics)continue;Vec delta=b.position-a.position;float distance=delta.Magnitude;
                if(distance<5.1f)
                {
                    float closest=float.MaxValue;Vec contact=delta;
                    for(int ai=-1;ai<=1;ai++)for(int bi=-1;bi<=1;bi++){Vec gap=(b.position+b.Forward*(bi*1.05f))-(a.position+a.Forward*(ai*1.05f));float d=gap.Magnitude;if(d<closest){closest=d;contact=gap;}}
                    delta=contact;distance=closest;
                }
                if(distance<2.05f)
                {
                    Vec centres=b.position-a.position;Vec normal=distance>.001f&&Vec.Dot(delta,centres)>=0?delta/distance:centres.Normalized;
                    float overlap=2.05f-distance;a.position-=normal*overlap*.5f;b.position+=normal*overlap*.5f;
                    float approach=Vec.Dot(a.velocity-b.velocity,normal);
                    if(approach>0){a.velocity-=normal*approach*.65f;b.velocity+=normal*approach*.65f;float damage=MathUtil.Clamp(approach*.013f,0,.3f);a.damage=MathUtil.Clamp(a.damage+damage,0,1);b.damage=MathUtil.Clamp(b.damage+damage,0,1);}
                }
            }
        }
        public List<Vehicle> Standings()
        {
            List<Vehicle> sorted=new List<Vehicle>(cars);
            sorted.Sort((a,b)=>
            {
                if(a.finished&&b.finished)return a.finishTime.CompareTo(b.finishTime);
                if(a.finished)return -1;if(b.finished)return 1;
                return (b.completedLaps+b.progress).CompareTo(a.completedLaps+a.progress);
            });return sorted;
        }
        public void Reset(Vehicle v)
        {
            float p=v.progress;int gate=v.nextGate;float elapsed=v.lapElapsed;
            if(v.finished||v.retired||v.servicing||v.speed>8)return;
            v.Place(track,p,0);v.nextGate=gate;v.lapElapsed=elapsed;v.lapInvalid=true;
        }
    }
}
