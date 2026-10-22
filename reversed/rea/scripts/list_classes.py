import re, pathlib
for f in [pathlib.Path(r"hsrl2\scripts\batches\batch_hero_combat3.py")]:
    s=f.read_text(encoding="utf-8").splitlines()
    for i,l in enumerate(s,1):
        if re.match(r"class \w+", l):
            print(f"{i}: {l.strip()}")
