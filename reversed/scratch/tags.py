import re
from pathlib import Path

p = Path(r"C:\HSagent\reversed\hsdata_255130\CardDefs.Bacon.xml")
txt = p.read_text(encoding="utf-8")
want = {230, 1508, 3880, 4744, 4756, 4757, 4916, 4921, 4922, 4929,
        4933, 4934, 4935, 4940, 4946, 4953, 4955}
found = {}
for m in re.finditer(r'<Tag enumID="(\d+)" name="([^"]*)"', txt):
    i = int(m.group(1))
    if i in want and i not in found:
        found[i] = m.group(2)
print("new tag names:", dict(sorted(found.items())))
for i in [2522, 2523, 2546, 2553, 4756, 4757]:
    hits = set(re.findall(rf'<Tag enumID="{i}" name="([^"]*)"', txt))
    print(i, hits)
