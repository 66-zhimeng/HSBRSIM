"use strict";
// Presentation only: all purchases, plays and movements still use the simulator mask.
let pointerDrag = null, actionBusy = false, musicAudio = null, musicState = "idle";
let suppressCardClickUntil = 0, musicAttempt = 0, suspendedMusic = false;
const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)");
let cachedCardArt = {};
let tiltedCard = null;
function clearCardTilt() {
  if (!tiltedCard) return;
  tiltedCard.style.removeProperty('--tilt-x'); tiltedCard.style.removeProperty('--tilt-y');
  tiltedCard = null;
}
function cardArtUrl(id) { return cachedCardArt[id] ? `/assets/cards/${cachedCardArt[id]}.webp` : `https://art.hearthstonejson.com/v1/256x/${encodeURIComponent(id)}.jpg`; }
const musicIcon = (muted) => `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M11 4 6 8H3v8h3l5 4Z"/>${muted ? '<path d="m16 9 5 6m0-6-5 6"/>' : '<path d="M15 8a6 6 0 0 1 0 8m3-11a10 10 0 0 1 0 14"/>'}</svg>`;

// The tavern's tracks play in turn. A file that is missing or does not load is left out, and
// a single track that is left loops natively (no gap between its end and its start).
const MUSIC_TRACKS = [
  {src:"/assets/tavern-music.mp3?v=1", title:"水手之家"},
  {src:"/assets/tavern-music-2.mp3?v=1", title:"深水"},
  {src:"/assets/tavern-music-3.mp3?v=1", title:"血帆"},
];
const MUSIC_TRACK_KEY = "bg_music_track";
let musicTrack = 0;
const musicBroken = new Set();
function loadMusicTrack(index) {
  musicTrack = index;
  musicAudio.loop = MUSIC_TRACKS.length - musicBroken.size <= 1;
  musicAudio.src = MUSIC_TRACKS[index].src;
  try { localStorage.setItem(MUSIC_TRACK_KEY, String(index)); } catch (_) {}
}
/** The next track after the current one that has not failed; -1 when none is left. */
function nextMusicTrack() {
  for (let k = 1; k <= MUSIC_TRACKS.length; k++) {
    const i = (musicTrack + k) % MUSIC_TRACKS.length;
    if (!musicBroken.has(i)) return i;
  }
  return -1;
}
function musicUI() {
  const button = document.getElementById("music-toggle");
  if (button) {
    button.innerHTML = musicIcon(!S.music);
    button.setAttribute("aria-pressed", String(!!S.music));
    button.dataset.state = musicState;
    button.title = !S.music ? "开启酒馆音乐" : musicState === "blocked" ? "音乐已开启：首次点击页面后播放" : musicState === "error" ? "音乐加载失败，点击关闭后可重试" : "暂停酒馆音乐";
    button.setAttribute("aria-label", button.title);
  }
  document.querySelectorAll('[data-s="music"]').forEach(el => el.checked = !!S.music);
  document.querySelectorAll('[data-s="musicVolume"]').forEach(el => el.value = S.musicVolume);
  document.querySelectorAll(".music-volume").forEach(el => el.textContent = `${Math.round(S.musicVolume * 100)}%`);
  document.querySelectorAll(".music-status").forEach(el => el.textContent = !S.music ? "已暂停" : musicState === "playing" ? (musicAudio && !musicAudio.loop ? `轮播中：《${MUSIC_TRACKS[musicTrack].title}》` : "循环播放中") : musicState === "blocked" ? "点击页面即可开始播放" : musicState === "error" ? "暂时无法播放，可重新开启重试" : "准备播放");
}
async function playMusic() {
  if (!S.music || !musicAudio || document.hidden) return;
  const attempt = ++musicAttempt;
  try {
    await musicAudio.play();
    if (attempt !== musicAttempt || !S.music || document.hidden) { if (!S.music || document.hidden) musicAudio.pause(); return; }
    musicState = "playing";
  } catch (e) {
    if (attempt !== musicAttempt || !S.music) return;
    musicState = e.name === "NotAllowedError" ? "blocked" : "error";
  }
  musicUI();
}
function applyTavernSettings() {
  S.musicVolume = Math.min(.3, Math.max(0, Number.isFinite(Number(S.musicVolume)) ? Number(S.musicVolume) : .12));
  document.body.classList.toggle("fx3d", !!S.effects3d);
  if (!S.effects3d) clearCardTilt();
  if (pointerDrag && !S.effects3d) finishPointerDrag(false);
  if (musicAudio) {
    musicAudio.volume = S.musicVolume;
    if (!S.music) { ++musicAttempt; musicAudio.pause(); musicState = "paused"; suspendedMusic = false; }
    else if (musicAudio.paused) playMusic();
  }
  document.querySelectorAll(".card[data-zone]").forEach(el => el.draggable = false);
  musicUI();
}
function setupTavernUX() {
  applyTavernSettings();
  // Presentation only: the new runtime remains the sole owner of pointer drag actions.
  document.addEventListener('pointermove', e => {
    const card = e.target.closest?.('#shop > .card,#board > .card,#hand > .card');
    if (!card || !S.effects3d || reducedMotion.matches || e.pointerType !== 'mouse' ||
        (typeof pointerController !== 'undefined' && pointerController?.active)) return clearCardTilt();
    if (tiltedCard !== card) clearCardTilt();
    tiltedCard = card;
    const r = card.getBoundingClientRect();
    card.style.setProperty('--tilt-x', `${Math.max(-4,Math.min(4,(.5-(e.clientY-r.top)/r.height)*8))}deg`);
    card.style.setProperty('--tilt-y', `${Math.max(-5,Math.min(5,((e.clientX-r.left)/r.width-.5)*10))}deg`);
  }, {passive:true});
  document.addEventListener('pointerout', e => { if (tiltedCard && !tiltedCard.contains(e.relatedTarget)) clearCardTilt(); }, {passive:true});
  window.addEventListener('blur', clearCardTilt);
  musicAudio = document.createElement("audio");
  musicAudio.id = "tavern-audio";
  musicAudio.preload = "none";
  // A returning visitor starts with the track after the one heard last.
  let last = NaN;
  try { last = parseInt(localStorage.getItem(MUSIC_TRACK_KEY), 10); } catch (_) {}
  loadMusicTrack(last >= 0 && last < MUSIC_TRACKS.length ? (last + 1) % MUSIC_TRACKS.length : 0);
  musicAudio.volume = S.musicVolume;
  document.body.appendChild(musicAudio);
  const nav = document.getElementById("support-tools");
  const button = document.createElement("button");
  button.id = "music-toggle"; button.type = "button";
  button.onclick = () => { S.music = !S.music; if (S.music) musicBroken.clear(); saveS(); applyTavernSettings(); };
  nav.insertBefore(button, document.getElementById("rankings-open"));
  const unlock = e => { if (e.isTrusted && S.music && musicAudio.paused && musicState !== "error") playMusic(); };
  document.addEventListener("pointerdown", unlock, {passive:true});
  document.addEventListener("keydown", unlock);
  musicAudio.addEventListener("ended", () => { const i = nextMusicTrack(); if (i >= 0) { loadMusicTrack(i); musicUI(); playMusic(); } });
  musicAudio.addEventListener("error", () => {
    musicBroken.add(musicTrack);
    const i = nextMusicTrack();
    if (i < 0) { musicState = "error"; musicUI(); return; }
    loadMusicTrack(i);
    if (S.music && !document.hidden) playMusic();
  });
  document.addEventListener("visibilitychange", () => {
    if (document.hidden) { suspendedMusic = S.music && !musicAudio.paused; ++musicAttempt; musicAudio.pause(); }
    else if (suspendedMusic && S.music) { suspendedMusic = false; playMusic(); }
  });
  document.addEventListener("click", e => {
    if (performance.now() < suppressCardClickUntil && e.target.closest(".card")) { e.preventDefault(); e.stopImmediatePropagation(); }
  }, true);
  document.addEventListener("keydown", e => {
    if (e.key === "Escape" && pointerDrag) { finishPointerDrag(false); e.preventDefault(); e.stopImmediatePropagation(); }
  }, true);
  window.addEventListener("blur", () => finishPointerDrag(false));
  applyTavernSettings();
}

