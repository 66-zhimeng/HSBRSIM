from pathlib import Path
p=Path(r"tools\live_state_mem.py")
s=p.read_text(encoding="utf-8")
s=s.replace('''            cls_pair = getattr(reader, "_game_state_class", None)
            if cls_pair:''','''            cls_pair = getattr(reader, "_game_state_class", None)
            diag["game_state_class_found"] = cls_pair is not None
            if cls_pair:''')
p.write_text(s,encoding="utf-8"); print("patched flag")
