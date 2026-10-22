import json
d=json.load(open(r"reversed\rea\official_implementation_audit.json",encoding="utf-8-sig"))
print("verdict:", json.dumps(d["verdict"],ensure_ascii=False))
for e in d["evidence"]:
    print(f"- {e['id']}: found={e['found']} file={e.get('file','?').split(chr(92))[-1]} line={e.get('line')} needle_lines={e.get('needle_lines')}")
print("\ncounts:", d["bg_symbol_semantics"]["counts"])
print("bgs:", {k:{kk:v[kk] for kk in ('types','methods','battleground_types','powerhistory_types','simulate','combat_methods')} for k,v in d["bgs_assemblies"].items()})
print("native:", d["native_plugins"]["libraries"])
print("assets:", d["assets"]["bundle_count"], d["assets"]["total_gb"], d["assets"]["categories"])
