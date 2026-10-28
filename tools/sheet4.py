from PIL import Image,ImageDraw
import os,sys,json
names=[x['name'] for x in json.load(open('assets/RealRacing/TrackList.json'))['tracks']]
os.makedirs('shots4',exist_ok=True)
for k in range(15):
    sheet=Image.new('RGB',(1920,1080))
    for i in range(4):
        t=k*4+i; f=f'shots/t{t}.png'
        if os.path.exists(f):
            im=Image.open(f).convert('RGB').resize((960,540)); d=ImageDraw.Draw(im); d.rectangle((0,500,960,540),fill=(0,0,0)); d.text((10,512),f'{t} {names[t]}',fill=(255,255,255)); sheet.paste(im,((i%2)*960,(i//2)*540))
    sheet.save(f'shots4/s{k:02d}.png')
print('sheets written')
