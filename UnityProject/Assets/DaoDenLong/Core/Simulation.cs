using System;
using System.Collections.Generic;
using System.Globalization;

namespace DaoDenLong.Core
{
    /// <summary>
    /// The numbers the game actually runs on, published unchanged through game.contract.json.
    /// Phase 1 already uses the values Ticket-02 standardises, so later phases change no behaviour.
    /// </summary>
    public sealed class Tuning
    {
        public double PlayerSpeed = 6.0;          // m/s; start and stop inside one tick (limit: 0.1 s)
        public int CarryCapacity = 10;
        public int LanternCost = 10;              // motes needed to light a lantern, deposits add up
        public int LanternCountNight1 = 5;
        public double PickupRadius = 1.0;         // XZ; plus the 1.5 m height rule below
        public double InteractRadius = 2.0;
        public int PulseCost = 3;
        public double PulseRadius = 5.0;
        public int Night1DurationS = 300;
        public int Flames = 3;
        public double InvulnS = 1.5;              // 90 ticks
        public double HitDropRatio = 0.5;         // floor(carried / 2) motes drop on a hit
        public double PulseStunS = 2.0;           // 120 ticks
        public double PulsePushM = 4.0;           // over the first 15 ticks
        public double PulseCooldownS = 3.0;       // 180 ticks
        public double SafeRadius = 6.0;           // lit lanterns keep the shades out
        public double ShadeSpeed = 4.0;
        public double ShadeHitRadius = 1.0;       // XZ; plus the 1.5 m height rule
        public bool FallEnabled = true;           // leaving the island into the cloud sea is punished

        // Numbers Phase 1 is free to choose. They match what Ticket-02 later pins down.
        public double JumpVelocity = 7.0;
        public double Gravity = -20.0;
        public double DetectRange = 12.0;         // wander -> chase
        public double DetachRange = 18.0;         // chase -> wander
        public int ShadeMax = 6;
        public int ShadeSpawnStartTick = 300;     // two shades here, then one every ShadeSpawnEvery
        public int ShadeSpawnEvery = 480;
        public int MoteSpawnStartTick = 240;      // one mote at 240, 480, ... below the cap
        public int MoteSpawnEvery = 240;          // PRD line: "tại các tick 240, 480, …"
        public int MoteCap = 30;
        public int StartMotes = 20;
        public int DroppedMoteTtlTicks = 480;
        public double PickupHeight = 1.5;
        public double HitHeight = 1.5;
        public int InvulnTicks = 90;
        public int PulseStunTicks = 120;
        public int PulsePushTicks = 15;
        public int PulseCooldownTicks = 180;
        public int SafeZonePushTicks = 30;
        public int RecoilTicks = 60;
        public double RecoilPushM = 2.0;
        public int FallWaitTicks = 60;
        public double FallY = -10.0;
        public int ShadeSpawnMinPlayerDist = 15;
        public int ScorePerMote = 10;             // per mote deposited into a lantern
        public int ScorePerLantern = 100;
        public int ScorePerStun = 5;
        public int ScorePerSecondLeft = 2;        // win bonus
        public int ScorePerFlameLeft = 50;        // win bonus

        public double IslandRadius = 40.0;        // Night 1 - Đảo Bến, cloud sea beyond the rim

        // Numbers Ticket-02 pins down for Phase 2. The five published in the contract tuning block first.
        public double DashSpeed = 14.0;           // m/s
        public double DashDurationS = 0.2;        // 12 ticks
        public double DashCooldownS = 1.5;        // 90 ticks, counted from the dash start
        public int Night2DurationS = 330;
        public int Night3DurationS = 360;

        // The rest Ticket-02 standardises but does not publish as contract keys.
        public int DashDurationTicks = 12;
        public int DashCooldownTicks = 90;
        public double CharmRunSpeed = 7.2;        // Bùa Gió: run speed while it lasts
        public int CharmDurationTicks = 600;      // refreshed by a re-pickup
        public int CharmRespawnTicks = 2700;      // the charm reappears at its own spot
        public double WindPushSpeed = 2.0;        // Night 3: a gust pushes Mây for 5 s every 30 s
        public int WindGustEveryTicks = 1800;
        public int WindGustTicks = 300;
        public double DrifterFlySpeed = 7.0;      // Gió Lạc chase and flee speed
        public double DrifterIdleSpeed = 2.0;     // idle: a slow glide
        public int DrifterMax = 2;                // at most two on the island
        public int DrifterSpawnStartTick = 1200;  // one at 1200, 2400, ...
        public int DrifterSpawnEvery = 1200;
        public int DrifterChaseMinCarried = 5;    // idle -> chase: carrying >= 5 and no more than 15 m away
        public double DrifterChaseRange = 15.0;
        public double DrifterStealRadius = 1.2;   // the XZ contact distance
        public int DrifterStealMax = 3;           // steals min(3, carried); once per drifter, ever
        public int DrifterFleeTicks = 240;        // then it vanishes with what it carries
        public int DrifterDropVanishTicks = 120;  // a freed drifter vanishes 120 ticks after the drop
        public double ShadeSpeedNight3 = 4.5;     // the standard table: the Night 3 number only
        public int ShadeEveryNight2 = 420;        // one every 7 s after tick 300, at most 8
        public int ShadeMaxNight2 = 8;
        public int ShadeEveryNight3 = 360;        // one every 6 s after tick 300, at most 10
        public int ShadeMaxNight3 = 10;

        public static Tuning Standard() { return new Tuning(); }

