# 不确定卡牌审计第二轮：真实对局证据

> 自动生成：`2026-10-19T01:12:48+00:00`；机器可读：`reversed/rea/evidence_round2.json`。
> 只读分析：未改引擎/数据/环境。

## 1. 覆盖与来源

| 卡 | 名称 | Power.log 行 | trace 出现 | state corpus | HDT log | XML |
|----|------|-------------:|-----------:|-------------:|--------:|----:|
| `BG31_171` | Moonsteel Juggernaut | 0 | 49 | 181 | 0 | 0 |
| `BGS_004` | Wrath Weaver | 8600 | 79 | 56 | 0 | 2 |
| `BG31_035` | Groundbreaker | 0 | 36 | 36 | 0 | 0 |
| `BG26_174` | Soul Rewinder | 10800 | 111 | 108 | 0 | 9 |
| `BG35_701` | Brazen Buccaneer | 0 | 61 | 61 | 0 | 0 |
| `BG32_822` | Fire-forged Evoker | 0 | 46 | 46 | 0 | 0 |
| `BG33_155` | Devout Hellcaller | 1400 | 30 | 28 | 0 | 0 |
| `BG34_320` | The Last One Standing | 1617 | 19 | 16 | 0 | 1 |
| `BGFYM_000` | C'Thun (Deity token) | 1052 | 22 | 17 | 0 | 0 |
| `BG25_008` | Eternal Knight | 3960 | 93 | 88 | 0 | 1 |
| `BG35_342` | Falling Sky Golem | 299 | 66 | 65 | 0 | 0 |
| `BG35_921` | Abyssal Bruiser | 0 | 21 | 21 | 0 | 0 |
| `BG_TTN_401` | Ancestral Automaton | 0 | 35 | 38 | 0 | 3 |
| `BG26_537` | Flourishing Frostling | 0 | 0 | 2 | 0 | 2 |
| `BG36_524` | Maritime Extortionist | 267 | 4 | 1 | 0 | 0 |
| `BG25_013` | Rot Hide Gnoll | 0 | 0 | 0 | 0 | 0 |
| `BG31_810` | Ultraviolet Ascendant | 240 | 1 | 0 | 0 | 0 |

说明：`Power.log 行` 为所有 `runs/**/Power.log`（含 live 采集）中的行命中数；`trace 出现` 含 board+hand+golden 的实体/事件出现次数；`state corpus` 为 `states.jsonl` 行命中。

## 2. 逐卡证据与裁定

### `BG31_171` Moonsteel Juggernaut — **can_implement**

- 类别：improve-this 数值序列；置信：high
- 结论：HDT trace 配对样本显示：Moonsteel 的 ScriptDataNum1 = 当前 Satellite 数值，ScriptDataNum2 = 下一次 EoT 后的 Satellite 数值（普通 +6/+6、金色 +12/+12）；手牌 Satellite 常滞后于 SCN1，支持“每次 EoT 把未来 Satellite 提高一步”，不是给 Moonsteel 自己加属性。Power.log 无原始样本（0 行），因此没有逐事件 EoT 序列。

- 证据 `official_traces_hdt/258fe84214186d8a.json`（trace `258fe84214186d8a` turn 10 player）：combat boundary snapshot → Moonsteel 595/589, SCN1=72, SCN2=78；hand Satellites=60/60,66/66,72/72。**推断**：SCN1=72 等于最新 Satellite；SCN2=78=SCN1+6（普通步长）
- 证据 `official_traces_hdt/652075ebff8d9242.json`（trace `652075ebff8d9242` turn 7 player）：combat boundary snapshot → Moonsteel 8/8, SCN1=18, SCN2=24；hand Satellites=6/6,12/12,18/18。**推断**：再次验证 SCN1=当前值18、SCN2=下次24；旧 Satellite 保留 6/12
- 证据 `official_traces_hdt/0311764c97bfa869.json`（trace `0311764c97bfa869` turn 15 player）：combat boundary snapshot → Golden Moonsteel 2803/2073, SCN1=102, SCN2=114；hand Satellites=102/102。**推断**：金色步长为 +12（114-102），与 CardDef BG31_171_G SCN2=12 一致
- 证据 `official_traces_hdt/571661347e3518c6.json`（trace `571661347e3518c6` turn 14 opponent）：combat boundary snapshot → Golden Moonsteel 396/324, SCN1=42, SCN2=54, Enchantments=AutoAssembler(8,8)。**推断**：金色 42→54（+12）；附魔是外源 buff，不改变 SCN1/SCN2 关系

