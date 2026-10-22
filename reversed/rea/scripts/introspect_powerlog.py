import sys, inspect, importlib
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
m=importlib.import_module("bgcontroller.client.powerlog")
print("DOC:", (m.__doc__ or "")[:500])
for attr,obj in sorted(vars(m).items()):
    if inspect.ismodule(obj) or attr.startswith("__"): continue
    if inspect.isfunction(obj):
        try: sig=str(inspect.signature(obj))
        except Exception: sig="(?)"
        print("def", attr, sig)
        d=(inspect.getdoc(obj) or "").splitlines()
        if d: print("   #", d[0][:150])
    elif inspect.isclass(obj) and obj.__module__==m.__name__:
        print("class", attr, "methods:", [a for a in vars(obj) if not a.startswith('_')][:20])
