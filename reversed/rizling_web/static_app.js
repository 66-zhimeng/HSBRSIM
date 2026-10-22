"use strict";
// Page settings (kept in the browser).
const S = Object.assign({ lang: "zh", anim: true, animSpeed: 1, autoReplay: true, timer: true, art: true, debug: false, log: false, quickBuy: true, repeatSpell: false, effects3d: true, music: true, musicVolume: .12 },
  (() => { try { return JSON.parse(localStorage.getItem("bg_ui") || "{}"); } catch (e) { return {}; } })());
function saveS() { try { localStorage.setItem("bg_ui", JSON.stringify(S)); } catch (e) { /* private storage */ } }
// Public mode (served by webui/public_server.py): a player's page without the debugging
// tools, the event log and the developer buttons, and with a short lobby.
const PUBLIC = !!window.BG_PUBLIC;
let V = null, CAT = [], CATBY = {};
let lang = S.lang;
let selHand = null;            // index of selected hand card
let dbg = { p: 0, sel: null, showReal: false, enabled: S.debug && !PUBLIC };
let timer = { enabled: S.timer, paused: false, speed: 1, left: 0, turn: -1, perTurn: null, firing: false };
let lastCombatTurnShown = -1;
let lobbyOpen = false;
let lastApiError = "", guideShown = false, supportFocus = null, hintFocus = null, hiddenChoice = null, hintRun = 0;
let drag = null;               // { zone, index, card } while dragging
let replayState = null;        // presentation state; V always remains the authoritative live game
let pointerController = null, viewGeneration = 0, commandEpoch = 0, mutationTail = Promise.resolve(), pendingCommands = 0;
let repeatSpellId = null, activeSpell = null;
const regionVersions = new Map();
// Taps expose actions; mouse and touch dragging share the pointer controller.
const TOUCH = !!(window.matchMedia && matchMedia("(hover: none) and (pointer: coarse)").matches);
let tapKey = null;
const KW = { TAUNT: "嘲讽", DIVINE_SHIELD: "圣盾", WINDFURY: "风怒", REBORN: "复生", VENOMOUS: "烈毒", POISONOUS: "剧毒", STEALTH: "潜行", MAGNETIC: "磁力", MEGA_WINDFURY: "超级风怒", CLEAVE: "顺劈", IMMUNE: "免疫", IMMUNE_ATTACKING: "攻击时免疫" };
const RACE = { BEAST: "野兽", DEMON: "恶魔", DRAGON: "龙", ELEMENTAL: "元素", MECHANICAL: "机械", MURLOC: "鱼人", PIRATE: "海盗", QUILBOAR: "野猪人", UNDEAD: "亡灵", ABERRATION: "畸变怪", NAGA: "纳迦", ALL: "全部类型" };
const $ = (id) => document.getElementById(id);
const esc = (s) => String(s ?? "").replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
const nm = (c) => (c ? (lang === "zh" ? c.name_zh || c.name : c.name) : "");
const tx = (c) => (c ? (lang === "zh" ? c.text_zh || c.text : c.text) : "");
const kwName = (k) => (lang === "zh" ? KW[k] || k : k);
const raceName = (r) => (lang === "zh" ? RACE[r] || r : r);
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

// ------------------------------------------------------------------ server calls
/** POST as this page (the public server refuses a POST without the header). */
function post(path, body) {
  return bgFetch(path, { method: "POST", headers: { "Content-Type": "application/json", "X-Requested-With": "bg" }, body: JSON.stringify(body || {}) });
}
/** Take a view from the server. On the public server it also says which room this is
 *  (`ROOM`); a room whose game has not started has no game to show (V stays null and the
 *  hall is shown). */
function setView(v) {
  restoreCombat(v);
  if (v && v.notice) toast(v.notice, "info", 9000);
  // An older answer of a shared game that arrived late (the news came in between).
  if (v && v.table && V && V.table && v.seed === V.seed && v.table.seq < V.table.seq) return;
  if (v && v.room && V && V.room && !v.table && !V.table && v.room.id === V.room.id && !!v.spectator === !!V.spectator && Number(v.room.rev) < Number(V.room.rev)) return;
  if (v && v.room) ROOM = v.room;
  if (v && v.clock) setDeadline(v.clock.deadline_in);
  if (v && v.table) { setDeadline(v.table.deadline_in); if (v.room) beat.game = v.room.id; }
  const selectedId = V && selHand !== null && V.me.hand[selHand] ? V.me.hand[selHand].hid : null;
  const next = v && v.waiting ? null : v;
  if (replayState && (!next || next.seed !== replayState.seed || next.seat !== replayState.seat || next.turn !== replayState.liveTurn || next.phase !== replayState.livePhase || next.me.alive !== replayState.alive || !!(next.table && next.table.left) !== replayState.left)) dismissCombat();
  const changedStage = V && (!next || V.seed !== next.seed || V.turn !== next.turn || V.phase !== next.phase || V.me.alive !== next.me.alive || V.me.ended_turn !== next.me.ended_turn);
  if (changedStage) {
    commandEpoch++; selHand = null; activeSpell = null; repeatSpellId = null; tapKey = null;
    if (pointerController) pointerController.cancel();
    endDrag(); hidePreview();
    if (!next || V.seed !== next.seed || V.seat !== next.seat) dismissCombat();
  }
  noteCombatDamage(v);
  V = next;
  connection.uncertain = false; connectionStatus(true);
  if (!changedStage && V && selectedId !== null) { const index = V.me.hand.findIndex(c => c.hid === selectedId); selHand = index < 0 ? null : index; }
  viewGeneration++;
}
function serializeMutation(work) {
  pendingCommands++; actionBusy = true;
  document.body.classList.add("command-pending");
  $("arena").setAttribute("aria-busy", "true");
  const promise = mutationTail.then(work);
  mutationTail = promise.catch(() => {});
  return promise.finally(() => {
    pendingCommands--; actionBusy = pendingCommands > 0;
    document.body.classList.toggle("command-pending", pendingCommands > 0);
    $("arena").setAttribute("aria-busy", String(pendingCommands > 0));
  });
}
async function requestApi(path, body) {
  let j;
  try {
    if (body !== undefined && connection.uncertain) return { ok: false, uncertain: true };
    j = await bgJson(path, body === undefined ? {} : { method: "POST", headers: { "Content-Type": "application/json", "X-Requested-With": "bg" }, body: JSON.stringify(body || {}) });
  } catch (e) {
    lastApiError = "连接中断，正在确认最新局面"; toast(lastApiError); commandEpoch++;
    if (body !== undefined && V) connection.uncertain = true;
    connectionStatus(false); beat.rev = null;
    if (body !== undefined && V) await recoverGameView();
    return { ok: false, uncertain: true };
  }
  if (j.login_required) { onDropped(); return j; }   // the public server dropped this visitor
  if (j.reload) { toast("页面有更新，正在刷新…", "info"); setTimeout(() => location.reload(), 900); return j; }
  // A hint is read-only. Applying its duplicated view would invalidate the generation
  // captured by aiHint itself; a newer real mutation must invalidate it instead.
  if (j.view && path !== "/api/hint") {
    setView(j.view);
    if (j.ok !== false && (path === "/api/new" || path === "/api/load")) { timer.key = null; timer.turn = -1; }
  }
  if (j.ok === false) { lastApiError = j.error || "这一步没有生效"; toast(lastApiError); }
  return j;
}
function api(path, body) {
  const playRequest = ["/api/act", "/api/undo", "/api/debug", "/api/load", "/api/timeout", "/api/hint", "/api/ai_step", "/api/ai_turn"].includes(path);
  if (playRequest && isReplaying()) return Promise.resolve({ ok: false, replaying: true });
  const readonly = body === undefined || path === "/api/hint";
  if (readonly) return requestApi(path, body);
  const stage = V && [V.seed, V.turn, V.phase, commandEpoch];
  return serializeMutation(() => {
    if (playRequest && isReplaying()) return { ok: false, replaying: true };
    if (path === "/api/timeout" && (!stage || !V || stage[0] !== V.seed || stage[1] !== V.turn || stage[2] !== V.phase || stage[3] !== commandEpoch)) return { ok: false, stale: true };
    return requestApi(path, body);
  });
}
async function refresh() {
  await mutationTail;
  const generation = viewGeneration;
  let j;
  try { j = await bgJson('/api/view'); } catch (e) { connectionStatus(false);beat.rev=null;return toast("连不上服务器，恢复后会自动同步"); }
  if (j.login_required) return onDropped();
  if (j.ok === false && !j.me) return toast(j.error || "服务器没有回应");
  if (generation !== viewGeneration) return;
  setView(j);
  if (V) render(); else if (PUBLIC) showPublicLobby();
}
async function act(a) {
  if (a === null || a === undefined || !V || V.spectator || connection.uncertain || isReplaying()) return { ok: false, replaying: isReplaying() };
  const intent = BGEngine.capture(V, a), epoch = commandEpoch;
  return serializeMutation(async () => {
    if (!V || V.spectator || connection.uncertain || epoch !== commandEpoch || isReplaying()) return { ok: false, stale: true };
    const action = BGEngine.resolve(V, intent);
    if (action === null) { toast("局面已变化，请重新选择这张牌", "info"); return { ok: false, stale: true }; }
    closeHint(false); hidePreview();
    const before = snapStats(V), turn = V.turn, gold = V.me.gold;
    const playIndex = typeof action === "object" && action.kind === "play" ? V.me.hand.findIndex(c => c.hid === action.hid) : action >= V.actions.PLAY && action < V.actions.MAGNETIZE ? Math.floor((action - V.actions.PLAY) / V.actions.N_POS) : -1;
    if (playIndex >= 0 && V.me.hand[playIndex] && V.me.hand[playIndex].type === "spell") activeSpell = V.me.hand[playIndex].hid;
    const wasSpell = V.me.hand.find(c => c.hid === activeSpell);
    const j = await requestApi("/api/act", { action, ...(PUBLIC ? { expected: { code: V.seed, turn: V.turn, phase: V.phase } } : {}) });
    if (!V) return j;
    if (j.ok !== false) {
      selHand = null;
      if (S.repeatSpell && wasSpell && V.turn === turn && !V.me.pending) repeatSpellId = wasSpell.id;
      activeSpell = V.me.pending && V.me.pending.kind === "target" ? activeSpell : null;
    }
    render();
    if (j.ok !== false && S.anim) recruitFx(before, turn);
    if (j.ok !== false && S.anim && V.turn === turn && V.me.gold > gold) floater(document.querySelector("#hero-economy .gold"), `+${V.me.gold - gold} 金币`, "buff", -18);
    return j;
  });
}
let endSent = false;
async function endTurn() {
  if (endSent || !V || V.spectator || isReplaying() || !legal(V.actions.END)) return;
  endSent = true;
  document.querySelectorAll(".end-btn").forEach(b => { b.disabled = true; b.textContent = "已结束 · 结算中…"; });
  try { return await act(V.actions.END); }
  finally { endSent = false; regionVersions.delete("hero"); if (V) render(); }
}

async function debugCmd(cmd) { const j = await api("/api/debug", cmd); render(); return j; }
/** A short message at the bottom: something went wrong (the default), or `kind` "info". */
function toast(msg, kind, ms) {
  const t = $("toast");
  t.textContent = msg;
  t.className = kind === "info" ? "info show" : "show";
  clearTimeout(t._h);
  t._h = setTimeout(() => t.classList.remove("show"), ms || 3500);
}
function legal(a) { return !!(V && !V.spectator && !connection.uncertain && V.mask[a] === 1); }

// ------------------------------------------------------------------ AI
/** A readable label for a flat action index (current view). */
function describeAction(a) {
  const A = V.actions, me = V.me;
  const card = (lst, i) => { const c = (lst || [])[i]; return c ? nm(c) + (c.golden ? "（金色）" : "") : "?"; };
  if (a === A.END) return "结束回合";
  if (a === A.REFRESH) return "刷新";
  if (a === A.FREEZE) return "冻结";
  if (a === A.UPGRADE) return "升级酒馆";
  if (a === A.CANCEL) return "取消";
  if (a >= A.BUY && a < A.SELL) return "购买 " + card(me.shop, a - A.BUY);
  if (a >= A.SELL && a < A.ACTIVATE) return "出售 " + card(me.board, a - A.SELL);
  if (a >= A.ACTIVATE && a < A.CHOOSE) return "激活 " + card(me.board, a - A.ACTIVATE);
  if (a >= A.CHOOSE && a < A.PLAY) {
    const i = a - A.CHOOSE, o = ((me.pending || {}).options || [])[i];
    return "选择第 " + (i + 1) + " 项" + (o && (o.name_zh || o.name) ? "：" + nm(o) : "");
  }
  if (a >= A.PLAY && a < A.MAGNETIZE) { const k = a - A.PLAY; return "打出 " + card(me.hand, Math.floor(k / 8)) + "（位置 " + (k % 8 + 1) + "）"; }
  if (a >= A.MAGNETIZE && a < A.MOVE) { const k = a - A.MAGNETIZE; return "磁力 " + card(me.hand, Math.floor(k / 7)) + " → " + card(me.board, k % 7); }
  return "动作 " + a;
}
async function aiHint() {
  await mutationTail;
  const generation = viewGeneration;
  if (V && V.spectator) return;
  const run = ++hintRun;
  hintFocus = document.activeElement;
  const j = await api("/api/hint", {});
  if (j.ok === false || run !== hintRun || lobbyOpen || generation !== viewGeneration) return;
  const r = j.result, box = $("ai-hint");
  const place = 1 + (1 - r.value) * 3.5;
  box.innerHTML = `<div class="hint-head"><b>AI 建议</b><button id="hint-close" type="button" aria-label="关闭 AI 建议">×</button></div><div class="hint-body"><div class="small">局面评估：预计第 ${place.toFixed(1)} 名</div>` +
    r.actions.map((x) => `<div class="hrow"><span>${esc(describeAction(x.action))}</span><span class="p">${(x.p * 100).toFixed(0)}%</span></div>`).join("") +
    `</div><div class="hint-foot"><button class="primary" id="hint-do" ${r.actions.length ? "" : "disabled"}>执行第一项</button></div>`;
  box.classList.add("show");
  $("hint-do").onclick = () => { closeHint(); if (generation === viewGeneration) act(r.actions[0].action); else toast("局面已变化，请重新获取建议", "info"); };
  $("hint-close").onclick = closeHint;
  $("hint-close").focus({preventScroll:true});
}
function closeHint(restore = true) { hintRun++; $("ai-hint").classList.remove("show"); if (restore && hintFocus && hintFocus.isConnected) hintFocus.focus({preventScroll:true}); }
async function aiTurn() {
  if (!V || V.spectator) return;
  aw.closed = false;
  $("ai-hint").classList.remove("show");
  const before = snapStats(V), turn = V.turn;
  const j = await api("/api/ai_turn", {});
  if (j.ok === false) return;
  render();
  if (S.anim) recruitFx(before, turn);
}

// ------------------------------------------------------------------ watching the AI play seat 0
const aw = { auto: false, delay: 800, min: false, run: 0, closed:false };
function closeAiWatch() { aw.closed = true; aw.auto = false; ++aw.run; $("ai-watch").classList.remove("show"); }
// Auto-play waits while a combat replay is open; pending-choice dialogs are the AI's to resolve.
function awBusy() { return $("combat-modal").classList.contains("show") || lobbyOpen; }
async function aiStep() {
  if (!V || V.spectator || V.phase === "game_over" || !V.me.alive) return false;
  const before = snapStats(V), turn = V.turn;
  const j = await api("/api/ai_step", {});
  if (j.ok === false) return false;
  render();
  if (S.anim) recruitFx(before, turn);
  return true;
}
async function aiAuto() {
  aw.auto = !aw.auto;
  const run = ++aw.run;
  renderAiWatch();
  while (aw.auto && run === aw.run) {
    if (!V || V.phase === "game_over" || !V.me.alive) { aw.auto = false; break; }
    if (awBusy()) { await sleep(300); continue; }  // let the combat replay finish
    if (!(await aiStep())) { aw.auto = false; break; }
    await sleep(aw.delay);
  }
  renderAiWatch();
}
function renderAiWatch() {
  const box = $("ai-watch");
  const show = V && V.ai && V.ai.loaded && (V.ai.spectate || (V.ai_log && V.ai_log.length));
  box.classList.toggle("show", !!show && !lobbyOpen && !isReplaying() && !aw.closed && !V.spectator);
  if (!show) return;
  box.classList.toggle("min", aw.min);
  const spect = V.ai.spectate, over = V.phase === "game_over" || !V.me.alive;
  const pct = (p) => `${(p * 100).toFixed(0)}%`;
  let h = `<div class="aw-head"><b>${spect ? "AI 观战（0 号位由 AI 操作）" : "AI 操作记录"}</b>`;
  if (spect && !over && !aw.min) {
    h += `<button id="aw-step" title="快捷键 N">单步</button><button id="aw-turn">走完本回合</button>`
      + `<button id="aw-auto" class="${aw.auto ? "primary" : ""}" title="快捷键 P">${aw.auto ? "⏸ 暂停" : "▶ 自动"}</button>`
      + `<select id="aw-speed">${[[300, "快"], [800, "中"], [1600, "慢"]].map(([v, t]) => `<option value="${v}" ${aw.delay === v ? "selected" : ""}>${t}</option>`).join("")}</select>`;
  }
  h += `<button id="aw-min">${aw.min ? "展开" : "收起"}</button><button id="aw-close" type="button" aria-label="关闭 AI 操作记录">×</button></div><div class="aw-body">`;
  const nx = V.ai_next;
  if (spect && nx) {
    h += `<div class="aw-sec">AI 现在的判断 <span class="small">（${nx.n_legal} 个可选动作；预计第 ${nx.expected_place.toFixed(1)} 名）</span></div>`;
    for (const x of nx.actions.filter((x, i) => i === 0 || x.p >= 0.005)) {
      h += `<div class="aw-row"><span>${esc(x.text)}</span><span class="p" style="text-align:right;color:var(--gold)">${pct(x.p)}</span><div class="aw-bar"><i style="width:${(x.p * 100).toFixed(1)}%"></i></div></div>`;
    }
    const mx = Math.max(...nx.place_probs, 0.01);
    h += `<div class="small" style="margin-top:6px">名次预测</div><div class="aw-dist">${nx.place_probs.map((q, i) => `<div style="height:${Math.max(2, (q / mx) * 40)}px" title="第 ${i + 1} 名 ${pct(q)}"><span>${i + 1}</span></div>`).join("")}</div><div style="height:12px"></div>`;
  } else if (spect && over) {
    h += `<div class="aw-sec">对局结束：AI 第 ${V.me.placement} 名</div>`;
  }
  const log = V.ai_log || [];
  if (log.length) {
    h += `<div class="aw-sec">AI 的操作（最新在上）</div>`;
    let turn = null;
    for (let i = log.length - 1; i >= 0; i--) {
      const e = log[i];
      if (e.turn !== turn) { turn = e.turn; h += `<div class="aw-turn">第 ${turn} 回合 · 金币 ${e.gold ?? "?"} · 预计第 ${e.expected_place.toFixed(1)} 名</div>`; }
      const alts = e.alts && e.alts.length ? `<span class="alt">备选：${e.alts.map((a) => `${esc(a.text)} ${pct(a.p)}`).join("；")}</span>` : "";
      const moves = e.moves ? `<span class="alt">结束前用模拟战斗调整站位：${e.moves} 次移动</span>` : "";
      h += `<div class="aw-act">${esc(e.text)} <span class="p">${pct(e.p)}</span>${alts}${moves}</div>`;
    }
  } else if (spect) {
    h += `<div class="small" style="margin-top:8px">点"单步"（N）或"自动"（P）开始。每一步都会显示 AI 选了什么、选它的概率和其它备选；结束回合时还会显示它用模拟战斗调整了几次站位。自动播放遇到战斗回放会暂停，关掉回放后继续（设置里可关闭自动回放）。</div>`;
  }
  box.innerHTML = h + "</div>";
  const on = (id, f) => { const el = $(id); if (el) el.onclick = f; };
  on("aw-step", () => { aw.auto = false; aiStep(); });
  on("aw-turn", () => { aw.auto = false; aiTurn(); });
  on("aw-auto", aiAuto);
  on("aw-min", () => { aw.min = !aw.min; renderAiWatch(); });
  on("aw-close", closeAiWatch);
  positionFloat(box);
  const sp = $("aw-speed");
  if (sp) sp.onchange = () => { aw.delay = parseInt(sp.value); };
}

// ------------------------------------------------------------------ card element
function isMinionCard(c) { return c.atk !== undefined && c.hp !== undefined && (c.type === "minion" || c.eid !== undefined || c.kind === "minion"); }
/** A stat as it is shown on a card: whole below 1000, then 1K, 1.5K, 10K, 100K, 1M, 1B
 *  (cut, never rounded up; the exact number stays in the tooltip and the detail window). */
function fmtStat(n) {
  if (typeof n !== "number" || !isFinite(n) || Math.abs(n) < 1000) return String(n);
  const a = Math.abs(n), [v, u] = a >= 1e9 ? [1e9, "B"] : a >= 1e6 ? [1e6, "M"] : [1e3, "K"];
  const x = a / v;
  return (n < 0 ? "-" : "") + (x >= 10 ? Math.floor(x) : Math.floor(x * 10) / 10) + u;
}
/** Attack / Health gems, coloured as in the game: white at or below the card's base stats,
 *  green above them, red Health while damaged. */
function statHtml(c) {
  const baseA = c.base_atk ?? c.atk, baseH = c.base_hp ?? c.hp, maxH = c.max_hp ?? c.hp;
  const a = c.atk > baseA ? " up" : "";
  const h = c.hp < maxH ? " hurt" : maxH > baseH ? " up" : "";
  const gem = (cls, value) => { const text = fmtStat(value); return `<div class="${cls}" data-digits="${text.length}" title="${value}">${text}</div>`; };
  return gem("atk" + a, c.atk) + gem("hp" + h, c.hp);
}
function cardEl(c, o = {}) {
  const d = document.createElement("div");
  const minion = isMinionCard(c);
  const spell = !minion && (c.type === "spell" || (c.kind && (c.kind.includes("spell") || c.kind.includes("trinket"))));
  const kws = c.keywords || [];
  d.className = "card" + (minion ? " minion" : "") + (spell ? " spell" : "") + (c.golden ? " golden" : "") + (kws.includes("DIVINE_SHIELD") ? " shield" : "") + (kws.includes("TAUNT") ? " taunt" : "") + (kws.includes("STEALTH") ? " stealth" : "") + (c.implemented === false ? " unimpl" : "") + (c.locked ? " locked" : "") + (c.frozen ? " frozen" : "") + (c.ticket ? " ticketed" : "");
  const tier = c.tier || 0;
  let h = `<div class="art">${S.art && c.id ? `<img src="${cardArtUrl(c.id)}" alt="" draggable="false" loading="lazy" decoding="async">` : ""}</div><span class="card-face" aria-hidden="true">${esc((nm(c) || "✦").slice(0, 1))}</span>`;
  if (tier) h += `<div class="tierb" data-tier="${esc(tier)}" aria-label="${esc(tier)}星">${"★".repeat(Math.min(tier, 7))}</div>`;
  if (c.ticket) h += `<div class="ticket-ribbon" title="暗月奖券：买下这张牌就得到一张奖券">暗月奖券</div>`;
  h += `<div class="cname">${esc(nm(c))}</div>`;
  if (c.races && c.races.length) h += `<div class="races">${c.races.map(raceName).join("/")}</div>`;
  if (kws.length) h += `<div class="kws">${kws.map((k) => `<span class="kw">${kwName(k)}${k === "DIVINE_SHIELD" && c.shield_hits > 1 ? "×" + c.shield_hits : ""}</span>`).join("")}</div>`;
  h += `<div class="ctext${c.live_text ? " live" : ""}"${c.live_text ? ' title="数值为当前实际值"' : ""}>${esc(tx(c))}</div>`;
  if (c.attached && c.attached.length) h += `<div class="att">${c.attached.map((a) => "◆" + esc(nm(a))).join(" ")}</div>`;
  // (a held card - Thorim's, Faelin's - is locked by its Hero Power's condition, not by turns)
  if (c.locked) h += c.held ? `<div class="att" title="英雄技能的条件达成后解锁，在此之前不能使用">锁定 · 达成技能条件后解锁</div>` : `<div class="att">锁定 ${c.locked} 回合</div>`;
  if (c.temporary) h += `<div class="att">临时（回合结束移除）</div>`;
  if (c.combined) h += `<div class="att">抉择：两种效果合并</div>`;
  if (c.link) h += `<div class="att">关联牌：打出一张弃掉另一张</div>`;
  if (c.group) h += `<div class="att">联动牌组：还可打出 ${c.group_left} 张</div>`;
  if (c.frozen) h += `<div class="att">❄ 已冻结</div>`;
  if (c.kept) h += `<div class="att">上回合保留</div>`;
  if (c.costs_health) h += `<div class="att">用生命值支付</div>`;
  if (c.id === "BG36_520t") h += `<div class="att">剩余 ${c.data} 回合打开</div>`;
  if (c.id === "TB_BaconShop_Triples_01") h += `<div class="att">发现 ${c.data} 本随从</div>`;
  if (c.triple_reward) h += `<div class="att">★ 打出获得三连奖励</div>`;
  if (o.price !== undefined && o.price !== null) h += `<div class="cost">${o.price}</div>`;
  if (minion) h += statHtml(c);
  if (o.tag) h += `<div class="tag${o.tagClass ? " " + o.tagClass : ""}">${esc(o.tag)}</div>`;
  if (o.dead) h += `<div class="skull">☠</div>`;
  d.innerHTML = h;
  d._card = c; d._price = o.price;
  d._actions = o.actions || [];
  const art = d.querySelector(".art img");
  if (art) art.onerror = e => { e.currentTarget.closest(".card").classList.add("art-failed"); e.currentTarget.hidden = true; };
  if (art) art.onload = e => { e.currentTarget.closest(".card").classList.add("has-art"); };
  if (!S.art) d.classList.add("no-art");
  d.setAttribute("aria-label", `${nm(c)}${minion ? `，攻击 ${c.atk}，生命 ${c.hp}` : ""}${o.price != null ? `，价格 ${o.price}` : ""}`);
  d.tabIndex = 0;
  d.setAttribute("role", "group");
  d.onkeydown = (e) => {
    if (e.target !== e.currentTarget) return;
    if (e.key === "Enter") { e.preventDefault(); e.stopPropagation(); if (o.onclick) o.onclick(e); }
    if (e.key === "i" || e.key === "I") { e.stopPropagation(); showDetail(c, o.price); }
  };
  if (o.key) d.dataset.key = o.key;
  if (TOUCH && o.key && o.key === tapKey && o.actions && o.actions.length) d.classList.add("tapped");
  if (o.actions && o.actions.length) {
    const bar = document.createElement("div");
    bar.className = "actions";
    for (const [label, fn, enabled] of o.actions) {
      const b = document.createElement("button");
      b.textContent = label; b.disabled = enabled === false;
      b.onclick = (e) => { e.stopPropagation(); fn(); };
      bar.appendChild(b);
    }
    d.appendChild(bar);
  }
  if (o.onclick) d.onclick = (e) => o.onclick(e);
  // Details: the ⓘ button or right-click.
  const info = document.createElement("button");
  info.type = "button";
  info.className = "info";
  info.textContent = "i";
  info.title = "查看详情（也可右键卡牌）";
  info.setAttribute("aria-label", "查看" + nm(c) + "详情");
  info.onclick = (e) => { e.stopPropagation(); showDetail(c, o.price); };
  d.appendChild(info);
  d.oncontextmenu = (e) => { e.preventDefault(); showDetail(c, o.price); };
  return d;
}

