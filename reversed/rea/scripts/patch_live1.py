from pathlib import Path
p=Path(r"tools\live_state_mem.py")
s=p.read_text(encoding="utf-8")

# 1) copy both data dirs
old='''def _ensure_data_dir() -> None:
    """bgcontroller 运行期需要 data/game_tags.json（在安装目录 _internal 中）。"""
    dst = PKG_DIR / "bgcontroller" / "data"
    src = AGENTIC_INTERNAL / "bgcontroller" / "data"
    if dst.exists() or not src.exists():
        return
    import shutil
    dst.parent.mkdir(parents=True, exist_ok=True)
    shutil.copytree(src, dst)'''
new='''def _ensure_data_dir() -> None:
    """运行期需要两份数据：bgcontroller/data（tag 枚举）与 bgassistant/data（卡牌目录）。"""
    import shutil
    for rel in ("bgcontroller/data", "bgassistant/data"):
        dst = PKG_DIR / rel.replace("/", os.sep)
        src = AGENTIC_INTERNAL / rel.replace("/", os.sep)
        if dst.exists() or not src.exists():
            continue
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copytree(src, dst)'''
assert old in s; s=s.replace(old,new)

# 2) load catalog module
old='''    from bgcontroller.client import memory as memory_mod  # type: ignore
    from bgcontroller.client import mapping as mapping_mod  # type: ignore
    from bgcontroller import state as state_mod  # type: ignore
    from bgcontroller import records as records_mod  # type: ignore
    _SESSION.update(memory=memory_mod, mapping=mapping_mod,
                    state=state_mod, records=records_mod, pkg=str(pkg))
    return _SESSION'''
new='''    from bgcontroller.client import memory as memory_mod  # type: ignore
    from bgcontroller.client import mapping as mapping_mod  # type: ignore
    from bgcontroller import state as state_mod  # type: ignore
    from bgcontroller import records as records_mod  # type: ignore
    from bgassistant import catalog as catalog_mod  # type: ignore
    from bgcontroller.client import powerlog as powerlog_mod  # type: ignore
    _SESSION.update(memory=memory_mod, mapping=mapping_mod,
                    state=state_mod, records=records_mod,
                    catalog=catalog_mod, powerlog=powerlog_mod, pkg=str(pkg))
    return _SESSION


def _mapper(mods: Optional[Dict[str, Any]] = None):
    """`Mapper` 需要卡牌目录表（Catalog），构造一次复用。"""
    mods = mods or _load_modules()
    if "mapper" not in _SESSION:
        table = mods["catalog"].Catalog()          # bgassistant/data/catalog.json
        _SESSION["mapper"] = mods["mapping"].Mapper(table)
    return _SESSION["mapper"]'''
assert old in s; s=s.replace(old,new)

# 3) frame_to_dict source param
old='''def frame_to_dict(frame: Any, snap: Optional[Dict[str, Any]] = None,
                  info: Optional[Dict[str, Any]] = None,
                  include_state: bool = False) -> Dict[str, Any]:'''
new='''def frame_to_dict(frame: Any, snap: Optional[Dict[str, Any]] = None,
                  info: Optional[Dict[str, Any]] = None,
                  include_state: bool = False,
                  source: str = "agenticassistant.memory.capture_snapshot "
                                "+ mapping.Mapper.frame") -> Dict[str, Any]:'''
assert old in s; s=s.replace(old,new)
s=s.replace('''        "source": "agenticassistant.memory.capture_snapshot + mapping.Mapper.frame",
        "read_only": True,''','''        "source": source,
        "read_only": True,''')

# 4) read_live_state use proper mapper
s=s.replace('''        frame = mapping.Mapper().frame(snap)''','''        frame = _mapper(mods).frame(snap)''')
p.write_text(s, encoding="utf-8")
print("patched core")
