import re
p=r"hsdata\CardDefs.xml"
s=open(p,encoding="utf-8").read()
# find entities whose enUS text contains "Also damages the minions next to"
pat=re.compile(r'<Entity CardID="([^"]+)"[^>]*>(.*?)</Entity>', re.S)
n=0
for m in pat.finditer(s):
    body=m.group(2)
    t=re.search(r'name="CARDTEXT".*?<enUS>(.*?)</enUS>', body, re.S)
    if t and "Also damages the minions next to" in t.group(1):
        line=s.count("\n",0,m.start())+1
        name=re.search(r'name="CARDNAME".*?<enUS>(.*?)</enUS>', body, re.S)
        print(f"{m.group(1)} | line {line} | {name.group(1) if name else '?'} | {t.group(1)[:160]}")
        n+=1
        if n>=5: break
