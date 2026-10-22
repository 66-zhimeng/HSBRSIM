import json
d=json.load(open(r"reversed\rea\damage_cap_evidence.json",encoding="utf-8-sig"))
ui=d["official_client_evidence"]["client_ui_code"]
for k,v in ui.items():
    print(k, "=>", v["file"], "line", v["line"])
print("\nTB shop snippet:", d["official_client_evidence"]["client_applies_cap_in_lethal_check"]["file"], d["official_client_evidence"]["client_applies_cap_in_lethal_check"]["line"])
print("GameState snippet:", d["official_client_evidence"]["client_listens_to_cap_tag_changes"]["file"], d["official_client_evidence"]["client_listens_to_cap_tag_changes"]["line"])
