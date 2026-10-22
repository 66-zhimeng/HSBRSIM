import re
p=r"reversed\rea\scripts\damage_cap_extract.py"
s=open(p,encoding="utf-8").read()
old='''    rows = {(r["turn"], r["alive_players"]): r["caps"] for r in summary["turn_alive_rows"]}
    checks = [
        ("turn1 cap=5", rows.get((1, 2)) == [5]),
        ("turn5 cap=10", rows.get((5, 2)) == [10]),
        ("turn9 cap=15", rows.get((9, 1)) == [15]),
        ("enabled 事件含 0", summary["enabled_values"].get(0) == 1),
    ]'''
new='''    rows = {(r["turn"], r["alive_players"]): r["caps"] for r in summary["turn_alive_rows"]}
    checks = [
        # create_game 初始块：cap 先于 Player 实体出现 → 存活人数未知（None）
        ("turn1 cap=5 (alive=None)", rows.get((1, None)) == [5]),
        ("turn5 cap=10 (alive=2)", rows.get((5, 2)) == [10]),
        ("turn9 cap=15 (alive=2)", rows.get((9, 2)) == [15]),
        ("turn9 cap=5 after loss (alive=1)", rows.get((9, 1)) == [5]),
        ("enabled 事件含 0", summary["enabled_values"].get(0) == 1),
    ]'''
assert old in s
s=s.replace(old,new)
open(p,"w",encoding="utf-8").write(s)
print("patched")