可推出：
- 普通 Moonsteel: 当前 Satellite 数值 = SCN1；下一档 = SCN1+6
- 金色 Moonsteel: 下一档 = SCN1+12
- 手牌 Satellite 的多个档位 (=6,12,18,...) 与历史 EoT 序列一致，SCN2 是“下一次”而非已发放值

不可推出 / 边界：
- Power.log 无原始 EoT 前后事件序列；无法直接观察 Moonsteel 自身 atk/health 是否变化（trace 中其高属性来自其它 buff）
- SCN2 的 +6/+12 是否叠加其它 improve 来源（饰品/畸变）未测试；仅普通/金色基础步长有样本

engine 建议：无需新原语：现有 create_minion/Buff/script-data 通道可实现“EoT 后未来 Satellite 数值 +6/+12”；实现前把该解释作为 Moonsteel 的 rule，不要用 trace 的 SCN2 直接当自身属性。

### `BGS_004` Wrath Weaver — **can_implement_official_slots**

- 类别：C 类 tag 槽位；置信：high_for_base / medium_for_golden
- 结论：Power.log 官方创建 tag 为 SCN1=2、SCN2=2（普通/金色均如此），实际每次恶魔触发后atk/health 各 +2（样本 7/9→9/11）；trace 的 ScriptDataNum2=1 是 HDT 侧槽位/版本差异，不能拿来当官方 +health 值。

- 证据 `runs/powerlog_2240/Power.log` line 96451：FULL_ENTITY - Creating ID=4768 CardID=BGS_004 → ATK=1, HEALTH=3, SCN1=2, SCN2=2。**推断**：官方创建 tag 槽位 1/2 = 2/2（CardDef 同值）
- 证据 `runs/powerlog_2240/Power.log` line 96503：TAG_CHANGE Entity=4768 tag=HEALTH value=9; line 96504 tag=ATK value=7 → 7/9；随后 112151/112152 为 9/11，121536/121537 为 11/13。**推断**：每次触发 atk/health 各 +2（不是 trace 的 +2/+1）
- 证据 `runs/live/fix3_real/Power.log` line 7119：FULL_ENTITY Creating BGS_004 → ATK=1, HEALTH=3, SCN1=2, SCN2=2。**推断**：同一官方槽位口径在另一局复现
- 证据 `official_traces_hdt/02b2ef86c6f36613.json`（trace `02b2ef86c6f36613` turn 6 opponent）：combat boundary snapshot → BGS_004 7/10, ScriptDataNum1=2, ScriptDataNum2=1。**推断**：trace 的 SCN2=1 与 Power.log SCN2=2 冲突：HDT trace 槽位不能直读

可推出：
- 官方槽位1/2=+2/+2；基础触发为每次 +2/+2

不可推出 / 边界：
- 金色触发次数/是否 4/4 没有干净的连续样本（官方 SCN 仍 2/2，golden 文本使触发 twice）

engine 建议：按官方 SCN1=2/SCN2=2 注入 +2/+2；不要采用 HDT trace 的 SCN2=1。金色需 engine 脚本按“twice”处理，tag 不能直接给出次数。

### `BG31_035` Groundbreaker — **deferred_no_raw**

- 类别：C 类 tag 槽位；置信：low
- 结论：Power.log 0 行；只有 36 个 HDT trace 快照。观察到 SCN1/SCN2/SCN3 三槽，但槽位语义与CardDef（NUM1=3 阈值、NUM2=1 增益）不对齐，且没有逐次打 Naga / 施法前后的连续序列，无法裁定 SCN2 到底是当前增益、Naga 次数还是法术进度。

- 证据 `official_traces_hdt/077d9ee1f9aff612.json`（trace `077d9ee1f9aff612` turn 13 player）：combat boundary snapshot → BG31_035 2268/2184, SCN1=2, SCN2=84, SCN3=2。**推断**：SCN2 随面板数值增大，但 SCN1/SCN3 仍 2；无法唯一映射
- 证据 `official_traces_hdt/116c80e0c89b9dac.json`（trace `116c80e0c89b9dac` turn 9 player）：combat boundary snapshot → 两张 BG31_035 161/61、86/36，均 SCN1=4, SCN2=10, SCN3=4。**推断**：同卡同回合两个实体 tag 相同，SCN2=10 与面板差值的比例不固定
- 证据 `official_traces_hdt/0ebb9c23013a15e4.json`（trace `0ebb9c23013a15e4` turn 11 opponent）：combat boundary snapshot → BG31_035 22/12, SCN1=2, SCN2=6, SCN3=2。**推断**：SCN2=6 更可能是已累计触发/增益，但缺触发前值

