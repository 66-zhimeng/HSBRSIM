import json, glob, os
# 1) JSON validity + key facts
files = [r"reversed\rea\client_constants.json",
         r"reversed\rea\bobsbuddy\rea_artifact.json",
         r"reversed\rea\bobsbuddy\rea_simulator_methods.json",
         r"reversed\rea\client_official\rea_summary.json",
         r"reversed\rea\hsmod\artifact.json",
         r"reversed\rea\bobsbuddy\rea_script_test\rea_summary.json"]
for f in files:
    try:
        d = json.load(open(f, encoding="utf-8-sig"))
        print("OK  ", f, os.path.getsize(f), "bytes")
    except Exception as e:
        print("FAIL", f, e)

# 2) BobsBuddy REA artifact facts
d = json.load(open(r"reversed\rea\bobsbuddy\rea_artifact.json", encoding="utf-8-sig"))
n = d["normalized_result"]
print("\nBobsBuddy artifact:")
print(" sha256:", n["artifact"]["sha256"])
print(" refs:", [f"{r.get('name')} {r.get('version')}" for r in n["references"]])
print(" resources:", [(r.get("name"), r.get("size")) for r in (n.get("resources") or [])])
print(" module:", n.get("module", {}).get("name"))
print(" provider:", d["provider"])
