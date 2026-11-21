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
        public int Night1DurationS = 240;         // v1.2: Ticket-03 sets the Night lengths to 240/270/300 s
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
        public int ScorePerMote = 5;              // v1.2 table: per mote deposited (new light, relight or top-up)
        public int ScorePerLantern = 100;         // first light of a lantern during the Night
        public int ScorePerRelight = 40;          // v1.2: a relight (ever_lit lantern) is worth less than a first light
        public int ScorePerStun = 5;
        public int ScorePerStunDrain = 15;        // v1.2: a total of +15 when the shade was in drain right before
        public int ScorePerSecondLeft = 0;        // v1.2: the time bonus is gone; key stays, value 0
        public int ScorePerFlameLeft = 50;        // win bonus, dawn only
        public int ScorePerDawnLantern = 50;      // v1.2 dawn bonus: per lit lantern, plus floor(fuel) points
        public int ScorePerDawnFuelPoint = 1;     // v1.2 dawn bonus: +1 per full fuel point on a lit lantern

        public double IslandRadius = 40.0;        // Night 1 - Đảo Bến, cloud sea beyond the rim

        // Numbers Ticket-02 pins down for Phase 2. The five published in the contract tuning block first.
        public double DashSpeed = 14.0;           // m/s
        public double DashDurationS = 0.2;        // 12 ticks
        public double DashCooldownS = 1.5;        // 90 ticks, counted from the dash start
        public int Night2DurationS = 270;         // v1.2 values, keys stay in the published order
        public int Night3DurationS = 300;

        // The rest Ticket-02 standardises but does not publish as contract keys.
        public int DashDurationTicks = 12;
        public int DashCooldownTicks = 90;
        public double CharmRunSpeed = 7.2;        // Bùa Gió: run speed while it lasts
        public double StepUpMax = 0.15;           // most a foot may rise per tick: tier edges are walls, ramps are not
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

        // Numbers Ticket-03 adds for Phase 3 (contract v1.2), appended after the published keys.
        // Fuel is counted in whole units of 1/60 of a fuel point, so decay (1 point/s) and drain
        // (5 points/s) are exact integers per tick and never drift (assumption, docs/notes/ASSUMPTIONS.md).
        public const long FuelUnitsPerPoint = 60; // one fuel point = 60 units
        public int FuelMax = 100;                 // a lantern lights up with full fuel
        public double FuelDecayPerS = 1.0;        // a lit lantern drinks one fuel point per second
        public int FuelPerMote = 10;              // one mote tops up ten fuel points
        public int RelightCost = 5;               // a lantern that has ever shone needs only five motes
        public double WinRatio = 0.6;             // win at dawn with ceil(0.6 x total) lanterns still lit
        public double DrainPerS = 5.0;            // a drain shade sips five fuel points per second
        public int LowFuelThreshold = 30;         // below this a lantern loses its safe zone (exactly 30 keeps it)
        public int CapacityMax = 20;              // carry cap ceiling: 10 + 2 x successful lightings, up to this
        public double DrainChaseRange = 6.0;      // a drain shade turns on Mây when it comes this close
        public double LowFuelWalkMax = 2.0;       // a wander shade starts sipping when this close to its weak lantern

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
            // v1.2 adds exactly these eight keys, appended in Ticket-03's order.
            o.Set("fuel_max", JsonValue.Int(FuelMax));
            o.Set("fuel_decay_per_s", JsonValue.Real(FuelDecayPerS, 1));
            o.Set("fuel_per_mote", JsonValue.Int(FuelPerMote));
            o.Set("relight_cost", JsonValue.Int(RelightCost));
            o.Set("win_ratio", JsonValue.Real(WinRatio, 1));
            o.Set("drain_per_s", JsonValue.Int((int)DrainPerS));
            o.Set("low_fuel_threshold", JsonValue.Int(LowFuelThreshold));
            o.Set("capacity_max", JsonValue.Int(CapacityMax));
            JsonValue s = JsonValue.Obj();
            // v1.2 score table: the five published keys keep their slots with new values,
            // the new bonus keys are appended (time bonus is gone, per_second_left stays at 0).
            s.Set("per_mote", JsonValue.Int(ScorePerMote));
            s.Set("per_lantern", JsonValue.Int(ScorePerLantern));
            s.Set("per_stun", JsonValue.Int(ScorePerStun));
            s.Set("per_second_left", JsonValue.Int(ScorePerSecondLeft));
            s.Set("per_flame_left", JsonValue.Int(ScorePerFlameLeft));
            s.Set("per_relight", JsonValue.Int(ScorePerRelight));
            s.Set("per_stun_drain", JsonValue.Int(ScorePerStunDrain));
            s.Set("per_dawn_lantern", JsonValue.Int(ScorePerDawnLantern));
            s.Set("per_dawn_fuel", JsonValue.Int(ScorePerDawnFuelPoint));
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
        public int Capacity;      // v1.2: the carry cap Mây is living under right now

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
            p.Set("capacity", JsonValue.Int(Capacity));   // v1.2: carried/capacity is what the HUD shows
            o.Set("player", p);

            JsonValue ls = JsonValue.Arr();
            foreach (Lantern l in Lanterns)
            {
                JsonValue lj = JsonValue.Obj();
                lj.Set("id", JsonValue.Str(l.Id));
                lj.Set("pos", SnapshotData.Pos(l.Pos, 3));
                lj.Set("lit", JsonValue.Bool(l.Lit));
                lj.Set("deposited", JsonValue.Int(l.Deposited));
                // v1.2: the fuel tank as a real number, 1 decimal (unlit lanterns sit at 0).
                lj.Set("fuel", JsonValue.Real(Math.Round(l.FuelUnits / (double)Tuning.FuelUnitsPerPoint, 1, MidpointRounding.AwayFromZero), 1));
                lj.Set("ever_lit", JsonValue.Bool(l.EverLit));
                lj.Set("relit_count", JsonValue.Int(l.RelitCount));
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
        public List<Zone> Zones = new List<Zone>();      // the walkable ground of the Night's map
        public List<Pad> Pads = new List<Pad>();
        private List<V3> _motePoints = new List<V3>();
        private List<V3> _shadePoints = new List<V3>();
        private int _drifterSeq, _gustIndex;
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
        public void Build(int seed) { Build(seed, 1); }

        /// <summary>Build one of the three standard Nights from scratch. No score, no events.</summary>
        public void Build(int seed, int night)
        {
            if (night < 1 || night > 3)
                throw new ArgumentOutOfRangeException("night", "night must be 1, 2 or 3");

            T = Tuning.Standard();
            Rng = new Rng((ulong)seed);
            Player = new Player { Flames = T.Flames, Grounded = true };
            Motes.Clear(); Shades.Clear(); Lanterns.Clear();
            Zones.Clear(); Pads.Clear(); Drifters.Clear(); Charms.Clear();
            Tick = 0; SimTick = 0; Score = 0; State = "playing"; CamYawDeg = 0;
            _over = false; _bonusApplied = false; _overReason = null;
            _nextShadeIndex = 0; _nextMoteIndex = 0;
            _randomMotes = true; _randomShades = true;
            _prevJump = false; _prevInteract = false; _prevPulse = false;
            Night = night; WindActive = false; WindDirDeg = 0; Paused = false;
            _prevPause = false; _prevDash = false;
            _randomDrifters = true; _windEnabled = (night == 3);   // the standard: only Đảo Chuông has wind
            _drifterSeq = 0; _gustIndex = 0;
            _motePoints = NightMotePoints(night);
            _shadePoints = NightShadePoints(night);
            DurationS = NightDurationS(night);
            BuildGeometry(night);

            // Which points are filled at the start is the seeded part of the run, so it is the first
            // thing that touches the RNG. The list order is the standard's own.
            List<int> order = new List<int>();
            for (int i = 0; i < _motePoints.Count; i++) order.Add(i);
            Shuffle(order);
            for (int i = 0; i < Math.Min(T.StartMotes, _motePoints.Count); i++)
                Motes.Add(NewMote(_motePoints[order[i]]));

            // No shades at the start: the first two arrive on the spawn schedule at tick 300, and the
            // ids S1, S2, ... follow the order they are born in, as the standard asks.
        }

        private double NightDurationS(int night)
        {
            if (night == 2) return T.Night2DurationS;
            if (night == 3) return T.Night3DurationS;
            return T.Night1DurationS;
        }

        private int ShadeEveryForNight()
        {
            if (Night == 2) return T.ShadeEveryNight2;
            if (Night == 3) return T.ShadeEveryNight3;
            return T.ShadeSpawnEvery;
        }

        private int ShadeMaxForNight()
        {
            if (Night == 2) return T.ShadeMaxNight2;
            if (Night == 3) return T.ShadeMaxNight3;
            return T.ShadeMax;
        }

        private double ShadeSpeedForNight() { return Night == 3 ? T.ShadeSpeedNight3 : T.ShadeSpeed; }

        private static V3 Polar(double cx, double cz, double r, double deg, double y)
        {
            double a = deg * Math.PI / 180.0;
            return new V3(cx + r * Math.Cos(a), y, cz + r * Math.Sin(a));
        }

        private List<V3> NightMotePoints(int night)
        {
            List<V3> pts = new List<V3>();
            if (night == 1) return MoteSpawnPoints();          // 8 points r12 (45°), 16 points r33 (22.5°)
            if (night == 2)
            {
                V3[] centres = { new V3(0, 0, 0), new V3(70, 0, 0) };
                foreach (V3 c in centres)
                {
                    for (int i = 0; i < 4; i++) pts.Add(Polar(c.X, c.Z, 10.0, i * 90.0, 0));
                    for (int i = 0; i < 8; i++) pts.Add(Polar(c.X, c.Z, 22.0, i * 45.0, 0));
                }
                return pts;
            }
            // Night 3: three tiers, so the points carry the height of the tier they sit on.
            for (int i = 0; i < 18; i++) pts.Add(Polar(0, 0, 34.0, i * 20.0, 0));        // tier 0
            for (int i = 0; i < 8; i++) pts.Add(Polar(0, 0, 18.0, 20.0 + i * 20.0, 3));  // tier 1
            for (int i = 0; i < 4; i++) pts.Add(Polar(0, 0, 5.0, 45.0 + i * 90.0, 6));   // tier 2
            return pts;
        }

        private List<V3> NightShadePoints(int night)
        {
            List<V3> pts = new List<V3>();
            if (night == 1) return ShadeSpawnPoints();         // 12 points r37, 15° + 30k
            if (night == 2)
            {
                V3[] centres = { new V3(0, 0, 0), new V3(70, 0, 0) };
                foreach (V3 c in centres)
                    for (int i = 0; i < 8; i++) pts.Add(Polar(c.X, c.Z, 26.0, 22.5 + i * 45.0, 0));
                return pts;
            }
            for (int i = 0; i < 16; i++) pts.Add(Polar(0, 0, 43.0, i * 22.5, 0));        // tier 0 ring
            return pts;
        }

        /// <summary>The standard map of the Night: walkable ground, jump pads, lanterns and charms.</summary>
        private void BuildGeometry(int night)
        {
            if (night == 1)
            {
                Zones.Add(new Zone { Kind = ZoneKind.Disc, Cx = 0, Cz = 0, R = 40.0, Y = 0 });
                Lanterns.Add(NewLantern("L1", new V3(0, 0, 25)));
                Lanterns.Add(NewLantern("L2", new V3(23.8, 0, 7.7)));
                Lanterns.Add(NewLantern("L3", new V3(14.7, 0, -20.2)));
                Lanterns.Add(NewLantern("L4", new V3(-14.7, 0, -20.2)));
                Lanterns.Add(NewLantern("L5", new V3(-23.8, 0, 7.7)));
                return;
            }
            if (night == 2)
            {
                Zones.Add(new Zone { Kind = ZoneKind.Disc, Cx = 0, Cz = 0, R = 28.0, Y = 0 });
                Zones.Add(new Zone { Kind = ZoneKind.Disc, Cx = 70, Cz = 0, R = 28.0, Y = 0 });
                Zones.Add(new Zone { Kind = ZoneKind.Rect, X0 = 26, X1 = 44, Z0 = -2, Z1 = 2, Y = 0 });
                // Bục nhảy: 14 m/s up and 24 m/s horizontal toward the other island's centre.
                Pads.Add(new Pad { Center = new V3(20, 0, 18), GroundY = 0, Vel = PadVel(20, 18, 70, 0, 14.0, 24.0) });
                Pads.Add(new Pad { Center = new V3(50, 0, -18), GroundY = 0, Vel = PadVel(50, -18, 0, 0, 14.0, 24.0) });
                Lanterns.Add(NewLantern("L1", new V3(0, 0, 20)));
                Lanterns.Add(NewLantern("L2", new V3(17, 0, -10)));
                Lanterns.Add(NewLantern("L3", new V3(-17, 0, -10)));
                Lanterns.Add(NewLantern("L4", new V3(70, 0, 20)));
                Lanterns.Add(NewLantern("L5", new V3(87, 0, -10)));
                Lanterns.Add(NewLantern("L6", new V3(53, 0, -10)));
                Charms.Add(new Charm { Id = "C1", Pos = new V3(35, 0, 0) });
                return;
            }
            // Night 3: three tiers, straight ramps, the bell tower that cannot be climbed.
            Zones.Add(new Zone { Kind = ZoneKind.Disc, Cx = 0, Cz = 0, R = 45.0, Y = 0 });
            Zones.Add(new Zone { Kind = ZoneKind.Annulus, Cx = 0, Cz = 0, InnerR = 12.0, R = 24.0, Y = 3, ZPositive = true });
            Zones.Add(new Zone { Kind = ZoneKind.Disc, Cx = 0, Cz = 0, R = 8.0, Y = 6 });
            Zones.Add(new Zone { Kind = ZoneKind.Ramp, Cx = 0, Cz = 0, AngleDeg = 30, RFoot = 30.0, RTop = 24.0, HalfWidth = 1.5, YFoot = 0, YTop = 3 });
            Zones.Add(new Zone { Kind = ZoneKind.Ramp, Cx = 0, Cz = 0, AngleDeg = 150, RFoot = 30.0, RTop = 24.0, HalfWidth = 1.5, YFoot = 0, YTop = 3 });
            Zones.Add(new Zone { Kind = ZoneKind.Blocker, Cx = 0, Cz = 0, R = 2.0, BlockY = 6 });   // Tháp chuông
            Pads.Add(new Pad { Center = new V3(0, 3, 13), GroundY = 3, Vel = new V3(0, 12, -8) });
            Lanterns.Add(NewLantern("L1", new V3(0, 6, 6)));
            Lanterns.Add(NewLantern("L2", new V3(18, 3, 10)));
            Lanterns.Add(NewLantern("L3", new V3(-18, 3, 10)));
            Lanterns.Add(NewLantern("L4", new V3(35, 0, -10)));
            Lanterns.Add(NewLantern("L5", new V3(-35, 0, -10)));
            Lanterns.Add(NewLantern("L6", new V3(20, 0, -35)));
            Lanterns.Add(NewLantern("L7", new V3(-20, 0, -35)));
            Charms.Add(new Charm { Id = "C1", Pos = new V3(30, 0, 25) });
            Charms.Add(new Charm { Id = "C2", Pos = new V3(-30, 0, 25) });
        }

        private static V3 PadVel(double px, double pz, double tx, double tz, double vy, double horiz)
        {
            double dx = tx - px, dz = tz - pz;
            double l = Math.Sqrt(dx * dx + dz * dz);
            return new V3(horiz * dx / l, vy, horiz * dz / l);
        }

        /// <summary>The point Mây starts a Night on, and the fall respawn point (assumption, see docs/notes).</summary>
        public V3 StartPoint { get { return Night == 2 ? new V3(0, 0, 0) : new V3(0, 0, 0); } }

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
            if (sc.HasPlayerSuccessfulLights) Player.SuccessfulLights = sc.PlayerSuccessfulLights;   // v1.2
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
                    if (sp.HasTarget) s.TargetLanternId = sp.TargetLanternId;   // v1.2
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
                // v1.2 setup fields: fuel in whole 1/60-point units, and the memory of having shone.
                if (lu.HasFuel) l.FuelUnits = (long)Math.Round(lu.Fuel * Tuning.FuelUnitsPerPoint);
                else if (lu.HasLit) l.FuelUnits = lu.Lit ? T.FuelMax * Tuning.FuelUnitsPerPoint : 0;
                if (lu.HasEverLit) l.EverLit = lu.EverLit;
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
            DecrementCounters();      // 2. timers (including the charm respawn)
            MovePlayer(input, fireJump, fireDash);   // 3. movement, jump, pads...
            if (Player.FallWaitTicks == 0) WindPush();   // ...and the wind, then the fall check
            if (T.FallEnabled) CheckFall();
            Pickups();                // 4. motes and charms
            Interact(fireInteract);   // 5. lanterns
            SafeZones();              // 6. the safe radius
            Pulse(firePulse);         // 7. Tỏa Sáng
            MoveShades();             // 8. shades and drifters
            MoveDrifters();
            DrifterContacts();        // 9. contacts: the drifters steal first...
            Contacts();               //    ...then the shades hit, by ascending id
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
            s.Capacity = (int)CapacityNow();       // v1.2
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
            // Bùa Gió: a taken charm counts its way back and reappears at its own spot (step 2's
            // "hồi bùa"). Like every new entity it can be picked up from the tick after it returns.
            foreach (Charm c in Charms)
            {
                if (c.Available || c.RespawnTicks <= 0) continue;
                c.RespawnTicks--;
                if (c.RespawnTicks == 0)
                {
                    c.Available = true;
                    c.SpawnTick = (int)SimTick;
                }
            }

            // v1.2 (Ticket-03): step 2 subtracts the fuel — every lit lantern drinks its own decay,
            // and every drain shade sips from the lantern it sits at. Fuel lives in whole units of
            // 1/60 of a fuel point, so a full tick is an exact integer and never drifts.
            if (T.FuelDecayPerS > 0)
            {
                long decay = (long)Math.Round(T.FuelDecayPerS);       // points/s == units/tick at 60 fps
                foreach (Lantern l in Lanterns) if (l.Lit) l.FuelUnits -= decay;
            }
            if (T.DrainPerS > 0)
            {
                long sip = (long)Math.Round(T.DrainPerS);             // points/s == units/tick at 60 fps
                foreach (Lantern l in SortedLanterns())
                {
                    if (!l.Lit) continue;
                    foreach (Shade s in SortedShades())
                    {
                        if (s.State != ShadeState.drain || s.TargetLanternId != l.Id) continue;
                        l.FuelUnits -= sip;
                        if (SimTick % 60 == 0)                        // the contract wants one line per minute of sipping
                            Raise(new SimEvent(Tick, "fuel_drain")
                                .Put("lantern_id", JsonValue.Str(l.Id))
                                .Put("shade_id", JsonValue.Str(s.Id))
                                .Put("fuel", JsonValue.Real(Math.Round(l.FuelUnits / (double)Tuning.FuelUnitsPerPoint, 1, MidpointRounding.AwayFromZero), 1)));
                    }
                }
            }

            // Immediately after step 2: a lantern whose tank ran dry goes out and remembers it shone.
            foreach (Lantern l in SortedLanterns())
            {
                if (!l.Lit || l.FuelUnits > 0) continue;
                l.Lit = false;
                l.FuelUnits = 0;
                l.Deposited = 0;          // the deposit is gone with the flame
                l.EverLit = true;
                Raise(new SimEvent(Tick, "lantern_out").Put("lantern_id", JsonValue.Str(l.Id)));
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

            // A jump pad owns the velocity until the next landing: the movement keys do not
            // override the launch, but a dash window does (the dash branch above already won).
            if (Player.Launched && !dashing) { vx = Player.LaunchVel.X; vz = Player.LaunchVel.Z; }

            if (dashing && !jump) vy = 0;             // an air dash keeps the altitude
            else
            {
                if (jump && Player.Grounded) vy = T.JumpVelocity;    // jump only from the ground
                if (!Player.Grounded || jump) vy += T.Gravity / TicksPerSecond;
            }

            double feetNow = Player.Pos.Y;

            V3 np = new V3(Player.Pos.X + vx / TicksPerSecond,
                           Player.Pos.Y + vy / TicksPerSecond,
                           Player.Pos.Z + vz / TicksPerSecond);

            // The ground a foot may stand on at the new point: the highest surface no higher than the
            // step-up window, NaN over the cloud sea, NegativeInfinity behind a wall (the bell tower).
            double g = GroundAt(np.X, np.Z, feetNow + T.StepUpMax);
            if (g == double.NegativeInfinity && Player.Grounded)
            {
                // The tower body: the walker is stopped where it is (the air path passes above it).
            }
            else if (!double.IsNaN(g) && np.Y <= g + 1e-9)
            {
                // The path touched down on a surface: land on it, feet first.
                np = new V3(np.X, g, np.Z);
                vy = 0;
                Player.Grounded = true;
                Player.Launched = false;
            }
            else Player.Grounded = false;      // mid-air, or over the cloud sea: gravity keeps pulling

            Player.Pos = np;
            Player.Vel = new V3(vx, vy, vz);
            if (len > 1e-12) Player.FacingDeg = Math.Atan2(wz, wx) * 180.0 / Math.PI;
            if (dashing) Player.DashTicksLeft--;   // the window counts down over the ticks it runs

            if (Player.Grounded) CheckPads();      // a foot touching a pad is launched for the next tick
        }

        /// <summary>
        /// The height a foot standing at (x, z) may rest on, given the highest surface it may reach
        /// (reach = current foot height + step-up window). NaN = no floor at all, NegativeInfinity =
        /// the point is inside a body you cannot climb (checked only at its own level, so you may
        /// walk under the bell tower from the tier below).
        /// </summary>
        private double GroundAt(double x, double z, double reach)
        {
            double best = double.NaN;
            foreach (Zone zone in Zones)
            {
                if (!zone.Contains(x, z)) continue;
                double h = zone.HeightAt(x, z);
                if (zone.Kind == ZoneKind.Blocker)
                {
                    // A blocker only blocks at the level it stands on; it is never a floor.
                    if (Math.Abs(reach - T.StepUpMax - zone.BlockY) <= 1e-9) return double.NegativeInfinity;
                    continue;
                }
                if (h > reach + 1e-9) continue;                   // that floor is above the reach: you walk under it
                if (double.IsNaN(best) || h > best) best = h;
            }
            return best;
        }

        private void CheckPads()
        {
            foreach (Pad pad in Pads)
            {
                if (V3.DistXZ(Player.Pos, pad.Center) > pad.Radius + 1e-9) continue;
                if (Math.Abs(Player.Pos.Y - pad.GroundY) > 1e-9) continue;
                Player.LaunchVel = pad.Vel;
                Player.Vel = pad.Vel;              // kept, unchanged, until the next landing
                Player.Launched = true;
                break;
            }
        }

        /// <summary>Night 3 wind: a gust every 30 s, 5 s long, random direction, pushing Mây 2 m/s.</summary>
        private void WindPush()
        {
            if (_windEnabled && SimTick > 0 && SimTick % T.WindGustEveryTicks <= T.WindGustTicks && SimTick % T.WindGustEveryTicks != 0)
            {
                int gust = (int)((SimTick - 1) / T.WindGustEveryTicks);
                if (gust != _gustIndex)
                {
                    _gustIndex = gust;
                    WindDirDeg = Rng.NextDouble() * 360.0;      // the direction is drawn once, at the gust start
                }
                WindActive = true;
            }
            else WindActive = false;
            if (!WindActive) return;
            double a = WindDirDeg * Math.PI / 180.0;
            // The push moves the body, on the ground and in the air alike (assumption, see docs/notes).
            Player.Pos = new V3(Player.Pos.X + Math.Cos(a) * T.WindPushSpeed / TicksPerSecond,
                                Player.Pos.Y,
                                Player.Pos.Z + Math.Sin(a) * T.WindPushSpeed / TicksPerSecond);
        }

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
                .Put("dropped", JsonValue.Int(0))
                .Put("lost", JsonValue.Int(lost)));
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
            if (Player.Carried >= CapacityNow()) return;
            foreach (Mote m in SortedMotes())
            {
                if (m.SpawnTick == SimTick) continue;
                if (V3.DistXZ(Player.Pos, m.Pos) > T.PickupRadius) continue;
                if (Math.Abs(Player.Pos.Y - m.Pos.Y) > T.PickupHeight) continue;
                Motes.Remove(m);
                Player.Carried++;
                Raise(new SimEvent(Tick, "mote_picked").Put("carried", JsonValue.Int(Player.Carried)));
                if (Player.Carried >= CapacityNow()) break;
            }

            // Bùa Gió: picked up by the same rules as a mote, id order, from the tick after it appears.
            foreach (Charm c in SortedCharms())
            {
                if (!c.Available || c.SpawnTick == SimTick) continue;
                if (V3.DistXZ(Player.Pos, c.Pos) > T.PickupRadius) continue;
                if (Math.Abs(Player.Pos.Y - c.Pos.Y) > T.PickupHeight) continue;
                Player.CharmTicksLeft = T.CharmDurationTicks;   // a re-pickup refreshes the same window
                c.Available = false;
                c.RespawnTicks = T.CharmRespawnTicks;
                Raise(new SimEvent(Tick, "charm_picked")
                    .Put("charm_id", JsonValue.Str(c.Id))
                    .Put("charm_ticks", JsonValue.Int(Player.CharmTicksLeft)));
            }
        }

        private void Interact(bool interact)
        {
            if (!interact || Player.FallWaitTicks > 0) return;
            Lantern best = null; double bestD = double.MaxValue;
            foreach (Lantern l in Lanterns)
            {
                // v1.2: selection sees unlit lanterns AND lit ones that can still take fuel;
                // a lit lantern at full fuel is not selectable at all - no event, no score.
                if (l.Lit && l.FuelUnits >= T.FuelMax * Tuning.FuelUnitsPerPoint) continue;
                double d = V3.DistXZ(Player.Pos, l.Pos);
                if (d > T.InteractRadius) continue;
                if (d < bestD - 1e-9 || (Math.Abs(d - bestD) <= 1e-9 && (best == null || string.CompareOrdinal(l.Id, best.Id) < 0)))
                { best = l; bestD = d; }
            }
            if (best == null || Player.Carried <= 0) return;

            long per = T.FuelPerMote * Tuning.FuelUnitsPerPoint;   // one mote, in fuel units
            long full = T.FuelMax * Tuning.FuelUnitsPerPoint;
            if (best.Lit)
            {
                // v1.2 top-up: every mote poured in adds ten fuel points, never over the maximum.
                long need = (full - best.FuelUnits + per - 1) / per;      // ceil, in whole motes
                int used = Math.Min(Player.Carried, (int)need);
                if (used <= 0) return;                                    // never happens: full ones are not selectable
                Player.Carried -= used;
                best.FuelUnits = Math.Min(full, best.FuelUnits + used * per);
                Score += used * T.ScorePerMote;
                Raise(new SimEvent(Tick, "refuel")
                    .Put("lantern_id", JsonValue.Str(best.Id))
                    .Put("amount", JsonValue.Int(used))
                    .Put("fuel", JsonValue.Real(Math.Round(best.FuelUnits / (double)Tuning.FuelUnitsPerPoint, 1, MidpointRounding.AwayFromZero), 1)));
                return;
            }

            long cost = best.EverLit ? T.RelightCost : best.Cost;      // v1.2: a lantern that has shone asks for five
            int give = (int)Math.Min(Player.Carried, cost - best.Deposited);
            if (give <= 0) return;
            Player.Carried -= give;
            best.Deposited += give;
            Score += give * T.ScorePerMote;
            Raise(new SimEvent(Tick, "deposit")
                .Put("lantern_id", JsonValue.Str(best.Id))
                .Put("amount", JsonValue.Int(give))
                .Put("deposited", JsonValue.Int(best.Deposited)));
            if (best.Deposited >= cost)
            {
                bool relight = best.EverLit;
                best.Lit = true;
                best.EverLit = true;
                if (relight) best.RelitCount++;                     // v1.2: the counter the snapshot prints
                Player.SuccessfulLights++;                          // v1.2: and the counter the capacity grows with
                best.FuelUnits = full;                              // a fresh light burns full
                Score += relight ? T.ScorePerRelight : T.ScorePerLantern;
                Raise(new SimEvent(Tick, "lantern_lit")
                    .Put("lantern_id", JsonValue.Str(best.Id))
                    .Put("relight", JsonValue.Bool(relight)));
            }
        }

        private void SafeZones()
        {
            foreach (Shade s in SortedShades())
            {
                if (s.SpawnTick == SimTick) continue;
                if (s.State == ShadeState.stunned) continue;
                if (s.PushTicksLeft > 0) continue;    // a push already on its way keeps its own pace to the edge
                foreach (Lantern l in Lanterns)
                {
                    if (!HasZone(l)) continue;                          // v1.2: only a lantern with fuel keeps its zone
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
                if (s.SpawnTick == SimTick) continue;
                if (V3.Dist(centre, s.Pos) > T.PulseRadius) continue;    // 3D distance to the shade centre
                bool wasDrain = s.State == ShadeState.drain;            // v1.2: a shade caught sipping is worth more
                s.State = ShadeState.stunned;
                s.StateTicksLeft = T.PulseStunTicks;
                s.PushLeft = T.PulsePushM;
                s.PushTicksLeft = T.PulsePushTicks;
                V3 dir = DirTo(centre, s.Pos);
                if (dir.X == 0 && dir.Z == 0) { s.PushDirX = 1; s.PushDirZ = 0; }   // same spot: push along +X
                else { s.PushDirX = dir.X; s.PushDirZ = dir.Z; }
                Score += wasDrain ? T.ScorePerStunDrain : T.ScorePerStun;
                ids.Add(JsonValue.Str(s.Id));
            }
            Raise(new SimEvent(Tick, "pulse").Put("stunned_ids", ids));

            // Tỏa Sáng meets the drifters after the shades: a chasing one flees without stealing; a
            // fleeing one that carries loot drops every mote it took, right here, and is gone after
            // 120 ticks. Nothing about a drifter costs a flame.
            foreach (Drifter d in SortedDrifters())
            {
                if (d.SpawnTick == SimTick || d.State == "removed") continue;
                if (V3.Dist(centre, d.Pos) > T.PulseRadius) continue;
                if (d.State == "chase") d.State = "flee";
                else if (d.State == "flee" && d.Stolen > 0)
                {
                    int dropped = d.Stolen;
                    d.Stolen = 0;
                    d.RemovalTicksLeft = T.DrifterDropVanishTicks;
                    for (int i = 0; i < dropped; i++)
                    {
                        Mote m = new Mote
                        {
                            Id = "M" + (++_nextMoteIndex).ToString(CultureInfo.InvariantCulture),
                            Pos = new V3(d.Pos.X, 0, d.Pos.Z),
                            Dropped = true,
                            TtlTicks = T.DroppedMoteTtlTicks,
                            SpawnTick = (int)SimTick
                        };
                        double g = GroundAt(m.Pos.X, m.Pos.Z, d.Pos.Y - 1.0 + 1e-9);
                        m.Pos = new V3(m.Pos.X, double.IsNaN(g) ? m.Pos.Y : g, m.Pos.Z);
                        Motes.Add(m);
                    }
                    Raise(new SimEvent(Tick, "drifter_drop")
                        .Put("drifter_id", JsonValue.Str(d.Id))
                        .Put("amount", JsonValue.Int(dropped)));
                }
            }
        }

        private void MoveShades()
        {
            foreach (Shade s in SortedShades())
            {
                if (s.SpawnTick == SimTick) continue;

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

                // v1.2 (Ticket-03): a drain shade stands at its weak lantern and sips; the siphon
                // itself ran in step 2. It leaves when Mây comes close, or when the lantern is gone,
                // back above the line, or out of reach.
                if (s.State == ShadeState.drain)
                {
                    Lantern dl = LanternById(s.TargetLanternId);
                    double dlat = dl == null ? double.MaxValue : V3.DistXZ(s.Pos, dl.Pos);
                    if (d <= T.DrainChaseRange) s.State = ShadeState.chase;
                    else if (dl == null || !dl.Lit || dl.FuelUnits >= ZoneFuelUnits() || dlat > T.LowFuelWalkMax + 1e-9) s.State = ShadeState.wander;
                    else continue;                                     // still sipping: it stands where it is
                }

                if (s.State == ShadeState.chase)
                {
                    if (d > T.DetachRange) s.State = ShadeState.wander;
                }
                else if (s.State == ShadeState.wander && d <= T.DetectRange) s.State = ShadeState.chase;

                double baseSpeed = ShadeSpeedForNight();
                double speed = s.State == ShadeState.chase ? baseSpeed : baseSpeed * 0.5;
                V3 dir;
                if (s.State == ShadeState.chase) dir = DirTo(s.Pos, Player.Pos);
                else
                {
                    Lantern weak = NearestWeakLantern(s.Pos);           // v1.2: the wander path leads to the weak light
                    if (weak != null && V3.DistXZ(s.Pos, weak.Pos) <= T.LowFuelWalkMax + 1e-9)
                    { s.State = ShadeState.drain; s.TargetLanternId = weak.Id; continue; }   // arrived: it stands, and sips
                    dir = weak != null ? DirTo(s.Pos, weak.Pos) : WanderDir(s);
                }
                V3 next = new V3(s.Pos.X + dir.X * speed / TicksPerSecond, s.Pos.Y, s.Pos.Z + dir.Z * speed / TicksPerSecond);

                // The safe radius blocks movement: a shade is stopped at the edge of the light.
                foreach (Lantern l in Lanterns)
                {
                    if (!HasZone(l)) continue;                          // v1.2: only a lantern with fuel keeps its zone
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

        // ---- Gió Lạc (v1.1, Nights 2-3). The body hovers 1 m over the ground under it; the contact
        // rules are XZ-only, so the hover height never changes an outcome (assumption, docs/notes).

        private void MoveDrifters()
        {
            for (int i = Drifters.Count - 1; i >= 0; i--)
            {
                Drifter d = Drifters[i];
                if (d.SpawnTick == SimTick) continue;     // a new entity acts from the next tick
                if (d.State == "removed") continue;       // frozen: it stays in the list as it left the play

                if (d.RemovalTicksLeft > 0)
                {
                    d.RemovalTicksLeft--;                 // the freed drifter vanishes 120 ticks after the drop
                    if (d.RemovalTicksLeft == 0) Drifters.RemoveAt(i);
                    continue;
                }

                double d2p = V3.DistXZ(d.Pos, Player.Pos);
                if (d.State == "idle" && Player.Carried >= T.DrifterChaseMinCarried && d2p <= T.DrifterChaseRange)
                    d.State = "chase";                    // checked every tick, as the standard asks
                else if (d.State == "chase" && (Player.Carried < T.DrifterChaseMinCarried || d2p > T.DrifterChaseRange))
                    d.State = "idle";                     // the condition is gone: back to a slow glide

                double speed = 0, dx = 0, dz = 0;
                if (d.State == "chase") { speed = T.DrifterFlySpeed; V3 dir = DirTo(d.Pos, Player.Pos); dx = dir.X; dz = dir.Z; }
                else if (d.State == "idle")
                {
                    speed = T.DrifterIdleSpeed;
                    if (d2p > 1e-12) { V3 dir = DirTo(d.Pos, Player.Pos); dx = dir.X; dz = dir.Z; }   // a calm glide after Mây
                }
                else if (d.State == "flee")
                {
                    speed = T.DrifterFlySpeed;
                    V3 dir = DirTo(Player.Pos, d.Pos);    // straight away from Mây, recomputed each tick
                    dx = dir.X; dz = dir.Z;
                    d.FleeTicksLeft--;
                    if (d.FleeTicksLeft <= 0)
                    {
                        d.State = "removed";              // vanishes with what it carries (assumption)
                        continue;
                    }
                }

                if (speed > 0)
                {
                    double nx = d.Pos.X + dx * speed / TicksPerSecond;
                    double nz = d.Pos.Z + dz * speed / TicksPerSecond;
                    double g = GroundAt(nx, nz, d.Pos.Y - 1.0 + 1e-9);
                    d.Pos = new V3(nx, (double.IsNaN(g) ? d.Pos.Y : g + 1.0), nz);
                }
            }
        }

        private void DrifterContacts()
        {
            if (Player.FallWaitTicks > 0) return;
            foreach (Drifter d in SortedDrifters())
            {
                if (d.SpawnTick == SimTick || d.State == "removed") continue;
                if (d.HasStolen) continue;                // each drifter steals at most once, ever
                if (V3.DistXZ(d.Pos, Player.Pos) > T.DrifterStealRadius) continue;
                // Mây's invulnerability does not stop a steal, and no flame is lost (the standard).
                int amount = Math.Min(T.DrifterStealMax, Player.Carried);
                Player.Carried -= amount;
                d.Stolen += amount;
                d.HasStolen = true;
                d.State = "flee";
                d.FleeTicksLeft = T.DrifterFleeTicks;
                Raise(new SimEvent(Tick, "drifter_steal")
                    .Put("drifter_id", JsonValue.Str(d.Id))
                    .Put("amount", JsonValue.Int(amount))
                    .Put("carried", JsonValue.Int(Player.Carried)));
            }
        }

        private void Contacts()
        {
            if (Player.FallWaitTicks > 0) return;
            foreach (Shade s in SortedShades())
            {
                if (s.SpawnTick == SimTick) continue;
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
                SpawnTick = (int)SimTick
            });
        }

        private void Spawns()
        {
            // Motes: one extra mote at 240, 480, ... while the cap is not reached.
            if (_randomMotes && SimTick >= T.MoteSpawnStartTick && SimTick % T.MoteSpawnEvery == 0 && Motes.Count < T.MoteCap)
            {
                List<V3> points = _motePoints;
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

            // Shades: two at tick 300, then one every interval while below the cap. The interval, the
            // cap and the points are the Night's own (480/6, 420/8, 360/10).
            if (_randomShades)
            {
                int every = ShadeEveryForNight();
                bool due = SimTick == T.ShadeSpawnStartTick ||
                           (SimTick > T.ShadeSpawnStartTick && (SimTick - T.ShadeSpawnStartTick) % every == 0);
                if (due)
                {
                    int wanted = SimTick == T.ShadeSpawnStartTick ? 2 : 1;
                    for (int i = 0; i < wanted; i++)
                    {
                        if (Shades.Count >= ShadeMaxForNight()) break;
                        List<V3> points = _shadePoints;
                        List<int> ok = new List<int>();
                        for (int p = 0; p < points.Count; p++)
                            if (V3.DistXZ(points[p], Player.Pos) >= T.ShadeSpawnMinPlayerDist) ok.Add(p);
                        if (ok.Count == 0) continue;                // no valid point left: skip this turn
                        Shade s = NewShade(points[ok[Rng.Next(ok.Count)]]);
                        s.SpawnTick = (int)SimTick;
                        Shades.Add(s);
                        Raise(new SimEvent(Tick, "shade_spawned").Put("id", JsonValue.Str(s.Id)));
                    }
                }
            }

            // Gió Lạc: one drifter at tick 1200, 2400, ... while fewer than two are on the island,
            // born on one of the Night's mist points at random (Nights 2-3 only, as the standard says).
            if (Night >= 2 && _randomDrifters && SimTick >= T.DrifterSpawnStartTick && SimTick % T.DrifterSpawnEvery == 0)
            {
                int active = 0;
                foreach (Drifter d in Drifters) if (d.State != "removed") active++;
                if (active < T.DrifterMax)
                {
                    List<V3> points = _shadePoints;
                    List<int> ok = new List<int>();
                    for (int p = 0; p < points.Count; p++)
                        if (V3.DistXZ(points[p], Player.Pos) >= T.ShadeSpawnMinPlayerDist) ok.Add(p);
                    if (ok.Count > 0)
                    {
                        V3 p = points[ok[Rng.Next(ok.Count)]];
                        Drifter d = new Drifter
                        {
                            Id = "D" + (++_drifterSeq).ToString(CultureInfo.InvariantCulture),
                            Pos = new V3(p.X, 1.0, p.Z),
                            State = "idle",
                            SpawnTick = (int)SimTick
                        };
                        Drifters.Add(d);
                        Raise(new SimEvent(Tick, "drifter_spawned").Put("id", JsonValue.Str(d.Id)));
                    }
                }
            }
        }

        private void CheckEnd()
        {
            if (Player.Flames <= 0) { End(false, "no_flames"); return; }     // always a loss, checked first
            // v1.2 (Ticket-03): no early win any more. The Night runs its full length and Mây
            // wins only by being alive at dawn with at least 60% of the lanterns lit.
            if (TimeLeftS > 0.0) return;
            long lit = 0;
            foreach (Lantern l in Lanterns) if (l.Lit) lit++;
            long need = (long)Math.Ceiling(T.WinRatio * Lanterns.Count);      // 0.6 x 5 = 2.999... -> 3, as the ticket spells it
            if (lit >= need) End(true, "dawn_survived");
            else End(false, "dawn");
        }

        private void End(bool won, string reason)
        {
            _over = true;
            State = won ? "won" : "lost";
            _overReason = reason;
            if (won && !_bonusApplied)
            {
                _bonusApplied = true;
                // v1.2: the time bonus is gone. The dawn bonus is paid only on a win at dawn:
                // per lit lantern +50 plus floor(fuel), plus +50 per flame left.
                foreach (Lantern l in Lanterns)
                    if (l.Lit) Score += T.ScorePerDawnLantern + l.FuelUnits / Tuning.FuelUnitsPerPoint * T.ScorePerDawnFuelPoint;
                Score += Player.Flames * T.ScorePerFlameLeft;
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

        // ---- v1.2 helpers (Ticket-03).

        /// <summary>Carry cap: 10 + 2 per successful lighting during this Night, up to the ceiling.</summary>
        private long CapacityNow()
        {
            long c = T.CarryCapacity + 2 * Player.SuccessfulLights;
            return c > T.CapacityMax ? T.CapacityMax : c;
        }

        /// <summary>Fuel units at which a lantern keeps its safe zone (exactly 30 keeps it).</summary>
        private long ZoneFuelUnits() { return T.LowFuelThreshold * Tuning.FuelUnitsPerPoint; }

        /// <summary>True when this lantern's light still counts as a safe zone: lit and not below the line.</summary>
        private bool HasZone(Lantern l) { return l.Lit && l.FuelUnits >= ZoneFuelUnits(); }

        /// <summary>The lantern with this id, or null. (v1.2)</summary>
        private Lantern LanternById(string id)
        {
            if (id == null) return null;
            foreach (Lantern l in Lanterns) if (l.Id == id) return l;
            return null;
        }

        /// <summary>The nearest lit lantern under the low-fuel line (tie -> smaller id), or null.</summary>
        private Lantern NearestWeakLantern(V3 from)
        {
            Lantern best = null; double bestD = double.MaxValue;
            foreach (Lantern l in Lanterns)
            {
                if (!l.Lit || l.FuelUnits >= ZoneFuelUnits()) continue;
                double d = V3.DistXZ(from, l.Pos);
                if (d < bestD - 1e-9 || (Math.Abs(d - bestD) <= 1e-9 && (best == null || string.CompareOrdinal(l.Id, best.Id) < 0)))
                { best = l; bestD = d; }
            }
            return best;
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
