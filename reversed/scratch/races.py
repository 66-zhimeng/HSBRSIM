import json
from collections import Counter
from pathlib import Path

for label, d in [("255130", Path(r"C:\HSagent\reversed\data_255130")),
                 ("current", Path(r"C:\HSagent\data"))]:
    cards = json.loads((d / "bg_pool_minions.json").read_text(encoding="utf-8"))
    races = Counter(c.get("card_race", 0) for c in cards)
    print("====", label, "pool minions", len(cards))
    print(" card_race:", dict(sorted(races.items())))
    unknown = [c["id"] for c in cards if c.get("card_race", 0) not in
               (0, 11, 14, 15, 17, 18, 20, 23, 24, 26, 43, 92)]
    print(" unknown race count:", len(unknown), unknown[:40])
    print(" race_tags count:", sum(1 for c in cards if c.get("race_tags")))
    # 池内随从中 id 前缀 BG36 等
    pref = Counter(c["id"].split("_")[0] for c in cards)
    print(" prefixes:", pref.most_common(12))
