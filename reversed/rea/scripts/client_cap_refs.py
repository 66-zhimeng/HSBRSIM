import pathlib, re
# 1) all decompiled client files mentioning DamageCap
for f in pathlib.Path(r"reversed\net\decompiled").glob("*.cs"):
    s=f.read_text(encoding="utf-8-sig", errors="replace")
    for m in re.finditer(r".{0,110}DamageCap.{0,160}", s, re.S):
        line=s.count("\n",0,m.start())+1
        print(f"{f.name}:{line}: {re.sub(chr(10),' ',m.group(0))[:280]}")
# 2) methods.tsv entries with DamageCap
for f in [pathlib.Path(r"reversed\net\Assembly-CSharp\methods.tsv"),
          pathlib.Path(r"reversed\net\Assembly-CSharp\types.tsv")]:
    for i,line in enumerate(f.read_text(encoding="utf-8-sig",errors="replace").splitlines(),1):
        if "DamageCap" in line:
            print(f"[{f.name}:{i}] {line[:300]}")
