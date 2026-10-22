import marshal, types, dis, pathlib
base=pathlib.Path(r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted\bgcontroller")
def load(f):
    b=(base/f).read_bytes()
    for off in (16,12,8):
        try: return marshal.loads(b[off:])
        except Exception: pass
    raise RuntimeError("cannot load")
def walk(co, path=""):
    yield path or "<module>", co
    for i,c in enumerate(co.co_consts):
        if isinstance(c, types.CodeType):
            yield from walk(c, (path+"." if path else "")+c.co_name)
for f in ["client/mapping.pyc","state.pyc","records.pyc"]:
    print("\n"+"#"*80); print("##", f)
    code=load(f)
    for path,co in walk(code):
        names=set(co.co_names)|{x for x in co.co_consts if isinstance(x,str)}
        if any(k in names for k in ("buy","sell","refresh","phase","recruit","combat","shop","Legal","choices")):
            print("\n---", path, "| firstlineno", co.co_firstlineno)
            print("  names:", co.co_names)
            strs=[c for c in co.co_consts if isinstance(c,str)]
            print("  strs:", strs[:40])
