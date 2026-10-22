"use strict";
let cosmeticsCatalog = null, collectionDraft = null, collectionTab = 'skins', collectionHero = null, emoteSlot = 0;
let cosmeticsReady = null, emoteSeen = new Set(), emoteRoom = null;
const RANKED_OPPONENTS = Array(7).fill('ai6_7');
function rankedPresets(hall) {
  return hall?.ranked_presets || [
    ['ai6_7','全 AI6.7',RANKED_OPPONENTS],['ai6_6','全 AI6.6',Array(7).fill('ai6_6')],['ai6_5','全 AI6.5',Array(7).fill('ai6_5')],
    ['mixed','混合阵容',['ai3','ai4','ai4','ai4','ai5','ai6','ai6_1']],['ai4','全 AI4.0',Array(7).fill('ai4')],
    ['ai6_1','全 AI6.1',Array(7).fill('ai6_1')],['ai5','全 AI5.0',Array(7).fill('ai5')]
  ].map(([id,label,opponents])=>({id,label,settings:{opponent_versions:opponents},
    available:opponents.every(key=>hall?.ai_versions?.some(v=>v.id===key))}));
}
function rankedOpponentNames(preset, hall) {
  const ids=preset?.settings.opponent_versions || RANKED_OPPONENTS;
  return [...new Set(ids)].map(id=>`${ids.filter(v=>v===id).length} 个 ${hall.ai_versions?.find(v=>v.id===id)?.name || id.toUpperCase().replace('_','.')}`).join(' · ');
}

function aiStarsHtml(version) {
  if (!version || !Number.isFinite(version.stars)) return '';
  const star = '<path d="m12 2 3.1 6.3 7 .9-5.1 5 1.2 7-6.2-3.3-6.2 3.3 1.2-7-5.1-5 7-.9Z"/>';
  const icons = Array.from({length:Math.ceil(version.stars)}, (_, i) =>
    `<svg viewBox="0 0 24 24" aria-hidden="true">${i < Math.floor(version.stars) ? star : `<g class="star-empty">${star}</g><g style="clip-path:inset(0 50% 0 0)">${star}</g>`}</svg>`).join('');
  return `<span class="ai-stars" role="img" aria-label="强度：${version.stars} 星" title="${version.stars} 星">${icons}</span>`;
}

