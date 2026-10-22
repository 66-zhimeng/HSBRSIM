import re
from pathlib import Path
base=Path(r"reversed\net\Assembly-CSharp")
types=(base/"types.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
methods=(base/"methods.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
for pat,label in [(r"\bSimulate","Simulate"),(r"\bPredict","Predict"),(r"\bCalculate","Calculate")]:
    ms=[l for l in methods if re.search(pat,l)]
    ts=[l for l in types if re.search(pat,l)]
    print("="*64); print(label, "methods:", len(ms), "types:", len(ts))
    for x in ms[:14]: print("  M:", x.strip()[:200])
    for x in ts[:6]: print("  T:", x.strip()[:200])
