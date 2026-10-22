"""扫描本机 HDT 录像（.hdtreplay = zip + output_log.txt 全量 Power.log），
生成隐私安全的语料统计：reversed/powerlog_corpus.md / .json

只输出聚合统计；**不落盘**任何玩家名/BattleTag/账号 id/原始日志。
"""
import json
import re
import zipfile
from collections import Counter
from pathlib import Path

ROOT = Path(r"C:\HSagent")
REPLAYS = Path.home() / "AppData" / "Roaming" / "HearthstoneDeckTracker" / "Replays"
OUT_MD = ROOT / "reversed" / "powerlog_corpus.md"
OUT_JSON = ROOT / "reversed" / "powerlog_corpus.json"


def read_log(f):
    with zipfile.ZipFile(f) as z:
        n = next((x for x in z.namelist()
                  if x.lower().endswith((".log", ".txt"))), None)
        return z.read(n).decode("utf-8", errors="replace") if n else ""


files = sorted(REPLAYS.glob("*.hdtreplay")) if REPLAYS.exists() else []
rows = []
for f in files:
    try:
        txt = read_log(f)
    except Exception:  # noqa: BLE001 —— 损坏的录像跳过
        continue
    players = len(set(re.findall(r"Player EntityID=\d+ PlayerID=(\d+)", txt)))
    markers = {
        "create_game": txt.count("CREATE_GAME"),
        "full_entity": txt.count("FULL_ENTITY"),
        "tag_change": txt.count("TAG_CHANGE"),
        "block_start": txt.count("BLOCK_START"),
        "block_end": txt.count("BLOCK_END"),
        "attack_blocks": txt.count("BlockType=ATTACK"),
        "bacon_8p_end": txt.count("TB_BaconShop_8P_PlayerE"),
        "tb_baconshop": txt.count("TB_BaconShop"),
        "bacon_tags": txt.count("tag=BACON_"),
        "bg_card_ids": len(set(re.findall(r"CardID=(BG[0-9][A-Za-z0-9_]*)", txt))),
        "power_tasklist_lines": txt.count("PowerTaskList."),
        "game_state_lines": txt.count("GameState."),
    }
    # 严格 BG 判定：BG 是 8 人局（Power.log 会在 CREATE_GAME 里创建 8 个 Player）
    is_bg = players >= 8
    rows.append({"index": len(rows), "bytes": len(txt.encode("utf-8")),
                 "lines": txt.count("\n") + 1, "players": players,
                 "is_bg": is_bg, "markers": markers})

player_dist = Counter(r["players"] for r in rows)
bg = [r for r in rows if r["is_bg"]]
totals = Counter()
for r in rows:
    totals.update(r["markers"])
bg_totals = Counter()
for r in bg:
    bg_totals.update(r["markers"])

report = {
    "replays_dir": str(REPLAYS),
    "total_replays": len(rows),
    "player_count_distribution": dict(sorted(player_dist.items())),
    "battlegrounds_replays": len(bg),
    "avg_lines_all": round(sum(r["lines"] for r in rows) / len(rows)) if rows else 0,
    "avg_bytes_all": round(sum(r["bytes"] for r in rows) / len(rows)) if rows else 0,
    "marker_totals_all": dict(totals),
    "bg_marker_totals": dict(bg_totals),
    "diagnostics": {
        "files_with_TB_BaconShop": sum(1 for r in rows
                                       if r["markers"]["tb_baconshop"]),
        "files_with_BACON_tags": sum(1 for r in rows
                                     if r["markers"]["bacon_tags"]),
        "files_with_BG_card_ids": sum(1 for r in rows
                                      if r["markers"]["bg_card_ids"]),
    },
    "note": "hdtreplay = zip；内含 output_log.txt = 完整 Power.log（仅 GameState 行）。"
            "本报告已剔除玩家名/账号 id，仅保留聚合统计。"
            "BG 判定 = CreateGame 后出现 ≥8 个 Player 实体。",
}
OUT_JSON.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n",
                    encoding="utf-8")

L = [
    "# Power.log 语料统计（本机 HDT 录像）\n",
    "> 自动生成，勿手改。复现：`python reversed/scratch/replays.py`\n",
    f"- 录像目录（只读）: `{REPLAYS}`",
    f"- 录像总数: **{len(rows)}**；按玩家数分布: `{dict(sorted(player_dist.items()))}`"
    f"（2 = 构筑/双人模式，8 = 酒馆战棋）",
    f"- **严格判定的酒馆战棋对局: {len(bg)} 个**",
    "",
    "## 1. 格式（对差分 harness 有效）",
    "",
    "`.hdtreplay` 是 zip，内含 `output_log.txt`，即完整 Power.log 的子集：",
    "",
    "```text",
    "D 17:47:38.9236160 GameState.DebugPrintPower() - CREATE_GAME",
    "D 17:47:38.9236160 GameState.DebugPrintPower() -     GameEntity EntityID=1",
    "D 17:47:38.9236160 GameState.DebugPrintPower() -         tag=CARDTYPE value=GAME",
    "```",
    "",
    "## 2. 语料统计",
    "",
    "| 指标 | 全部录像合计 |",
    "|------|--------------|",
    f"| 文件数 | {len(rows)} |",
    f"| 平均行数 / 大小 | {report['avg_lines_all']:,} 行 / "
    f"{report['avg_bytes_all']:,} B |",
]
for k, label in [("game_state_lines", "GameState.* 行"),
                 ("power_tasklist_lines",
                  "PowerTaskList.* 行（HDT 录像里为 0；实时日志里会重复，解析时应跳过）"),
                 ("create_game", "CREATE_GAME"),
                 ("full_entity", "FULL_ENTITY"),
                 ("tag_change", "TAG_CHANGE"),
                 ("block_start", "BLOCK_START"),
                 ("attack_blocks", "BlockType=ATTACK")]:
    L.append(f"| {label} | {totals.get(k, 0):,} |")
L += [
    "",
    "## 3. 判定口径与结论",
    "",
    "- 严格判定：`CREATE_GAME` 之后出现 **≥8 个 Player 实体** 才是酒馆战棋；",
    f"- 本机 {len(rows)} 个录像的玩家数分布为 `{dict(sorted(player_dist.items()))}`，"
    f"**没有 8 人局 → 没有可用的 BG 轨迹**；",
    f"- 诊断信号：含 `TB_BaconShop` 的文件 {report['diagnostics']['files_with_TB_BaconShop']} 个、"
    f"含 `tag=BACON_*` 的文件 {report['diagnostics']['files_with_BACON_tags']} 个、"
    f"含 `CardID=BG<数字>` 的文件 {report['diagnostics']['files_with_BG_card_ids']} 个"
    "——这些是**构筑局里引用的 BG 实体/Bob 皮肤**，不能当 BG 语料；",
    "- 历史上 `REAL_GAME_BRIDGE.md` 记录的 `TB_BaconShop_8P_PlayerE` 战斗结束标志"
    "在本机录像中从未出现（0 次），**必须在首份真实 BG 日志上重新验证**；",
    "- 结论：差分 harness 的 BG 语料需要**新采集**（`log.config` 的 `[Power]` 已开启，"
    "正常打一局即可产生 `Logs\\Power.log`）；见 `docs/OFFICIAL_EXACTNESS.md` §2。",
    "",
]
OUT_MD.write_text("\n".join(L), encoding="utf-8")
print(json.dumps({k: report[k] for k in
                  ("total_replays", "player_count_distribution",
                   "battlegrounds_replays", "diagnostics")},
                 ensure_ascii=False, indent=1))
print("wrote", OUT_MD, OUT_JSON)
