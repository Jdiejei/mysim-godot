import argparse,hashlib,json,os,pathlib,struct,tarfile
from cryptography.hazmat.primitives.ciphers import Cipher,algorithms,modes
MAGIC=b'MYSIMENC1'
def decrypt(src,dest,key):
 with open(src,'rb') as f:
  if f.read(len(MAGIC))!=MAGIC:raise ValueError('invalid encrypted input')
  nonce=f.read(12);f.seek(-16,2);tag=f.read(16);end=f.tell()-16;f.seek(len(MAGIC)+12)
  dec=Cipher(algorithms.AES(bytes.fromhex(key)),modes.GCM(nonce,tag)).decryptor()
  with open(dest,'wb') as w:
   while f.tell()<end:w.write(dec.update(f.read(min(4*1024*1024,end-f.tell()))))
   w.write(dec.finalize())
def main():
 p=argparse.ArgumentParser();p.add_argument('encrypted');p.add_argument('destination');a=p.parse_args();dest=pathlib.Path(a.destination);dest.mkdir(parents=True,exist_ok=True);plain=dest/'input.tar'
 decrypt(a.encrypted,plain,os.environ['MYSIM_INTAKE_KEY'])
 with tarfile.open(plain) as t:t.extractall(dest,filter='data')
 plain.unlink()
if __name__=='__main__':main()
