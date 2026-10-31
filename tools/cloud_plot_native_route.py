"""Runner-only trajectory diagnostic from actual native telemetry, never a game render."""
import json,sys,pathlib,math
from PIL import Image,ImageDraw,ImageFont
source=pathlib.Path(sys.argv[1]);data=json.loads(source.read_text());trace=data['telemetry'];route=data['route'];assert trace and all('x' in p and 'z' in p for p in trace)
points=route+[[p['x'],p['z']] for p in trace];assert all(math.isfinite(v) for p in points for v in p)
mn=[min(p[i] for p in points) for i in (0,1)];mx=[max(p[i] for p in points) for i in (0,1)];scale=min(1080/max(1,mx[0]-mn[0]),780/max(1,mx[1]-mn[1]))
def xy(p):return(60+(p[0]-mn[0])*scale,910-(p[1]-mn[1])*scale)
img=Image.new('RGB',(1200,1040),'#111c28');draw=ImageDraw.Draw(img)
try:font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',18)
except OSError:font=ImageFont.load_default()
draw.text((32,20),f"Actual native trajectory: track{data['track']} car{data['car']} | ok={data['ok']} | laps={data['laps']} | resets={data['ruleResets']}",font=font,fill='white')
draw.text((32,50),'Grey: source route | blue: throttle | orange: braking | magenta: reverse | green start / red end',font=font,fill='#c4d4e2')
draw.line([xy(p) for p in route+[route[0]]],fill='#778393',width=max(2,round(data['routeWidth']*scale)))
for a,b in zip(trace,trace[1:]):draw.line([xy([a['x'],a['z']]),xy([b['x'],b['z']])],fill='#e270f0' if b['reverse'] else '#ffbd62' if b['brake']>.1 else '#53d5ed',width=2)
for point,color in ((trace[0],'#6aff99'),(trace[-1],'#ff5050')):
 x,y=xy([point['x'],point['z']]);draw.ellipse((x-6,y-6,x+6,y+6),fill=color)
last=trace[-1];draw.text((32,955),f"End progress={last['progress']:.4f} gate={last['nextGate']} lateral={last['lateral']:.2f}m speed={last['speed']:.2f}m/s",font=font,fill='white')
draw.text((32,985),f"AI recoveries={last['aiRecoveries']} | lapInvalid={last['lapInvalid']} | pit={last['pitActive']} | chassis/tyre contacts={last['chassisContacts']}/{last['contacts']}",font=font,fill='#c4d4e2')
img.save(source.with_suffix('.trajectory.png'));print('Saved actual route diagnostic, not gameplay visual acceptance')
