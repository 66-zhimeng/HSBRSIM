p=r"reversed\rea\scripts\damage_cap_evidence.py"
s=open(p,encoding="utf-8").read()
old='''    return {
        "client_version": "37.0.0.255130",'''
new='''    ui_code = {
        "recent_combats_panel_reads_and_displays_tag": snippet(
            ROOT / "reversed/rea/client/PlayerLeaderboardRecentCombatsPanel.cs",
            "m_damageCapValue = GameState", 1, 9),
        "leaderboard_manager_fx_on_cap_change": snippet(
            ROOT / "reversed/rea/client/PlayerLeaderboardManager.cs",
            "bool enabled = GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP_ENABLED)",
            1, 8),
        "entity_enabled_flag": snippet(
            ROOT / "reversed/rea/client/Entity.cs",
            "public bool GetRealtimeBaconDamageCapEnabled()", 4, 3),
        "damage_cap_panel_settext": snippet(
            ROOT / "reversed/rea/client/DamageCapPanel.cs", "public void SetText", 0, 12),
    }
    return {
        "client_version": "37.0.0.255130",'''
assert old in s
s=s.replace(old,new,1)
old2='''        "client_ui_members": cap_methods,'''
new2='''        "client_ui_members": cap_methods,
        "client_ui_code": ui_code,'''
assert old2 in s
s=s.replace(old2,new2,1)
open(p,"w",encoding="utf-8").write(s)
print("patched evidence script")
