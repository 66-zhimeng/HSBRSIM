import re
from pathlib import Path
# bgsclient samples
for asm in ("blizzard.bgsclient","blizzard.bgssdk"):
    base=Path(r"reversed\net")/asm
    t=(base/"types.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
    m=(base/"methods.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
    print("="*70); print(asm, "types",len(t),"methods",len(m))
    for pat,label in [(r"Battleground", "Battleground"), (r"Bacon","Bacon"),
                      (r"Combat","Combat"), (r"Simulat","Simulat"), (r"PowerHistory","PowerHistory")]:
        ts=[x for x in t if re.search(pat,x,re.I)]
        ms=[x for x in m if re.search(pat,x,re.I)]
        print(f"  {label:12s} types={len(ts):5d} methods={len(ms):5d}  sampleT={ts[:3]}")
        if label in ("Combat","Simulat","PowerHistory") and ms:
            for x in ms[:6]: print("      M:",x.strip()[:220])