        /// <summary>The "tuning" object of game.contract.json, keys in the order the PRD lists them.</summary>
        public JsonValue ToContractJson()
        {
            JsonValue o = JsonValue.Obj();
            o.Set("player_speed", JsonValue.Real(PlayerSpeed, 1));
            o.Set("carry_capacity", JsonValue.Int(CarryCapacity));
            o.Set("lantern_cost", JsonValue.Int(LanternCost));
            o.Set("lantern_count_night1", JsonValue.Int(LanternCountNight1));
            o.Set("pickup_radius", JsonValue.Real(PickupRadius, 1));
            o.Set("interact_radius", JsonValue.Real(InteractRadius, 1));
            o.Set("pulse_cost", JsonValue.Int(PulseCost));
            o.Set("pulse_radius", JsonValue.Real(PulseRadius, 0));
            o.Set("night1_duration_s", JsonValue.Int(Night1DurationS));
            o.Set("flames", JsonValue.Int(Flames));
            o.Set("invuln_s", JsonValue.Real(InvulnS, 1));
            o.Set("hit_drop_ratio", JsonValue.Real(HitDropRatio, 1));
            o.Set("pulse_stun_s", JsonValue.Real(PulseStunS, 1));
            o.Set("pulse_push_m", JsonValue.Real(PulsePushM, 0));
            o.Set("pulse_cooldown_s", JsonValue.Real(PulseCooldownS, 0));
            o.Set("safe_radius", JsonValue.Real(SafeRadius, 0));
            o.Set("shade_speed", JsonValue.Real(ShadeSpeed, 1));
            o.Set("shade_hit_radius", JsonValue.Real(ShadeHitRadius, 1));
            o.Set("fall_enabled", JsonValue.Bool(FallEnabled));
            // v1.1 adds exactly these five keys, in this order, as Ticket-02 spells them.
            o.Set("dash_speed", JsonValue.Real(DashSpeed, 1));
            o.Set("dash_duration_s", JsonValue.Real(DashDurationS, 1));
            o.Set("dash_cooldown_s", JsonValue.Real(DashCooldownS, 1));
            o.Set("night2_duration_s", JsonValue.Int(Night2DurationS));
            o.Set("night3_duration_s", JsonValue.Int(Night3DurationS));
            JsonValue s = JsonValue.Obj();
            s.Set("per_mote", JsonValue.Int(ScorePerMote));
            s.Set("per_lantern", JsonValue.Int(ScorePerLantern));
            s.Set("per_stun", JsonValue.Int(ScorePerStun));
            s.Set("per_second_left", JsonValue.Int(ScorePerSecondLeft));
            s.Set("per_flame_left", JsonValue.Int(ScorePerFlameLeft));
            o.Set("score", s);
            return o;
        }
    }

    /// <summary>One event line of the JSONL output.</summary>
    public sealed class SimEvent
    {
        public long Tick;
        public string Name;
        public List<KeyValuePair<string, JsonValue>> Data = new List<KeyValuePair<string, JsonValue>>();

        public SimEvent(long tick, string name) { Tick = tick; Name = name; }

        public SimEvent Put(string key, JsonValue value)
        {
            for (int i = 0; i < Data.Count; i++)
                if (Data[i].Key == key) { Data[i] = new KeyValuePair<string, JsonValue>(key, value); return this; }
            Data.Add(new KeyValuePair<string, JsonValue>(key, value));
            return this;
        }

        public JsonValue ToJson()
        {
            JsonValue o = JsonValue.Obj();
            o.Set("type", JsonValue.Str("event"));
            o.Set("tick", JsonValue.Int(Tick));
            o.Set("name", JsonValue.Str(Name));
            JsonValue d = JsonValue.Obj();
            foreach (KeyValuePair<string, JsonValue> kv in Data) d.Set(kv.Key, kv.Value);
            o.Set("data", d);
            return o;
        }
    }

    /// <summary>Snapshot line, the Test Contract v1.1 shape: v1.0 plus night, wind, the player fields and the three arrays.</summary>
    public sealed class Snapshot
    {
        public long Tick, SimTick, Score;
        public string State = "playing";
        public double TimeLeftS;
        public V3 PlayerPos;
        public int Flames, Carried;
        public List<Lantern> Lanterns = new List<Lantern>();
        public List<Shade> Shades = new List<Shade>();
        public List<Mote> Motes = new List<Mote>();

        // v1.1 additions.
        public int Night;
        public bool WindActive;
        public double WindDirDeg;
        public Player PlayerRef;
        public List<Drifter> Drifters = new List<Drifter>();
        public List<Charm> Charms = new List<Charm>();

