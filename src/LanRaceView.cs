using Godot;
using System;
using MySimulator.Rebuild;
namespace MySim {
// LAN phase-2 race-view adapters (the granted Main.cs scope): the GODOT half of the
// race-session bridge. LanCarView owns ONE persistent car visual - created on first sight,
// posed in place from every snapshot, never recreated per packet (the central-risk rule);
// LanRaceView creates them under the world and carries the race-state readout;
// LanClientRunner ticks the session, drives the applier and follows the local car with a
// simple chase camera. The single-player path never constructs any of this: the runner
// exists only while a LAN session handed over from the lobby is live. MVP limits (stated
// in the LAN plan): pure snapshot rendering, no local physics, no prediction.
public partial class LanCarView : Node3D, ICarView {
    Node3D visual;
    public string Driver;
    public float Damage, Fuel, ServiceRemaining;
    public int CompletedLaps;
    public bool Finished, PitActive, Servicing;
    public void EnsureCreated(int carModel, string driver) {
        Driver = driver;
        if (visual != null || carModel < 0 || carModel >= CarCatalog.Count) return;
        visual = CarLoader.Load(CarCatalog.Folders[carModel]);
        AddChild(visual);
        CarLoader.ApplyExact(visual, carModel);
    }
    public void ApplyPose(float x, float z, float heading, bool hasBodyPose, float bodyHeight, float bodyQx, float bodyQy, float bodyQz, float bodyQw) {
        // Logic and world coordinates are 1:1 for X/Z (Contact.ToWorld is the same mapping
        // the single-player visual path uses); Y comes from the body pose when the host
        // simulates natively, else the ground plane.
        Position = new Vector3(x, hasBodyPose ? bodyHeight : 0f, z);
        if (hasBodyPose) {
            var q = new Quaternion(bodyQx, bodyQy, bodyQz, bodyQw);
            if (q.LengthSquared() > 0.8f && q.LengthSquared() < 1.2f) Transform = new Transform3D(new Basis(q), Position);
            else Rotation = new Vector3(0, heading, 0);
        } else Rotation = new Vector3(0, heading, 0);
    }
    public void ApplyState(float damage, float fuel, int completedLaps, bool finished, float finishTime, bool pitActive, bool servicing, float serviceRemaining) {
        Damage = damage; Fuel = fuel; CompletedLaps = completedLaps; Finished = finished;
        PitActive = pitActive; Servicing = servicing; ServiceRemaining = serviceRemaining;
    }
}
public sealed class LanRaceView : IRaceView {
    readonly Node3D world;
    public float Clock, Countdown, Wetness;
    public bool Started;
    public int TrackVariant, TargetLaps;
    public LanRaceView(Node3D world) { this.world = world; }
    public ICarView CreateCarView(int id) {
        var view = new LanCarView { Name = "LanCar" + id };
        world?.AddChild(view);
        return view;
    }
    public void ApplyRaceState(float clock, float countdown, bool started, int trackVariant, int targetLaps, float wetness) {
        Clock = clock; Countdown = countdown; Started = started; TrackVariant = trackVariant; TargetLaps = targetLaps; Wetness = wetness;
    }
}
public partial class LanClientRunner : Node {
    public GameSession Session;
    public Node3D World;
    public Camera3D Cam;
    public Action OnExit;
    public bool EscWas;
    SnapshotApplier applier;
    LanRaceView raceView;
    Label status;
    int lastAppliedSequence = -1;
    public override void _Ready() {
        raceView = new LanRaceView(World);
        applier = new SnapshotApplier(raceView);
        status = new Label { Position = new Vector2(48, 24), Text = "LAN" };
        status.AddThemeFontSizeOverride("font_size", 24);
        var layer = new CanvasLayer { Layer = 25 };
        layer.AddChild(status);
        AddChild(layer);
    }
    public override void _Process(double delta) {
        if (Session == null) return;
        try { Session.Tick((float)delta); }
        catch (Exception e) { status.Text = "LAN session error: " + e.Message; return; }
        // GameSession swaps in a new Race per STATE packet; the applier keeps the car views
        // persistent and updates them in place. Apply only on a new sequence number.
        if (Session.race != null && Session.lastSequence != lastAppliedSequence) {
            lastAppliedSequence = Session.lastSequence;
            applier.Apply(Session.race);
            if (Cam != null && applier.ViewFor(Session.localId) is LanCarView mine && mine.Position.LengthSquared() > 0.01f) {
                var behind = -mine.GlobalTransform.Basis.Z;
                Cam.GlobalPosition = mine.GlobalPosition + new Vector3(0, 5f, 0) + behind * 9f;
                Cam.LookAt(mine.GlobalPosition + Vector3.Up);
            }
        }
        status.Text = (raceView.Started ? "RACE" : "GRID") + "  laps " + raceView.TargetLaps + "  t " + raceView.Clock.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + (raceView.Started ? "" : "  countdown " + raceView.Countdown.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
        bool escNow = Input.IsKeyPressed(Key.Escape);
        if (escNow && !EscWas) { Session.Dispose(); Session = null; OnExit?.Invoke(); }
        EscWas = escNow;
    }
    public override void _ExitTree() { Session?.Dispose(); Session = null; }
}
}
