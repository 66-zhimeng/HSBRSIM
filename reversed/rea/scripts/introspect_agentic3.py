import sys, inspect, importlib, dataclasses
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
for name in ["bgassistant.reader","bgcontroller.client.mapping","bgcontroller.client.mechanics","bgcontroller.state","bgcontroller.records"]:
    try: m=importlib.import_module(name)
    except Exception as e: print("###",name,"IMPORT FAIL",repr(e)); continue
    print("\n"+"="*80); print("###",name); print("DOC:",(m.__doc__ or "")[:300])
    for attr,obj in sorted(vars(m).items()):
        if inspect.ismodule(obj) or attr.startswith("__"): continue
        if inspect.isfunction(obj):
            try: sig=str(inspect.signature(obj))
            except Exception: sig="(?)"
            print(f"  def {attr}{sig}")
            d=(inspect.getdoc(obj) or "").strip().splitlines()
            if d: print("       #",d[0][:150])
        elif inspect.isclass(obj) and obj.__module__==name:
            print(f"  class {attr}:")
            if dataclasses.is_dataclass(obj):
                for f in dataclasses.fields(obj):
                    print(f"      field {f.name}: {f.type}")
            for a2,o2 in vars(obj).items():
                if a2.startswith("_"): continue
                if inspect.isfunction(o2):
                    try: sig2=str(inspect.signature(o2))
                    except Exception: sig2="(?)"
                    print(f"      def {a2}{sig2}")
