import re
from pathlib import Path

p = Path(r"C:\HSagent\reversed\hsdata_255130\CardDefs.xml")
txt = p.read_text(encoding="utf-8")
print("len", len(txt))
for pat in ['CardID="BG', 'CardID="TB_Bacon', 'value="1453"', 'name="CARD_SET"',
            'name="CARDTYPE"', 'name="CARDNAME"']:
    print(pat, txt.count(pat))
p2 = Path(r"C:\HSagent\hsdata\CardDefs.xml")
t2 = p2.read_text(encoding="utf-8", errors="replace")
print("OLD len", len(t2))
for pat in ['CardID="BG', 'CardID="TB_Bacon', 'value="1453"', 'name="CARD_SET"']:
    print("OLD", pat, t2.count(pat))
print("OLD head:", t2[:300])
