using Godot;
using System;
namespace MySim {
// Explicit experiment: recover real visible paved runoff geometry as solid support.
// No new plane, route override, barrier exclusion, UV change or source-mesh deformation.
public static class SourceRunoffSupport {
 public static void Build(Node3D layer){int meshes=0,triangles=0;var nodes=layer.FindChildren("*","MeshInstance3D",true,false);foreach(var node in nodes){var mesh=(MeshInstance3D)node;if(!mesh.IsVisibleInTree()||mesh.Mesh==null||!mesh.Name.ToString().StartsWith("gnd_road_runoff",StringComparison.OrdinalIgnoreCase))continue;var faces=mesh.Mesh.GetFaces();if(faces.Length==0)continue;WallBuilder.AddFaces(mesh,faces,1,mesh.Name+"_source_runoff");meshes++;triangles+=faces.Length/3;}nodes.Clear();GD.Print("SOURCE_RUNOFF_SUPPORT meshes=",meshes," intactTriangles=",triangles);}
}
}
