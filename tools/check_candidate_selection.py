from candidate_selection import select_candidate
from types import SimpleNamespace
class Object:
 def __init__(self,name,vertices=36,faces=56):self.name=name;self.data=SimpleNamespace(name=name,vertices=range(vertices),polygons=range(faces))
 def get(self,key,default):return default
r={'mesh':'gnd_road_track_39_mm302','procedure_id':'unique-id','vertices':36,'faces':56}
root=Object(r['mesh']+'__program_3');helpers=[Object(root.name+'__LOD'+str(i)) for i in (1,2,3)]+[Object(root.name+'__original_collision',18,10)]
assert select_candidate([*helpers,root],r)==(root,'explicit_program_root')
try:select_candidate([root,Object(r['mesh']+'__program_4'),*helpers],r)
except ValueError:pass
else:raise AssertionError('ambiguous program roots accepted')
try:select_candidate(helpers,r)
except ValueError:pass
else:raise AssertionError('helpers accepted as main object')
other=Object(r['mesh']+'__program_10',12,8);assert select_candidate([root,other,*helpers],r)==(root,'unique_program_root_topology')
exact=Object(r['mesh']);assert select_candidate([exact,root,*helpers],r)[0] is exact
print('PASS candidate selection: explicit root, helper exclusion, exact name and ambiguity rejection')
