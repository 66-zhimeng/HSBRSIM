from pathlib import Path
p=Path(r"tools\live_state_mem.py")
s=p.read_text(encoding="utf-8")
old='''    cost = d.get("cost")
    d["affordable"] = (isinstance(cost, int) and cost <= gold
                       and d.get("kind") not in ("bare", "spellcraft"))
    if buyable is not None:
        d["buyable"] = bool(buyable)
    return d'''
new='''    cost = d.get("cost")
    if buyable is not None:                    # 酒馆槽位：给“可否购买”语义
        d["affordable"] = (isinstance(cost, int) and cost <= gold)
        d["buyable"] = bool(buyable)           # 客户端 legal.buy 给出（服务器合法选项）
        d["can_buy"] = bool(d["buyable"] and d["affordable"])
    else:                                       # 手牌/其他区域
        d["affordable"] = None
        d["buyable"] = None
        d["can_buy"] = None
    return d'''
assert old in s; s=s.replace(old,new)
p.write_text(s,encoding="utf-8")
print("patched item semantics")
