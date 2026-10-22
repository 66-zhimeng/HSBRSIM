import json
from pathlib import Path

base = Path(r"C:\HSagent\reversed\net")


def load(asm):
    p = base / asm / "enums.json"
    return json.loads(p.read_text(encoding="utf-8-sig")) if p.exists() else {}


for asm in ["Assembly-CSharp", "HearthDb", "BobsBuddy", "blizzard.bgssdk", "blizzard.bgsclient"]:
    d = load(asm)
    print("====", asm, "enums:", len(d))
    for full, e in d.items():
        if e["name"] in {
            "GAME_TAG", "GameTag", "TAG_CARDTYPE", "CardType", "TAG_ZONE", "Zone",
            "TAG_RACE", "Race", "TAG_SPELL_SCHOOL", "SpellSchool", "TAG_PREMIUM",
            "Premium", "TAG_RARITY", "Rarity", "TAG_CARD_SET", "CardSet",
            "TAG_PLAYSTATE", "PlayState", "TAG_STEP", "Step", "TAG_TYPE", "TAG_STATE",
        }:
            print(f"  {full!r} name={e['name']!r} ns={e['namespace']!r} n={len(e['values'])}")
            print("    first:", list(e["values"].items())[:8])

# 比较 GAME_TAG 命名
ac = load("Assembly-CSharp")
hd = load("HearthDb")
g = next(e for e in ac.values() if e["name"] == "GAME_TAG")
hg = next((e for e in hd.values() if e["name"] == "GameTag"), None)
print("client GAME_TAG n=", len(g["values"]))
if hg:
    print("hearthdb GameTag n=", len(hg["values"]))
    client = g["values"]
    hdv = hg["values"]
    common = set(client) & set(hdv)
    print("name intersection:", len(common))
    mismatch = [(k, client[k], hdv[k]) for k in common if client[k] != hdv[k]]
    print("value mismatch count:", len(mismatch), mismatch[:20])
    print("client only sample:", [x for x in client if x not in hdv][:10])
    print("hearthdb only sample:", [x for x in hdv if x not in client][:10])
