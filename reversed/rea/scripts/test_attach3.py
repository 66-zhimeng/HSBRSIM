import sys, json
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
info=M.process_info(); r=M.SnapshotReader(info['pid'])
try:
    image=r.attach_image(info['mono_base']); print("image:", image)
    r.bind_metadata(image)
    layouts=r._object_layouts
    print("layouts type:", type(layouts), "len:", len(layouts))
    keys=list(layouts.keys()) if hasattr(layouts,'keys') else layouts
    print("layout keys sample:", [str(k) for k in list(keys)[:40]])
    for name in ("GameState","Entity","Player","GameEntity","EntityDef","Card"):
        cls=None
        # layouts may keyed by class name or MonoClass ptr
        if hasattr(layouts,'get'):
            for k in layouts:
                kn = k if isinstance(k,str) else str(k)
                if name in kn:
                    cls=k; break
        print(f"\n[{name}] layout key: {cls!r}")
        if cls is not None:
            try:
                print("  fields:", r.class_fields(cls))
            except Exception as e: print("  class_fields err:", e)
            try:
                print("  offsets:", r.field_offsets(cls, ("m_entityMap","m_tags","m_cardIdInternal","m_gameEntity","m_playerMap","m_local","m_choicesMap","m_availableRacesInBattlegroundsExcludingAmalgam")))
            except Exception as e: print("  field_offsets err:", e)
finally:
    r.close()
