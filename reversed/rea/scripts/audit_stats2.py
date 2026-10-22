import re, hashlib, pathlib
from collections import Counter
managed=pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Hearthstone_Data\Managed")
base=pathlib.Path(r"reversed\net\Assembly-CSharp")
types=(base/"types.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
methods=(base/"methods.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
def cnt(pat, lines): return len([l for l in lines if re.search(pat,l)])
print("=== Assembly-CSharp surface ===")
print("types", len(types), "methods", len(methods))
for pat,label in [(r"Bacon|Battleground","Bacon/Battleground"),(r"Combat","Combat(substr)"),
                  (r"Simulate|Predict|Calculate|Resolve|Execute|Adjudicat","Simulate/Predict/Calculate/Resolve/Execute"),
                  (r"^class\t(PowerTask|PowerTaskList|PowerProcessor|PowerQueue)","power pipeline classes")]:
    print(f"  {label:42s} types={cnt(pat,types):5d} methods={cnt(pat,methods):6d}")
# BG types derivation share
bg=[l for l in types if re.search(r"Bacon|Battleground",l)]
mono=[l for l in bg if re.search(r"MonoBehaviour|ScriptableObject|UnityEngine\.UI|PlayMaker",l)]
print(f"  BG types deriving MonoBehaviour/ScriptableObject: {len(mono)}/{len(bg)}")
plain=[l for l in bg if not re.search(r"MonoBehaviour|ScriptableObject|UnityEngine\.UI|PlayMaker",l)]
print("  sample plain BG classes:", [l.split(chr(9))[1] for l in plain[:10]])
# hashes
for rel in ["Hearthstone_Data/Managed/Assembly-CSharp.dll",
            "Hearthstone_Data/Managed/blizzard.bgsclient.dll",
            "Hearthstone_Data/Managed/blizzard.bgssdk.dll",
            "Hearthstone_Data/Plugins/x86_64/libacsdk_x64.dll"]:
    p=pathlib.Path(r"C:\Program Files (x86)\Hearthstone")/rel
    h=hashlib.sha256(p.read_bytes()).hexdigest()
    print(f"HASH {rel}: {h[:16]}… size={p.stat().st_size}")
