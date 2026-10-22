import json, re, sys, io
for p in [r'reversed\net\key_enums.json', r'reversed\tag_diff.json']:
    try:
        s = open(p, encoding='utf-8-sig').read()
    except Exception as e:
        print(p, 'ERR', e); continue
    hits = set()
    for m in re.finditer(r'"([^"]*DAMAGE_CAP[^"]*)"\s*:\s*(-?\d+)', s):
        hits.add((m.group(1), m.group(2)))
    for m in re.finditer(r'([A-Z_]{4,}DAMAGE_CAP[A-Z_]*)[^A-Za-z0-9]{1,25}(-?\d{1,6})', s):
        hits.add((m.group(1), m.group(2)))
    for h in sorted(hits):
        print(p, '->', h)
