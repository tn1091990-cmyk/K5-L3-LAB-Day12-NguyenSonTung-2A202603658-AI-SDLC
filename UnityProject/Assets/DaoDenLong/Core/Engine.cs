using System;
using System.Collections.Generic;

namespace DaoDenLong.Core
{
    /// <summary>
    /// The Test Contract run loop (section B/E), kept out of System.IO so the headless runner, the
    /// test suite and the Unity editor all execute exactly the same sequence of lines.
    /// </summary>
    public static class Engine
    {
        /// <summary>
        /// Run a Night and return the JSONL lines as a list of JSON values, in output order.
        /// Tick 0 is the state after the scenario and before any input; step k reads the input of
        /// tick k, produces state k+1, and its events carry tick k+1. The last line is "end".
        /// </summary>
        public static List<JsonValue> Run(int seed, Scenario scenario, List<InputFrame> inputs, long ticks, int snapshotEvery, int night = 1)
        {
            if (ticks <= 0) throw new FormatException("--ticks must be a positive integer");
            if (snapshotEvery <= 0) throw new FormatException("--snapshot-every must be a positive integer");

            Simulation sim = new Simulation();
            sim.Build(seed, night);
            if (scenario != null) sim.ApplyScenario(scenario);

            List<InputFrame> frames = inputs ?? new List<InputFrame>();
            while (frames.Count < ticks) frames.Add(new InputFrame { Tick = frames.Count });

            List<JsonValue> lines = new List<JsonValue>();
            // v1.1: the run announces its Night first — the night_start line precedes the tick-0
            // snapshot (assumption: docs/notes/ASSUMPTIONS.md).
            lines.Add(new SimEvent(0, "night_start").Put("night", JsonValue.Int(night)).ToJson());
            if (0 % snapshotEvery == 0) lines.Add(sim.TakeSnapshot().ToJson());

            for (long k = 0; k < ticks; k++)
            {
                sim.Step(frames[(int)k]);
                foreach (SimEvent e in sim.PendingEvents) lines.Add(e.ToJson());
                if (sim.Tick % snapshotEvery == 0) lines.Add(sim.TakeSnapshot().ToJson());
                if (sim.IsOver) break;
            }

            JsonValue end = JsonValue.Obj();
            end.Set("type", JsonValue.Str("end"));
            end.Set("ticks", JsonValue.Int(sim.Tick));
            end.Set("state", JsonValue.Str(sim.State));
            end.Set("score", JsonValue.Int(sim.Score));
            lines.Add(end);
            return lines;
        }
    }
}