function gameOptionsHtml(value, hall, prefix = 'pl') {
  const multi = prefix === 'mr', versions = hall.ai_versions || [{id:'bot',label:'练习机器人（入门）'}];
  const defaultOpponent = versions.some(v => v.id === 'ai4') ? 'ai4' : hall.default_ai || 'bot';
  const presets = rankedPresets(hall), rankedAvailable = presets.some(p=>p.available);
  // Opening the settings starts at the server default, even if an old game saved mixed.
  const selectedPreset = presets.find(p=>p.id===(hall.default_ranked_preset || 'ai6_7') && p.available)
    || presets.find(p=>p.available) || presets[0];
  const mode = multi ? 'custom' : value.mode || (rankedAvailable ? 'ranked' : 'custom');
  const option = (id, selected, label) => `<option value="${esc(id)}" ${id === selected ? 'selected' : ''}>${esc(label)}</option>`;
  const opponents = value.opponent_versions || Array(7).fill(value.opponents === 'bots' || value.fill === 'bots' ? 'bot' : defaultOpponent);
  const aiSelect = (id, selected, attrs = '') => `<select id="${id}" ${attrs}>${versions.map(v => option(v.id,selected,v.label)).join('')}</select><span class="ai-rating" data-ai-rating-for="${id}">${aiStarsHtml(versions.find(v=>v.id===selected))}</span>`;
  return `${multi ? '<input id="mr-mode" type="hidden" value="custom">' : `<input id="pl-mode" type="hidden" value="${mode}"><div class="preset-cards"><button type="button" data-preset="ranked" ${!rankedAvailable ? 'disabled' : ''}><span class="preset-icon">♜</span><span><b>计分模式</b><small>${presets.length} 种预设 · 各自独立排行</small></span></button><button type="button" data-preset="custom"><span class="preset-icon">⚙</span><span><b>自定义模式</b><small>自由配置 · 练习与探索</small></span></button></div><div id="ranked-summary" class="ranked-summary"><label>计分预设<select id="pl-ranked-preset">${presets.map(p=>`<option value="${esc(p.id)}" ${p.id===selectedPreset.id ? 'selected' : ''} ${p.available ? '' : 'disabled'}>${esc(p.label)}</option>`).join('')}</select></label><div id="ranked-opponents">${esc(rankedOpponentNames(selectedPreset,hall))}</div>2 个候选英雄 · 不可更换<br>随机五种种族 · 正常时间 ×1.5<br>自行操作计分</div>`}
    <div id="${prefix}-custom" class="custom-options">
      <div class="option-grid"><label>游戏规则<select id="${prefix}-scen">${option('standard',value.scenario || 'standard','标准酒馆')}${option('S0',value.scenario,'简化练习')}${option('experimental',value.scenario,'任务＋伙伴＋时空扭曲（实验）')}</select></label>
      <label>整局时间<select id="${prefix}-timer">${Object.entries(TIMER_NAME).map(([id,label]) => option(id,value.timer || 'normal',label)).join('')}</select></label>
      <label>候选英雄<select id="${prefix}-heroes">${option('2',String(value.hero_choices || 2),'2 个候选')}${option('4',String(value.hero_choices || 2),'4 个候选')}</select></label>
      <label>更换次数<select id="${prefix}-rerolls">${[0,1,2,3,4].map(n => option(String(n),String(value.hero_rerolls || 0),n + ' 次')).join('')}</select></label></div>
      <details class="opponent-options"><summary>对手版本 <span>分别设置七个座位</span></summary><label class="all-opponents">统一设为${aiSelect(prefix+'-all-ai',defaultOpponent)}</label><div class="opponent-grid">${opponents.map((id,i) => `<label>对手 ${i+1}${aiSelect(prefix+'-opponent-'+i,versions.some(v=>v.id===id) ? id : defaultOpponent,`data-opponent="${i}" data-prefix="${prefix}"`)}</label>`).join('')}</div></details>
      <details class="race-options"><summary>种族设定 <span>固定种族，或从候选池随机抽取</span></summary><label>抽取方式<select id="${prefix}-race-mode"><option value="fixed" ${value.race_pool?.length ? '' : 'selected'}>指定种族，剩余随机补齐</option><option value="pool" ${value.race_pool?.length ? 'selected' : ''}>候选池随机抽取五种</option></select></label><div class="race-grid">${['BEAST','DEMON','DRAGON','ELEMENTAL','MECHANICAL','MURLOC','PIRATE','QUILBOAR','UNDEAD','ABERRATION'].map(r => `<label><input type="checkbox" data-race="${r}" data-prefix="${prefix}" ${(value.race_pool?.length ? value.race_pool : value.races || []).includes(r) ? 'checked' : ''}>${esc(raceName(r))}</label>`).join('')}</div><button type="button" id="${prefix}-random-races">全部随机</button><small id="${prefix}-race-note">不选择则完全随机。</small></details>
      ${multi ? `<input id="mr-fill" type="hidden" value="${value.fill || 'ai'}">` : `<input id="pl-opp" type="hidden" value="ai"><label class="seat-option">你的操作方式<select id="pl-seat"><option value="human" ${value.seat !== 'ai' ? 'selected' : ''}>自己操作</option><option value="ai" ${value.seat === 'ai' ? 'selected' : ''} ${!hall.ai?.assist ? 'disabled' : ''}>观看 AI 操作</option></select></label>`}
    </div>`;
}
function readGameOptions(prefix='pl') {
  const mode=$(prefix+'-mode').value;
  if(mode==='ranked') {
    const preset=rankedPresets(H).find(p=>p.id===$('pl-ranked-preset').value);
    return {mode,ranked_preset:preset.id,scenario:'standard',timer:'normal',hero_choices:2,hero_rerolls:0,races:[],
      opponent_versions:[...preset.settings.opponent_versions],opponents:'ai',spectate:false,seat:'human'};
  }
  const scenario=$(prefix+'-scen').value;
  const versions=[...document.querySelectorAll(`select[data-prefix="${prefix}"][data-opponent]`)].map(el=>el.value);
  const races=scenario==='S0' ? [] : [...document.querySelectorAll(`input[data-prefix="${prefix}"][data-race]:checked`)].map(el=>el.dataset.race);
  const pool=$(prefix+'-race-mode').value==='pool';
  return {mode:'custom',scenario,timer:$(prefix+'-timer').value,hero_choices:Number($(prefix+'-heroes').value),
    hero_rerolls:scenario==='S0' ? 0 : Number($(prefix+'-rerolls').value),
    races:pool ? [] : races,race_pool:pool ? races : [],
    opponent_versions:versions,opponents:versions.some(v=>v!=='bot') ? 'ai' : 'bots',fill:versions.some(v=>v!=='bot') ? 'ai' : 'bots',
    spectate:prefix==='pl' && $('pl-seat').value==='ai',seat:prefix==='pl' ? $('pl-seat').value : 'human'};
}
function wireGameOptions(prefix='pl') {
  const update=()=>{
    const ranked=$(prefix+'-mode').value==='ranked';
    $(prefix+'-custom').hidden=ranked;
    if(prefix==='pl') {
      $('ranked-summary').hidden=!ranked;
      document.querySelectorAll('[data-preset]').forEach(b=>b.classList.toggle('on',b.dataset.preset===$(prefix+'-mode').value));
    }
    const races=[...document.querySelectorAll(`input[data-prefix="${prefix}"][data-race]`)];
    const fixed=races.filter(el=>el.checked).length, simplified=$(prefix+'-scen').value==='S0';
    const pool=$(prefix+'-race-mode').value==='pool';
    races.forEach(el=>el.disabled=simplified || !pool && fixed>=5 && !el.checked);
    $(prefix+'-race-note').textContent=pool ? '至少选择五种，开局随机抽取五种；不选择则全部随机。' : '最多指定五种；不足五种时剩余随机补齐。';
    $(prefix+'-heroes').disabled=simplified;
    $(prefix+'-race-mode').disabled=simplified;
    $(prefix+'-rerolls').disabled=simplified;
  };
  if(prefix==='pl') {
    document.querySelectorAll('[data-preset]').forEach(b=>b.onclick=()=>{$('pl-mode').value=b.dataset.preset;update();});
    $('pl-ranked-preset').onchange=()=>{
      const preset=rankedPresets(H).find(p=>p.id===$('pl-ranked-preset').value);
      $('ranked-opponents').textContent=rankedOpponentNames(preset,H);
    };
  }
  $(prefix+'-scen').onchange=update;
  $(prefix+'-race-mode').onchange=()=>{if($(prefix+'-race-mode').value==='fixed'){[...document.querySelectorAll(`input[data-prefix="${prefix}"][data-race]:checked`)].slice(5).forEach(el=>el.checked=false);}update();};
  const refreshRating = el => {
    const box=document.querySelector(`[data-ai-rating-for="${el.id}"]`);
    if(box) box.innerHTML=aiStarsHtml(H?.ai_versions?.find(v=>v.id===el.value));
  };
  const seatSelects=[...document.querySelectorAll(`select[data-prefix="${prefix}"][data-opponent]`)];
  seatSelects.forEach(el=>el.onchange=()=>refreshRating(el));
  $(prefix+'-all-ai').onchange=()=>{
    refreshRating($(prefix+'-all-ai'));
    seatSelects.forEach(el=>{el.value=$(prefix+'-all-ai').value;refreshRating(el);});
  };
  document.querySelectorAll(`input[data-prefix="${prefix}"][data-race]`).forEach(el=>el.onchange=update);
  $(prefix+'-random-races').onclick=()=>{document.querySelectorAll(`input[data-prefix="${prefix}"][data-race]`).forEach(el=>el.checked=false);update();};
  update();
}

