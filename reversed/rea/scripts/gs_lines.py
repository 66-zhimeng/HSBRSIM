lines=open(r"reversed\net\decompiled\GameState.cs",encoding="utf-8").read().splitlines()
for i in range(1900,1915):
    print(f"{i+1}: {lines[i]}")
