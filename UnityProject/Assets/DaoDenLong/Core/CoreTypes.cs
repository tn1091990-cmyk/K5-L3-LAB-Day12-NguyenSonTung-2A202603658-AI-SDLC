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

    public enum ShadeState { wander, chase, recoil, stunned }

    public sealed class Shade
    {
        public string Id;
        public V3 Pos;              // centre of the shade (its contract reference point)
        public double Hover = 1.0;  // hover height above ground; the physics point is the centre
        public ShadeState State = ShadeState.wander;
        public int StateTicksLeft;
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
    }

    /// <summary>One fixed-tick input frame (Test Contract section C).</summary>
    public sealed class InputFrame
    {
        public long Tick;
        public double MoveX, MoveZ;
        public double CamYawDeg;
        public bool Jump, Interact, Pulse;
    }

    /// <summary>One lantern entry of a scenario file: only the fields present in the JSON are applied.</summary>
    public sealed class LanternUpdate
    {
        public string Id;
        public bool HasPos; public V3 Pos;
        public bool HasLit; public bool Lit;
        public bool HasDeposited; public int Deposited;
    }

    /// <summary>One shade entry of a scenario file.</summary>
    public sealed class ShadeSpec
    {
        public string Id;
        public V3 Pos;
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
    }
}
