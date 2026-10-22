import sys, dataclasses
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller import state, records
try:
    s=state.GameState()
    print("GameState() OK:", s.turn, s.phase, len(s.shop), len(s.board))
except Exception as e:
    print("GameState() FAIL:", repr(e))
try:
    l=records.Legal()
    print("Legal() OK:", l.end, l.buy)
except Exception as e:
    print("Legal() FAIL:", repr(e))
try:
    m=state.Minion()
    print("Minion() OK:", m.card, m.atk, m.hp)
except Exception as e:
    print("Minion() FAIL:", repr(e))
try:
    it=state.Item()
    print("Item() OK:", it.kind, it.card, it.cost)
except Exception as e:
    print("Item() FAIL:", repr(e))
