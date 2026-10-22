import json
d=json.load(open(r"reversed\rea\official_implementation_audit.json",encoding="utf-8-sig"))
print("mono:", d["bg_symbol_semantics"]["counts"].get("bacon_battleground_monobehaviour_types"))
print("bobsbuddy:", json.dumps(d["bobsbuddy_inventory"],ensure_ascii=False,indent=1))
print("dotnet:", d["environment"]["dotnet"], "| ilspy:", d["environment"]["ilspycmd"])
print("verdict:", d["verdict"])