function setupCollections() {
  window.addEventListener('resize',positionEmoteBubbles);
  document.addEventListener('scroll',positionEmoteBubbles,true);
  cosmeticsReady=fetch('/assets/cosmetics.json').then(r=>r.ok ? r.json() : Promise.reject()).then(c=>cosmeticsCatalog=c).catch(()=>null);
  const hint=document.createElement('div'); hint.id='portrait-hint';hint.innerHTML='<span>横屏操作，阵容看得更清楚</span><button type="button">横屏 / 全屏</button>';
  hint.querySelector('button').onclick=enterLandscape;document.body.appendChild(hint);
  const button=document.createElement('button');button.id='landscape-open';button.type='button';button.textContent='全屏';button.title='进入全屏并尝试切换横屏';button.onclick=enterLandscape;
  $('support-tools').insertBefore(button,$('music-toggle'));
  window.addEventListener('resize',applyArenaViewport);
  document.addEventListener('fullscreenchange',applyArenaViewport);
  applyArenaViewport();
}
async function enterLandscape() {
  try {if(!document.fullscreenElement && document.documentElement.requestFullscreen) await document.documentElement.requestFullscreen({navigationUI:'hide'});}
  catch(e) {}
  try {if(screen.orientation?.lock) await screen.orientation.lock('landscape');}
  catch(e) {}
  if(innerHeight>innerWidth) toast('请将手机横过来，界面会自动适配横屏','info');
  applyArenaViewport();
}
function applyArenaViewport() {
  document.body.classList.toggle('in-game',!!V && !V.lobby && !lobbyOpen);
  document.body.classList.toggle('mobile-portrait',TOUCH && innerHeight>innerWidth);

}
async function collectionOpen() {
  if(!H?.me) return toast('请先登录大厅');
  await cosmeticsReady;
  if(!cosmeticsCatalog) return toast('收藏素材暂时加载失败，请刷新重试');
  const j=await api('/api/collection');if(!j.ok) return;
  collectionDraft=structuredClone(j.collection);
  collectionHero=cosmeticsCatalog.heroes.some(h=>h.id===V?.me?.hero?.id) ? V.me.hero.id : cosmeticsCatalog.heroes[0]?.id;
  renderCollection();supportOpen('collection-modal');
}
function renderCollection() {
  const c=cosmeticsCatalog,p=collectionDraft;
  const box=$('collection-box');
  box.innerHTML=`<div class="collection-header"><div><div class="eyebrow">THE COLLECTION</div><h2>我的收藏</h2></div><button class="dialog-dismiss" id="collection-close" aria-label="关闭我的收藏">×</button></div><div class="collection-toolbar"><div class="tabs"><button data-collection-tab="skins" class="${collectionTab==='skins' ? 'on' : ''}">英雄皮肤</button><button data-collection-tab="emotes" class="${collectionTab==='emotes' ? 'on' : ''}">六个表情</button></div><button class="primary" id="collection-save">保存装备</button></div><p class="small">皮肤与表情只改变外观，装备按昵称保存。</p><div id="collection-content"></div><div id="collection-status" class="small" aria-live="polite"></div>`;
  if(collectionTab==='skins') {
    const hero=c.heroes.find(h=>h.id===collectionHero);
    const skins=hero ? [{id:null,name:'默认皮肤',image:hero.image},...c.skins.filter(s=>s.hero===hero.id)] : [];
    $('collection-content').innerHTML=`<label class="collection-hero-select">选择英雄<select id="collection-hero">${c.heroes.map(h=>`<option value="${esc(h.id)}" ${h.id===collectionHero ? 'selected' : ''}>${esc(h.name)}</option>`).join('')}</select></label><div class="skin-grid">${skins.map((s,i)=>`<button class="skin-card ${p.skins[collectionHero]===s.id || !p.skins[collectionHero] && !s.id ? 'equipped' : ''}" data-skin-index="${i}"><img src="/assets/${esc(s.image)}" alt="" loading="lazy"><b>${esc(s.name)}</b><small>${p.skins[collectionHero]===s.id || !p.skins[collectionHero] && !s.id ? '已装备' : '选择皮肤'}</small></button>`).join('')}</div>`;
    $('collection-hero').onchange=()=>{collectionHero=$('collection-hero').value;renderCollection();};
    box.querySelectorAll('[data-skin-index]').forEach(b=>b.onclick=()=>{const s=skins[Number(b.dataset.skinIndex)];if(s.id) p.skins[collectionHero]=s.id;else delete p.skins[collectionHero];renderCollection();});
  } else {
    const image=id=>c.emotes.find(e=>e.id===id);
    $('collection-content').innerHTML=`<p class="small">先选择一个槽位，再点下方表情。已装备的表情会互换位置。</p><div class="equipped-emotes">${p.emotes.map((id,i)=>`<button class="emote-slot ${i===emoteSlot ? 'on' : ''}" data-emote-slot="${i}"><img src="/assets/${esc(image(id)?.image)}" alt="${esc(image(id)?.name)}"><span>${i+1}</span></button>`).join('')}</div><div class="emote-grid">${c.emotes.map(e=>`<button data-equip-emote="${e.id}" title="${esc(e.name)}"><img src="/assets/${esc(e.image)}" alt=""><span>${esc(e.name)}</span></button>`).join('')}</div>`;
    box.querySelectorAll('[data-emote-slot]').forEach(b=>b.onclick=()=>{emoteSlot=Number(b.dataset.emoteSlot);renderCollection();});
    box.querySelectorAll('[data-equip-emote]').forEach(b=>b.onclick=()=>{const id=b.dataset.equipEmote,old=p.emotes.indexOf(id);if(old>=0) [p.emotes[old],p.emotes[emoteSlot]]=[p.emotes[emoteSlot],p.emotes[old]];else p.emotes[emoteSlot]=id;renderCollection();});
  }
  box.querySelectorAll('[data-collection-tab]').forEach(b=>b.onclick=()=>{collectionTab=b.dataset.collectionTab;renderCollection();});
  $('collection-close').onclick=()=>supportClose('collection-modal');
  $('collection-save').onclick=async()=>{
    $('collection-save').disabled=true;
    const j=await api('/api/collection',p);
    if(j.ok){H.collection=j.collection;$('collection-status').textContent='装备已保存';await refresh();}
    if($('collection-save')) $('collection-save').disabled=false;
  };
}
function renderEmotes() {
  const roomKey=V.room?.id+':'+V.seed+':'+V.seat;
  if(emoteRoom!==roomKey) {emoteRoom=roomKey;emoteSeen.clear();closeHeroEmotes(false);document.querySelectorAll('.emote-bubble').forEach(el=>el.remove());}
  for(const e of V.emote_events || []) showEmoteEvent(e);
  if(!canSendHeroEmotes()) {closeHeroEmotes(false);return;}
  document.querySelectorAll('#hero-area .hero-portrait,.lb-entry.me .hero-portrait').forEach(el=>{
    el.tabIndex=0;
    el.setAttribute('role','button');
    el.setAttribute('aria-label','右键或长按英雄头像发送表情');
    el.setAttribute('aria-haspopup','dialog');
    el.title='右键发送表情 · 手机长按 · 键盘 Enter';
  });
  const picker=$('hero-emote-menu');
  if(picker && !picker.hidden) fillHeroEmotes(picker);
}
const heroEmotes = {anchor:null, press:null, timer:null};
function canSendHeroEmotes() {
  return PUBLIC && V?.me?.hero && !V.spectator && !V.ai?.spectate && !lobbyOpen;
}
function closeHeroEmotes(focus=true) {
  const picker=$('hero-emote-menu');
  if(!picker || picker.hidden) return;
  picker.hidden=true;
  if(focus) (heroEmotes.anchor?.isConnected ? heroEmotes.anchor : document.querySelector('#hero-area .hero-portrait'))?.focus({preventScroll:true});
}
function fillHeroEmotes(picker) {
  const ids=V.equipped_emotes || H?.collection?.emotes || ['1','2','3','4','5','6'];
  const signature=JSON.stringify(ids);
  if(picker.dataset.emotes===signature) return;
  picker.dataset.emotes=signature;
  picker.innerHTML=`<div class="hero-emote-heading"><b id="hero-emote-title">发送表情</b><button type="button" class="ghost" aria-label="关闭表情菜单">×</button></div><div class="hero-emote-grid">${ids.map(id=>`<button type="button" data-send-emote="${esc(id)}" aria-label="发送表情 ${esc(id)}"><img src="/assets/emotes/${encodeURIComponent(id)}.png" alt="" loading="lazy"></button>`).join('')}</div>`;
  picker.querySelector('.hero-emote-heading button').onclick=()=>closeHeroEmotes();
  picker.querySelectorAll('[data-send-emote]').forEach(b=>b.onclick=async()=>{
    if(picker.dataset.sending) return;
    picker.dataset.sending='true';
    picker.querySelectorAll('[data-send-emote]').forEach(el=>el.disabled=true);
    try {
      const j=await api('/api/emote',{emote:b.dataset.sendEmote});
      if(j.ok) {showEmoteEvent(j.event);closeHeroEmotes();}
    } finally {
      delete picker.dataset.sending;
      picker.querySelectorAll('[data-send-emote]').forEach(el=>el.disabled=false);
    }
  });
}
function openHeroEmotes(anchor) {
  if(!canSendHeroEmotes()) return;
  let picker=$('hero-emote-menu');
  if(!picker) {
    picker=document.createElement('div');picker.id='hero-emote-menu';
    picker.setAttribute('role','dialog');picker.setAttribute('aria-labelledby','hero-emote-title');
    document.body.appendChild(picker);
  }
  heroEmotes.anchor=anchor;
  fillHeroEmotes(picker);
  picker.hidden=false;
  positionHeroEmotes();
  picker.querySelector('[data-send-emote]')?.focus({preventScroll:true});
}
function positionHeroEmotes() {
  const picker=$('hero-emote-menu');
  if(!picker || picker.hidden) return;
  const anchor=heroEmotes.anchor?.isConnected ? heroEmotes.anchor : document.querySelector('#hero-area .hero-portrait');
  if(!anchor || !canSendHeroEmotes()) return closeHeroEmotes(false);
  const rect=anchor.getBoundingClientRect(), viewport=window.visualViewport;
  const left=viewport?.offsetLeft || 0, top=viewport?.offsetTop || 0;
  const width=viewport?.width || innerWidth, height=viewport?.height || innerHeight;
  picker.style.left=Math.max(left+8,Math.min(left+width-picker.offsetWidth-8,rect.left+rect.width/2-picker.offsetWidth/2))+'px';
  const above=rect.top-picker.offsetHeight-8;
  picker.style.top=Math.max(top+8,Math.min(top+height-picker.offsetHeight-8,above>=top+8 ? above : rect.bottom+8))+'px';
}
function heroEmoteTarget(target) { return target.closest?.('#hero-area .hero-portrait,.lb-entry.me .hero-portrait'); }
document.addEventListener('contextmenu',e=>{
  const anchor=heroEmoteTarget(e.target);
  if(!anchor || !canSendHeroEmotes()) return;
  e.preventDefault();openHeroEmotes(anchor);
});
document.addEventListener('pointerdown',e=>{
  if(!e.target.closest('#hero-emote-menu') && !heroEmoteTarget(e.target)) closeHeroEmotes(false);
  clearTimeout(heroEmotes.timer);
  const anchor=heroEmoteTarget(e.target);
  if(!anchor || e.pointerType==='mouse' || !canSendHeroEmotes()) return;
  heroEmotes.press={x:e.clientX,y:e.clientY,id:e.pointerId};
  heroEmotes.timer=setTimeout(()=>{
    const current=anchor.isConnected ? anchor : document.querySelector('#hero-area .hero-portrait');
    if(current) openHeroEmotes(current);
    heroEmotes.press=null;
  },500);
});
document.addEventListener('pointermove',e=>{
  const press=heroEmotes.press;
  if(press && e.pointerId===press.id && Math.hypot(e.clientX-press.x,e.clientY-press.y)>10) {clearTimeout(heroEmotes.timer);heroEmotes.press=null;}
});
for(const type of ['pointerup','pointercancel']) document.addEventListener(type,()=>{clearTimeout(heroEmotes.timer);heroEmotes.press=null;});
document.addEventListener('keydown',e=>{
  if(e.key==='Escape' && $('hero-emote-menu') && !$('hero-emote-menu').hidden) {e.preventDefault();e.stopImmediatePropagation();closeHeroEmotes();return;}
  const anchor=heroEmoteTarget(e.target);
  if(anchor && (e.key==='ContextMenu' || e.key==='F10' && e.shiftKey || e.key==='Enter' || e.key===' ')) {e.preventDefault();openHeroEmotes(anchor);}
},true);
window.addEventListener('resize',positionHeroEmotes);
document.addEventListener('scroll',e=>{if(!e.target.closest?.('#hero-emote-menu')) positionHeroEmotes();},true);
function showEmoteEvent(event) {
  const key=V?.room?.id+':'+event.id;
  if(emoteSeen.has(key)) return;
  emoteSeen.add(key);if(emoteSeen.size>100) emoteSeen.delete(emoteSeen.values().next().value);
  const lifetime=Math.min(4500,Math.max(0,6000-(Date.now()-event.at*1000)));
  if(!lifetime) return;
  // One bubble per seat. Everyone, including ourselves, uses their scoreboard row.
  document.querySelectorAll(`.emote-bubble[data-seat="${Number(event.seat)}"]`).forEach(el=>el.remove());
  const bubble=document.createElement('div');bubble.className='emote-bubble';
  bubble.dataset.seat=String(event.seat);
  bubble.innerHTML=`<img src="/assets/emotes/${encodeURIComponent(event.emote)}.png" alt="表情"><span>${esc(event.username)}</span>`;
  document.body.appendChild(bubble);positionEmoteBubbles();setTimeout(()=>bubble.remove(),lifetime);
}
function positionEmoteBubbles() {
  let moving=false;
  const list=$('leaderboard'),visible=list?.getBoundingClientRect();
  for(const bubble of document.querySelectorAll('.emote-bubble')) {
    const row=document.querySelector(`.lb-entry[data-p="${Number(bubble.dataset.seat)}"]`);
    const r=row?.getBoundingClientRect();
    bubble.hidden=!r || lobbyOpen || r.bottom<Math.max(0,visible.top) || r.top>Math.min(innerHeight,visible.bottom);
    if(bubble.hidden) continue;
    bubble.style.left=Math.max(4,Math.min(innerWidth-bubble.offsetWidth-4,r.right+6))+'px';
    bubble.style.top=Math.max(8,Math.min(innerHeight-bubble.offsetHeight-8,r.top+r.height/2-bubble.offsetHeight/2))+'px';
    moving ||= row.getAnimations().some(a=>a.playState==='running');
  }
  if(moving && !positionEmoteBubbles.frame) positionEmoteBubbles.frame=requestAnimationFrame(()=>{positionEmoteBubbles.frame=null;positionEmoteBubbles();});
}
