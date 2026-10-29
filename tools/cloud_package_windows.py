# Encrypt a Windows build to release parts without creating a second full-size archive.
import pathlib,os,tarfile,subprocess,hashlib,json
from cryptography.hazmat.primitives.ciphers import Cipher,algorithms,modes
root=pathlib.Path('build-cloud');parts=[]
class Writer:
 def __init__(self):self.f=None;self.size=0
 def open(self):
  self.path=pathlib.Path(os.environ.get('RUNNER_TEMP','/tmp'))/f'windows-build-{len(parts):05}.enc.part';self.f=self.path.open('wb');self.size=0;self.sha=hashlib.sha256()
 def flushpart(self):
  if not self.f:return
  self.f.close()
  for attempt in (1,2):
   p=subprocess.run(['gh','release','upload','repair-review-20261029',str(self.path),'--clobber'])
   if p.returncode==0:break
  if p.returncode:raise RuntimeError('Windows part upload failed after one retry')
  parts.append({'name':self.path.name,'bytes':self.size,'sha256':self.sha.hexdigest()});self.path.unlink();self.f=None
 def write(self,data):
  total=len(data)
  while data:
   if self.f is None:self.open()
   take=min(len(data),512*1024*1024-self.size);b=data[:take];self.f.write(b);self.sha.update(b);self.size+=take;data=data[take:]
   if self.size>=512*1024*1024:self.flushpart()
  return total
writer=Writer();nonce=os.urandom(12);writer.write(b'MYSIMENC1'+nonce);cipher=Cipher(algorithms.AES(bytes.fromhex(os.environ['MYSIM_INTAKE_KEY'])),modes.GCM(nonce)).encryptor()
class Encrypt:
 def write(self,b):writer.write(cipher.update(b));return len(b)
with tarfile.open(fileobj=Encrypt(),mode='w|') as tar:tar.add(root,arcname='MySim')
writer.write(cipher.finalize());writer.write(cipher.tag);writer.flushpart()
p=pathlib.Path('windows-build-manifest.json');p.write_text(json.dumps({'parts':parts,'source_commit':os.environ['GITHUB_SHA'],'format':'concatenate parts, then AES-GCM decrypt with the project intake key','verified_windows_logic':True,'visual_acceptance':False},indent=2));subprocess.run(['gh','release','upload','repair-review-20261029',str(p),'--clobber'],check=True)
