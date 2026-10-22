/* Browser runtime. Pure action helpers also work in Node via module.exports. */
(function (global) {
  "use strict";
  const animations = new WeakMap();
  const runningAnimations = new Set();
  const rootEvents = ["onclick", "ondblclick", "oncontextmenu", "ondragstart", "ondragend", "ondragover", "ondragleave", "ondrop", "onpointerdown", "onpointerup", "onpointerenter", "onpointerleave", "onkeydown", "onfocus", "onblur", "onload", "onerror", "onchange", "oninput"];
  const reducedMotion = () => !!(global.matchMedia && global.matchMedia("(prefers-reduced-motion: reduce)").matches);
  const keyOf = (node) => node && node.nodeType === 1 ? node.dataset.key || node.id || null : null;
  function cancelAnimations() {
    for (const animation of runningAnimations) animation.cancel();
    runningAnimations.clear();
  }
  if (global.document) global.document.addEventListener("visibilitychange", () => { if (global.document.hidden) cancelAnimations(); });
  if (global.matchMedia) {
    const preference = global.matchMedia("(prefers-reduced-motion: reduce)");
    if (preference.addEventListener) preference.addEventListener("change", () => { if (preference.matches) cancelAnimations(); });
  }
  function patchNode(prev, next) {
    if (prev === next) return prev;
    if (prev.nodeType !== next.nodeType || prev.nodeName !== next.nodeName) return next;
    if (prev.nodeType !== 1) {
      if (prev.nodeValue !== next.nodeValue) prev.nodeValue = next.nodeValue;
      return prev;
    }
    const failedImage = prev.tagName === "IMG" && prev.getAttribute("src") === next.getAttribute("src") && prev.complete && !prev.naturalWidth;
    for (const attr of Array.from(prev.attributes)) if (!next.hasAttribute(attr.name)) prev.removeAttribute(attr.name);
    for (const attr of Array.from(next.attributes)) if (prev.getAttribute(attr.name) !== attr.value) prev.setAttribute(attr.name, attr.value);
    for (const event of rootEvents) prev[event] = next[event];
    for (const field of ["_card", "_price", "_source", "_actions"]) if (field in prev || field in next) prev[field] = next[field];
    const prior = Array.from(prev.childNodes), fresh = Array.from(next.childNodes);
    for (let i = 0; i < fresh.length; i++) {
      if (!prior[i]) prev.appendChild(fresh[i]);
      else {
        const patched = patchNode(prior[i], fresh[i]);
        if (patched !== prior[i]) prior[i].replaceWith(patched);
      }
    }
    for (let i = fresh.length; i < prior.length; i++) prior[i].remove();
    if (failedImage && prev.onerror) prev.onerror({ currentTarget: prev, target: prev });
    else if (prev.tagName === "IMG" && prev.complete && prev.naturalWidth && prev.onload) prev.onload({ currentTarget: prev, target: prev });
    return prev;
  }

  /** Root handlers must use on* properties or delegation; never index-closing addEventListener. */
  function reconcile(host, freshNodes, options) {
    const fresh = Array.from(freshNodes), old = new Map(), rects = new Map();
    const focused = host.contains(host.ownerDocument.activeElement) ? host.ownerDocument.activeElement : null;
    const animate = (!options || options.animate !== false) && !reducedMotion() && !host.ownerDocument.hidden;
    for (const child of Array.from(host.children)) {
      const key = keyOf(child);
      if (key) old.set(key, child);
      if (animate && key) rects.set(key, child.getBoundingClientRect());
      const animation = animations.get(child);
      if (animation) { animation.cancel(); animations.delete(child); }
    }
    const used = new Set(), nodes = fresh.map((next) => {
      const key = keyOf(next), prev = key && old.get(key);
      if (!prev || prev.tagName !== next.tagName || used.has(prev)) return next;
      used.add(prev);
      return patchNode(prev, next);
    });
    // insertBefore only moves displaced nodes; untouched cards retain hover/focus.
    let cursor = host.firstChild;
    for (const node of nodes) {
      if (node !== cursor) host.insertBefore(node, cursor);
      cursor = node.nextSibling;
    }
    while (cursor) { const next = cursor.nextSibling; cursor.remove(); cursor = next; }
    if (focused && focused.isConnected && host.ownerDocument.activeElement !== focused) focused.focus({ preventScroll: true });
    if (animate) for (const node of nodes) {
      const before = rects.get(keyOf(node));
      if (!before || !node.animate || !node.isConnected) continue;
      const after = node.getBoundingClientRect(), x = before.left - after.left, y = before.top - after.top;
      if (Math.abs(x) + Math.abs(y) < 1) continue;
      const animation = node.animate([{ transform: `translate(${x}px, ${y}px)` }, { transform: "translate(0, 0)" }], { duration: 220, easing: "cubic-bezier(.2,.8,.2,1)" });
      animations.set(node, animation);
      runningAnimations.add(animation);
      const done = () => { runningAnimations.delete(animation); if (animations.get(node) === animation) animations.delete(node); };
      animation.onfinish = done; animation.oncancel = done;
    }
    return nodes;
  }

  function stableString(value) {
    if (value === null || typeof value !== "object") return JSON.stringify(value);
    if (Array.isArray(value)) return "[" + value.map(stableString).join(",") + "]";
    return "{" + Object.keys(value).sort().map((key) => JSON.stringify(key) + ":" + stableString(value[key])).join(",") + "}";
  }
  function identity(card, zone) {
    if (!card) return null;
    const field = zone === "shop" ? "sid" : zone === "hand" ? "hid" : "eid";
    return card[field] === undefined || card[field] === null ? null : String(card[field]);
  }
  function source(view, zone, index) {
    return { zone, id: identity((view.me[zone] || [])[index], zone) };
  }
  function find(view, ref) {
    return ref && ref.id !== null ? (view.me[ref.zone] || []).findIndex((card) => identity(card, ref.zone) === ref.id) : -1;
  }
  function boundary(board, pos) {
    return { left: identity(board[pos - 1], "board"), right: identity(board[pos], "board"), empty: board.length === 0 };
  }
  function resolveBoundary(board, anchor) {
    if (!anchor) return null;
    const right = anchor.right === null ? -1 : board.findIndex((card) => identity(card, "board") === anchor.right);
    if (right >= 0) return right;
    const left = anchor.left === null ? -1 : board.findIndex((card) => identity(card, "board") === anchor.left);
    if (left >= 0) return left + 1;
    return anchor.empty && board.length === 0 ? 0 : null;
  }
  function pendingKey(view) { return stableString([view.me.pending, view.me.casting, view.me.pending_id]); }
  function capture(view, action) {
    if (!view || !view.me || view.spectator) return null;
    const A = view.actions, me = view.me;
    const intent = { seed: view.seed, turn: view.turn, phase: view.phase, seat: view.seat };
    if (action && typeof action === "object") {
      if (action.kind === "claim_buddy") {
        if (!me.buddy_button || !me.buddy_button.available || me.buddy_button.claims !== action.expected_claims) return null;
        return Object.assign(intent, { kind: "claim_buddy", claims: action.expected_claims });
      }
      const index = me.hand.findIndex(c => c.hid === action.hid), card = me.hand[index];
      if (index < 10 || !card || me.pending) return null;
      if (action.kind === "play" && (card.web_play_positions || []).includes(action.pos)) {
        return Object.assign(intent, { kind: "PLAY", source: source(view, "hand", index), spell: card.type === "spell", position: card.type === "spell" ? null : boundary(me.board, action.pos) });
      }
      const target = me.board.findIndex(c => c.eid === action.board);
      if (action.kind === "magnetize" && (card.web_magnetize || []).includes(target)) {
        return Object.assign(intent, { kind: "MAGNETIZE", source: source(view, "hand", index), target: source(view, "board", target) });
      }
      return null;
    }
    if (!Number.isInteger(action) || !view.mask || view.mask[action] !== 1) return null;
    if (action < A.BUY) {
      intent.kind = "constant"; intent.action = action;
      if (action === A.CANCEL) intent.pending = pendingKey(view);
    } else if (action < A.SELL) { intent.kind = "BUY"; intent.source = source(view, "shop", action - A.BUY); }
    else if (action < A.ACTIVATE) { intent.kind = "SELL"; intent.source = source(view, "board", action - A.SELL); }
    else if (action < A.CHOOSE) { intent.kind = "ACTIVATE"; intent.source = source(view, "board", action - A.ACTIVATE); }
    else if (action < A.PLAY) {
      intent.kind = "CHOOSE"; intent.option = action - A.CHOOSE; intent.pending = pendingKey(view);
      const pending = me.pending || {};
      if (pending.kind === "target") intent.target = pending.targets && pending.targets[intent.option];
      else intent.optionValue = stableString((pending.options || [])[intent.option]);
    } else if (action < A.MAGNETIZE) {
      const n = action - A.PLAY, hand = Math.floor(n / A.N_POS), card = me.hand[hand];
      intent.kind = "PLAY"; intent.source = source(view, "hand", hand);
      intent.spell = !!card && card.type === "spell";
      intent.position = intent.spell ? null : boundary(me.board, n % A.N_POS);
    } else if (action < A.MOVE) {
      const n = action - A.MAGNETIZE;
      intent.kind = "MAGNETIZE"; intent.source = source(view, "hand", Math.floor(n / A.N_BOARD));
      intent.target = source(view, "board", n % A.N_BOARD);
    } else {
      const n = action - A.MOVE, from = Math.floor(n / A.N_BOARD), to = n % A.N_BOARD;
      intent.kind = "MOVE"; intent.source = source(view, "board", from);
      intent.position = boundary(me.board.filter((_, i) => i !== from), to);
    }
    return intent.source && intent.source.id === null ? null : intent;
  }
  function resolve(view, intent) {
    if (!view || !view.me || view.spectator || !intent || ["seed", "turn", "phase", "seat"].some((key) => view[key] !== intent[key])) return null;
    const A = view.actions, me = view.me, index = intent.source ? find(view, intent.source) : -1;
    if (intent.source && index < 0) return null;
    let action = null;
    if (intent.kind === "claim_buddy") {
      return me.buddy_button && me.buddy_button.available && me.buddy_button.claims === intent.claims
        ? { kind: "claim_buddy", expected_claims: intent.claims } : null;
    }
    if (intent.kind === "constant") {
      if (intent.pending !== undefined && intent.pending !== pendingKey(view)) return null;
      action = intent.action;
    } else if (["BUY", "SELL", "ACTIVATE"].includes(intent.kind)) action = A[intent.kind] + index;
    else if (intent.kind === "CHOOSE") {
      if (intent.pending !== pendingKey(view)) return null;
      const pending = me.pending || {};
      let option = intent.option;
      if (pending.kind === "target") {
        option = (pending.targets || []).indexOf(intent.target);
        if (option < 0) return null;
      } else if (stableString((pending.options || [])[option]) !== intent.optionValue) return null;
      action = A.CHOOSE + option;
    } else if (intent.kind === "PLAY") {
      const pos = intent.spell ? 0 : resolveBoundary(me.board, intent.position);
      if (pos === null || pos >= A.N_POS) return null;
      if (index >= 10) return !me.pending && (me.hand[index].web_play_positions || []).includes(pos) ? { kind: "play", hid: me.hand[index].hid, pos } : null;
      action = A.PLAY + index * A.N_POS + pos;
    } else if (intent.kind === "MAGNETIZE") {
      const target = find(view, intent.target);
      if (target < 0) return null;
      if (index >= 10) return !me.pending && (me.hand[index].web_magnetize || []).includes(target) ? { kind: "magnetize", hid: me.hand[index].hid, board: me.board[target].eid } : null;
      action = A.MAGNETIZE + index * A.N_BOARD + target;
    } else if (intent.kind === "MOVE") {
      const pos = resolveBoundary(me.board.filter((_, i) => i !== index), intent.position);
      if (pos === null || pos === index || pos >= A.N_BOARD) return null;
      action = A.MOVE + index * A.N_BOARD + pos;
    }
    return Number.isInteger(action) && view.mask && view.mask[action] === 1 ? action : null;
  }

  /** One delegated pointer listener, with touch hold, drag ghost, and symmetric cleanup. */
  function createPointerDrag(options) {
    const root = options.root, doc = root.ownerDocument, win = doc.defaultView;
    let state = null, frame = 0, suppressUntil = 0, destroyed = false;
    const clock = () => win.performance.now();
    function payload(event) {
      return Object.assign({}, state.info, { x: event.clientX, y: event.clientY, event, pointerType: state.type });
    }
    function moveGhost() {
      frame = 0;
      if (!state || !state.ghost) return;
      const lift = state.type === "touch" ? 60 : 24;
      state.ghost.style.transform = `translate3d(${state.x - state.width / 2}px, ${state.y - state.height / 2 - lift}px,0) rotate(3deg)`;
    }
    function cleanup(event, drop) {
      if (!state) return;
      const previous = state, detail = payload(event || previous.last);
      state = null;
      clearTimeout(previous.hold);
      if (frame) { win.cancelAnimationFrame(frame); frame = 0; }
      if (previous.ghost) previous.ghost.remove();
      previous.info.element.classList.remove("pointer-drag-source");
      doc.body.classList.remove("bg-pointer-dragging");
      try { if (root.hasPointerCapture(previous.pointer)) root.releasePointerCapture(previous.pointer); } catch (_) { /* detached root */ }
      if (previous.active) {
        suppressUntil = clock() + 450;
        if (drop && options.onDrop) options.onDrop(detail);
        else if (options.onCancel) options.onCancel(detail);
      }
    }
    function start(event) {
      if (!state || state.active) return;
      const detail = payload(event);
      if (options.onStart && options.onStart(detail) === false) { cleanup(event, false); return; }
      if (!state) return;
      state.active = true;
      const element = state.info.element, rect = element.getBoundingClientRect();
      state.width = rect.width; state.height = rect.height;
      const ghost = element.cloneNode(true);
      ghost.removeAttribute("id"); ghost.querySelectorAll("[id]").forEach((node) => node.removeAttribute("id"));
      ghost.className += " bg-drag-ghost";
      ghost.setAttribute("aria-hidden", "true");
      ghost.style.cssText = `position:fixed;left:0;top:0;width:${rect.width}px;height:${rect.height}px;pointer-events:none;z-index:10020;opacity:.9;margin:0;transition:none;will-change:transform;`;
      doc.body.appendChild(ghost); state.ghost = ghost;
      element.classList.add("pointer-drag-source"); doc.body.classList.add("bg-pointer-dragging");
      try { root.setPointerCapture(state.pointer); } catch (_) { /* optional browser support */ }
      moveGhost();
    }
    function down(event) {
      if (state || destroyed || event.button !== 0 || event.isPrimary === false) return;
      if (event.target.closest("button,a,input,select,textarea,summary,[data-no-drag],.detail-btn,.details")) return;
      const element = event.target.closest(".card[data-zone]");
      if (!element || !root.contains(element)) return;
      const info = { element, zone: element.dataset.zone, index: Number(element.dataset.index ?? element.dataset.idx), id: element.dataset.id, key: element.dataset.key };
      if (options.canStart && !options.canStart(Object.assign({ event }, info))) return;
      state = { info, pointer: event.pointerId, type: event.pointerType, x: event.clientX, y: event.clientY, startX: event.clientX, startY: event.clientY, at: clock(), active: false, last: event };
      // A stationary hold arms the next movement. Early scrolling is left to the browser.
      if (event.pointerType === "touch") state.hold = setTimeout(() => { if (state) state.armed = true; }, 170);
    }
    function move(event) {
      if (!state || event.pointerId !== state.pointer) return;
      state.last = event; state.x = event.clientX; state.y = event.clientY;
      if (!state.active) {
        const distance = Math.hypot(state.x - state.startX, state.y - state.startY);
        if (distance < (state.type === "touch" ? 10 : 6)) return;
        if (state.type === "touch" && !state.armed) { cleanup(event, false); return; }
        start(event);
      }
      if (!state || !state.active) return;
      if (event.cancelable) event.preventDefault();
      if (!frame) frame = win.requestAnimationFrame(moveGhost);
      if (options.onMove) options.onMove(payload(event));
    }
    function up(event) { if (state && event.pointerId === state.pointer) cleanup(event, true); }
    function cancel(event) { if (state && (!event.pointerId || event.pointerId === state.pointer)) cleanup(state.last, false); }
    function lostCapture(event) {
      // Touch initially captures the pressed card implicitly. Transferring that capture
      // to the delegated root emits a bubbling loss from the card; it is not a cancelled
      // drag. Only loss by the actual capture owner ends our gesture.
      if (event.target === root && !root.hasPointerCapture(event.pointerId)) cancel(event);
    }
    function key(event) { if (event.key === "Escape" && state) { event.preventDefault(); cleanup(state.last, false); } }
    function click(event) { if (clock() < suppressUntil) { suppressUntil = 0; event.preventDefault(); event.stopImmediatePropagation(); } }
    function nativeDrag(event) { if (event.target.closest(".card[data-zone]")) event.preventDefault(); }
    function visibility() { if (doc.hidden) cancel({}); }
    root.addEventListener("pointerdown", down);
    doc.addEventListener("pointermove", move, { passive: false });
    doc.addEventListener("pointerup", up);
    doc.addEventListener("pointercancel", cancel);
    root.addEventListener("lostpointercapture", lostCapture);
    root.addEventListener("click", click, true);
    root.addEventListener("dragstart", nativeDrag);
    doc.addEventListener("keydown", key);
    doc.addEventListener("visibilitychange", visibility);
    win.addEventListener("blur", cancel);
    return {
      cancel: () => cancel({}),
      get active() { return !!state && state.active; },
      destroy() {
        cancel({}); destroyed = true;
        root.removeEventListener("pointerdown", down);
        doc.removeEventListener("pointermove", move); doc.removeEventListener("pointerup", up); doc.removeEventListener("pointercancel", cancel);
        root.removeEventListener("lostpointercapture", lostCapture); root.removeEventListener("click", click, true); root.removeEventListener("dragstart", nativeDrag);
        doc.removeEventListener("keydown", key); doc.removeEventListener("visibilitychange", visibility); win.removeEventListener("blur", cancel);
      }
    };
  }
  const api = { reconcile, capture, resolve, createPointerDrag, reducedMotion, cancelAnimations, pendingKey };
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  global.BGEngine = api;
})(typeof window !== "undefined" ? window : globalThis);
