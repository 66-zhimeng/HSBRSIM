import sys, dis, types, importlib
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import mapping as Map
# recursively find code objects with 'buy' or 'phase'
def walk(co, out, path=""):
    for c in co.co_consts:
        if isinstance(c, types.CodeType):
            walk(c, out, path+"/"+c.co_name)
    names = co.co_names + tuple(x.decode() if isinstance(x,bytes) else str(x) for x in co.co_consts if isinstance(x,(str,bytes)))
    for key in ("buy","phase","recruit","combat","choices","shop"):
        if key in names:
            out.append((path or co.co_name, key, co))
        break
out=[]; walk(Map.__dict__.get("Mapper").frame.__code__, out, "Mapper.frame")
# also module-level functions
for n,o in vars(Map).items():
    if isinstance(o, types.FunctionType):
        walk(o.__code__, out, n)
print("functions mentioning buy/phase/etc:", [(p,k) for p,k,_ in out][:30])
for path,key,co in out:
    if key in ("buy","phase"):
        print("\n"+"="*70); print(path, "key", key, "| names:", co.co_names[:40]); print("consts:", [c for c in co.co_consts if not hasattr(c,'co_code')][:30])
        for idx,ins in enumerate(dis.get_instructions(co)):
            if idx>70: break
            print(f"  {ins.offset:5d} {ins.opname:24s} {str(ins.argval)[:90]}")