可推出：
- HDT 记录了三槽 ScriptDataNum1/2/3；SCN2 随成长显著变化，SCN1/SCN3 常为 1/2/4

不可推出 / 边界：
- SCN2 与 Naga 次数、每 3 法术进度、当前 +X/+X 三者的对应关系
- 官方 Power.log 的槽位/是否存在 SCN1=3 的初始值（无样本）

engine 建议：保持 DEFERRED；等 Power.log 真实触发序列或 engine-analyst 给槽位映射后再实现，不要用 HDT 快照猜。

### `BG26_174` Soul Rewinder — **deferred_slot_conflict**

- 类别：C 类 tag 槽位；置信：medium
- 结论：Power.log 官方创建槽位是 SCN3=2（base）/4（金色），与 CardDef NUM3=2/4、文本 `{2}` 一致；但 HDT trace 主要记录 ScriptDataNum2（如 5/9/20/21），槽位发生 2↔3 偏移，无法用 trace 裁定回血/计数增量。

- 证据 `runs/live/live_final_powerlog/Power.log` line 76046：FULL_ENTITY Creating BG26_174 → ATK=4,HEALTH=2, SCN3=2；line 76071 SCN2=7（战斗/回血过程中动态）。**推断**：官方有 SCN2 动态值，但创建默认槽位是 SCN3=2；不能把 trace 的 SCN2 当 NUM3
- 证据 `runs/powerlog_2240/Power.log` line 120181：FULL_ENTITY Creating BG26_174_G（金色） → ATK=8,HEALTH=4, SCN3=4；line 120212 SCN3=2→ line 120359 SCN3=4。**推断**：金色官方槽位 SCN3=4；HDT trace 却常见 SCN1/SCN2，需显式映射
- 证据 `official_traces_hdt/1779cc01ecb9c134.json`（trace `1779cc01ecb9c134` turn 8 player）：combat boundary snapshot → BG26_174 8/7, ScriptDataNum2=21。**推断**：trace 单槽 SCN2=21；与官方创建 SCN3=2 不直接对应

可推出：
- 官方 Power.log 创建槽位 SCN3=2/4（base/golden），与 CardDef NUM3 对齐

不可推出 / 边界：
- SCN2 动态值表示累计回血量、触发次数还是别的；无法裁定 HDT trace 槽位映射

engine 建议：DEFERRED：先在 harness/traceizer 做显式 2↔3 槽位映射并验证；不要直接读 trace ScriptDataNum2 实现。

### `BG35_701` Brazen Buccaneer — **partial_deferred**

- 类别：C 类 tag 槽位；置信：medium
- 结论：无 Power.log 样本；61 个 HDT trace。SCN1/SCN2 与 CardDef 完全一致（base 2/3、golden 4/6），可裁定为每次 EoT buff 的 atk/health；SCN3（13–21）只在部分样本出现，数值与面板/回合不形成可验证关系，不能裁定是否为“本回合打出的牌数”。

- 证据 `official_traces_hdt/0f4c69f5a7355e6c.json`（trace `0f4c69f5a7355e6c` turn 10 player）：combat boundary snapshot → BG35_701 6/6, SCN1=3, SCN2=3, SCN3=13。**推断**：SCN1/2=3/3 与 CardDef base 2/3 不完全一致（可能被其它效果改），SCN3=13 无法解释
- 证据 `official_traces_hdt/13420534fe921b0d.json`（trace `13420534fe921b0d` turn 10 player）：combat boundary snapshot → BG35_701 541/571, SCN1=2, SCN2=3, SCN3=20。**推断**：大面板配 SCN1/2=2/3（基础 buff 量），SCN3=20 很可能是重复次数，但无触发序列
- 证据 `official_traces_hdt/10a617338f450c69.json`（trace `10a617338f450c69` turn 12 opponent）：combat boundary snapshot → BG35_701 22/36 golden, SCN1=4, SCN2=6。**推断**：金色 SCN1/2=4/6 与 CardDef 一致；无 SCN3

可推出：
- SCN1/2 = 每次 EoT 给最左海盗的 +atk/+health（base 2/3，golden 4/6，部分样本 3/3 为其它修正）