let previewTimer = null;
function hidePreview() {
  clearTimeout(previewTimer);
  $("card-preview").classList.remove("show");
}
function hideActionDock() { $("card-actions").classList.remove("show"); }
function renderActionDock() {
  const dock = $("card-actions");
  if (!TOUCH || !V || V.me.pending || lobbyOpen || document.querySelector(".modal.show")) return hideActionDock();
  const selected = [...$("board").querySelectorAll(".card")].find(el => el.dataset.key === tapKey);
  if (!selected) return hideActionDock();
  dock.replaceChildren();
  const name = document.createElement("span"); name.className = "small"; name.textContent = nm(selected._card); dock.appendChild(name);
  for (const [label, fn, enabled] of selected._actions || []) {
    const button = document.createElement("button"); button.textContent = label; button.disabled = enabled === false;
    if (label.includes("出售")) { button.classList.add("danger"); button.dataset.cardAction = "sell"; }
    button.onclick = fn; dock.appendChild(button);
  }
  const close = document.createElement("button"); close.textContent = "取消"; close.onclick = () => { tapKey = null; hideActionDock(); renderBoardAndHand(); }; dock.appendChild(close);
  dock.classList.add("show");
}
function showPreview(el) {
  if (!el || !el._card || drag || TOUCH || el.closest("#card-preview,.modal:not(.stage-replay)")) return;
  if (document.querySelector(".modal.show:not(.stage-replay),#ai-hint.show")) return;
  const host = $("card-preview");
  host.replaceChildren(cardEl(el._card, { price: el._price }));
  host.classList.add("show");
  const rect = el.getBoundingClientRect(), bounds = host.getBoundingClientRect();
  let x = rect.right + 14;
  if (x + bounds.width > innerWidth - 12) x = rect.left - bounds.width - 14;
  x = Math.max(10, Math.min(innerWidth - bounds.width - 10, x));
  const nav = PUBLIC ? $("support-tools").getBoundingClientRect().bottom : 8;
  const y = Math.max(nav + 8, Math.min(innerHeight - bounds.height - 12, rect.top - 24));
  Object.assign(host.style, { left: x + "px", top: y + "px" });
}
function setupCardPreviews() {
  document.addEventListener("pointerover", e => {
    if (e.pointerType === "touch") return;
    const card = e.target.closest(".card");
    if (!card || card.contains(e.relatedTarget)) return;
    clearTimeout(previewTimer);
    previewTimer = setTimeout(() => showPreview(card), 280);
  });
  document.addEventListener("pointerout", e => { const card = e.target.closest(".card"); if (card && !card.contains(e.relatedTarget)) hidePreview(); });
  document.addEventListener("focusin", e => { if (!TOUCH && e.target.matches(".card")) showPreview(e.target); });
  document.addEventListener("focusout", hidePreview);
  document.addEventListener("scroll", hidePreview, true);
  window.addEventListener("resize", hidePreview);
}

const MODKIND = { add: "增益", set: "设为", merge: "三连继承", tavern: "酒馆加成", magnetic: "磁力吸附" };
function signed(n) { return (n >= 0 ? "+" : "") + n; }
/** Detail panel: identity, stat breakdown, keywords, attached gifts/magnetic cards, texts. */
function showDetail(c, price) {
  const box = $("detail-box");
  const isMinion = c.atk !== undefined && (c.type === "minion" || c.eid !== undefined || c.kind === "minion");
  const b = c.breakdown;
  let h = `<h3>${esc(c.name_zh || "")} / ${esc(c.name)} ${c.golden ? "（金色）" : ""}</h3>
    <div class="small">ID ${esc(c.id)}${c.eid ? " · 实体 #" + c.eid : ""} · ${c.tier ? c.tier + " 本" : ""} ${c.races && c.races.length ? " · " + c.races.map(raceName).join("/") : ""}${price !== undefined ? " · 价格 " + price : ""}${c.implemented === false ? ' · <b style="color:#e46b6b">效果未实现（只按身材和关键词参战）</b>' : ""}</div>
    <p>${esc(c.text_zh || "")}<br><span class="small">${esc(c.text || "")}</span></p>`;
  if (c.golden_text && !c.golden) h += `<div class="small">金色版：${esc(c.golden_text.text_zh || c.golden_text.text)}</div>`;
  if (isMinion) {
    h += `<div><b style="font-size:16px">当前 ${c.atk}/${c.hp}</b>${b && b.damage > 0 ? `（最大生命 ${c.max_hp}，已受伤 ${b.damage}）` : ""}</div>`;
    if (b) {
      let rows = `<tr><td>基础身材（卡牌数据）</td><td></td><td class="n">${b.base[0]}/${b.base[1]}</td></tr>`;
      for (const m of b.mods) {
        const src = m.src ? `${esc(m.src.name_zh || m.src.name)} <span class="small">${esc(m.src.id)}</span>` : '<span class="small">（系统）</span>';
        rows += `<tr><td>${src}</td><td>${MODKIND[m.kind] || m.kind}</td><td class="n">${m.kind === "set" ? "= " + m.atk + "/" + m.hp : signed(m.atk) + "/" + signed(m.hp)}</td></tr>`;
      }
      if (b.this_game_bonus[0] || b.this_game_bonus[1]) rows += `<tr><td>本局类型加成（如亡灵攻击、甲虫、永恒骑士、黑赐计数）</td><td>本局</td><td class="n">${signed(b.this_game_bonus[0])}/${signed(b.this_game_bonus[1])}</td></tr>`;
      if (b.temporary[0] || b.temporary[1]) rows += `<tr><td>临时增益（到你的下个回合开始时移除）</td><td>临时</td><td class="n">${signed(b.temporary[0])}/${signed(b.temporary[1])}</td></tr>`;
      if (b.other[0] || b.other[1]) rows += `<tr><td>其它 / 未记录来源</td><td></td><td class="n">${signed(b.other[0])}/${signed(b.other[1])}</td></tr>`;
      rows += `<tr class="sum"><td>合计（攻击/最大生命）</td><td></td><td class="n">${c.atk}/${c.max_hp}</td></tr>`;
      h += `<h3 style="margin-top:8px">身材构成</h3><table class="dt"><tr><th>来源</th><th>类型</th><th style="text-align:right">攻/血</th></tr>${rows}</table>`;
    }
    const kws = (c.keywords || []).map((k) => kwName(k) + (k === "DIVINE_SHIELD" && c.shield_max > 1 ? `（剩 ${c.shield_hits}/${c.shield_max} 层）` : "")).join("、");
    h += `<div>关键词：${kws || "无"}</div>`;
    if (c.attached && c.attached.length) {
      h += `<h3 style="margin-top:8px">附加效果（黑暗之赐 / 磁力 / 其他）</h3>`;
      for (const a of c.attached) {
        const kind = a.kind === "dark_gift" ? "黑暗之赐" : a.kind === "minion" ? "磁力吸附" : "附加";
        h += `<div class="pill" style="display:block;margin:3px 0;padding:4px 8px"><b>[${kind}] ${esc(a.name_zh || a.name)}</b> <span class="small">${esc(a.id)}</span><br>${esc(a.text_zh || a.text)}<br><span class="small">${esc(a.text)}</span></div>`;
      }
    }
    const extra = [];
    if (c.triple_reward) extra.push("打出时获得三连奖励");
    if (c.activate_cost !== null && c.activate_cost !== undefined) extra.push(`发动费用 ${c.activate_cost}，本回合已发动 ${c.activations} 次`);
    if (c.pool_copies !== undefined) extra.push(`占用公共卡池 ${c.pool_copies} 张`);
    if (extra.length) h += `<div class="small" style="margin-top:6px">${extra.join(" · ")}</div>`;
  }
  h += `<div style="margin-top:10px"><button onclick="document.getElementById('detail-modal').classList.remove('show')">关闭</button></div>`;
  box.innerHTML = h;
  dialogHeader("detail-modal", "卡牌详情");
  $("detail-modal").classList.add("show");
}

// ------------------------------------------------------------------ floating numbers
function floater(el, text, cls, dy = 0) {
  if (!el || !el.isConnected) return;
  const r = el.getBoundingClientRect();
  const f = document.createElement("div");
  f.className = "fl " + cls;
  f.textContent = text;
  f.style.left = r.left + r.width / 2 + "px";
  f.style.top = r.top + r.height / 2 + dy + "px";
  f.addEventListener("animationend", () => f.remove());
  $("fx").appendChild(f);
}
const keyEl = (root, key) => root.querySelector(`[data-key="${key}"]`);
/** Attack / max Health of my minions by element key (to show what an action changed). */
function snapStats(v) {
  const m = {};
  if (!v || !v.me) return m;
  for (const c of v.me.board) m["e" + c.eid] = [c.atk, c.max_hp ?? c.hp];
  for (const c of v.me.hand) if (c.type === "minion") m["h" + c.hid] = [c.atk, c.max_hp ?? c.hp];
  for (const c of v.me.shop) if (c.type === "minion") m["s" + c.sid] = [c.atk, c.max_hp ?? c.hp];
  return m;
}
/** After a recruit action: green numbers on minions that gained stats, a pop on new cards. */
function recruitFx(before, turn) {
  const after = snapStats(V), sameTurn = V.turn === turn;
  for (const [k, [a, h]] of Object.entries(after)) {
    const el = keyEl($("main"), k);
    if (!el) continue;
    const old = before[k];
    if (!old) { if (sameTurn) el.classList.add("pop"); continue; }
    const da = a - old[0], dh = h - old[1];
    if (da || dh) floater(el, `${signed(da)}/${signed(dh)}`, da < 0 || dh < 0 ? "dmg" : "buff");
  }
}

// ------------------------------------------------------------------ rendering
function pendingTargets() { const p = V.me.pending; return p && p.kind === "target" ? p.targets : null; }
function targetIndex(id) { const t = pendingTargets(); return t ? t.indexOf(id) : -1; }
function dbgClick(e, zone, index) {
  if (!dbg.enabled || !(e.altKey || dbg.pick)) return false;
  dbg.sel = { p: 0, zone, index }; dbg.p = 0; dbg.pick = false; renderDebug(); renderBoardAndHand(); return true;
}

function render() {
  applyArenaViewport();
  if (!V) return;
  document.body.classList.toggle("multi", !!V.table);
  document.body.classList.toggle("spectating", !!V.spectator);
  const badge = $("spectator-badge");
  badge.hidden = lobbyOpen || !!V.spectator || !V.spectators;
  badge.textContent = `👁 ${V.spectators || 0} 人观战`;
  if (V.lobby && !lobbyOpen) showLobby();
  $("app").classList.toggle("nodebug", !dbg.enabled);
  maybeShowCombat();
  const update = (name, state, fn) => {
    const key = JSON.stringify([state, lang, S.art, S.anim, dbg.enabled]);
    if (regionVersions.get(name) === key) return;
    regionVersions.set(name, key); fn();
  };
  update("leaderboard", [V.opponents, V.table, V.me.hero, V.me.health, V.me.armor, V.me.tier, V.me.alive, V.me.placement, V.lobby_races, V.seed, V.config], renderLeaderboard);
  update("top", [V.turn, V.phase, V.table, V.user, V.ai, V.spectator, V.undo_depth, V.me.pending, replayState && [replayState.turn, replayState.preview], V.combats && V.combats.map(c => [c.turn, c.a, c.b])], renderTop);
  renderTavern(); renderBoardAndHand();
  update("hero", [V.me, V.mask, V.table, V.spectator, V.emote_events, V.equipped_emotes, S.repeatSpell], renderHero);
  renderEmotes(); renderPending(); renderLog(); renderDebug(); syncTimer(); renderAiWatch();
  $("debug").inert = isReplaying();
  $("arena").dataset.phase = V.phase;
}

function renderLeaderboard() {
  const lb = $("leaderboard");
  const before=scoreboardBeforeUpdate();
  const entries=new Map([...lb.querySelectorAll('.lb-entry[data-p]')].map(el=>[Number(el.dataset.p),el]));
  const all = [{ ...V.me, id: V.seat, me: true }].concat(V.opponents);
  all.sort((a, b) => (b.alive - a.alive) || (b.alive ? (b.health + b.armor) - (a.health + a.armor) : a.placement - b.placement) || a.id-b.id);
  let heading=lb.querySelector('h3');if(!heading){heading=document.createElement('h3');lb.prepend(heading);}
  heading.textContent=`排行榜 · 存活 ${V.alive}`;
  // A shared game: who the humans are and whether they are still busy this turn.
  const humans = {};
  for (const h of (V.table && V.table.players) || []) humans[h.seat] = h;
  for (const p of all) {
    const e = entries.get(p.id) || document.createElement("div");
    const h = humans[p.id];
    e.dataset.p = String(p.id);
    e.className = "lb-entry" + (p.me ? " me" : "") + (p.alive ? "" : " dead") + (p.was_last_opponent ? " lastopp" : "") + (p.is_next_opponent ? " nextopp" : "") + (h ? " human" : "");
    const pct = Math.max(0, Math.min(100, (p.health / 30) * 100));
    const mark = !h || !p.alive || V.table.finished ? "" : h.left ? '<i class="st left" title="已离开，座位已交给 AI 或机器人">离</i>' : h.acting ? '<i class="st busy" title="还在操作">…</i>' : '<i class="st done" title="已结束回合">✓</i>';
    const who = h ? `${mark}<b class="un">${esc(h.username)}</b>${p.me ? '<span class="small">（你）</span>' : ""}<br>` : p.me ? "【你】" : `<span class="small">P${p.id}</span> `;
    const streak=Number(p.win_streak ?? p.stats?.streak ?? 0);
    const content=`${heroArt(p.hero)}<div class="tier">${p.tier}</div><div class="nm"><div class="lb-player-line">${who}<span class="hero-nm">${esc(nm(p.hero))}</span>${p.alive ? "" : ` · 第${p.placement}名`}</div><span class="lb-tribe" title="${esc(warbandTitle(p.last_warband))}">${esc(warbandLabel(p.last_warband))}</span>${p.is_next_opponent ? '<span class="pill">下个对手</span>' : ''}</div><div class="lb-vitals"><div><span class="hpv">${p.health}</span> <span class="arm">${p.armor ? "+" + p.armor : ""}</span></div>${streak>=2 ? `<span class="lb-streak" title="${streak} 连胜" aria-label="${streak} 连胜">🔥<b>${streak}</b></span>` : ''}</div><div class="hbar" style="width:${p.alive ? pct : 0}%"></div>`;
    if(e._content!==content){e.innerHTML=content;e._content=content;}
    e.onclick = () => (p.me ? null : showOpponent(p.id));
    e.title = (p.ai_label ? p.ai_label + ' · ' : '') + (p.me ? "" : "点击查看战队  ") + (p.was_last_opponent ? "上回合对手" : "");
    lb.appendChild(e);
  }
  for(const [id,e] of entries) if(!all.some(p=>p.id===id)) e.remove();
  const info = lb.querySelector('.lb-info') || document.createElement("div");
  info.className = "small lb-info";
  info.style.marginTop = "8px";
  const cfg = V.config || {};
  // A phone shows none of this block: its minion types are in the Tavern's header.
  info.innerHTML = `<b style="color:var(--text)">本局种族</b><span class="lb-more"><br></span> ${V.lobby_races.map((r) => `<span class="pill">${raceName(r)}</span>`).join("")}<span class="lb-more"><br>`
    + (V.has_aberrations ? `神明：${esc(nm(V.deity))}（每场 ${cfg.deity_threshold} 个畸变怪死亡后唤醒）` : "本局没有畸变怪：没有神明")
    + `<br>悖论体：${esc(V.paradox ? nm(V.paradox) : "")}`
    + (cfg.timewarp_enabled ? `<br>时空扭曲：第 ${cfg.timewarp_turns.join("、")} 回合` : "")
    + `<br>版本：${esc(V.version || "")}<br>${PUBLIC ? "对局编号" : "种子"}：${esc(V.seed)}`
    + (cfg.max_turns ? `<br>回合上限：${cfg.max_turns}` : "") + "</span>";
  lb.appendChild(info);
  scoreboardAfterUpdate(before,all.map(p=>p.id).join(','));
  positionEmoteBubbles();
}

function renderTop() {
  const abandon = $('game-abandon');
  if (abandon) { abandon.hidden = !(PUBLIC && V && !lobbyOpen && !V.lobby && !V.spectator && !V.table?.left && V.me?.alive && V.phase !== 'game_over'); abandon.disabled = !!pendingCommands; abandon.onclick = abandonGame; }
  const t = $("topbar");
  t.innerHTML = "";
  const add = (el) => t.appendChild(el);
  const b = (label, fn, cls) => {
    const x = document.createElement("button"); x.textContent = label; x.onclick = fn;
    const short = {"回放上一场战斗":"回放", "AI 代打本回合":"AI代打", "AI 操作记录":"AI记录", "大厅 / 新对局":"大厅", "大厅 / 房间":"大厅", "返回招募 · 继续选择":"继续选择"}[label];
    if (short) { x.dataset.mobileLabel = short; x.title = label; x.setAttribute("aria-label", label); }
    if (cls) x.className = cls;
    return x;
  };
  const turn = document.createElement("span");
  turn.className = "turn";
  turn.textContent = isReplaying() ? `第 ${replayState.turn} 回合 · 战斗` : V.phase === "hero_select" ? "选择英雄" : `第 ${V.turn} 回合`;
  add(turn);
  const ph = document.createElement("span");
  ph.className = "small";
  ph.textContent = isReplaying() ? "战斗回放" : V.phase === "game_over" ? "对局结束" : V.me.alive ? "" : `你已被淘汰（第 ${V.me.placement} 名）`;
  add(ph);
  if (V.lobby_races && V.lobby_races.length) {   // always in sight: the leaderboard's copy is easy to miss
    const races = document.createElement("span");
    races.className = "chip races";
    races.title = "本局出现的随从种族";
    races.innerHTML = lobbyRacesHtml();
    add(races);
  }
  if (V.user) {             // the public server: who is playing here
    const who = document.createElement("span");
    who.className = "chip user";
    who.innerHTML = `${avatar(V.user.username)}<b>${esc(V.user.username)}</b>`;
    add(who);
  }
  if (V.spectator) {
    const label = document.createElement("span"); label.className = "chip"; label.textContent = `观战 ${V.spectator.username} · 只读`; add(label);
    add(b("退出观战", stopSpectating, "primary"));
  }
  if (V.table) {            // a shared game: who everybody is waiting for
    const others = V.table.waiting.filter((n) => n !== V.me.username);
    const st = document.createElement("span");
    st.className = "chip wait" + (others.length ? "" : " ready");
    st.textContent = V.table.finished ? "对局已结束"
      : V.table.left ? "你已离开这局"
      : !V.me.alive ? (others.length ? `还在操作：${others.join("、")}` : "观战中")
      : V.phase === "hero_select" ? (others.length ? `等待选英雄：${others.join("、")}` : "选好英雄就开始")
      : V.me.ended_turn ? (others.length ? `等待 ${others.join("、")}` : "所有人都结束了，开战…")
      : others.length ? `${others.length} 位玩家还在操作` : "其他玩家都结束了，就等你";
    add(st);
  }
  const sp = document.createElement("span"); sp.className = "spacer"; add(sp);
  const lastMine = myLatestCombat();
  if (lastMine) add(b("回放上一场战斗", () => showCombat(lastMine)));
  if (V.me.pending && V.me.pending.kind !== "target") add(b(isReplaying() ? "返回招募 · 继续选择" : "继续选择", () => { if (isReplaying()) dismissCombat(true); hiddenChoice = null; renderPending(); }));
  if (!isReplaying() && V.ai && V.ai.loaded && V.me.alive && V.phase !== "game_over" && !V.table && !V.spectator && aiAssist()) {
    add(b("AI 建议", aiHint));
    add(b("AI 代打本回合", aiTurn));
  }
  if (!PUBLIC && !isReplaying()) add(b(`撤销 (${V.undo_depth})`, async () => { await api("/api/undo", {}); render(); }));
  if (!V.spectator && V.ai && V.ai.loaded && (V.ai.spectate || (V.ai_log || []).length)) add(b("AI 操作记录", () => { aw.closed = false; renderAiWatch(); }));
  add(b(V.table ? "大厅 / 房间" : "大厅 / 新对局", () => showLobby()));
  add(b("设置", () => showSettings()));
  if (!PUBLIC) {
    add(b(dbg.enabled ? "隐藏调试" : "调试面板", () => { dbg.enabled = !dbg.enabled; S.debug = dbg.enabled; saveS(); render(); }));
    add(b("开发进度", () => window.open("/progress", "_blank")));
  }
}

/** Shop minions that would complete (or nearly complete) a Golden. */
function tripleState(c) {
  if (c.type !== "minion" || c.triple_need === undefined) return 0;
  if (c.owned + 1 >= c.triple_need) return 2;
  const inShop = V.me.shop.filter((x) => x.type === "minion" && !x.golden && x.id === c.id).length;
  return c.owned + inShop >= c.triple_need ? 1 : 0;
}

/** The lobby's minion types, for the places that keep them in sight. */
function lobbyRacesHtml() {
  return V.lobby_races && V.lobby_races.length ? `<span class="small">本局种族</span> ${V.lobby_races.map((r) => `<b>${raceName(r)}</b>`).join(" ")}` : "";
}
function renderTavern() {
  const me = V.me, A = V.actions;
  $("shop").style.setProperty("--shop-count", Math.max(7, me.shop.length));
  $("tavern-title").innerHTML = `<b>鲍勃的酒馆</b> · ${"★".repeat(me.tier)} ${me.tier} 本 ${me.frozen ? '<span class="pill">❄ 已冻结</span>' : ""}`;
  $("tavern-races").innerHTML = lobbyRacesHtml();   // a phone shows the types here, a wide page in the top bar
  const tb = $("tavern-buttons");
  tb.innerHTML = "";
  const mk = (label, a, cls, title) => { const x = document.createElement("button"); x.innerHTML = label; x.disabled = !legal(a); x.onclick = () => act(a); if (cls) x.className = cls; if (title) x.title = title; tb.appendChild(x); };
  mk(me.can_upgrade_tier ? `升级酒馆 <b>${me.upgrade_price}</b>` : "已满级", A.UPGRADE, "primary");
  mk(`刷新 <b>${me.refresh_price}</b>${me.free_refreshes ? `（免费×${me.free_refreshes}）` : ""}`, A.REFRESH, "", "快捷键 R");
  mk(me.frozen ? "解冻" : "冻结", A.FREEZE, "", "快捷键 F");
  const shop = $("shop");
  const nodes = [];
  $("bob-status").textContent = me.frozen ? "❄ 酒馆已冻结" : me.ended_turn ? "等大家准备好就开战" : "把随从拖到这里出售";
  $("tavern-host").classList.toggle("frozen", me.frozen);
  shop.classList.toggle("empty", !me.shop.length);
  me.shop.forEach((c, i) => {
    const ti = targetIndex(c.sid);
    const tr = tripleState(c);
    const el = cardEl(c, {
      key: "s" + c.sid,
      price: c.price,
      tag: tr === 2 ? "三连！" : tr === 1 ? "可凑三连" : undefined,
      tagClass: "gold",
      actions: !S.quickBuy || TOUCH ? [["购买", () => act(A.BUY + i), legal(A.BUY + i)]] : [],
      onclick: (e) => { if (dbgClick(e, "shop", i)) return; if (ti >= 0) return act(A.CHOOSE + ti); if (!S.quickBuy || TOUCH) { tapKey = tapKey === "s" + c.sid ? null : "s" + c.sid; renderTavern(); return; } if (legal(A.BUY + i)) act(A.BUY + i); else toast("无法购买（金币不足或手牌已满）"); },
    });
    if (tr === 2) el.classList.add("triple"); else if (tr === 1) el.classList.add("triple-near");
    if (ti >= 0) el.classList.add("targetable");
    if (dbg.sel && dbg.sel.zone === "shop" && dbg.sel.index === i && dbg.p === 0) el.classList.add("dbgsel");
    el.dataset.id = c.sid;
    makeDraggable(el, "shop", i, c);
    nodes.push(el);
  });
  BGEngine.reconcile(shop, nodes, { animate: S.anim });
}

