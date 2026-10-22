import sys, inspect
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgassistant import catalog as C
print("Catalog.__init__:", inspect.signature(C.Catalog.__init__))
print("doc:", inspect.getdoc(C.Catalog.__init__))
print("class attrs:", [a for a in vars(C.Catalog) if not a.startswith('_')])
code=C.Catalog.__init__.__code__
print("names:", code.co_names); print("consts:", [c for c in code.co_consts if not hasattr(c,'co_code')])
print("\nmodule names:", [n for n in vars(C) if not n.startswith('__')])
# find table property
for a in vars(C.Catalog):
    o=getattr(C.Catalog,a)
    if isinstance(o, property): print("property:", a)
print("\nReader usage of table:")
import bgassistant.reader as Rd
code=Rd.Reader.__init__.__code__
print([n for n in code.co_names])
