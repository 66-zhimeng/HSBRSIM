import re, pathlib
base=pathlib.Path(r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted\bgcontroller")
for f in ["client/mapping.pyc","state.pyc","records.pyc","client/choices.pyc"]:
    b=(base/f).read_bytes()
    print("="*60); print(f)
    for kw in [b"recruit",b"combat",b"hero_select",b"hero",b"game_over",b"shop",b"tavern",b"phase",b"CHOICE",b"buy",b"sell",b"refresh"]:
        c=b.count(kw)
        if c: print(f"  {kw.decode():12s} {c}")
    # extract printable strings containing phase-ish words
    txt=re.findall(rb"[ -~]{4,60}", b)
    hits=[t.decode() for t in txt if re.search(rb"(recruit|combat|hero|game_over|shop|phase|tavern)", t, re.I)]
    print("  sample:", sorted(set(hits))[:40])
