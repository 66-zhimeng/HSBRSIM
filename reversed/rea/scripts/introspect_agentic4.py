import sys, inspect, importlib, dataclasses, dis
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
from bgcontroller.client import mono as Mono
from bgcontroller import records as R
from bgcontroller.client import mapping as Map
print("=== memory private/public names ===")
print([n for n in vars(M) if not n.startswith('__')])
print("\n=== mono names ===")
print([n for n in vars(Mono) if not n.startswith('__')])
print("\n=== records.Frame fields ===")
for n in ("Frame","Legal","Play","Ref","Use"):
    cls=getattr(R,n)
    print(n, [f.name+':'+str(f.type) for f in dataclasses.fields(cls)])
print("\n=== Mapper.frame signature/doc ===")
print(inspect.signature(Map.Mapper.frame)); print(inspect.getdoc(Map.Mapper.frame))