        public JsonValue ToJson()
        {
            JsonValue o = JsonValue.Obj();
            o.Set("type", JsonValue.Str("snapshot"));
            o.Set("tick", JsonValue.Int(Tick));
            o.Set("sim_tick", JsonValue.Int(SimTick));
            o.Set("state", JsonValue.Str(State));
            o.Set("time_left_s", JsonValue.Real(Math.Round(TimeLeftS, 3, MidpointRounding.AwayFromZero), 3));
            o.Set("score", JsonValue.Int(Score));
            o.Set("night", JsonValue.Int(Night));

            JsonValue w = JsonValue.Obj();
            w.Set("active", JsonValue.Bool(WindActive));
            w.Set("dir_deg", JsonValue.Real(Math.Round(WindDirDeg, 1, MidpointRounding.AwayFromZero), 1));
            o.Set("wind", w);

            JsonValue p = JsonValue.Obj();
            p.Set("pos", SnapshotData.Pos(PlayerPos, 3));
            p.Set("flames", JsonValue.Int(Flames));
            p.Set("carried", JsonValue.Int(Carried));
            if (PlayerRef != null)
            {
                p.Set("vel", SnapshotData.Pos(PlayerRef.Vel, 3));
                p.Set("grounded", JsonValue.Bool(PlayerRef.Grounded));
                p.Set("invuln", JsonValue.Int(PlayerRef.InvulnTicks));
                p.Set("dash_cd_s", JsonValue.Real(Math.Round(PlayerRef.DashCdTicks / (double)Simulation.TicksPerSecond, 3, MidpointRounding.AwayFromZero), 3));
                p.Set("pulse_cd_s", JsonValue.Real(Math.Round(PlayerRef.PulseCdTicks / (double)Simulation.TicksPerSecond, 3, MidpointRounding.AwayFromZero), 3));
                p.Set("charm_s", JsonValue.Real(Math.Round(PlayerRef.CharmTicksLeft / (double)Simulation.TicksPerSecond, 3, MidpointRounding.AwayFromZero), 3));
            }
            o.Set("player", p);

            JsonValue ls = JsonValue.Arr();
            foreach (Lantern l in Lanterns)
            {
                JsonValue lj = JsonValue.Obj();
                lj.Set("id", JsonValue.Str(l.Id));
                lj.Set("pos", SnapshotData.Pos(l.Pos, 3));
                lj.Set("lit", JsonValue.Bool(l.Lit));
                lj.Set("deposited", JsonValue.Int(l.Deposited));
                ls.Add(lj);
            }
            o.Set("lanterns", ls);

            JsonValue ss = JsonValue.Arr();
            foreach (Shade s in Shades)
            {
                JsonValue sj = JsonValue.Obj();
                sj.Set("id", JsonValue.Str(s.Id));
                sj.Set("pos", SnapshotData.Pos(s.Pos, 3));
                sj.Set("state", JsonValue.Str(s.State.ToString()));
                ss.Add(sj);
            }
            o.Set("shades", ss);

            JsonValue ms = JsonValue.Arr();
            foreach (Mote m in Motes)
            {
                JsonValue mj = JsonValue.Obj();
                mj.Set("id", JsonValue.Str(m.Id));
                mj.Set("pos", SnapshotData.Pos(m.Pos, 3));
                mj.Set("kind", JsonValue.Str(m.Dropped ? "dropped" : "random"));
                mj.Set("ttl_s", m.Dropped
                    ? JsonValue.Real(Math.Round(m.TtlTicks / (double)Simulation.TicksPerSecond, 3, MidpointRounding.AwayFromZero), 3)
                    : JsonValue.Null());
                ms.Add(mj);
            }
            o.Set("motes", ms);
            o.Set("motes_active", JsonValue.Int(Motes.Count));

            JsonValue ds = JsonValue.Arr();
            foreach (Drifter d in Drifters)
            {
                JsonValue dj = JsonValue.Obj();
                dj.Set("id", JsonValue.Str(d.Id));
                dj.Set("pos", SnapshotData.Pos(d.Pos, 3));
                dj.Set("state", JsonValue.Str(d.State));
                dj.Set("stolen", JsonValue.Int(d.Stolen));
                ds.Add(dj);
            }
            o.Set("drifters", ds);

            JsonValue cs = JsonValue.Arr();
            foreach (Charm c in Charms)
            {
                JsonValue cj = JsonValue.Obj();
                cj.Set("id", JsonValue.Str(c.Id));
                cj.Set("pos", SnapshotData.Pos(c.Pos, 3));
                cj.Set("available", JsonValue.Bool(c.Available));
                if (!c.Available)
                    cj.Set("respawn_s", JsonValue.Real(Math.Round(c.RespawnTicks / (double)Simulation.TicksPerSecond, 3, MidpointRounding.AwayFromZero), 3));
                cs.Add(cj);
            }
            o.Set("charms", cs);
            return o;
        }
    }

    internal static class SnapshotData
    {
        public static JsonValue Pos(V3 v, int decimals)
        {
            JsonValue a = JsonValue.Arr();
            a.Add(JsonValue.Real(Math.Round(v.X, decimals, MidpointRounding.AwayFromZero), decimals));
            a.Add(JsonValue.Real(Math.Round(v.Y, decimals, MidpointRounding.AwayFromZero), decimals));
            a.Add(JsonValue.Real(Math.Round(v.Z, decimals, MidpointRounding.AwayFromZero), decimals));
            return a;
        }
    }

    /// <summary>
    /// One Night of gameplay at a fixed 60 ticks per second, with no window and no GPU. Unity's view
    /// layer and the headless runner both drive this class, so what the graders measure is what you play.
    /// </summary>
    public sealed class Simulation
    {
        public const int TicksPerSecond = 60;

        public Tuning T = Tuning.Standard();
        public Rng Rng = new Rng(1);
        public Player Player = new Player();
        public List<Mote> Motes = new List<Mote>();
        public List<Shade> Shades = new List<Shade>();
        public List<Lantern> Lanterns = new List<Lantern>();

        public long Tick;             // steps already run: the number a snapshot is stamped with
        public long SimTick;          // gameplay steps run (equal to Tick while nothing is paused)
        public long Score;
        public string State = "playing";
        public double DurationS = 300.0;      // length of the Night, in seconds
        public double CamYawDeg;              // the camera angle the last input carried

        private readonly List<SimEvent> _pending = new List<SimEvent>();
        private int _nextShadeIndex, _nextMoteIndex;
        private bool _over, _bonusApplied;
        // v1.1 additions: drifters, charms, the Night number, wind, pause.
        public int Night = 1;
        public bool WindActive;
        public double WindDirDeg;
        public bool Paused;
        public List<Drifter> Drifters = new List<Drifter>();
        public List<Charm> Charms = new List<Charm>();
        private bool _prevPause, _prevDash;
        private bool _randomDrifters = true, _windEnabled;        private string _overReason;
        private bool _randomMotes = true, _randomShades = true;
        private int _lastFallLost;                        // motes vanished on the last fall (for the Phase 2 event field)
        private bool _prevJump, _prevInteract, _prevPulse;    // action edges (contract section C)

        public double TimeLeftS { get { return DurationS - SimTick / (double)TicksPerSecond; } }
        public IReadOnlyList<SimEvent> PendingEvents { get { return _pending; } }
        public bool IsOver { get { return _over; } }
        public string OverReason { get { return _overReason; } }

