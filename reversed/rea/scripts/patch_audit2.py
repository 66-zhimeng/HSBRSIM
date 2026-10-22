p=r"reversed\rea\scripts\audit_official_impl.py"
s=open(p,encoding="utf-8").read()
# 1) dynamic MonoBehaviour share
old='''        "counts": {
            "types_total": len(types),
            "methods_total": len(methods),
            "bacon_battleground_types": len(match(types, r"Bacon|Battleground")),
            "bacon_battleground_methods": len(match(methods, r"Bacon|Battleground")),'''
new='''        "counts": {
            "types_total": len(types),
            "methods_total": len(methods),
            "bacon_battleground_types": len(match(types, r"Bacon|Battleground")),
            "bacon_battleground_methods": len(match(methods, r"Bacon|Battleground")),
            "bacon_battleground_monobehaviour_types": len(
                [ln for ln in match(types, r"Bacon|Battleground")
                 if re.search(r"MonoBehaviour|ScriptableObject|UnityEngine\\.UI|PlayMaker", ln)]),'''
assert old in s; s=s.replace(old,new)
# 2) migration uses dynamic count
s=s.replace('"bg_monobehaviour_types": 53,',
            '"bg_monobehaviour_types": symbols["counts"].get("bacon_battleground_monobehaviour_types"),')
# 3) BobsBuddy inventory for route B
old2='''def migration_assessment(scan, symbols, bgs, native, assets, evidence):'''
new2='''def bobsbuddy_inventory():
    rows = []
    for name in ("BobsBuddy.dll", "BobsBuddy.Common.dll", "HearthDb.dll"):
        p = HDT_APP / name
        rows.append({"name": name, "exists": p.exists(),
                     "bytes": p.stat().st_size if p.exists() else None,
                     "sha256_16": sha256_16(p) if p.exists() else None})
    return {"hdt_app": str(HDT_APP), "assemblies": rows,
            "note": "BobsBuddy.dll 为第三方完整战斗实现（非官方），可作为 .NET 桥接/第二实现参照"}


def migration_assessment(scan, symbols, bgs, native, assets, evidence):'''
assert old2 in s; s=s.replace(old2,new2)
# 4) include inventory in result
old3='''        "native_plugins": native,'''
new3='''        "native_plugins": native,
        "bobsbuddy_inventory": bobsbuddy_inventory(),'''
assert old3 in s; s=s.replace(old3,new3)
open(p,"w",encoding="utf-8").write(s)
print("patched audit script")
