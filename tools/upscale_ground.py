import json,os,sys,subprocess,tempfile,shutil
import numpy as np
from PIL import Image
EXE=r'C:\Users\shash\AppData\Local\Temp\resr\x\realesrgan-ncnn-vulkan.exe'; MODELS=r'C:\Users\shash\AppData\Local\Temp\resr\x\models'
files=json.load(open('upscale_list.json')); done=0; fail=0
for p in files:
    if p.endswith('_up.png'): continue
    im=Image.open(p).convert('RGBA'); w,h=im.size
    if w>1024 or h>1024 or w<64: continue
    a=np.array(im); pad=min(32,max(8,min(w,h)//4)); rgb=np.pad(a[:,:,:3],((pad,pad),(pad,pad),(0,0)),mode='wrap')
    with tempfile.TemporaryDirectory() as tmp:
        i=os.path.join(tmp,'i.png'); o=os.path.join(tmp,'o.png'); Image.fromarray(rgb).save(i)
        r=subprocess.run([EXE,'-i',i,'-o',o,'-m',MODELS,'-n','realesrgan-x4plus','-s','4','-t','256'],capture_output=True,timeout=900)
        if r.returncode or not os.path.exists(o): fail+=1; print('FAIL',p,r.stderr[-200:]); continue
        big=Image.open(o).convert('RGB'); big=big.crop((pad*4,pad*4,(pad+w)*4,(pad+h)*4)).resize((w*2,h*2),Image.Resampling.LANCZOS)
        out=big.convert('RGBA'); al=Image.fromarray(a[:,:,3]).resize((w*2,h*2),Image.Resampling.LANCZOS); out.putalpha(al)
        shutil.copyfile(p,p+'.orig_backup') if not os.path.exists(p+'.orig_backup') else None
        out.save(p); done+=1
    if done%10==0: print('done',done,flush=True)
print('UPSCALED',done,'FAILED',fail)
