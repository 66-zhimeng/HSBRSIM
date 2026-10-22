import sys, dis, inspect
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import mono as Mono, memory as M
def dump(name, fn):
    print("\n"+"="*80); print(name, "| sig:", inspect.signature(fn))
    print("doc:", (inspect.getdoc(fn) or "")[:300])
    code=fn.__code__
    print("co_names:", code.co_names)
    print("co_consts:", [c for c in code.co_consts if not hasattr(c,'co_code')])
for name in ("attach_image","game_state","metadata_context","bind_metadata"):
    fn=getattr(Mono.Reader, name, None) or getattr(Mono, name, None)
    if fn: dump("mono."+name, fn)
for name in ("__init__",):
    dump("SnapshotReader."+name, M.SnapshotReader.__dict__.get(name))
dump("capture_snapshot", M.capture_snapshot)
dump("process_info", M.process_info)
dump("session_build", M.session_build)
