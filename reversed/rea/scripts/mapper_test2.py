import sys, inspect
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import mapping as Map
from bgassistant import catalog as C
for fn_name in ("__init__","card","from_dbf"):
    fn=getattr(Map.Mapper, fn_name, None)
    if fn is None: continue
    print("\nMapper."+fn_name, inspect.signature(fn))
    co=fn.__code__
    print(" names:", co.co_names)
    print(" consts:", [c for c in co.co_consts if not hasattr(c,'co_code')])
print("\n=== try Catalog ===")
try:
    cat=C.Catalog()
    print("Catalog() OK; attrs:", [a for a in vars(cat) if not a.startswith('_')])
    print("version:", cat.version, "cards:", len(cat.by_id))
    print("get BG20_HERO_100:", cat.get("BG20_HERO_100"))
except Exception as e:
    print("Catalog() FAIL:", repr(e))
print("\n=== try Mapper ===")
for tbl_desc, tbl in [("Catalog", lambda: C.Catalog()), ("CARDS", lambda: Map.CARDS)]:
    try:
        t=tbl(); m=Map.Mapper(t); print(tbl_desc, "Mapper OK", type(m), [a for a in vars(m) if not a.startswith('_')])
    except Exception as e:
        print(tbl_desc, "Mapper FAIL:", type(e).__name__, e)