function renderBoardAndHand() {
  const me = V.me, A = V.actions;
  const board = $("board");
  const boardNodes = [];
  board.classList.toggle("empty", !me.board.length);
  const sel = selHand !== null ? me.hand[selHand] : null;
  const selMinion = sel && sel.type === "minion";
  const slot = (pos) => {
    const s = document.createElement("div");
    s.dataset.key = "slot" + pos;
    s.className = "slot";
    const a = handPlacement(selHand, pos);
    if (selMinion && a !== null) {
      s.classList.add("active");
      s.onclick = (e) => { const choice = handPlacement(selHand, pos, e.altKey); if (choice !== null) act(choice); };
      s.title = "放到这里；可磁力吸附时自动吸附，Alt 单独打出";
    }
    return s;
  };
  me.board.forEach((m, i) => {
    boardNodes.push(slot(i));
    const ti = targetIndex(m.eid);
    const acts = [["出售", () => act(A.SELL + i), legal(A.SELL + i)]];
    if (m.activate_cost !== null && m.activate_cost !== undefined) acts.push([`发动(${m.activate_cost})`, () => act(A.ACTIVATE + i), legal(A.ACTIVATE + i)]);
    if (selMinion && canHandMagnetize(selHand, i)) acts.unshift(["吸附", () => act(handMagnetizeAction(selHand, i)), true]);
    if (TOUCH) {
      acts.push(["详情", () => showDetail(m), true]);
      // No dragging on a touch screen: reorder with buttons, one place at a time.
      const mv = (j) => A.MOVE + i * A.N_BOARD + j;
      if (i > 0) acts.push(["◀", () => act(mv(i - 1)), legal(mv(i - 1))]);
      if (i < me.board.length - 1) acts.push(["▶", () => act(mv(i + 1)), legal(mv(i + 1))]);
    }
    const el = cardEl(m, { key: "e" + m.eid, actions: ti >= 0 ? [] : acts, onclick: (e) => { if (dbgClick(e, "board", i)) return; if (ti >= 0) return act(A.CHOOSE + ti); if (tryRepeatSpell(m.eid)) return; tapKey = tapKey === "e" + m.eid ? null : "e" + m.eid; renderBoardAndHand(); } });
    if (tapKey === "e" + m.eid) el.classList.add("tapped");
    if (ti >= 0) el.classList.add("targetable");
    if (selMinion && canHandMagnetize(selHand, i)) el.style.boxShadow = "0 0 0 3px #b58cff";
    if (dbg.sel && dbg.sel.zone === "board" && dbg.sel.index === i && dbg.p === 0) el.classList.add("dbgsel");
    el.dataset.id = m.eid;
    el.dataset.idx = i;
    makeDraggable(el, "board", i, m);
    boardNodes.push(el);
  });
  boardNodes.push(slot(me.board.length));
  BGEngine.reconcile(board, boardNodes, { animate: S.anim });
  $("board-count").textContent = `${me.board.length}/${V.config.board_size}`;
  $("board-hint").textContent = TOUCH
    ? (selMinion ? "点“＋”打出到那个位置，点空白处放到最右边；磁力随从先点机械，再点“吸附”" : "点一下随从：出售、左移、右移")
    : (selMinion ? "拖到空位出牌 · 发光目标可磁力吸附 · Alt 单独打出" : "拖动调整站位 · 拖到鲍勃出售");

  const hand = $("hand");
  const handNodes = [];
  hand.style.setProperty("--hand-count", Math.max(1, me.hand.length));
  hand.classList.toggle("crowded", me.hand.length > 5);
  hand.classList.toggle("empty", !me.hand.length);
  $("hand-title").textContent = `手牌 ${me.hand.length}/${me.hand_limit || V.config.hand_size}${me.hand.length > V.config.hand_size ? "（临时扩容）" : ""}`;
  me.hand.forEach((c, i) => {
    const ti = targetIndex(c.hid);
    const acts = [];
    if (c.type === "spell") acts.push(["施放", () => beginSpell(i), canHandPlay(i, 0)]);
    const el = cardEl(c, {
      key: "h" + c.hid,
      price: c.type === "spell" ? c.cost : undefined,
      actions: ti >= 0 ? [] : acts,
      onclick: (e) => {
        if (dbgClick(e, "hand", i)) return;
        if (ti >= 0) return act(A.CHOOSE + ti);
        if (c.type === "minion") { activeSpell = null; repeatSpellId = null; selHand = selHand === i ? null : i; renderBoardAndHand(); }
        else { tapKey = tapKey === "h" + c.hid ? null : "h" + c.hid; renderBoardAndHand(); }
      },
    });
    if (selHand === i) el.classList.add("selected");
    if (me.casting === c.hid) el.classList.add("casting");
    if (repeatSpellId && c.id === repeatSpellId && c.type === "spell") el.classList.add("repeat-ready");
    if (tapKey === "h" + c.hid) el.classList.add("tapped");
    if (ti >= 0) el.classList.add("targetable");
    if (dbg.sel && dbg.sel.zone === "hand" && dbg.sel.index === i && dbg.p === 0) el.classList.add("dbgsel");
    el.dataset.id = c.hid;
    if (!c.locked) makeDraggable(el, "hand", i, c);
    handNodes.push(el);
  });
  BGEngine.reconcile(hand, handNodes, { animate: S.anim });
  if (repeatSpellId && !me.hand.some(c => c.id === repeatSpellId && c.type === "spell")) repeatSpellId = null;
  renderActionDock();
}

async function beginSpell(index) {
  const card = V && V.me.hand[index];
  if (!card || card.type !== "spell") return;
  return act(handPlayAction(index, 0));
}
function tryRepeatSpell(target) {
  if (!S.repeatSpell || !repeatSpellId || V.me.pending) return false;
  const index = V.me.hand.findIndex(c => c.type === "spell" && c.id === repeatSpellId);
  if (index < 0) { repeatSpellId = null; return false; }
  castOn(index, target);
  return true;
}

/** What holds for the seat's whole game: the size of its Blood Gems (the base +1/+1 and
 *  what the game has added) and the extra stats its Tavern spells give. */
function globalBonuses() {
  const st = (V.me && V.me.stats) || {};
  const pair = (a, h) => `+${fmtStat(a)}/+${fmtStat(h)}`;
  return [
    { label: "宝石加成", value: pair(1 + (st.gem_atk || 0), 1 + (st.gem_hp || 0)), raised: !!(st.gem_atk || st.gem_hp), title: "每颗血色宝石给的攻击 / 生命（基础 +1/+1 加上本局已获得的加成）" },
    { label: "法强加成", value: pair(st.spell_atk || 0, st.spell_hp || 0), raised: !!(st.spell_atk || st.spell_hp), title: "你的酒馆法术额外给的攻击 / 生命" },
  ];
}
function renderBonuses() {
  const list = globalBonuses();
  $("global-bonuses").innerHTML = `<div class="side-title">全局加成</div>` + list.map((b) => `<div class="gstat${b.raised ? " raised" : ""}" title="${b.title}"><span>${b.label}</span><b>${b.value}</b></div>`).join("");
  // (a phone has no right column: the same numbers under the Tavern's title)
  $("tavern-bonus").innerHTML = list.map((b) => `<span class="small">${b.label}</span> <b>${b.value}</b>`).join("　");
}
function renderHero() {
  const me = V.me, A = V.actions;
  const area = $("hero-area");
  const previousHero = area.querySelector(".hero-box");
  const heroKey = JSON.stringify([me.hero?.id, me.hero?.skin_id]);
  area.innerHTML = `
    <div class="hero-box" data-hero-key="${esc(heroKey)}">
      <div class="hero-portrait">${S.art && me.hero ? `<img src="${me.hero.skin_id ? "/assets/skins/" + encodeURIComponent(me.hero.skin_id) + ".webp" : cardArtUrl(me.hero.id)}" alt="" decoding="async">` : ""}<span>${esc((nm(me.hero) || "你").slice(0, 1))}</span></div>
      <div class="hn">${esc(nm(me.hero)) || "尚未选择英雄"}</div>
      <div class="hstat"><span class="h">♥ ${me.health}</span> <span class="a">${me.armor ? "护甲 " + me.armor : ""}</span></div>
    </div>
    <div id="hp-slot" style="display:flex;flex-direction:column;gap:4px"></div>
    <div id="hero-economy" aria-label="本回合金币"></div>`;
  if (previousHero && previousHero.dataset.heroKey === heroKey) {
    previousHero.querySelector('.hstat').innerHTML = area.querySelector('.hstat').innerHTML;
    area.querySelector('.hero-box').replaceWith(previousHero);
  }
  renderBonuses();
  const portrait = area.querySelector(".hero-portrait img");
  if (portrait) { portrait.onerror = e => { e.currentTarget.hidden = true; }; portrait.onload = e => { e.currentTarget.parentElement.classList.add("has-art"); }; }
  // The phone layout also has a short button per active Hero Power in the bottom bar (the
  // hero panel is below the hand there, out of sight most of the time).
  const minis = [];
  const powers = me.hero_powers || (me.hero_power ? [me.hero_power] : []);
  for (const hp of powers) {
    const b = document.createElement("button");
    b.className = "hp-btn";
    const a = hp.action ?? A.HERO_POWER;
    const now = hp.discover_races && hp.discover_races.length ? `<br><span class="hp-now">本回合发现：<b>${hp.discover_races.map(raceName).join("/")}</b></span>` : "";
    b.innerHTML = `${itemArt(hp, 'power-art')}<span class="power-copy"><b>${esc(nm(hp))}</b><span class="power-price">${hp.passive ? '被动' : `${hp.price ?? '—'} 金币`}</span><span class="power-text">${esc(tx(hp))}${now}</span></span>`;
    b.classList.toggle('passive', !!hp.passive);
    b.disabled = !hp.passive && !legal(a);
    b.onclick = () => hp.passive ? showDetail(hp) : act(a);
    b.title = `${nm(hp)}：${tx(hp)}\n本回合使用 ${hp.uses_turn} 次 / 本局 ${hp.uses_game} 次  计数 ${hp.counter}`;
    b.setAttribute('aria-label', `${nm(hp)}，${hp.passive ? '被动技能，查看详情' : `${hp.price ?? '—'} 金币`}`);
    $("hp-slot").appendChild(b);
    {
      const mb = document.createElement("button");
      mb.className = 'mini-power';
      mb.classList.toggle('passive', !!hp.passive);
      mb.dataset.action = a;
      mb.innerHTML = `${itemArt(hp, 'power-art')}<span class="mini-copy"><b class="power-label">${esc(nm(hp)) || (hp.slot ? '技能2' : '英雄技能')}</b><span class="power-price">${hp.discover_races?.length ? hp.discover_races.map(raceName).join('/') + ' · ' : ''}${hp.passive ? '被动 · 详情' : `${hp.price ?? '—'} 金币`}</span></span>`;
      mb.title = nm(hp) + '：' + tx(hp);
      mb.setAttribute('aria-label', b.getAttribute('aria-label'));
      mb.disabled = b.disabled;
      mb.onclick = b.onclick;
      minis.push(mb);
    }
  }
  if (me.buddy_button) {
    const b = me.buddy_button, button = document.createElement("button");
    button.className = "hp-btn";
    button.textContent = b.claims >= 2 ? "伙伴已领取完毕" : `领取伙伴 ×${b.quantity} · ${b.price} 金币（${b.claims}/2）`;
    button.disabled = !b.available;
    button.title = "每场最终战斗胜利或平局减3，败北减2；领取后折扣清零";
    button.onclick = () => act({kind: "claim_buddy", expected_claims: b.claims});
    $("hp-slot").appendChild(button);
    const mini = document.createElement("button");
    mini.textContent = b.claims >= 2 ? "伙伴已领取" : `伙伴 ×${b.quantity} · ${b.price} 金`;
    mini.disabled = button.disabled;
    mini.onclick = button.onclick;
    minis.push(mini);
  }
  // Always-visible text uses the live view (including dynamic values), not catalog text.
  const details = new Map();
  const effectCard = (card, kind, extra = '') => {
    if (!card) return '';
    const key = String(details.size); details.set(key, card);
    return `<button class="effect-card" data-effect-detail="${key}">${itemArt(card, 'effect-art')}<span class="effect-copy"><span class="effect-kind">${esc(kind)}${extra}</span><b>${esc(nm(card))}</b><span class="effect-text">${esc(tx(card))}</span></span></button>`;
  };
  const effects = [];
  for (const t of me.trinkets || []) effects.push(effectCard(t, '已装备饰品'));
  for (const q of me.quests || []) effects.push(`<article class="quest-effect">${effectCard(q.quest, '任务', ` · ${q.progress}/${q.goal}`)}<progress max="${Math.max(1,q.goal)}" value="${q.progress}" aria-label="任务进度 ${q.progress}/${q.goal}"></progress>${effectCard(q.reward, '完成奖励')}</article>`);
  for (const t of me.secrets || []) effects.push(effectCard(t, '奥秘'));
  for (const t of me.rewards || []) effects.push(effectCard(t, '奖励'));
  if (me.buddy) effects.push(effectCard(me.buddy, '伙伴'));
  if (me.deity) effects.push(effectCard(me.deity.card, '神明', ` · ${fmtStat(me.deity.atk)}/${fmtStat(me.deity.hp)}${me.deity.golden ? ' 金色' : ''}`));
  for (const x of me.effects || []) effects.push(effectCard(x.card, '本局持续', x.stacks > 1 ? ` ×${x.stacks}` : ''));
  for (const x of me.scheduled || []) effects.push(effectCard(x.card, `倒计时 ${x.turns_left} 回合${x.if_won ? ' · 需获胜' : ''}`));
  for (const t of me.hand_queue || []) effects.push(effectCard(t, '等待手牌空位'));
  if (V.config.experimental_web) effects.push(`<div class="effect-note">实验规则 · 第 4 回合任务三选一 · 伙伴 · 时空扭曲<br>时空货币余额：${me.chronum_balance || 0}（回合结束失效）</div>`);
  $('info-strip').innerHTML = effects.join('') || '<p class="effects-empty">尚未获得饰品、任务或持续效果。</p>';
  $('effects-count').textContent = details.size ? `(${details.size})` : '';
  $('info-strip').querySelectorAll('[data-effect-detail]').forEach(b => b.onclick = () => showDetail(details.get(b.dataset.effectDetail)));
  renderEmotes();
  area.hidden = false;
  const ctl = $("ctl");
  const nCry = Math.min(10, Math.max(me.max_gold, me.gold));
  const crystals = Array.from({ length: nCry }, (_, i) => `<span class="${i < me.gold ? "" : "empty"}"></span>`).join("") + (me.gold > 10 ? ` <b>+${me.gold - 10}</b>` : "");
  const dg = me.dark_gift;
  const opp = me.next_is_ghost ? "幽灵（已淘汰玩家的战队）" : me.next_opponent !== null && me.next_opponent !== undefined ? esc(nm((V.opponents.find((o) => o.id === me.next_opponent) || {}).hero)) || "P" + me.next_opponent : "—";
  ctl.innerHTML = `
    <div class="next-opponent small">本回合对手：${opp}</div>
    <div id="hp-mini"></div>
    <div id="dg-slot"></div>
    <div class="turn-controls">
      <div class="timer" id="timer">--</div>
      <div id="end-slot" style="margin-top:4px"></div>
    </div>`;
  $('hero-economy').innerHTML = `<div class="gold"><span class="g-label">金币</span><b>${me.gold}</b><span class="gold-max"> / ${me.max_gold}</span></div><div class="crystals">${crystals}</div><div class="economy-note">随从 ${me.minion_price} 金${me.gold_next_turn ? ` · 下回合 +${me.gold_next_turn}` : ''}${me.max_gold_bonus ? ` · 上限 +${me.max_gold_bonus}` : ''}</div>`;
  for (const mb of minis) $("hp-mini").appendChild(mb);
  $('hp-mini').style.setProperty('--power-count', Math.max(1, minis.length));
  $('hp-mini').classList.toggle('multiple-powers', minis.length > 1);
  const g = document.createElement("button");
  g.style.width = "100%";
  const tiers = (t) => (t && t.length ? t.join("、") + " 级" : "—");
  g.className = 'dark-gift-button';
  const giftStatus = dg.uses >= dg.max_uses ? '次数用尽' : dg.used_this_turn ? '本回合已用' : V.turn < dg.unlock_turn ? `${dg.unlock_turn}回合解锁` : me.gold < dg.cost ? '金币不足' : !legal(A.DARK_GIFT) ? '暂不可用' : '点击选择';
  g.innerHTML = `${itemArt({id:'BG36_MidGameEffect_000t75'},'gift-art')}<span class="gift-copy gift-long"><b>黑暗之赐 <span class="power-price">${dg.cost} 金</span></b><span class="small">${dg.tiers && dg.tiers.length ? tiers(dg.tiers) + '随从' : `第 ${dg.unlock_turn} 回合解锁`}</span><span class="small">${dg.max_uses - dg.uses} 次可用${dg.used_this_turn ? ' · 本回合已用' : ''}</span></span><span class="gift-copy gift-short"><b>黑暗之赐</b><span class="power-price">${dg.cost} 金 · ${Math.max(0,dg.max_uses - dg.uses)} 次</span><span class="small">${giftStatus}</span></span>`;
  g.title = `花 ${dg.cost} 金，从 3 个带黑暗之赐的随从里选一个。\n本回合提供的随从等级：${tiers(dg.tiers)}；下回合：${tiers(dg.next_tiers)}。\n${(dg.type_slots || []).map((x) => `第 ${x[0]} 回合起至少 ${x[1]} 个`).join("，")}是你战队里数量最多的种族（或能配合它的中立随从）。\n同时提供 5 级和 6 级时，其中一个一定是 6 级。`;
  g.disabled = !legal(A.DARK_GIFT);
  g.onclick = () => act(A.DARK_GIFT);
  // Hidden when the rules turn the Dark Gift off (e.g. scenario S0).
  if (dg.max_uses > 0) $("dg-slot").appendChild(g);
  const e = document.createElement("button");
  e.className = "end-btn" + (endSent || me.ended_turn ? " sent" : "");
  e.style.width = "100%";
  e.textContent = endSent ? "已结束 · 结算中…" : me.ended_turn ? "已结束 · 等待中…" : "结束回合";
  e.title = "快捷键：空格";
  e.disabled = endSent || !legal(A.END);
  e.onclick = endTurn;
  $("end-slot").appendChild(e);
  if (S.repeatSpell) {
    const cancel = document.createElement("button"); cancel.className = "repeat-toggle";
    cancel.textContent = repeatSpellId ? "连续施法中 · 取消" : "连续施法已启用";
    cancel.onclick = () => { repeatSpellId = null; activeSpell = null; S.repeatSpell = false; saveS(); regionVersions.delete("hero"); render(); };
    ctl.appendChild(cancel);
  }
}

function renderPending() {
  const m = $("modal"), box = $("modal-box");
  if (V.spectator || lobbyOpen) { m.classList.remove("show"); return; }
  const p = V.me.pending;
  const banner = $("banner");
  banner.innerHTML = "";
  // The server has already entered recruit while the client plays the prior combat.
  // Do not expose or consume that live choice until the player returns to recruit.
  if (isReplaying() || lobbyOpen) { m.classList.remove("show"); return; }
  m.style.opacity = "";
  if (!p || p.kind === "target") {
    hiddenChoice = null;
    delete m.dataset.choice;
    m.classList.remove("show");
    if (p && p.kind === "target") {
      if (pointerController) pointerController.cancel();
      banner.innerHTML = `<div class="banner bar">请选择目标（绿色虚线高亮的卡牌：你的随从或酒馆随从）${V.me.cancellable ? '<button id="cancel-target">取消（Esc）</button>' : ""}</div>`;
      if (V.me.cancellable) $("cancel-target").onclick = () => act(V.actions.CANCEL);
    }
    if (!lobbyOpen && (V.phase === "game_over" || (!V.me.alive && V.me.placement))) showGameOver();
    return;
  }
  if (pointerController) pointerController.cancel();
  hidePreview();
  if (hiddenChoice === choiceKey()) { m.classList.remove("show"); return; }
  // Polls must not replace a choice under the player's pointer or keyboard focus.
  const choice = JSON.stringify([choiceKey(), V.me.gold, V.hero_rerolls_left, V.me.cancellable, S.art, S.lang]);
  m.style.opacity = '';
  if (m.dataset.choice === choice) { m.classList.add('show'); return; }
  m.dataset.choice = choice;
  const A = V.actions;
  let h = "";
  if (p.kind === "hero") h = `<h3>选择你的英雄</h3><div class="small hero-reroll-note">${V.hero_rerolls_left ? `剩余 ${V.hero_rerolls_left} 次更换 · 可替换一个候选英雄` : '本局没有英雄更换次数'}${V.game_settings?.mode === 'ranked' ? ' · 计分模式' : ''}</div><div class="races-line">${lobbyRacesHtml()}</div>`;
  else if (p.kind === "discover") h = `<h3>发现：${esc(nm(p.source))}</h3>`;
  else if (p.kind === "choose_one") h = `<h3>抉择：${esc(nm(p.source))}</h3><div class="small">${esc(tx(p.source))}</div>`;
  else if (p.kind === "trinket") h = `<h3>选择一个饰品购买（当前金币 ${V.me.gold}，必须先选择才能继续招募）</h3>`;
  else if (p.kind === "quest") h = "<h3>选择一个任务</h3>";
  else if (p.kind === "timewarp") h = `<h3>${p.major ? "大型" : "小型"}时空扭曲酒馆 · 时空货币 ${p.chronum}</h3><div class="small">用时空货币购买，不花金币；这里不能刷新、冻结或出售。买不起任何牌时自动离开。</div>`;
  else if (p.kind === "hero_power") h = `<h3>选择英雄技能（${p.slot ? "第二技能" : "替换当前技能"}）</h3>`;
  else h = "<h3>选择</h3>";
  box.innerHTML = h;
  const row = document.createElement("div");
  row.className = "choices";
  p.options.forEach((o, i) => {
    let el;
    if (p.kind === "hero") {
      el = document.createElement("div");
      el.className = "hero-choice";
      el.tabIndex = 0;
      el.setAttribute('role', 'button');
      el.setAttribute('aria-label', '选择英雄 ' + nm(o));
      el.innerHTML = `${heroArt({...o,skin_id:PUBLIC ? H?.collection?.skins[o.id] : null}, "hero-art")}<div class="hn" style="font-size:16px;font-weight:bold">${esc(nm(o))}</div><div class="small">护甲 ${o.armor}</div><hr><b>${esc(nm(o.hero_power))}</b>（${o.hero_power ? o.hero_power.cost : "-"} 金）<div>${esc(tx(o.hero_power))}</div>`;
      el.onclick = () => act(A.CHOOSE + i);
      el.onkeydown = e => { if (e.target === el && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); act(A.CHOOSE + i); } };
      if (V.hero_rerolls_left > 0) {
        const swap = document.createElement('button'); swap.className='hero-reroll'; swap.textContent='↻ 更换此英雄';
        swap.onclick=async e => {e.stopPropagation(); swap.disabled=true; const j=await api('/api/hero/reroll',{index:i,seed:V.seed,turn:V.turn,phase:V.phase}); if(j.ok) render(); else swap.disabled=false;};
        el.appendChild(swap);
      }
    } else if (p.kind === "quest") {
      el = document.createElement("div");
      el.className = "hero-choice";
      el.innerHTML = `<b>${esc(nm(o))}</b>（目标 ${o.goal}）<div class="small">${esc(tx(o))}</div><hr><b>奖励：${esc(nm(o.reward))}</b><div class="small">${esc(tx(o.reward))}</div>`;
      el.onclick = () => act(A.CHOOSE + i);
    } else if (p.kind === "hero_power") {
      el = document.createElement("div");
      el.className = "hero-choice";
      el.innerHTML = `${itemArt(o,'choice-power-art')}<b>${esc(nm(o))}</b>（${o.cost ?? "-"} 金）<div class="small">${esc(tx(o))}</div>`;
      el.onclick = () => act(A.CHOOSE + i);
    } else if (p.kind === "choose_one") {
      el = document.createElement("div");
      el.className = "hero-choice";
      el.innerHTML = `<b>选项 ${i + 1}</b><div class="small">${esc(tx(o))}</div>`;
      el.onclick = () => act(A.CHOOSE + i);
    } else if (p.kind === "timewarp") {
      const ok = legal(A.CHOOSE + i);
      el = cardEl(o, { price: o.chronum_cost, tag: "时空货币", onclick: () => (ok ? act(A.CHOOSE + i) : toast("时空货币不足")) });
      if (!ok) el.classList.add("disabled");
    } else if (p.kind === "trinket") {
      const ok = legal(A.CHOOSE + i);
      const off = o.base_cost !== undefined && o.base_cost !== o.cost ? `（原价 ${o.base_cost}）` : "";
      const w = o.weight !== undefined && o.weight !== 100 ? ` 权重 ${o.weight}%` : "";
      el = cardEl(o, { price: o.cost, tag: (o.kind === "trinket_greater" ? "大型" : "小型") + off + w, onclick: () => (ok ? act(A.CHOOSE + i) : toast("金币不足")) });
      if (!ok) el.classList.add("disabled");
    } else {
      el = cardEl(o, { onclick: () => act(A.CHOOSE + i) });
    }
    if (el.classList.contains("hero-choice")) {
      el.tabIndex = 0; el.setAttribute("role", "button");
      el.onkeydown = e => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); e.stopPropagation(); el.click(); } };
    }
    const gifts = (o.attached || []).filter(gift => gift.kind === 'dark_gift');
    if (p.kind === 'discover' && gifts.length) {
      const choice = document.createElement('div'); choice.className = 'gift-choice';
      const preview = document.createElement('div'); preview.className = 'gift-preview';
      preview.innerHTML = gifts.map(gift => `<div class="gift-preview-entry">${itemArt(gift, 'gift-preview-art')}<div><b>${esc(nm(gift))}</b><p>${esc(tx(gift))}</p></div></div>`).join('');
      choice.append(el, preview); row.appendChild(choice);
    } else row.appendChild(el);
  });
  box.appendChild(row);
  const foot = document.createElement("div");
  foot.className = "bar";
  foot.style.justifyContent = "center";
  if (p.kind === "timewarp") {
    const c = document.createElement("button");
    c.textContent = "离开时空扭曲酒馆" + (p.chronum ? `（剩下的 ${p.chronum} 个时空货币会作废）` : "");
    c.onclick = () => act(A.CHOOSE + p.leave);
    foot.appendChild(c);
  }
  if (V.me.cancellable) {
    const c = document.createElement("button");
    c.textContent = "取消（Esc）";
    c.onclick = () => act(A.CANCEL);
    foot.appendChild(c);
  }
  if (p.kind !== "hero") {
    // Look at the board behind the choice.
    const peek = document.createElement("button");
    peek.className = "ghost";
    peek.textContent = "按住查看战场";
    const hide = (on) => (m.style.opacity = on ? "0.08" : "");
    peek.onpointerdown = () => hide(true);
    peek.onpointerup = peek.onpointerleave = () => hide(false);
    foot.appendChild(peek);
  }
  box.appendChild(foot);
  dialogHeader("modal", "待完成的选择", () => { hiddenChoice = choiceKey(); m.classList.remove("show"); }, "收起");
  m.classList.add("show");
}