不可推出 / 边界：
- SCN3 是否 = 本回合已打出的牌数 / 触发次数；无逐 EoT 序列

engine 建议：先按 CardDef 实现 SCN1/2；SCN3 仍 DEFERRED 或由引擎从本回合已打牌数内部推导，不要外部注入猜测。

### `BG32_822` Fire-forged Evoker — **can_implement_from_trace_relation**

- 类别：C 类 tag 槽位；置信：high
- 结论：无 Power.log 样本，但 46 个 HDT trace（20 个可校验样本）给出稳定的三槽关系：SCN3 = 已施放酒馆法术数；SCN1 = 当前 atk buff（base=2*SCN3+2，golden=2*SCN3+4）；SCN2 = 当前 health buff（base=SCN3+1，golden=SCN3+2）。20/20 校验通过。

- 证据 `official_traces_hdt/102114d8f8e698a0.json`（trace `102114d8f8e698a0` turn 13 opponent）：combat boundary snapshot → BG32_822 309/276, SCN1=62, SCN2=31, SCN3=30。**推断**：base: atk 2*30+2=62；health 30+1=31
- 证据 `official_traces_hdt/0baf27326c6a4fb3.json`（trace `0baf27326c6a4fb3` turn 13 player）：combat boundary snapshot → Golden 49/38, SCN1=90, SCN2=45, SCN3=43。**推断**：golden: atk 2*43+4=90；health 43+2=45
- 证据 `official_traces_hdt/370694182484bb4f.json`（trace `370694182484bb4f` turn 8 player）：combat boundary snapshot → Base 8/5, SCN1=4, SCN2=2, SCN3=1。**推断**：base 初值：atk 2*1+2=4；health 1+1=2
- 证据 `official_traces_hdt/0502f1b15163d311.json`（trace `0502f1b15163d311` turn 5 opponent）：combat boundary snapshot → hand BG32_822 8/5, SCN1=2, SCN2=1（无 SCN3）。**推断**：初始 buff 2/1（base）；与 0 法术公式一致

可推出：
- SCN3 = 本局已施放酒馆法术数（0..73 样本范围）
- base: SCN1=2*SCN3+2, SCN2=SCN3+1；golden: +4 / +2 基值
- 每施放 1 个酒馆法术，atk buff +2、health buff +1

不可推出 / 边界：
- 没有 Power.log 逐次施法事件，无法验证“施法时立即更新”还是回合结束更新；也无 golden 初始 4/2 的独立样本

engine 建议：可按上述公式实现或由引擎在 Tavern spell cast 事件累加；SCN3 可由引擎自己计数，不必外部注入。

### `BG33_155` Devout Hellcaller — **partial_base_confirmed**

- 类别：C 类 tag 槽位；置信：high_for_base / no_golden_raw
- 结论：Power.log 官方创建 SCN1=2、SCN2=2（base），每次另一友方恶魔造成伤害后 atk/health 各 +2（8/8→10/10→12/12）；HDT trace 记 SCN1=1、SCN2=2（base）/2、4（golden），与官方不一致。base 可裁定；金色无 raw 样本。

- 证据 `runs/live/fix3_real/Power.log` line 26076：FULL_ENTITY Creating BG33_155 → ATK=4,HEALTH=4, SCN1=2, SCN2=2。**推断**：官方 base 槽位1/2 = 2/2
- 证据 `runs/live/fix3_real/Power.log` line 27762：FULL_ENTITY Creating BG33_155; changes 27817/27818 8/8; 27977/27978 10/10; 28145/28146 12/12 → 4/4 → 8/8 → 10/10 → 12/12。**推断**：每次触发 +2/+2（不是 trace 的 1/2）
- 证据 `official_traces_hdt/11177a238772476e.json`（trace `11177a238772476e` turn 9 player）：combat boundary snapshot → BG33_155 74/89, SCN1=1, SCN2=2。**推断**：trace 槽位值 ≠ Power.log 官方 2/2；HDT 侧不可直读

可推出：
- 官方 base 槽位1/2=+2/+2；每次触发 +2/+2

不可推出 / 边界：
- 金色 raw 样本缺失（HDT trace 金色 SCN1=2/SCN2=4 不可信）

engine 建议：base 按 +2/+2 实现；金色保持 DEFERRED 或按卡面 twice 处理，等 Power.log 金色样本。

### `BG34_320` The Last One Standing — **partial_all_multiselect**

