import sys, dis, inspect
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
from bgcontroller.client import mono as Mono
from bgcontroller.client.memory import SnapshotReader
print("SnapshotReader MRO:", [c.__name__ for c in SnapshotReader.__mro__])
print("__init__", inspect.signature(SnapshotReader.__init__))
for meth in ("attach_image","bind_metadata","game_state","class_fields","entity"):
    fn=getattr(Mono.Reader, meth, None)
    if fn is None: continue
    print("\n"+"="*70); print("Reader."+meth, inspect.signature(fn), "| doc:", (inspect.getdoc(fn) or "")[:200])
    code=fn.__code__
    print("co_names:", code.co_names)
    print("co_consts:", [c for c in code.co_consts if not hasattr(c,'co_code')][:30])
    for idx,ins in enumerate(dis.get_instructions(code)):
        if idx>55: break
        print(f"  {ins.offset:5d} {ins.opname:24s} {str(ins.argval)[:100]}")
