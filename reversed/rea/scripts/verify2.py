import json
d = json.load(open(r"reversed\rea\bobsbuddy\rea_artifact.json", encoding="utf-8-sig"))
n = d["normalized_result"]
print("arch:", n["pe"]["architecture"], "machine:", n["pe"]["machine"])
print("metadata status:", n["metadata"]["status"], "tables:", len(n["metadata"].get("table_row_counts") or {}))
print("coverage:", n["coverage"])
print("assembly:", n["assembly"]["name"], n["assembly"]["version"])
# Resources details
print("resources:", json.dumps(n.get("resources"))[:300])
