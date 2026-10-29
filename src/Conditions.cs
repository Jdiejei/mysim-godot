using Godot;
namespace MySim {
// Time-of-day and weather presets (menu settings "tod": 0 noon, 1 morning, 2 sunset, 3 night; "wet": 0 clear, 1 overcast, 2 rain).
public static class Conditions {
 public static readonly string[] TodNames = { "Noon", "Morning", "Sunset", "Night" }, WetNames = { "Clear", "Overcast", "Rain" };
 public static readonly float[] Wetness = { 0f, 0.15f, 0.75f };
 public static int Tod => Mathf.Clamp(System.Environment.GetEnvironmentVariable("MYSIM_TOD") != null ? int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_TOD")) : Settings.Get("tod", 0), 0, 3);
 public static int Wet => Mathf.Clamp(System.Environment.GetEnvironmentVariable("MYSIM_WET") != null ? int.Parse(System.Environment.GetEnvironmentVariable("MYSIM_WET")) : Settings.Get("wet", 0), 0, 2);
 public static float WetnessValue => Wetness[Wet];
 public static void Apply(Godot.Environment env, DirectionalLight3D sun, ProceduralSkyMaterial sky) {
  int tod = Tod, wet = Wet;
  Color amb = new Color(0.62f, 0.66f, 0.72f), fog = new Color(0.63f, 0.70f, 0.76f), top = new Color(0.38f, 0.55f, 0.82f), hor = new Color(0.72f, 0.80f, 0.88f), sunCol = new Color(1f, 0.97f, 0.92f); float sunE = 1.7f, ambE = 1.0f, expo = 1.0f; Vector3 rot = new Vector3(-48, -35, 0);
  if (tod == 1) { rot = new Vector3(-20, -60, 0); sunCol = new Color(1f, 0.86f, 0.68f); sunE = 1.5f; amb = new Color(0.56f, 0.6f, 0.7f); fog = new Color(0.82f, 0.78f, 0.74f); top = new Color(0.42f, 0.56f, 0.8f); hor = new Color(0.95f, 0.82f, 0.68f); }
  else if (tod == 2) { rot = new Vector3(-9, -120, 0); sunCol = new Color(1f, 0.58f, 0.32f); sunE = 1.3f; amb = new Color(0.48f, 0.42f, 0.5f); fog = new Color(0.86f, 0.62f, 0.45f); top = new Color(0.28f, 0.34f, 0.58f); hor = new Color(1f, 0.6f, 0.34f); }
  else if (tod == 3) { rot = new Vector3(-38, -35, 0); sunCol = new Color(0.6f, 0.7f, 1f); sunE = 0.7f; amb = new Color(0.3f, 0.35f, 0.55f); ambE = 1.5f; expo = 1.5f; fog = new Color(0.05f, 0.07f, 0.12f); top = new Color(0.01f, 0.02f, 0.06f); hor = new Color(0.05f, 0.07f, 0.13f); }
  float fogD = 0.00012f;
  if (wet == 1) { sunE *= 0.5f; amb = amb.Lerp(new Color(0.6f, 0.62f, 0.66f), 0.5f); fog = fog.Lerp(new Color(0.62f, 0.64f, 0.67f), 0.6f); top = top.Lerp(new Color(0.55f, 0.58f, 0.63f), 0.7f); hor = hor.Lerp(new Color(0.66f, 0.68f, 0.7f), 0.7f); fogD = 0.0007f; }
  else if (wet == 2) { sunE *= 0.25f; amb = amb.Lerp(new Color(0.42f, 0.44f, 0.48f), 0.6f); fog = fog.Lerp(new Color(0.42f, 0.44f, 0.47f), 0.7f); top = top.Lerp(new Color(0.34f, 0.36f, 0.4f), 0.8f); hor = hor.Lerp(new Color(0.46f, 0.48f, 0.5f), 0.8f); fogD = 0.0018f; }
  env.AmbientLightColor = amb; env.AmbientLightEnergy = ambE; env.FogLightColor = fog; env.FogDensity = fogD; env.TonemapExposure = expo;
  sun.RotationDegrees = rot; sun.LightColor = sunCol; sun.LightEnergy = sunE;
  sky.SkyTopColor = top; sky.SkyHorizonColor = hor; sky.GroundHorizonColor = hor; sky.GroundBottomColor = hor.Darkened(0.3f);
 }
 public static void AddRain(Node3D parent) {
  var mat = new ParticleProcessMaterial { EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box, EmissionBoxExtents = new Vector3(28, 0.2f, 28), Direction = new Vector3(0.08f, -1, 0), Spread = 2f, InitialVelocityMin = 24f, InitialVelocityMax = 30f, Gravity = new Vector3(0, -6, 0) };
  var mesh = new BoxMesh { Size = new Vector3(0.012f, 0.55f, 0.012f), Material = new StandardMaterial3D { ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, AlbedoColor = new Color(0.8f, 0.85f, 0.95f, 0.45f), Transparency = BaseMaterial3D.TransparencyEnum.Alpha } };
  parent.AddChild(new GpuParticles3D { Amount = 4000, Lifetime = 0.9, ProcessMaterial = mat, DrawPass1 = mesh, Position = new Vector3(0, 14, 0), VisibilityAabb = new Aabb(new Vector3(-40, -20, -40), new Vector3(80, 40, 80)), LocalCoords = false });
 }
}}