function showGameOver() {
  if (showGameOver.shown === V.seed + ":" + V.me.placement) return;
  showGameOver.shown = V.seed + ":" + V.me.placement;
  const m = $("modal"), box = $("modal-box");
  const rows = [{ id: V.seat, hero: V.me.hero, placement: V.me.placement, me: true, username: V.me.username }].concat(V.opponents).filter((x) => x.placement).sort((a, b) => a.placement - b.placement);
  const multi = !!V.table, over = V.phase === "game_over" || (multi && V.table.finished);
  box.innerHTML = `<div class="over-box"><div class="over-rank r${V.me.placement}">第 ${V.me.placement} 名</div><h3>${over ? "对局结束" : "你已被淘汰"}</h3>`
    + `<div class="over-list">${rows.map((r) => `<div class="over-row${r.me ? " me" : ""}"><span class="rk p${r.placement}">${r.placement}</span><span class="who">${r.username ? `<b>${esc(r.username)}</b>` : r.me ? "【你】" : "P" + r.id}</span><span class="hero">${esc(nm(r.hero))}</span></div>`).join("")}</div>`
    + (multi && !over ? `<div class="small">其他玩家还在打，你可以留下来看排名变化。</div>` : "")
    + `<div class="bar" style="margin-top:12px;justify-content:center"><button class="primary" id="go-lobby">${multi ? "离开房间，回大厅" : "返回大厅"}</button><button id="over-close">${multi && !over ? "留下观看" : "关闭"}</button></div></div>`;
  $("go-lobby").onclick = () => { m.classList.remove("show"); if (multi) leaveRoom(""); else showLobby(); };
  $("over-close").onclick = () => m.classList.remove("show");
  dialogHeader("modal", "对局结果");
  m.classList.add("show");
}

/** Is this log line about my seat? */
function isMine(line) {
  const body = line.replace(/^\[t\d+\] /, "");
  const me = "p" + V.seat;
  return new RegExp("(^|\\s|↳ )" + me + "(\\b|'s)").test(body);
}
/** Make my seat readable in log text. */
function meText(line) {
  const me = "p" + V.seat;
  return line.replace(new RegExp("\\b" + me + "'s\\b", "g"), "【你的】").replace(new RegExp("\\b" + me + "\\b", "g"), "【你】");
}
function renderLog() {
  const l = $("log");
  const onlyMine = $("log-mine").checked;
  const lines = (V.log_tail || []).filter((x) => !onlyMine || x.includes("===") || isMine(x));
  l.innerHTML = lines.map((x) => `<div class="${x.includes("===") ? "turn" : isMine(x) ? "mine" : ""}${x.includes("↳") ? " stat" : ""}">${esc(meText(x))}</div>`).join("");
  if ($("log-follow").checked) l.scrollTop = l.scrollHeight;
  renderTurnNotice();
}
let noticeDismissed = -1;
let noticeShownTurn = -1, noticeTimer = null;
function clearTurnNotice() {
  clearTimeout(noticeTimer);
  noticeTimer = null;
  noticeShownTurn = -1;
  $("turn-notice").innerHTML = "";
}
function renderTurnNotice() {
  const box = $("turn-notice");
  // turn_events holds only my own start-of-turn lines (captured per player by the engine).
  const interesting = V.me.turn_events || [];
  if (!interesting.length || noticeDismissed === V.turn || V.phase !== "recruit") { clearTurnNotice(); return; }
  // Polling must not restart the one-second lifetime or re-open this turn's notice.
  if (noticeShownTurn === V.turn) return;
  clearTurnNotice();
  const turn = V.turn;
  noticeShownTurn = turn;
  box.innerHTML = `<div class="notice"><div class="bar"><b>第 ${V.turn} 回合开始时发生了：</b><button id="notice-close" style="margin-left:auto">知道了</button></div>${interesting.map((x) => `<div class="ln">${esc(meText(x.replace(/^\[t\d+\] /, "")))}</div>`).join("")}</div>`;
  const dismiss = () => { noticeDismissed = turn; clearTurnNotice(); };
  $("notice-close").onclick = dismiss;
  noticeTimer = setTimeout(dismiss, 1000);
}
document.addEventListener("change", (e) => { if (e.target && e.target.id === "log-mine") renderLog(); });

// ------------------------------------------------------------------ opponents
function showOpponent(id) {
  const o = V.opponents.find((x) => x.id === id);
  const m = $("opp-modal"), box = $("opp-box");
  const extra = (o.quests || []).map((q) => `<span class="pill" title="${esc(tx(q.reward))}">${esc(nm(q.quest))} ${q.progress}/${q.goal} → ${esc(nm(q.reward))}</span>`).join("")
    + (o.rewards || []).map((r) => `<span class="pill" title="${esc(tx(r))}">${esc(nm(r))}</span>`).join("")
    + (o.buddy_claims === null || o.buddy_claims === undefined ? "" : `<span class="pill">伙伴领取 ${o.buddy_claims}/2</span>`);
  if (PUBLIC) {
    // Only what a player may know: public stats and the warband of the last combat round.
    box.innerHTML = `<h3>${o.username ? esc(o.username) + " ·" : "P" + o.id} ${esc(nm(o.hero))} · ${o.tier} 本 · 生命 ${o.health}${o.armor ? " 护甲 " + o.armor : ""}${o.alive ? "" : " · 第" + o.placement + "名"}</h3>
      <div class="small">${extra}</div><div class="opp-view"><div class="zone-title">最近一轮战斗时的战队</div><div class="row" id="opp-last" data-empty="还没有交过手"></div></div>
      <div class="bar" style="margin-top:8px"><button onclick="document.getElementById('opp-modal').classList.remove('show')">关闭</button></div>`;
    o.last_board.forEach((c) => $("opp-last").appendChild(cardEl(c)));
    if (!o.last_board.length) $("opp-last").classList.add("empty");
    dialogHeader("opp-modal", "对手信息");
    m.classList.add("show");
    return;
  }
  box.innerHTML = `<h3>P${o.id} ${esc(nm(o.hero))} · ${o.tier} 本 · 生命 ${o.health}${o.armor ? " 护甲 " + o.armor : ""}${o.alive ? "" : " · 第" + o.placement + "名"}</h3>
    <div class="small">${extra}<br>手牌 ${o.hand_size} 张　饰品：${o.trinkets.map((t) => esc(nm(t))).join("、") || "无"}</div>
    <div class="opp-view"><div class="zone-title">最后一次看到的战队（上回合结束时）</div><div class="row" id="opp-last"></div>
    <div class="zone-title">当前真实战队（调试可见）</div><div class="row" id="opp-real"></div></div>
    <div class="bar" style="margin-top:8px">
      <button id="opp-prev">战斗预演：我 vs P${o.id}</button>
      <button id="opp-edit">在调试面板中编辑此玩家</button>
      <button onclick="document.getElementById('opp-modal').classList.remove('show')">关闭</button>
    </div><div id="opp-prev-out" class="small"></div>`;
  o.last_board.forEach((c) => $("opp-last").appendChild(cardEl(c)));
  o.board.forEach((c, i) => $("opp-real").appendChild(cardEl(c, { onclick: () => { dbg.p = o.id; dbg.sel = { p: o.id, zone: "board", index: i }; renderDebug(); toast(`已选中 P${o.id} 的随从用于调试`); } })));
  $("opp-prev").onclick = () => runPreview(V.seat, o.id, $("opp-prev-out"));
  $("opp-edit").onclick = () => { dbg.p = o.id; dbg.sel = null; m.classList.remove("show"); renderDebug(); };
  dialogHeader("opp-modal", "对手信息");
  m.classList.add("show");
}

async function runPreview(a, b, out) {
  const n = parseInt(($("dbg-prev-n") || {}).value || "300");
  const j = await debugCmd({ cmd: "combat_preview", a, b, n });
  if (!j.ok) return;
  const r = j.result;
  out.innerHTML = `模拟 ${r.n} 场：P${a} 胜 <b>${(r.win_a * 100).toFixed(1)}%</b>（平均伤害 ${r.avg_damage_by_a.toFixed(1)}），平 ${(r.tie * 100).toFixed(1)}%，P${b} 胜 <b>${(r.win_b * 100).toFixed(1)}%</b>（平均伤害 ${r.avg_damage_by_b.toFixed(1)}） <button id="prev-replay">查看一场样例回放</button>`;
  $("prev-replay").onclick = () => showCombat(r.sample, true);
}

// ------------------------------------------------------------------ drag and drop
function makeDraggable(el, zone, index, card) {
  el.draggable = false;
  el.dataset.zone = zone;
  el.dataset.index = index;
  el.dataset.id = zone === "shop" ? card.sid : zone === "hand" ? card.hid : card.eid;
}
function endDrag() {
  drag = null;
  $("ins-marker").style.display = "none";
  $("target-line").classList.remove("show");
  $("interaction-hint").textContent = "";
  document.body.classList.remove("interacting", "selling-drag");
  document.querySelectorAll(".drop-ok,.drop-target,.preview-shift").forEach(el => {
    el.classList.remove("drop-ok", "drop-target", "preview-shift");
    el.style.removeProperty("--preview-shift");
    el.style.removeProperty("translate");
  });
}
const dragIsMinion = () => drag && drag.card.type === "minion";
const dragIsSpell = () => drag && drag.zone === "hand" && drag.card.type === "spell";
/** A Magnetic dropped immediately left of a legal host attaches, including on a full board. */
function placementAction(a, hand, pos, boardCount, isLegal, standalone = false) {
  if (!standalone && pos < boardCount) {
    const magnet = a.MAGNETIZE + hand * a.N_BOARD + pos;
    if (isLegal(magnet)) return magnet;
  }
  const play = a.PLAY + hand * a.N_POS + pos;
  return isLegal(play) ? play : null;
}
// END PLACEMENT
function handPlayAction(i, pos) {
  return i < 10 ? V.actions.PLAY + i * V.actions.N_POS + pos : {kind: "play", hid: V.me.hand[i].hid, pos};
}
function handMagnetizeAction(i, board) {
  return i < 10 ? V.actions.MAGNETIZE + i * V.actions.N_BOARD + board : {kind: "magnetize", hid: V.me.hand[i].hid, board: V.me.board[board].eid};
}
function canHandPlay(i, pos) {
  if (i === null || !V.me.hand[i]) return false;
  return !V.spectator && !connection.uncertain && (i < 10 ? legal(handPlayAction(i, pos)) : (V.me.hand[i].web_play_positions || []).includes(pos));
}
function canHandMagnetize(i, board) {
  if (i === null || !V.me.hand[i]) return false;
  return !V.spectator && !connection.uncertain && (i < 10 ? legal(handMagnetizeAction(i, board)) : (V.me.hand[i].web_magnetize || []).includes(board));
}
function handPlacement(i, pos, standalone = false) {
  if (i === null) return null;
  if (!standalone && pos < V.me.board.length && canHandMagnetize(i, pos)) return handMagnetizeAction(i, pos);
  return canHandPlay(i, pos) ? handPlayAction(i, pos) : null;
}

function insertPos(x) {
  const cards = Array.from($("board").querySelectorAll(".card"));
  let pos = 0;
  for (const c of cards) { const r = c.getBoundingClientRect(); if (x > r.left + r.width / 2) pos++; }
  return { pos, cards };
}
function showMarker(x) {
  const { pos, cards } = insertPos(x);
  const mk = $("ins-marker"), row = $("board").getBoundingClientRect();
  let left = row.left + row.width / 2, top = row.top + 10, height = row.height - 20;
  if (cards.length) {
    const r = cards[Math.min(pos, cards.length - 1)].getBoundingClientRect();
    left = pos < cards.length ? r.left - 5 : r.right + 2; top = r.top; height = r.height;
  }
  Object.assign(mk.style, { display: "block", left: left + "px", top: top + "px", height: height + "px" });
  return pos;
}
async function castOn(i, id) {
  const card = V && V.me.hand[i];
  if (!card || !canHandPlay(i, 0)) return toast("现在不能施放这张牌");
  const result = await act(handPlayAction(i, 0));
  if (!result || result.ok === false || !V) return;
  const targets = pendingTargets();
  if (targets && id !== undefined) {
    const k = targets.indexOf(id);
    if (k >= 0) await act(V.actions.CHOOSE + k);
    else toast("请选择高亮的合法目标", "info");
  }
}
function draggedIndex() {
  if (!drag || !V || drag.seed !== V.seed || drag.turn !== V.turn) return -1;
  const key = drag.zone === "shop" ? "sid" : drag.zone === "hand" ? "hid" : "eid";
  return V.me[drag.zone].findIndex(c => String(c[key]) === String(drag.id));
}
function sellRegionBounds() {
  const arena = $('arena').getBoundingClientRect();
  const cards = [...$('shop').querySelectorAll('.card')].map(el => el.getBoundingClientRect()).filter(r => r.width && r.height);
  const bottom = cards.length ? Math.min(...cards.map(r => r.top)) : $('shop').getBoundingClientRect().top;
  return {left:arena.left, right:arena.right, top:arena.top, bottom};
}
function dropCandidate(x, y, event) {
  const index = draggedIndex();
  if (index < 0) return { text: "局面已变化，请重新选择" };
  const hit = document.elementFromPoint(x, y), A = V.actions;
  if (!hit) return { text: "松开取消" };
  const card = hit.closest("#board .card,#shop .card"), host = card && Number(card.dataset.id);
  const sell = drag.zone === 'board' ? sellRegionBounds() : null;
  const aboveShop = sell && hit.closest('#arena') && !hit.closest('.modal') && x >= sell.left && x <= sell.right && y >= sell.top && y < sell.bottom;
  if (drag.zone === "board" && (aboveShop || hit.closest("#tavern-host"))) {
    const action = A.SELL + index;
    const value = drag.card.sell_price;
    return { action: legal(action) ? action : null, element: $("sell-zone"), text: legal(action) ? `出售 ${nm(drag.card)}${value != null ? ` · 获得 ${value} 金币` : " · 结算出售效果"}` : "现在不能出售" };
  }
  if (drag.zone === "shop" && hit.closest("#hand-zone,#board-zone,#bottom")) {
    const action = A.BUY + index;
    return { action: legal(action) ? action : null, element: $("hand-zone"), text: legal(action) ? `购买 ${nm(drag.card)} · ${drag.card.price} 金币` : "无法购买：金币不足或手牌已满" };
  }
  if (dragIsSpell() && hit.closest("#board-zone,#tavern-zone")) {
    const action = canHandPlay(index, 0) ? handPlayAction(index, 0) : null;
    return { spell: true, index, target: host, element: card || $("board-zone"), action, text: action !== null ? `施放 ${nm(drag.card)}${card ? " → " + nm(card._card) : ""}` : "现在不能施放" };
  }
  if (hit.closest("#board-zone")) {
    const { pos, cards } = insertPos(x);
    if (drag.zone === "board") {
      const to = pos > index ? pos - 1 : pos;
      const action = A.MOVE + index * A.N_BOARD + to;
      return { action: to !== index && legal(action) ? action : null, pos, cards, text: to === index ? "保持当前位置" : `移动到第 ${to + 1} 位` };
    }
    if (drag.zone === "hand" && dragIsMinion()) {
      const targetIndex = card && card.dataset.zone === "board" ? Number(card.dataset.index) : pos;
      const magnet = targetIndex < V.me.board.length ? handMagnetizeAction(index, targetIndex) : null;
      if (!(event && event.altKey) && targetIndex < V.me.board.length && canHandMagnetize(index, targetIndex)) {
        return { action: magnet, element: cards[targetIndex], text: `磁力吸附 → ${nm(V.me.board[targetIndex])} · Alt 单独打出` };
      }
      const action = handPlayAction(index, pos);
      return { action: canHandPlay(index, pos) ? action : null, pos, cards, text: canHandPlay(index, pos) ? `打出 ${nm(drag.card)} · 第 ${pos + 1} 位` : "无法打出：战队已满或卡牌被锁定" };
    }
  }
  return { text: drag.zone === "board" ? "拖到战队调整站位，拖到商店卡牌上方出售" : "松开取消" };
}
function pointerPreview(detail) {
  if (!drag) return;
  document.body.classList.toggle('selling-drag', drag.zone === 'board');
  if (drag.zone === 'board') {
    const bounds = sellRegionBounds();
    $('sell-zone').style.height = Math.max(0, bounds.bottom - bounds.top) + 'px';
  }
  document.querySelectorAll(".drop-ok,.drop-target,.preview-shift").forEach(el => {
    el.classList.remove("drop-ok", "drop-target", "preview-shift"); el.style.removeProperty("translate");
  });
  $("ins-marker").style.display = "none";
  const result = dropCandidate(detail.x, detail.y, detail.event);
  $("interaction-hint").textContent = result.text;
  $("interaction-hint").classList.toggle("invalid", result.action == null);
  if (result.action != null) {
    if (result.element) result.element.classList.add(result.element.classList.contains("card") ? "drop-target" : "drop-ok");
    else if (result.pos !== undefined) {
      showMarker(detail.x);
      const sourceIndex = draggedIndex();
      (result.cards || []).forEach((el, i) => {
        if (drag.zone === "board" && i === sourceIndex) return;
        let shift = i >= result.pos ? 8 : -8;
        if (drag.zone === "board") shift = i >= Math.min(sourceIndex, result.pos) && i < Math.max(sourceIndex + 1, result.pos) ? (result.pos > sourceIndex ? -12 : 12) : 0;
        el.classList.add("preview-shift"); el.style.translate = shift + "px 0";
      });
    }
  }
  if (dragIsSpell()) {
    const line = $("target-line"), start = drag.element.getBoundingClientRect();
    line.setAttribute("viewBox", `0 0 ${innerWidth} ${innerHeight}`);
    const sx = start.left + start.width / 2, sy = start.top + start.height / 2;
    line.querySelector("path").setAttribute("d", `M ${sx} ${sy} Q ${sx} ${detail.y} ${detail.x} ${detail.y}`);
    line.classList.add("show");
  }
}
function setupDropZones() {
  pointerController = BGEngine.createPointerDrag({
    root: $("arena"),
    canStart: detail => !!(V && !V.spectator && !connection.uncertain && V.phase === "recruit" && V.me.alive && !V.me.ended_turn && !V.me.pending && !lobbyOpen && !document.querySelector(".modal.show") && !detail.element._card.locked),
    onStart: detail => {
      const list = V.me[detail.zone], card = list && list[detail.index];
      if (!card) return false;
      hidePreview(); repeatSpellId = null;
      drag = { zone: detail.zone, index: detail.index, id: detail.id, card, element: detail.element, seed: V.seed, turn: V.turn };
      document.body.classList.add("interacting"); pointerPreview(detail);
    },
    onMove: pointerPreview,
    onDrop: detail => {
      const candidate = dropCandidate(detail.x, detail.y, detail.event);
      endDrag();
      if (candidate.action == null) return;
      if (candidate.spell) castOn(candidate.index, candidate.target); else act(candidate.action);
    },
    onCancel: endDrag
  });
}

// ------------------------------------------------------------------ combat replay
function isReplaying() { return !!replayState && $("combat-modal").classList.contains("show"); }
function setCombatScene(active) {
  document.body.classList.toggle("replaying", active);
  for (const child of $("arena").children) {
    if (!["combat-modal", "interaction-hint", "command-status", "target-line"].includes(child.id)) child.inert = active;
  }
  $("debug").inert = active;
  if (active) { $("modal").classList.remove("show"); $("banner").replaceChildren(); $("ai-watch").classList.remove("show"); }
}
/** The most recent combat involving my seat (highest turn). */
function myLatestCombat() {
  return (V.combats || []).filter((c) => c.a === V.seat || c.b === V.seat).sort((x, y) => y.turn - x.turn)[0];
}
function maybeShowCombat() {
  const mine = myLatestCombat();
  if (!mine || mine.turn === lastCombatTurnShown || !S.autoReplay || lobbyOpen) { if (mine) {lastCombatTurnShown = mine.turn;if(!lobbyOpen && !$('combat-modal').classList.contains('show')) showCombatDamage(mine);} return; }
  lastCombatTurnShown = mine.turn;
  showCombat(mine, false, { automatic: true });
}

