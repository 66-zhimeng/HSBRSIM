import json
ids = {
 "D1":"4c3c73caa189365e","D2":"aa4c75414541d87b","D3":"b2683903c4464b6f",
 "D4":"f5d1c647b2f449d1","D5":"9e80beef2ada485a","D6":"11285dbbe007f132",
}
for k,tid in ids.items():
    p = rf"runs\official_traces\{tid}.json"
    try:
        d = json.load(open(p, encoding="utf-8-sig"))
    except Exception as e:
        print(k, tid, "ERR", e); continue
    print("="*70)
    print(f"{k} trace={tid} turn={d.get('turn')} official={d.get('official_result')} lethal={d.get('official_lethal')} bb={d.get('bb_rates')}")
    print(" heroes:", d.get("heroes"))
    for side in ("player","opponent"):
        s = d[side]
        print(f" {side}: hero_power={s.get('hero_power_id')} used={s.get('hero_power_used')} data={s.get('hero_power_data')} second={s.get('second_hero_power_id')}")
        print(f"   detected={s.get('detected')} unsupported={s.get('unsupported')} missing={s.get('missing_fields')}")
        for m in s.get("minions", []):
            extra = {kk:vv for kk,vv in m.items() if kk not in ("slot","name","card_id","atk","health","golden","keywords")}
            print(f"   [{m['slot']}] {m['name']} {m['atk']}/{m['health']} kw={m.get('keywords')} golden={m.get('golden')} {extra}")
        if s.get("hand"):
            print("   hand:", [(h["name"], h["atk"], h["health"]) for h in s["hand"]])
    print(" trace missing:", d.get("missing_fields"), "notes:", d.get("notes"), "boundary:", d.get("boundary_mode"))
