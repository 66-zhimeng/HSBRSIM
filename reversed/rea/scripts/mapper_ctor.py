import sys, inspect, dis
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import mapping as Map
print("Mapper.__init__:", inspect.signature(Map.Mapper.__init__))
print("doc:", inspect.getdoc(Map.Mapper.__init__))
print("CARDS type:", type(Map.CARDS), "len:", len(Map.CARDS) if hasattr(Map.CARDS,'__len__') else '?')
try:
    k=next(iter(Map.CARDS)); print("CARDS sample:", k, Map.CARDS[k])
except Exception as e: print("CARDS sample err", e)
# reader construction
import bgassistant.reader as Rd
print("\nReader.__init__:", inspect.signature(Rd.Reader.__init__))
code=Rd.Reader.__init__.__code__
print("co_names:", code.co_names)
print("co_consts:", [c for c in code.co_consts if not hasattr(c,'co_code')])
if hasattr(Rd,'catalog'):
    print("reader module catalog attr:", Rd.catalog)
for n in ("load","load_catalog","Catalog"):
    if hasattr(Rd,n): print("reader has", n, getattr(Rd,n))