        /// <summary>Build the Night from scratch for the given seed. No score, no events.</summary>
        public void Build(int seed)
        {
            T = Tuning.Standard();
            Rng = new Rng((ulong)seed);
            Player = new Player { Flames = T.Flames, Grounded = true };
            Motes.Clear(); Shades.Clear(); Lanterns.Clear();
            Tick = 0; SimTick = 0; Score = 0; State = "playing"; CamYawDeg = 0;
            _over = false; _bonusApplied = false; _overReason = null;
            _nextShadeIndex = 0; _nextMoteIndex = 0;
            _randomMotes = true; _randomShades = true;
            _prevJump = false; _prevInteract = false; _prevPulse = false;
            Night = 1; WindActive = false; WindDirDeg = 0; Paused = false;
            _prevPause = false; _prevDash = false;
            _randomDrifters = true; _windEnabled = false;
            Drifters.Clear(); Charms.Clear();
            DurationS = T.Night1DurationS;

            // Five big lanterns: L1 straight ahead on +Z, the rest on a pentagon of radius 25 m.
            Lanterns.Add(NewLantern("L1", new V3(0, 0, 25)));
            Lanterns.Add(NewLantern("L2", new V3(23.8, 0, 7.7)));
            Lanterns.Add(NewLantern("L3", new V3(14.7, 0, -20.2)));
            Lanterns.Add(NewLantern("L4", new V3(-14.7, 0, -20.2)));
            Lanterns.Add(NewLantern("L5", new V3(-23.8, 0, 7.7)));

            // Mote points: eight on a 12 m circle (every 45°), sixteen on a 33 m circle (every 22.5°).
            // Which points are filled at the start is the seeded part of the run.
            List<V3> points = MoteSpawnPoints();
            List<int> order = new List<int>();
            for (int i = 0; i < points.Count; i++) order.Add(i);
            Shuffle(order);
            for (int i = 0; i < Math.Min(T.StartMotes, points.Count); i++)
                Motes.Add(NewMote(points[order[i]]));

            // No shades at the start: the first two arrive on the spawn schedule at tick 300, and the
            // ids S1, S2, ... follow the order they are born in, as the standard asks.
        }

        public static List<V3> MoteSpawnPoints()
        {
            List<V3> pts = new List<V3>();
            for (int i = 0; i < 8; i++)
            {
                double r = i * 45.0 * Math.PI / 180.0;
                pts.Add(new V3(12.0 * Math.Cos(r), 0, 12.0 * Math.Sin(r)));
            }
            for (int i = 0; i < 16; i++)
            {
                double r = i * 22.5 * Math.PI / 180.0;
                pts.Add(new V3(33.0 * Math.Cos(r), 0, 33.0 * Math.Sin(r)));
            }
            return pts;
        }

        /// <summary>The twelve shade points on a 37 m circle, 15° + 30k, as Ticket-02 pins down.</summary>
        public static List<V3> ShadeSpawnPoints()
        {
            List<V3> pts = new List<V3>();
            for (int i = 0; i < 12; i++)
            {
                double r = (15.0 + i * 30.0) * Math.PI / 180.0;
                pts.Add(new V3(37.0 * Math.Cos(r), 0, 37.0 * Math.Sin(r)));
            }
            return pts;
        }

        private Lantern NewLantern(string id, V3 pos)
        {
            return new Lantern { Id = id, Pos = pos, Cost = T.LanternCost };
        }

        private Mote NewMote(V3 pos)
        {
            return new Mote { Id = "M" + (++_nextMoteIndex).ToString(CultureInfo.InvariantCulture), Pos = pos };
        }

        private Shade NewShade(V3 groundPos)
        {
            return new Shade
            {
                Id = "S" + (++_nextShadeIndex).ToString(CultureInfo.InvariantCulture),
                Hover = 1.0,
                // shade.pos is the shade's centre, so it sits one metre above the ground point
                Pos = new V3(groundPos.X, 1.0, groundPos.Z)
            };
        }

        /// <summary>Apply a scenario on top of the built Night. Setup never scores and never events.</summary>
        public void ApplyScenario(Scenario sc)
        {
            if (sc == null) return;
            if (sc.HasPlayer)
            {
                Player.Pos = sc.PlayerPos;
                Player.Carried = sc.Carried;
                Player.Flames = sc.Flames;
            }
            if (sc.HasPlayerTicks)  // v1.1 setup counters, applied as given
            {
                Player.InvulnTicks = sc.PlayerInvulnTicks;
                Player.DashCdTicks = sc.PlayerDashCdTicks;
                Player.PulseCdTicks = sc.PlayerPulseCdTicks;
                Player.CharmTicksLeft = sc.PlayerCharmTicks;
            }
            _randomMotes = sc.RandomMotes;
            _randomShades = sc.RandomShades;
            if (sc.HasMotes)
            {
                // A replaced list re-letters its ids from M1, as the contract's example shape implies.
                Motes.Clear();
                _nextMoteIndex = 0;
                foreach (V3 p in sc.Motes) Motes.Add(NewMote(p));
            }
            else if (!sc.RandomMotes) Motes.Clear();
            if (sc.HasShades)
            {
                Shades.Clear();
                foreach (ShadeSpec sp in sc.Shades)
                {
                    Shade s = new Shade { Hover = 1.0, Pos = sp.Pos, State = ShadeState.wander };
                    s.Id = string.IsNullOrEmpty(sp.Id)
                        ? "S" + (++_nextShadeIndex).ToString(CultureInfo.InvariantCulture)
                        : sp.Id;
                    if (sp.HasState) s.State = sp.State;
                    if (sp.HasStateTicksLeft) s.StateTicksLeft = sp.StateTicksLeft;
                    Shades.Add(s);
                }
            }
            else if (!sc.RandomShades) Shades.Clear();
            foreach (LanternUpdate lu in sc.Lanterns)
            {
                Lantern l = FindLantern(lu.Id);
                if (l == null) continue;
                if (lu.HasPos) l.Pos = lu.Pos;
                if (lu.HasLit) l.Lit = lu.Lit;
                if (lu.HasDeposited) l.Deposited = lu.Deposited;
            }
            if (sc.HasRandomDrifters) _randomDrifters = sc.RandomDrifters;
            if (sc.HasWindEnabled) _windEnabled = sc.WindEnabled;
            if (sc.HasDrifters)
            {
                Drifters.Clear();
                foreach (DrifterSpec d in sc.Drifters) Drifters.Add(new Drifter { Id = d.Id, Pos = d.Pos });
            }
            if (sc.HasCharms)
            {
                Charms.Clear();
                foreach (CharmSpec c in sc.Charms)
                    Charms.Add(new Charm { Id = c.Id, Pos = c.Pos, Available = !c.HasAvailable || c.Available });
            }
            if (!double.IsNaN(sc.TimeLeftS)) DurationS = sc.TimeLeftS;  // at tick 0, time_left_s is that value
        }

