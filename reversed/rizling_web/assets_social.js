"use strict";
// Keep per-match effects separate from recruitment redraws and manual replay controls.
const scoreboardFx = {game:null, order:null, deferred:null, damage:null, seen:new Set()};
const connection = {failures:0, next:0, uncertain:false};
function combatKey(v, c) { return `${v.seed}:${v.seat}:${c.turn}`; }
function viewHeaders() {
  if (!V?.combats?.length) return {};
  const c = myLatestCombat();
  return c ? {'X-BG-Combat':combatKey(V,c)} : {};
}
function restoreCombat(v) {
  if (v?.combat_unchanged && V && v.seed===V.seed && v.seat===V.seat) v.combats=V.combats;
}
function connectionStatus(ok) {
  connection.failures=ok ? 0 : connection.failures+1;
  connection.next=ok ? 0 : performance.now()+Math.min(12000,1200*2**Math.min(connection.failures,4));
  let badge=$('connection-state');
  if (!badge) {badge=document.createElement('div');badge.id='connection-state';badge.setAttribute('role','status');document.body.appendChild(badge);}
  badge.hidden=ok && !connection.uncertain;
  badge.textContent=connection.uncertain ? '正在确认操作结果 · 同步后恢复操作' : navigator.onLine ? '连接中断 · 正在重新同步…' : '网络已离线 · 恢复后自动同步';
}
async function bgFetch(path, options={}, readJson=false) {
  const controller=new AbortController();
  // A turn with seven AI opponents can take longer; mutations are never retried.
  const timeout=setTimeout(()=>controller.abort(),options.method==='POST' ? 120000 : 8000);
  try {
    const response=await fetch(path,{...options,headers:{...viewHeaders(),...options.headers},signal:controller.signal});
    if (response.status>=500) throw new Error('server unavailable');
    const result=readJson ? await response.json() : response;
    connectionStatus(true);
    return result;
  } finally {clearTimeout(timeout);}
}
const bgJson=(path,options)=>bgFetch(path,options,true);
async function recoverGameView() {
  try {
    const j=await bgJson('/api/view');
    if (j.login_required) {await onDropped();return false;}
    if (j.ok===false && !j.me) return false;
    setView(j);if(V) render();
    beat.rev=null;
    return true;
  } catch(e) {connectionStatus(false);beat.rev=null;return false;}
}
function startHeartbeat() {
  const tick=async()=>{
    await heartbeat();
    const interval=(document.hidden ? 5000 : inMulti() ? 1200 : 2400)+Math.random()*200;
    startHeartbeat.timer=setTimeout(tick,Math.max(interval,connection.next-performance.now()));
  };
  clearTimeout(startHeartbeat.timer);startHeartbeat.timer=setTimeout(tick,1200+Math.random()*200);
  window.addEventListener('online',()=>{connection.next=0;beat.rev=null;heartbeat(true);});
  window.addEventListener('offline',()=>connectionStatus(false));
}
function scoreboardBeforeUpdate() {
  const key=V?.seed+':'+V?.seat;
  if (scoreboardFx.game!==key) {
    scoreboardFx.game=key;scoreboardFx.order=null;scoreboardFx.deferred=null;
  }
  return new Map([...document.querySelectorAll('.lb-entry[data-p]')].map(el=>[el.dataset.p,el.getBoundingClientRect()]));
}
function scoreboardAfterUpdate(before, order) {
  const changed=scoreboardFx.order && scoreboardFx.order!==order;
  scoreboardFx.order=order;
  if (!changed || !S.anim || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  const c=myLatestCombat();
  if ($('combat-modal').classList.contains('show') || S.autoReplay && c && c.turn!==lastCombatTurnShown && !lobbyOpen) {
    if (!scoreboardFx.deferred) scoreboardFx.deferred=before;
    return;
  }
  animateScoreboard(before);
}
function animateScoreboard(before=scoreboardFx.deferred) {
  scoreboardFx.deferred=null;
  if (!before || !S.anim || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  for (const el of document.querySelectorAll('.lb-entry[data-p]')) {
    const old=before.get(el.dataset.p),now=el.getBoundingClientRect();
    if (!old || Math.abs(old.top-now.top)<2 && Math.abs(old.left-now.left)<2) continue;
    el._rankAnimation?.cancel();
    el._rankAnimation=el.animate([{transform:`translate(${old.left-now.left}px,${old.top-now.top}px)`,filter:'brightness(1.35)'},{transform:'translate(0,0)',filter:'brightness(1)'}],{duration:480,easing:'cubic-bezier(.2,.8,.2,1)'});
  }
  positionEmoteBubbles();
}
function warbandLabel(summary) {
  if (!summary) return '尚未出战';
  if (!summary.size) return '空阵容';
  if (summary.mixed) return '混合';
  return summary.race ? raceName(summary.race)+summary.count : '无种族';
}
function warbandTitle(summary) {
  if (!summary) return '首场战斗结束后显示阵容种族';
  if (!summary.size) return '上一场出战阵容：没有随从';
  return '上一场出战阵容：'+((summary.counts || []).map(x=>raceName(x.race)+x.count).join(' · ') || '无种族')+`（${summary.size} 个随从）`;
}
function noteCombatDamage(v) {
  if (!v?.me || v.lobby) return;
  const c=(v.combats || []).filter(c=>c.a===v.seat || c.b===v.seat).sort((a,b)=>b.turn-a.turn)[0];
  if (!c) return;
  const key=combatKey(v,c);
  if (scoreboardFx.seen.has(key)) return;
  // The engine's signed last_damage includes the real cap and armor absorption.
  const damage=Math.max(0,-Number(v.me.last_damage ?? v.me.stats?.last_damage ?? 0));
  scoreboardFx.damage={key,damage,turn:c.turn};
}
function showCombatDamage(rec) {
  const hit=scoreboardFx.damage;
  if (!hit || !V || rec && combatKey(V,rec)!==hit.key || scoreboardFx.seen.has(hit.key)) return;
  scoreboardFx.seen.add(hit.key);scoreboardFx.damage=null;
  if (scoreboardFx.seen.size>100) scoreboardFx.seen.delete(scoreboardFx.seen.values().next().value);
  if (!hit.damage || lobbyOpen) return;
  document.querySelectorAll('.hero-impact').forEach(el=>el.remove());
  const replay=$('combat-modal').classList.contains('show');
  const anchor=replay ? $('cb-result') : document.querySelector('.hero-box');
  const r=anchor?.getBoundingClientRect();
  const impact=document.createElement('div');impact.className='hero-impact';impact.dataset.combat=hit.key;
  impact.setAttribute('role','status');
  impact.innerHTML=`<b>−${hit.damage}</b><span>受到 ${hit.damage} 点伤害</span>`;
  impact.style.left=Math.max(85,Math.min(innerWidth-85,r ? r.left+r.width/2 : innerWidth/2))+'px';
  document.body.appendChild(impact);
  const ctl=$('ctl').getBoundingClientRect();
  const bottom=!replay && getComputedStyle($('ctl')).position==='fixed' ? Math.min(innerHeight,ctl.top) : innerHeight;
  impact.style.top=Math.max(65,Math.min(bottom-impact.offsetHeight-12,r ? r.top-25 : innerHeight/2))+'px';
  if (S.anim && !matchMedia('(prefers-reduced-motion: reduce)').matches) {
    impact.animate([{opacity:0,transform:'translate(-50%,15px) scale(.65)'},{opacity:1,transform:'translate(-50%,-6px) scale(1.15)',offset:.15},{opacity:1,transform:'translate(-50%,-15px)',offset:.75},{opacity:0,transform:'translate(-50%,-40px)'}],{duration:2400,easing:'ease-out',fill:'forwards'});
  }
  setTimeout(()=>impact.remove(),2600);
}
function resetSocialFx() {
  scoreboardFx.game=scoreboardFx.order=scoreboardFx.deferred=scoreboardFx.damage=null;
  document.querySelectorAll('.hero-impact,.emote-bubble').forEach(el=>el.remove());
}
async function abandonGame() {
  if (actionBusy || pendingCommands || !V || V.spectator) return;
  actionBusy=true;
  try {
    const j=await api('/api/game/abandon',{});
    if (!j.ok) return;
    H=j;V=null;ROOM=null;freshGame();beat.rev=beat.state=beat.game=null;setDeadline(null);
    if (showCombat.close) showCombat.close();
    for (const el of document.querySelectorAll('.modal.show')) el.classList.remove('show');
    showPublicLobby();
  } finally {actionBusy=false;}
}
