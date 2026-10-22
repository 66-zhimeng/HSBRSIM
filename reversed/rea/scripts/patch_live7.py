from pathlib import Path
p=Path(r"tools\live_state_mem.py")
s=p.read_text(encoding="utf-8")
old='''        game = powerlog.LogGame()
        best = None
        line_no = 0'''
new='''        game = powerlog.LogGame()
        best = None
        last_any = None
        line_no = 0'''
assert old in s; s=s.replace(old,new)
old2='''                    best = (frame, snap, line_no)
            if best is None:'''
new2='''                    last_any = (frame, snap, line_no)
                    if int(getattr(st, "gold", 0) or 0) > 0:
                        best = last_any
            if best is None:
                best = last_any
            if best is None:'''
assert old2 in s; s=s.replace(old2,new2)
p.write_text(s,encoding="utf-8")
print("patched selection")
