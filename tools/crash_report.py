import json,glob,os,re,sys,collections
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
src=open('src/Logic/SourceRoutes.cs').read()
arrays=re.findall(r'new Vec\[\]\{(.*?)\}\s*[,}]',src,flags=re.S)
tracks=[[ (float(a),float(b)) for a,b in re.findall(r'new Vec\(([-\d.]+)f,([-\d.]+)f\)',blk)] for blk in arrays]
names=re.findall(r'"([A-Z0-9 ÜÄÖ\.\-]+)"',re.search(r'Names=\{(.*?)\}',src,flags=re.S).group(1))
rows=[]
for f in glob.glob(sys.argv[1]+'/**/*.jsonl',recursive=True):
    for l in open(f):
        l=l.strip()
        if l: rows.append(json.loads(l))
summ=[r for r in rows if r.get('summary')]
rows=[r for r in rows if not r.get('summary')]
TOT=sum(r['crashes'] for r in summ); FLG=sum(r['flagged'] for r in summ)
sumtrack=collections.Counter(); sumflag=collections.Counter(); sumcar=collections.Counter(); sumcarflag=collections.Counter()
for r in summ:
    sumtrack[r['track']]+=r['crashes']; sumflag[r['track']]+=r['flagged']; sumcar[r['car']]+=r['crashes']; sumcarflag[r['car']]+=r['flagged']
print('crashes run',TOT,'flagged',FLG,'stored rows',len(rows))
flagc=collections.Counter(); bytrack=collections.defaultdict(collections.Counter); bycar=collections.defaultdict(collections.Counter); tot=collections.Counter(); totcar=collections.Counter()
for r in rows:
    tot[r['track']]+=1; totcar[r.get('car',8)]+=1
    for fl in r['flags']:
        flagc[fl]+=1; bytrack[r['track']][fl]+=1; bycar[r.get('car',8)][fl]+=1
out=['# Crash campaign report','',f'Crashes run: {TOT}; with at least one flag: {FLG} ({100*FLG/max(TOT,1):.1f}%)','', '## Flags overall','']
for k,v in flagc.most_common(): out.append(f'- {k}: {v} ({100*v/max(TOT,1):.1f}% of all crashes)')
out+=['','## Worst layouts (share of crashes with any flag)','']
bad=[(sumflag[t]/max(sumtrack[t],1),t) for t in sumtrack]
for share,t in sorted(bad,reverse=True)[:15]: out.append(f'- layout {t} {names[t] if t<len(names) else ""}: {100*share:.0f}% of {sumtrack[t]} crashes flagged; '+', '.join(f'{k} {v}' for k,v in bytrack[t].most_common(3)))
out+=['','## By car','']
for c in sorted(sumcar): out.append(f'- car {c}: {sumcar[c]} crashes, {100*sumcarflag[c]/max(sumcar[c],1):.0f}% flagged; '+', '.join(f'{k} {v}' for k,v in bycar[c].most_common(3)))
os.makedirs('crashmap/maps',exist_ok=True)
col={'GROUND_JUMP':'red','VOID_GROUND':'magenta','ESCAPED_TRACK':'orange','STUCK_AT_WALL':'blue','SPEED_GAIN':'green','NONFINITE':'black','HEIGHT_RANGE':'brown','NO_WALL_CONTACT_AT_HIGH_ANGLE':'cyan'}
for t in sorted(tot):
    if t>=len(tracks) or len(tracks[t])<3: continue
    pts=tracks[t]; n=len(pts); fig,ax=plt.subplots(figsize=(7,7)); ax.plot([p[0] for p in pts],[p[1] for p in pts],color='lightgray',lw=3)
    cnt=collections.defaultdict(collections.Counter)
    for r in rows:
        if r['track']!=t: continue
        for fl in r['flags']: cnt[round(r['p'],3)][fl]+=1
    for p,c in cnt.items():
        i=int(p*n)%n; x,z=pts[i]; top=c.most_common(1)[0]; ax.scatter([x],[z],s=20+4*sum(c.values())**0.5*5,c=col.get(top[0],'gray'),alpha=.7)
    ax.set_aspect('equal'); ax.set_title(f'{t} {names[t] if t<len(names) else ""} - crash defects ({sumtrack[t]} crashes)'); fig.savefig(f'crashmap/maps/layout_{t:02d}.png',dpi=70); plt.close(fig)
open('crashmap/CRASH_REPORT.md','w').write('\n'.join(out)); print('\n'.join(out[:30]))
