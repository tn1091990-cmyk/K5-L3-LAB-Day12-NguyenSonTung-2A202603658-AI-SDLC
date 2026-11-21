using System;
using System.Collections.Generic;

namespace DaoDenLong.Core
{
    /// <summary>
    /// v1.3 (TICKET-04) — the benchmark scene, built through the normal scenario path so the
    /// numbers measured are the numbers played. Night 1 with exactly 200 shades in wander,
    /// exactly 100 motes and L1-L3 lit with fuel 100 (L4/L5 unlit); scatter uses the seeded RNG.
    /// During the benchmark the spawn / damage / pickup / fuel-decay / end-condition systems are
    /// switched off (Simulation.Bench); movement runs the normal wander rules.
    /// </summary>
    public static class BenchScene
    {
        public const int ShadeCount = 200;
        public const int MoteCount = 100;

        /// <summary>Build the scene for the given seed. Scatter order is pinned: shades first,
        /// then motes, each drawn as a uniform-disk point (r = R*sqrt(u), theta = 2*pi*u).</summary>
        public static Scenario BuildScenario(long seed)
        {
            Rng rng = new Rng(seed == 0 ? 0x9E3779B97F4A7C15UL : (ulong)seed);
            double islandRadius = Tuning.Standard().IslandRadius;

            Scenario sc = new Scenario();
            sc.RandomShades = false;
            sc.RandomMotes = false;
            sc.HasShades = true;
            sc.HasMotes = true;

            for (int i = 1; i <= ShadeCount; i++)
                sc.Shades.Add(new ShadeSpec { Id = "B" + i.ToString(System.Globalization.CultureInfo.InvariantCulture), Pos = Scatter(rng, islandRadius) });
            for (int i = 0; i < MoteCount; i++)
                sc.Motes.Add(Scatter(rng, islandRadius));

            foreach (string id in new string[] { "L1", "L2", "L3" })
                sc.Lanterns.Add(new LanternUpdate { Id = id, HasLit = true, Lit = true, HasFuel = true, Fuel = 100.0 });

            return sc;
        }

        private static V3 Scatter(Rng rng, double radius)
        {
            double r = radius * Math.Sqrt(rng.NextDouble());
            double theta = rng.NextDouble() * 2.0 * Math.PI;
            return new V3(r * Math.Cos(theta), 0.0, r * Math.Sin(theta));
        }
    }
}
