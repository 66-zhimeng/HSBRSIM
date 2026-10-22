import pathlib, re
win = pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Data\Win")
files = list(win.glob("*.unity3d"))
total = sum(f.stat().st_size for f in files)
print("bundles:", len(files), "total MB:", round(total/1e6,1))
sel = [f for f in files if re.search(r"bacon|battleground|board|combat|asset_manifest|script", f.name, re.I)]
print("selected name-match:", len(sel))
keys=[b"Assembly-CSharp", b"MonoScript", b"CombatSimulator", b"BattlegroundsCombat", b"BaconCombatStep", b"PowerTaskList", b"BaconBoardTester"]
for f in sel[:50]:
    data=f.read_bytes()
    hits={k.decode():data.count(k) for k in keys if data.count(k)}
    print(f"{f.name:60s} {len(data)/1e6:6.1f}MB {hits}")