let combatRun = 0;
function showCombat(rec, preview = false, { automatic = false } = {}) {
  if (!rec || !rec.frames || !rec.frames.length) return toast("没有战斗记录");
  if (showCombat.close) showCombat.close(false);
  const latest = myLatestCombat();
  if (!preview && latest && latest.turn === rec.turn) lastCombatTurnShown = rec.turn;
  hidePreview(); hideActionDock();
  if (pointerController) pointerController.cancel();
  const m = $("combat-modal"), box = $("combat-box"), run = ++combatRun;
  replayState = { seed: V.seed, seat: V.seat, turn: rec.turn, liveTurn: V.turn, livePhase: V.phase, alive: V.me.alive, left: !!(V.table && V.table.left), preview, automatic };
  const frames = rec.frames, n = frames.length, replaySeed = V.seed, seat = V.seat, mySide = rec.a === seat ? 0 : 1;
  const oppId = mySide === 0 ? rec.b : rec.a, opp = V.opponents.find(o => o.id === oppId) || {};
  const playerHero = V.me.hero;
  const oppName = oppId == null ? "幽灵" : (opp.username ? opp.username + " · " : "") + (nm(opp.hero) || "P" + oppId);
  const portrait = (hero, name, cls) => `<div class="cb-portrait ${cls}">${S.art && hero ? `<img src="${hero.skin_id ? "/assets/skins/" + encodeURIComponent(hero.skin_id) + ".webp" : cardArtUrl(hero.id)}" alt="">` : ""}<span class="cb-portrait-name">${esc(name)}</span></div>`;
  box.innerHTML = `<h3>第 ${rec.turn} 回合${preview ? " · 战斗预演" : " · 战斗回放"}</h3>
    ${portrait(opp.hero, oppName, "cb-opponent")}
    <div class="cb-board" id="cb-top"></div>
    <div class="cb-result" id="cb-result" role="status"></div>
    <div class="cb-board" id="cb-bot"></div>
    ${portrait(playerHero, nm(playerHero) || "你的英雄", "cb-player")}
    <div class="cb-controls bar">
      <button id="cb-first" title="回到开头">|◀</button><button id="cb-prev">◀ 上一步</button><button id="cb-play" class="primary"></button><button id="cb-next">下一步 ▶</button><button id="cb-last">结果 ▶|</button>
      <label>速度 <select id="cb-speed"><option>0.5</option><option>1</option><option>2</option><option>4</option></select></label>
      <label><input type="checkbox" id="cb-anim"> 动画</label><span id="cb-live-status" class="small"></span>
    </div>
    <div class="cb-timeline"><input id="cb-seek" type="range" min="0" max="${n - 1}" value="0" aria-label="战斗回放进度"><span class="small" id="cb-step"></span></div>
    <details class="cb-events"><summary>战斗事件</summary><div class="cb-log" id="cb-log"></div></details>`;
  box.querySelectorAll(".cb-portrait img").forEach(img => { img.onerror = e => { e.currentTarget.hidden = true; }; });
  let i = 0, playing = false, speed = S.animSpeed || 1, motionEpoch = 0, loopEpoch = 0, motion = null, busy = false;
  let heroResolved = false, finishTimer = null;
  const live = () => run === combatRun && m.classList.contains("show");
  const useMotion = () => S.anim && !BGEngine.reducedMotion() && !document.hidden;
  const outcome = () => rec.winner === 2 ? (rec.truncated ? "平局（模拟器截断）" : "平局") : ((rec.winner === 0) === (mySide === 0) ? `胜利 · 造成 ${rec.damage} 点伤害` : `落败 · 受到 ${rec.damage} 点伤害`);
  const cancelMotion = () => {
    motionEpoch++; busy = false; clearTimeout(finishTimer); finishTimer = null;
    BGEngine.cancelAnimations();
    if (motion) { motion.cancel(); motion = null; }
    box.querySelectorAll(".attacking,.drop-target,.hero-attacking,.hero-hit").forEach(el => el.classList.remove("attacking", "drop-target", "hero-attacking", "hero-hit"));
  };
  const drawFrame = (k, previous) => {
    const frame = frames[k], fx = [], epoch = motionEpoch;
    for (const [side, host] of [[1 - mySide, $("cb-top")], [mySide, $("cb-bot")]]) {
      const current = frame.boards[side], old = previous == null ? null : frames[previous].boards[side];
      const nodes = current.map(card => {
        const el = cardEl(card, { key: "c" + card.eid });
        const before = old && old.find(x => x.eid === card.eid);
        if (old && !before && useMotion()) el.classList.add("pop");
        if (before && useMotion()) {
          const maxBefore = before.max_hp ?? before.hp, maxNow = card.max_hp ?? card.hp;
          const da = card.atk - before.atk, dh = maxNow - maxBefore;
          const damage = Math.max(0, before.hp + dh - card.hp);
          if (damage) fx.push([card.eid, "−" + damage, "dmg", 0]);
          if (da || dh) fx.push([card.eid, `${signed(da)}/${signed(dh)}`, "buff", -24]);
          const hadShield = (before.keywords || []).includes("DIVINE_SHIELD"), hasShield = (card.keywords || []).includes("DIVINE_SHIELD");
          if (hadShield !== hasShield) fx.push([card.eid, hasShield ? "获得圣盾" : "圣盾破裂", "info", 24]);
          const gained = (card.keywords || []).filter(x => x !== "DIVINE_SHIELD" && !(before.keywords || []).includes(x));
          if (gained.length) fx.push([card.eid, gained.map(kwName).join(" · "), "info", 42]);
        }
        return el;
      });
      // Deaths leave a short overlay, never a live hit target or an extra lineup slot.
      if (old && useMotion()) for (const dead of old.filter(x => !current.some(c => c.eid === x.eid))) {
        const oldEl = keyEl(host, "c" + dead.eid);
        if (!oldEl) continue;
        const rect = oldEl.getBoundingClientRect(), ghost = oldEl.cloneNode(true);
        ghost.classList.add("dying", "replay-death");
        Object.assign(ghost.style, { position:"fixed", left:rect.left+"px", top:rect.top+"px", width:rect.width+"px", height:rect.height+"px", pointerEvents:"none" });
        ghost.addEventListener("animationend", () => ghost.remove()); $("fx").appendChild(ghost);
        setTimeout(() => ghost.remove(), 700);
      }
      BGEngine.reconcile(host, nodes, { animate: useMotion() && previous != null });
    }
    requestAnimationFrame(() => { if (live() && epoch === motionEpoch && i === k) fx.forEach(([id,...rest]) => floater(keyEl(box,"c"+id), ...rest)); });
    $("cb-log").textContent = (frame.log || []).map(line => meText(line).replace(/\bp(\d+)\b/g, (text, id) => Number(id) === oppId ? "【对手】" : text)).join("\n") || "（无事件）";
  };
  const hud = () => {
    $("cb-play").textContent = playing ? "暂停" : "▶ 播放";
    $("cb-step").textContent = `${i + 1} / ${n}${i === 0 ? " · 战斗开始前" : ""}`;
    $("cb-result").textContent = i === n - 1 && heroResolved ? outcome() : i === n - 1 ? "随从战斗结束 · 英雄结算" : "战斗进行中";
    $("cb-result").classList.toggle("hero-resolved", i === n - 1 && heroResolved);
    $("cb-speed").value = String(speed); $("cb-anim").checked = S.anim; $("cb-seek").value = i;
    $("cb-first").disabled = $("cb-prev").disabled = i === 0;
    $("cb-next").disabled = $("cb-last").disabled = i === n - 1 && heroResolved;
  };
  const lunge = async (attacker, defender) => {
    const source = keyEl(box, "c" + attacker), target = keyEl(box, "c" + defender);
    if (!source || !target) return;
    const a = source.getBoundingClientRect(), b = target.getBoundingClientRect();
    source.classList.add("attacking"); target.classList.add("drop-target");
    const dx = b.left + b.width/2 - a.left - a.width/2, dy = b.top + b.height/2 - a.top - a.height/2;
    const animation = source.animate([{transform:"translate(0,0)"},{transform:`translate(${dx*.72}px,${dy*.72}px) scale(1.06)`,offset:.55},{transform:"translate(0,0)"}], {duration:380/speed,easing:"ease-in-out"});
    motion = animation;
    try { await animation.finished; } catch (_) { /* seek/close cancels the current attack */ }
    if (motion === animation) { motion = null; source.classList.remove("attacking"); target.classList.remove("drop-target"); }
  };
  const settleHeroes = async () => {
    if (heroResolved || !live()) return;
    const epoch = motionEpoch;
    if (rec.winner !== 2 && rec.damage > 0) {
      const won = rec.winner === mySide;
      const source = box.querySelector(won ? ".cb-player" : ".cb-opponent"), target = box.querySelector(won ? ".cb-opponent" : ".cb-player");
      if (useMotion()) {
        const a = source.getBoundingClientRect(), b = target.getBoundingClientRect();
        const dx = b.left + b.width / 2 - a.left - a.width / 2, dy = b.top + b.height / 2 - a.top - a.height / 2;
        source.classList.add("hero-attacking");
        const animation = source.animate([{ transform: "translate(0,0)" }, { transform: `translate(${dx * .9}px,${dy * .9}px) scale(1.12)`, offset: .52 }, { transform: "translate(0,0)" }], { duration: 620 / speed, easing: "cubic-bezier(.3,0,.35,1)" });
        const finished = animation.finished.catch(() => {});
        motion = animation;
        await sleep(310 / speed);
        if (live() && epoch === motionEpoch && motion === animation) {
          target.classList.add("hero-hit"); floater(target, `−${rec.damage}`, "dmg");
        }
        await finished;
        if (motion === animation) { motion = null; source.classList.remove("hero-attacking"); target.classList.remove("hero-hit"); }
      }
      if (!live() || epoch !== motionEpoch) return;
      target.dataset.damage = String(rec.damage); target.setAttribute("aria-label", `受到 ${rec.damage} 点英雄伤害`);
    }
    if (live() && epoch === motionEpoch) { heroResolved = true; hud(); }
  };
  const advance = async () => {
    if (busy || !live() || (i >= n - 1 && heroResolved)) return;
    const epoch = motionEpoch; busy = true;
    if (i < n - 1) {
      if (useMotion()) for (const [attacker, defender] of frames[i + 1].attacks || []) { if (!live() || epoch !== motionEpoch) break; await lunge(attacker, defender); }
      if (live() && epoch === motionEpoch) { const old = i; i++; drawFrame(i, old); hud(); }
    }
    if (live() && epoch === motionEpoch && i === n - 1) await settleHeroes();
    if (epoch === motionEpoch) busy = false;
  };
  const pause = () => { playing = false; loopEpoch++; cancelMotion(); hud(); };
  const jump = k => {
    pause(); $("fx").replaceChildren(); i = Math.max(0, Math.min(n - 1, k)); heroResolved = i === n - 1;
    box.querySelectorAll(".cb-portrait").forEach(el => { delete el.dataset.damage; el.removeAttribute("aria-label"); });
    drawFrame(i, null);
    if (heroResolved && rec.winner !== 2 && rec.damage > 0) {
      const target = box.querySelector(rec.winner === mySide ? ".cb-opponent" : ".cb-player"); target.dataset.damage = String(rec.damage); target.setAttribute("aria-label", `受到 ${rec.damage} 点英雄伤害`);
    }
    hud();
  };
  const play = async () => {
    if (playing) return pause();
    if (i >= n - 1 && heroResolved) {
      i = 0; heroResolved = false;
      box.querySelectorAll(".cb-portrait").forEach(el => { delete el.dataset.damage; el.removeAttribute("aria-label"); }); drawFrame(i, null);
    }
    playing = true; const token = ++loopEpoch; hud();
    while (live() && playing && token === loopEpoch && (i < n - 1 || !heroResolved)) {
      await sleep((useMotion() ? 360 : 500) / speed);
      if (!live() || !playing || token !== loopEpoch) break;
      await advance();
    }
    if (live() && token === loopEpoch) {
      playing = false; hud();
      if (automatic && i === n - 1 && heroResolved) finishTimer = setTimeout(() => { if (live() && token === loopEpoch && !playing) close(true); }, 900 / speed);
    }
  };
  const close = (resume = true) => {
    if (run !== combatRun) return;
    cancelMotion(); loopEpoch++; playing = false; combatRun++;
    m.classList.remove("show"); replayState = null; setCombatScene(false); $("fx").replaceChildren();
    document.removeEventListener("visibilitychange", visibility);
    lastTimerTick = performance.now();
    if (!preview && !heroResolved) showCombatDamage(rec);
    if (!preview && V && V.seed === replaySeed && V.seat === seat) { const key = combatKey(V, rec); scoreboardFx.seen.add(key); if (scoreboardFx.damage?.key === key) scoreboardFx.damage = null; }
    animateScoreboard();
    if (resume && V && !lobbyOpen) { regionVersions.delete("top"); render(); }
  };
  const visibility = () => { if (document.hidden && live()) pause(); };
  document.addEventListener("visibilitychange", visibility);
  $("cb-first").onclick = () => jump(0); $("cb-prev").onclick = () => jump(i - 1);
  $("cb-next").onclick = async () => { pause(); await advance(); };
  $("cb-last").onclick = () => jump(n - 1); $("cb-play").onclick = play;
  $("cb-seek").oninput = e => jump(Number(e.target.value));
  $("cb-speed").onchange = e => { speed = Number(e.target.value); S.animSpeed = speed; saveS(); };
  $("cb-anim").onchange = e => { S.anim = e.target.checked; cancelMotion(); saveS(); drawFrame(i, null); };
  showCombat.close = close;
  dialogHeader("combat-modal", `第 ${rec.turn} 回合${preview ? " · 战斗预演" : " · 战斗回放"}`, () => close(true), "返回招募");
  m.querySelector(".dialog-dismiss").id = "cb-close";
  m.classList.add("show"); setCombatScene(true); renderTop();
  drawFrame(0, null); hud(); play();
}

function dismissCombat(resume = false) {
  if (isReplaying() && showCombat.close) { showCombat.close(resume); return; }
  $("combat-modal").classList.remove("show");
  replayState = null; setCombatScene(false);
}

// ------------------------------------------------------------------ timer
function turnSeconds(turn) { return timer.perTurn || Math.min(90, 40 + 5 * turn); }
function syncTimer() {
  if (!V) return;
  const key = JSON.stringify([V.seed, V.seat, V.room && V.room.id, V.turn]);
  if (key !== timer.key) { timer.key = key; timer.turn = V.turn; timer.left = turnSeconds(V.turn); lastTimerTick = performance.now(); }
}
function timerActive() { return V && !V.table && !(PUBLIC && V.clock) && !V.spectator && timer.enabled && !timer.paused && !(V.ai && V.ai.spectate) && V.phase === "recruit" && V.me.alive && !V.me.ended_turn && !lobbyOpen && !isReplaying(); }
let lastTimerTick = performance.now();
setInterval(async () => {
  const now = performance.now(), elapsed = Math.max(0, (now - lastTimerTick) / 1000); lastTimerTick = now;
  const el = $("timer");
  if (!el || !V) return;
  if (V.table || (PUBLIC && V.clock)) {
    // Public games run on the server's clock: it ends the turn of whoever is late.
    const left = beat.deadlineAt === null ? null : Math.max(0, (beat.deadlineAt - performance.now()) / 1000);
    const quiet = left === null || (V.table && V.table.timer === "off" && left > 60);
    el.textContent = (V.table ? V.table.finished : !V.me.alive || V.phase === "game_over") ? "已结束" : quiet ? "不限时" : `计时 ${Math.floor(left / 60)}:${String(Math.floor(left % 60)).padStart(2, "0")}`;
    el.classList.toggle("low", !quiet && left < 10 && !V.me.ended_turn);
    const liveStatus = $("cb-live-status");
    if (liveStatus) liveStatus.textContent = V.me.ended_turn ? "已结束回合" : quiet ? "在线招募 · 不限时" : `在线招募剩余 ${Math.ceil(left)} 秒`;
    if (isReplaying() && !quiet && left <= 5 && V.me.alive && !V.me.ended_turn) { dismissCombat(true); toast("在线招募即将结束，已返回当前回合", "info"); }
    return;
  }
  if (timerActive()) timer.left -= elapsed * timer.speed;
  const s = Math.max(0, timer.left);
  el.textContent = !timer.enabled ? "计时 关" : `计时 ${Math.floor(s / 60)}:${String(Math.floor(s % 60)).padStart(2, "0")}${timer.paused || isReplaying() ? " (暂停)" : ""}${timer.speed !== 1 ? " ×" + timer.speed : ""}`;
  el.classList.toggle("low", s < 10);
  const liveStatus = $("cb-live-status");
  if (liveStatus) liveStatus.textContent = !timer.enabled ? "招募计时关闭" : isReplaying() ? "招募计时已暂停" : `招募剩余 ${Math.ceil(s)} 秒`;
  if (timerActive() && timer.left <= 0 && !timer.firing) {
    timer.firing = true;
    toast("时间到！自动结束回合");
    await api("/api/timeout", {});
    timer.firing = false;
    render();
  }
}, 100);

// ------------------------------------------------------------------ settings
function settingsHtml() {
  const cb = (k, label) => `<div class="lf"><label><input type="checkbox" data-s="${k}" ${S[k] ? "checked" : ""}> ${label}</label></div>`;
  return cb("music", "酒馆背景音乐 · 低音量循环")
    + `<div class="lf music-controls"><label class="k">音乐音量</label><input aria-label="音乐音量" type="range" data-s="musicVolume" min="0" max="0.3" step="0.01" value="${S.musicVolume}"><span class="music-volume">${Math.round(S.musicVolume * 100)}%</span><span class="small music-status"></span></div><div class="music-credit">《水手之家》· David Arkenstone / World of Warcraft / Blizzard Entertainment<br><a href="https://music.163.com/song?id=1234865" target="_blank" rel="noopener noreferrer">网易云音乐 ↗</a></div>`
    + cb("effects3d", "3D 卡牌效果（关闭后仍可拖动出牌）")
    + cb("anim", "动画效果（战斗中的攻击、伤害、增益和死亡；招募阶段的增益数字）")
    + `<div class="lf"><label class="k">动画速度</label><select data-s="animSpeed">${[0.5, 1, 2, 4].map((x) => `<option ${x === S.animSpeed ? "selected" : ""}>${x}</option>`).join("")}</select></div>`
    + cb("autoReplay", "回合结束后自动播放我的战斗")
    + (PUBLIC ? `<div class="lf small">对局计时：${V && !V.lobby ? TIMER_NAME[(V.clock || V.table || {}).timer] || "开局前设置" : "开局前选择，整局固定"}。默认 1.5 倍；不限时成绩单独统计。</div>` : cb("timer", "单人对局的回合计时器"))
    + cb("art", "显示卡图（当前可用卡牌从本地加载）")
    + cb("quickBuy", "电脑点击商店卡牌直接购买")
    + cb("repeatSpell", "连续施法：施放后保留同类法术；Esc 退出")
    + `<div class="lf"><label class="k">语言</label><select data-s="lang"><option value="zh" ${S.lang === "zh" ? "selected" : ""}>中文</option><option value="en" ${S.lang === "en" ? "selected" : ""}>English</option></select></div>`;
}
function wireSettings(root) {
  root.querySelectorAll("[data-s]").forEach((el) => {
    el.onchange = () => {
      const k = el.dataset.s;
      S[k] = el.type === "checkbox" ? el.checked : ["animSpeed", "musicVolume"].includes(k) ? parseFloat(el.value) : el.value;
      lang = S.lang; timer.enabled = S.timer;
      if (k === "repeatSpell" && !S.repeatSpell) { repeatSpellId = null; activeSpell = null; }
      regionVersions.clear(); hidePreview();
      saveS(); applyTavernSettings();
      if (V && !lobbyOpen) render();
    };
    if (el.dataset.s === "musicVolume") el.oninput = el.onchange;
  });
  musicUI();
}
function showSettings() {
  const box = $("settings-box");
  box.innerHTML = `<h3>界面设置</h3>${settingsHtml()}<div class="small">快捷键：空格结束回合，R 刷新，F 冻结，${PUBLIC ? "" : "Ctrl+Z 撤销，"}Esc 取消。</div><div style="margin-top:10px"><button id="set-close" class="primary">完成</button></div>`;
  wireSettings(box);

  $("set-close").onclick = () => $("settings-modal").classList.remove("show");
  dialogHeader("settings-modal", "界面设置");
  $("settings-modal").classList.add("show");
}

// ------------------------------------------------------------------ lobby
const LOBBY_KEY = "bg_lobby";
function showLobby() {
  dismissCombat(); hidePreview(); hideActionDock();
  if (!lobbyOpen) for (const id of ["modal", "opp-modal", "detail-modal", "settings-modal"]) $(id).classList.remove("show");
  if (PUBLIC) return showPublicLobby();
  if (!showLobby.fetching) {
    // Refresh the human-vs-AI summary in the background and redraw once it arrives.
    showLobby.fetching = true;
    fetch("/api/human_stats").then((r) => r.json()).then((j) => { showLobby.humanStats = j; if (lobbyOpen) showLobby(); }).catch(() => {});
  } else {
    showLobby.fetching = false;
  }
  lobbyOpen = true; if ($('game-abandon')) $('game-abandon').hidden = true;
  const cfg = (V && V.config) || {};
  const saved = (() => { try { return JSON.parse(localStorage.getItem(LOBBY_KEY) || "{}"); } catch (e) { return {}; } })();
  const L = Object.assign({ seed: "", num_players: 8, hero_choices: cfg.hero_choices || 4, hero: "", types: [], timewarp: false, content: "Implemented", max_turns: cfg.max_turns || 100, opponents: "bots", scenario: "" }, saved);
  const ai = (V && V.ai) || {};
  if (!ai.loaded && L.opponents === "ai") L.opponents = "bots";
  if (!ai.loaded) L.seat = "human";
  const hs = showLobby.humanStats;
  const hsText = hs && hs.all.games ? `你和 AI 已打 ${hs.all.games} 局，平均第 ${hs.all.mean_place.toFixed(2)} 名` + (hs.clean.games ? `（其中没用撤销和 AI 代打的 ${hs.clean.games} 局：平均第 ${hs.clean.mean_place.toFixed(2)} 名）` : "") + `。记录在 ${esc(hs.file)}。` : "你和 AI 对局的名次会自动记录，用来估计 AI 的水平。";
  const aiInfo = ai.loaded ? `已加载模型：${esc(ai.info.path)}（训练场景 ${esc(ai.info.scenario || "?")}，第 ${ai.info.it ?? "?"} 轮）` : ai.error ? `模型加载失败：${esc(ai.error)}` : "没有加载模型：用 <code>scripts/webui.sh --ai &lt;模型&gt;</code> 启动，或放到 models/bgai-s0.pt。";
  const types = Object.keys(RACE).filter((r) => r !== "ALL" && r !== "NAGA");
  const heroes = CAT.filter((c) => c.kind === "hero" && c.in_pool && c.hero_power_scripted).sort((a, b) => a.name_zh.localeCompare(b.name_zh, "zh"));
  const inGame = V && !V.lobby;
  $("lobby-box").innerHTML = `
    <div class="lobby-title"><h1>酒馆战棋模拟器</h1><div class="small">单人模式 · ${esc((V && V.version) || "")} · 你在 0 号位，其余座位由启发式机器人或 AI 操作</div></div>
    <div class="lobby-grid">
      <div class="panel">
        <h3>对局</h3>
        <div class="lf"><label class="k">随机种子</label><input type="text" id="lb-seed" placeholder="留空 = 随机" value="${esc(L.seed)}"></div>
        <div class="lf"><label class="k">玩家人数</label><select id="lb-players">${[2, 3, 4, 5, 6, 7, 8].map((x) => `<option ${x === L.num_players ? "selected" : ""}>${x}</option>`).join("")}</select></div>
        <div class="lf"><label class="k">英雄选项数</label><select id="lb-choices">${[2, 3, 4].map((x) => `<option ${x === L.hero_choices ? "selected" : ""}>${x}</option>`).join("")}</select></div>
        <div class="lf"><label class="k">指定英雄</label><select id="lb-hero" style="flex:1"><option value="">随机（从选项里挑）</option>${heroes.map((h) => `<option value="${h.id}" ${h.id === L.hero ? "selected" : ""}>${esc(h.name_zh)} / ${esc(h.name)}</option>`).join("")}</select></div>
        <div class="small">指定的英雄会出现在你的英雄选项里（第一个）。</div>
      </div>
      <div class="panel">
        <h3>规则</h3>
        <div class="lf"><label><input type="checkbox" id="lb-tw" ${L.timewarp ? "checked" : ""}> 开启时空扭曲酒馆系统（第 6、9 回合造访）</label></div>
        <div class="lf"><label class="k">卡牌范围</label><select id="lb-content"><option value="Implemented" ${L.content === "Implemented" ? "selected" : ""}>只用已实现效果的卡</option><option value="AllVanilla" ${L.content === "AllVanilla" ? "selected" : ""}>全部卡（未实现的只有身材）</option></select></div>
        <div class="lf"><label class="k">回合上限</label><input type="number" id="lb-maxturns" min="1" value="${L.max_turns}"></div>
        <div class="small">其它规则沿用模拟器默认配置（当前版本默认：黑暗之赐与饰品开启，时空扭曲关闭）。</div>
      </div>
      <div class="panel wide">
        <h3>对手与 AI</h3>
        <div class="lf"><label class="k">其他座位</label><select id="lb-opp"><option value="bots" ${L.opponents === "bots" ? "selected" : ""}>启发式机器人</option><option value="ai" ${L.opponents === "ai" ? "selected" : ""} ${ai.loaded ? "" : "disabled"}>AI 模型${ai.loaded ? "" : "（未加载）"}</option></select></div>
        <div class="lf"><label class="k">你的座位</label><select id="lb-seat"><option value="human" ${L.seat !== "ai" ? "selected" : ""}>自己操作</option><option value="ai" ${L.seat === "ai" ? "selected" : ""} ${ai.loaded ? "" : "disabled"}>AI 操作（观战）${ai.loaded ? "" : "（未加载）"}</option></select></div>
        <div class="lf"><label class="k">规则预设</label><select id="lb-scen"><option value="" ${L.scenario === "" ? "selected" : ""}>按本页的设置</option><option value="experimental" ${L.scenario === "experimental" ? "selected" : ""}>实验：任务＋伙伴＋时空扭曲</option><option value="S0" ${L.scenario === "S0" ? "selected" : ""}>S0：AI 训练场景</option></select></div>
        <div class="small">S0：8 人都是帕奇维克（开局 60 血），种族固定为野兽、恶魔、龙、机械、鱼人，没有酒馆法术、饰品和黑暗之赐；选了 S0 时会覆盖本页的英雄、种族等设置。AI 目前只在 S0 里训练过，在其他规则下也能操作，但水平没有保证。<br>${aiInfo}<br>${hsText}</div>
      </div>
      <div class="panel wide">
        <h3>本局种族 <span class="small" id="lb-types-n"></span></h3>
        <div class="lf"><div class="types">${types.map((r) => `<label><input type="checkbox" class="lb-type" value="${r}" ${L.types.includes(r) ? "checked" : ""}> ${RACE[r]}</label>`).join("")}</div></div>
        <div class="small">勾选的种族一定出现，其余位置随机补足到 ${cfg.lobby_races || 5} 个。都不勾 = 完全随机（畸变怪约 50% 出现）。</div>
      </div>
      <div class="panel wide">
        <h3>界面</h3>
        ${settingsHtml()}
      </div>
    </div>
    <div class="lobby-go">
      <button class="primary" id="lb-start">开始新对局</button>
      ${inGame ? '<button id="lb-back">继续当前对局</button>' : ""}
    </div>`;
  wireSettings($("lobby-box"));
  const maxTypes = cfg.lobby_races || 5;
  const boxes = Array.from(document.querySelectorAll(".lb-type"));
  const syncTypes = () => {
    const n = boxes.filter((b) => b.checked).length;
    $("lb-types-n").textContent = `已指定 ${n}/${maxTypes}`;
    boxes.forEach((b) => (b.disabled = !b.checked && n >= maxTypes));
  };
  boxes.forEach((b) => (b.onchange = syncTypes));
  syncTypes();
  $("lb-start").onclick = async () => {
    const L2 = {
      seed: $("lb-seed").value.trim(), num_players: parseInt($("lb-players").value), hero_choices: parseInt($("lb-choices").value),
      hero: $("lb-hero").value, types: boxes.filter((b) => b.checked).map((b) => b.value), timewarp: $("lb-tw").checked,
      content: $("lb-content").value, max_turns: parseInt($("lb-maxturns").value) || 100,
      opponents: $("lb-opp").value, scenario: $("lb-scen").value, seat: $("lb-seat").value,
    };
    localStorage.setItem(LOBBY_KEY, JSON.stringify(Object.assign({}, L2, { seed: "" })));  // a kept seed would replay the same game
    if (L2.seed && !/^\d+$/.test(L2.seed)) return toast("随机种子必须是非负整数");
    const config = { num_players: L2.num_players, hero_choices: L2.hero_choices, lobby_types: L2.types, timewarp_enabled: L2.timewarp, content: L2.content, max_turns: L2.max_turns };
    $("lb-start").disabled = true;
    const j = await api("/api/new", { seed: L2.seed === "" ? null : parseInt(L2.seed), config, hero: L2.hero || null, opponents: L2.opponents, scenario: L2.scenario || null, spectate: L2.seat === "ai", timer:L2.timer });
    aw.auto = false;
    $("lb-start").disabled = false;
    if (j.ok === false) return;
    freshGame();
    closeLobby();
  };
  if (inGame) $("lb-back").onclick = closeLobby;
  $("lobby").classList.add("show");
}
// ------------------------------------------------------------------ the hall (public server)
// H is what GET /api/lobby says: `me` (null until this visitor has a username; else the
// username and the room they are in), the capacity, the players online and the rooms.
// ROOM is the room of the last view. Every player owns a single-player room; rooms that
// several players join are opened from the hall.
const PUBLIC_LOBBY_KEY = "bg_public_lobby", ROOM_OPT_KEY = "bg_room_options", HALL_TAB_KEY = "bg_hall_tab", NAME_KEY = "bg_username";
const USERNAME_RE = /^[A-Za-z][A-Za-z0-9_]{2,15}$/;
const USERNAME_RULE = "3 到 16 位：英文字母开头，只能有英文字母、数字和下划线";
let H = null, ROOM = null, hallTimer = null, hallStats = null;
const stored = (k) => { try { return JSON.parse(localStorage.getItem(k) || "{}") || {}; } catch (e) { return {}; } };
const store = (k, v) => { try { localStorage.setItem(k, typeof v === "string" ? v : JSON.stringify(v)); } catch (e) {} };
const savedName = () => { try { const n = localStorage.getItem(NAME_KEY) || ""; return USERNAME_RE.test(n) ? n : ""; } catch (e) { return ""; } };
const inMulti = () => !!(H && H.me && !H.me.watching && H.me.kind === "multi");
const myRoom = () => (H && H.me ? H.rooms.find((r) => r.id === H.me.room) : null);
const aiAssist = () => (!PUBLIC || !H || !H.ai || H.ai.assist !== false) && V?.game_settings?.mode !== 'ranked';
async function loadHall() {
  try { const j = await bgJson('/api/lobby'); if (j && j.capacity) { H = j; syncAnnouncement(); } } catch (e) {connectionStatus(false);}
  return H;
}
// The operator's announcement (H.announcement): shown every time the page is entered, and
// again when another one is published while the page is open (the heartbeat tells).
let announceShown = null;
function syncAnnouncement() {
  if (!PUBLIC) return;
  const a = H && H.announcement;
  // The top bar has its button only while there is an announcement (every button of the bar
  // is one that can be pressed).
  let button = $("announce-open");
  if (a && !button) {
    const nav = $("support-tools");
    button = document.createElement("button");
    button.id = "announce-open"; button.type = "button"; button.textContent = "公告";
    button.onclick = showAnnouncement;
    nav.insertBefore(button, nav.querySelector("button"));
  } else if (!a && button) button.remove();
  if (!a) { if ($("announce-modal").classList.contains("show")) supportClose("announce-modal"); return; }
  if (a.id === announceShown) return;
  announceShown = a.id;
  showAnnouncement();
}
function showAnnouncement() {
  const a = H && H.announcement;
  if (!a) return;
  $("announce-text").textContent = a.text;
  $("announce-time").textContent = a.time ? "发布于 " + a.time.replace("T", " ") : "";
  supportOpen("announce-modal");
}
/** A request that the server answers with the hall (logging in and out, the room calls).
 *  Returns "" or what went wrong. */
