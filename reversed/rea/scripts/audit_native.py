import re, pathlib
native = pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Hearthstone_Data\Plugins\x86_64\libacsdk_x64.dll")
print("native size MB:", round(native.stat().st_size/1e6,1))
data=native.read_bytes()
keys=[b"Battleground",b"Bacon",b"Combat",b"PowerHistory",b"Simulate",b"DamageCap",b"Battle.net",b"AntiCheat",b"Warden",b"acsdk"]
for k in keys:
    c=data.count(k)
    print(f"  {k.decode():14s} ascii={c}")
# utf16
for k in [b"Battleground",b"Combat",b"PowerHistory"]:
    u=k.decode().encode("utf-16-le")
    print(f"  {k.decode():14s} utf16={data.count(u)}")
# a few context strings
for k in [b"Battleground",b"Combat",b"PowerHistory"]:
    i=data.find(k)
    if i>=0:
        seg=data[max(0,i-80):i+120]
        print("  ctx",k.decode(),":",re.sub(rb"[^\x20-\x7e]",b".",seg).decode("ascii"))