function heroArt(card, cls = "hero-portrait") {
  return S.art && card && card.id ? `<img class="${cls}" src="${card.skin_id ? '/assets/skins/' + encodeURIComponent(card.skin_id) + '.webp' : cardArtUrl(card.id)}" alt="" loading="lazy" onerror="this.hidden=true">` : "";
}
function itemArt(card, cls = 'relic-art') { return S.art && card?.id ? `<img class="${cls}" src="${cardArtUrl(card.id)}" alt="" loading="lazy" onerror="this.hidden=true">` : ''; }
function captureCardRects() {
  if (!S.effects3d || !S.anim || reducedMotion.matches) return null;
  return new Map([...document.querySelectorAll("#shop .card,#board .card,#hand .card")].map(el => [el.dataset.key, el.getBoundingClientRect()]));
}
function animateCardChanges(before) {
  if (!before || !S.effects3d || !S.anim || reducedMotion.matches) return;
  document.querySelectorAll("#shop .card,#board .card,#hand .card").forEach(el => {
    const old = before.get(el.dataset.key), rect = el.getBoundingClientRect();
    if (old) {
      const x = old.left - rect.left, y = old.top - rect.top;
      if (Math.abs(x) + Math.abs(y) > 2) el.animate([{transform:`translate(${x}px,${y}px) rotateY(-8deg)`},{transform:"translate(0,0) rotateY(0deg)"}], {duration:280 / S.animSpeed, easing:"cubic-bezier(.2,.8,.2,1)"});
    } else el.animate([{opacity:.4,transform:"perspective(700px) translateY(18px) rotateX(-16deg) scale(.9)"},{opacity:1,transform:"perspective(700px) translateY(0) rotateX(0) scale(1)"}], {duration:300 / S.animSpeed, easing:"cubic-bezier(.2,.8,.2,1)"});
  });
}

