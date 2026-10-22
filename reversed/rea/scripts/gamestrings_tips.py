import re, pathlib
base = pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Strings\zhCN")
g = (base/"GLOBAL.txt").read_text(encoding="utf-8-sig", errors="replace").splitlines()
print("=== GLOBAL_KEYWORD keys (names only) ===")
keys=[l.split("\t")[0] for l in g if l.startswith("GLOBAL_KEYWORD_") and l.split("\t")[0].endswith(("_TEXT","")) ]
names=[k for k in keys if not k.endswith("_TEXT")]
print(", ".join(names))
print("\n=== 相邻/顺劈/复生 tips in GLUE ===")
gl = (base/"GLUE.txt").read_text(encoding="utf-8-sig", errors="replace").splitlines()
for i,l in enumerate(gl):
    k,_,v=l.partition("\t")
    if k.startswith("GLUE_TIP_BACON") or "相邻" in v or "顺劈" in v:
        print(f"{i+1}: {k} = {v[:260]}")
