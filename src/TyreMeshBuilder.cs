using Godot;
using System;
namespace MySim {
// Replacement candidate built in hub-centred coordinates. Source mesh/UVs remain untouched.
public static class TyreMeshBuilder {
 public static ArrayMesh Build(float radius,float width,bool wet){
  if(!float.IsFinite(radius)||!float.IsFinite(width)||radius<.15f||radius>.6f||width<.1f||width>.8f)throw new ArgumentOutOfRangeException("Invalid recovered tyre dimensions");
  // Closed section: bead, rounded sidewall, shoulder, contact crown, opposite sidewall and inner liner.
  var section=new[]{new Vector2(-.43f,.64f),new Vector2(-.49f,.70f),new Vector2(-.52f,.82f),new Vector2(-.51f,.91f),new Vector2(-.46f,.975f),new Vector2(-.39f,.995f),new Vector2(-.30f,1),new Vector2(-.20f,1.002f),new Vector2(-.10f,1.003f),new Vector2(0,1.004f),new Vector2(.10f,1.003f),new Vector2(.20f,1.002f),new Vector2(.30f,1),new Vector2(.39f,.995f),new Vector2(.46f,.975f),new Vector2(.51f,.91f),new Vector2(.52f,.82f),new Vector2(.49f,.70f),new Vector2(.43f,.64f),new Vector2(.38f,.70f),new Vector2(-.38f,.70f)};
  // Wet candidates add continuous drainage channels without altering the dry section.
  // Channel positions are normalized tyre width; depths below are metres.
  if(wet){
   var crown=new System.Collections.Generic.SortedSet<float>();for(int j=5;j<=13;j++)crown.Add(section[j].X);
   foreach(float channel in new[]{-.27f,-.09f,.09f,.27f})foreach(float edge in new[]{-.023f,-.012f,0f,.012f,.023f})crown.Add(channel+edge);
   var detailed=new System.Collections.Generic.List<Vector2>();
   for(int j=0;j<5;j++)detailed.Add(section[j]);
    foreach(float x in crown){
     float y=1;for(int j=4;j<14;j++)if(x>=section[j].X&&x<=section[j+1].X){y=Mathf.Lerp(section[j].Y,section[j+1].Y,(x-section[j].X)/(section[j+1].X-section[j].X));break;}
     float depth=0;foreach(float channel in new[]{-.27f,-.09f,.09f,.27f})depth=Mathf.Max(depth,Mathf.Clamp((.023f-Mathf.Abs(x-channel))/.011f,0,1)*.006f);
     detailed.Add(new Vector2(x,y-depth/radius));
    }
   for(int j=14;j<section.Length;j++)detailed.Add(section[j]);section=detailed.ToArray();
  }
  const int segments=384;using var tool=new SurfaceTool();tool.Begin(Mesh.PrimitiveType.Triangles);
  Vector3 Point(int angle,int cross){int j=(cross+section.Length)%section.Length;float theta=Mathf.Tau*angle/segments;var p=section[j];float r=p.Y*radius;
   // Paired angled channels connect the four longitudinal channels to the shoulders.
   if(wet&&Mathf.Abs(p.X)<.46f){float phase=Mathf.PosMod(angle-Mathf.Abs(p.X)*48f,16f);float edge=Mathf.Min(phase,16f-phase);r-=Mathf.Clamp((1.4f-edge)/.8f,0,1)*.004f;}
   return new Vector3(p.X*width,r*Mathf.Cos(theta),r*Mathf.Sin(theta));
  }
  void Vertex(int angle,int cross){int j=(cross+section.Length)%section.Length;var around=Point(angle+1,j)-Point(angle-1,j);var across=Point(angle,j+1)-Point(angle,j-1);var normal=around.Cross(across).Normalized();tool.SetNormal(normal);tool.SetUV(new Vector2(angle/(float)segments,j/(float)section.Length));tool.AddVertex(Point(angle,j));}
  for(int i=0;i<segments;i++)for(int j=0;j<section.Length;j++){Vertex(i,j);Vertex(i,j+1);Vertex(i+1,j);Vertex(i+1,j);Vertex(i,j+1);Vertex(i+1,j+1);}tool.Index();return tool.Commit();
 }
 public static MeshInstance3D Create(float radius,float width,bool wet){var part=new MeshInstance3D {Name="RebuiltRubberTyre",Mesh=Build(radius,width,wet),MaterialOverride=new StandardMaterial3D {AlbedoColor=new Color(.025f,.027f,.029f),Roughness=.88f,Metallic=0,MetallicSpecular=.22f,CullMode=BaseMaterial3D.CullModeEnum.Disabled}};part.SetMeta("radius",radius);part.SetMeta("width",width);part.SetMeta("wet",wet);return part;}
 public static void SetWet(MeshInstance3D part,bool wet){if(part.GetMeta("wet").AsBool()==wet)return;part.Mesh=Build(part.GetMeta("radius").AsSingle(),part.GetMeta("width").AsSingle(),wet);part.SetMeta("wet",wet);}
}
}
