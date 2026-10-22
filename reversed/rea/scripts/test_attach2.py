import sys, traceback
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
info = M.process_info()
print("info:", info)
r = M.SnapshotReader(info['pid'])
print("reader:", type(r).__name__, "attrs:", [a for a in dir(r) if not a.startswith('__')][:30])
try:
    image = r.attach_image(info['mono_base'])
    print("image:", type(image).__name__)
    print("image keys/attrs:", [a for a in dir(image) if not a.startswith('__')][:40])
    for a in ("name","assembly","classes","types","base","address","header"):
        if hasattr(image,a):
            v=getattr(image,a)
            try: print(f"  image.{a} = {str(v)[:200]}")
            except Exception: pass
    try:
        gs = r.game_state(image)
        print("game_state:", gs)
    except Exception as e:
        print("game_state raised:", type(e).__name__, e)
finally:
    try: r.close()
    except Exception: pass
