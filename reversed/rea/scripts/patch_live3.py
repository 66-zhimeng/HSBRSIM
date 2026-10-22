from pathlib import Path
p=Path(r"tools\live_state_mem.py")
s=p.read_text(encoding="utf-8")
old='''    if snap.get("game_over") or getattr(state, "alive", True) is False:
        return "game_over", raw
    if legal.get("end"):
        return "game_over", raw
    if raw in ("recruit", "combat"):'''
new='''    if snap.get("game_over") or getattr(state, "alive", True) is False:
        return "game_over", raw
    # 注意：Legal.end = “可以结束回合”按钮，不是“对局结束”，不能用来判 game_over。
    if raw in ("recruit", "combat"):'''
assert old in s; s=s.replace(old,new)
p.write_text(s,encoding="utf-8")
print("patched phase")