- 类别：Rally + ALL 多次选取；置信：high_for_duplicate_ALL / medium_for_algorithm
- 结论：Power.log 实测 Rally 触发时 Sub-spell TargetCount=3，目标同时包含两张 ALL 随从（BG32_111 与 BG34_320 自身）和一张 ABERRATION（BG36_103）；随后每目标各生成一张 BG34_320e 附魔（+15/+15，等于 CardDef），并对 SETASIDE 副本也生成镜像附魔。证明多张 ALL 随从会各自被选中（不合并成一个“ALL 槽”），但随机/顺序/重复选取规则仍不完整。

- 证据 `runs/powerlog_2214/Power.log` line 159706：BLOCK_START BlockType=TRIGGER ... cardId=BG34_320 player=3 TriggerKeyword=BACON_RALLY → TargetCount=3；line 159709-159711 Targets=[BG32_111(ALL), BG36_103(ABERRATION), BG34_320(ALL)]。**推断**：两张 ALL 都被选；自身也可被选
- 证据 `runs/powerlog_2214/Power.log` line 159712：FULL_ENTITY Creating ID=10006 CardID=（随后 SHOW_ENTITY CardID=BG34_320e） → line 159716/159719 BG34_320e ATTACHED=7469；NUM1=NUM2=15；后续 10007→9357、10008→7461、10009→9382、10010→8694、10011→9351。**推断**：每个 play target 一个附魔（+15/+15），SETASIDE 镜像实体也各一个；不是同一实体重复叠加
- 证据 `runs/powerlog_2214/Power.log` line 214184：turn20 另一次 Rally trigger (eid=9351) 同样出现 → 。**推断**：同一局可重复触发；支持 Rally 是每次攻击触发而非一次性

可推出：
- Rally 对每种“类型”选取一个友好随从；多张 ALL 随从会分别被选中（两张 ALL 都拿到 +15/+15）
- 自身（BG34_320）可作为目标
- SETASIDE 战斗副本会镜像获得同额附魔（日志层双写，不是额外多选一次）

不可推出 / 边界：
- 单张 ALL 随从是否会因匹配多个缺失类型而被重复选取（本样本有两张 ALL，无法区分“每个 ALL 一次”与“每个类型一次”）
- 多个同类型非 ALL 随从时的随机/最左/最高攻选择规则
- 空类型、满场、目标死亡等边界

engine 建议：先实现“每张 ALL 随从各被选一次 + 自身可选取”的证据版；随机/重复选择留 DEFERRED，需更多带两张以上 ALL 的样本。

### `BGFYM_000` C'Thun (Deity token) — **can_implement_floor_equal_split**

- 类别：C'Thun awaken split 分配；置信：high_for_n2_n3 / medium_for_edge
- 结论：Power.log 两次 awaken split 显示：C'Thun 自身属性按“当前其它友方随从数量 N”均分，每目标得到 floor(atk/N)、floor(health/N)；余数不分配（191/2→95 每目标，138/2→69；33/3→11）。没有 ceil/随机加权的证据。

- 证据 `runs/powerlog_2214/Power.log` line 172605：BLOCK_START TRIGGER BGFYM_000 id=10064 player=11；SUB_SPELL Bacon_Cthun_Missile_Buff_Super → C'Thun 191/138（172573/172574 最后状态）；10067→9775 NUM1=95/NUM2=69；10068→9780 NUM1=95/NUM2=69（172682 后续 96）。**推断**：N=2 个 target；floor(191/2)=95、floor(138/2)=69；atk 余数1未分
- 证据 `runs/powerlog_2214/Power.log` line 174028：BLOCK_START TRIGGER BGFYM_000 id=10097 player=3 → C'Thun 33/33；10101→8694=11/11、10103→7469=11/11、10104→7459=11/11。**推断**：N=3；floor(33/3)=11/11 每目标
- 证据 `runs/powerlog_2214/Power.log` line 108099：首次 awaken id=5941（C'Thun 1/1）；5943→3899=1/1 → 。**推断**：N=1 时目标得到全部 1/1

可推出：
- split 只作用于“其它友方随从”（不含自身；token 自己随后死亡/移除）
- 每个目标独立计算 floor(stat/N)，各目标相同；余数未再分配

不可推出 / 边界：
- N=0（满场/无其它随从）时是否无效果、是否保留 token 属性
- atk/health 是否分别独立取整（样本 191/138 均为 floor；无奇数单维样本）
- 余数是否可能随机给某一目标（本样本没有；不能排除低概率）

