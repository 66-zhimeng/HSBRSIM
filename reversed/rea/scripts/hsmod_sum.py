import json
for p in [r"reversed\rea\hsmod\artifact.json"]:
    d = json.load(open(p, encoding="utf-8-sig"))
    n = d["normalized_result"]
    print("file:", p)
    print(" sha256:", n["artifact"]["sha256"])
    print(" bytes:", n["artifact"]["byte_length"], "arch:", n["pe"]["architecture"], "machine:", n["pe"]["machine"])
    print(" asm:", n["assembly"]["name"], n["assembly"]["version"])
    print(" refs:", len(n["references"]), "metadata:", n["metadata"]["status"], "coverage:", n["coverage"])
    print(" provider:", d["provider"]["id"], d["provider"]["version"])
