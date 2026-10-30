"""Limit only isolated diagnostics' importer scope; full-game/Windows campaigns stay complete."""
import os,json,pathlib
root=pathlib.Path.cwd();depth=os.environ.get('IMPORT_REVIEW_DEPTH','');out=root/'outputs/cloud-game';out.mkdir(parents=True,exist_ok=True)
scopes={'car_panel_uv':([6],[]),'car_uv_roles':([1,6,16],[]),'car_uv':([1,6,16],[]),'precise_ai':([16],[43,15]),'contact_trace':([16],[43,15])}
if depth not in scopes:
 print('Full source import retained for',depth);raise SystemExit(0)
cars,tracks=scopes[depth];catalog=['2018_ferrari_488_gt3','2018_audi_r8_gt4','2018_bmw_m4_gt4','2018_chevrolet_camaro_gt4r','2018_bmw_m8_gte','2018_chevrolet_camaro_zl1_1le','2018_lamborghini_huracan_gt3','2016_mercedes_amg_gt3','2015_mclaren_650s_gt3','2018_acura_nsx_gt3','2018_bmw_m6_gt3','2018_porsche_911_rsr','2016_ford_gt_lm','2018_astonmartin_gte','2016_chevrolet_c7r','2019_f1_mercedes_w10','2019_f1_ferrari_sf90','2019_f1_red_bull_rb15','2019_f1_mclaren_mcl34','2015_mclaren_p1_gtr','2017_bugatti_chiron','2018_porsche_919_hybrid_evo','2017_oreca_07_lmp2','2018_porsche_911_gt3_rs']
keep={'common',*(catalog[i].casefold() for i in cars)};trackList=json.loads((root/'assets/RealRacing/TrackList.json').read_text())['tracks']
for i in tracks:
 track=trackList[i];keep.add(track['map'].casefold())
 for path in track['variationPaths']:
  marker='Assets/RealRacing/Source/'
  if marker in path:keep.add(path.split(marker,1)[1].split('/')[0].casefold())
source=root/'assets/RealRacing/Source';available={p.name.casefold() for p in source.iterdir() if p.is_dir()};missing=keep-available
if missing:raise ValueError('Diagnostic source folders missing: '+str(sorted(missing)))
skipped=[]
for folder in source.iterdir():
 if folder.is_dir() and folder.name.casefold() not in keep:
  marker=folder/'.gdignore'
  if marker.exists():raise ValueError('Unexpected pre-existing import exclusion: '+str(folder))
  marker.write_text('Temporary isolated-diagnostic import scope; original assets remain intact.\n');skipped.append(folder.name)
(out/'import-scope.json').write_text(json.dumps({'depth':depth,'cars':cars,'tracks':tracks,'retained_source_folders':sorted(keep),'excluded_unrelated_source_folders':sorted(skipped),'scope':'isolated diagnostic only; no all-assets acceptance'},indent=2))
print('Isolated import scope',depth,'retains',sorted(keep),'excludes unrelated folders',len(skipped))
