import json
import re
from collections import Counter
from pathlib import Path

from lxml import etree

for label, p in [("255130", r"C:\HSagent\reversed\hsdata_255130\CardDefs.xml"),
                 ("249896", r"C:\HSagent\hsdata\CardDefs.xml")]:
    sets = Counter()
    prefixes = Counter()
    n = 0
    for _e, elem in etree.iterparse(p, events=("end",), tag="Entity"):
        cid = elem.get("CardID")
        if cid:
            n += 1
            cs = None
            for t in elem.findall("Tag"):
                if t.get("enumID") == "183":
                    cs = int(t.get("value", "0"))
                    break
            sets[cs] += 1
            prefixes[re.sub(r"[0-9].*$", "", cid)[:12]] += 1
        elem.clear()
        while elem.getprevious() is not None:
            del elem.getparent()[0]
    print("====", label, "entities", n)
    print(" top sets:", sets.most_common(15))
    print(" BG(1453):", sets.get(1453))
    print(" prefixes:", prefixes.most_common(20))
