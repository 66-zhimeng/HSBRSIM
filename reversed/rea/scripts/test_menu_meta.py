import sys, json
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
info=M.process_info(); r=M.SnapshotReader(info['pid'])
try:
    image=r.attach_image(info['mono_base']); r.bind_metadata(image)
    gs=r.game_state(image); print("gs:", gs)
    gsc=getattr(r,'_game_state_class',None); print("_game_state_class:", gsc)
    if gsc:
        cls = gsc[1] if isinstance(gsc,(tuple,list)) and len(gsc)>1 else gsc
        try: fields=r.class_fields(cls); print("fields:", fields)
        except Exception as e: print("class_fields err", e)
        names=("m_entityMap","m_playerMap","m_local","m_choicesMap","m_gameEntity",
               "m_availableRacesInBattlegroundsExcludingAmalgam","m_options","m_queuedChoices")
        try: print("offsets:", r.field_offsets(cls,names))
        except Exception as e: print("field_offsets err", e)
finally:
    r.close()
