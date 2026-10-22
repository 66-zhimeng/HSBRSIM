import sys, dataclasses
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller import state
for cls in (state.GameState, state.Item, state.Minion, state.HeroPower, state.Trinket if hasattr(state,'Trinket') else state.Deity, state.Quest, state.Opponent):
    print("\n"+cls.__name__+":")
    for f in dataclasses.fields(cls):
        d=f.default if f.default is not dataclasses.MISSING else ('factory' if f.default_factory is not dataclasses.MISSING else '')
        print(f"  {f.name}: {f.type}  default={d!r}")