        public Lantern FindLantern(string id)
        {
            foreach (Lantern l in Lanterns) if (l.Id == id) return l;
            return null;
        }

        /// <summary>
        /// Run one contract step: step k reads the input of tick k and produces state k+1, and events
        /// raised during it carry tick k+1.
        /// </summary>
        public void Step(InputFrame input)
        {
            _pending.Clear();
            if (_over) return;
            if (input == null) input = new InputFrame();

            Tick++;
            CamYawDeg = input.CamYawDeg;

            // Step 1: read the input, handling pause first. Turning pause on stops the step right
            // here — the tick counter advances but sim time, cooldowns, spawns and TTLs hold still.
            bool firePause = input.Pause && !_prevPause;
            _prevPause = input.Pause;
            if (firePause && !Paused)
            {
                Paused = true; State = "paused";
                Raise(new SimEvent(Tick, "paused"));
                return;
            }
            if (firePause && Paused)
            {
                Paused = false; State = "playing";
                Raise(new SimEvent(Tick, "resumed"));
            }
            if (Paused) return;     // frozen: nothing below this line runs, no input edge is consumed

            SimTick++;

            // Contract section C: an action fires on the false -> true edge, not while a key is held.
            bool fireJump = input.Jump && !_prevJump;
            bool fireInteract = input.Interact && !_prevInteract;
            bool firePulse = input.Pulse && !_prevPulse;
            _prevJump = input.Jump; _prevInteract = input.Interact; _prevPulse = input.Pulse;
            bool fireDash = input.Dash && !_prevDash;
            _prevDash = input.Dash;

            // The order Ticket-02 pins down, adopted in Phase 1 so later phases add nothing to it.
            DecrementCounters();      // 2. timers
            MovePlayer(input, fireJump, fireDash);   // 3. movement and jump, then the fall check
            if (T.FallEnabled) CheckFall();
            Pickups();                // 4. motes
            Interact(fireInteract);   // 5. lanterns
            SafeZones();              // 6. the safe radius
            Pulse(firePulse);         // 7. Tỏa Sáng
            MoveShades();             // 8. shades
            Contacts();               // 9. contacts
            Spawns();                 // 10. scheduled spawns
            CheckEnd();               // 11. win and lose
        }

        /// <summary>The snapshot of the current state, for tick 0 and every --snapshot-every tick.</summary>
        public Snapshot TakeSnapshot()
        {
            Snapshot s = new Snapshot();
            s.Tick = Tick; s.SimTick = SimTick; s.State = State; s.TimeLeftS = TimeLeftS; s.Score = Score;
            s.PlayerPos = Player.Pos; s.Flames = Player.Flames; s.Carried = Player.Carried;
            s.Lanterns = SortedLanterns(); s.Shades = SortedShades(); s.Motes = SortedMotes();
            s.Night = Night; s.WindActive = WindActive; s.WindDirDeg = WindDirDeg;
            s.PlayerRef = Player; s.Drifters = SortedDrifters(); s.Charms = SortedCharms();
            return s;
        }

        private void DecrementCounters()
        {
            if (Player.InvulnTicks > 0) Player.InvulnTicks--;
            if (Player.PulseCdTicks > 0) Player.PulseCdTicks--;
            if (Player.DashCdTicks > 0) Player.DashCdTicks--;
            if (Player.CharmTicksLeft > 0) Player.CharmTicksLeft--;
            if (Player.FallWaitTicks > 0)
            {
                Player.FallWaitTicks--;
                if (Player.FallWaitTicks == 0) Respawn();
            }
            foreach (Shade s in Shades)
            {
                if (s.State == ShadeState.spawning) s.State = ShadeState.wander;   // a shade is "spawning" only on its birth tick
                if (s.StateTicksLeft > 0)
                {
                    s.StateTicksLeft--;
                    if (s.StateTicksLeft == 0 && (s.State == ShadeState.stunned || s.State == ShadeState.recoil))
                        s.State = ShadeState.wander;
                }
                if (s.NoDamageTicks > 0) s.NoDamageTicks--;
            }
            for (int i = Motes.Count - 1; i >= 0; i--)
            {
                Mote m = Motes[i];
                if (m.Dropped && m.TtlTicks > 0)
                {
                    m.TtlTicks--;
                    if (m.TtlTicks <= 0) Motes.RemoveAt(i);
                }
            }
        }