async function hallPost(path, body) {
  return serializeMutation(async () => {
    let j;
    try { j = await (await post(path, body)).json(); } catch (e) { return "连不上服务器，请稍后再试"; }
    if (j.login_required && path !== "/api/login") { onDropped(); return j.error || "请先设置用户名"; }
    if (!j.ok) return j.error || "这一步没有生效，请稍后再试";
    H = j;
    return "";
  });
}
/** Enter the hall as `name` (or change the name). Returns "" or what went wrong. */
async function login(name) {
  const err = await hallPost("/api/login", { username: name });
  if (!err) store(NAME_KEY, H.me.username);
  return err;
}
const GUIDE_KEY = "bg_first_login_guide_v1";
const dialogClosers = {}, dialogFocus = {};
function choiceKey() { return V && V.me.pending ? JSON.stringify([V.seed, V.turn, V.me.pending]) : null; }
function closeDialog(id) {
  (dialogClosers[id] || (() => $(id).classList.remove("show")))();
  const focus = dialogFocus[id]; if (focus && focus.isConnected) focus.focus({preventScroll:true});
}
function dialogHeader(id, title, close, label = "关闭") {
  closeHint(false);
  hideActionDock(); hidePreview();
  const modal = $(id), box = modal.querySelector(".modal-box");
  box.tabIndex = -1;
  dialogClosers[id] = close || (() => modal.classList.remove("show"));
  dialogFocus[id] = document.activeElement;
  modal.setAttribute("role", "dialog"); modal.setAttribute("aria-modal", "true");
  if (box.querySelector(":scope > .dialog-head")) return;
  const existing = box.querySelector(":scope > .rankings-head");
  if (existing) { existing.classList.add("dialog-head"); $("rankings-close").classList.add("dialog-dismiss"); $("rankings-close").onclick = () => closeDialog(id); box.scrollTop = 0; return; }
  const duplicate = box.querySelector(":scope > h3"); if (duplicate && duplicate.textContent === title) duplicate.remove();
  const head = document.createElement("div"); head.className = "dialog-head";
  const heading = document.createElement("h3"); heading.className = "dialog-title"; heading.id = id + "-heading"; heading.textContent = title;
  const button = document.createElement("button"); button.type = "button"; button.textContent = label; button.className = "dialog-dismiss"; button.setAttribute("aria-label", label + title); button.onclick = () => closeDialog(id);
  head.append(heading, button); box.prepend(head); modal.setAttribute("aria-labelledby", heading.id);
  box.scrollTop = 0;
  requestAnimationFrame(() => { if (modal.classList.contains("show") && !modal.contains(document.activeElement)) button.focus({preventScroll:true}); });
}
const floatPositions = {};
function floatBounds() {
  const vv = window.visualViewport, top = vv ? vv.offsetTop : 0, height = vv ? vv.height : innerHeight;
  const nav = $("support-tools"), bar = $("topbar");
  const headerBottom = Math.max(top, nav && nav.getClientRects().length ? nav.getBoundingClientRect().bottom : top,
    !lobbyOpen && bar && bar.getClientRects().length ? bar.getBoundingClientRect().bottom : top);
  let safeTop = headerBottom + 8;
  const footer = $("ctl");
  let bottom = innerWidth <= 760 && !document.body.classList.contains("game-landscape") && footer && footer.getClientRects().length
    ? Math.min(top + height - 8, footer.getBoundingClientRect().top - 8) : top + height - 12;
  // Short screens have room above the game bar, occupied by the opponent strip.
  // Use that space instead of clipping the close control or covering the menus/footer.
  const navBottom = nav && nav.getClientRects().length ? nav.getBoundingClientRect().bottom : top;
  const aboveBottom = bar && bar.getClientRects().length ? bar.getBoundingClientRect().top - 8 : bottom;
  if (bottom - safeTop < 140 && aboveBottom - navBottom >= Math.max(100, bottom - safeTop + 16)) {
    safeTop = navBottom + 8; bottom = aboveBottom;
  }
  return {top:safeTop, bottom:Math.max(safeTop + 60, bottom), width:vv ? vv.width : innerWidth};
}
function positionFloat(box) {
  if (!box || !box.classList.contains("show")) return;
  const b = floatBounds(), width = Math.min(360, b.width - 16), pos = floatPositions[box.id];
  document.documentElement.style.setProperty("--float-safe-top", b.top + "px");
  document.documentElement.style.setProperty("--float-max-height", Math.max(45, b.bottom - b.top) + "px");
  box.style.width = width + "px"; box.style.right = "auto";
  box.style.maxHeight = Math.max(60, b.bottom - b.top) + "px";
  const rect = box.getBoundingClientRect();
  const x = Math.max(8, Math.min(b.width - width - 8, pos ? pos.x : b.width - width - 12));
  const y = Math.max(b.top, Math.min(b.bottom - rect.height, pos ? pos.y : b.top));
  box.style.left = x + "px"; box.style.top = y + "px";
}
function setupFloatDrag() {
  let drag = null;
  document.addEventListener("pointerdown", (e) => {
    const head = e.target.closest(".hint-head, .aw-head");
    if (!head || e.target.closest("button, select, input, a") || e.button !== 0) return;
    const box = head.closest("#ai-hint, #ai-watch"), r = box.getBoundingClientRect();
    drag = {box, id:e.pointerId, dx:e.clientX-r.left, dy:e.clientY-r.top};
    head.setPointerCapture(e.pointerId); e.preventDefault();
  });
  document.addEventListener("pointermove", (e) => {
    if (!drag || drag.id !== e.pointerId) return;
    floatPositions[drag.box.id] = {x:e.clientX-drag.dx, y:e.clientY-drag.dy};
    positionFloat(drag.box); e.preventDefault();
  });
  for (const name of ["pointerup", "pointercancel", "lostpointercapture"]) document.addEventListener(name, () => { drag = null; });
}
function setupOverlays() {
  setupFloatDrag();
  const updateViewport = () => {
    const viewport = window.visualViewport;
    document.documentElement.style.setProperty("--visible-height", (viewport ? viewport.height : innerHeight) + "px");
    document.documentElement.style.setProperty("--visible-top", (viewport ? viewport.offsetTop : 0) + "px");
    positionFloat($("ai-hint")); positionFloat($("ai-watch"));
  };
  updateViewport(); window.addEventListener("resize", updateViewport);
  if (window.visualViewport) { visualViewport.addEventListener("resize", updateViewport); visualViewport.addEventListener("scroll", updateViewport); }
  document.querySelectorAll(".modal").forEach((modal) => modal.addEventListener("click", (e) => {
    if (e.target !== modal) return;
    e.stopImmediatePropagation();
    // A required choice only folds through its explicit control or Escape. A tap
    // on the newly displayed backdrop must not dismiss hero/discover selection.
    if (modal.id === 'modal' && V?.me.pending) return;
    closeDialog(modal.id);
  }));
  document.addEventListener("pointerdown", (e) => { if (!e.target.closest("#ai-hint") && $("ai-hint").classList.contains("show")) closeHint(false); }, true);
  document.addEventListener("keydown", (e) => {
    const overlays = [...document.querySelectorAll(".modal.show, #ai-hint.show")].sort((a,b) => Number(getComputedStyle(b).zIndex) - Number(getComputedStyle(a).zIndex));
    const overlay = overlays[0];
    if (!overlay) {
      if (e.key === "Escape" && $("ai-watch").classList.contains("show")) { closeAiWatch(); e.preventDefault(); e.stopImmediatePropagation(); }
      return;
    }
    if (e.key === "Escape") { e.preventDefault(); e.stopImmediatePropagation(); if (overlay.id === "ai-hint") closeHint(); else closeDialog(overlay.id); return; }
    if (e.key === "Tab") {
      const els = [...overlay.querySelectorAll("button:not(:disabled), textarea, input, select, summary, [tabindex='0']")].filter((el) => !el.disabled && el.getClientRects().length);
      if (els.length && (e.shiftKey && (document.activeElement === els[0] || !overlay.contains(document.activeElement)))) { e.preventDefault(); els[els.length-1].focus(); }
      else if (els.length && (!e.shiftKey && (document.activeElement === els[els.length-1] || !overlay.contains(document.activeElement)))) { e.preventDefault(); els[0].focus(); }
    }
    if ((e.key === " " || e.key === "Enter") && !overlay.contains(e.target) && !e.target.closest("#support-tools")) { e.preventDefault(); e.stopImmediatePropagation(); }
    // Events inside the dialog reach its cards and buttons. The game shortcut listener
    // independently ignores open overlays, including Ctrl+Z behind a detail dialog.
  }, true);
}
let rankingsData = null, rankingsMode = 'all', rankingsCategory = 'ranked', rankingsPreset = 'ai6_7', rankingsMetric = 'score', rankingsLoading = false, rankingsRun = 0;
function renderRankings() {
  if (!rankingsData) return;
  const rows = rankingsData.boards[rankingsMode] || [];
  const presetName=rankingsData.ranked_presets?.find(p=>p.id===rankingsData.preset)?.label;
  $("rankings-status").textContent = `${presetName ? presetName+' · ' : ''}已收录 ${rankingsData.games} 场对局 · ${rankingsData.players} 位玩家`;
  $("rankings-list").innerHTML = rows.length ? rows.map((r, i) => {
    const best = r.best, lineup = best.final_lineup;
    const hero = lineup && lineup.hero ? nm(lineup.hero) : best.hero;
    const board = lineup && lineup.board;
    const warband = board == null ? '<div class="rank-empty">这场历史对局没有保存终局阵容。</div>' : board.length ? `<div class="rank-warband">${board.map(c => `<div class="rank-minion${c.golden ? ' golden' : ''}">${itemArt(c, 'rank-card-art')}<b>${esc(nm(c) || c.id)}</b><div class="small">${(c.keywords || []).map(k => esc(kwName(k))).join(' · ')}</div><div class="rank-stats">${esc(c.atk)} / ${esc(c.max_hp ?? c.hp)}</div></div>`).join('')}</div>` : '<div class="rank-empty">终局战队为空。</div>';
    return `<details class="rankings-entry" ${i === 0 ? 'open' : ''}><summary><span class="rank-number">${r.rank <= 3 ? ['Ⅰ','Ⅱ','Ⅲ'][r.rank-1] : r.rank}</span><div class="rank-person"><b>${esc(r.username)}</b><span class="small">${r.games} 场 · 平均 ${Number(r.mean_place).toFixed(2)} 名</span></div><div class="rank-metric ${rankingsMetric === 'score' ? 'highlight' : ''}"><b>${r.score}</b><span>总分</span></div><div class="rank-metric ${rankingsMetric === 'wins' ? 'highlight' : ''}"><b>${r.wins}</b><span>吃鸡数</span></div></summary><div class="rank-lineup"><div class="rank-game">${lineup?.hero ? heroArt(lineup.hero) : ''}<span>${esc(hero || '英雄未记录')} · 第 ${esc(best.placement)} 名 · ${esc(best.turn ?? '—')} 回合<br>${esc(String(best.time || '').replace('T', ' '))} · 最佳对局终局阵容</span></div>${warband}</div></details>`;
  }).join('') : `<div class="rank-empty">${rankingsCategory === 'ranked' ? '此预设暂无完成的计分对局。完成一局，争夺第一个名次。' : '还没有已完成的对局。'}</div>`;
  const preview = $("hall-ranking-preview");
  if (preview) preview.innerHTML = `<span class="small">${esc(presetName || '其他模式 · 全部对局')} · ${rankingsMetric==='score' ? '总分榜' : '吃鸡榜'}</span>` + (rankingsData.boards.all.slice(0, 3).map(r => `<span class="rank-preview-item">#${r.rank} <b>${esc(r.username)}</b> ${r.score} 分 · ${r.wins} 次吃鸡</span>`).join('') || '<span class="small">计分榜等你登顶。</span>');
}
async function loadRankings() {
  const run = ++rankingsRun;
  rankingsLoading = true; $("rankings-refresh").disabled = true;
  $("rankings-status").textContent = "正在读取成绩…";
  try {
    const response = await fetch(`/api/leaderboard?mode=${rankingsCategory}&metric=${rankingsMetric}&preset=${rankingsPreset}`, { cache:'no-store' });
    const result = await response.json();
    if (!response.ok || !result.ok) throw new Error(result.error || "排行榜暂时不可用，请稍后刷新。");
    if (run === rankingsRun) { rankingsData = result; renderRankings(); }
  } catch (err) { $("rankings-status").textContent = err.message || "读取失败，请稍后刷新。"; }
  finally { if (run === rankingsRun) { rankingsLoading = false; $("rankings-refresh").disabled = false; } }
}
function showRankings() { supportOpen("rankings-modal"); if (rankingsData) renderRankings(); loadRankings(); }
function guideDone() { try { return localStorage.getItem(GUIDE_KEY) === "done"; } catch (e) { return guideShown; } }
function supportOpen(id) {
  closeHint(false); hidePreview(); hideActionDock();
  supportFocus = dialogFocus[id] = document.activeElement;
  for (const other of ["guide-modal", "feedback-modal", "rankings-modal", "collection-modal", "announce-modal"]) if (other !== id) $(other).classList.remove("show");
  $(id).classList.add("show");
  const focus = $(id).querySelector("textarea, button"); if (focus) focus.focus({preventScroll:true});
}
function supportClose(id) { $(id).classList.remove("show"); if (supportFocus && supportFocus.isConnected) supportFocus.focus(); }
function showGuide() { guideShown = true; supportOpen("guide-modal"); }
function maybeGuide() { if (PUBLIC && H && H.me && !guideShown && !guideDone() && !$("announce-modal").classList.contains("show")) showGuide(); }
/** The feedback button: a press opens the form, a drag moves the button. Its place is kept in
 *  the browser as fractions of the free space, so it stays inside when the window changes. */
const FEEDBACK_POS_KEY = "bg_feedback_pos";
function setupFeedbackButton() {
  const b = $("feedback-open");
  let pos = null, drag = null, moved = false;
  try { pos = JSON.parse(localStorage.getItem(FEEDBACK_POS_KEY)); } catch (e) {}
  const area = () => {   // where the button may be: below the top bar, inside the visible page
    const vv = window.visualViewport, top = $("support-tools").getBoundingClientRect().bottom + 6;
    return { left: 6, top, right: (vv ? vv.width : innerWidth) - b.offsetWidth - 6, bottom: (vv ? vv.height : innerHeight) - b.offsetHeight - 6 };
  };
  const place = () => {
    if (!pos || !Number.isFinite(pos.x) || !Number.isFinite(pos.y)) {
      b.style.left = b.style.top = b.style.right = b.style.bottom = '';
      if (document.body.classList.contains('game-landscape')) {
        const nav = $('support-tools'), brand = nav.querySelector('.nav-brand').getBoundingClientRect();
        const buttons = [...nav.querySelectorAll('button')].map(el => el.getBoundingClientRect()).filter(r => r.width);
        const right = Math.min(...buttons.map(r => r.left), nav.getBoundingClientRect().right);
        if (right - brand.right >= b.offsetWidth + 16) {
          const r = nav.getBoundingClientRect();
          b.style.left = (brand.right + 8) + 'px';
          b.style.top = (r.top + (r.height - b.offsetHeight) / 2) + 'px';
          b.style.right = b.style.bottom = 'auto';
        }
      }
      return;
    }
    const a = area(), unit = (v) => Math.min(1, Math.max(0, v));
    let x = Math.round(a.left + unit(pos.x) * Math.max(0, a.right - a.left));
    let y = Math.round(a.top + unit(pos.y) * Math.max(0, a.bottom - a.top));
    // A saved drag position may land over End Turn after rotation or a layout change.
    for (const end of document.querySelectorAll('.end-btn')) {
      const r = end.getBoundingClientRect();
      if (r.width && r.height && x < r.right && x + b.offsetWidth > r.left && y < r.bottom && y + b.offsetHeight > r.top) {
        x = Math.max(a.left, r.left - b.offsetWidth - 8);
      }
    }
    b.style.left = x + "px";
    b.style.top = y + "px";
    b.style.right = b.style.bottom = "auto";
  };
  b.addEventListener("pointerdown", (e) => {
    if (e.button !== 0) return;
    const r = b.getBoundingClientRect();
    drag = { id: e.pointerId, dx: e.clientX - r.left, dy: e.clientY - r.top, x: e.clientX, y: e.clientY };
    moved = false;
    try { b.setPointerCapture(e.pointerId); } catch (_) {}
  });
  b.addEventListener("pointermove", (e) => {
    if (!drag || drag.id !== e.pointerId) return;
    if (!moved && Math.hypot(e.clientX - drag.x, e.clientY - drag.y) < 6) return;
    moved = true; b.classList.add("dragging");
    const a = area();
    const x = Math.min(a.right, Math.max(a.left, e.clientX - drag.dx)), y = Math.min(a.bottom, Math.max(a.top, e.clientY - drag.dy));
    pos = { x: (x - a.left) / Math.max(1, a.right - a.left), y: (y - a.top) / Math.max(1, a.bottom - a.top) };
    place();
  });
  const drop = (e) => {
    if (!drag || drag.id !== e.pointerId) return;
    drag = null; b.classList.remove("dragging");
    if (moved) store(FEEDBACK_POS_KEY, JSON.stringify(pos));
  };
  b.addEventListener("pointerup", drop);
  b.addEventListener("pointercancel", drop);
  b.addEventListener("click", (e) => {
    if (moved) { moved = false; e.preventDefault(); return; }   // the click that ends a drag opens nothing
    $("guide-modal").classList.remove("show"); supportOpen("feedback-modal");
  });
  addEventListener("resize", place);
  if (window.visualViewport) visualViewport.addEventListener("resize", place);
  new MutationObserver(place).observe(document.body, {attributes:true, attributeFilter:['class']});
  place();
}
function setupSupport() {
  $("rankings-open").onclick = showRankings;
  $("rankings-close").onclick = () => supportClose("rankings-modal");
  $("rankings-refresh").onclick = loadRankings;
  $('rankings-metric').onchange = () => { rankingsMetric=$('rankings-metric').value; loadRankings(); };
  $('rankings-preset').onchange = () => { rankingsPreset=$('rankings-preset').value; loadRankings(); };
  $('rankings-tabs').querySelectorAll('button').forEach(b => { b.onclick=() => {rankingsCategory=b.dataset.category; $('rankings-preset-control').hidden=rankingsCategory!=='ranked'; $('rankings-tabs').querySelectorAll('button').forEach(x => x.classList.toggle('on',x===b)); loadRankings();}; });
  $("rankings-modal").onclick = (e) => { if (e.target === $("rankings-modal")) supportClose("rankings-modal"); };
  $("guide-open").onclick = showGuide;
  const closeGuide = () => { store(GUIDE_KEY, "done"); supportClose("guide-modal"); };
  dialogHeader("guide-modal", "新手引导", closeGuide);
  dialogHeader("feedback-modal", "反馈问题", () => supportClose("feedback-modal"));
  const closeAnnouncement = () => { supportClose("announce-modal"); maybeGuide(); };   // a first visit: the guide comes after the announcement
  dialogHeader("announce-modal", "公告", closeAnnouncement);
  $("announce-close").onclick = closeAnnouncement;
  $("announce-modal").onclick = (e) => { if (e.target === $("announce-modal")) closeAnnouncement(); };
  dialogHeader("rankings-modal", "玩家排行榜", () => supportClose("rankings-modal"));
  $("guide-close").onclick = closeGuide;
  setupFeedbackButton();
  $("feedback-close").onclick = () => supportClose("feedback-modal");
  $("feedback-form").onsubmit = async (e) => {
    e.preventDefault(); const send = $("feedback-send"), status = $("feedback-status");
    if (send.disabled) return;
    send.disabled = true; status.className = "support-status"; status.textContent = "正在提交…";
    try {
      const context = { turn:V && V.turn, phase:V && V.phase, hero:V && V.me && V.me.hero && V.me.hero.id, last_error:lastApiError };
      const response = await post("/api/feedback", { category:$("feedback-category").value, message:$("feedback-message").value, context });
      const result = await response.json();
      if (!response.ok || !result.ok) throw new Error(result.error || "提交失败，请稍后重试");
      status.textContent = "已收到，反馈编号 " + result.feedback_id + "。你可以继续游戏。";
      $("feedback-message").value = "";
    } catch (err) { status.className = "support-status error"; status.textContent = err.message || "连不上服务器，内容已保留，请稍后重试。"; }
    finally { send.disabled = false; }
  };
}
/** First thing on the public page: the hall, the remembered username, then the game. */
async function startPublic() {
  await loadHall();
  if (H && !H.me && savedName()) await login(savedName());   // e.g. after a server restart
  if (H && H.me) await refresh(); else showPublicLobby();
  maybeGuide();
  startHeartbeat();
  document.addEventListener("visibilitychange", () => { if (!document.hidden) heartbeat(true); });
}
/** A game begins (or another one is shown): nothing of the last one is carried over. */
function freshGame() {
  resetSocialFx();
  clearTurnNotice();
  lastCombatTurnShown = -1; showGameOver.shown = null; noticeDismissed = -1; selHand = null; dbg.sel = null; aw.auto = false; aw.closed = false; tapKey = null;
  commandEpoch++; repeatSpellId = null; activeSpell = null; regionVersions.clear();
  timer.key = null; timer.turn = -1; timer.firing = false; lastTimerTick = performance.now();
  dismissCombat();
  if (pointerController) pointerController.cancel();
  endDrag(); hidePreview();
  hiddenChoice = null;
  delete $('modal').dataset.choice;
  $('modal').style.opacity = '';
}

// Presence and news. An open page asks the server every few seconds. That is how the server
// knows the page is still there (a player whose page stays silent for `away_seconds` is
// dropped and their game ends), and how the page learns what changed in its room: someone
// joined, the owner started, another player ended the turn, the clock ran out.
const beat = { n: 0, busy: false, rev: null, state: null, game: null, deadlineAt: null, dirty: false };
function setDeadline(seconds) {
  beat.deadlineAt = seconds === null || seconds === undefined ? null : performance.now() + seconds * 1000;
}
async function heartbeat(now) {
  if (!H || !H.me || beat.busy || pendingCommands || drag || pointerDrag || actionBusy) return;
  // Where the server counts a hidden page as away, a hidden page keeps quiet.
  if (H.away_hidden && document.hidden) return;
  // Every 1.2 s in a room with other players, every 2.4 s otherwise.
  if (now!==true && performance.now()<connection.next) return;
  beat.busy = true;
  try {
    const j = await bgJson('/api/poll');
    if (j.login_required) await onDropped();
    else if (j.ok) await onPoll(j);
  } catch (e) { connectionStatus(false);beat.rev=null; }
  beat.busy = false;
}
async function onPoll(j) {
  if (pendingCommands) { beat.dirty = true; return; }
  if (drag || pointerDrag || actionBusy) return; // keep the revision pending until the gesture ends
  if (j.notice) toast(j.notice, "info", 9000);
  // Another announcement than the one this page has (or none any more): fetch the hall again.
  if (j.announcement !== undefined && (j.announcement || null) !== ((H.announcement && H.announcement.id) || null)) await loadHall();
  if (j.deadline_in !== undefined) setDeadline(j.deadline_in);
  const moved = j.room !== H.me.room || j.kind !== H.me.kind || !!j.watching !== !!H.me.watching;
  const changed = moved || j.rev !== beat.rev || j.state !== beat.state || !!j.notice || beat.dirty;
  beat.rev = j.rev; beat.state = j.state;
  if (!changed) return;
  if (j.watching || (j.kind === "solo" && (j.state === "playing" || j.state === "finished" || j.state === "watching"))) {
    if (moved) { await loadHall(); freshGame(); V = null; }
    if(!await softRefresh()) {beat.rev=null;return;}
    if (j.watching && V && !lobbyOpen) closeLobby();
    if (lobbyOpen) { await loadHall(); updateHall(); }
    return;
  }
  if (j.kind === "multi" && (j.state === "playing" || j.state === "finished")) {
    // The shared game is on: show it, or bring it up to date.
    const first = beat.game !== j.room;
    if (first) { beat.game = j.room; freshGame(); V = null; }
    if (moved || lobbyOpen || first) await loadHall();
    if(!await softRefresh()) {beat.rev=null;return;}
    if (first && V && V.table) closeLobby(); else if (lobbyOpen) updateHall();
    return;
  }
  // A room that has not started, or my own room again.
  beat.game = null;
  await loadHall();
  if (!H || !H.me) return;
  if (moved || j.notice || (V && V.table)) { freshGame(); await refresh(); }
  if (lobbyOpen && $("hall-players") && !moved && !j.notice) updateHall(); else showPublicLobby();
}
/** Bring a shared game up to date after news from the server. While the stage (turn and
 *  phase) is the same, only what the others did is new: the tavern, the warband and the hand
 *  under the player's fingers are left as they are. */
