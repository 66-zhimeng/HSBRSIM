from pathlib import Path
p=Path(r"tools\live_state_mem.py")
s=p.read_text(encoding="utf-8")

# add LOGS_ROOT constant
s=s.replace('''PKG_DIR = EXTRACTED / "PYZ.pyz_extracted"''',
'''PKG_DIR = EXTRACTED / "PYZ.pyz_extracted"
LOGS_ROOT = Path(r"C:\\Program Files (x86)\\Hearthstone\\Logs")''')

# insert powerlog fallback after read_live_state function (before hotkeys section)
anchor='''# ───────────────────────────── 热键暂停 ─────────────────────────────'''
powerlog_fn='''def read_state_from_powerlog(path: Optional[Path] = None,
                             want_phase: str = "recruit",
                             stride: int = 2000,
                             include_state: bool = False,
                             max_files: int = 5) -> Dict[str, Any]:
    """fallback：从官方 Power.log 重建某一帧（`LogGame.snapshot` → `Mapper.frame`）。

    内存读取不可用/无活动对局时的等价通道；Power.log 是服务器下发的 PowerHistory，
    与内存快照走同一个 `Mapper`，因此字段 schema 完全一致（但**不是实时**）。
    """
    mods = _load_modules()
    powerlog, mapper = mods["powerlog"], _mapper(mods)
    if path is not None:
        files = [Path(path)]
    else:
        files = sorted(LOGS_ROOT.rglob("Power.log"),
                       key=lambda p: p.stat().st_mtime, reverse=True)[:max_files]
    if not files:
        return {"schema": SCHEMA, "phase": "unavailable", "attached": False,
                "source": "powerlog", "error": "未找到 Power.log（先启动游戏并开启日志）"}
    errors: List[str] = []
    for log_path in files:
        if not log_path.exists():
            continue
        game = powerlog.LogGame()
        best = None
        line_no = 0
        try:
            with log_path.open("r", encoding="utf-8", errors="replace") as fh:
                for line_no, line in enumerate(fh, 1):
                    game.feed(line)
                    if line_no % stride:
                        continue
                    snap = game.snapshot()
                    if snap.get("game_over"):
                        continue
                    try:
                        frame = mapper.frame(snap)
                    except Exception:  # noqa: BLE001 - 过渡态快照跳过
                        continue
                    if frame is None:
                        continue
                    st = frame.state
                    if want_phase not in ("any", str(getattr(st, "phase", ""))):
                        continue
                    if want_phase == "recruit" and not list(getattr(st, "shop", []) or []):
                        continue
                    best = (frame, snap, line_no)
            if best is None:
                errors.append(f"{log_path}: 无匹配 {want_phase} 帧")
                continue
            frame, snap, line_no = best
            info = {"source": "powerlog", "log_file": str(log_path),
                    "log_line": line_no, "wanted_phase": want_phase}
            out = frame_to_dict(frame, snap, info, include_state=include_state,
                                source="powerlog.LogGame.snapshot + mapping.Mapper.frame")
            out["attached"] = False
            out["log_file"] = str(log_path)
            out["log_line"] = line_no
            return out
        except Exception as exc:  # noqa: BLE001
            errors.append(f"{log_path}: {type(exc).__name__}: {exc}")
    return {"schema": SCHEMA, "phase": "no_powerlog_frame", "attached": False,
            "source": "powerlog", "errors": errors}


''' + anchor
assert anchor in s
s=s.replace(anchor, powerlog_fn, 1)

# docstring usage
s=s.replace('''  # 自测（不读游戏）
  C:\\\\Python313\\\\python.exe tools\\\\live_state_mem.py --self-test''',
'''  # fallback：从官方 Power.log 重建一帧（无活动对局/内存不可用时）
  C:\\\\Python313\\\\python.exe tools\\\\live_state_mem.py --from-powerlog ^
      --sample runs\\\\live\\\\state_from_powerlog_sample.json
  # 自测（不读游戏）
  C:\\\\Python313\\\\python.exe tools\\\\live_state_mem.py --self-test''')

# CLI args
s=s.replace('''    ap.add_argument("--attach-test", action="store_true")''',
'''    ap.add_argument("--attach-test", action="store_true")
    ap.add_argument("--from-powerlog", nargs="?", const="auto", default=None,
                    metavar="PATH", help="从 Power.log 重建一帧（fallback）")
    ap.add_argument("--powerlog-phase", default="recruit",
                    choices=("recruit", "combat", "any"))
    ap.add_argument("--powerlog-stride", type=int, default=2000)''')

# CLI branch before watch
s=s.replace('''    if args.watch:
        return watch(args.pid, args.interval, args.out, args.jsonl,''',
'''    if args.from_powerlog:
        path = None if args.from_powerlog == "auto" else Path(args.from_powerlog)
        state = read_state_from_powerlog(path, want_phase=args.powerlog_phase,
                                         stride=args.powerlog_stride,
                                         include_state=args.include_raw)
        if args.sample:
            _write_json(args.sample, state)
            print(f"[sample] written {args.sample}")
        print(json.dumps(state, ensure_ascii=False, indent=1))
        return 0 if state.get("phase") not in ("no_powerlog_frame", "unavailable") else 1
    if args.watch:
        return watch(args.pid, args.interval, args.out, args.jsonl,''')

p.write_text(s, encoding="utf-8")
print("patched powerlog fallback")
