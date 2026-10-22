import sys
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
info=M.process_info(); r=M.SnapshotReader(info['pid'])
try:
    image=r.attach_image(info['mono_base']); r.bind_metadata(image)
    print("image:", hex(image) if image else image)
    print("_game_state_class:", r._game_state_class)
    print("_metadata keys:", list(r._metadata.keys()) if hasattr(r._metadata,'keys') else r._metadata)
    print("_class_layouts:", type(r._class_layouts), len(r._class_layouts) if hasattr(r._class_layouts,'__len__') else '?')
    print("_public_class_cache:", type(r._public_class_cache), len(r._public_class_cache) if hasattr(r._public_class_cache,'__len__') else '?')
    gsc=r._game_state_class
    if gsc:
        cls=gsc[1] if isinstance(gsc,(tuple,list)) and len(gsc)>1 else gsc
        print("game_state cls ptr:", hex(cls) if isinstance(cls,int) else cls)
        print("GameState fields:", r.class_fields(cls))
        print("GameState offsets:", r.field_offsets(cls, ("m_entityMap","m_playerMap","m_local","m_choicesMap","m_gameEntity","m_availableRacesInBattlegroundsExcludingAmalgam")))
        print("_game_state_instance:", r._game_state_instance if hasattr(r,'_game_state_instance') else 'n/a')
finally:
    r.close()
