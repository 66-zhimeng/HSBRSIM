import sys, inspect, dataclasses, importlib, dis, types
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
for name in ["bgcontroller.client.memory","bgcontroller.client.tags","bgcontroller.client.leaderboard","bgcontroller.client.history","bgcontroller.client.choices"]:
    m=importlib.import_module(name)
    print("\n"+"="*80); print("###",name)
    print("DOC:", (m.__doc__ or "").strip()[:400])
    for attr,obj in sorted(vars(m).items()):
        if inspect.ismodule(obj): continue
        if inspect.isfunction(obj):
            try: sig=str(inspect.signature(obj))
            except Exception: sig="(?)"
            print(f"  def {attr}{sig}")
            d=(inspect.getdoc(obj) or "").strip().splitlines()
            if d: print("       #",d[0][:150])
        elif inspect.isclass(obj) and obj.__module__==name:
            print(f"  class {attr}: {[a for a in vars(obj) if not a.startswith('_')]}")
        elif attr.startswith("_") and isinstance(obj,(int,float,str,bytes,tuple,list,dict,frozenset)) and not isinstance(obj,types.ModuleType):
            s=repr(obj)
            print(f"  CONST {attr} = {s[:200]}")
        elif not attr.startswith("_") and isinstance(obj,(int,float,str,bytes,tuple,list,dict,frozenset)) and not callable(obj):
            print(f"  CONST {attr} = {repr(obj)[:200]}")
