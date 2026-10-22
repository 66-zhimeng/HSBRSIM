p=r"reversed\rea\scripts\damage_cap_extract.py"
s=open(p,encoding="utf-8").read()
old='''        if tag == CAP_TAG:
            cap = value
        else:
            enabled = value
        alive = None
        states = [s for s in players.values() if s in (0, 1, 2, 3, 4, 5)]
        if states:
            alive = sum(1 for s in states if s == PLAYSTATE_PLAYING)
        if cap is not None:
            observations.append({
                "line": lineno,
                "turn": turn,
                "alive_players": alive,
                "cap": cap,
                "enabled": enabled,
                "event": "tag_change" if is_change else "initial",
                "entity": entity,
            })
            if turn is not None and alive is not None:
                by_turn_alive[(turn, alive)].add(cap)'''
new='''        if tag == CAP_TAG:
            cap = value
        else:
            enabled = value
        alive = None
        states = [s for s in players.values() if s in (0, 1, 2, 3, 4, 5)]
        if states:
            alive = sum(1 for s in states if s == PLAYSTATE_PLAYING)
        if cap is not None:
            observations.append({
                "line": lineno,
                "turn": turn,
                "alive_players": alive,
                "cap": cap,
                "enabled": enabled,
                "event": "tag_change" if is_change else "initial",
                "entity": entity,
            })
            # 观察表只收 cap 事件（enabled 变化单独留在 observations，避免重复行）
            if tag == CAP_TAG and turn is not None:
                by_turn_alive[(turn, alive)].add(cap)'''
assert old in s
s=s.replace(old,new)
old2='''        ("turn9 cap=5 after loss (alive=1)", rows.get((9, 1)) == [5]),'''
new2='''        ("turn9 cap=5 after loss (alive=1)", rows.get((9, 1)) == [5]),'''
s=s.replace(old2,new2)
open(p,"w",encoding="utf-8").write(s)
print("patched2")
