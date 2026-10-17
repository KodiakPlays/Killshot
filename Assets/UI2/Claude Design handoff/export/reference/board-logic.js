// Star Wolf board logic, verbatim from Star Wolf Board Live.dc.html. Reference only.

const TOOLS = ['CURSOR','POINT','RULER','COMPASS','ERASE'];

const MOUNTS = [
  { id:'RGA', arc:42,  halfArc:80,  range:30000, label:'RAILGUN TURRET A', short:'RG-A', loaded:'2 / 2', ammo:'22' },
  { id:'RGB', arc:250, halfArc:80,  range:30000, label:'RAILGUN TURRET B', short:'RG-B', loaded:'1 / 2', ammo:'19' },
  { id:'PDA', arc:42,  halfArc:35,  range:4000,  label:'PDC GROUP A',      short:'PDC-A',  loaded:'BELT',  ammo:'2400' },
  { id:'PDB', arc:200, halfArc:35,  range:4000,  label:'PDC GROUP B',      short:'PDC-B',  loaded:'BELT',  ammo:'2180' },
  { id:'VLS', arc:0,   halfArc:180, range:40000, label:'MISSILE VLS',      short:'VLS',         loaded:'8 / 8', ammo:'8' }
];

const SYSTEMS = [
  { id:'RG', label:'RAILGUN', mounts:['RGA','RGB'], shot:70 },
  { id:'PD', label:'PDC',     mounts:['PDA','PDB'], shot:25 },
  { id:'MS', label:'MISSILE', mounts:['VLS'],       shot:40 }
];
const WARHEADS = ['HE', 'EMP', 'PENETRATOR'];
const QUADRANTS = [
  { id:'fwd',  label:'FWD' },
  { id:'stbd', label:'STBD' },
  { id:'aft',  label:'AFT' },
  { id:'port', label:'PORT' }
];

const SWITCHES = [
  { id:'load',    label:'LOAD',    sub:'ROUND IN RAIL' },
  { id:'angle',   label:'ANGLE',   sub:'ELEV SET' },
  { id:'bearing', label:'BEARING', sub:'ON AXIS' },
  { id:'arm',     label:'ARM',     sub:'GATE LIVE' }
];
const CLEAR_SW = { load:false, angle:false, bearing:false, arm:false };

const ORDERS_INTCP = [
  { id:'destroy', label:'DESTROY' },
  { id:'disable', label:'DISABLE' },
  { id:'escort',  label:'ESCORT' }
];
const ORDERS_ASSLT = [
  { id:'destroy', label:'DESTROY' },
  { id:'disable', label:'DISABLE' },
  { id:'secure',  label:'SECURE' }
];

const SQUADS = [
  { key:'gw', name:'INTERCEPTOR', short:'INTCP', orders:ORDERS_INTCP, type:'STARFIGHTER', role:'INTERCEPT / ESCORT', craft:[
    { tag:'01', pilot:'HALLORAN', fuel:100, state:'READY' },
    { tag:'02', pilot:'VOSS',     fuel:100, state:'READY' },
    { tag:'03', pilot:'IDE',      fuel:96,  state:'READY' },
    { tag:'04', pilot:'MARCHETTI',fuel:88,  state:'READY' },
    { tag:'05', pilot:'REYES',    fuel:41,  state:'REARM' },
    { tag:'06', pilot:'OKONKWO',  fuel:0,   state:'DOWN'  }
  ]},
  { key:'ih', name:'ASSAULT BOAT', short:'ASSLT', orders:ORDERS_ASSLT, type:'ASSAULT BOAT', role:'BOARDING / SECURE', craft:[
    { tag:'11', pilot:'DRAKOS',   fuel:100, state:'READY' },
    { tag:'12', pilot:'SANTOS',   fuel:100, state:'READY' },
    { tag:'13', pilot:'ELLERY',   fuel:74,  state:'READY' },
    { tag:'14', pilot:'BRENNAN',  fuel:62,  state:'REARM' }
  ]}
];
const LAUNCH_STAGES = [
  { id:'crew', label:'CREW',     rate:22 },
  { id:'fuel', label:'FUEL',     rate:18 },
  { id:'ord',  label:'ORDNANCE', rate:14 },
  { id:'evac', label:'EVAC',     rate:20 },
  { id:'doors',label:'DOORS',    rate:26 },
  { id:'cat',  label:'CATAPULT', rate:12 }
];
const CLEAR_LAUNCH = { crew:false, fuel:false, ord:false, evac:false, doors:false, cat:false };
const CLEAR_PROG   = { crew:0, fuel:0, ord:0, evac:0, doors:0, cat:0 };



const PARTY_TICKS = 6;
const REACTOR_UNITS = 10;
const BUS_MAX = 5;
const BUS_MODES = ['VENT', 'STANDBY', 'DRAW'];
const BUSES = [
  { key:'eng', name:'ENGINES',   short:'ENG' },
  { key:'wep', name:'WEAPONS',   short:'WEP' },
  { key:'sen', name:'SENSORS',   short:'SEN' },
  { key:'aux', name:'AUXILIARY', short:'AUX' }
];

const CLASSES = { t07:'CRUISER', c17:'CORVETTE', c22:'FRIGATE', ftr:'FIGHTER', bc1:'BOAT' };
const TRACKS = [
  { key:'t07', name:'TRACK 07',   bearing:48,  range:24300, spd:1.10, hostile:true,  sig:['THERM','RAD'] },
  { key:'c17', name:'CONTACT 17', bearing:325, range:31000, spd:0.82, hostile:true,  sig:['RAD','EM'] },
  { key:'c22', name:'CONTACT 22', bearing:149, range:16800, spd:1.44, hostile:true,  sig:['THERM','EM'] },
  { key:'ftr', name:'FTR-2',      bearing:241, range:12200, spd:2.60, hostile:false, own:true, sig:['THERM','UV'] },
  { key:'bc1', name:'BC-01',      bearing:71,  range:9500,  spd:0.41, hostile:false, own:true, sig:['UV','EM'] }
];

const COMPARTMENTS = [
  { key:'mag', tag:'MAG', name:'MAGAZINE',    x:'43%',   w:'14%', y:'0%',   h:'9%' },
  { key:'bri', tag:'BRI', name:'BRIDGE',      x:'42%',   w:'16%', y:'20%',  h:'9%' },
  { key:'hab', tag:'HAB', name:'HABITATION',  x:'41.5%', w:'17%', y:'40%',  h:'9%' },
  { key:'hgr', tag:'HGR', name:'HANGAR BAY',  x:'40.5%', w:'19%', y:'60%',  h:'9%' },
  { key:'rct', tag:'RCT', name:'REACTOR',     x:'37%',   w:'26%', y:'80%',  h:'9%' },
  { key:'eng', tag:'ENG', name:'ENGINE ROOM', x:'35%',   w:'30%', y:'100%', h:'9%' }
];


const AP_ATT = [
  { key:'NOSE', label:'NOSE ON', note:'NOSE ON TARGET',       cell:'1 / 2', ax:'8px',  ay:'-2px', clip:'polygon(50% 0%, 100% 100%, 0 100%)' },
  { key:'STBD', label:'STBD BM', note:'STBD BEAM TO TARGET',  cell:'2 / 3', ax:'17px', ay:'8px',  clip:'polygon(100% 50%, 0 0, 0 100%)' },
  { key:'PORT', label:'PORT BM', note:'PORT BEAM TO TARGET',  cell:'2 / 1', ax:'-1px', ay:'8px',  clip:'polygon(0 50%, 100% 0, 100% 100%)' },
  { key:'TAIL', label:'TAIL ON', note:'TAIL TO TARGET',       cell:'3 / 2', ax:'8px',  ay:'17px', clip:'polygon(50% 100%, 100% 0, 0 0)' }
];

const AP_MODES = [
  { key:'DIRECT',  label:'DIRECT',   g:'\u2014' },
  { key:'FLANK',   label:'FLANKING', g:'+0.5 G' },
  { key:'EVASIVE', label:'EVASIVE',  g:'+1.2 G' }
];

const AP_RANGES = [2000, 4000, 8000, 12000, 18000, 24000];

const SIGNAL = { chan: 2, freq: 72.2 };
const CIPHER_KEY = [22, 14, 11, 5];
const MESSAGE = 'HOSTILE FLEET INBOUND VECTOR TWO SEVEN NINE HOLD STATION AWAIT RELIEF';
const AZ = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';

function vigenere(text, key, dir) {
  let li = 0;
  return text.split('').map(ch => {
    const i = AZ.indexOf(ch);
    if (i < 0) return ch;
    const k = key[li % key.length];
    li += 1;
    return AZ[((i + dir * k) % 26 + 26) % 26];
  }).join('');
}

const CIPHERTEXT = vigenere(MESSAGE, CIPHER_KEY, 1);

const ENG_REPAIRS = [
  { id:'reboot', label:'REBOOT',  flex:3 },
  { id:'party',  label:'REPAIR',  flex:1 },
  { id:'seal',   label:'SEAL',    flex:1 },
  { id:'vent',   label:'VENT',    flex:1 },
  { id:'dum1',   label:'DUMMY 1', flex:1, tbd:true },
  { id:'dum2',   label:'DUMMY 2', flex:1, tbd:true }
];

const ENG_ORDERS = [
  { id:'party', label:'PARTY', sub:'+1 INTEG',    cost:30 },
  { id:'seal',  label:'SEAL',  sub:'RESTORE ATM', cost:25 },
  { id:'vent',  label:'VENT',  sub:'PURGE COMPT', cost:15 },
  { id:'fire',  label:'FIRE',  sub:'SUPPRESS',    cost:35, tbd:true },
  { id:'blkhd', label:'BLKHD', sub:'SEAL FRAME',  cost:20, tbd:true },
  { id:'scrub', label:'SCRUB', sub:'RAD PURGE',   cost:40, tbd:true }
];

const ALERTS = ['CONDITION 3', 'CONDITION 2', 'CONDITION 1'];


const SPECTRA = [
  { key:'THERM', label:'THERM', note:'DRIVE PLUMES &middot; RADIATORS' },
  { key:'RAD',   label:'RAD',   note:'REACTORS &middot; WARHEADS' },
  { key:'UV',    label:'UV',    note:'COLD HULL OCCLUSION &middot; SHORT' },
  { key:'EM',    label:'EM',    note:'EMITTERS &middot; COMMS TRAFFIC' }
];

function angDiff(a, b) { let d = Math.abs(((a - b) % 360 + 540) % 360 - 180); return d; }

class Component extends DCLogic {
  steelRef = React.createRef();
  scopeRef = React.createRef();
  scopeT = 0;
  state = {
    zoom: 1, tool: 'CURSOR', weaponsOpen: false, mount: 'RGA', target: 't07',
    sw: { load: false, angle: false, bearing: false, arm: false },
    scopeZoom: 1.4, lever: 0,
    paused: true, rate: 1, missionT: 7412, powerOpen: false,
    senOpen: false, senBoost: 2, coneBrg: 62, coneArc: 40, idd: { bc1: true },
    spectrum: 'THERM', cls: { bc1: true }, dradis: true, active: false, ownSig: 0, wepCharge: 0,
    engOpen: false, engSel: 'rct', alert: 0,
    // the board opens undamaged: ten units available, nothing gated.
    // damage is something that happens, not the state the operator learns first.
    comp: {
      mag: { integ: 4, crew: 4,  atmo: 'VENTED', cond: null },
      bri: { integ: 5, crew: 9,  atmo: 'SEALED', cond: null },
      hab: { integ: 3, crew: 18, atmo: 'SEALED', cond: 'RAD' },
      hgr: { integ: 2, crew: 7,  atmo: 'SEALED', cond: 'FIRE' },
      rct: { integ: 5, crew: 6,  atmo: 'SEALED', cond: null },
      eng: { integ: 5, crew: 8,  atmo: 'SEALED', cond: null }
    },
    dcThrown: {}, party: null, activeDial: null,
    marks: [], draft: null, markSeq: 0,
    auxCharge: 0, orderFail: null,
    commsOpen: false, chan: 1, freq: 34.0, locked: false, dials: [0, 0, 0, 0],
    apAtt: 'NOSE', apRange: 2, apMode: 'DIRECT',
    hangarOpen: false, doorsOpen: false, squad: 'gw', order: 'destroy', launched: {},
    lsw: { crew:false, fuel:false, ord:false, evac:false, doors:false, cat:false }, lbrg: 62,
    busFill: { eng: 0, wep: 0, sen: 0, aux: 0 },
    busMode: { eng: 'STANDBY', wep: 'STANDBY', sen: 'STANDBY', aux: 'STANDBY' }, helmOpen: false, ordersOpen: false, hdg: 83, thr: 40, gridTint: (this.props && this.props.gridTint) || 'WHITE',
    ordHdg: 83, sys: 'RG', warhead: 0, salvo: 4, loaders: false, scopeDown: false,
    quads: { fwd: true, stbd: true, aft: false, port: false },
    sol: { RG: null, PD: null, MS: null },
    lprog: { crew:0, fuel:0, ord:0, evac:0, doors:0, cat:0 },
    fired: false,
    acked: false
  };

  ack = () => this.setState({ acked: true });

  togglePause = () => this.setState(s => ({ paused: !s.paused }));
  toggleHelm = () => this.setState(s => ({ helmOpen: !s.helmOpen }));
  toggleOrders = () => this.setState(s => ({ ordersOpen: !s.ordersOpen }));
  cycleComp = () => this.setState(s => {
    const i = COMPARTMENTS.findIndex(c => c.key === s.engSel);
    return { engSel: COMPARTMENTS[(i + 1) % COMPARTMENTS.length].key };
  });
  togglePower = () => this.setState(s => ({ commsOpen: false, engOpen: false, powerOpen: !s.powerOpen, weaponsOpen: false, senOpen: false, hangarOpen: false }));
  toggleSensors = () => this.setState(s => ({ commsOpen: false, engOpen: false, senOpen: !s.senOpen, weaponsOpen: false, powerOpen: false, hangarOpen: false }));

  setAtt = (k) => () => this.setState(st => ({ apAtt: st.apAtt === k ? null : k }));
  setApMode = (k) => () => this.setState({ apMode: k });
  apRangeUp = () => this.setState(st => ({ apRange: Math.min(AP_RANGES.length - 1, st.apRange + 1) }));
  apRangeDown = () => this.setState(st => ({ apRange: Math.max(0, st.apRange - 1) }));

  stabNum() {
    return Math.max(0, Math.min(100, 100 - (this.gLoadNum() - 1) * 45));
  }

  gLoadNum() {
    const st = this.state;
    let g = 1 + (st.thr / 100) * 1.0;
    if (st.apMode === 'FLANK') g += 0.5;
    if (st.apMode === 'EVASIVE') g += 1.2;
    return g;
  }

  toggleComms = () => this.setState(st => ({ commsOpen: !st.commsOpen, weaponsOpen: false, powerOpen: false, senOpen: false, hangarOpen: false, engOpen: false }));
  cycleChan = () => this.setState(st => ({ chan: st.chan >= 3 ? 1 : st.chan + 1, locked: false }));