        private void MovePlayer(InputFrame input, bool jump, bool dash)
        {
            if (Player.FallWaitTicks > 0) return;     // the body is away: nothing moves, nothing is picked up

            double theta = input.CamYawDeg * Math.PI / 180.0;
            double mx = input.MoveX, mz = input.MoveZ;
            double len = Math.Sqrt(mx * mx + mz * mz);
            if (len > 1.0) { mx /= len; mz /= len; }  // a longer vector is normalised to length 1
            double wx = mx * Math.Cos(theta) + mz * Math.Sin(theta);
            double wz = -mx * Math.Sin(theta) + mz * Math.Cos(theta);

            // v1.1 dash: rising edge, off cooldown. The direction is the movement direction, or the
            // facing when standing still; it is locked for the whole window. No invulnerability, and
            // the cooldown counts from the moment the dash starts.
            if (dash && Player.DashCdTicks == 0 && Player.DashTicksLeft == 0)
            {
                double dx = wx, dz = wz;
                if (dx * dx + dz * dz < 1e-12) { dx = Math.Sin(theta); dz = Math.Cos(theta); }
                double dl = Math.Sqrt(dx * dx + dz * dz);
                Player.DashDirX = dx / dl; Player.DashDirZ = dz / dl;
                Player.DashTicksLeft = T.DashDurationTicks;
                if (Player.CharmTicksLeft == 0) Player.DashCdTicks = T.DashCooldownTicks;
                Raise(new SimEvent(Tick, "dash"));
            }
            bool dashing = Player.DashTicksLeft > 0;
            if (dashing) { wx = Player.DashDirX; wz = Player.DashDirZ; }

            double sp = dashing ? T.DashSpeed : (Player.CharmTicksLeft > 0 ? T.CharmRunSpeed : T.PlayerSpeed);
            double vx = wx * sp, vz = wz * sp;
            double vy = Player.Vel.Y;

            if (dashing && !jump) vy = 0;             // an air dash keeps the altitude
            else
            {
                if (jump && Player.Grounded) vy = T.JumpVelocity;    // jump only from the ground
                if (!Player.Grounded || jump) vy += T.Gravity / TicksPerSecond;
            }

            V3 np = new V3(Player.Pos.X + vx / TicksPerSecond,
                           Player.Pos.Y + vy / TicksPerSecond,
                           Player.Pos.Z + vz / TicksPerSecond);

            if (OnIsland(np))
            {
                if (np.Y <= 0.0)
                {
                    np = new V3(np.X, 0, np.Z);
                    vy = 0;
                    Player.Grounded = true;
                }
                else Player.Grounded = false;
            }
            else Player.Grounded = false;      // over the cloud sea: gravity keeps pulling

            Player.Pos = np;
            Player.Vel = new V3(vx, vy, vz);
            if (len > 1e-12) Player.FacingDeg = Math.Atan2(wz, wx) * 180.0 / Math.PI;
            if (dashing) Player.DashTicksLeft--;   // the window counts down over the ticks it runs
        }

        private bool OnIsland(V3 p) { return Math.Sqrt(p.X * p.X + p.Z * p.Z) <= T.IslandRadius; }

        private void CheckFall()
        {
            if (Player.FallWaitTicks > 0) return;     // already falling: the penalty fires once per fall
            if (Player.Pos.Y > T.FallY) return;
            // Once, ignoring invulnerability: one flame lost, every mote lost, then the wait.
            int lost = Player.Carried;
            Player.Carried = 0;
            Player.Flames--;
            // Nothing is physically dropped on a fall — the motes vanish; the lost count is the
            // Phase 2 extra field. dropped therefore reports 0 here.
            Raise(new SimEvent(Tick, "player_hit")
                .Put("by", JsonValue.Str("fall"))
                .Put("flames", JsonValue.Int(Player.Flames))
                .Put("dropped", JsonValue.Int(0)));
            if (Player.Flames <= 0) { End(false, "no_flames"); return; }   // lose at once, no respawn
            _lastFallLost = lost;   // kept for the Phase 2 event field; Phase 1 output stays as specced
            Player.FallWaitTicks = T.FallWaitTicks;
            Player.Vel = new V3(0, 0, 0);
            Player.Grounded = true;
            Player.Pos = new V3(Player.Pos.X, -100, Player.Pos.Z);         // off the world during the wait
        }

        private void Respawn()
        {
            Player.Pos = new V3(0, 0, 0);
            Player.Vel = new V3(0, 0, 0);
            Player.Grounded = true;
            Player.InvulnTicks = T.InvulnTicks;
        }

        private void Pickups()
        {
            if (Player.FallWaitTicks > 0) return;
            if (Player.Carried >= T.CarryCapacity) return;
            foreach (Mote m in SortedMotes())
            {
                if (m.SpawnTick == Tick) continue;
                if (V3.DistXZ(Player.Pos, m.Pos) > T.PickupRadius) continue;
                if (Math.Abs(Player.Pos.Y - m.Pos.Y) > T.PickupHeight) continue;
                Motes.Remove(m);
                Player.Carried++;
                Raise(new SimEvent(Tick, "mote_picked").Put("carried", JsonValue.Int(Player.Carried)));
                if (Player.Carried >= T.CarryCapacity) break;
            }
        }

        private void Interact(bool interact)
        {
            if (!interact || Player.FallWaitTicks > 0) return;
            Lantern best = null; double bestD = double.MaxValue;
            foreach (Lantern l in Lanterns)
            {
                if (l.Lit) continue;                                        // a lit lantern takes nothing more
                double d = V3.DistXZ(Player.Pos, l.Pos);
                if (d > T.InteractRadius) continue;
                if (d < bestD - 1e-9 || (Math.Abs(d - bestD) <= 1e-9 && (best == null || string.CompareOrdinal(l.Id, best.Id) < 0)))
                { best = l; bestD = d; }
            }
            if (best == null || Player.Carried <= 0) return;
            int give = Math.Min(Player.Carried, best.Cost - best.Deposited);
            if (give <= 0) return;
            Player.Carried -= give;
            best.Deposited += give;
            Score += give * T.ScorePerMote;
            Raise(new SimEvent(Tick, "deposit")
                .Put("lantern_id", JsonValue.Str(best.Id))
                .Put("amount", JsonValue.Int(give))
                .Put("deposited", JsonValue.Int(best.Deposited)));
            if (best.Deposited >= best.Cost)
            {
                best.Lit = true;
                Score += T.ScorePerLantern;
                Raise(new SimEvent(Tick, "lantern_lit").Put("lantern_id", JsonValue.Str(best.Id)));
            }
        }

