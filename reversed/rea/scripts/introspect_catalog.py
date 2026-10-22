import sys, inspect, importlib, dataclasses
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
m=importlib.import_module("bgassistant.catalog")
print("DOC:", (m.__doc__ or "")[:300])
for attr,obj in sorted(vars(m).items()):
    if inspect.ismodule(obj) or attr.startswith("__"): continue
    if inspect.isfunction(obj):
        try: sig=str(inspect.signature(obj))
        except Exception: sig="(?)"
        print("def", attr, sig)
        d=(inspect.getdoc(obj) or "").splitlines()
        if d: print("   #", d[0][:150])
    elif inspect.isclass(obj) and obj.__module__==m.__name__:
        print("class", attr, "| fields:", ([f.name for f in dataclasses.fields(obj)] if dataclasses.is_dataclass(obj) else [a for a in vars(obj) if not a.startswith('_')][:15]))
        for a2 in [a for a in vars(obj) if not a.startswith('_')][:15]:
            o2=getattr(obj,a2)
            if callable(o2):
                try: print("    def", a2, inspect.signature(o2))
                except Exception: pass
