import re, pathlib
base = pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Strings\zhCN")
for name in ("GLUE.txt","GLOBAL.txt","GAMEPLAY.txt"):
    f=base/name
    lines=f.read_text(encoding="utf-8-sig", errors="replace").splitlines()
    print("="*80); print(name, "lines:", len(lines))
    for i,l in enumerate(lines):
        key,_,val = l.partition("\t")
        if re.search(r"上限|复生|剧毒|烈毒|顺劈", val) and ("BACON" in key or "TIP" in key or "KEYWORD" in key):
            print(f"{i+1}: {key} = {val[:500]}")