        private void SafeZones()
        {
            foreach (Shade s in SortedShades())
            {
                if (s.SpawnTick == Tick) continue;
                if (s.State == ShadeState.stunned) continue;
                if (s.PushTicksLeft > 0) continue;    // a push already on its way keeps its own pace to the edge
                foreach (Lantern l in Lanterns)
                {
                    if (!l.Lit) continue;
                    double d = V3.DistXZ(s.Pos, l.Pos);
                    if (d >= T.SafeRadius - 1e-9) continue;
                    // A shade caught inside when the light comes on is pushed to the edge, without damage.
                    s.WasInside = true;
                    s.PushLeft = T.SafeRadius - d;
                    s.PushTicksLeft = T.SafeZonePushTicks;
                    s.NoDamageTicks = T.SafeZonePushTicks;
                    V3 dir = DirTo(l.Pos, s.Pos);
                    s.PushDirX = dir.X; s.PushDirZ = dir.Z;
                    break;
                }
            }
        }

        private void Pulse(bool pulse)
        {
            if (!pulse || Player.FallWaitTicks > 0) return;
            if (Player.PulseCdTicks > 0) return;
            if (Player.Carried < T.PulseCost) return;   // not enough light: nothing happens, no cost
            Player.Carried -= T.PulseCost;
            Player.PulseCdTicks = T.PulseCooldownTicks;
            V3 centre = new V3(Player.Pos.X, Player.Pos.Y + 0.8, Player.Pos.Z);
            JsonValue ids = JsonValue.Arr();
            foreach (Shade s in SortedShades())
            {
                if (s.SpawnTick == Tick) continue;
                if (V3.Dist(centre, s.Pos) > T.PulseRadius) continue;    // 3D distance to the shade centre
                s.State = ShadeState.stunned;
                s.StateTicksLeft = T.PulseStunTicks;
                s.PushLeft = T.PulsePushM;
                s.PushTicksLeft = T.PulsePushTicks;
                V3 dir = DirTo(centre, s.Pos);
                if (dir.X == 0 && dir.Z == 0) { s.PushDirX = 1; s.PushDirZ = 0; }   // same spot: push along +X
                else { s.PushDirX = dir.X; s.PushDirZ = dir.Z; }
                Score += T.ScorePerStun;
                ids.Add(JsonValue.Str(s.Id));
            }
            Raise(new SimEvent(Tick, "pulse").Put("stunned_ids", ids));
        }

        private void MoveShades()
        {
            foreach (Shade s in SortedShades())
            {
                if (s.SpawnTick == Tick) continue;

                if (s.PushTicksLeft > 0 && s.PushLeft > 1e-12)
                {
                    double step = s.PushLeft / s.PushTicksLeft;       // the whole push inside its window
                    s.Pos = new V3(s.Pos.X + s.PushDirX * step, s.Pos.Y, s.Pos.Z + s.PushDirZ * step);
                    s.PushLeft -= step;
                    s.PushTicksLeft--;
                    continue;
                }

                if (s.State == ShadeState.stunned) continue;          // held still by the light

                double d = V3.DistXZ(s.Pos, Player.Pos);
                if (s.State == ShadeState.chase)
                {
                    if (d > T.DetachRange) s.State = ShadeState.wander;
                }
                else if (s.State == ShadeState.wander && d <= T.DetectRange) s.State = ShadeState.chase;

                double speed = s.State == ShadeState.chase ? T.ShadeSpeed : T.ShadeSpeed * 0.5;
                V3 dir = s.State == ShadeState.chase ? DirTo(s.Pos, Player.Pos) : WanderDir(s);
                V3 next = new V3(s.Pos.X + dir.X * speed / TicksPerSecond, s.Pos.Y, s.Pos.Z + dir.Z * speed / TicksPerSecond);

                // The safe radius blocks movement: a shade is stopped at the edge of the light.
                foreach (Lantern l in Lanterns)
                {
                    if (!l.Lit) continue;
                    double nd = V3.DistXZ(next, l.Pos);
                    if (nd >= T.SafeRadius - 1e-9) continue;
                    V3 away = DirTo(l.Pos, next);
                    next = new V3(l.Pos.X + away.X * T.SafeRadius, next.Y, l.Pos.Z + away.Z * T.SafeRadius);
                }

                s.Pos = next;
            }
        }

        private V3 WanderDir(Shade s)
        {
            // A calm drift back toward the middle of the island after a shade loses the scent.
            double a = Math.Atan2(s.Pos.Z, s.Pos.X) + Math.PI;
            return new V3(Math.Cos(a), 0, Math.Sin(a));
        }

        private void Contacts()
        {
            if (Player.FallWaitTicks > 0) return;
            foreach (Shade s in SortedShades())
            {
                if (s.SpawnTick == Tick) continue;
                if (s.State == ShadeState.stunned) continue;          // stunned shades deal no damage
                if (s.NoDamageTicks > 0) continue;                    // being pushed out of the light
                if (V3.DistXZ(s.Pos, Player.Pos) > T.ShadeHitRadius) continue;
                if (Math.Abs(s.Pos.Y - Player.Pos.Y) > T.HitHeight) continue;
                if (Player.InvulnTicks > 0) continue;                 // Mây is glowing: the shade does nothing

                Player.Flames--;
                int drop = (int)Math.Floor(Player.Carried * T.HitDropRatio);
                Player.Carried -= drop;
                for (int i = 0; i < drop; i++) DropMote();
                Player.InvulnTicks = T.InvulnTicks;
                s.State = ShadeState.recoil;
                s.StateTicksLeft = T.RecoilTicks;
                s.PushLeft = T.RecoilPushM;
                s.PushTicksLeft = T.RecoilTicks;
                V3 away = DirTo(Player.Pos, s.Pos);
                s.PushDirX = away.X; s.PushDirZ = away.Z;

                Raise(new SimEvent(Tick, "player_hit")
                    .Put("by", JsonValue.Str("shade"))
                    .Put("flames", JsonValue.Int(Player.Flames))
                    .Put("dropped", JsonValue.Int(drop)));
                break;        // at most one flame lost per tick
            }
        }

