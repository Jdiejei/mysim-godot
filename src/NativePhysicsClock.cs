using Godot;
using System;
namespace MySim {
public static class NativePhysicsClock {
 public static void Configure(){int hz=int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_PHYSICS_HZ")??"120");if(hz!=120&&hz!=240)throw new ArgumentOutOfRangeException("MYSIM_NATIVE_PHYSICS_HZ");Engine.PhysicsTicksPerSecond=hz;string recordingFps=System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_RECORD_FPS");if(recordingFps!=null){int fps=int.Parse(recordingFps);if(fps!=15)throw new ArgumentOutOfRangeException("MYSIM_NATIVE_RECORD_FPS");Engine.MaxPhysicsStepsPerFrame=Math.Max(Engine.MaxPhysicsStepsPerFrame,(hz+fps-1)/fps);GD.Print("NATIVE_RECORD_FPS=",fps," MAX_PHYSICS_STEPS=",Engine.MaxPhysicsStepsPerFrame);}GD.Print("NATIVE_PHYSICS_HZ=",hz);}
}
}

