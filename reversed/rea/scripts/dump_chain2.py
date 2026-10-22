import sys, dis, inspect
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import mono as Mono, memory as M
def dump(name, fn):
    print("\n"+"="*80); print(name, "| sig:", inspect.signature(fn))
    code=fn.__code__
    print("doc:", (inspect.getdoc(fn) or "")[:220])
    print("co_names:", code.co_names)
    print("co_consts:", [c for c in code.co_consts if not hasattr(c,'co_code')])
dump("Reader.__init__", Mono.Reader.__init__)
dump("SnapshotReader.__init__", M.SnapshotReader.__init__)
dump("process_info", M.process_info)
dump("process_running", M.process_running)
dump("newest_running_process", M.newest_running_process)
dump("session_build", M.session_build)
dump("process_info(own)", M.process_info)
