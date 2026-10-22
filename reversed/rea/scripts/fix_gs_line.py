p=r"reversed\rea\damage_cap_evidence.md"
s=open(p,encoding="utf-8").read()
s=s.replace("`reversed/net/decompiled/GameState.cs:1904-1906`：","`reversed/net/decompiled/GameState.cs:1908-1910`：")
s=s.replace("`GameState.cs:1905`","`GameState.cs:1908-1910`")
s=s.replace("（`GameState.cs:1905`）","（`GameState.cs:1908-1910`）")
open(p,"w",encoding="utf-8").write(s)
print("fixed GameState line refs")
