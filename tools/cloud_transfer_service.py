import pathlib,sys,os,msvcrt,time,traceback,runpy,json
root=pathlib.Path(__file__).resolve().parents[1];os.chdir(root);base=pathlib.Path('D:/MySim-transport/repair-20261029');base.mkdir(parents=True,exist_ok=True)
lock=(base/'service.lock').open('a+b');lock.seek(0);lock.write(b'0');lock.flush();lock.seek(0)
try:msvcrt.locking(lock.fileno(),msvcrt.LK_NBLCK,1)
except OSError:sys.exit(0)
log=(root/'outputs/repair-20261029/cloud-transfer-service.log').open('a',encoding='utf-8',buffering=1);sys.stdout=log;sys.stderr=log
(root/'outputs/repair-20261029/cloud-transport.pid').write_text(str(os.getpid()))
for attempt in (1,2):
 try:
  print('SERVICE_START',os.getpid(),'attempt',attempt,time.time(),flush=True);runpy.run_path(str(root/'tools/cloud_transport.py'),run_name='__main__');print('SERVICE_FINISHED',time.time(),flush=True);break
 except Exception:
  traceback.print_exc();print('SERVICE_FAILURE',attempt,time.time(),flush=True)
  if attempt==1:time.sleep(60)
