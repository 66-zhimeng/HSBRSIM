import re, pathlib
from collections import Counter
base=pathlib.Path(r"reversed\net\Assembly-CSharp")
types=(base/"types.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
methods=(base/"methods.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
def count(pat, lines):
    return len([l for l in lines if re.search(pat,l,re.I)])
print("BG surface (Assembly-CSharp):")
for pat,label in [(r"Bacon|Battleground","Bacon/Battleground"),(r"\bCombat\b","Combat(word)"),
                  (r"^class\tBaconBoard|^class\tTB_BaconShop","core BG UI classes")]:
    print(f"  {label:24s} types={count(pat,types):5d} methods={count(pat,methods):6d}")
# power pipeline
for pat,label in [(r"^class\t(PowerTask|PowerTaskList|PowerProcessor|PowerQueue|PowerHistoryTimeline)","power pipeline types"),
                  (r"^Power","power methods")]:
    print(f"  {label:24s} types={count(pat,types):5d} methods={count(pat,methods):6d}")
# unity dependency among BG methods
bg_methods=[l for l in methods if re.search(r"Bacon|Battleground",l,re.I)]
unity=[l for l in bg_methods if re.search(r"UnityEngine|UIBButton|Actor|Spell|Card|Zone|PlayMaker|UberText|Material|Texture|GameObject",l)]
print(f"  BG methods referencing Unity/UI/presentation types: {len(unity)}/{len(bg_methods)}")
# combat word methods
combat_methods=[l for l in methods if re.search(r"\bCombat\b",l)]
print(f"  'Combat' methods total: {len(combat_methods)}")
print("  samples:", [m.strip()[:120] for m in combat_methods[:8]])
