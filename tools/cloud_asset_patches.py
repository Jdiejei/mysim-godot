import pathlib,json,subprocess,tempfile,os,tarfile,hashlib
from cloud_decrypt import decrypt
root=pathlib.Path.cwd()
for patch in json.loads((root/'data/asset_patches.json').read_text()):
 with tempfile.TemporaryDirectory() as temp:
  folder=pathlib.Path(temp);subprocess.run(['gh','release','download','repair-inputs-20261029','--repo','instagramshashank8-bot/mysim-godot','--pattern',patch['part'],'--dir',temp],check=True)
  plain=folder/'patch.tar';decrypt(folder/patch['part'],plain,os.environ['MYSIM_INTAKE_KEY'])
  with tarfile.open(plain) as archive:
   for member in archive.getmembers():
    if not member.isfile() or member.name not in patch['files']:raise ValueError('Unexpected asset patch entry')
    data=archive.extractfile(member).read()
    if hashlib.sha256(data).hexdigest()!=patch['files'][member.name]:raise ValueError('Asset patch checksum mismatch')
    path=root/member.name
    if not path.resolve().is_relative_to((root/'assets').resolve()):raise ValueError('Asset patch outside assets')
    path.parent.mkdir(parents=True,exist_ok=True);path.write_bytes(data)
  print('Verified recovered asset patch',patch['part'])
