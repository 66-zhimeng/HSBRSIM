import sys, inspect, dis
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import mapping as Map
fn=getattr(Map.Mapper,"_legal",None)
print("_legal:", fn and inspect.signature(fn))
if fn:
    print("doc:", inspect.getdoc(fn))
    print("names:", fn.__code__.co_names)
    print("consts:", [c for c in fn.__code__.co_consts if not hasattr(c,'co_code')])
    for i,ins in enumerate(dis.get_instructions(fn)):
        if i>90: break
        print(f"  {ins.offset:4d} {ins.opname:22s} {str(ins.argval)[:90]}")