  startTune = (e) => {
    const box = e.currentTarget.getBoundingClientRect();
    const cx = box.left + box.width / 2, cy = box.top + box.height / 2;
    const base = Math.atan2(e.clientY - cy, e.clientX - cx);
    const f0 = this.state.freq;
    const move = (ev) => {
      let d = Math.atan2(ev.clientY - cy, ev.clientX - cx) - base;
      while (d > Math.PI) d -= Math.PI * 2;
      while (d < -Math.PI) d += Math.PI * 2;
      const f = Math.max(1, Math.min(100, f0 + (d / Math.PI) * 26));
      this.setState({ freq: Math.round(f * 10) / 10, locked: false });
    };
    const up = () => { window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up); };
    window.addEventListener('mousemove', move);
    window.addEventListener('mouseup', up);
    e.preventDefault();
  };

  tuneOff() {
    if (this.state.chan !== SIGNAL.chan) return 99;
    return Math.abs(this.state.freq - SIGNAL.freq);
  }

  lockSignal = () => this.setState(st => (this.tuneOff() <= 0.3 ? { locked: true } : null));

  stepDial = (i) => (e) => {
    const shift = !!(e && e.shiftKey);
    this.setState(st => {
      if (!st.locked) return null;
      const d = st.dials.slice();
      d[i] = ((d[i] + (shift ? -1 : 1)) % 26 + 26) % 26;
      return { dials: d, activeDial: i };
    });
    clearTimeout(this._dialT);
    this._dialT = setTimeout(() => this.setState({ activeDial: null }), 1100);
  };


  toggleEng = () => this.setState(s => ({ commsOpen: false, engOpen: !s.engOpen, weaponsOpen: false, powerOpen: false, senOpen: false, hangarOpen: false }));





  doAlert = () => this.setState(st => ({ alert: (st.alert + 1) % ALERTS.length }));

  runOrder = (o) => () => {
    const st = this.state;
    if (st.auxCharge < o.cost) {
      this.setState({ orderFail: o.id });
      clearTimeout(this._failT);
      this._failT = setTimeout(() => this.setState({ orderFail: null }), 420);
      this.flickBack(o.id);
      return;
    }
    const block = this.orderBlock(o.id);
    if (block) {
      this.setState({ orderFail: o.id });
      clearTimeout(this._failT);
      this._failT = setTimeout(() => this.setState({ orderFail: null }), 420);
      this.flickBack(o.id);
      return;
    }
    this.setState(prev => ({ auxCharge: Math.max(0, prev.auxCharge - o.cost) }));
    this.applyOrder(o.id);
    this.flickBack(o.id);
  };

  flickBack(id) {
    this.setState(st => ({ dcThrown: Object.assign({}, st.dcThrown, { [id]: true }) }));
    clearTimeout(this._dcT && this._dcT[id]);
    this._dcT = this._dcT || {};
    this._dcT[id] = setTimeout(() => {
      this.setState(st => ({ dcThrown: Object.assign({}, st.dcThrown, { [id]: false }) }));
    }, 400);
  }

  orderBlock(id) {
    const c = this.state.comp[this.state.engSel];
    if (id === 'party') {
      if (c.integ >= 5) return 'COMPARTMENT AT FULL INTEGRITY';
      if (c.atmo === 'VENTED') return 'CANNOT WORK A VENTED COMPARTMENT';
      if (c.crew < 1) return 'NO CREW ABOARD';
    }
    if (id === 'seal' && c.atmo !== 'VENTED') return 'COMPARTMENT ALREADY SEALED';
    if (id === 'vent') {
      if (c.atmo === 'VENTED') return 'ALREADY VENTED';
      if (c.crew > 0) return c.crew + ' CREW ABOARD';
    }
    return null;
  }

  engBlock(id) {
    const s = this.state, c = s.comp[s.engSel];
    if (id === 'party')  return s.party ? 'DC PARTY COMMITTED \u2014 ' + (PARTY_TICKS - s.party.t) + ' TO RUN' : (c.crew < 1 ? 'NO CREW ABOARD' : (c.integ >= 5 ? 'COMPARTMENT AT FULL INTEGRITY' : null));
    if (id === 'seal')   return c.atmo === 'SEALED' ? 'COMPARTMENT ALREADY SEALED' : (c.crew < 1 ? 'NO CREW ABOARD' : null);
    if (id === 'vent')   return c.atmo === 'VENTED' ? 'COMPARTMENT ALREADY VENTED' : null;
    if (id === 'reboot') return (c.cond || c.integ < 5) ? null : 'SYSTEMS NOMINAL \u2014 NOTHING TO RESTART';
    return 'NOT FITTED';
  }

  pickComp = (k) => () => this.setState({ engSel: k });

  engRun = (id) => () => {
    if (this.engBlock(id)) return;
    this.applyOrder(id);
    this.flickBack('eng_' + id);
  };

  applyOrder(id) {
    this.setState(st => {
      const comp = Object.assign({}, st.comp);
      const c = comp[st.engSel];
      // a repair is dispatched, not applied: the party is committed until it finishes
      if (id === 'party') return { party: { comp: st.engSel, t: 0 } };
      // sealing restores atmosphere; it does not put a fire out — air is what feeds it
      if (id === 'seal') comp[st.engSel] = Object.assign({}, c, { atmo: 'SEALED' });
      // venting takes the air, so a fire goes with it
      if (id === 'vent') comp[st.engSel] = Object.assign({}, c, { atmo: 'VENTED', cond: c.cond === 'FIRE' ? null : c.cond });
      if (id === 'reboot') comp[st.engSel] = Object.assign({}, c, { cond: null, integ: Math.min(5, c.integ + 1) });
      return { comp };
    });
  }


  toggleHangar = () => this.setState(s => ({ commsOpen: false, hangarOpen: !s.hangarOpen, engOpen: false, weaponsOpen: false, powerOpen: false, senOpen: false }));
  toggleDoors = () => this.setState(s => ({ doorsOpen: !s.doorsOpen }));
  setLbrg = (e) => {
    const n = parseInt(String(e.target.value).replace(/[^0-9]/g, ''), 10);
    const v = isNaN(n) ? 0 : ((n % 360) + 360) % 360;
    this.setState({ lbrg: v });
  };

  launchDev() { return Math.abs(((this.state.lbrg - this.state.hdg + 540) % 360) - 180); }

  stageCells() {
    const s = this.state;
    return LAUNCH_STAGES.map((g, i) => {
        const on = !!s.lsw[g.id];
        const p = s.lprog[g.id] || 0;
        const prevOk = i === 0 || (s.lprog[LAUNCH_STAGES[i - 1].id] || 0) >= 100;
        const blocked = this.stageBlock(g.id);
        const live = prevOk && !blocked && !s.launched[s.squad];
        const done = p >= 100;
        const ink = done ? '#8FC33A' : (on ? '#E0A020' : '#E04B3C');
        return {
          label: g.label,
          press: this.pressStage(g.id),
          cursor: live || on ? 'pointer' : 'not-allowed',
          fill: p + '%',
          fillInk: ink,
          lamp: on || done ? ink : '#22262A',
          glow: on || done ? '0 0 7px ' + ink : 'none',
          ink: done ? '#E8EDF0' : (live ? '#C9D2D8' : '#77828A'),
          padTop: on ? '12px' : '3px',
          slotTop: on ? '3px' : '21px',
          glossTop: on ? '13px' : '0px',
          ridgeA: on ? '6px' : '11px',
          pad: on
            ? 'linear-gradient(180deg,#B2281A,#7A1509 62%,#5A0E05)'
            : (live ? 'linear-gradient(180deg,#E85742,#B22B1B 58%,#7E1B0E)' : 'linear-gradient(180deg,#5A4340,#3A2724 58%,#2A1A18)'),
          padShadow: on
            ? 'inset 0 2px 5px rgba(0,0,0,.6), 0 0 12px rgba(224,75,60,.35)'
            : 'inset 0 -2px 4px rgba(0,0,0,.45), 0 2px 4px rgba(0,0,0,.8)'
        };
    });
  }

  stageBlock(id) {
    const s = this.state;
    if (id === 'crew' && !this.flightOpsReleased()) return 'FLIGHT OPS NOT RELEASED \u2014 HELD AT DC-1 ALERT CONDITION 3';
    if (id === 'fuel' && !this.hgrOk()) return 'HANGAR BAY DAMAGED \u2014 FUEL LINES DOWN';
    if (id === 'ord' && !this.magOk()) return 'MAGAZINE HIT \u2014 NO ORDNANCE';
    if (id === 'doors' && !s.doorsOpen) return 'BAY DOOR MASTER SHUT';
    if (id === 'cat' && this.busUnits('aux') < 1) return 'NO AUX UNITS \u2014 CATAPULT WILL NOT CHARGE';
    return null;
  }

  pressStage = (id) => () => {
    const s = this.state, order = LAUNCH_STAGES.map(x => x.id), i = order.indexOf(id);
    if (s.launched[s.squad]) return;
    if (s.lsw[id]) {
      const next = Object.assign({}, s.lsw), prog = Object.assign({}, s.lprog);
      order.slice(i).forEach(k => { next[k] = false; prog[k] = 0; });
      this.setState({ lsw: next, lprog: prog });
      return;
    }
    if (i > 0 && (s.lprog[order[i - 1]] || 0) < 100) return;
    if (this.stageBlock(id)) return;
    this.setState(st => ({ lsw: Object.assign({}, st.lsw, { [id]: true }) }));
  };
  cycleSquad = () => this.setState(s => {
    const i = SQUADS.findIndex(q => q.key === s.squad);
    const next = SQUADS[(i + 1) % SQUADS.length];
    return { squad: next.key, order: next.orders[0].id, lsw: Object.assign({}, CLEAR_LAUNCH), lprog: Object.assign({}, CLEAR_PROG) };
  });
  cycleOrder = () => this.setState(s => {
    const list = this.squadOrders();
    const i = list.findIndex(o => o.id === s.order);
    return { order: list[(i + 1) % list.length].id };
  });

  squadReady(q) { return q.craft.filter(c => c.state === 'READY').length; }

  squadOrders() { const q = SQUADS.find(x => x.key === this.state.squad); return (q && q.orders) || ORDERS_INTCP; }

  launchBlock() {
    const s = this.state, q = SQUADS.find(x => x.key === s.squad);
    if (!q) return 'NO SQUADRON SELECTED';
    if (s.launched[s.squad]) return null;
    if (!this.flightOpsReleased()) return 'FLIGHT OPS NOT RELEASED \u2014 HELD AT DC-1 ALERT CONDITION 3';
    if (!this.hgrOk()) return 'HANGAR BAY DAMAGED \u2014 CANNOT SPOT CRAFT';
    if (this.squadReady(q) === 0) return 'NO CRAFT READY';
    if (this.launchDev() > 60) return 'BEARING OUTSIDE LAUNCH CONE';
    return null;
  }

  launchSquad = () => {
    const s = this.state;
    if (s.launched[s.squad]) return;
    if (this.launchBlock()) return;
    if (!LAUNCH_STAGES.every(g => (s.lprog[g.id] || 0) >= 100)) return;
    this.setState(st => ({ launched: Object.assign({}, st.launched, { [st.squad]: st.order }) }));
  };
  recallSquad = () => this.setState(s => {
    const l = Object.assign({}, s.launched);
    delete l[s.squad];
    return { launched: l, lsw: Object.assign({}, CLEAR_LAUNCH), lprog: Object.assign({}, CLEAR_PROG) };
  });
  startGain = (e) => {
    const cap = this.gainCap();
    const box = e.currentTarget.getBoundingClientRect();
    const set = (clientY) => {
      const t = 1 - (clientY - (box.top + 19)) / (box.height - 38);
      const v = Math.max(1, Math.min(5, cap, Math.round(1 + t * 4)));
      this.setState({ senBoost: v });
    };
    set(e.clientY);
    const move = (ev) => set(ev.clientY);
    const up = () => { window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up); };
    window.addEventListener('mousemove', move);
    window.addEventListener('mouseup', up);
    e.preventDefault();
  };

  coneWiden = () => this.setState(s => ({ coneArc: Math.min(90, s.coneArc + 10) }));
  coneNarrow = () => this.setState(s => ({ coneArc: Math.max(15, s.coneArc - 10) }));

  busUnits(k) { return this.state.busFill[k] || 0; }

  // R-05 — a compartment's integrity governs the system housed in it.
  // A vented compartment reads as zero: nobody can crew it, whatever its structure.
  compHealth(k) {
    const c = this.state.comp[k];
    if (!c) return 0;
    return c.atmo === 'VENTED' ? 0 : c.integ;
  }

  // Reactor damage caps what the core can make. Floored at 2 units so AUX can always
  // charge and a damage-control party can always be sent — a hit must never be unrecoverable.
  reactorCap() { return Math.max(2, Math.round((this.compHealth('rct') / 5) * REACTOR_UNITS)); }

  magOk() { return this.compHealth('mag') >= 3; }   // shells and warheads are struck up from here
  hgrOk() { return this.compHealth('hgr') >= 3; }   // craft cannot be spotted in a wrecked bay
  briOk() { return this.compHealth('bri') >= 3; }   // the plot computer lives on the bridge
  engFactor() { return this.compHealth('eng') / 5; } // engine room damage scales usable thrust

  // R-06 — alert condition sets crew disposition and what the board will release.
  // CONDITION 3 cruising, 2 action stations manned, 1 battle stations closed up.
  weaponsReleased() { return this.state.alert >= 1; }
  flightOpsReleased() { return this.state.alert >= 1; }
  repairRate() { return this.state.alert === 2 ? 2 : 1; }

  gainCap() { return Math.max(1, this.busUnits('sen')); }
  thrCap() { return Math.round(this.busUnits('eng') * 20 * this.engFactor()); }
  wepReady() { return this.busUnits('wep') >= 2 && this.magOk() && this.weaponsReleased(); }

  gridCell() {
    // 60.9px is 1000 km at 1x; pick the step whose screen size lands nearest 61px
    const pxPerKm = (60.9 * this.state.zoom) / 1000;
    const STEPS = [5000, 2000, 1000, 500, 200, 100, 50, 20];
    let best = STEPS[0], bestErr = Infinity;
    STEPS.forEach(km => {
      const err = Math.abs(pxPerKm * km - 61);
      if (err < bestErr) { bestErr = err; best = km; }
    });
    return { km: best, px: pxPerKm * best };
  }

  senRangeKm() { return 12000 + this.state.senBoost * 7000; }

  passiveRangeKm() {
    return this.state.spectrum === 'UV' ? this.senRangeKm() * 0.55 : this.senRangeKm();
  }

  heard(t) {
    if (t.own || !this.state.dradis) return false;
    return t.sig.indexOf(this.state.spectrum) >= 0 && t.range <= this.passiveRangeKm();
  }

  setSpectrum = (k) => () => this.setState({ spectrum: k });

  scopeOn(t) {
    const b = this.state.scopeBrg == null ? 62 : this.state.scopeBrg;
    return this.state.scopeDown && angDiff(t.bearing, b) <= 4;
  }

  scopeOffDeg() {
    const t = TRACKS.find(x => x.key === this.state.target);
    if (!t) return 999;
    return angDiff(t.bearing, this.state.scopeBrg == null ? 62 : this.state.scopeBrg);
  }
  scopeOnAxis() { return this.scopeOffDeg() <= 4; }

  scopeSubject() { return TRACKS.find(t => this.heard(t) && this.scopeOn(t)) || null; }

  classify = () => this.setState(st => {
    const t = this.scopeSubject();
    if (!t) return null;
    return { cls: Object.assign({}, st.cls, { [t.key]: true }) };
  });

  lookAt = (k) => () => {
    const t = TRACKS.find(x => x.key === k);
    if (t) this.setState({ scopeDown: true, scopeBrg: t.bearing, slewDx: 0 });
  };

  trackers() {
    if (this.state.ownSig < 45) return 0;
    return TRACKS.filter(t => !t.own && t.hostile !== false).length;
  }

  coneCovers(t) {
    if (!this.state.active) return false;
    const d = Math.abs(((t.bearing - this.state.coneBrg + 540) % 360) - 180);
    return d <= this.state.coneArc && t.range <= this.senRangeKm();
  }

  startCone = (e) => {
    const el = e.currentTarget.getBoundingClientRect();
    this._cone = { cx: el.left + el.width / 2, cy: el.top + el.height / 2 };
    window.addEventListener('mousemove', this.moveCone);
    window.addEventListener('mouseup', this.endCone);
    this.moveCone(e);
    e.preventDefault();
  };
  moveCone = (e) => {
    if (!this._cone) return;
    const a = Math.atan2(e.clientX - this._cone.cx, this._cone.cy - e.clientY) * 180 / Math.PI;
    this.setState({ coneBrg: Math.round((a + 360) % 360) });
  };
  endCone = () => {
    this._cone = null;
    window.removeEventListener('mousemove', this.moveCone);
    window.removeEventListener('mouseup', this.endCone);
    this.resolveLit();
  };
  resolveLit = () => this.setState(s => {
    const idd = Object.assign({}, s.idd);
    TRACKS.forEach(t => { if (this.coneCovers(t)) idd[t.key] = true; });
    return { idd };
  });
  identify = (k) => () => this.setState(s => {
    const t = TRACKS.find(x => x.key === k);
    if (!t || !this.coneCovers(t) || !this.heard(t)) return null;
    return { idd: Object.assign({}, s.idd, { [k]: true }) };
  });
  toggleDradis = () => this.setState(st => ({ dradis: !st.dradis }));
  toggleActive = () => this.setState(st => ({ active: !st.active }));
  tickPower = () => {
    const st = this.state;
    const patch = {};

    // bus fill: drain first so a bus dropping out of DRAW frees its units the same tick
    const fill = Object.assign({}, st.busFill);
    let total = BUSES.reduce((a, b) => a + fill[b.key], 0);
    const coreCap = this.reactorCap();
    let moved = false;
    // only VENT sheds units; STANDBY holds what the bus already carries
    BUSES.forEach(b => {
      if (st.busMode[b.key] === 'VENT' && fill[b.key] > 0) { fill[b.key] -= 1; total -= 1; moved = true; }
    });
    BUSES.forEach(b => {
      if (st.busMode[b.key] === 'DRAW' && fill[b.key] < BUS_MAX && total < coreCap) { fill[b.key] += 1; total += 1; moved = true; }
    });
    if (total > coreCap) {
      for (let i = BUSES.length - 1; i >= 0 && total > coreCap; i--) {
        const k = BUSES[i].key;
        while (fill[k] > 0 && total > coreCap) { fill[k] -= 1; total -= 1; moved = true; }
      }
    }
    if (moved) patch.busFill = fill;

    // the damage-control party: time to work, then the repair lands
    if (st.party) {
      const t = st.party.t + 1;
      if (t >= PARTY_TICKS) {
        const comp = Object.assign({}, st.comp);
        const c = comp[st.party.comp];
        comp[st.party.comp] = Object.assign({}, c, { integ: Math.min(5, c.integ + this.repairRate()) });
        patch.comp = comp;
        patch.party = null;
      } else {
        patch.party = { comp: st.party.comp, t: t };
      }
    }


    // launch telemetry: a thrown stage runs out over several ticks, on this same timer
    const prog = Object.assign({}, st.lprog);
    let ran = false;
    LAUNCH_STAGES.forEach(g => {
      const want = st.lsw[g.id] ? Math.min(100, (prog[g.id] || 0) + g.rate) : 0;
      if (want !== (prog[g.id] || 0)) { prog[g.id] = want; ran = true; }
    });
    if (ran) patch.lprog = prog;

    // everything downstream reads the fill this tick will commit
    const senCap = Math.max(1, fill.sen || 0);
    const engCap = (fill.eng || 0) * 20;
    if (st.senBoost > senCap) patch.senBoost = senCap;
    if (st.thr > engCap) patch.thr = Math.round(engCap);

    const auxRate = (fill.aux || 0) * 3.2;
    if (auxRate > 0 && st.auxCharge < 100) patch.auxCharge = Math.min(100, st.auxCharge + auxRate);

    const wepFeed = (fill.wep || 0) * 5;
    const wepNext = wepFeed > 0 ? Math.min(100, st.wepCharge + wepFeed) : Math.max(0, st.wepCharge - 3);
    if (wepNext !== st.wepCharge) patch.wepCharge = wepNext;

    const sigWant = st.active ? 100 : 0;
    if (st.ownSig !== sigWant) {
      patch.ownSig = Math.max(0, Math.min(100, st.ownSig + (st.active ? 9 : -6)));
    }

    if (!st.paused) patch.missionT = st.missionT + st.rate;

    // the bow swings toward the ordered heading; a damaged engine room swings it slower
    if (st.hdg !== st.ordHdg) {
      const d = ((st.ordHdg - st.hdg + 540) % 360) - 180;
      const rate = Math.max(1, Math.round(4 * this.engFactor()));
      patch.hdg = Math.abs(d) <= rate ? st.ordHdg : ((st.hdg + Math.sign(d) * rate) % 360 + 360) % 360;
    }

    if (Object.keys(patch).length) this.setState(patch);
  };
  cycleBus = (k) => () => this.setState(s => {
    if (s.shed) return null;   // every bus is held at VENT by the shed rocker
    const mode = Object.assign({}, s.busMode);
    const dir = Object.assign({}, s.busDir || {});
    const i = BUS_MODES.indexOf(mode[k]);
    let d = dir[k] === -1 ? -1 : 1;
    if (i + d > BUS_MODES.length - 1 || i + d < 0) d = -d;
    mode[k] = BUS_MODES[i + d];
    dir[k] = d;
    return { busMode: mode, busDir: dir };
  });
  toggleShed = () => this.setState(s => {
    const mode = Object.assign({}, s.busMode);
    BUSES.forEach(b => { mode[b.key] = s.shed ? 'STANDBY' : 'VENT'; });
    return { shed: !s.shed, busMode: mode };
  });
  thrDown = (e) => {
    const box = e.currentTarget.getBoundingClientRect();
    this._thr = box;
    const cap = this.thrCap();
    const set = (cy) => this.setState({ thr: Math.max(0, Math.min(100, cap, Math.round((1 - (cy - box.top) / box.height) * 100))) });
    set(e.clientY);
    this._thrSet = set;
    e.preventDefault();
  };
  pickRate = (r) => () => this.setState({ rate: r, paused: false });
  toggleTint = () => this.setState(s => ({ gridTint: s.gridTint === 'WHITE' ? 'RED' : 'WHITE' }));

  flip = (id) => () => {
    const order = SWITCHES.map(w => w.id);
    const i = order.indexOf(id);
    // read inside the updater so a fast sequence of clicks sees committed state
    this.setState(s => {
      const cur = s.sw;
      if (cur[id]) {
        const sw = Object.assign({}, cur);
        for (let j = i; j < order.length; j++) sw[order[j]] = false;
        return { sw, fired: false, lever: 0 };
      }
      if (i > 0 && !cur[order[i - 1]]) return null;
        if (id === 'load' && !this.wepReady()) return null;
        if (id === 'load' && !s.sol[s.sys]) return null;
      if (id === 'load' && s.sys === 'PD' && !s.loaders) return null;
      if (id === 'bearing' && !this.onBearing()) return null;
      if (id === 'arm' && s.wepCharge < this.shotCost()) return null;
      return { sw: Object.assign({}, cur, { [id]: true }), fired: false };
    });
  };

  pickSystem = (id) => () => this.setState(st => {
    const sy = SYSTEMS.find(x => x.id === id);
    return { sys: id, mount: sy.mounts.indexOf(st.mount) >= 0 ? st.mount : sy.mounts[0], sw: Object.assign({}, CLEAR_SW), fired: false };
  });
  toggleScope = () => this.setState(s => ({ scopeDown: !s.scopeDown }));
  pickWarhead = (i) => () => this.setState({ warhead: i, sw: Object.assign({}, CLEAR_SW) });
  cycleSystem = () => {
    const i = SYSTEMS.findIndex(y => y.id === this.state.sys);
    this.pickSystem(SYSTEMS[(i + 1) % SYSTEMS.length].id)();
  };
  cycleMount = () => this.setState(st => {
    const sy = SYSTEMS.find(y => y.id === st.sys);
    if (!sy || sy.mounts.length < 2) return null;
    const i = sy.mounts.indexOf(st.mount);
    return { mount: sy.mounts[(i + 1) % sy.mounts.length], sw: Object.assign({}, CLEAR_SW), fired: false };
  });
  cycleWarhead = () => this.pickWarhead((this.state.warhead + 1) % WARHEADS.length)();

  // a knurled knob: horizontal drag, one step per pitch px
  stepKnob = (step, pitch) => (e) => {
    let x0 = e.clientX;
    const move = (ev) => {
      const n = Math.trunc((ev.clientX - x0) / pitch);
      if (n) { x0 += n * pitch; step(n); }
    };
    const up = () => { window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up); };
    window.addEventListener('mousemove', move);
    window.addEventListener('mouseup', up);
    e.preventDefault();
    e.stopPropagation();
  };

  toggleLoaders = () => this.setState(s => ({ loaders: !s.loaders, sw: Object.assign({}, CLEAR_SW) }));
  toggleQuad = (id) => () => this.setState(s => ({ quads: Object.assign({}, s.quads, { [id]: !s.quads[id] }) }));

  trackKnown() { return !!this.state.idd[this.state.target]; }

  trackIdFor() {
    const t = TRACKS.find(x => x.key === this.state.target);
    if (!t || !this.trackKnown()) return null;
    return 'TK-' + String(t.bearing).padStart(3, '0') + '-' + t.key.toUpperCase();
  }

  computeSol = () => this.setState(st => {
    const t = TRACKS.find(x => x.key === st.target);
    if (!t || !st.idd[t.key]) return null;
    if (!this.briOk()) return null;
    const rel = ((t.bearing - st.hdg + 540) % 360) - 180;
    return {
      sol: Object.assign({}, st.sol, {
        [st.sys]: { rng: t.range, brg: Math.round(rel), spd: t.spd }
      }),
      sw: Object.assign({}, CLEAR_SW),
      fired: false
    };
  });

  nudgeSol = (field, d) => () => this.setState(st => {
    const cur = st.sol[st.sys];
    if (!cur) return null;
    const next = Object.assign({}, cur);
    if (field === 'rng') next.rng = Math.max(0, next.rng + d * 100);
    if (field === 'brg') next.brg = Math.max(-180, Math.min(180, next.brg + d));
    if (field === 'spd') next.spd = Math.max(0, +(next.spd + d * 0.05).toFixed(2));
    return { sol: Object.assign({}, st.sol, { [st.sys]: next }), sw: Object.assign({}, CLEAR_SW) };
  });


  shotCost() {
    const sy = SYSTEMS.find(x => x.id === this.state.sys);
    return sy ? sy.shot : 50;
  }

  drawWep() {
    this.setState(st => ({ wepCharge: Math.max(0, st.wepCharge - this.shotCost()) }));
  }

  leverDown = (e) => { this._lever = { x: e.clientX, v: this.state.lever }; e.preventDefault(); };
  llDown = (e) => {
    if (this.launchBlock() || !LAUNCH_STAGES.every(g => (this.state.lprog[g.id] || 0) >= 100)) return;
    this._llever = { x: e.clientX, v: this.state.llever || 0 };
    e.preventDefault();
  };

  offBearing() {
    const s = this.state;
    const tgt = TRACKS.find(t => t.key === s.target);
    const brg = s.scopeBrg == null ? 62 : s.scopeBrg;
    if (!tgt) return null;
    return ((tgt.bearing - brg + 540) % 360) - 180;
  }

  onBearing() { const o = this.offBearing(); return o != null && Math.abs(o) <= 4; }

  startSlew = (e) => {
    this._drag = { x: e.clientX, brg: this.state.scopeBrg == null ? 62 : this.state.scopeBrg };
    this.setState({ slewing: true, slewDx: 0 });
    e.preventDefault();
  };

  moveSlew = (e) => {
    if (this._thrSet) { this._thrSet(e.clientY); return; }
    if (this._llever) {
      const v = Math.max(0, Math.min(1, this._llever.v + (e.clientX - this._llever.x) / 210));
      this.setState({ llever: v });
      if (v > 0.9 && !this._launched) { this._launched = true; this.launchSquad(); }
      return;
    }
    if (this._lever) {
      const v = Math.max(0, Math.min(1, this._lever.v + (e.clientX - this._lever.x) / 210));
      this.setState({ lever: v });
      if (v > 0.9 && this.allSet() && !this._shot) { this._shot = true; this.setState({ fired: true }); this.drawWep(); }
      return;
    }
    if (!this._drag) return;
    const dx = e.clientX - this._drag.x;
    if (Math.abs(dx) < 3) return;
    const next = ((this._drag.brg + dx * 0.34) % 360 + 360) % 360;
    this.setState(s => ({
      scopeBrg: next,
      slewDx: dx,
      sw: Object.assign({}, s.sw, { bearing: false, arm: false }),
      lever: 0,
      fired: false
    }));
  };

  endLever() { this._thrSet = null; this._lever = null; this._llever = null; this._shot = false; this._launched = false; this.setState({ lever: 0, llever: 0 }); }

  endSlew = () => {
    if (this._drag) this.setState({ slewing: false, slewDx: 0 });
    this._drag = null;
    if (this._lever || this._llever || this._thrSet) this.endLever();
  };

  magDown = (e) => {
    const x0 = e.clientX, z0 = this.state.scopeZoom;
    const move = (ev) => {
      const z = Math.max(1, Math.min(8, z0 + (ev.clientX - x0) / 40));
      this.setState({ scopeZoom: Math.round(z * 10) / 10 });
    };
    const up = () => { window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up); };
    window.addEventListener('mousemove', move);
    window.addEventListener('mouseup', up);
    e.preventDefault();
    e.stopPropagation();
  };

  slewTo = (e) => {
    if (e) { e.stopPropagation(); e.preventDefault(); }
    this._drag = null;
    const tgt = TRACKS.find(t => t.key === this.state.target);
    if (tgt) this.setState({ scopeBrg: tgt.bearing, slewing: false, slewDx: 0 });
  };


  allSet() {
    const s = this.state;
    if (!s.target || !s.sol[s.sys]) return false;
    if (s.sys === 'PD' && !s.loaders) return false;
    return SWITCHES.every(w => s.sw[w.id]);
  }

  setZoom = (z) => {
    const nz = Math.min(1, Math.max(0.1, Math.round(z * 100) / 100));
    this.setState({ zoom: nz });
  };

  scaleDown = (e) => {
    const x0 = e.clientX, z0 = this.state.zoom;
    const move = (ev) => this.setZoom(z0 * Math.pow(10, (ev.clientX - x0) / 160));
    const up = () => { window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up); };
    window.addEventListener('mousemove', move);
    window.addEventListener('mouseup', up);
    e.preventDefault();
    e.stopPropagation();
  };

  onWheel = (e) => {
    e.preventDefault();
    this.setZoom(this.state.zoom * (e.deltaY < 0 ? 1.12 : 1 / 1.12));
  };

  pickTool = (t) => () => this.setState({ tool: t, draft: null });

  // plot tools. Marks are stored in plot-layer px at 1x, own ship at 0,0, north up.
  plotPt(e) {
    const g = this._plot;
    return { x: Math.round((e.clientX - g.cx) / g.k), y: Math.round((e.clientY - g.cy) / g.k) };
  }
  plotDown = (e) => {
    const tool = this.state.tool;
    const el = e.currentTarget, r = el.getBoundingClientRect();
    this._plot = { cx: r.left + r.width / 2, cy: r.top + r.height / 2, k: (r.width / el.offsetWidth) * this.state.zoom };
    const p = this.plotPt(e);
    e.preventDefault();
    if (tool === 'POINT') {
      this.setState(st => ({ marks: st.marks.concat([{ id: st.markSeq + 1, kind: 'point', x: p.x, y: p.y, t: st.missionT }]), markSeq: st.markSeq + 1 }));
      return;
    }
    if (tool === 'RULER') this.setState({ draft: { kind: 'ruler', x: p.x, y: p.y, x2: p.x, y2: p.y } });
    else if (tool === 'COMPASS') this.setState({ draft: { kind: 'ring', x: 0, y: 0, x2: p.x, y2: p.y } });
    else return;
    window.addEventListener('mousemove', this.plotMove);
    window.addEventListener('mouseup', this.plotUp);
  };
  plotMove = (e) => {
    const p = this.plotPt(e);
    this.setState(st => st.draft ? { draft: Object.assign({}, st.draft, { x2: p.x, y2: p.y }) } : null);
  };
  plotUp = () => {
    window.removeEventListener('mousemove', this.plotMove);
    window.removeEventListener('mouseup', this.plotUp);
    this.setState(st => {
      const d = st.draft;
      if (!d) return null;
      const len = Math.hypot(d.x2 - d.x, d.y2 - d.y);
      if (len < 4) return { draft: null };
      return { draft: null, marks: st.marks.concat([Object.assign({ id: st.markSeq + 1 }, d)]), markSeq: st.markSeq + 1 };
    });
  };
  eraseMark = (id) => () => {
    if (this.state.tool !== 'ERASE') return;
    this.setState(st => ({ marks: st.marks.filter(m => m.id !== id) }));
  };

  pressErase = () => {
    if (this.state.tool === 'ERASE') {
      this.setState({ tool: 'CURSOR', marks: [], draft: null });
      return;
    }
    this.setState({ tool: 'ERASE' });
  };


  pickTarget = (k) => () => {
    if (!this.validSet().has(k)) return;
    this.setState({ target: k, fired: false, sw: Object.assign({}, CLEAR_SW), lever: 0 });
  };

  toggleWeapons = () => this.setState(s => ({ commsOpen: false, engOpen: false, weaponsOpen: !s.weaponsOpen, powerOpen: false, senOpen: false, hangarOpen: false }));

  validSet() {
    const m = MOUNTS.find(x => x.id === this.state.mount);
    const out = new Set();
    if (!m) return out;
    TRACKS.forEach(t => {
      if (!t.hostile) return;
      if (t.range > m.range) return;
      if (angDiff(t.bearing, m.arc) > m.halfArc) return;
      out.add(t.key);
    });
    return out;
  }

  onKey = (e) => {
    const k = (e.key || '').toLowerCase();
    const tag = (e.target && e.target.tagName) || '';
    if (tag === 'INPUT' || tag === 'TEXTAREA') return;

    if (k !== 'a' && k !== 'd') return;
    this.setState(st => ({ ordHdg: ((st.ordHdg + (k === 'd' ? 3 : -3)) % 360 + 360) % 360, apAtt: null }));
  };

  componentDidMount() {
    window.addEventListener('keydown', this.onKey);
    this._pwTimer = setInterval(this.tickPower, 420);
    const cv = this.steelRef.current;
    if (cv) {
      this._onRestore = () => this.initSteel();
      cv.addEventListener('webglcontextrestored', this._onRestore, false);
      cv.addEventListener('webglcontextlost', (e) => { e.preventDefault(); }, false);
    }
    this.initSteel();
    window.addEventListener('mousemove', this.moveSlew);
    window.addEventListener('mouseup', this.endSlew);
    const loop = () => { this.scopeT += 0.0009; this.drawScope(); this._raf = requestAnimationFrame(loop); };
    this._raf = requestAnimationFrame(loop);
  }

  drawScope() {
    const cv = this.scopeRef.current;
    if (!cv) return;
    const w = cv.clientWidth, h = cv.clientHeight;
    if (!w || !h) return;
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    if (cv.width !== Math.round(w * dpr)) { cv.width = Math.round(w * dpr); cv.height = Math.round(h * dpr); }
    const ctx = cv.getContext('2d');
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    ctx.fillStyle = '#0B0405';
    ctx.fillRect(0, 0, w, h);

    const A = (a) => 'rgba(224,75,60,' + a + ')';
    const t = this.scopeT, z = this.state.scopeZoom || 1.4;
    const cy = h * 0.5, vp = w / 2;
    const HW = (w / 2) * z, HH = (h * 0.27) * z;
    const FW = Math.max(10, HW * 0.13), FH = Math.max(6, HH * 0.13);

    // corridor half-height at a given distance out from the boresight
    const env = (dx) => {
      const k = Math.max(0, Math.min(1, (dx - FW) / (HW - FW)));
      return FH + (HH - FH) * k;
    };

    // a tight glow at the far aperture only — no horizon wash
    const gf = ctx.createRadialGradient(vp, cy, 0, vp, cy, HW * 0.28);
    gf.addColorStop(0, A(0.16));
    gf.addColorStop(1, A(0));
    ctx.fillStyle = gf;
    ctx.fillRect(0, 0, w, h);

    // vertical ribs shed from both edges, drifting slowly inward
    const N = 9;
    for (let i = 0; i < N; i++) {
      const u = ((i / N) + t) % 1;
      const near = 1 - u;
      const HOLD = FW * 2.6;
      const dx = HOLD + (HW - HOLD) * near;
      const e = env(dx);
      ctx.lineWidth = 0.7 + 1.1 * near;
      ctx.strokeStyle = A(0.14 + 0.58 * near);
      [vp - dx, vp + dx].forEach((x) => {
        ctx.beginPath(); ctx.moveTo(x, cy - e); ctx.lineTo(x, cy + e); ctx.stroke();
      });
    }

    // longitudinal rails, running from the near edge to the far aperture
    [1, 0.58, 0.24].forEach((k, idx) => {
      ctx.strokeStyle = A(idx === 0 ? 0.6 : 0.2);
      ctx.lineWidth = idx === 0 ? 1.4 : 1;
      [-1, 1].forEach((sy) => {
        [-1, 1].forEach((sx) => {
          ctx.beginPath();
          ctx.moveTo(vp + sx * HW, cy + sy * HH * k);
          ctx.lineTo(vp + sx * FW, cy + sy * FH * k);
          ctx.stroke();
        });
      });
    });

    // far aperture — the corridor terminates on a small bright frame
    ctx.strokeStyle = A(0.85);
    ctx.lineWidth = 1.6;
    ctx.beginPath(); ctx.rect(vp - FW, cy - FH, FW * 2, FH * 2); ctx.stroke();

    // horizon and elevation ladder
    ctx.strokeStyle = A(0.5); ctx.lineWidth = 1.4;
    ctx.beginPath(); ctx.moveTo(0, cy); ctx.lineTo(26, cy); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(w, cy); ctx.lineTo(w - 26, cy); ctx.stroke();
    for (let i = -3; i <= 3; i++) {
      if (!i) continue;
      const y = cy + i * (h * 0.11);
      if (y < 4 || y > h - 4) continue;
      ctx.strokeStyle = A(0.28);
      ctx.beginPath(); ctx.moveTo(w - 26, y); ctx.lineTo(w - 26 - (i % 2 === 0 ? 12 : 6), y); ctx.stroke();
    }
  }

  componentWillUnmount() {
    window.removeEventListener('keydown', this.onKey);
    clearInterval(this._pwTimer);
    cancelAnimationFrame(this._raf);
    const cv = this.steelRef.current;
    if (cv && this._onRestore) cv.removeEventListener('webglcontextrestored', this._onRestore);
  }

  initSteel() {
    const cv = this.steelRef.current;
    if (!cv) return;
    const opts = { preserveDrawingBuffer: true, alpha: false, antialias: false };
    const gl = cv.getContext('webgl', opts) || cv.getContext('experimental-webgl', opts);
    if (!gl) { cv.style.background = 'linear-gradient(158deg,#33383E,#15181B)'; return; }

    const vs = `attribute vec2 p; varying vec2 vUv;
      void main(){ vUv = p*0.5+0.5; gl_Position = vec4(p,0.0,1.0); }`;

    const fs = `precision highp float;
      varying vec2 vUv;
      float hash(vec2 p){ return fract(sin(dot(p, vec2(127.1,311.7)))*43758.5453123); }
      float vnoise(vec2 p){
        vec2 i = floor(p), f = fract(p);
        f = f*f*(3.0-2.0*f);
        return mix(mix(hash(i), hash(i+vec2(1.0,0.0)), f.x),
                   mix(hash(i+vec2(0.0,1.0)), hash(i+vec2(1.0,1.0)), f.x), f.y);
      }
      void main(){
        vec2 uv = vUv;
        vec2 q = vec2(uv.x, 1.0 - uv.y);

        // anisotropic brushed grain, stretched along X
        float g = 0.0;
        g += vnoise(vec2(q.x*1700.0, q.y*7.0))  * 0.46;
        g += vnoise(vec2(q.x*3600.0, q.y*13.0)) * 0.28;
        g += vnoise(vec2(q.x*420.0,  q.y*3.5))  * 0.18;
        g += vnoise(vec2(q.x*9000.0, q.y*26.0)) * 0.08;
        g -= 0.5;

        // broad machined sheen from upper-left
        float d = length((q - vec2(0.30,0.16)) * vec2(0.72,1.25));
        float broad = pow(clamp(1.0 - d, 0.0, 1.0), 1.7);

        // long anisotropic highlight band across the plate
        float band = exp(-pow((q.y - 0.20)*2.9, 2.0)) * 0.20
                   + exp(-pow((q.y - 0.86)*3.6, 2.0)) * 0.10;

        vec3 base = vec3(0.112, 0.128, 0.146);
        vec3 hi   = vec3(0.315, 0.345, 0.380);
        vec3 col  = mix(base, hi, clamp(broad*0.62 + band, 0.0, 1.0));

        col += g * 0.062;

        // sparse deep scratches following the grain
        float s = smoothstep(0.9955, 1.0, vnoise(vec2(q.x*70.0, q.y*1400.0)));
        col += s * 0.075;
        float s2 = smoothstep(0.9975, 1.0, vnoise(vec2(q.x*40.0, q.y*2600.0)));
        col -= s2 * 0.035;

        // plate vignette
        col *= 1.0 - 0.5 * length((uv - 0.5) * vec2(1.05, 0.95));

        gl_FragColor = vec4(col, 1.0);
      }`;

    const mk = (t, src) => {
      const sh = gl.createShader(t);
      gl.shaderSource(sh, src); gl.compileShader(sh);
      if (!gl.getShaderParameter(sh, gl.COMPILE_STATUS)) { console.warn(gl.getShaderInfoLog(sh)); return null; }
      return sh;
    };
    const v = mk(gl.VERTEX_SHADER, vs), f = mk(gl.FRAGMENT_SHADER, fs);
    if (!v || !f) return;

    const prog = gl.createProgram();
    gl.attachShader(prog, v); gl.attachShader(prog, f); gl.linkProgram(prog);
    if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) { console.warn(gl.getProgramInfoLog(prog)); return; }
    gl.useProgram(prog);

    const buf = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([-1,-1, 3,-1, -1,3]), gl.STATIC_DRAW);
    const loc = gl.getAttribLocation(prog, 'p');
    gl.enableVertexAttribArray(loc);
    gl.vertexAttribPointer(loc, 2, gl.FLOAT, false, 0, 0);

    gl.viewport(0, 0, cv.width, cv.height);
    gl.drawArrays(gl.TRIANGLES, 0, 3);
  }

  renderVals() {
    const sysOpen = { WEP: this.state.weaponsOpen, SEN: this.state.senOpen, PWR: this.state.powerOpen,
      HGR: this.state.hangarOpen, ENG: this.state.engOpen, COM: this.state.commsOpen };
    const s = this.state;
    const valid = this.validSet();
    const m = MOUNTS.find(x => x.id === s.mount) || MOUNTS[1];
    const tgt = TRACKS.find(t => t.key === s.target);

    const v = {
      steelRef: this.steelRef,
      onWheel: this.onWheel,
      crtOn: (this.props.crt ?? true) && (this.props.crtScan ?? 34) + (this.props.crtVignette ?? 46) > 0,
      crtScan: 'repeating-linear-gradient(0deg, rgba(0,0,0,' + (((this.props.crtScan ?? 34) / 100) * 0.9).toFixed(3) + ') 0 1px, rgba(0,0,0,0) 1px 3px)',
      crtVignette: 'radial-gradient(ellipse at 50% 48%, rgba(0,0,0,0) ' + (72 - (this.props.crtVignette ?? 46) * 0.35).toFixed(0) + '%, rgba(0,0,0,' + (((this.props.crtVignette ?? 46) / 100) * 0.95).toFixed(3) + ') 100%)',
      cellMinor: (() => { const c = this.gridCell(); return c.px.toFixed(2) + 'px ' + (c.px * 0.9557).toFixed(2) + 'px'; })(),
      cellMajor: (() => { const c = this.gridCell(); return (c.px * 5).toFixed(2) + 'px ' + (c.px * 5 * 0.9557).toFixed(2) + 'px'; })(),
      gridUnit: (() => { const km = this.gridCell().km; return km >= 1000 ? (km / 1000) + ',000 KM' : km + ' KM'; })(),
      pressErase: this.pressErase,
      eraseTitle: 'ERASE',
      zoom: s.zoom,
      plotDown: this.plotDown,
      plotCapture: (s.tool === 'POINT' || s.tool === 'RULER' || s.tool === 'COMPASS') ? 'auto' : 'none',
      marksView: (() => {
        const KM = 1000 / 60.9;
        const brg = (dx, dy) => String(Math.round(((Math.atan2(dx, -dy) * 180 / Math.PI) + 360) % 360) % 360).padStart(3, '0');
        const km = (px) => Math.round(px * KM).toLocaleString('en-US');
        const clk = (t) => { const p = (n) => String(n).padStart(2, '0'); return p(Math.floor(t / 3600) % 100) + ':' + p(Math.floor(t / 60) % 60) + ':' + p(t % 60); };
        const erasing = s.tool === 'ERASE';
        const all = s.marks.concat(s.draft ? [Object.assign({ id: 0, draft: true }, s.draft)] : []);
        return all.map(m => {
          const base = { isPoint: m.kind === 'point', isLine: m.kind === 'ruler', isRing: m.kind === 'ring', erase: this.eraseMark(m.id), pe: (erasing && !m.draft) ? 'auto' : 'none', cursor: erasing ? 'pointer' : 'default', op: m.draft ? '.75' : '1' };
          if (m.kind === 'point') return Object.assign(base, { left: 'calc(50% + ' + m.x + 'px)', top: 'calc(50% + ' + m.y + 'px)', label: clk(m.t) });
          const dx = m.x2 - m.x, dy = m.y2 - m.y, len = Math.hypot(dx, dy);
          const rot = (Math.atan2(dy, dx) * 180 / Math.PI).toFixed(2) + 'deg';
          const label = 'RNG ' + km(len) + ' km \u00B7 BRG ' + brg(dx, dy) + '\u00B0';
          if (m.kind === 'ruler') return Object.assign(base, { left: 'calc(50% + ' + m.x + 'px)', top: 'calc(50% + ' + m.y + 'px)', len: len.toFixed(1) + 'px', rot, label, labelLeft: 'calc(50% + ' + Math.round(m.x + dx / 2) + 'px)', labelTop: 'calc(50% + ' + Math.round(m.y + dy / 2) + 'px)' });
          return Object.assign(base, { d: (2 * len).toFixed(1) + 'px', r: (-len).toFixed(1) + 'px', len: len.toFixed(1) + 'px', rot, label, labelLeft: 'calc(50% + ' + m.x2 + 'px)', labelTop: 'calc(50% + ' + m.y2 + 'px)' });
        });
      })(),
      invZoom: 1 / s.zoom,
      markScale: Math.max(1, 0.45 / s.zoom).toFixed(3),
      zoomLabel: 'x' + (s.zoom >= 1 ? '1.0' : s.zoom.toFixed(2)),
      knobAngle: (-140 + (Math.log(s.zoom / 0.1) / Math.log(10)) * 280) + 'deg',
      scaleDown: this.scaleDown,
      tintAngle: s.gridTint === 'WHITE' ? '60deg' : '120deg',
      tintPositions: [['WHITE', 'WHT'], ['RED', 'R-O']].map(([k, short]) => {
        const on = s.gridTint === k;
        return {
          short,
          ink: on ? '#E8EDF0' : '#77828A',
          lamp: on ? '#E8EDF0' : '#1B2024',
          glow: on ? '0 0 6px rgba(232,237,240,.7)' : 'none'
        };
      }),

      toggleWeapons: this.toggleWeapons,

      mountArc: m.halfArc >= 180 ? 'ALL ROUND' : ('ARC ' + String(m.arc).padStart(3, '0') + '\u00B0 \u00B1' + m.halfArc + '\u00B0'),
      mountRange: m.range.toLocaleString() + ' km',

      targetName: tgt ? tgt.name : '\u2014',

      mountShort: m.short,
      drawerX: s.weaponsOpen ? '0px' : '376px',
      drawerFade: s.weaponsOpen ? 1 : 0,

      // scope geometry — vanishing point tracks the bearing offset from own heading
      // event channel — what changed while time was running
      unacked: !s.acked,
      ack: this.ack,
      changeCount: '3',
      missionClock: (() => {
        const t = s.missionT, p = (n) => String(n).padStart(2, '0');
        return p(Math.floor(t / 3600) % 100) + ':' + p(Math.floor(t / 60) % 60) + ':' + p(t % 60);
      })(),
      togglePower: this.togglePower,
      toggleHangar: this.toggleHangar,
      hangarX: s.hangarOpen ? '0px' : '376px',
      hangarFade: s.hangarOpen ? 1 : 0,
      hangarReady: (() => {
        const n = SQUADS.reduce((a, q) => a + this.squadReady(q), 0);
        const air = Object.keys(s.launched).length;
        return air ? n + ' READY / ' + air + ' AIRBORNE' : n + ' CRAFT READY';
      })(),
      toggleDoors: this.toggleDoors,
      doorAngle: s.doorsOpen ? '120deg' : '60deg',
      doorLamp: s.doorsOpen ? '#E04B3C' : '#8FC33A',
      doorGlow: s.doorsOpen ? '0 0 7px rgba(224,75,60,.75)' : '0 0 7px rgba(143,195,58,.75)',
      catInk: (s.lprog.cat || 0) >= 100 ? '#4A9FD8' : ((s.lprog.cat || 0) > 0 ? '#E0A020' : '#77828A'),
      catPct: Math.round(s.lprog.cat || 0) + '%',
      lbrgLabel: String(s.lbrg).padStart(3, '0'),
      setLbrg: this.setLbrg,
      devLabel: (() => { const d = this.launchDev(); return (d > 60 ? '+' : '') + d.toFixed(0) + '\u00B0'; })(),
      devInk: this.launchDev() > 60 ? '#E04B3C' : (this.launchDev() > 40 ? '#E0A020' : '#8FC33A'),
      squadName: (() => { const q = SQUADS.find(x => x.key === s.squad); return q ? q.name : '\u2014'; })(),
      stagesA: this.stageCells().slice(0, 3),
      stagesB: this.stageCells().slice(3),
      orderName: (() => { const o = this.squadOrders().find(x => x.id === s.order); return o ? o.label : '\u2014'; })(),
      doorLabel: s.doorsOpen ? 'OPEN' : 'SEALED',
      doorInk: s.doorsOpen ? '#4A9FD8' : '#77828A',
      pressLabel: s.doorsOpen ? 'VACUUM' : '1.0 ATM',
      pressInk: s.doorsOpen ? '#E0A020' : '#8FC33A',
      catLabel: (s.lprog.cat || 0) >= 100 ? 'CHARGED' : ((s.lprog.cat || 0) > 0 ? 'CHARGING' : 'HOLD'),
      catFill: Math.round(s.lprog.cat || 0) + '%',
      squads: SQUADS.map(q => {
        const sel = s.squad === q.key;
        const air = s.launched[q.key];
        return {
          name: q.name,
          bezel: sel ? 'linear-gradient(158deg,#6A737B,#2A3036)' : 'linear-gradient(158deg,#3E444A,#1C2024)',
          selLamp: sel ? '#E8EDF0' : '#1B2024',
          selGlow: sel ? '0 0 7px rgba(232,237,240,.7)' : 'none',
          nameInk: sel ? '#E8EDF0' : '#C9D2D8',
          state: air ? 'AIRBORNE \u2014 ' + air.toUpperCase() : 'IN BAY',
          stateInk: air ? '#4A9FD8' : '#C9D2D8',
          readyLabel: this.squadReady(q) + ' OF ' + q.craft.length + ' READY',
          roleLabel: q.role,
          craft: q.craft.map(c => ({
            tag: c.tag,
            tip: c.pilot + ' \u2014 ' + c.state + ' \u2014 FUEL ' + c.fuel + '%',
            fuel: c.fuel + '%',
            ink: air ? '#4A9FD8' : (c.state === 'DOWN' ? '#E04B3C' : (c.state === 'REARM' ? '#E0A020' : '#8FC33A'))
          }))
        };
      }),
      cycleOrder: this.cycleOrder,
      orderAngle: (60 + Math.max(0, this.squadOrders().findIndex(o => o.id === s.order)) * 30) + 'deg',
      cycleSquad: this.cycleSquad,
      squadAngle: (60 + SQUADS.findIndex(q => q.key === s.squad) * 60) + 'deg',
      interlock: (() => {
        if (s.launched[s.squad]) return 'SQUADRON AIRBORNE \u2014 RECALL TO RECOVER';
        const running = LAUNCH_STAGES.find(g => s.lsw[g.id] && (s.lprog[g.id] || 0) < 100);
        if (running) return running.label + ' \u2014 ' + Math.round(s.lprog[running.id] || 0) + '%';
        const next = LAUNCH_STAGES.find(g => (s.lprog[g.id] || 0) < 100);
        if (next) { const sb = this.stageBlock(next.id); return sb || (next.label + ' NOT SET'); }
        return this.launchBlock() || 'ALL SYSTEMS GO';
      })(),
      interlockInk: (() => {
        if (s.launched[s.squad]) return '#4A9FD8';
        if (this.launchBlock()) return '#E0A020';
        return LAUNCH_STAGES.every(g => (s.lprog[g.id] || 0) >= 100) ? '#8FC33A' : '#E0A020';
      })(),
      launchLamp: (() => {
        if (s.launched[s.squad]) return '#4A9FD8';
        return (this.launchBlock() || !LAUNCH_STAGES.every(g => (s.lprog[g.id] || 0) >= 100)) ? '#22262A' : '#8FC33A';
      })(),
      launchGlow: (() => {
        if (s.launched[s.squad]) return '0 0 7px rgba(74,159,216,.7)';
        return (this.launchBlock() || !LAUNCH_STAGES.every(g => (s.lprog[g.id] || 0) >= 100)) ? 'none' : '0 0 7px rgba(143,195,58,.7)';
      })(),
      launchCursor: (this.launchBlock() || !LAUNCH_STAGES.every(g => (s.lprog[g.id] || 0) >= 100)) ? 'not-allowed' : 'grab',
      llDown: this.llDown,
      llLeft: (2 + (s.llever || 0) * 72) + '%',
      llTrack: Math.round((s.llever || 0) * 100) + '%',
      llHint: s.launched[s.squad] ? 'AIRBORNE' : ((this.launchBlock() || !LAUNCH_STAGES.every(g => (s.lprog[g.id] || 0) >= 100)) ? 'SAFE' : 'READY'),
      llHintInk: s.launched[s.squad] ? '#4A9FD8' : ((this.launchBlock() || !LAUNCH_STAGES.every(g => (s.lprog[g.id] || 0) >= 100)) ? '#5B666F' : '#8FC33A'),

      recallLamp: s.launched[s.squad] ? '#E0A020' : '#22262A',
      recallGlow: s.launched[s.squad] ? '0 0 7px rgba(224,160,32,.7)' : 'none',
      recallSquad: this.recallSquad,
      toggleSensors: this.toggleSensors,
      senPanelX: s.senOpen ? '0px' : '376px',
      senPanelFade: s.senOpen ? 1 : 0,
      senD: (300 + s.senBoost * 96) + 'px',
      spectra: SPECTRA.map(sp => {
        const on = sp.key === s.spectrum;
        const n = TRACKS.filter(t => !t.own && t.sig.indexOf(sp.key) >= 0 && t.range <= (sp.key === 'UV' ? this.senRangeKm() * 0.55 : this.senRangeKm())).length;
        return {
          label: sp.label,
          count: String(n),
          pick: this.setSpectrum(sp.key),
          ink: on ? '#B6F04A' : '#8FC33A',
          bg: on ? 'rgba(143,195,58,.15)' : 'rgba(0,0,0,0)',
          edge: on ? 'inset 0 0 0 1px rgba(143,195,58,.6)' : 'inset 0 0 0 1px rgba(143,195,58,.16)',
          lamp: on ? '#B6F04A' : '#1B2810'
        };
      }),
      passives: TRACKS.filter(t => this.heard(t) && !s.idd[t.key]).map(t => {
        const on = this.scopeOn(t);
        return {
          len: Math.round((300 + s.senBoost * 96) / 2 * (s.spectrum === 'UV' ? 0.55 : 1)) + 'px',
          rot: (t.bearing - 90) + 'deg',
          counter: (90 - t.bearing) + 'deg',
          ink: on ? '#E0A020' : (s.gridTint === 'RED' ? '#E04B3C' : '#E8EDF0'),
          op: on ? 1 : 0.6,
          tag: 'BRG ' + String(t.bearing).padStart(3, '0'),
          look: this.lookAt(t.key)
        };
      }),
      scopeSubjectName: (() => { const t = this.scopeSubject(); return t ? (s.cls[t.key] ? t.name : 'BOGEY ' + t.key.toUpperCase()) : '\u2014'; })(),
      scopeSubjectClass: (() => { const t = this.scopeSubject(); if (!t) return 'NO SUBJECT ON BEARING'; return s.cls[t.key] ? (CLASSES[t.key] || 'UNKN') + ' HULL' : 'HULL UNCLASSIFIED'; })(),
      classify: this.classify,
      classifyInk: this.scopeSubject() ? '#E8EDF0' : '#5E6870',
      classifyCursor: this.scopeSubject() ? 'pointer' : 'not-allowed',
      classifyLamp: (() => { const t = this.scopeSubject(); if (!t) return '#22262A'; return s.cls[t.key] ? '#8FC33A' : '#E0A020'; })(),
      classifyGlow: (() => { const t = this.scopeSubject(); if (!t) return 'none'; return s.cls[t.key] ? '0 0 7px rgba(143,195,58,.8)' : '0 0 7px rgba(224,160,32,.8)'; })(),
      slewLamp: this.scopeOnAxis() ? '#8FC33A' : '#E0A020',
      slewLampGlow: this.scopeOnAxis() ? '0 0 7px rgba(143,195,58,.8)' : '0 0 7px rgba(224,160,32,.7)',
      senCapTop: (-(150 + s.senBoost * 48) - 8) + 'px',
      coneHalfLabel: s.coneArc,
      senEdge: s.gridTint === 'RED' ? 'rgba(224,75,60,.3)' : 'rgba(232,237,240,.26)',
      senGlow: s.gridTint === 'RED' ? 'rgba(224,75,60,.05)' : 'rgba(232,237,240,.04)',
      sweepDur: (5.4 - s.senBoost * 0.5).toFixed(1) + 's',
      sweepInk: s.gridTint === 'RED' ? 'rgba(255,140,120,.5)' : 'rgba(232,237,240,.42)',
      sweepTail: s.gridTint === 'RED' ? 'rgba(224,75,60,.11)' : 'rgba(232,237,240,.09)',
      sweepEdge: s.gridTint === 'RED' ? 'rgba(255,150,130,.75)' : 'rgba(232,237,240,.7)',
      coneFrom: (s.coneBrg - s.coneArc) + 'deg',
      coneArc: (s.coneArc * 2) + 'deg',
      coneArcLabel: s.coneArc * 2,
      coneWiden: this.coneWiden,
      coneNarrow: this.coneNarrow,
      emitLabel: s.active ? 'EMITTING' : 'SET OFF',
      emitInk: s.active ? '#E0A020' : '#77828A',
      coneInk: 'rgba(143,195,58,.2)',
      coneInkSoft: 'rgba(143,195,58,.05)',
      coneKnobFrom: (s.coneBrg - s.coneArc) + 'deg',
      coneNeedle: (s.coneBrg - 180) + 'deg',
      coneBrgLabel: String(s.coneBrg).padStart(3, '0'),
      senRangeLabel: this.senRangeKm().toLocaleString() + ' KM',
      boostStep: s.senBoost,
      startGain: this.startGain,
      gainCursor: 'grab',
      gainCapTop: (9 + (5 - s.senBoost) * 23.6) + 'px',
      gainTrack: (9 + (s.senBoost - 1) * 23.6 + 10) + 'px',
      boostCost: 'SEN BUS ALLOWS ' + this.gainCap() + ' OF 5',
      boostCostColor: this.gainCap() < 5 ? '#E0A020' : '#8FC33A',
      capPct: Math.round(s.wepCharge) + '%',
      capCells: (() => {
        const lit = Math.round((s.wepCharge / 100) * 10);
        const mk = Math.round((this.shotCost() / 100) * 10);
        const out = [];
        for (let k = 1; k <= 10; k++) out.push({ on: k <= lit, mark: k === mk });
        return out;
      })(),
      capLabel: this.shotCost() + '% PER SHOT',
      capInk: s.wepCharge >= this.shotCost() ? '#8FC33A' : (s.busFill.wep > 0 ? '#E0A020' : '#E04B3C'),
      capLamp: s.wepCharge >= this.shotCost() ? '#8FC33A' : (s.busFill.wep > 0 ? '#E0A020' : '#22262A'),
      capNote: s.busFill.wep < 1
        ? 'WEP BUS DEAD \u2014 CAPACITOR BLEEDING OFF'
        : (s.wepCharge >= this.shotCost()
            ? 'CHARGED \u2014 GATE WILL ARM'
            : 'CHARGING AT +' + ((s.busFill.wep || 0) * 5) + '/S \u2014 GATE LOCKED'),
      dradisOn: s.dradis,
      sigPct: Math.round(s.ownSig) + '%',
      sigCells: (() => {
        const lit = Math.round((s.ownSig / 100) * 10), out = [];
        for (let k = 1; k <= 10; k++) out.push({ on: k <= lit, ink: k >= 8 ? '#E04B3C' : (k >= 5 ? '#E0A020' : '#FFFFFF') });
        return out;
      })(),
      sigInk: s.ownSig >= 45 ? '#E04B3C' : (s.ownSig > 0 ? '#E0A020' : '#C9D2D8'),
      sigLamp: s.ownSig >= 45 ? '#E04B3C' : (s.ownSig > 0 ? '#E0A020' : '#22262A'),
      sigPulse: s.ownSig >= 45 ? 'bd-blink 1.1s steps(1) infinite' : 'none',
      sigNote: s.ownSig >= 45
        ? this.trackers() + ' HOSTILE ' + (this.trackers() === 1 ? 'SET HAS' : 'SETS HAVE') + ' YOUR RANGE'
        : (s.ownSig > 0 ? 'EMISSIONS DECAYING \u2014 NOT YET RESOLVED' : 'RUNNING SILENT'),
      tracked: s.ownSig >= 45,
      trackedCount: this.trackers(),
      dradisOff: !s.dradis,
      activeOn: s.active,
      setToggles: [
        { id:'dradis', label:'DRADIS', sub:'PASSIVE SWEEP', on:s.dradis, press:this.toggleDradis },
        { id:'active', label:'ACTIVE SCAN', sub:'EMITS \u2014 RAISES E-SIG', on:s.active, press:this.toggleActive }
      ].map(t => ({
        label: t.label, sub: t.sub, press: t.press,
        padTop: t.on ? '13px' : '3px',
        slotTop: t.on ? '3px' : '24px',
        glossTop: t.on ? '14px' : '0px',
        ridgeA: t.on ? '7px' : '13px',
        ridgeB: t.on ? '11px' : '17px',
        pad: t.on
          ? 'linear-gradient(180deg,#B2281A,#7A1509 62%,#5A0E05)'
          : 'linear-gradient(180deg,#E85742,#B22B1B 58%,#7E1B0E)',
        padShadow: t.on
          ? 'inset 0 2px 5px rgba(0,0,0,.6), 0 0 12px rgba(224,75,60,.35)'
          : 'inset 0 -2px 4px rgba(0,0,0,.45), 0 2px 4px rgba(0,0,0,.8)',
        lamp: t.on ? (t.id === 'active' ? '#E0A020' : '#8FC33A') : '#22262A',
        ink: t.on ? '#E8EDF0' : '#6E7B85'
      })),
      startCone: this.startCone,
      bogeyCount: (() => { const n = TRACKS.filter(t => this.heard(t)).length; return n + ' ON ' + s.spectrum; })(),
      bogeys: TRACKS.filter(t => !t.own && (this.heard(t) || s.idd[t.key] || s.cls[t.key])).map(t => {
        const pinged = !!s.idd[t.key], seen = !!s.cls[t.key], audible = this.heard(t);
        const lit = this.coneCovers(t) && audible;
        return {
          name: pinged ? t.name : 'BOGEY ' + t.key.toUpperCase(),
          brg: String(t.bearing).padStart(3, '0'),
          rng: pinged ? Math.round(t.range / 1000) + 'k' : '\u2014',
          klass: seen ? (CLASSES[t.key] || 'UNKN') : (lit ? 'RESOLVING' : '\u2014'),
          classColor: seen ? (t.hostile ? '#E04B3C' : '#8FC33A') : (lit ? '#E0A020' : '#77828A'),
          dot: pinged ? (t.hostile ? '#E04B3C' : '#8FC33A') : (audible ? '#E0A020' : '#5E6870'),
          row: lit ? 'rgba(143,195,58,.07)' : 'rgba(0,0,0,0)',
          identify: this.identify(t.key)
        };
      }),
      res_t07: !!s.idd.t07, res_c17: !!s.idd.c17, res_c22: !!s.idd.c22,

      commsX: s.commsOpen ? '0px' : '376px',
      commsFade: s.commsOpen ? 1 : 0,
      toggleComms: this.toggleComms,
      cycleChan: this.cycleChan,
      chanAngle: ((s.chan - 2) * 40) + 'deg',
      chanLamps: [1, 2, 3].map(n => ({
        n: String(n),
        lamp: s.chan === n ? '#B6F04A' : '#1B2024',
        glow: s.chan === n ? '0 0 7px rgba(182,240,74,.8)' : 'none',
        ink: s.chan === n ? '#B6F04A' : '#77828A'
      })),
      freqLabel: s.freq.toFixed(1),
      freqBearing: (((s.freq - 1) / 99) * 360).toFixed(2) + 'deg',
      tuneAngle: (-150 + ((s.freq - 1) / 99) * 300) + 'deg',
      startTune: this.startTune,
      lockSignal: this.lockSignal,
      spectrumBars: (() => {
        const out = [], off = this.tuneOff();
        for (let k = 0; k < 56; k++) {
          const f = 1 + (k / 55) * 99;
          let hgt = 8 + ((k * 137) % 23);
          if (s.chan === SIGNAL.chan) {
            const d = Math.abs(f - SIGNAL.freq);
            hgt += Math.max(0, 62 - d * 7);
          }
          const near = Math.abs(f - s.freq) < 2;
          out.push({
            h: Math.min(98, hgt).toFixed(0) + '%',
            len: (7 + (Math.min(98, hgt) / 98) * 27).toFixed(1) + 'px',
            rot: ((k / 56) * 360).toFixed(2) + 'deg',
            ink: near ? '#B6F04A' : (hgt > 46 ? 'rgba(143,195,58,.6)' : 'rgba(143,195,58,.26)')
          });
        }
        return out;
      })(),
      sigLamp: (() => { const o = this.tuneOff(); return o <= 0.3 ? '#8FC33A' : (o < 12 ? '#E0A020' : '#22262A'); })(),
      lockCap: s.locked
        ? 'linear-gradient(180deg,#B2281A,#7A1509 62%,#5A0E05)'
        : (this.tuneOff() <= 0.3 ? 'linear-gradient(180deg,#E85742,#B22B1B 58%,#7E1B0E)' : 'linear-gradient(180deg,#5A4340,#3A2724 58%,#2A1A18)'),
      lockCapShadow: s.locked
        ? 'inset 0 2px 5px rgba(0,0,0,.6), 0 0 12px rgba(224,75,60,.35)'
        : 'inset 0 -2px 4px rgba(0,0,0,.45), 0 2px 4px rgba(0,0,0,.8)',
      lockPadTop: s.locked ? '13px' : '3px',
      lockSlot: s.locked ? '3px' : '24px',
      lockGloss: s.locked ? '14px' : '0px',
      lockRidge: s.locked ? '7px' : '13px',
      lockRidgeB: s.locked ? '11px' : '17px',
      lockLamp: s.locked ? '#E04B3C' : (this.tuneOff() <= 0.3 ? '#E0A020' : '#22262A'),
      lockGlow: s.locked ? '0 0 7px rgba(224,75,60,.75)' : (this.tuneOff() <= 0.3 ? '0 0 7px rgba(224,160,32,.7)' : 'none'),
      tuneNote: s.locked
        ? 'CARRIER LOCKED \u2014 TRAFFIC IS ENCIPHERED'
        : (s.chan !== SIGNAL.chan ? 'NO CARRIER ON THIS BAND' : (this.tuneOff() <= 0.3 ? 'CARRIER FOUND' : 'SEEKING')),
      tuneNoteInk: s.locked ? '#8FC33A' : (this.tuneOff() <= 0.3 ? '#E0A020' : '#77828A'),
      commsState: s.locked ? (s.dials.every((d, i) => d === CIPHER_KEY[i]) ? 'MESSAGE READ' : 'DECIPHERING') : 'INCOMING SIGNAL',
      commsInk: s.locked ? (s.dials.every((d, i) => d === CIPHER_KEY[i]) ? '#8FC33A' : '#E0A020') : '#4A9FD8',
      plainChars: (() => {
        const txt = s.locked ? vigenere(CIPHERTEXT, s.dials, -1) : CIPHERTEXT;
        const base = s.locked ? '#E8EDF0' : 'rgba(232,237,240,.42)';
        let li = 0;
        return txt.split('').map((ch, n) => {
          const isLetter = AZ.indexOf(ch) >= 0;
          const slot = isLetter ? (li++ % CIPHER_KEY.length) : -1;
          const live = s.activeDial !== null && s.activeDial !== undefined && slot === s.activeDial;
          return { ch: ch === ' ' ? '\u00A0' : ch, ink: live ? '#E04B3C' : base };
        });
      })(),
      solvedLabel: s.locked ? (s.dials.every((d, i) => d === CIPHER_KEY[i]) ? 'PLAIN' : 'CIPHER') : 'NO CARRIER',
      solvedInk: s.locked && s.dials.every((d, i) => d === CIPHER_KEY[i]) ? '#8FC33A' : '#77828A',
      keyDials: s.dials.map((d, i) => ({
        letter: AZ[d],
        angle: (d * (360 / 26)) + 'deg',
        step: this.stepDial(i),
        cursor: s.locked ? 'pointer' : 'not-allowed',
        ink: (s.activeDial === i) ? '#E04B3C' : (s.locked ? '#B6F04A' : 'rgba(143,195,58,.42)'),
        glow: (s.activeDial === i) ? 'rgba(224,75,60,.55)' : (s.locked ? 'rgba(182,240,74,.45)' : 'rgba(0,0,0,0)'),
        bg: s.locked ? 'rgba(232,237,240,.05)' : 'rgba(0,0,0,.4)',
        edge: s.locked ? 'inset 0 0 0 1px rgba(255,255,255,.1)' : 'inset 0 0 0 1px rgba(255,255,255,.05)'
      })),
      engX: s.engOpen ? '0px' : '376px',
      engFade: s.engOpen ? 1 : 0,
      toggleEng: this.toggleEng,
      alertLabel: ALERTS[s.alert],
      alertAngle: (60 + s.alert * 30) + 'deg',
      alertPositions: ALERTS.map((label, i) => {
        const on = i === s.alert;
        return {
          label: label.replace('CONDITION ', 'C'),
          ink: on ? '#E8EDF0' : '#77828A',
          lamp: on ? '#E8EDF0' : '#1B2024',
          glow: on ? '0 0 6px rgba(232,237,240,.7)' : 'none'
        };
      }),
      alertInk: s.alert === 2 ? '#E04B3C' : (s.alert === 1 ? '#E0A020' : '#8FC33A'),
      compartments: COMPARTMENTS.map(c => {
        const st2 = s.comp[c.key], on = c.key === s.engSel;
        const vented = st2.atmo === 'VENTED';
        const cond = st2.cond || 'NOM';
        const seg = 72, gap = 7;
        let stops = [];
        for (let i = 0; i < 5; i++) {
          const lit = st2.integ >= (i + 1);
          const from = i * seg, to = from + seg - gap;
          stops.push((lit ? '#E8EDF0' : '#1A1F23') + ' ' + from + 'deg ' + to + 'deg');
          stops.push('rgba(0,0,0,0) ' + to + 'deg ' + (from + seg) + 'deg');
        }
        return {
          tag: c.tag, x: c.x, w: c.w, y: c.y, h: c.h,
          pick: this.pickComp(c.key),
          lamp: vented ? '#E04B3C' : (st2.integ >= 4 ? '#8FC33A' : (st2.integ >= 2 ? '#E0A020' : '#E04B3C')),
          tagInk: on ? '#E8EDF0' : '#8A959D',
          cellBg: on ? 'rgba(232,237,240,.16)' : 'rgba(232,237,240,.05)',
          cellEdge: on ? 'inset 0 0 0 1px #E8EDF0' : 'inset 0 0 0 1px rgba(232,237,240,.3)',
          rowBg: on ? 'rgba(232,237,240,.07)' : 'rgba(0,0,0,0)',
          ring: 'conic-gradient(from -90deg, ' + stops.join(', ') + ')',
          ringDim: on ? '1' : '.72',
          atmo: vented ? 'VENT' : 'SEAL',
          atmoInk: vented ? '#E04B3C' : '#8FC33A',
          cond: cond,
          condInk: cond === 'FIRE' ? '#E04B3C' : (cond === 'RAD' ? '#E0A020' : '#8FC33A'),
          crew: String(st2.crew),
          crewInk: st2.crew > 0 ? '#C9D2D8' : '#E04B3C'
        };
      }),




      compName: (COMPARTMENTS.find(c => c.key === s.engSel) || COMPARTMENTS[0]).name,
      doAlert: this.doAlert,
      cycleComp: this.cycleComp,
      compAngle: (40 + COMPARTMENTS.findIndex(c => c.key === s.engSel) * (100 / Math.max(1, COMPARTMENTS.length - 1))) + 'deg',
      compTicks: COMPARTMENTS.map((c, i) => ({ a: (40 + i * (100 / Math.max(1, COMPARTMENTS.length - 1))) + 'deg' })),
      compLamp: (() => { const h = this.compHealth(s.engSel); return h >= 4 ? '#8FC33A' : (h >= 2 ? '#E0A020' : '#E04B3C'); })(),
      compGlow: (() => { const h = this.compHealth(s.engSel); const k = h >= 4 ? '143,195,58' : (h >= 2 ? '224,160,32' : '224,75,60'); return '0 0 7px rgba(' + k + ',.75)'; })(),
      auxPct: Math.round(s.auxCharge) + '%',
      auxInk: s.auxCharge >= 30 ? '#8FC33A' : (s.busFill.aux > 0 ? '#E0A020' : '#E04B3C'),
      auxRate: s.busFill.aux > 0 ? '+' + (s.busFill.aux * 3.2).toFixed(1) + '/S' : 'NO FEED',
      auxCells: (() => {
        const lit = Math.round((s.auxCharge / 100) * 10);
        const out = [];
        for (let k = 1; k <= 10; k++) out.push({ on: k <= lit });
        return out;
      })(),
      orderSwitches: ENG_ORDERS.map(o => {
        const thrown = !!s.dcThrown[o.id];
        const failed = s.orderFail === o.id;
        const afford = s.auxCharge >= o.cost;
        const blocked = this.orderBlock(o.id);
        const open = !o.tbd && afford && !blocked;
        return {
          label: o.label,
          cost: o.tbd ? 'RESVD' : o.cost + '%',
          costInk: o.tbd ? '#C9D2D8' : '#E8EDF0',
          press: o.tbd ? null : this.runOrder(o),
          cursor: open ? 'pointer' : 'not-allowed',
          padTop: thrown ? '11px' : '2px',
          slotTop: thrown ? '2px' : '20px',
          glossTop: thrown ? '12px' : '0px',
          ridgeA: thrown ? '6px' : '11px',
          ridgeB: thrown ? '9px' : '14px',
          pad: thrown
            ? 'linear-gradient(180deg,#B2281A,#7A1509 62%,#5A0E05)'
            : (o.tbd ? 'linear-gradient(180deg,#4A4744,#302D2B 58%,#232120)'
              : (open ? 'linear-gradient(180deg,#E85742,#B22B1B 58%,#7E1B0E)' : 'linear-gradient(180deg,#5A4340,#3A2724 58%,#2A1A18)')),
          padShadow: thrown
            ? 'inset 0 2px 5px rgba(0,0,0,.6), 0 0 12px rgba(224,75,60,.35)'
            : 'inset 0 -2px 4px rgba(0,0,0,.45), 0 2px 4px rgba(0,0,0,.8)',
          lamp: failed ? '#E04B3C' : (thrown ? '#B6F04A' : (open ? '#5E6870' : '#22262A')),
          ink: failed ? '#E8938A' : '#C9D2D8',
          lampShadow: o.tbd
            ? 'inset 0 1px 2px rgba(0,0,0,.9), inset 0 0 0 1px rgba(255,255,255,.07)'
            : '0 0 6px ' + (failed ? '#E04B3C' : (thrown ? '#B6F04A' : (open ? '#5E6870' : '#22262A'))) + ', inset 0 -1px 1px rgba(0,0,0,.4)',
          blank: !!o.tbd,
          live: !o.tbd
        };
      }),
      degradedLine: (() => {
        const out = [];
        if (this.reactorCap() < REACTOR_UNITS) out.push('RCT \u25B8 CORE ' + this.reactorCap() + ' OF ' + REACTOR_UNITS);
        if (!this.hgrOk()) out.push('HGR \u25B8 FLIGHT OPS DOWN');
        if (!this.magOk()) out.push('MAG \u25B8 NO ORDNANCE');
        if (this.compHealth('eng') < 5) out.push('ENG \u25B8 THRUST ' + Math.round(this.engFactor() * 100) + '%');
        return out.length ? out.join('   ') : 'ALL SYSTEMS NOMINAL';
      })(),
      degradedInk: (() => {
        const any = this.reactorCap() < REACTOR_UNITS || !this.hgrOk() || !this.magOk() || this.compHealth('eng') < 5;
        return any ? '#E0A020' : '#8FC33A';
      })(),
      partyLine: (() => {
        const p = s.party;
        if (!p) return 'DC PARTY \u25B8 STANDBY';
        const c = COMPARTMENTS.find(x => x.key === p.comp);
        return 'DC PARTY \u25B8 ' + (c ? c.name : p.comp) + '   ' + p.t + ' OF ' + PARTY_TICKS;
      })(),
      partyInk: s.party ? '#4A9FD8' : '#5B666F',
      hazardLine: (() => {
        const n = COMPARTMENTS.filter(c => s.comp[c.key].cond || s.comp[c.key].atmo === 'VENTED').length;
        const crew = COMPARTMENTS.reduce((a, c) => a + s.comp[c.key].crew, 0);
        const w = n === 1 ? ' COMPARTMENT IN HAZARD' : ' COMPARTMENTS IN HAZARD';
        return (n ? n + w : 'NO HAZARD') + ' \u00B7 COMPLEMENT ' + crew;
      })(),
      hazardInk: (() => {
        const losing = COMPARTMENTS.some(c => s.comp[c.key].cond);
        const n = COMPARTMENTS.filter(c => s.comp[c.key].cond || s.comp[c.key].atmo === 'VENTED').length;
        return losing ? '#E04B3C' : (n ? '#E0A020' : '#8FC33A');
      })(),
      engNote: (() => {
        const blocked = ENG_REPAIRS.filter(o => !o.tbd).map(o => this.engBlock(o.id));
        const open = ENG_REPAIRS.filter((o, i) => !o.tbd && !blocked[i]);
        if (open.length) return open.map(o => o.label).join(' \u00B7 ') + ' AVAILABLE';
        const first = blocked.find(Boolean);
        return first || 'NO WORK AVAILABLE';
      })(),
      engNoteInk: (() => {
        const anyOpen = ENG_REPAIRS.some(o => !o.tbd && !this.engBlock(o.id));
        return anyOpen ? '#8FC33A' : '#E0A020';
      })(),
      engSwitches: ENG_REPAIRS.map(o => {
        const thrown = !!s.dcThrown['eng_' + o.id];
        const blocked = o.tbd ? 'NOT FITTED' : this.engBlock(o.id);
        const open = !o.tbd && !blocked;
        return {
          label: o.label,
          flex: o.flex,
          swW: (o.flex > 1 ? (o.flex * 30) : 22) + 'px',
          press: o.tbd ? null : this.engRun(o.id),
          cursor: open ? 'pointer' : 'not-allowed',
          labelInk: open ? '#C9D2D8' : '#77828A',
          blank: !!o.tbd,
          live: !o.tbd,
          lamp: thrown ? '#8FC33A' : (open ? '#5E6870' : '#22262A'),
          lampShadow: thrown ? '0 0 7px rgba(143,195,58,.75)' : 'inset 0 -1px 1px rgba(0,0,0,.4)',
          padTop: thrown ? '11px' : '2px',
          slotTop: thrown ? '2px' : '20px',
          glossTop: thrown ? '12px' : '0px',
          ridgeA: o.flex > 1 ? (thrown ? '5px' : '9px') : (thrown ? '6px' : '11px'),
          ridgeB: o.flex > 1 ? (thrown ? '9px' : '13px') : (thrown ? '9px' : '14px'),
          pad: thrown
            ? 'linear-gradient(180deg,#B2281A,#7A1509 62%,#5A0E05)'
            : (open ? 'linear-gradient(180deg,#E85742,#B22B1B 58%,#7E1B0E)' : 'linear-gradient(180deg,#5A4340,#3A2724 58%,#2A1A18)'),
          padShadow: thrown
            ? 'inset 0 2px 5px rgba(0,0,0,.6), 0 0 12px rgba(224,75,60,.35)'
            : 'inset 0 -2px 4px rgba(0,0,0,.45), 0 2px 4px rgba(0,0,0,.8)'
        };
      }),




      orderNote: (() => {
        if (s.busFill.aux < 1) return 'AUX BUS DEAD \u2014 NO ORDER WILL INITIATE';
        const f = s.orderFail;
        if (f) {
          const o = ENG_ORDERS.find(x => x.id === f);
          if (s.auxCharge < o.cost) return 'INSUFFICIENT AUX \u2014 ' + o.cost + '% REQUIRED';
          return (this.orderBlock(f) || 'ORDER REFUSED').toUpperCase();
        }
        return 'ALLOCATE AUX UNITS TO CHARGE FASTER';
      })(),
      orderNoteInk: (s.busFill.aux < 1 || s.orderFail) ? '#E0A020' : '#939EA6',

      alertRelease: this.weaponsReleased() ? 'WEAPONS AND FLIGHT OPS RELEASED' : 'WEAPONS AND FLIGHT OPS NOT RELEASED',
      alertReleaseInk: this.weaponsReleased() ? '#8FC33A' : '#E0A020',
      apAtt: AP_ATT.map(a => {
        const on = a.key === s.apAtt;
        return {
          label: a.label, pick: this.setAtt(a.key),
          arrowX: a.ax, arrowY: a.ay, arrowClip: a.clip, cell: a.cell,
          ink: on ? '#E8EDF0' : '#77828A',
          core: on ? '#E8EDF0' : '#000000',
          arrowInk: on ? '#8FC33A' : '#5E6870',
          arrowCore: on ? '#8FC33A' : '#000000',
          bg: 'rgba(0,0,0,0)',
          edge: on
            ? 'inset 0 0 0 1px #8FC33A, 0 0 9px rgba(143,195,58,.45)'
            : 'inset 0 0 0 1px rgba(232,237,240,.26)'
        };
      }),
      apAttNote: s.apAtt
        ? 'HOLDING ' + (AP_ATT.find(a => a.key === s.apAtt) || {}).note
        : 'NO ATTITUDE HOLD \u2014 FREE DRIFT',
      apModes: AP_MODES.map(m => {
        const on = m.key === s.apMode;
        return {
          label: m.label, g: m.g, pick: this.setApMode(m.key),
          ink: on ? '#E8EDF0' : '#6E7B85',
          lamp: on ? (m.key === 'EVASIVE' ? '#E0A020' : '#8FC33A') : '#22262A',
          bg: on ? 'rgba(232,237,240,.07)' : 'rgba(0,0,0,.4)',
          edge: on ? 'inset 0 0 0 1px rgba(232,237,240,.42)' : 'inset 0 0 0 1px rgba(255,255,255,.08)'
        };
      }),
      apRangeLabel: (AP_RANGES[s.apRange] / 1000) + 'k KM',
      apRangeUp: this.apRangeUp,
      apRangeDown: this.apRangeDown,
      apRangeUpInk: s.apRange >= AP_RANGES.length - 1 ? '#5E6870' : '#C9D2D8',
      apRangeDownInk: s.apRange <= 0 ? '#5E6870' : '#C9D2D8',
      apState: s.apAtt ? 'AUTO \u2014 ' + s.apMode : 'MANUAL',
      apLamp: s.apAtt ? '#8FC33A' : '#5E6870',
      gLoad: this.gLoadNum().toFixed(1) + ' G',
      gInk: (() => { const g = this.gLoadNum(); return g >= 3.4 ? '#E04B3C' : (g >= 2.2 ? '#E0A020' : '#E8EDF0'); })(),
      stabPct: Math.round(this.stabNum()) + '%',
      stabCells: (() => {
        const lit = Math.round((this.stabNum() / 100) * 9);
        const out = [];
        for (let k = 9; k >= 1; k--) out.push({ on: k <= lit });
        return out;
      })(),
      hazShow: this.stabNum() < 50 ? 1 : 0,
      powerX: s.powerOpen ? '0px' : '376px',
      powerFade: s.powerOpen ? 1 : 0,
      reactorLabel: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) + ' OF ' + this.reactorCap() + ' UNITS AVAILABLE',
      toggleShed: this.toggleShed,
      shedNote: s.shed ? 'ALL BUSES VENTING \u2014 SELECTORS HELD' : 'SHED READY',
      shedNoteInk: s.shed ? '#E04B3C' : '#C9D2D8',
      shedLamp: s.shed ? '#E04B3C' : '#22262A',
      shedGlow: s.shed ? '0 0 7px rgba(224,75,60,.75)' : 'none',
      shedPadTop: s.shed ? '13px' : '3px',
      shedSlotTop: s.shed ? '3px' : '24px',
      shedGlossTop: s.shed ? '14px' : '0px',
      shedRidgeA: s.shed ? '7px' : '13px',
      shedRidgeB: s.shed ? '11px' : '17px',
      shedPad: s.shed
        ? 'linear-gradient(180deg,#B2281A,#7A1509 62%,#5A0E05)'
        : 'linear-gradient(180deg,#E85742,#B22B1B 58%,#7E1B0E)',
      shedPadShadow: s.shed
        ? 'inset 0 2px 5px rgba(0,0,0,.6), 0 0 12px rgba(224,75,60,.35)'
        : 'inset 0 -2px 4px rgba(0,0,0,.45), 0 2px 4px rgba(0,0,0,.8)',
      coreHealthLabel: this.compHealth('rct') + ' / 5',
      coreHealthColor: this.compHealth('rct') >= 4 ? '#8FC33A' : (this.compHealth('rct') >= 2 ? '#E0A020' : '#E04B3C'),
      coreCells: [1,2,3,4,5].map(i => {
        const lit = this.compHealth('rct') >= i;
        const ink = this.compHealth('rct') >= 4 ? '#8FC33A' : (this.compHealth('rct') >= 2 ? '#E0A020' : '#E04B3C');
        return { fill: lit ? ink : '#2A2F34', glow: lit ? '0 0 6px ' + ink : 'none' };
      }),
      coreDerated: this.reactorCap() < REACTOR_UNITS,
      coreDerateNote: 'CORE DERATED \u2014 ' + (REACTOR_UNITS - this.reactorCap()) + ' UNITS LOST TO DAMAGE',
      r1: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 1,
      r2: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 2,
      r3: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 3,
      r4: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 4,
      r5: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 5,
      r6: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 6,
      r7: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 7,
      r8: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 8,
      r9: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 9,
      r10: (this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0)) >= 10,
      buses: BUSES.map(b => {
        const fill = s.busFill[b.key], mode = s.busMode[b.key];
        const total = BUSES.reduce((a, x) => a + s.busFill[x.key], 0);
        const climbing = mode === 'DRAW' && fill < BUS_MAX && total < this.reactorCap();
        return {
          short: b.short,
          s1: fill >= 1, s2: fill >= 2, s3: fill >= 3, s4: fill >= 4, s5: fill >= 5,
          p1: climbing && fill === 0, p2: climbing && fill === 1, p3: climbing && fill === 2,
          p4: climbing && fill === 3, p5: climbing && fill === 4,
          units: String(fill),
          mode: mode === 'STANDBY' ? 'STBY' : mode,
          modeColor: mode === 'VENT' ? '#E04B3C' : (mode === 'DRAW' ? '#8FC33A' : '#E0A020'),
          lamp: mode === 'VENT' ? '#E04B3C' : (mode === 'DRAW' ? '#8FC33A' : '#E0A020'),
          modeAngle: (mode === 'VENT' ? -45 : (mode === 'DRAW' ? 45 : 0)) + 'deg',
          cycle: this.cycleBus(b.key)
        };
      }),
      helmOpen: s.helmOpen,
      toggleHelm: this.toggleHelm,
      helmLatchX: s.helmOpen ? '16px' : '1px',
      ordersOpen: s.ordersOpen,
      toggleOrders: this.toggleOrders,
      ordersLatchX: s.ordersOpen ? '16px' : '1px',
      helmHdg: String(s.hdg).padStart(3, '0'),
      turnLabel: (() => {
        const d = ((s.ordHdg - s.hdg + 540) % 360) - 180;
        if (Math.abs(d) < 1) return 'STEADY';
        return (d < 0 ? 'PORT ' : 'STBD ') + String(Math.round(Math.abs(d))).padStart(3, '0') + '\u00B0';
      })(),
      turnInk: s.hdg === s.ordHdg ? '#8FC33A' : '#E0A020',
      hdgRing: (-s.hdg) + 'deg',
      helmThr: s.thr,
      thrTrack: (7 + (s.thr / 100) * 57 + 8) + 'px',
      thrCapBottom: (7 + (s.thr / 100) * 47) + 'px',

      thrDown: this.thrDown,
      togglePause: this.togglePause,
      pauseLabel: s.paused ? 'PAUSED' : 'RUNNING',
      pauseInk: s.paused ? '#E0A020' : '#8A959D',
      pauseLamp: s.paused ? '#E0A020' : '#5E6870',
      pauseBg: s.paused ? 'linear-gradient(158deg,#3A2A10,#211707)' : 'linear-gradient(158deg,#22262A,#12151800)',
      pauseBorder: s.paused ? 'rgba(224,160,32,.55)' : 'rgba(255,255,255,.1)',
      rates: [0.25, 1, 5, 10].map(r => {
        const on = s.rate === r && !s.paused;
        return {
          label: 'x' + r, pick: this.pickRate(r),
          ink: on ? '#E8EDF0' : '#8A959D',
          bg: on ? 'repeating-linear-gradient(158deg, rgba(255,255,255,.034) 0 1px, rgba(0,0,0,.03) 1px 2px), linear-gradient(158deg,#555D64,#272C31)' : 'repeating-linear-gradient(158deg, rgba(255,255,255,.026) 0 1px, rgba(0,0,0,.026) 1px 2px), linear-gradient(158deg,#383E44,#1A1E22)',
          edge: on ? 'inset 0 1px 0 rgba(255,255,255,.3), inset 0 0 0 1px rgba(232,237,240,.4)' : 'inset 0 1px 0 rgba(255,255,255,.16)'
        };
      }),
      gridInk: s.gridTint === 'RED' ? '#E04B3C' : '#E8EDF0',
      tintWhite: s.gridTint === 'WHITE',
      tintRed: s.gridTint === 'RED',
      toggleTint: this.toggleTint,

      loadLabel: BUSES.reduce((a, b) => a + s.busFill[b.key], 0) + ' / ' + this.reactorCap() + ' UNITS',
      loadColor: (() => { const left = this.reactorCap() - BUSES.reduce((a, b) => a + s.busFill[b.key], 0); return left <= 0 ? '#E04B3C' : (left <= 2 ? '#E0A020' : '#8FC33A'); })(),

      scopeRef: this.scopeRef,
      scopeZoomLabel: (s.scopeZoom || 1.4).toFixed(1) + '\u00D7',

      interlocks: SWITCHES.map((w, i) => {
        const on = !!s.sw[w.id];
        const prevOk = i === 0 || !!s.sw[SWITCHES[i - 1].id];
        const gated = (w.id === 'bearing' && !this.onBearing())
          || (w.id === 'arm' && s.wepCharge < this.shotCost())
          || (w.id === 'load' && (!this.wepReady() || !s.sol[s.sys] || (s.sys === 'PD' && !s.loaders)));
        const open = prevOk && !gated;
        return {
          label: w.label,
          hint: on ? 'SET' : (open ? w.sub
            : (w.id === 'load' && !this.weaponsReleased() ? 'NOT RELEASED'
            : (w.id === 'load' && !this.magOk() ? 'MAGAZINE HIT'
            : (w.id === 'load' && !this.wepReady() ? 'NO WEP BUS'
            : (w.id === 'arm' && prevOk && s.wepCharge < this.shotCost() ? 'CHARGING' : 'LOCKED'))))),
          color: on ? '#E0A020' : (open ? '#C9D2D8' : '#5B666F'),
          on: on, off: !on,
          cursor: open || on ? 'pointer' : 'not-allowed',
          padTop: on ? '13px' : '3px',
          slotTop: on ? '3px' : '24px',
          glossTop: on ? '14px' : '0px',
          ridgeA: on ? '7px' : '13px',
          ridgeB: on ? '11px' : '17px',
          pad: on
            ? 'linear-gradient(180deg,#B2281A,#7A1509 62%,#5A0E05)'
            : (open ? 'linear-gradient(180deg,#E85742,#B22B1B 58%,#7E1B0E)' : 'linear-gradient(180deg,#5A4340,#3A2724 58%,#2A1A18)'),
          padShadow: on
            ? 'inset 0 2px 5px rgba(0,0,0,.6), 0 0 12px rgba(224,75,60,.35)'
            : 'inset 0 -2px 4px rgba(0,0,0,.45), 0 2px 4px rgba(0,0,0,.8)',
          lamp: on ? '#FFC44D' : (open ? '#5E6870' : '#22262A'),
          flip: this.flip(w.id)
        };
      }),

      leverDown: this.leverDown,
      leverLeft: (2 + (s.lever || 0) * 72) + '%',
      leverTrack: Math.round((s.lever || 0) * 100) + '%',
      sysAngle: [60, 90, 120][Math.max(0, SYSTEMS.findIndex(y => y.id === s.sys))] + 'deg',
      sysPositions: SYSTEMS.map(y => {
        const on = s.sys === y.id;
        const armed = y.mounts.indexOf(s.mount) >= 0 && !!s.sw.arm;
        const lamp = armed ? '#E04B3C' : (on ? '#E8EDF0' : '#1B2024');
        return { short: y.label, ink: on ? '#E8EDF0' : '#77828A', lamp, glow: (armed || on) ? '0 0 6px ' + lamp : 'none' };
      }),
      cycleSystem: this.cycleSystem,
      mountLabel: (() => { const m = MOUNTS.find(x => x.id === s.mount); return m ? m.label : '\u2014'; })(),
      mountAngle: (() => {
        const sy = SYSTEMS.find(y => y.id === s.sys), n = sy ? sy.mounts.length : 1, i = sy ? sy.mounts.indexOf(s.mount) : 0;
        return (n === 1 ? 90 : [60, 120][i]) + 'deg';
      })(),
      mountTicks: (() => { const sy = SYSTEMS.find(y => y.id === s.sys); return (sy && sy.mounts.length === 1) ? [{ a: '-90deg' }] : [{ a: '-120deg' }, { a: '-60deg' }]; })(),
      mountPositions: (() => {
        const sy = SYSTEMS.find(y => y.id === s.sys);
        return (sy ? sy.mounts : []).map(id => {
          const m = MOUNTS.find(x => x.id === id), on = s.mount === id;
          return { short: m ? m.short : id, ink: on ? '#E8EDF0' : '#77828A', lamp: on ? '#E8EDF0' : '#1B2024', glow: on ? '0 0 6px rgba(232,237,240,.7)' : 'none' };
        });
      })(),
      cycleMount: this.cycleMount,
      isPdc: s.sys === 'PD',
      isMissile: s.sys === 'MS',
      sysState: (() => {
        const m = MOUNTS.find(x => x.id === s.mount);
        if (!m) return '\u2014';
        if (s.sys === 'RG') return 'RAIL ' + m.loaded + ' \u00B7 SLUGS ' + m.ammo;
        if (s.sys === 'PD') return m.ammo + ' RDS \u00B7 LOADERS ' + (s.loaders ? 'ENGAGED' : 'OFF');
        return WARHEADS[s.warhead] + ' \u00B7 SALVO ' + s.salvo + ' OF 8';
      })(),
      sysStateInk: (s.sys === 'PD' && !s.loaders) ? '#E0A020' : '#E8EDF0',
      warheadAngle: [60, 90, 120][s.warhead] + 'deg',
      warheadPositions: WARHEADS.map((w, i) => {
        const on = s.warhead === i;
        return { short: w, ink: on ? '#E8EDF0' : '#77828A', lamp: on ? '#E8EDF0' : '#1B2024', glow: on ? '0 0 6px rgba(232,237,240,.7)' : 'none' };
      }),
      cycleWarhead: this.cycleWarhead,
      salvo: String(s.salvo),
      salvoKnob: this.stepKnob((d) => this.setState(st => ({ salvo: Math.max(1, Math.min(8, st.salvo + d)) })), 14),
      salvoAngle: (-140 + (s.salvo - 1) / 7 * 280) + 'deg',
      quadrants: QUADRANTS.map(q => {
        const on = !!s.quads[q.id];
        return { label: q.label, toggle: this.toggleQuad(q.id), lamp: on ? '#8FC33A' : '#1B2024', glow: on ? '0 0 6px rgba(143,195,58,.8)' : 'none' };
      }),
      toggleLoaders: this.toggleLoaders,
      loaderLamp: s.loaders ? '#8FC33A' : '#E0A020',
      loaderGlow: s.loaders ? '0 0 6px rgba(143,195,58,.8)' : '0 0 6px rgba(224,160,32,.7)',
      computeSol: this.computeSol,
      solLamp: s.sol[s.sys] ? '#8FC33A' : (this.trackIdFor() ? '#E0A020' : '#1B2024'),
      solGlow: s.sol[s.sys] ? '0 0 6px rgba(143,195,58,.8)' : (this.trackIdFor() ? '0 0 6px rgba(224,160,32,.7)' : 'none'),
      trackId: this.trackIdFor() || 'NO TRACK',
      trackIdInk: this.trackIdFor() ? '#E8EDF0' : '#77828A',
      trackSrc: this.trackIdFor() ? 'FROM SENSOR LOCK' : 'NOT IDENTIFIED ON SENSORS',
      solInk: s.sol[s.sys] ? '#E8EDF0' : '#5B666F',
      solRange: s.sol[s.sys] ? s.sol[s.sys].rng.toLocaleString() : '\u2014',
      solBrg: s.sol[s.sys] ? ((s.sol[s.sys].brg > 0 ? '+' : '') + s.sol[s.sys].brg) : '\u2014',
      solSpd: s.sol[s.sys] ? s.sol[s.sys].spd.toFixed(2) : '\u2014',
      solCursor: s.sol[s.sys] ? 'ew-resize' : 'not-allowed',
      rngKnob: this.stepKnob((d) => this.nudgeSol('rng', d)(), 8),
      brgKnob: this.stepKnob((d) => this.nudgeSol('brg', d)(), 6),
      spdKnob: this.stepKnob((d) => this.nudgeSol('spd', d)(), 8),
      rngAngle: (s.sol[s.sys] ? (s.sol[s.sys].rng / 100 * 12) % 360 : 0) + 'deg',
      brgAngle: (s.sol[s.sys] ? s.sol[s.sys].brg * 6 : 0) + 'deg',
      spdAngle: (s.sol[s.sys] ? Math.round(s.sol[s.sys].spd / 0.05) * 12 % 360 : 0) + 'deg',

      toggleScope: this.toggleScope,
      scopeBoxH: s.scopeDown ? '656px' : '44px',

      stubLabel: s.scopeDown ? 'LOWERED' : 'HOUSED',
      stubInk: s.scopeDown ? '#E8EDF0' : '#6E7B85',
      stubLens: s.scopeDown ? '#C9D2D8' : '#5E6870',
      sightLamp: s.scopeDown ? '#8FC33A' : '#22262A',
      sightGlow: s.scopeDown ? 'rgba(143,195,58,.7)' : 'rgba(0,0,0,0)',
      brgEchoLabel: this.onBearing() ? 'ON BEARING' : ('OFF ' + (() => { const o = this.offBearing(); return o == null ? '\u2014' : (o > 0 ? '+' : '') + o.toFixed(1) + '\u00B0'; })()),
      brgEchoInk: this.onBearing() ? '#8FC33A' : '#E0A020',
      startSlew: this.startSlew,
      slewCursor: s.slewing ? 'grabbing' : 'ew-resize',
      slewRail: s.slewing ? 'inset 0 0 0 1px rgba(224,160,32,.65), inset 0 4px 26px rgba(0,0,0,.95), 0 0 20px rgba(224,160,32,.14)' : 'inset 0 0 0 1px rgba(255,255,255,.12), inset 0 4px 26px rgba(0,0,0,.95)',
      slewLabel: s.slewing ? ((s.slewDx || 0) >= 0 ? 'SLEW \u25B6' : '\u25C0 SLEW') : 'SLEW',
      slewInk: s.slewing ? '#E0A020' : '#8C979F',
      slewTo: this.slewTo,
      scopeBrgLabel: String(Math.round(s.scopeBrg == null ? 62 : s.scopeBrg)).padStart(3, '0'),
      tapeShift: (-((s.scopeBrg == null ? 62 : s.scopeBrg) * 2.4) % 48) + 'px',
      onBearing: this.onBearing(),
      offBearingLabel: (() => { const o = this.offBearing(); return o == null ? '\u2014' : (o > 0 ? '+' : '') + o.toFixed(1) + '\u00B0'; })(),
      offAxisInk: this.scopeOnAxis() ? '#8FC33A' : (this.scopeOffDeg() <= 12 ? '#E0A020' : '#E8EDF0'),
      magDown: this.magDown,
      magAngle: ((this.state.scopeZoom - 1) / 7 * 280 - 140).toFixed(1) + 'deg',
      tgtX: (() => { const o = this.offBearing(); return o == null ? 'calc(50% - 300px)' : ('calc(50% + ' + Math.max(-150, Math.min(150, o * 5.2)).toFixed(1) + 'px)'); })(),
      tgtVisible: (() => { const o = this.offBearing(); return o != null && Math.abs(o) < 27; })(),
      scopeConv: tgt ? Math.max(6, Math.min(96, Math.round(100 - (tgt.range / m.range) * 100))) + '%' : '0%',

      readyInk: s.fired ? '#E04B3C' : (this.allSet() ? '#E8EDF0' : '#C9D2D8'),
      readyLabel: (() => {
        if (s.fired) return 'ROUNDS AWAY';
        if (this.allSet()) return 'SOLUTION LOCKED \u2014 MOUNT READY';
        if (!tgt) return 'NO TARGET DESIGNATED';
        if (!this.trackIdFor()) return 'NO TRACKING ID \u2014 NOT IDENTIFIED';
        if (!this.briOk()) return 'BRIDGE DAMAGED \u2014 PLOT COMPUTER OFFLINE';
        if (!s.sol[s.sys]) return 'NO FIRING SOLUTION';
        if (!this.weaponsReleased()) return 'WEAPONS NOT RELEASED \u2014 CONDITION 2 REQUIRED';
        if (!this.magOk()) return 'MAGAZINE DAMAGED \u2014 NO ROUNDS STRUCK UP';
        if (s.sys === 'PD' && !s.loaders) return 'PDC AUTOLOADERS OFF';
        return 'ARMING SEQUENCE INCOMPLETE';
      })(),};


    TOOLS.forEach(t => {
      const on = s.tool === t;
      v['tool_' + t] = on;
      v['pick_' + t] = this.pickTool(t);
      v['toolLamp_' + t] = on ? '#B6F04A' : '#1A1F23';
      v['toolGlow_' + t] = on ? '0 0 8px #B6F04A' : 'none';
    });
    TRACKS.forEach(t => {
      v['valid_' + t.key] = valid.has(t.key);
      v['sel_' + t.key] = (s.target === t.key);
      v['pickTgt_' + t.key] = this.pickTarget(t.key);
    });
    Object.keys(sysOpen).forEach(k => {
      v['sysLamp_' + k] = sysOpen[k] ? '#B6F04A' : '#1A1F23';
      v['sysGlow_' + k] = sysOpen[k] ? '0 0 8px #B6F04A' : 'none';
    });
    return v;
  }
}
