import pathlib, re
roots=[pathlib.Path(r"reversed\net"), pathlib.Path(r"reversed\rea\client")]
pat=re.compile(r"damage.?cap", re.I)
for root in roots:
    for f in root.rglob("*"):
        if f.suffix.lower() not in (".cs",".json",".tsv",".txt",".md"): continue
        try: s=f.read_text(encoding="utf-8-sig", errors="replace")
        except Exception: continue
        for m in pat.finditer(s):
            line = s.count("\n",0,m.start())+1
            seg = s[max(0,m.start()-120):m.start()+160].replace("\n"," ")
            print(f"{f}:{line}: ...{seg}...")
            break
