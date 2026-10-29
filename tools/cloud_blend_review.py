# Run with Blender --background --disable-autoexec --python this_file -- request.json result.json
import bpy,sys,json,math,hashlib,pathlib
args=sys.argv[sys.argv.index('--')+1:];request=json.load(open(args[0]));rows=[]
for r in request:
 try:
  bpy.ops.wm.open_mainfile(filepath=r['blend_path'],load_ui=False)
  objects=[o for o in bpy.data.objects if o.type=='MESH'];matches=[o for o in objects if o.name==r['mesh'] or o.data.name==r['mesh']]
  if not matches:raise ValueError('named source mesh missing; not substituting a different object')
  o=matches[0];m=o.data;finite=all(math.isfinite(c) for v in m.vertices for c in v.co);uv_finite=all(math.isfinite(c) for layer in m.uv_layers for u in layer.data for c in u.uv)
  if not finite or not uv_finite:raise ValueError('nonfinite geometry or UV')
  h=hashlib.sha256()
  for v in m.vertices:h.update((','.join(format(float(c),'.9g') for c in v.co)+';').encode())
  missing=[im.name for im in bpy.data.images if im.source=='FILE' and not im.packed_file and im.filepath and not pathlib.Path(bpy.path.abspath(im.filepath)).exists()]
  bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
  dest=pathlib.Path(r['glb_path']);dest.parent.mkdir(parents=True,exist_ok=True)
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=False,export_texcoords=True,export_normals=True)
  rows.append({'id':r['procedure_id'],'ok':True,'vertices':len(m.vertices),'faces':len(m.polygons),'uv_layers':len(m.uv_layers),'geometry_sha256':h.hexdigest(),'missing_textures':missing,'quality_accepted':False,'manual_reviewed':False,'glb':str(dest)})
 except Exception as e:rows.append({'id':r['procedure_id'],'ok':False,'error':str(e),'quality_accepted':False,'manual_reviewed':False})
pathlib.Path(args[1]).write_text(json.dumps(rows,indent=2))
