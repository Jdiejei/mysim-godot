using Godot;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MySimulator.Rebuild;
namespace MySim {
// Headless crash campaign: for N points around a track, drive in with the AI, then crash at several speeds and angles; log physics/contact anomalies.
public static class CrashBatch {
 static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);
 public static void Run(PhysicsDirectSpaceState3D space, int trackIdx, int points, int shard, int shards, string outPath) {
  string carSpec = System.Environment.GetEnvironmentVariable("MYSIM_CARS") ?? "8"; int c0 = int.Parse(carSpec.Split('-')[0]), c1 = carSpec.Contains("-") ? int.Parse(carSpec.Split('-')[1]) : c0;
  var sp0 = System.Environment.GetEnvironmentVariable("MYSIM_SPACING"); if (sp0 != null) points = Mathf.Max(8, (int)(new Circuit(trackIdx).length / float.Parse(sp0, CultureInfo.InvariantCulture))); var sbAll = new StringBuilder(); int doneAll = 0, flaggedAll = 0;
  for (int carIdx = c0; carIdx <= c1; carIdx++) {
  var sb = new StringBuilder(); var speeds = new[] { 100f, 180f, 260f }; var angles = new[] { 10f, 25f, 45f, 90f, -10f, -25f, -45f, -90f };
  var box = new Vector3(VehicleDimensions.Width[carIdx], Mathf.Clamp(VehicleDimensions.Height[carIdx], .6f, 1.5f), VehicleDimensions.Length[carIdx]); const float dt = 1f / 120f; int done = 0, flagged = 0;
  var plist = new List<float>(); { var ck = new Circuit(trackIdx); float L = ck.length, pos = 0; while (pos < L) { plist.Add(pos / L); float h0 = ck.Heading(pos / L), h1 = ck.Heading(Mathf.Min(pos + 25f, L - 0.01f) / L); float bend = Mathf.Abs(MathUtil.Wrap(h1 - h0)) * 57.2958f; float spacing = sp0 != null ? float.Parse(sp0, CultureInfo.InvariantCulture) : L / Mathf.Max(8, points); if (!float.IsFinite(spacing) || spacing <= 0) throw new System.ArgumentException("Crash spacing must be positive and finite"); pos += bend > 4f ? spacing * .4f : spacing; } }
  for (int pi = 0; pi < plist.Count; pi++) {
   if (pi % shards != shard) continue; float p = plist[pi];
   foreach (var sp in speeds) foreach (var ang in angles) {
    var race = new Race(trackIdx, 5, 0f); var v = race.Add("CRASH", false, Compound.Medium); v.carModel = carIdx; race.countdown = 0; race.started = true;
    float runIn = Mathf.Clamp(sp / 3.6f * 4f / race.track.length, 0.004f, 0.08f); v.Place(race.track, ((p - runIn) % 1f + 1f) % 1f, 0f);
    var w0 = Contact.ToWorld(v.position, 0); var g0 = Contact.Ground(space, new Vector3(w0.X, 0, w0.Z), 0f, 1e9f);
    // find the starting height with a wide ray
    var rq = PhysicsRayQueryParameters3D.Create(new Vector3(w0.X, 400, w0.Z), new Vector3(w0.X, -400, w0.Z)); rq.CollisionMask = 1; rq.HitBackFaces = true; var h0 = space.IntersectRay(rq);
    if (h0.Count == 0) { sb.Append("{\"track\":").Append(trackIdx).Append(",\"car\":").Append(carIdx).Append(",\"p\":").Append(F(p)).Append(",\"speed\":").Append(F(sp)).Append(",\"angle\":").Append(F(ang)).Append(",\"flags\":[\"NO_START_GROUND\"]}\n"); flagged++; done++; continue; }
    float y = ((Vector3)h0["position"]).Y; float lastY = y; float prevRef = y;
    int jumps = 0, voids = 0, wallHits = 0, steps = 0, contained = 0, hitGain = 0; float maxY = y, minY = y, maxLat = 0, maxDy = 0, hitX = 0, hitY = 0, hitZ = 0; bool nonFinite = false; float crashStartT = 5f; float tSim = 0; bool crashing = false; int crashSteps = 0; float maxSpeedAfter = 0;
    int total = (int)((5f + 4f) / dt);
    for (int i = 0; i < total; i++) {
     tSim = i * dt; if (!crashing && tSim >= crashStartT) { crashing = true; float h = v.heading + ang * Mathf.Pi / 180f; v.heading = h; float spd = Mathf.Max(v.speed, sp / 3.6f); v.velocity = new Vec(Mathf.Sin(h) * spd, Mathf.Cos(h) * spd); v.speed = spd; v.yawRate = 0; v.dyn = null; v.hasIntegrated = false; }
     if (crashing) { v.human = true; v.input = new DriverInput(1f, 0f, 0f); crashSteps++; } else v.human = false;
     var start = v.position; race.Step(dt);
     if (!float.IsFinite(v.position.x) || !float.IsFinite(v.position.z) || !float.IsFinite(v.speed)) { nonFinite = true; break; }
     float spBefore = v.speed; if (Contact.Walls(space, v, start, lastY, box)) { wallHits++; if (v.speed > spBefore + 0.05f) hitGain++; }
     var w = Contact.ToWorld(v.position, 0); var g = Contact.Ground(space, w, lastY, 0.6f); if (Contact.Contain(v, start, g.near, 30f)) { contained++; w = Contact.ToWorld(v.position, 0); g = Contact.Ground(space, w, lastY, 0.6f); }
     Contact.MarkGood(v, g.near && Mathf.Abs(v.lateral) <= 30f); if (!g.found) voids++; else { if (g.clamped) { jumps++; if (jumps == 1) { hitX = w.X; hitY = g.rawY; hitZ = w.Z; } } maxDy = Mathf.Max(maxDy, Mathf.Abs(g.rawY - lastY)); lastY = g.y; }
     maxY = Mathf.Max(maxY, lastY); minY = Mathf.Min(minY, lastY); maxLat = Mathf.Max(maxLat, Mathf.Abs(v.lateral)); if (crashing) maxSpeedAfter = Mathf.Max(maxSpeedAfter, v.speed); steps++;
    }
    var flags = new List<string>();
    if (nonFinite) flags.Add("NONFINITE"); if (voids > 0) flags.Add("VOID_GROUND"); if (jumps > 0) flags.Add("GROUND_JUMP"); if (maxLat > 25f) flags.Add("ESCAPED_TRACK"); if (maxY - minY > 25f) flags.Add("HEIGHT_RANGE");
    if (hitGain > 0) flags.Add("SPEED_GAIN_AT_WALL"); if (wallHits == 0 && Mathf.Abs(ang) >= 45f) flags.Add("NO_WALL_CONTACT_AT_HIGH_ANGLE");
    if (v.speed < 0.5f && Mathf.Abs(v.lateral) > 6f) flags.Add("STUCK_AT_WALL");
    if (flags.Count > 0) flagged++; done++; if (flags.Count == 0 && System.Environment.GetEnvironmentVariable("MYSIM_ONLYFLAGGED") != null) continue;
    sb.Append("{\"track\":").Append(trackIdx).Append(",\"car\":").Append(carIdx).Append(",\"p\":").Append(F(p)).Append(",\"speed\":").Append(F(sp)).Append(",\"angle\":").Append(F(ang)).Append(",\"wallHits\":").Append(wallHits).Append(",\"jumps\":").Append(jumps).Append(",\"voids\":").Append(voids).Append(",\"contained\":").Append(contained)
      .Append(",\"maxLat\":").Append(F(maxLat)).Append(",\"maxDy\":").Append(F(maxDy)).Append(",\"y\":[").Append(F(minY)).Append(",").Append(F(maxY)).Append("],\"endSpeed\":").Append(F(v.speed)).Append(",\"endLat\":").Append(F(v.lateral))
      .Append(",\"jumpAt\":[").Append(F(hitX)).Append(",").Append(F(hitY)).Append(",").Append(F(hitZ)).Append("],\"flags\":[");
    for (int k = 0; k < flags.Count; k++) { if (k > 0) sb.Append(","); sb.Append("\"").Append(flags[k]).Append("\""); }
    sb.Append("]}\n");
   }
  }
  sbAll.Append(sb.ToString()); sb.Clear(); sbAll.Append("{\"summary\":true,\"track\":").Append(trackIdx).Append(",\"car\":").Append(carIdx).Append(",\"crashes\":").Append(done).Append(",\"flagged\":").Append(flagged).Append(",\"flags\":[]}\n"); doneAll += done; flaggedAll += flagged; done = 0; flagged = 0; }
  using (var f = FileAccess.Open(outPath, FileAccess.ModeFlags.Write)) f.StoreString(sbAll.ToString());
  GD.Print("CRASHBATCH track=", trackIdx, " shard=", shard, "/", shards, " crashes=", doneAll, " flagged=", flaggedAll, " out=", outPath);
 }
}}




