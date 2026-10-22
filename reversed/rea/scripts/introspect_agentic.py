import sys, inspect
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
mods = ["bgcontroller.client.mono","bgcontroller.client.memory","bgcontroller.state","bgcontroller.records",
        "bgcontroller.client.tags","bgcontroller.client.choices","bgcontroller.client.leaderboard",
        "bgcontroller.client.history"]
import importlib
for name in mods:
    try:
        m = importlib.import_module(name)
    except Exception as e:
        print(f"\n### {name}: IMPORT FAIL {e!r}"); continue
    print("\n" + "="*78); print("###", name, "|", (m.__doc__ or "").strip().splitlines()[:1])
    for attr, obj in sorted(vars(m).items()):
        if attr.startswith("_") or inspect.ismodule(obj): continue
        if inspect.isfunction(obj):
            try: sig = str(inspect.signature(obj))
            except Exception: sig = "(?)"
            print(f"  def {attr}{sig}")
            d = (inspect.getdoc(obj) or "").strip().splitlines()
            if d: print("      #", d[0][:160])
        elif inspect.isclass(obj):
            print(f"  class {attr}:")
            for a2, o2 in vars(obj).items():
                if a2.startswith("_"): continue
                if inspect.isfunction(o2):
                    try: sig2 = str(inspect.signature(o2))
                    except Exception: sig2 = "(?)"
                    print(f"      def {a2}{sig2}")
