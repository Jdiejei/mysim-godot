import pathlib,tarfile,os,json,hashlib,sys
from cryptography.hazmat.primitives.ciphers import Cipher,algorithms,modes
src=pathlib.Path(sys.argv[1]);dest=pathlib.Path(sys.argv[2]);plain=dest.with_suffix('.tar')
with tarfile.open(plain,'w') as t:t.add(src,arcname='review')
nonce=os.urandom(12);enc=Cipher(algorithms.AES(bytes.fromhex(os.environ['MYSIM_INTAKE_KEY'])),modes.GCM(nonce)).encryptor()
with plain.open('rb') as f,dest.open('wb') as w:
 w.write(b'MYSIMENC1'+nonce)
 for b in iter(lambda:f.read(4*1024*1024),b''):w.write(enc.update(b))
 w.write(enc.finalize());w.write(enc.tag)
plain.unlink();print('encrypted review output',dest.name,dest.stat().st_size)
