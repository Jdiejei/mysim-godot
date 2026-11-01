using Godot;
using MySimulator.Rebuild;
namespace MySim {
// Compound markings are separate geometry; source tyres, rims, textures and UVs stay intact.
public static class TyreAppearance {
 public static Color Colour(Compound compound)=>compound switch {Compound.Soft=>new Color(.9f,.06f,.04f),Compound.Medium=>new Color(1f,.78f,.04f),Compound.Hard=>new Color(.92f,.93f,.94f),_=>new Color(.08f,.38f,.95f)};
 static ArrayMesh FlatMarking(float radius,float side){
  using var tool=new SurfaceTool();tool.Begin(Mesh.PrimitiveType.Triangles);const int segments=192;
  void V(int angle,float radial){float theta=Mathf.Tau*angle/segments;tool.SetNormal(Vector3.Right*side);tool.AddVertex(new Vector3(0,radius*radial*Mathf.Cos(theta),radius*radial*Mathf.Sin(theta)));}
  for(int i=0;i<segments;i++){if(side>0){V(i,.810f);V(i+1,.810f);V(i,.822f);V(i,.822f);V(i+1,.810f);V(i+1,.822f);}else{V(i,.810f);V(i,.822f);V(i+1,.810f);V(i,.822f);V(i+1,.822f);V(i+1,.810f);}}tool.Index();return tool.Commit();
 }
 static void Lettering(Node3D markings,Vector3 centre,float radius,float sidewall){
  if(System.Environment.GetEnvironmentVariable("MYSIM_WHEEL_LETTERING")!="1")return;
  foreach(float side in new[]{-1f,1f})foreach(var band in new[]{("MYSIM",Mathf.Pi*.5f),("RACING",Mathf.Pi*1.5f)}){
   string text=band.Item1;float spacing=.105f;
   for(int i=0;i<text.Length;i++){float angle=band.Item2+(i-(text.Length-1)*.5f)*spacing;var radial=new Vector3(0,Mathf.Cos(angle),Mathf.Sin(angle));var tangent=new Vector3(0,-Mathf.Sin(angle),Mathf.Cos(angle))*(-side);
    var glyph=new TextMesh {Text=text[i].ToString(),Font=ThemeDB.FallbackFont,FontSize=64,PixelSize=radius*.0010f,Depth=.0003f,HorizontalAlignment=HorizontalAlignment.Center};
    var node=new MeshInstance3D {Name="SidewallLetter",Mesh=glyph,Position=centre+Vector3.Right*(side*(sidewall+.0005f))+radial*(radius*.70f),Basis=new Basis(tangent,radial,tangent.Cross(radial)),CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};markings.AddChild(node);
   }
  }
 }
 public static void Apply(Node3D[] pivots,Compound compound){
  if(pivots==null)return;
  foreach(var pivot in pivots){
   var spin=pivot.GetNode<Node3D>("Spin");var rebuilt=spin.GetNodeOrNull<MeshInstance3D>("RebuiltRubberTyre");if(rebuilt!=null)TyreMeshBuilder.SetWet(rebuilt,compound==Compound.Wet);Node3D markings=spin.GetNodeOrNull<Node3D>("CompoundMarkings");
   if(markings==null){
    markings=new Node3D {Name="CompoundMarkings"};spin.AddChild(markings);
    foreach(var node in spin.GetChildren())if(node is MeshInstance3D tyre&&(rebuilt!=null?tyre==rebuilt:tyre.Visible&&(tyre.Name.ToString().Contains("TYRE")||tyre.Name.ToString().Contains("TIRE")))){
     var bounds=tyre.Transform*tyre.GetAabb();var center=bounds.GetCenter();float radius=rebuilt!=null?rebuilt.GetMeta("radius").AsSingle():Mathf.Max(bounds.Size.Y,bounds.Size.Z)*.5f;float sidewall=rebuilt!=null?rebuilt.GetMeta("width").AsSingle()*.525f:bounds.Size.X*.5f+.0015f;
     if(radius<.15f||radius>.6f)continue;
     foreach(float side in new[]{-1f,1f}){
      var ring=new MeshInstance3D {Name=side<0?"InnerSidewall":"OuterSidewall",Mesh=new TorusMesh {InnerRadius=radius*.805f,OuterRadius=radius*.83f,Rings=96,RingSegments=8},Position=center+Vector3.Right*(side*sidewall),Rotation=new Vector3(0,0,Mathf.Pi*.5f),CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};
      if(System.Environment.GetEnvironmentVariable("MYSIM_FLAT_TYRE_MARKINGS")=="1"){ring.Mesh=FlatMarking(radius,side);ring.Rotation=Vector3.Zero;ring.Position=center+Vector3.Right*(side*(sidewall+.0002f));}
      markings.AddChild(ring);
     }
     if(rebuilt!=null)Lettering(markings,center,radius,sidewall);
     break;
    }
   }
   foreach(var child in markings.GetChildren())if(child is MeshInstance3D ring)ring.MaterialOverride=new StandardMaterial3D {AlbedoColor=Colour(compound),Roughness=.85f,MetallicSpecular=.1f};
   pivot.SetMeta("displayed_compound",(int)compound);
  }
 }
}
}