async function softRefresh() {
  beat.dirty = true;
  if (pendingCommands) return;
  const generation = viewGeneration;
  let j;
  try { j = await bgJson("/api/view"); } catch (e) { return; }
  if (pendingCommands || generation !== viewGeneration) return;
  beat.dirty = false;
  if (j.login_required) return onDropped();
  restoreCombat(j);
  const same = !connection.uncertain && !V?.spectator && !j.spectator && V && V.table && j.table && j.seed === V.seed && j.turn === V.turn && j.phase === V.phase
    && !!j.me.ended_turn === !!V.me.ended_turn && j.me.alive === V.me.alive && j.table.left === V.table.left;
  if (!same) { setView(j); if (V) render(); else showPublicLobby(); return true; }
  if (j.table.seq < V.table.seq) return true;
  ROOM = j.room;
  Object.assign(V, { table: j.table, room: j.room, opponents: j.opponents, alive: j.alive, spectators:j.spectators,emote_events:j.emote_events,equipped_emotes:j.equipped_emotes });
  viewGeneration++;
  setDeadline(j.table.deadline_in);
  const badge = $("spectator-badge"); badge.hidden = lobbyOpen || !V.spectators; badge.textContent = `👁 ${V.spectators || 0} 人观战`;
  renderLeaderboard(); renderTop();renderEmotes();
  if (j.table.finished) renderPending();
  return true;
}
/** The server no longer knows this visitor: the page was away for too long (closed lid, a
 *  phone that put the browser to sleep), or the server restarted. Come back under the same
 *  name; the game is gone. */
async function onDropped() {
  if (onDropped.busy) return;
  onDropped.busy = true;
  // Dropped again right after coming back: the browser does not keep the cookie.
  const again = performance.now() - (onDropped.last || -1e9) < 4000;
  const hadGame = !!(V && !V.lobby) || inMulti();
  const away = H && H.away_seconds;
  if (H) H.me = null;
  freshGame();
  connection.uncertain=false;
  V = null; ROOM = null; beat.rev = beat.state = beat.game = null; setDeadline(null);
  for (const id of ["modal", "combat-modal", "opp-modal", "detail-modal"]) $(id).classList.remove("show");
  const err = again ? "登录状态没有保住：请允许这个网站使用 Cookie，然后再试" : savedName() ? await login(savedName()) : "先取一个用户名";
  onDropped.last = performance.now();
  onDropped.busy = false;
  if (err || !H || !H.me) { await loadHall(); lobbyOpen = true; $("lobby").classList.add("show"); return renderLogin(again || savedName() ? err : ""); }
  await refresh();
  showPublicLobby();
  if (hadGame) toast(away ? `连接已断开（页面超过 ${away} 秒没有动静，或者服务器重启过），刚才的对局没有保留` : "和服务器的连接断过，刚才的对局没有保留", "info", 9000);
}

/** A span of seconds as the hall's notes say it: "30 秒", "5 分钟". */
const spanText = (sec) => (sec >= 60 && sec % 60 === 0 ? `${sec / 60} 分钟` : `${Math.round(sec)} 秒`);
const RULES_NAME = { experimental: "任务＋伙伴＋时空扭曲（实验）", standard: "标准规则", S0: "S0 简化规则" };
const TIMER_NAME = { normal: "正常时长 ×1.5（默认）", standard: "正常时长 ×1", relaxed: "正常时长 ×2", off: "不限时" };
const fillName = (f) => (f === "ai" ? "AI" : "机器人");
function stateText(p) {
  if (p.state === "spectating") return `观战中 · 第 ${p.turn ?? 1} 回合`;
  if (p.state === "waiting") return "在房间里等待开始";
  if (p.state === "playing") return `对局中 · 第 ${p.turn ?? 1} 回合${p.hero ? " · " + esc(p.hero) : ""}`;
  if (p.state === "watching") return `观看 AI · 第 ${p.turn} 回合`;
  if (p.state === "finished") return p.placement ? `已结束 · 第 ${p.placement} 名` : "已结束";
  return "在大厅";
}
function capText() {
  const c = H.capacity;
  return `<span class="chip${c.online >= c.max ? " full" : ""}"><i class="dot"></i>在线 ${c.online} / ${c.max}</span>`;
}
function showPublicLobby() {
  dismissCombat(); hidePreview(); hideActionDock();
  if (!lobbyOpen) for (const id of ["modal", "opp-modal", "detail-modal", "settings-modal"]) $(id).classList.remove("show");
  lobbyOpen = true; if ($('game-abandon')) $('game-abandon').hidden = true;
  $("lobby").classList.add("show");
  $("ai-watch").classList.remove("show");
  $("spectator-badge").hidden = true;
  if (!H || !H.me) return renderLogin("");
  renderHall();
  updateHall();
  fetch("/api/human_stats").then((r) => r.json()).then((j) => { if (j && j.all) { hallStats = j; if (lobbyOpen) updateHall(); } }).catch(() => {});
  clearInterval(hallTimer);
  hallTimer = setInterval(async () => {
    if (!lobbyOpen) return clearInterval(hallTimer);
    await loadHall();
    if (!lobbyOpen || !H) return;
    if (H.me) return updateHall();
    onDropped();
  }, 3000);
}
/** The card that asks for a username; nothing else can be done before. */
function renderLogin(err) {
  clearInterval(hallTimer);
  const full = H && H.capacity.online >= H.capacity.max;
  $("lobby-box").innerHTML = `
    <div class="login-wrap">
      <div class="brand"><div class="brand-mark">⚔</div><h1>酒馆战棋模拟器</h1><div class="brand-sub">和朋友同局对战，或者挑战训练出来的 AI</div></div>
      <div class="tavern-welcome"><div class="eyebrow">WELCOME TO THE TAVERN</div><p>炉火正旺，空位已留。来一场属于你的酒馆冒险。</p></div>
      <div class="panel login-card">
        <h3>为你的英雄取个名字</h3>
        <div class="small">${USERNAME_RULE}。别的玩家在大厅和对局里看到的就是它。</div>
        <input type="text" id="login-name" maxlength="16" autocomplete="username" autocapitalize="off" spellcheck="false" placeholder="例如 Bob_2026" value="${esc(savedName())}">
        <div class="login-err" id="login-err">${esc(err || (full ? "服务器现在满员，有人离开后才能进" : ""))}</div>
        <button class="primary" id="login-go">进入大厅</button>
        <div class="login-foot">${H ? capText() : ""}</div>
      </div>
      <div class="tavern-features"><span><i>✦</i>挑战 AI</span><span><i>⚔</i>好友同局</span><span><i>♛</i>争夺榜首</span></div>
    </div>`;
  const go = async () => {
    const name = $("login-name").value.trim();
    if (!USERNAME_RE.test(name)) { $("login-err").textContent = "用户名要" + USERNAME_RULE; return; }
    $("login-go").disabled = true;
    const e = await login(name);
    if (e) { await loadHall(); return renderLogin(e); }
    beat.rev = beat.state = beat.game = null;
    await refresh();                       // the game view; opens the hall when no game runs
    showPublicLobby();
    maybeGuide();
  };
  $("login-go").onclick = go;
  $("login-name").onkeydown = (e) => { if (e.key === "Enter") go(); };
  if (!TOUCH) $("login-name").focus();
}
/** The hall's frame: drawn when it opens, so that a refresh of the lists does not reset
 *  the forms. */
function renderHall() {
  const ai = H.ai || {};
  // The rules the loaded model was trained with are the default.
  const aiFull = !!(ai.info && ai.info.scenario === "full");
  const L = Object.assign({ scenario: H.default_scenario || (aiFull ? "standard" : "S0"), opponents: ai.loaded ? "ai" : "bots", seat: "human", timer:"normal" }, stored(PUBLIC_LOBBY_KEY));
  const M = Object.assign({ scenario: H.default_scenario || "standard", fill: ai.loaded ? "ai" : "bots", timer: "normal" }, stored(ROOM_OPT_KEY));
  if (!ai.loaded) { L.opponents = "bots"; L.seat = "human"; M.fill = "bots"; }
  if (!aiAssist()) L.seat = "human";
  const inGame = V && !V.lobby && !V.table && !V.spectator;
  const tab = (() => { try { return localStorage.getItem(HALL_TAB_KEY) === "multi" ? "multi" : "solo"; } catch (e) { return "solo"; } })();
  const opt = (v, cur, label, disabled) => `<option value="${v}" ${v === cur ? "selected" : ""} ${disabled ? "disabled" : ""}>${label}</option>`;
  const rules = (cur) => opt("experimental", cur, RULES_NAME.experimental) + opt("standard", cur, "标准规则（36.6.3）" + (aiFull ? " · AI 训练场景" : "")) + opt("S0", cur, aiFull ? "S0 简化规则" : "S0 · AI 训练场景");
  const fills = (cur) => opt("ai", cur, "AI" + (ai.loaded ? "" : "（未加载）"), !ai.loaded) + opt("bots", cur, "简单机器人");
  const timers = (cur) => Object.keys(TIMER_NAME).map((k) => opt(k, cur, TIMER_NAME[k])).join("");
  $("lobby-box").innerHTML = `
    <div class="hall-top">
      <div class="brand small-brand"><div class="brand-mark">⚔</div><div><h1>酒馆战棋模拟器</h1><div class="brand-sub">大厅</div></div></div>
      <div class="hall-status"><span id="hall-cap"></span><span class="chip user"><span class="avatar" id="hall-avatar"></span><b id="hall-me"></b></span><button id="hall-collection">我的收藏</button><button class="ghost" id="hall-rename">改名</button><button class="ghost" id="hall-logout">退出</button></div>
    </div>
    <div class="tavern-welcome"><div class="eyebrow">YOUR NEXT ADVENTURE</div><h2>下一场传奇，从这里开始</h2><p>招募伙伴，打造阵容，争夺最后的胜利。</p></div>
    <div id="hall-room"></div>
    <div class="hall-grid">
      <div class="panel" id="hall-new">
        <div class="tabs" id="hall-tabs"><button data-tab="solo" class="${tab === "solo" ? "on" : ""}">单人对局</button><button data-tab="multi" class="${tab === "multi" ? "on" : ""}">多人房间</button></div>
        <div data-pane="solo" ${tab === "solo" ? "" : "hidden"}>
          <div class="small pane-note">选一个模式，与七位对手展开下一场对决。</div>
          ${gameOptionsHtml(L,H)}
          <div class="go-row">
            <button class="primary big" id="pl-start">开始新对局</button>
            ${inGame ? '<button class="big" id="pl-back">继续当前对局</button>' : ""}
          </div>
          <div class="small" id="hall-stats"></div>
        </div>
        <div data-pane="multi" ${tab === "multi" ? "" : "hidden"}>
          <div class="small pane-note">开一个房间，最多 ${H.room_size || 8} 位玩家同局；人不够的座位由 AI 或机器人补上。</div>
          ${gameOptionsHtml(M,H,'mr')}
          <div class="go-row"><button class="primary big" id="mr-create">创建房间</button></div>
          <div class="small">创建后把网址发给朋友，他们在“房间”列表里点“加入”。所有人同时招募，都结束回合后开战；时间到了会自动结束回合。</div>
        </div>
      </div>
      <div class="panel">
        <h3>房间</h3>
        <div class="hall-list" id="hall-rooms"></div>
      </div>
      <div class="panel">
        <h3>在线玩家</h3>
        <div class="hall-list" id="hall-players"></div>
      </div>
      <div class="panel wide rank-preview"><h3>玩家排行榜</h3><div id="hall-ranking-preview" class="bar"><span class="small">正在读取成绩…</span></div><button id="hall-rankings-open">成绩与终局阵容 →</button></div>
      <details class="panel wide">
        <summary>说明</summary>
        <div class="small" style="margin-top:6px">
          <b>计分模式</b>：可选择混合阵容、全 AI4.0、全 AI6.1 或全 AI5.0。两个英雄候选，无更换次数，随机五种种族，正常时间 ×1.5。自行操作的成绩进入对应预设的独立计分榜。<br><b>自定义模式</b>：可自由设置对手版本、英雄候选、更换次数与种族池；成绩进入其他模式榜。<br>
          <b>实验规则</b>：沿用当前版本，所有玩家第4回合选择额外任务，第6、9回合造访时空酒馆；可领取两次伙伴。无可用伙伴的英雄不参与选将。<br><b>S0</b>：8 人都是帕奇维克（开局 60 血），种族固定为野兽、恶魔、龙、机械、鱼人，没有酒馆法术、饰品和黑暗之赐。${aiFull ? "AI 没有专门练过这个简化规则。" : "AI 是在这个规则下训练的。"}<br>
          <b>多人房间</b>：所有玩家同时招募；最后一位玩家结束回合（或时间到）后一起开战。中途离开的玩家，座位交给${ai.loaded ? " AI " : "机器人"}继续打${H.game_idle_seconds ? `；对局等着你操作而你超过 ${spanText(H.game_idle_seconds)}没有任何操作，也算离开（回合被计时结束不算操作，自己结束回合后等别人不算挂机）` : ""}。房间里没有真人玩家时对局立刻结束，房间关闭。<br>
          ${TOUCH ? "操作：点酒馆的牌购买；点手牌里的随从，再点战队里的“＋”打出；点战队的牌可以出售或左右移动；牌下方的 i 看详情。" : "操作：拖动卡牌购买、打出、出售；空格结束回合，R 刷新，F 冻结。"}${aiAssist() && ai.loaded ? "自定义单人对局中可以点“AI 建议”看 AI 会怎么走。" : ""}<br>
          服务器最多同时容纳 ${H.capacity.max} 位玩家。${H.away_seconds ? `断开连接（关掉页面、断网，或页面在后台停住）超过 ${spanText(H.away_seconds)}没有恢复，就算离开，对局不保留；在这之前恢复连接可以接着打。` : ""}${H.game_idle_seconds ? `单人对局超过 ${spanText(H.game_idle_seconds)}没有操作也会结束。` : ""}
        </div>
      </details>
      <details class="panel wide">
        <summary>界面设置</summary>
        <div style="margin-top:6px">${settingsHtml()}</div>
      </details>
    </div>`;
  $("hall-rankings-open").onclick = showRankings;
  $('hall-collection').onclick = collectionOpen;
  wireGameOptions('pl'); wireGameOptions('mr');
  if (rankingsData) renderRankings();
  loadRankings();
  wireSettings($("lobby-box"));
  $("hall-tabs").querySelectorAll("button").forEach((b) => {
    b.onclick = () => {
      store(HALL_TAB_KEY, b.dataset.tab);
      $("hall-tabs").querySelectorAll("button").forEach((x) => x.classList.toggle("on", x === b));
      $("hall-new").querySelectorAll("[data-pane]").forEach((x) => (x.hidden = x.dataset.pane !== b.dataset.tab));
    };
  });
  $("pl-start").onclick = async () => {
    if (inMulti() && !confirm("开单人对局会离开现在的多人房间，确定吗？")) return;
    const L2 = readGameOptions();
    if (H.me.watching) await stopSpectating(false);
    store(PUBLIC_LOBBY_KEY, L2);
    $("pl-start").disabled = true;
    if (inMulti()) { const e = await hallPost("/api/room/leave"); if (e) { toast(e); $("pl-start").disabled = false; return; } V = null; }
    const j = await api("/api/new", L2);
    if ($("pl-start")) $("pl-start").disabled = false;
    if (j.ok === false || !V) return;
    freshGame(); beat.game = null;
    await loadHall();
    closeLobby();
  };
  if (inGame) $("pl-back").onclick = closeLobby;
  $("mr-create").onclick = async () => {
    if (inGame && !confirm("创建房间会放弃正在打的这局单人对局，确定吗？")) return;
    const o = readGameOptions('mr');
    if (H.me.watching) await stopSpectating(false);
    store(ROOM_OPT_KEY, o);
    $("mr-create").disabled = true;
    const e = await hallPost("/api/room/create", o);
    if ($("mr-create")) $("mr-create").disabled = false;
    if (e) return toast(e);
    V = null; beat.rev = beat.state = null;
    renderHall(); updateHall();
  };
  $("hall-rename").onclick = async () => {
    if (inMulti()) return toast("在多人房间里不能改名，先离开房间");
    const name = (prompt("新的用户名（" + USERNAME_RULE + "）", H.me.username) || "").trim();
    if (!name || name === H.me.username) return;
    const e = USERNAME_RE.test(name) ? await login(name) : "用户名要" + USERNAME_RULE;
    if (e) toast(e); else updateHall();
  };
  $("hall-logout").onclick = async () => {
    if ((inGame || inMulti()) && !confirm("退出会丢掉正在打的这一局，确定吗？")) return;
    try { localStorage.removeItem(NAME_KEY); } catch (e) {}
    try { const j = await (await post("/api/logout")).json(); if (j && j.capacity) H = j; } catch (e) {}
    if (H) H.me = null;
    V = null; ROOM = null; beat.rev = beat.state = beat.game = null;
    renderLogin("");
  };
}
/** Join an open room from the list. */
async function joinRoom(id) {
  if (H.me.watching) await stopSpectating(false);
  if (V && !V.lobby && !V.table && !confirm("加入房间会放弃正在打的这局单人对局，确定吗？")) return;
  const e = await hallPost("/api/room/join", { room: id });
  if (e) { toast(e); await loadHall(); return updateHall(); }
  V = null; beat.rev = beat.state = null;
  renderHall(); updateHall();
}
/** Leave the room with several players (a started game goes on without me). */
async function leaveRoom(ask) {
  if (ask && !confirm(ask)) return;
  const e = await hallPost("/api/room/leave");
  if (e) return toast(e);
  freshGame();
  V = null; ROOM = null; beat.rev = beat.state = beat.game = null; setDeadline(null);
  for (const id of ["modal", "combat-modal", "opp-modal", "detail-modal"]) $(id).classList.remove("show");
  await refresh();
  showPublicLobby();
}
async function startSpectating(room, seat) {
  const e = await hallPost("/api/spectate", {room, seat});
  if (e) return toast(e);
  freshGame(); beat.rev = beat.state = null; await refresh();
  if (V && V.spectator) closeLobby();
}
async function stopSpectating(show = true) {
  const e = await hallPost("/api/spectate/leave");
  if (e) return toast(e);
  freshGame(); V = null; ROOM = null; beat.rev = beat.state = beat.game = null; setDeadline(null);
  for (const id of ["modal", "combat-modal", "opp-modal", "detail-modal"]) $(id).classList.remove("show");
  ++combatRun; await refresh(); if (show) showPublicLobby();
}
const avatar = (name, cls) => `<span class="avatar ${cls || ""}">${esc((name || "?").slice(0, 1).toUpperCase())}</span>`;
/** The panel of the room I am in with other players: the seats, the options, the buttons. */
function roomPanelHtml(r) {
  const me = H.me, owner = me.owner;
  const seats = r.seats.map((s) => s.kind === "human"
    ? `<div class="seat human${s.username === me.username ? " me" : ""}${s.left ? " left" : ""}">${avatar(s.username)}<div class="sn">${esc(s.username)}</div><div class="ss">${s.left ? "已离开" : s.username === r.owner ? "房主" : s.username === me.username ? "你" : "玩家"}</div></div>`
    : `<div class="seat npc">${avatar(s.kind === "ai" ? "AI" : "机", "npc")}<div class="sn">${esc(s.ai_label || (s.kind === "ai" ? "AI" : "机器人"))}</div><div class="ss">空位补齐</div></div>`).join("");
  const opt = (v, cur, label, disabled) => `<option value="${v}" ${v === cur ? "selected" : ""} ${disabled ? "disabled" : ""}>${label}</option>`;
  const aiOk = H.ai && H.ai.loaded;
  let head, body, foot;
  if (r.state === "open") {
    head = `等待开始 · ${r.humans} / ${r.seats.length} 位玩家`;
    body = owner
      ? `<div class="room-opts"><label>规则 <select data-ro="scenario">${opt("experimental", r.rules, RULES_NAME.experimental)}${opt("standard", r.rules, RULES_NAME.standard)}${opt("S0", r.rules, RULES_NAME.S0)}</select></label>
          <label>空座位 <select data-ro="fill">${opt("ai", r.opponents, "AI", !aiOk)}${opt("bots", r.opponents, "简单机器人")}</select></label>
          <label>回合计时 <select data-ro="timer">${Object.keys(TIMER_NAME).map((k) => opt(k, r.timer, TIMER_NAME[k])).join("")}</select></label></div>`
      : `<div class="room-opts"><span class="pill">${RULES_NAME[r.rules] || esc(r.rules)}</span><span class="pill">空位：${fillName(r.opponents)}</span><span class="pill">${TIMER_NAME[r.timer] || ""}</span></div>`;
    foot = (owner ? `<button class="primary big" id="room-start">开始对局</button>` : `<span class="wait-note"><i class="spin"></i>等房主 ${esc(r.owner)} 开始…</span>`)
      + `<button id="room-leave">离开房间</button><span class="small">其他玩家在大厅的房间列表里点“加入”就能进来。</span>`;
  } else {
    const done = r.state === "finished";
    head = done ? "对局已结束" : `对局进行中 · 第 ${r.turn ?? 1} 回合`;
    body = `<div class="room-opts"><span class="pill">${RULES_NAME[r.rules] || esc(r.rules)}</span><span class="pill">空位：${fillName(r.opponents)}</span><span class="pill">${TIMER_NAME[r.timer] || ""}</span></div>`;
    foot = `<button class="primary big" id="room-back">${done ? "查看结果" : "回到对局"}</button><button id="room-leave">${done ? "离开房间" : "离开房间（座位交给" + fillName(r.opponents) + "）"}</button>`;
  }
  const settings=r.game_settings;
  if(settings) body+=`<div class="small room-config-summary">${settings.hero_choices} 个英雄候选 · ${settings.hero_rerolls} 次更换 · ${settings.race_pool?.length ? '候选池随机：' + settings.race_pool.map(raceName).map(esc).join(' / ') : settings.races.length ? settings.races.map(raceName).map(esc).join(' / ') + (settings.races.length<5 ? '，其余随机' : '') : '随机五种种族'}</div>`;
  return `<div class="panel room-panel"><div class="room-head"><h3>我的房间 <span class="small">${esc(r.id)}</span></h3><span class="chip ${r.state}">${head}</span></div><div class="seats">${seats}</div>${body}<div class="room-foot">${foot}</div></div>`;
}
/** The parts of the hall that change: capacity, my room, the rooms, the players, my record. */
function updateHall() {
  if (!H || !H.me || !$("hall-players")) return;
  $("hall-cap").innerHTML = capText();
  $("hall-me").textContent = H.me.username;
  $("hall-avatar").textContent = H.me.username.slice(0, 1).toUpperCase();
  // My room with other players. Redrawn only when it changed: its selects must not close
  // under the owner's hand.
  const mine = inMulti() ? myRoom() : null;
  const sig = mine ? JSON.stringify([mine, H.me]) : "";
  const host = $("hall-room");
  if (host.dataset.sig !== sig) {
    host.dataset.sig = sig;
    host.innerHTML = mine ? roomPanelHtml(mine) : "";
    if (mine) {
      const on = (id, f) => { const el = $(id); if (el) el.onclick = f; };
      on("room-start", async () => {
        $("room-start").disabled = true;
        const e = await hallPost("/api/room/start");
        if (e) { toast(e); if ($("room-start")) $("room-start").disabled = false; return; }
        heartbeat(true);
      });
      on("room-leave", () => leaveRoom(mine.state === "playing" ? "离开后你的座位会交给" + fillName(mine.opponents) + "，不能再回来。确定离开吗？" : ""));
      on("room-back", async () => { if (!V || !V.table) await refresh(); if (V && V.table) closeLobby(); });
      host.querySelectorAll("[data-ro]").forEach((el) => {
        el.onchange = async () => {
          const e = await hallPost("/api/room/options", { [el.dataset.ro]: el.value });
          if (e) toast(e);
          updateHall();
        };
      });
    }
  }
  // A player who has not done anything for three minutes is dimmed.
  const away = (p) => p.username !== H.me.username && p.idle >= 180;
  $("hall-players").innerHTML = H.players.map((p) => `<div class="hall-row${p.username === H.me.username ? " me" : ""}${away(p) ? " away" : ""}">${avatar(p.username)}<span class="who">${esc(p.username)}${p.username === H.me.username ? '<span class="small">（你）</span>' : ""}</span><span class="what">${stateText(p)}${away(p) ? ` · ${Math.floor(p.idle / 60)} 分钟没操作` : ""}</span></div>`).join("");
  const rooms = H.rooms.map((r) => {
    const here = r.id === H.me.room;
    const watches = !here && r.watchable ? r.seats.filter((seat) => seat.kind === "human" && !seat.left).map((seat) => `<button data-watch="${esc(r.id)}" data-seat="${seat.seat}">观战${r.kind === "multi" ? " " + esc(seat.username) : ""}</button>`).join("") : "";
    const others = r.seats.filter((s) => s.kind !== "human").length;
    if (r.kind !== "multi") {
      const state = r.state === "finished" ? "已结束" : r.state === "watching" ? `观看 AI · 第 ${r.turn} 回合` : `第 ${r.turn} 回合`;
      return `<div class="hall-row solo${here ? " me" : ""}">${avatar(r.owner)}<span class="who">${esc(r.owner)} <span class="small">的单人对局</span></span><span class="what">${RULES_NAME[r.rules] || esc(r.rules)} · ${others} 个${fillName(r.opponents)}<br>${state}</span>${watches}</div>`;
    }
    const state = r.state === "open" ? `<span class="chip open">等待中 ${r.humans}/${r.seats.length}</span>` : r.state === "finished" ? '<span class="chip finished">已结束</span>' : `<span class="chip playing">第 ${r.turn ?? 1} 回合</span>`;
    const names = r.members.map((n) => esc(n)).join("、") || "（都离开了）";
    const btn = here ? '<span class="small">你在这里</span>' : r.joinable ? `<button class="primary" data-join="${esc(r.id)}">加入</button>` : "";
    return `<div class="room-card${here ? " me" : ""}"><div class="rc-top"><b>${esc(r.owner)} 的房间</b>${state}</div><div class="rc-sub">${RULES_NAME[r.rules] || esc(r.rules)} · 空位 ${fillName(r.opponents)} · ${TIMER_NAME[r.timer] || ""}</div><div class="rc-bot"><span class="rc-names">${names}</span><span class="bar">${btn}${watches}</span></div></div>`;
  });
  $("hall-rooms").innerHTML = rooms.join("") || '<div class="hall-empty">还没有房间。<br>在左边创建一个，或者先开一局单人对局。</div>';
  $("hall-rooms").querySelectorAll("[data-join]").forEach((b) => (b.onclick = () => joinRoom(b.dataset.join)));
  $("hall-rooms").querySelectorAll("[data-watch]").forEach((b) => (b.onclick = () => startSpectating(b.dataset.watch, Number(b.dataset.seat))));
  $("hall-watch-back")?.remove();
  if (H.me.watching) { const controls = document.createElement("div"); controls.id = "hall-watch-back"; controls.className = "bar"; controls.innerHTML = '<button data-return>回到观战</button><button data-stop>退出观战</button>'; controls.querySelector("[data-return]").onclick = async () => { await refresh(); closeLobby(); }; controls.querySelector("[data-stop]").onclick = () => stopSpectating(); $("hall-room").appendChild(controls); }
  const st = hallStats;
  if ($("hall-stats")) $("hall-stats").textContent = st && st.all.games ? `你已打完 ${st.all.games} 局，平均第 ${st.all.mean_place.toFixed(2)} 名。` : "";
}
function closeLobby() {
  if (PUBLIC && (!V || V.lobby)) return;      // no game to go back to
  lobbyOpen = false;
  if (PUBLIC) clearInterval(hallTimer);
  $("lobby").classList.remove("show");
  dismissCombat();
  hiddenChoice = null;
  for (const id of ["modal", "combat-modal", "opp-modal", "detail-modal"]) $(id).classList.remove("show");
  render();
}

