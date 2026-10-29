"""Select a render candidate without mistaking generated LOD/collision helpers for it."""
import re

def select_candidate(objects, request):
 name=request['mesh']
 exact=[o for o in objects if o.name==name or o.data.name==name]
 if len(exact)==1:return exact[0],'name'
 if len(exact)>1:raise ValueError('ambiguous exact candidate names: '+repr([o.name for o in exact]))
 # The pipeline appends a program suffix to the authored render object; helpers append another suffix.
 pattern=re.compile(re.escape(name)+r'__program_\d+$')
 roots=[o for o in objects if pattern.fullmatch(o.name)]
 if len(roots)==1:return roots[0],'explicit_program_root'
 if len(roots)>1:
  measured=[o for o in roots if len(o.data.vertices)==int(request['vertices']) and len(o.data.polygons)==int(request['faces'])]
  if len(measured)==1:return measured[0],'unique_program_root_topology'
  raise ValueError('ambiguous program roots: '+repr([(o.name,len(o.data.vertices),len(o.data.polygons)) for o in roots]))
 eligible=[o for o in objects if not re.search(r'__(LOD\d+|original_collision)$',o.name,re.I)]
 matches=[o for o in eligible if str(o.get('source_mesh',''))==name or request['procedure_id'] in o.name]
 reason='provenance'
 if not matches:
  matches=[o for o in eligible if len(o.data.vertices)==int(request['vertices']) and len(o.data.polygons)==int(request['faces'])];reason='unique_manifest_topology'
 if len(matches)==1:return matches[0],reason
 if len(objects)==1:return objects[0],'single_checksum_verified_candidate'
 raise ValueError('ambiguous candidate objects: '+repr([(o.name,len(o.data.vertices),len(o.data.polygons)) for o in objects[:16]]))
