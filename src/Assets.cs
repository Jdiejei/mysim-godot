using Godot;
using System.Collections.Generic;
namespace MySim {
// Directory listing that also works in exported builds, where files appear as "x.fbx.import" / "x.png.remap".
public static class Assets {
 public static List<string> Files(string dir) {
  var set = new SortedSet<string>();
  var arr = DirAccess.GetFilesAt(dir);
  if (arr != null) foreach (var f in arr) { string n = f; if (n.EndsWith(".import")) n = n.Substring(0, n.Length - 7); else if (n.EndsWith(".remap")) n = n.Substring(0, n.Length - 6); set.Add(n); }
  return new List<string>(set);
 }
}}
