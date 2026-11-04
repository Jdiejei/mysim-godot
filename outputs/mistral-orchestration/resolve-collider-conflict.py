import pathlib
p = pathlib.Path('src/NativeCarColliders.cs')
t = p.read_text()
start = t.index('<<<<<<< HEAD')
sep = t.index('=======')
end = t.index('>>>>>>> mistral/physics-gameplay')
head_side = t[start + len('<<<<<<< HEAD\n'):sep]
ours_side = t[sep + len('=======\n'):end]
# Physics side wins: their WheelTemplate (without WHEELGUARD|BRAKE_CALIPER) + the HubForWheelAssemblyPart
# block. The hub-restore supersedes the 0de3123 ablation per the reviewer's restore mandate.
t = t[:start] + ours_side.rstrip('\n') + '\n' + t[end + len('>>>>>>> mistral/physics-gameplay\n'):]
p.write_text(t)
print('resolved; markers left:', t.count('<<<<<<<') + t.count('>>>>>>>'))
print('head_side dropped:', head_side.count('WHEELGUARD') > 0)
