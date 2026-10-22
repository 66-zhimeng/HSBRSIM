# `generate_bg_data.py` 消费 255130 数据 — 试跑报告

> 自动生成，勿手改。复现：`python tools/rea_generate_probe.py`

- 生成时间: 2026-10-18T07:53:15+00:00
- 输入 XML: `C:\HSagent\reversed\data_255130_input\CardDefs.merged.xml`
- 输出目录: `C:\HSagent\reversed\data_255130`（**未触碰 `data/`**）
- 生成器: tools/generate_bg_data.py (未修改，经 importlib 猴补丁输入输出路径)
- 返回码: 0

## 1. 生成结果（vs 当前 `data/`）

| 文件 | 255130 条目 | 当前 data/ 条目 | Δ |
|------|-------------|-----------------|----|
| `bg_cards.json` | 3534 | 3405 | 129 |
| `bg_pool_minions.json` | 273 | 247 | 26 |
| `bg_pool_spells.json` | 76 | 75 | 1 |
| `bg_heroes.json` | 118 | 116 | 2 |
| `bg_hero_powers.json` | 165 | 162 | 3 |
| `bg_trinkets.json` | 409 | 379 | 30 |
| `bg_anomalies.json` | 108 | 108 | 0 |
| `bg_quest_rewards.json` | 73 | 73 | 0 |
| `bg_tavern_spells.json` | 205 | 203 | 2 |
| `bg_dark_gifts.json` | 47 | 43 | 4 |
| `pool_minion_texts.json` | 273 | 247 | 26 |

### 1.1 bg_summary 对比

| 字段 | 255130 | 当前 |
|------|--------|------|
| `patch` | 37.0.0.255130.254477 | 36.2.2.249896 |
| `total_bg_cards` | 3534 | 3405 |
| `dark_gifts` | 47 | 43 |
| `hero_powers` | 165 | 162 |
| `tavern_spells` | 205 | 203 |
| `quest_rewards` | 73 | 73 |
| `anomalies` | 108 | 108 |
| `trinkets` | 409 | 379 |
| `pool_minions.count` | 273 | 247 |
| `pool_minions.by_tier` | {'1': 25, '2': 37, '3': 45, '4': 62, '5': 56, '6': 35, '7': 13} | {'1': 22, '2': 34, '3': 43, '4': 55, '5': 46, '6': 35, '7': 12} |
| `pool_spells.count` | 76 | 75 |
| `heroes.count` | 118 | 116 |

## 2. 引擎能否加载新数据（`CardDB.load`）

- **255130 生成数据**: 加载成功 — 3817 张卡、池内随从 273、池内法术 76、无法映射 card_type 14
  - INVALID 类型样例: `BG22_BuddyMeter`, `BG36_Button_DarkGift`, `TB_BaconShopLockAll_Button`, `TB_BaconShopTechUp02_Button`, `TB_BaconShopTechUp03_Button`, `TB_BaconShopTechUp04_Button`, `TB_BaconShopTechUp05_Button`, `TB_BaconShopTechUp06_Button`, `TB_BaconShopTechUp07_Button`, `TB_BaconShop_1p_Reroll_Button`, `TB_BaconShop_8p_Reroll_Button`, `TB_BaconShop_DragBuy`, `TB_BaconShop_DragBuy_Spell`, `TB_BaconShop_DragSell`
- **当前 data/**: 加载成功 — 3683 张卡、池内随从 247、池内法术 75、无法映射 card_type 14
  - INVALID 类型样例: `BG22_BuddyMeter`, `BG36_Button_DarkGift`, `TB_BaconShopLockAll_Button`, `TB_BaconShopTechUp02_Button`, `TB_BaconShopTechUp03_Button`, `TB_BaconShopTechUp04_Button`, `TB_BaconShopTechUp05_Button`, `TB_BaconShopTechUp06_Button`, `TB_BaconShopTechUp07_Button`, `TB_BaconShop_1p_Reroll_Button`, `TB_BaconShop_8p_Reroll_Button`, `TB_BaconShop_DragBuy`, `TB_BaconShop_DragBuy_Spell`, `TB_BaconShop_DragSell`

## 3. 结论

- 生成器**无需修改**即可消费 255130 的 CardDefs（结构兼容）。
- 差异主要体现在条目数/数值：例如池内随从 273 vs 247。
- 若 INVALID card_type 数量上升，说明出现生成器未收录的新 CardType；需在 `generate_bg_data.py` 增补分支（本轮不改工具，仅记录）。