engine 建议：可实现 equal split: 对每个 other friendly minion 加 floor(atk/N)/floor(health/N)，余数丢弃；保留 N=0 边界 DEFERRED。

## 3. P2b 8 族动态 ScriptDataNum

8 族动态 ScriptDataNum 卡（P2b）：printed 面板含其它 buff，适配器 (printed-base)/step 不整除时保留缺口

| 卡 | 名称 | trace | corpus | 状态/样本 |
|----|------|------:|-------:|-----------|
| `BG25_008` | Eternal Knight | 93 | 88 | 已有 trace/raw；P2b 失败样本保留；official_traces_hdt/0a36546bf7a9fbd7.json: BG25_008 95/44 golden, tags SCN1=8/SCN2=4；base 8/4，step8/4 → atk (95-8)/8 不整除 |
| `BG35_342` | Falling Sky Golem | 66 | 65 | 新增 raw 实证（live Power.log）；runs/live/live_final_powerlog/Power.log:173216 BG35_342 初始 4/2 tags SCN1=4/SCN2=2；173294 变 64/30 → atk (64-4)/4=15，health (30-2)/2=14 不一致 → adapter_skip 合理 |
| `BG35_921` | Abyssal Bruiser | 21 | 21 | 仅 trace；P2b 失败；0834b4802acd88bb: BG35_921 116/107 tags SCN1=1/SCN2=1；base 2/1，step2/1 → atk57 vs health106 不一致 |
| `BG_TTN_401` | Ancestral Automaton | 35 | 38 | trace 无 ScriptDataNum tag；raw 无样本；CardDef num 缺失；0b7c047cbff31dee: BG_TTN_401 3/4，无 tags；无法验证动态计数 |
| `BG26_537` | Flourishing Frostling | 0 | 2 | 无 trace/raw 样本；仅 XML/corpus 名称出现；XML/corpus 2 次；无 ScriptDataNum |
| `BG36_524` | Maritime Extortionist | 4 | 1 | 少量样本；raw 有基础序列；runs/powerlog_2214/Power.log:92450 BG36_524 初始 7/7 tags SCN1=7/SCN2=7；92548 变 14/14（+7/+7）→ 步长与 tag 一致 |
| `BG25_013` | Rot Hide Gnoll | 0 | 0 | 无样本（Corpus/XML 也 0；仅 CardDef 文本）；无 |
| `BG31_810` | Ultraviolet Ascendant | 1 | 0 | 仅 1 个 raw 实体（live Power.log），无 ScriptDataNum tag；已知公式冲突仍 DEFERRED；runs/live/live_final_powerlog/Power.log:63050 BG31_810 6/6 初始，无 SCN tag；trace 17493f10 的 9/6 冲突仍未解 |

## 4. 跨源一致性警告（重要）

- **Power.log 官方 TAG_SCRIPT_DATA_NUM_* 与 HDT trace 的 `ScriptDataNum*` 槽位不完全一致**：Wrath Weaver、Devout Hellcaller、Soul Rewinder 的 raw 创建值/trace 值出现 2↔3 或 1↔2 偏移；用 trace 槽位实现前必须做显式映射。
- HDT trace 是**战斗边界快照**，不是逐事件序列；Moonsteel、Fire-forged Evoker 的结论来自多快照数值关系（20/20 公式校验），但没有 EoT/施法瞬间序列。
- 没有样本就写“无样本”，所有 DEFERRED 卡均未用卡面/猜值填充。

## 5. 与 engine-analyst 对齐建议

- **可据证据实现**：`BG31_171`（未来 Satellite +6/+12）、`BGS_004`（官方 +2/+2）、`BG32_822`（SCN3=法术数，base/golden 公式）、`BG33_155` base（+2/+2）、`BGFYM_000`（equal floor split）。
- **部分可做**：`BG34_320`（两张 ALL 各自被选；随机/重复规则待样本）、`BG35_701`（SCN1/2 可，SCN3 待定）、`BG25_008`/`BG35_342`/`BG36_524`（P2b 适配器样本补充）。
- **仍 DEFERRED**：`BG31_035`（无 raw，槽位不明）、`BG26_174`（槽位冲突）、`BG_TTN_401`（无 tag/raw）、`BG26_537`（无样本）、`BG25_013`（无样本）、`BG31_810`（公式冲突无新证据）。

