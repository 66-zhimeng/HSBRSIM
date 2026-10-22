import importlib.util, json
spec=importlib.util.spec_from_file_location("dce", r"reversed\rea\scripts\damage_cap_extract.py")
m=importlib.util.module_from_spec(spec); spec.loader.exec_module(m)
res=m.parse_powerlog(m.SELF_TEST_TEXT)
print("obs:")
for o in res["observations"]: print(o)
print("enabled:", m.summarize(res)["enabled_values"])
