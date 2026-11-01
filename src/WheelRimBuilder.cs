using Godot;
using System;
namespace MySim {
// Replacement asset candidate: hollow bead barrel, swept forged spokes and machined hub.
// Axis X; outer face -X, matching recovered tyre templates before per-side mirroring.
public static class WheelRimBuilder {
 static StandardMaterial3D Metal(Color colour,float roughness)=>new StandardMaterial3D {AlbedoColor=colour,Metallic=.9f,Roughness=roughness,MetallicSpecular=.5f,CullMode=BaseMaterial3D.CullModeEnum.Disabled};
 static void Vertex(SurfaceTool tool,Vector3 p,Vector3 n,Vector2 uv){tool.SetNormal(n);tool.SetUV(uv);tool.AddVertex(p);}
 static ArrayMesh Lathe(Vector2[] profile,int segments){
  var tool=new SurfaceTool();tool.Begin(Mesh.PrimitiveType.Triangles);
  void V(int circle,int point){int j=(point+profile.Length)%profile.Length;float angle=Mathf.Tau*circle/segments;var p=profile[j];var tangent=profile[(j+1)%profile.Length]-profile[(j-1+profile.Length)%profile.Length];var n=new Vector3(-tangent.Y,tangent.X*Mathf.Cos(angle),tangent.X*Mathf.Sin(angle)).Normalized();Vertex(tool,new Vector3(p.X,p.Y*Mathf.Cos(angle),p.Y*Mathf.Sin(angle)),n,new Vector2(circle/(float)segments,j/(float)profile.Length));}
  for(int i=0;i<segments;i++)for(int j=0;j<profile.Length;j++){V(i,j);V(i,j+1);V(i+1,j);V(i+1,j);V(i,j+1);V(i+1,j+1);}
  return tool.Commit();
 }
 static ArrayMesh Spoke(float radius,float width,float angle,float sweep=.16f,float spokeWidth=.062f){
  const int steps=20,sides=10;var tool=new SurfaceTool();tool.Begin(Mesh.PrimitiveType.Triangles);
  void V(int along,int around){float t=along/(float)steps,theta=angle+sweep*t*t,phi=Mathf.Tau*around/sides;
   float distance=Mathf.Lerp(radius*.19f,radius*.96f,t),halfWidth=Mathf.Lerp(radius*spokeWidth,radius*spokeWidth*.55f,t),halfDepth=radius*.027f;
   var tangent=new Vector3(0,-Mathf.Sin(theta),Mathf.Cos(theta));var centre=new Vector3(-width*.42f+(1-t)*radius*.10f,distance*Mathf.Cos(theta),distance*Mathf.Sin(theta));
   var position=centre+Vector3.Right*(halfDepth*Mathf.Cos(phi))+tangent*(halfWidth*Mathf.Sin(phi));var normal=(Vector3.Right*(Mathf.Cos(phi)/halfDepth)+tangent*(Mathf.Sin(phi)/halfWidth)).Normalized();Vertex(tool,position,normal,new Vector2(t,around/(float)sides));
  }
  for(int i=0;i<steps;i++)for(int j=0;j<sides;j++){V(i,j);V(i,j+1);V(i+1,j);V(i+1,j);V(i,j+1);V(i+1,j+1);}
  return tool.Commit();
 }
 public static void BrakeCandidate(Node3D spin,Node3D fixedBrake,float rimRadius,float tyreWidth){
  if(System.Environment.GetEnvironmentVariable("MYSIM_REBUILT_BRAKES")!="1")return;
  // Authored candidate packaging within the recovered rim envelope; not manufacturer measurements.
  float outer=rimRadius*.85f,inner=rimRadius*.53f,thickness=Mathf.Min(.028f,tyreWidth*.1f);var ceramic=Metal(new Color(.23f,.24f,.25f),.72f);ceramic.Metallic=.22f;var iron=Metal(new Color(.12f,.13f,.14f),.55f);
  foreach(var parent in new[]{spin,fixedBrake})foreach(var child in parent.GetChildren())if(child is MeshInstance3D source){string name=source.Name.ToString().ToUpperInvariant();if(name.Contains("CALIPER")||name.Contains("DISC")||name.Contains("ROTOR")){source.Visible=false;source.SetMeta("wheel_hidden",true);}}
  var rotor=new Node3D{Name="RebuiltVentilatedBrake"};spin.AddChild(rotor);
  foreach(float side in new[]{-1f,1f}){float x=side*thickness*.5f;var profile=new[]{new Vector2(x-.002f,inner),new Vector2(x-.002f,outer),new Vector2(x+.002f,outer),new Vector2(x+.002f,inner)};rotor.AddChild(new MeshInstance3D{Name="FrictionPlate",Mesh=Lathe(profile,128),MaterialOverride=ceramic});}
  for(int i=0;i<32;i++){float angle=Mathf.Tau*i/32;var radial=new Vector3(0,Mathf.Cos(angle),Mathf.Sin(angle));rotor.AddChild(new MeshInstance3D{Name="CoolingVane",Mesh=new BoxMesh{Size=new Vector3(thickness,outer-inner,.0025f)},Position=radial*((inner+outer)*.5f),Rotation=new Vector3(angle,0,0),MaterialOverride=iron});}
  var bell=new[]{new Vector2(-thickness*.5f,inner*.35f),new Vector2(-thickness*.5f,inner),new Vector2(thickness*.5f,inner),new Vector2(thickness*.5f,inner*.35f)};rotor.AddChild(new MeshInstance3D{Name="MountingBell",Mesh=Lathe(bell,96),MaterialOverride=iron});
  var caliper=new Node3D{Name="RebuiltFixedCaliper"};fixedBrake.AddChild(caliper);var finish=Metal(new Color(.24f,.25f,.27f),.42f);
  for(int i=-2;i<=2;i++){float angle=i*.11f;var radial=new Vector3(0,Mathf.Cos(angle),Mathf.Sin(angle));caliper.AddChild(new MeshInstance3D{Name="CaliperHousing",Mesh=new BoxMesh{Size=new Vector3(thickness*1.65f,outer*.19f,outer*.14f)},Position=radial*(outer*.94f),Rotation=new Vector3(angle,0,0),MaterialOverride=finish});}
 }
 public static Node3D Build(float radius,float tyreWidth,int design,int car=-1){
  if(!float.IsFinite(radius)||radius<.1f||radius>.45f||tyreWidth<.1f||tyreWidth>.7f)throw new ArgumentOutOfRangeException("Recovered wheel dimensions invalid");
  var root=new Node3D {Name="RebuiltForgedRim"};float width=tyreWidth*.8f;var metal=Metal(new Color(.24f,.27f,.30f),.24f);var edge=Metal(new Color(.36f,.38f,.40f),.32f);var hubMetal=Metal(new Color(.12f,.14f,.16f),.35f);
  float sweep=.16f,spokeWidth=.062f;
  if(car>=15&&car<=18){
   using var document=System.Text.Json.JsonDocument.Parse(FileAccess.GetFileAsString("res://data/f1_wheel_profiles.json"));var spec=document.RootElement.GetProperty("cars").GetProperty(car.ToString());var colour=spec.GetProperty("metal");sweep=spec.GetProperty("sweep").GetSingle();spokeWidth=spec.GetProperty("spokeWidth").GetSingle();float roughness=spec.GetProperty("roughness").GetSingle();
   if(spec.GetProperty("spokes").GetInt32()!=10||!float.IsFinite(sweep)||Mathf.Abs(sweep)>.4f||!float.IsFinite(spokeWidth)||spokeWidth<.03f||spokeWidth>.09f||!float.IsFinite(roughness)||roughness<.1f||roughness>.8f)throw new Exception("Invalid custom F1 wheel profile");
   metal=Metal(new Color(colour[0].GetSingle(),colour[1].GetSingle(),colour[2].GetSingle()),roughness);design=2;root.SetMeta("custom_f1_car",car);root.SetMeta("custom_f1_profile",spec.GetProperty("name").GetString());
  }
  void Add(string name,Mesh mesh,Material material){root.AddChild(new MeshInstance3D {Name=name,Mesh=mesh,MaterialOverride=material});}
  var profile=new[]{new Vector2(-width*.48f,radius*.93f),new Vector2(-width*.48f,radius*.99f),new Vector2(-width*.46f,radius*1.008f),new Vector2(-width*.43f,radius),new Vector2(-width*.40f,radius*.96f),new Vector2(width*.40f,radius*.96f),new Vector2(width*.43f,radius),new Vector2(width*.46f,radius*1.008f),new Vector2(width*.48f,radius*.99f),new Vector2(width*.48f,radius*.93f)};
  Add("HollowBeadBarrel",Lathe(profile,192),edge);
  int pairs=design==0?5:design==1?7:10;
  for(int i=0;i<pairs;i++)foreach(float offset in design==2?new[]{0f}:new[]{-.045f,.045f})Add("ForgedSpoke"+i+"_"+(offset<0?"A":"B"),Spoke(radius,width,Mathf.Tau*i/pairs+offset,sweep,spokeWidth),metal);
  var hubProfile=new[]{new Vector2(-width*.46f,radius*.075f),new Vector2(-width*.48f,radius*.1f),new Vector2(-width*.48f,radius*.18f),new Vector2(-width*.44f,radius*.23f),new Vector2(-width*.35f,radius*.23f),new Vector2(-width*.35f,radius*.075f)};
  Add("MachinedHub",Lathe(hubProfile,128),hubMetal);
  int nuts=design==2?1:5;for(int i=0;i<nuts;i++){float angle=Mathf.Tau*i/5;root.AddChild(new MeshInstance3D {Name="LugNut"+i,Mesh=new CylinderMesh {TopRadius=radius*(design==2?.085f:.026f),BottomRadius=radius*(design==2?.085f:.026f),Height=radius*.055f,RadialSegments=6},MaterialOverride=edge,Position=new Vector3(-width*.49f,(design==2?0:radius*.145f*Mathf.Cos(angle)),(design==2?0:radius*.145f*Mathf.Sin(angle))),Rotation=new Vector3(0,0,Mathf.Pi*.5f)});}
  root.SetMeta("outer_face_axis",-1);root.SetMeta("rim_radius",radius);root.SetMeta("tyre_width",tyreWidth);return root;
 }
}
}