/** Wrapped mobile rows need y as well as x. Native desktop dragging uses the same rule. */
function cardInsertPosition(x, y) {
  const cards = [...document.querySelectorAll("#board > .card")];
  const rows = [];
  cards.forEach((el, index) => {
    const rect = el.getBoundingClientRect();
    let row = rows.find(r => Math.abs(r.top - rect.top) < rect.height / 2);
    if (!row) { row = {top:rect.top, bottom:rect.bottom, items:[]}; rows.push(row); }
    row.items.push({el, index, rect}); row.bottom = Math.max(row.bottom, rect.bottom);
  });
  let pos = 0;
  if (rows.length) {
    const row = y === undefined ? rows[0] : rows.reduce((best, r) => Math.abs(y - (r.top + r.bottom) / 2) < Math.abs(y - (best.top + best.bottom) / 2) ? r : best);
    pos = row.items[0].index;
    for (const c of row.items) if (x > c.rect.left + c.rect.width / 2) pos = c.index + 1;
  }
  return {pos, cards};
}
function dropPlan(d, zone, element, x, y, standalone = false) {
  if (!V || V.spectator || V.phase !== "recruit" || pendingTargets()) return null;
  const a = V.actions, target = element && element.closest(".card"), {pos} = cardInsertPosition(x, y);
  const checked = (action, label) => legal(action) ? {action, label} : null;
  if ((zone === "hand" || zone === "board") && d.zone === "shop") return checked(a.BUY + d.index, "购买 → 手牌");
  if (zone === "tavern" && d.zone === "board") return checked(a.SELL + d.index, "出售随从");
  if (zone === "board" && d.zone === "board") {
    const to = pos > d.index ? pos - 1 : pos;
    if (to === d.index) return {noop:true, label:"保持当前位置"};
    return checked(a.MOVE + d.index * a.N_BOARD + to, "调整随从位置");
  }
  if ((zone === "board" || zone === "tavern") && d.zone === "hand" && d.card.type === "spell") {
    if (!canHandPlay(d.index, 0)) return null;
    return {spell:d.index, target:target ? Number(target.dataset.id) : undefined, label:target ? "施放法术 → " + target.querySelector(".cname").textContent : "施放法术"};
  }
  if (zone === "board" && d.zone === "hand" && d.card.type === "minion") {
    // handPlacement also serves the cards beyond the tenth of a temporarily larger hand.
    const action = handPlacement(d.index, pos, standalone);
    const magnetic = action !== null && (typeof action === "object" ? action.kind === "magnetize" : action >= a.MAGNETIZE && action < a.MOVE);
    if (action !== null) return {action, pos, magnetic, label:magnetic ? "磁力吸附（Alt 可单独打出）" : "打出到此位置"};
  }
  return null;
}
async function executeDrop(plan) {
  if (!plan || plan.noop) return;
  if (plan.spell !== undefined) return castOn(plan.spell, plan.target);
  return act(plan.action);
}
function bindCardPointer(el, zone, index, card) {
  el.dataset.zone = zone;
  el.draggable = false;
  el.addEventListener("pointermove", e => {
    if (pointerDrag || !S.effects3d || reducedMotion.matches || e.pointerType !== "mouse") return;
    const r = el.getBoundingClientRect();
    el.style.setProperty("--tilt-x", `${(0.5 - (e.clientY - r.top) / r.height) * 8}deg`);
    el.style.setProperty("--tilt-y", `${((e.clientX - r.left) / r.width - .5) * 10}deg`);
  });
  el.addEventListener("pointerleave", () => { el.style.removeProperty("--tilt-x"); el.style.removeProperty("--tilt-y"); });
  el.addEventListener("pointerdown", e => {
    if (!S.effects3d || e.button !== 0 || pointerDrag || actionBusy || !V || V.spectator || V.phase !== "recruit" || lobbyOpen || V.me.ended_turn || V.me.pending || document.querySelector(".modal.show") || e.target.closest("button,.info,.actions")) return;
    const rect = el.getBoundingClientRect();
    pointerDrag = {zone,index,card,el,id:e.pointerId,x:e.clientX,y:e.clientY,startX:e.clientX,startY:e.clientY,offsetX:e.clientX-rect.left,offsetY:e.clientY-rect.top,rect,active:false,turn:V.turn,room:V.room && V.room.id,phase:V.phase,plan:null,raf:0};
    try { el.setPointerCapture(e.pointerId); } catch (_) { pointerDrag = null; }
  });
  el.addEventListener("pointermove", e => {
    const d = pointerDrag;
    if (!d || d.id !== e.pointerId) return;
    d.x = e.clientX; d.y = e.clientY; d.alt = e.altKey;
    if (!d.active && Math.hypot(d.x - d.startX, d.y - d.startY) < 7) return;
    e.preventDefault();
    if (!d.active) {
      d.active = true; drag = d;
      d.ghost = el.cloneNode(true); d.ghost.classList.add("drag-ghost"); d.ghost.classList.remove("dragging","selected","tapped");
      d.ghost.removeAttribute("id"); d.ghost.style.width = d.rect.width + "px"; d.ghost.style.minHeight = d.rect.height + "px"; d.ghost.style.height = d.rect.height + "px";
      document.body.appendChild(d.ghost); el.classList.add("dragging"); document.body.classList.add("card-dragging");
      d.message = document.createElement("div"); d.message.className = "drag-message"; d.message.textContent = "拖到目标区域 · 松手确认"; document.body.appendChild(d.message);
    }
    if (!d.raf) d.raf = requestAnimationFrame(() => { d.raf = 0; updatePointerDrag(d); });
  });
  el.addEventListener("pointerup", e => { if (pointerDrag && pointerDrag.id === e.pointerId) { pointerDrag.x=e.clientX; pointerDrag.y=e.clientY; pointerDrag.alt=e.altKey; finishPointerDrag(true); } });
  el.addEventListener("pointercancel", () => finishPointerDrag(false));
  el.addEventListener("lostpointercapture", () => { if (pointerDrag && pointerDrag.el === el) finishPointerDrag(false); });
}
function updatePointerDrag(d) {
  if (!d.active || pointerDrag !== d) return;
  const tilt = reducedMotion.matches ? "" : ` rotateX(10deg) rotateZ(${Math.max(-7,Math.min(7,(d.x-d.startX)/35))}deg) scale(1.06)`;
  d.ghost.style.transform = `translate3d(${d.x-d.offsetX}px,${d.y-d.offsetY-8}px,0)${tilt}`;
  document.querySelectorAll(".drop-ok,.drop-target").forEach(el => el.classList.remove("drop-ok","drop-target"));
  document.getElementById("ins-marker").style.display = "none";
  const hit = document.elementFromPoint(d.x,d.y);
  const zoneEl = hit && hit.closest("#tavern-zone,#board-zone,#hand-zone");
  const zone = zoneEl && ({"tavern-zone":"tavern","board-zone":"board","hand-zone":"hand"})[zoneEl.id];
  d.plan = zone ? dropPlan(d,zone,hit,d.x,d.y,d.alt) : null;
  d.message.textContent = d.plan ? d.plan.label : "移到酒馆、战队或手牌 · Esc 取消";
  if (d.plan) {
    zoneEl.classList.add("drop-ok");
    if (d.plan.magnetic) cardInsertPosition(d.x,d.y).cards[d.plan.pos]?.classList.add("drop-target");
    else if (d.plan.spell !== undefined) hit.closest(".card")?.classList.add("drop-target");
    else if (zone === "board" && d.zone !== "shop") showMarker(d.x,d.y);
  }
  // Auto-scroll a long phone board while holding a card near a viewport edge.
  if (TOUCH && !document.body.classList.contains("game-landscape") && !document.querySelector(".modal.show")) {
    const bottom = document.getElementById("ctl").getBoundingClientRect().top;
    const top = document.getElementById("topbar").getBoundingClientRect().bottom;
    const step = d.y > bottom-35 ? 7 : d.y < top+30 ? -7 : 0;
    if (step) { window.scrollBy(0,step); d.raf=requestAnimationFrame(() => {d.raf=0;updatePointerDrag(d);}); }
  }
}
function finishPointerDrag(commit) {
  const d = pointerDrag;
  if (!d) return;
  if (d.active && commit) updatePointerDrag(d);
  pointerDrag = null;
  if (d.raf) cancelAnimationFrame(d.raf);
  if (d.active) suppressCardClickUntil = performance.now() + 400;
  d.el.classList.remove("dragging"); d.ghost?.remove(); d.message?.remove(); document.body.classList.remove("card-dragging"); endDrag();
  try { d.el.releasePointerCapture(d.id); } catch (_) {}
  if (!commit || !d.active || !d.plan || !V || V.turn !== d.turn || V.phase !== d.phase || (V.room && V.room.id) !== d.room || V.spectator) return;
  const source = ({shop:V.me.shop,hand:V.me.hand,board:V.me.board})[d.zone]?.[d.index];
  if (!source || (source.eid ?? source.hid ?? source.sid) !== (d.card.eid ?? d.card.hid ?? d.card.sid)) return;
  executeDrop(d.plan);
}
