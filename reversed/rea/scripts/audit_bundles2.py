import pathlib
from collections import Counter
win=pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Data\Win")
files=list(win.glob("*.unity3d"))
cat=Counter()
for f in files:
    n=f.name.lower()
    for c in ("cardasset","carddef","cardtexture","sound","video","music","hero","board","battleground","bacon","prefab","material","texture","asset_manifest","initial","portpremium","essential","ui"):
        if c in n: cat[c]+=1; break
    else: cat["other"]+=1
print("bundle categories:", dict(cat.most_common()))
print("total files:", len(files), "total GB:", round(sum(f.stat().st_size for f in files)/1e9,2))
# other dirs under Hearthstone_Data
hd=pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Hearthstone_Data")
print("Hearthstone_Data dirs:", [p.name for p in hd.iterdir() if p.is_dir()])
for d in ("Resources","StreamingAssets"):
    p=hd/d
    if p.exists(): print(d, "files:", len(list(p.rglob("*"))))