// ------------------------------------------------------------------ debug panel
function catOptions(filterFn) {
  return CAT.filter(filterFn).map((c) => `<option value="${c.id}">${esc(c.name_zh)} / ${esc(c.name)}${c.tier ? " ★" + c.tier : ""}${c.implemented ? "" : "（未实现）"}</option>`).join("");
}
function playerOf(p) { return p === V.seat ? V.me : V.opponents.find((o) => o.id === p); }

function renderDebug() {
  const d = $("debug");
  if (!dbg.enabled || !V) return;
  const openState = {};
  d.querySelectorAll("details").forEach((x) => (openState[x.dataset.k] = x.open));
  const isOpen = (k, def) => (k in openState ? openState[k] : def);
  const P = playerOf(dbg.p) || V.me;
  const isMe = dbg.p === V.seat;
  const sel = dbg.sel;
  let selTxt = "（Alt+点击卡牌，或点“拾取”后点击卡牌）";
  if (sel) {
    const pl = playerOf(sel.p);
    const zone = sel.zone === "board" ? (sel.p === V.seat ? pl.board : pl.board) : sel.zone === "hand" ? V.me.hand : V.me.shop;
    const c = zone && zone[sel.index];
    selTxt = c ? `P${sel.p} ${sel.zone}[${sel.index}] ${esc(nm(c))} ${c.atk ?? ""}/${c.hp ?? ""}` : "（已失效）";
  }
  const kwBoxes = Object.keys(KW).map((k) => `<label><input type="checkbox" class="dbg-kw" value="${k}"> ${KW[k]}</label>`).join("");
  d.innerHTML = `
  <h3>调试面板</h3>
  <div class="f"><label>操作玩家</label><select id="dbg-p">${[V.seat].concat(V.opponents.map((o) => o.id)).map((p) => `<option value="${p}" ${p === dbg.p ? "selected" : ""}>${p === V.seat ? "你 (P" + p + ")" : "P" + p}</option>`).join("")}</select></div>
  <details data-k="timer" ${isOpen("timer", true) ? "open" : ""}><summary>回合计时器</summary>
    <div class="f"><label><input type="checkbox" id="dbg-timer-on" ${timer.enabled ? "checked" : ""}> 启用</label>
      <button id="dbg-timer-pause">${timer.paused ? "继续" : "暂停"}</button>
      <label>速度</label><select id="dbg-timer-speed">${[1, 2, 5, 10, 30].map((s) => `<option ${s === timer.speed ? "selected" : ""}>${s}</option>`).join("")}</select></div>
    <div class="f"><button id="dbg-timer-add">+30 秒</button><button id="dbg-timer-5">剩 5 秒</button><button id="dbg-timer-0">立即超时</button></div>
    <div class="f"><label>每回合秒数（空=默认 40+5×回合，最多 90）</label><input type="number" id="dbg-timer-per" value="${timer.perTurn || ""}"></div>
  </details>
  <details data-k="player" ${isOpen("player", true) ? "open" : ""}><summary>玩家属性（P${dbg.p}）</summary>
    <div class="f"><label>金币</label><input type="number" id="dbg-gold" value="${isMe ? V.me.gold : ""}" placeholder="${isMe ? "" : "-"}">
      <label>酒馆等级</label><input type="number" id="dbg-tier" value="${P.tier}"></div>
    <div class="f"><label>生命</label><input type="number" id="dbg-health" value="${P.health}"><label>护甲</label><input type="number" id="dbg-armor" value="${P.armor}"></div>
    <div class="f"><label>最大金币加成</label><input type="number" id="dbg-maxg" value="${isMe ? V.me.max_gold_bonus : ""}"><label>免费刷新</label><input type="number" id="dbg-free" value="${isMe ? V.me.free_refreshes : ""}"></div>
    <div class="f"><label>下回合金币</label><input type="number" id="dbg-gnext" value="${isMe ? V.me.gold_next_turn : ""}"><label>升级费用</label><input type="number" id="dbg-upc" value="${isMe ? V.me.upgrade_price : ""}"></div>
    <div class="f"><button id="dbg-apply-player" class="primary">应用</button><button id="dbg-gold99">金币 = 99</button></div>
    <div class="f"><label>回合数</label><input type="number" id="dbg-turn" value="${V.turn}"><button id="dbg-set-turn">设置回合</button></div>
    <div class="f"><button id="dbg-countdown">倒计时全部推进到下回合触发</button><button id="dbg-reset-limits">重置本回合次数限制</button></div>
    <div class="f"><label>神明属性</label><input type="number" id="dbg-deity-a" value="${isMe && V.me.deity ? V.me.deity.atk : 1}"><input type="number" id="dbg-deity-h" value="${isMe && V.me.deity ? V.me.deity.hp : 1}"><label><input type="checkbox" id="dbg-deity-g"> 金色</label><button id="dbg-deity">设置</button></div>
  </details>
  <details data-k="add" ${isOpen("add", true) ? "open" : ""}><summary>添加卡牌</summary>
    <div class="f"><input id="dbg-search" placeholder="搜索名字/ID…" style="width:100%"></div>
    <div class="f"><select id="dbg-card" style="width:100%" size="6"></select></div>
    <div class="f"><label>位置</label><select id="dbg-zone"><option value="board">战队</option><option value="hand">手牌</option><option value="shop">酒馆</option></select>
      <label><input type="checkbox" id="dbg-golden"> 金色</label>
      <label>插入位</label><input type="number" id="dbg-pos" placeholder="末尾"></div>
    <div class="f"><label>攻</label><input type="number" id="dbg-atk" placeholder="默认"><label>血</label><input type="number" id="dbg-hp" placeholder="默认">
      <button id="dbg-add" class="primary">添加</button></div>
    <div class="f"><label>范围</label><select id="dbg-scope"><option value="pool">当前卡池</option><option value="reachable">当前可达衍生物</option><option value="all">全部资产（含历史/皮肤）</option></select>
      <label><input type="checkbox" id="dbg-only-impl" checked> 只显示已实现</label>
      <select id="dbg-kind"><option value="minion">随从</option><option value="tavern_spell">酒馆法术</option><option value="spell">特殊法术</option><option value="trinket">饰品</option><option value="hero">英雄</option><option value="hero_power">英雄技能</option><option value="quest">任务</option><option value="quest_reward">任务奖励</option><option value="dark_gift">黑暗之赐</option></select></div>
    <div class="f"><button id="dbg-give-trinket">给予所选饰品</button><button id="dbg-set-hero">换成所选英雄</button><button id="dbg-attach">把所选黑赐/卡附加到选中随从</button></div>
    <div class="f"><button id="dbg-set-hp0">设为英雄技能</button><button id="dbg-set-hp1">设为第二技能</button><button id="dbg-give-quest">开始所选任务（随机奖励）</button><button id="dbg-give-reward">给予所选任务奖励</button><button id="dbg-add-secret">放置所选奥秘</button></div>
    <div class="f small">奥秘在"特殊法术"里（ID 以 TB_Bacon_Secrets_ 开头）；暗月奖品也在"特殊法术"里，用"添加"放进手牌。</div>
  </details>
  <details data-k="edit" ${isOpen("edit", true) ? "open" : ""}><summary>编辑选中随从</summary>
    <div class="f small">${selTxt} <button id="dbg-pick">拾取</button></div>
    <div class="f"><label>攻</label><input type="number" id="dbg-e-atk"><label>血</label><input type="number" id="dbg-e-hp"><label><input type="checkbox" id="dbg-e-gold"> 变金色</label></div>
    <div class="kwgrid">${kwBoxes}</div>
    <div class="f"><label><input type="checkbox" id="dbg-e-kwset"> 覆盖关键词</label><button id="dbg-edit" class="primary">应用</button><button id="dbg-remove" class="danger">移除</button></div>
  </details>
  <details data-k="zones" ${isOpen("zones", false) ? "open" : ""}><summary>区域 / 酒馆</summary>
    <div class="f"><button id="dbg-clear-board" class="danger">清空战队</button><button id="dbg-clear-hand" class="danger">清空手牌</button><button id="dbg-clear-shop" class="danger">清空酒馆</button></div>
    <div class="f"><button id="dbg-reroll">免费刷新酒馆</button><button id="dbg-offer-l">提供小型饰品</button><button id="dbg-offer-g">提供大型饰品</button></div>
    <div class="f"><button id="dbg-tw-minor">造访小型时空扭曲</button><button id="dbg-tw-major">造访大型时空扭曲</button></div>
    <div class="f"><label>复制战队：从</label><select id="dbg-copy-from">${V.opponents.map((o) => `<option value="${o.id}">P${o.id}</option>`).join("")}</select><button id="dbg-copy">复制到 P${dbg.p} 的战队</button></div>
  </details>
  <details data-k="combat" ${isOpen("combat", true) ? "open" : ""}><summary>战斗</summary>
    <div class="f"><label>预演：P${dbg.p} vs</label><select id="dbg-prev-b">${[V.seat].concat(V.opponents.map((o) => o.id)).filter((x) => x !== dbg.p).map((x) => `<option value="${x}">P${x}</option>`).join("")}</select>
      <label>场数</label><input type="number" id="dbg-prev-n" value="300"><button id="dbg-prev">运行</button></div>
    <div id="dbg-prev-out" class="small"></div>
  </details>
  <details data-k="save" ${isOpen("save", false) ? "open" : ""}><summary>存档 / 状态</summary>
    <div class="f"><button id="dbg-undo">撤销 (${V.undo_depth})</button><button id="dbg-save">下载状态快照</button><label class="small">加载快照 <input type="file" id="dbg-load" accept=".json"></label></div>
    <div class="f"><a href="/api/snapshot" target="_blank" style="color:var(--sel)">查看原始状态 JSON</a></div>
    <div class="small">本局规则配置：棋盘 ${V.config.board_size}、手牌 ${V.config.hand_size}、卡池 ${V.config.pool_copies_by_tier.join("/")}、内容模式 ${V.config.content}</div>
  </details>`;
  // wiring
  $("dbg-p").onchange = (e) => { dbg.p = parseInt(e.target.value); dbg.sel = null; renderDebug(); };
  $("dbg-timer-on").onchange = (e) => (timer.enabled = e.target.checked);
  $("dbg-timer-pause").onclick = () => { timer.paused = !timer.paused; renderDebug(); };
  $("dbg-timer-speed").onchange = (e) => (timer.speed = parseFloat(e.target.value));
  $("dbg-timer-add").onclick = () => (timer.left += 30);
  $("dbg-timer-5").onclick = () => (timer.left = 5);
  $("dbg-timer-0").onclick = () => (timer.left = 0);
  $("dbg-timer-per").onchange = (e) => { timer.perTurn = parseInt(e.target.value) || null; timer.left = turnSeconds(V.turn); };
  const num = (id) => { const v = $(id).value; return v === "" ? undefined : parseInt(v); };
  $("dbg-apply-player").onclick = () => debugCmd({ cmd: "set_player", p: dbg.p, gold: num("dbg-gold"), tier: num("dbg-tier"), health: num("dbg-health"), armor: num("dbg-armor"), max_gold_bonus: num("dbg-maxg"), free_refreshes: num("dbg-free"), gold_next_turn: num("dbg-gnext"), upgrade_cost: num("dbg-upc") });
  $("dbg-gold99").onclick = () => debugCmd({ cmd: "set_player", p: dbg.p, gold: 99 });
  $("dbg-set-turn").onclick = () => debugCmd({ cmd: "set_turn", turn: num("dbg-turn") });
  $("dbg-countdown").onclick = () => debugCmd({ cmd: "advance_countdowns", p: dbg.p });
  $("dbg-reset-limits").onclick = () => debugCmd({ cmd: "reset_limits", p: dbg.p });
  $("dbg-deity").onclick = () => debugCmd({ cmd: "set_deity", p: dbg.p, atk: num("dbg-deity-a"), hp: num("dbg-deity-h"), golden: $("dbg-deity-g").checked });
  const fillCards = () => {
    const q = $("dbg-search").value.trim().toLowerCase();
    const kind = $("dbg-kind").value;
    const onlyImpl = $("dbg-only-impl").checked;
    const scope = $("dbg-scope").value;
    // Quests, Rewards and Hero Powers have no pool flag: the scope filter doesn't apply.
    const unscoped = ["quest", "quest_reward", "hero_power"].includes(kind);
    const inScope = (c) => unscoped || scope === "all" || (scope === "pool" ? c.in_pool && (c.kind !== "minion" || c.pool_tag <= 1) : c.reachable && !c.in_pool);
    $("dbg-card").innerHTML = catOptions((c) => inScope(c) && (kind === "trinket" ? c.kind.startsWith("trinket") : c.kind === kind) && (!onlyImpl || c.implemented) && (!q || c.name.toLowerCase().includes(q) || c.name_zh.includes(q) || c.id.toLowerCase().includes(q)) && !(kind === "minion" && c.normal_id));
  };
  $("dbg-search").oninput = fillCards; $("dbg-kind").onchange = fillCards; $("dbg-only-impl").onchange = fillCards; $("dbg-scope").onchange = fillCards;
  fillCards();
  $("dbg-add").onclick = () => {
    const id = $("dbg-card").value; if (!id) return toast("先选择一张卡");
    const c = CATBY[id];
    if (c.kind === "minion") debugCmd({ cmd: "add_minion", p: dbg.p, zone: $("dbg-zone").value, card: id, golden: $("dbg-golden").checked, pos: num("dbg-pos"), atk: num("dbg-atk"), hp: num("dbg-hp") });
    else if (c.kind.includes("spell")) debugCmd({ cmd: "add_spell", p: dbg.p, zone: $("dbg-zone").value === "shop" ? "shop" : "hand", card: id, data: c.id === "TB_BaconShop_Triples_01" ? Math.min(6, (playerOf(dbg.p).tier || 1) + 1) : 0 });
    else toast("这类卡请用下方按钮（饰品/英雄/附加）");
  };
  $("dbg-give-trinket").onclick = () => debugCmd({ cmd: "give_trinket", p: dbg.p, card: $("dbg-card").value });
  $("dbg-set-hero").onclick = () => debugCmd({ cmd: "set_hero", p: dbg.p, hero: $("dbg-card").value });
  $("dbg-set-hp0").onclick = () => debugCmd({ cmd: "set_hero_power", p: dbg.p, slot: 0, card: $("dbg-card").value });
  $("dbg-set-hp1").onclick = () => debugCmd({ cmd: "set_hero_power", p: dbg.p, slot: 1, card: $("dbg-card").value });
  $("dbg-give-quest").onclick = () => debugCmd({ cmd: "give_quest", p: dbg.p, quest: $("dbg-card").value });
  $("dbg-give-reward").onclick = () => debugCmd({ cmd: "give_reward", p: dbg.p, card: $("dbg-card").value });
  $("dbg-add-secret").onclick = () => debugCmd({ cmd: "add_secret", p: dbg.p, card: $("dbg-card").value });
  $("dbg-attach").onclick = () => { if (!dbg.sel) return toast("先选中一个随从"); debugCmd({ cmd: "edit_minion", p: dbg.sel.p, zone: dbg.sel.zone, index: dbg.sel.index, attach: $("dbg-card").value }); };
  $("dbg-pick").onclick = () => { dbg.pick = true; toast("点击一张你的随从（战队/手牌/酒馆）"); };
  if (sel) {
    const pl = playerOf(sel.p);
    const zone = sel.zone === "board" ? pl.board : sel.zone === "hand" ? V.me.hand : V.me.shop;
    const c = zone && zone[sel.index];
    if (c) {
      $("dbg-e-atk").value = c.atk ?? ""; $("dbg-e-hp").value = c.hp ?? "";
      document.querySelectorAll(".dbg-kw").forEach((b) => (b.checked = (c.keywords || []).includes(b.value)));
    }
  }
  $("dbg-edit").onclick = () => {
    if (!dbg.sel) return toast("先选中一个随从");
    const kws = $("dbg-e-kwset").checked ? Array.from(document.querySelectorAll(".dbg-kw")).filter((b) => b.checked).map((b) => b.value) : undefined;
    debugCmd({ cmd: "edit_minion", p: dbg.sel.p, zone: dbg.sel.zone, index: dbg.sel.index, atk: num("dbg-e-atk"), hp: num("dbg-e-hp"), golden: $("dbg-e-gold").checked || undefined, keywords: kws });
  };
  $("dbg-remove").onclick = async () => { if (!dbg.sel) return toast("先选中一个随从"); await debugCmd({ cmd: "remove", p: dbg.sel.p, zone: dbg.sel.zone, index: dbg.sel.index }); dbg.sel = null; renderDebug(); };
  $("dbg-clear-board").onclick = () => debugCmd({ cmd: "clear_zone", p: dbg.p, zone: "board" });
  $("dbg-clear-hand").onclick = () => debugCmd({ cmd: "clear_zone", p: dbg.p, zone: "hand" });
  $("dbg-clear-shop").onclick = () => debugCmd({ cmd: "clear_zone", p: dbg.p, zone: "shop" });
  $("dbg-reroll").onclick = () => debugCmd({ cmd: "refresh_shop", p: dbg.p });
  $("dbg-tw-minor").onclick = () => debugCmd({ cmd: "open_timewarp", p: dbg.p, major: false });
  $("dbg-tw-major").onclick = () => debugCmd({ cmd: "open_timewarp", p: dbg.p, major: true });
  $("dbg-offer-l").onclick = () => debugCmd({ cmd: "offer_trinkets", p: dbg.p, greater: false });
  $("dbg-offer-g").onclick = () => debugCmd({ cmd: "offer_trinkets", p: dbg.p, greater: true });
  $("dbg-copy").onclick = async () => {
    const from = V.opponents.find((o) => o.id === parseInt($("dbg-copy-from").value));
    await debugCmd({ cmd: "clear_zone", p: dbg.p, zone: "board" });
    for (const m of from.board) {
      await debugCmd({ cmd: "add_minion", p: dbg.p, zone: "board", card: m.golden ? (CAT.find((c) => c.cid === m.cid) || {}).id : m.id, atk: m.atk, hp: m.hp });
    }
    render();
  };
  $("dbg-prev").onclick = () => runPreview(dbg.p, parseInt($("dbg-prev-b").value), $("dbg-prev-out"));
  $("dbg-undo").onclick = async () => { await api("/api/undo", {}); render(); };
  $("dbg-save").onclick = async () => {
    const s = await (await fetch("/api/snapshot")).text();
    const a = document.createElement("a");
    a.href = URL.createObjectURL(new Blob([s], { type: "application/json" }));
    a.download = `bg_state_seed${V.seed}_turn${V.turn}.json`; a.click();
  };
  $("dbg-load").onchange = async (e) => { const f = e.target.files[0]; if (!f) return; const text = await f.text(); await api("/api/load", { state: text }); render(); };
}

document.addEventListener("keydown", async (e) => {
  if (e.repeat || e.defaultPrevented) return;
  if ((e.key === " " || e.key === "Enter") && e.target.closest("button, summary, [role=button]")) return;
  if (document.querySelector(".modal.show,#ai-hint.show")) return;
  if (["INPUT", "SELECT", "TEXTAREA"].includes(e.target.tagName) || lobbyOpen || ["feedback-modal", "guide-modal", "rankings-modal"].some((id) => $(id).classList.contains("show"))) return;
  if ((e.ctrlKey || e.metaKey) && e.key === "z") { if (!PUBLIC) { e.preventDefault(); await api("/api/undo", {}); render(); } }
  else if (e.key === "Escape") {
    if (pointerController && pointerController.active) { pointerController.cancel(); return; }
    if ($("combat-modal").classList.contains("show")) { if (showCombat.close) showCombat.close(); return; }
    for (const id of ["detail-modal", "settings-modal", "opp-modal"]) if ($(id).classList.contains("show")) { $(id).classList.remove("show"); return; }
    if (V && legal(V.actions.CANCEL)) { await act(V.actions.CANCEL); return; }
    selHand = null; repeatSpellId = null; activeSpell = null; tapKey = null; dbg.pick = false; hidePreview(); if (V) { regionVersions.delete("hero"); render(); }
  }
  else if (V && V.ai && V.ai.spectate && e.key === "n" && !$("combat-modal").classList.contains("show")) { aw.auto = false; aiStep(); }
  else if (V && V.ai && V.ai.spectate && e.key === "p" && !$("combat-modal").classList.contains("show")) aiAuto();
  else if ($("combat-modal").classList.contains("show") || $("modal").classList.contains("show")) return;
  else if (e.key === " " && V && legal(V.actions.END)) { e.preventDefault(); endTurn(); }
  else if (e.key === "r" && V && legal(V.actions.REFRESH)) act(V.actions.REFRESH);
  else if (e.key === "f" && V && legal(V.actions.FREEZE)) act(V.actions.FREEZE);
});
for (const id of ["opp-modal", "detail-modal", "settings-modal"]) $(id).addEventListener("click", (e) => { if (e.target === $(id)) $(id).classList.remove("show"); });
$("log-panel").open = S.log;
$("log-panel").addEventListener("toggle", () => { S.log = $("log-panel").open; saveS(); });

/** Touch screens retain a complete tap path alongside pointer dragging. */
function setupTouch() {
  document.body.classList.add("touch");
  $("board").dataset.empty = "点手牌里的随从，再点这里打出";
  // With a hand minion selected, a tap on the empty part of the warband plays it rightmost.
  $("board-zone").addEventListener("click", (e) => {
    if (selHand === null || !V || e.target.closest(".card, .slot, button")) return;
    const c = V.me.hand[selHand], pos = V.me.board.length;
    if (c && c.type === "minion" && canHandPlay(selHand, pos)) act(handPlayAction(selHand, pos));
  });
  document.addEventListener("click", e => {
    if (!tapKey || e.target.closest(".card,#card-actions")) return;
    tapKey = null; hideActionDock();
    if (V && !lobbyOpen) { renderBoardAndHand(); renderTavern(); }
  });
}

(async () => {
  if (PUBLIC) document.body.classList.add("public");
  setupSupport();
  setupTavernUX();
  setupCollections();
  setupOverlays();
  setupCardPreviews();
  if (TOUCH) setupTouch();
  try {
  [CAT, cachedCardArt] = await Promise.all([(await fetch("/api/catalog")).json(), fetch("/assets/card-art.json").then(r => r.ok ? r.json() : {}).catch(() => ({}))]);
  } catch (_) { toast("卡牌目录加载失败，请刷新页面重试"); return; }
  for (const c of CAT) CATBY[c.id] = c;
  setupDropZones();
  // The public server wants a username before anything else (the hall).
  if (PUBLIC) await startPublic(); else await refresh();
})();
