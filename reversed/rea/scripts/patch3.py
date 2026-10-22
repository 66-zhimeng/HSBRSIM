p=r"reversed\rea\scripts\damage_cap_extract.py"
s=open(p,encoding="utf-8").read()
s=s.replace('("enabled 事件含 0", summary["enabled_values"].get(0) == 1),',
            '("enabled 事件含 0", summary["enabled_values"].get(0, 0) >= 1),')
open(p,"w",encoding="utf-8").write(s)