        private void DropMote()
        {
            double a = Rng.NextDouble() * 2.0 * Math.PI;
            double r = 1.5 + Rng.NextDouble() * 1.0;              // the 1.5-2.5 m ring around Mây
            V3 p = new V3(Player.Pos.X + r * Math.Cos(a), 0, Player.Pos.Z + r * Math.Sin(a));
            Motes.Add(new Mote
            {
                Id = "M" + (++_nextMoteIndex).ToString(CultureInfo.InvariantCulture),
                Pos = p,
                Dropped = true,
                TtlTicks = T.DroppedMoteTtlTicks,
                SpawnTick = (int)Tick
            });
        }

        private void Spawns()
        {
            // Motes: one extra mote at 240, 480, ... while the cap is not reached.
            if (_randomMotes && Tick >= T.MoteSpawnStartTick && Tick % T.MoteSpawnEvery == 0 && Motes.Count < T.MoteCap)
            {
                List<V3> points = MoteSpawnPoints();
                List<int> free = new List<int>();
                for (int i = 0; i < points.Count; i++)
                {
                    bool taken = false;
                    foreach (Mote m in Motes) if (V3.DistXZ(m.Pos, points[i]) < 0.5) { taken = true; break; }
                    if (!taken) free.Add(i);
                }
                if (free.Count > 0)
                {
                    V3 p = points[free[Rng.Next(free.Count)]];
                    Motes.Add(NewMote(p));
                    Raise(new SimEvent(Tick, "mote_spawned").Put("id", JsonValue.Str(Motes[Motes.Count - 1].Id)));
                }
            }

            // Shades: two at tick 300, then one every 480 ticks while below the cap.
            if (_randomShades)
            {
                bool due = Tick == T.ShadeSpawnStartTick ||
                           (Tick > T.ShadeSpawnStartTick && (Tick - T.ShadeSpawnStartTick) % T.ShadeSpawnEvery == 0);
                if (due)
                {
                    int wanted = Tick == T.ShadeSpawnStartTick ? 2 : 1;
                    for (int i = 0; i < wanted; i++)
                    {
                        if (Shades.Count >= T.ShadeMax) break;
                        List<V3> points = ShadeSpawnPoints();
                        List<int> ok = new List<int>();
                        for (int p = 0; p < points.Count; p++)
                            if (V3.DistXZ(points[p], Player.Pos) >= T.ShadeSpawnMinPlayerDist) ok.Add(p);
                        if (ok.Count == 0) continue;                // no valid point left: skip this turn
                        Shade s = NewShade(points[ok[Rng.Next(ok.Count)]]);
                        s.SpawnTick = (int)Tick;
                        Shades.Add(s);
                        Raise(new SimEvent(Tick, "shade_spawned").Put("id", JsonValue.Str(s.Id)));
                    }
                }
            }
        }

        private void CheckEnd()
        {
            if (Player.Flames <= 0) { End(false, "no_flames"); return; }     // always a loss, checked first
            bool allLit = Lanterns.Count > 0;
            foreach (Lantern l in Lanterns) if (!l.Lit) { allLit = false; break; }
            if (allLit) { End(true, "all_lit"); return; }
            if (TimeLeftS <= 0.0) { End(false, "dawn"); return; }            // Phase 1 assumption, see docs/notes
        }

        private void End(bool won, string reason)
        {
            _over = true;
            State = won ? "won" : "lost";
            _overReason = reason;
            if (won && !_bonusApplied)
            {
                _bonusApplied = true;
                Score += (long)Math.Floor(TimeLeftS) * T.ScorePerSecondLeft + Player.Flames * T.ScorePerFlameLeft;
            }
            Raise(new SimEvent(Tick, "game_over")
                .Put("result", JsonValue.Str(State))
                .Put("reason", JsonValue.Str(reason)));
        }

        private void Raise(SimEvent e) { _pending.Add(e); }

        private void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                int t = list[i]; list[i] = list[j]; list[j] = t;
            }
        }

        // Output order is by numeric id, so two runs of the same seed print the same lines.
        private List<Drifter> SortedDrifters()
        {
            List<Drifter> l = new List<Drifter>(Drifters);
            l.Sort((a, b) => NaturalCompare(a.Id, b.Id));
            return l;
        }

        private List<Charm> SortedCharms()
        {
            List<Charm> l = new List<Charm>(Charms);
            l.Sort((a, b) => NaturalCompare(a.Id, b.Id));
            return l;
        }

        private List<Shade> SortedShades()
        {
            List<Shade> l = new List<Shade>(Shades);
            l.Sort((a, b) => NaturalCompare(a.Id, b.Id));
            return l;
        }

        private List<Mote> SortedMotes()
        {
            List<Mote> l = new List<Mote>(Motes);
            l.Sort((a, b) => NaturalCompare(a.Id, b.Id));
            return l;
        }

        private List<Lantern> SortedLanterns()
        {
            List<Lantern> l = new List<Lantern>(Lanterns);
            l.Sort((a, b) => NaturalCompare(a.Id, b.Id));
            return l;
        }

        private static int NaturalCompare(string a, string b)
        {
            int i = 0, j = 0;
            while (i < a.Length && j < b.Length)
            {
                char ca = a[i], cb = b[j];
                if (char.IsDigit(ca) && char.IsDigit(cb))
                {
                    int si = i, sj = j;
                    while (i < a.Length && char.IsDigit(a[i])) i++;
                    while (j < b.Length && char.IsDigit(b[j])) j++;
                    string na = a.Substring(si, i - si).TrimStart('0');
                    string nb = b.Substring(sj, j - sj).TrimStart('0');
                    if (na.Length != nb.Length) return na.Length - nb.Length;
                    int c = string.CompareOrdinal(na, nb);
                    if (c != 0) return c;
                }
                else
                {
                    if (ca != cb) return ca < cb ? -1 : 1;
                    i++; j++;
                }
            }
            return (a.Length - i) - (b.Length - j);
        }

        private static V3 DirTo(V3 from, V3 to)
        {
            double dx = to.X - from.X, dz = to.Z - from.Z;
            double l = Math.Sqrt(dx * dx + dz * dz);
            if (l < 1e-12) return new V3(0, 0, 0);
            return new V3(dx / l, 0, dz / l);
        }
    }
}
