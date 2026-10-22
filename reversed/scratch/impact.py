"""统计 255130 数据变更会命中引擎/测试的哪些位置（升级影响面）。"""
import json
import re
from collections import Counter
from pathlib import Path

ROOT = Path(r"C:\HSagent")
d = json.loads((ROOT / "reversed" / "data_diff_255130_vs_249896.json").read_text(encoding="utf-8"))

changed_ids = set()
for k, items in d["value_changes"].items():
    for x in items:
        changed_ids.add(x["id"])
added = set(d["bg_added"])
removed = set(d["bg_removed"])

# 分类：本体 / 金色 / 附魔 / 其他
def kind(cid):
    if cid.endswith("_G"):
        return "golden"
    if cid.endswith("e") or re.search(r"e\d*$", cid):
        return "enchantment"
    return "base"

kinds = Counter(kind(c) for c in changed_ids)
print("changed ids:", len(changed_ids), dict(kinds))

new_pool = json.loads((ROOT / "reversed" / "data_255130" / "bg_pool_minions.json").read_text(encoding="utf-8"))
new_ids = {c["id"] for c in new_pool}
added_pool = sorted(added & new_ids)
print("added pool minions:", len(added_pool), added_pool[:10])

patterns = {"added": sorted(added), "removed": sorted(removed),
            "value_changed": sorted(changed_ids),
            "added_pool": added_pool}
rx = {k: re.compile("|".join(re.escape(i) for i in v)) if v else None
      for k, v in patterns.items()}

hits = {k: [] for k in patterns}
for sub in ("hsrl2", "tests"):
    for p in (ROOT / sub).rglob("*.py"):
        text = p.read_text(encoding="utf-8", errors="replace")
        for k, r in rx.items():
            if r is None:
                continue
            ids = sorted(set(r.findall(text)))
            if ids:
                hits[k].append({"file": str(p.relative_to(ROOT)),
                                "n_ids": len(ids), "ids": ids[:25]})

test_files = {k: sorted({h["file"] for h in v
                         if h["file"].replace("\\", "/").startswith(
                             ("hsrl2/tests", "tests/"))})
              for k, v in hits.items()}
result = {
    "counts": {k: len(v) for k, v in patterns.items()},
    "changed_kinds": dict(kinds),
    "added_pool_minions": added_pool,
    "file_hit_counts": {k: len(v) for k, v in hits.items()},
    "test_file_counts": {k: len(v) for k, v in test_files.items()},
    "test_files": test_files,
    "hits": hits,
}
(ROOT / "reversed" / "engine_impact_255130.json").write_text(
    json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
print("file_hits:", result["file_hit_counts"])
print("test_files:", result["test_file_counts"])
for k in patterns:
    if hits[k]:
        print(f"--- {k} ---")
        for h in hits[k][:10]:
            print(" ", h["file"], h["n_ids"])
