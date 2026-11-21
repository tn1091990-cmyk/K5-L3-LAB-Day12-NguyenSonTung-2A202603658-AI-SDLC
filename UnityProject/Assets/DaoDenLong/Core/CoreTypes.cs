using System;
using System.Collections.Generic;
using System.Globalization;

namespace DaoDenLong.Core
{
    /// <summary>Right-handed world vector, Y up, island ground at Y = 0 (Test Contract unit: metres).</summary>
    public struct V3
    {
        public double X, Y, Z;
        public V3(double x, double y, double z) { X = x; Y = y; Z = z; }
        public V3 Add(V3 o) { return new V3(X + o.X, Y + o.Y, Z + o.Z); }
        public V3 Sub(V3 o) { return new V3(X - o.X, Y - o.Y, Z - o.Z); }
        public V3 Mul(double s) { return new V3(X * s, Y * s, Z * s); }
        public double LenXZ() { return Math.Sqrt(X * X + Z * Z); }
        public double Len() { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        /// <summary>Contract distances are measured on the XZ plane between the reference points.</summary>
        public static double DistXZ(V3 a, V3 b) { double dx = a.X - b.X, dz = a.Z - b.Z; return Math.Sqrt(dx * dx + dz * dz); }
        public static double Dist(V3 a, V3 b) { return a.Sub(b).Len(); }
        public V3 NormXZ()
        {
            double l = LenXZ();
            if (l < 1e-12) return new V3(0, 0, 0);
            return new V3(X / l, 0, Z / l);
        }
        public static V3 Lerp(V3 a, V3 b, double t) { return new V3(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t); }
        public string F(int decimals)
        {
            return "[" + R(X, decimals) + "," + R(Y, decimals) + "," + R(Z, decimals) + "]";
        }
        internal static string R(double v, int d)
        {
            if (v == 0) v = 0; // collapse -0.0 so two identical runs print identical text
            return v.ToString("F" + d.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Deterministic seeded RNG (SplitMix64). The contract requires the same seed + scenario + inputs
    /// to produce byte-identical output between runs, so no framework Random is used.
    /// </summary>
    public sealed class Rng
    {
        private ulong _state;
        public Rng(ulong seed) { _state = seed == 0 ? 0x9E3779B97F4A7C15UL : seed; }
        public ulong NextUInt64()
        {
            ulong z = (_state += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
        /// <summary>Uniform in [0, 1).</summary>
        public double NextDouble() { return (NextUInt64() >> 11) * (1.0 / 9007199254740992.0); }
        /// <summary>Uniform integer in [0, n).</summary>
        public int Next(int n) { return n <= 0 ? 0 : (int)(NextUInt64() % (ulong)n); }
    }

    /// <summary>v1.1 state names exactly as the board spells them. spawning lasts until the shade's first processed tick.
    /// drain joins the list in v1.2 (Ticket-03): a shade sipping the oil of a low-fuel lantern.</summary>
    public enum ShadeState { spawning, wander, chase, recoil, stunned, drain }

    public sealed class Shade
    {
        public string Id;
        public V3 Pos;              // centre of the shade (its contract reference point)
        public double Hover = 1.0;  // hover height above ground; the physics point is the centre
        public ShadeState State = ShadeState.wander;
        public int StateTicksLeft;
        public string TargetLanternId;  // v1.2: the weak lantern this shade is draining (scenario-settable too)
        public double DetachDist = 18.0;
        public double PushLeft;     // metres still to push during the first ticks of a stun
        public int PushTicksLeft;
        public int RecoilTicksLeft;
        public int NoDamageTicks;   // grace while being pushed out of a safe zone
        public int SpawnTick;       // a new entity only starts interacting from the tick after it spawns
        public int PushSeq;         // order of the most recent push, for deterministic processing
        public double PushDirX, PushDirZ;
        public bool WasInside;      // it was already inside the light when the lantern lit
    }

    /// <summary>Mây, the little lamp keeper. Reference point: the feet.</summary>
    public sealed class Player
    {
        public V3 Pos;              // feet
        public V3 Vel;
        public bool Grounded = true;
        public int Flames;
        public int Carried;
        public int InvulnTicks;
        public int PulseCdTicks;
        public int FallWaitTicks;   // > 0 while waiting to respawn: no pickup, no hits
        public double FacingDeg;
        public int SuccessfulLights; // v1.2: successful lightings during this Night (relights count), drives capacity

        // Phase 2 (v1.1): dash and the Bùa Gió charm.
        public int DashCdTicks;     // cooldown counted from the moment the dash starts
        public int DashTicksLeft;   // > 0 while the dash is running (12 ticks)
        public double DashDirX, DashDirZ;   // direction locked at the start of the dash
        public int CharmTicksLeft;  // > 0 while Bùa Gió is active (600 ticks, refreshed on re-pickup)
        public bool Launched;       // a jump pad owns the velocity until the next landing
        public V3 LaunchVel;        // the launch velocity, kept unchanged until landing
    }

    public sealed class Mote
    {
        public string Id;
        public V3 Pos;              // point on the ground (hover is display only)
        public bool Dropped;        // dropped by a hit: lives 480 ticks, pickable from the tick after
        public int TtlTicks;        // dropped motes only; placed motes live for the whole Night
        public int SpawnTick;
    }

    public sealed class Lantern
    {
        public string Id;
        public V3 Pos;              // foot of the pole
        public bool Lit;
        public int Deposited;       // motes delivered to this lantern
        public int Cost;            // motes needed to light it
        // v1.2 (Ticket-03): the oil tank, in whole units of 1/60 fuel point (see Simulation.Tuning).
        public long FuelUnits;      // exact: decay 1 unit/tick, drain 5 units/tick
        public bool EverLit;        // has ever shone this Night: relights are cheaper and score less
        public int RelitCount;      // successful relightings during this Night
    }

    /// <summary>Gió Lạc, the wandering wind (v1.1, Nights 2-3). Reference point: its flying body.</summary>
    public sealed class Drifter
    {
        public string Id;
        public V3 Pos;
        public string State = "idle";   // idle / chase / flee / removed, the v1.1 snapshot names
        public int Stolen;              // motes it is carrying
        public bool HasStolen;          // each drifter steals at most once, ever
        public int FleeTicksLeft;       // flee window; then it vanishes with what it carries
        public int RemovalTicksLeft;    // set when a pulse frees it: vanishes 120 ticks later
        public int SpawnTick;           // interacts from the tick after it appears
    }

    /// <summary>Bùa Gió, the red wind charm (v1.1). Pickup follows the mote rules.</summary>
    public sealed class Charm
    {
        public string Id;
        public V3 Pos;
        public bool Available = true;
        public int RespawnTicks;        // counts down only while taken; then it reappears at its spot
        public int SpawnTick;
    }

    /// <summary>A jump pad (v1.1, Nights 2-3). A grounded body whose feet are on the pad's ground
    /// level inside its radius is launched with Vel; the launch velocity is then kept, unchanged,
    /// until the next landing (movement keys do not override it; a dash window does).</summary>
    public sealed class Pad
    {
        public V3 Center;                 // pad centre, on its ground level
        public double GroundY;            // the surface the launch fires from
        public V3 Vel;                    // the launch velocity, kept unchanged until landing
        public double Radius = 1.0;
    }

    public enum ZoneKind { Disc, Annulus, Rect, Ramp, Blocker }

    /// <summary>One walkable piece of ground for the Phase 2 maps: islands, bridge, tiers, ramps.
    /// Heights are exact. The player may climb at most Tuning.StepUpMax metres per tick, so tier
    /// edges are walls and only the ramps lead up (assumptions: docs/notes/ASSUMPTIONS.md).</summary>
    public sealed class Zone
    {
        public ZoneKind Kind;
        public double Cx, Cz, R;                  // Disc / Annulus / Blocker: centre and radius
        public double InnerR;                     // Annulus: inner radius
        public bool ZPositive;                    // Annulus: only the z > 0 half (Night 3 tier 1)
        public double X0, X1, Z0, Z1;             // Rect: the bridge deck
        public double Y;                          // flat height of the surface
        public double AngleDeg, RFoot, RTop, HalfWidth, YFoot, YTop;   // Ramp: a strip along a radius
        public double BlockY;                     // Blocker: it only blocks a walker standing at this height

        // Boundaries are exact double comparisons — no tolerance: Phase 1 pinned the rim step, and a
        // tolerance would quietly move it (see T10). Simple and exact beats clever.
        public bool Contains(double x, double z)
        {
            switch (Kind)
            {
                case ZoneKind.Disc:
                    return Dist2(x - Cx, z - Cz) <= R * R;
                case ZoneKind.Annulus:
                    if (ZPositive && z <= 0.0) return false;
                    double d = Dist2(x - Cx, z - Cz);
                    return d >= InnerR * InnerR && d <= R * R;
                case ZoneKind.Rect:
                    return x >= X0 && x <= X1 && z >= Z0 && z <= Z1;
                case ZoneKind.Ramp:
                    double a = AngleDeg * Math.PI / 180.0;
                    double along = (x - Cx) * Math.Cos(a) + (z - Cz) * Math.Sin(a);
                    double perp = -(x - Cx) * Math.Sin(a) + (z - Cz) * Math.Cos(a);
                    double lo = Math.Min(RFoot, RTop), hi = Math.Max(RFoot, RTop);
                    return along >= lo && along <= hi && Math.Abs(perp) <= HalfWidth;
                case ZoneKind.Blocker:
                    return Dist2(x - Cx, z - Cz) <= R * R;
            }
            return false;
        }

        /// <summary>Surface height at a point the zone contains (the ramp slopes linearly).</summary>
        public double HeightAt(double x, double z)
        {
            if (Kind != ZoneKind.Ramp) return Y;
            double a = AngleDeg * Math.PI / 180.0;
            double along = (x - Cx) * Math.Cos(a) + (z - Cz) * Math.Sin(a);
            double t = (along - RFoot) / (RTop - RFoot);            // 0 at the foot, 1 at the top
            if (t < 0.0) t = 0.0; else if (t > 1.0) t = 1.0;
            return YFoot + (YTop - YFoot) * t;
        }

        private static double Dist2(double dx, double dz) { return dx * dx + dz * dz; }
    }

    /// <summary>One fixed-tick input frame (Test Contract section C).</summary>
    public sealed class InputFrame
    {
        public long Tick;
        public double MoveX, MoveZ;
        public double CamYawDeg;
        public bool Jump, Interact, Pulse;
        public bool Dash, Pause;    // v1.1: both fire on the false -> true edge
    }

    /// <summary>One lantern entry of a scenario file: only the fields present in the JSON are applied.</summary>
    public sealed class LanternUpdate
    {
        public string Id;
        public bool HasPos; public V3 Pos;
        public bool HasLit; public bool Lit;
        public bool HasDeposited; public int Deposited;
        public bool HasFuel; public double Fuel;          // v1.2: fuel points; a lit lantern without it defaults to full
        public bool HasEverLit; public bool EverLit;      // v1.2
    }

    /// <summary>One shade entry of a scenario file.</summary>
    public sealed class ShadeSpec
    {
        public string Id;
        public V3 Pos;
        public bool HasState; public ShadeState State;   // v1.1
        public bool HasStateTicksLeft; public int StateTicksLeft;
        public bool HasTarget; public string TargetLanternId;   // v1.2: a drain shade's lantern
    }

    /// <summary>One drifter entry of a scenario file (v1.1): replaces the default drifter list.</summary>
    public sealed class DrifterSpec
    {
        public string Id;
        public V3 Pos;
    }

    /// <summary>One charm entry of a scenario file (v1.1): replaces the default charm list.</summary>
    public sealed class CharmSpec
    {
        public string Id;
        public V3 Pos;
        public bool HasAvailable; public bool Available;
    }

    /// <summary>Scenario file (Test Contract section D). A missing list keeps the level default.</summary>
    public sealed class Scenario
    {
        public bool HasPlayer;
        public V3 PlayerPos;
        public int Carried;
        public int Flames = 3;    // a scenario that only positions or loads Mây keeps the standard flame count
        public bool RandomMotes = true, RandomShades = true;
        public bool HasMotes, HasShades;
        public List<V3> Motes = new List<V3>();              // replaces the default mote list
        public List<ShadeSpec> Shades = new List<ShadeSpec>(); // replaces the default shade list
        public List<LanternUpdate> Lanterns = new List<LanternUpdate>(); // per-id update of the level lanterns
        public double TimeLeftS = double.NaN;

        // Test Contract v1.1: setup counters, the new lists and the two flags.
        public bool HasPlayerTicks;
        public int PlayerInvulnTicks, PlayerDashCdTicks, PlayerPulseCdTicks, PlayerCharmTicks;
        public bool HasPlayerSuccessfulLights; public int PlayerSuccessfulLights;   // v1.2
        public bool HasRandomDrifters, HasWindEnabled;  // a flag is only applied when the key is present
        public bool RandomDrifters = true, WindEnabled;
        public bool HasDrifters, HasCharms;
        public List<DrifterSpec> Drifters = new List<DrifterSpec>();   // replaces the default drifter list
        public List<CharmSpec> Charms = new List<CharmSpec>();         // replaces the default charm list
    }
}
