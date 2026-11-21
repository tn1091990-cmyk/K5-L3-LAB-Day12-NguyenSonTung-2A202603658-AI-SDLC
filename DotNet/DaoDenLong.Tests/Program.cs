// DaoDenLong.Tests — Phase 1 test suite for the shared deterministic core.
//
// Self-contained (no NuGet packages): every test drives Engine.Run with an inline
// scenario + inputs document and asserts on the JSONL lines — the very output the
// hidden lab-board tests will diff. One command runs everything:
//
//     dotnet run --project DotNet/DaoDenLong.Tests
//
// Each test prints PASS/FAIL; the process exits with the number of failed tests.
// Numbers checked here are the standard constants of the Test Contract.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DaoDenLong.Core;

namespace DaoDenLong.Tests
{
    internal static class Program
    {
        private static int _passed;
        private static int _failed;
        private static readonly List<string> Failures = new List<string>();

        private static int Main()
        {
            var tests = new (string Name, Action Body)[]
            {
                ("T01 determinism: same seed+scenario+inputs byte-identical", T01_Determinism),
                ("T02 tick model: snapshot 0 post-scenario, step k stamps k+1, no setup score", T02_TickModel),
                ("T03 movement: 6 m/s, diagonal normalised, camera yaw, jump only from ground", T03_Movement),
                ("T04 pickup: XZ radius 1.0 m inclusive, height rule 1.5 m", T04_PickupRadius),
                ("T05 capacity: 10 motes, pickup refused at cap", T05_Capacity),
                ("T06 interact: radius 2.0 m, deposit memory, tie -> smaller id", T06_Interact),
                ("T07 pulse: cost 3, radius 5 m 3D, cooldown 3 s, stun+push, +5 per hit", T07_Pulse),
                ("T08 shade hit: -1 flame, drop floor(carried/2) in ring, invuln 90, recoil", T08_ShadeHit),
                ("T09 safe zone: shade inside pushed to 6 m edge within 30 ticks, no damage", T09_SafeZone),
                ("T10 fall off island: one penalty, motes lost, 60-tick wait, respawn", T10_FallOffIsland),
                ("T11 win/lose order: no_flames first, no early win, dawn_survived (v1.2)", T11_WinLose),
                ("T12 spawns: motes 240/480/720, two shades at 300, ids in birth order", T12_SpawnSchedule),
                ("T13 input semantics: defaults, sticky fields, edges, strict tick order", T13_InputSemantics),
                ("T14 malformed scenario/inputs are rejected (exit code 2 path)", T14_MalformedInput),
                ("T15 output shape: JSONL lines, 3-decimal coords, no early stop (v1.2), end line last", T15_OutputShape),
                ("T16 snapshot ids in natural order (S1 < S2 < S10)", T16_NaturalIdOrder),
                ("T17 dash: 14 m/s x 12 ticks, cooldown 90 from the start, air dash keeps altitude", T17_Dash),
                ("T18 pause: tick advances, sim frozen, snapshot says paused, resume works", T18_Pause),
                ("T19 contract v1.2: new tuning keys, new snapshot fields, format rejections", T19_ContractV11),
                ("T20 multi-night: per-night schedules, clocks, night_start line", T20_MultiNight),
                ("T21 jump pads: launch velocity kept until landing; keys do not override; dash does", T21_JumpPads),
                ("T22 drifters: steal once, flee 240, pulse-flee, pulse-drop, vanish", T22_Drifters),
                ("T23 charm: pickup, 7.2 m/s, dash off cooldown, expiry, respawn cycle", T23_Charm),
                ("T24 wind (Night 3): gust windows, seeded direction, deterministic drift", T24_Wind),
                ("T25 fuel (v1.2): top-up math, relight costs 5, refuel scores +5", T25_Fuel),
                ("T26 drain (v1.2): walk-in, drain events every 60, lantern out, push-out", T26_Drain),
                ("T27 win rules (v1.2): no early win, dawn threshold, dawn_survived", T27_WinRules),
                ("T28 capacity (v1.2): +2 per successful light, capped at 20", T28_CapacityScale),
                ("T29 v1.2 fields: scenario fuel/ever_lit/drain-target round-trip", T29_FieldsV12),
                ("T30 leaderboard: name rules, top-10, score desc, ties keep achievement order", T30_Leaderboard),
                ("T31 soak inputs: same seed, same stream; the three Night clocks", T31_SoakInputs),
            };

            foreach (var (name, body) in tests)
            {
                string fail;
                try { body(); fail = null; }
                catch (Exception e) { fail = e.Message + " | stack: " + (e.StackTrace ?? "").Replace("\r\n", " <- "); }

                if (fail == null) { _passed++; Console.WriteLine("PASS  " + name); }
                else { _failed++; Failures.Add(name + "\n      " + fail); Console.WriteLine("FAIL  " + name + "\n      " + fail); }
            }

            Console.WriteLine();
            Console.WriteLine($"{_passed}/{_passed + _failed} tests passed.");
            foreach (string f in Failures) Console.WriteLine("  - " + f);
            return _failed;
        }

        // ---- harness ----------------------------------------------------------

        private static List<JsonValue> Run(int seed, string scenarioJson, string inputsJson, long ticks, int snapshotEvery, int night = 1)
        {
            Scenario scenario = Loader.ScenarioFromJson(scenarioJson ?? "{}");
            List<InputFrame> inputs = Loader.InputsFromJson(inputsJson ?? "{\"frames\":[]}", (int)ticks);
            return Engine.Run(seed, scenario, inputs, ticks, snapshotEvery, night);
        }

        private static List<JsonValue> OfType(List<JsonValue> lines, string type)
        {
            var r = new List<JsonValue>();
            foreach (JsonValue l in lines) if (T(l.Get("type")) == type) r.Add(l);
            return r;
        }

        private static List<JsonValue> EventsNamed(List<JsonValue> lines, string name)
        {
            var r = new List<JsonValue>();
            foreach (JsonValue l in OfType(lines, "event")) if (T(l.Get("name")) == name) r.Add(l);
            return r;
        }

        // Events that came from the simulation itself, i.e. everything except the runner's
        // v1.1 night_start announcement (T02 pins "setup scores nothing, emits nothing").
        private static List<JsonValue> SimEventsOf(List<JsonValue> lines)
        {
            var r = new List<JsonValue>();
            foreach (JsonValue l in OfType(lines, "event")) if (T(l.Get("name")) != "night_start") r.Add(l);
            return r;
        }

        private static JsonValue SnapAt(List<JsonValue> lines, long tick)
        {
            foreach (JsonValue l in OfType(lines, "snapshot"))
                if (l.Get("tick").I == tick) return l;
            throw new Exception("no snapshot at tick " + tick);
        }

        private static double N(JsonValue v) { return v.Num(double.NaN); }
        private static string T(JsonValue v) { return v.Text(null); }
        private static bool B(JsonValue v) { return v.Flag(false); }
        private static double Axis(JsonValue pos, int i) { return pos.Vec(3)[i]; }

        private static void Assert(bool cond, string what)
        {
            if (!cond) throw new Exception(what);
        }

        private static void AssertNear(double actual, double expected, double tol, string what)
        {
            if (Math.Abs(actual - expected) > tol)
                throw new Exception($"{what}: got {actual.ToString("0.####", CultureInfo.InvariantCulture)}, expected {expected.ToString("0.####", CultureInfo.InvariantCulture)} ± {tol}");
        }

        private static void AssertEq(long actual, long expected, string what)
        {
            if (actual != expected) throw new Exception($"{what}: got {actual}, expected {expected}");
        }

        private static void AssertEq(string actual, string expected, string what)
        {
            if (actual != expected) throw new Exception($"{what}: got \"{actual}\", expected \"{expected}\"");
        }

        private static void AssertIds(JsonValue arr, params string[] expected)
        {
            // Members are objects with an id field (snapshots) or plain strings (event data).
            var got = new List<string>();
            foreach (JsonValue m in arr.Arr_) got.Add(m.Get("id") != null ? T(m.Get("id")) : T(m));
            AssertEq(string.Join(",", got), string.Join(",", expected), "id order");
        }

        private static void ThrowsFormat(Action a, string what)
        {
            bool threw = false;
            try { a(); }
            catch (FormatException) { threw = true; }
            Assert(threw, what + " must be rejected (FormatException -> runner exit 2)");
        }

        private static string Serialize(List<JsonValue> lines)
        {
            var sb = new StringBuilder();
            foreach (JsonValue l in lines) sb.Append(l.Serialize()).Append('\n');
            return sb.ToString();
        }

        // Value-for-value tree compare (numbers by value, key order included) so the contract
        // file and the core's own tuning print can be checked against each other.
        private static bool SameTree(JsonValue a, JsonValue b)
        {
            if (a == null || b == null) return a == null && b == null;
            switch (a.Type)
            {
                case JsonValue.Kind.Obj:
                    if (b.Type != JsonValue.Kind.Obj || a.Members.Count != b.Members.Count) return false;
                    for (int i = 0; i < a.Members.Count; i++)
                        if (a.Members[i].Key != b.Members[i].Key) return false;   // the ladder pins the order
                    for (int i = 0; i < a.Members.Count; i++)
                        if (!SameTree(a.Members[i].Value, b.Members[i].Value)) return false;
                    return true;
                case JsonValue.Kind.NumInt:
                    return (b.Type == JsonValue.Kind.NumInt && a.I == b.I)
                        || b.Type == JsonValue.Kind.NumReal && Math.Abs(a.I - b.D) < 1e-9;
                case JsonValue.Kind.NumReal:
                    return (b.Type == JsonValue.Kind.NumReal && Math.Abs(a.D - b.D) < 1e-9)
                        || b.Type == JsonValue.Kind.NumInt && Math.Abs(a.D - b.I) < 1e-9;
                case JsonValue.Kind.Str: return b.Type == JsonValue.Kind.Str && a.S == b.S;
                case JsonValue.Kind.Bool: return b.Type == JsonValue.Kind.Bool && a.B == b.B;
                default: return false;
            }
        }

        // ---- tests ------------------------------------------------------------

        private static void T01_Determinism()
        {
            const string sc = "{\"motes\":[[10,0,0],[0,0,-12],[12,0,12]],\"shades\":[{\"pos\":[20,0,20],\"id\":\"S1\"},{\"pos\":[-20,0,20]}]}";
            const string inp = "{\"frames\":[{\"tick\":0,\"move\":[0.7,0.7]},{\"tick\":60,\"move\":[0,0]}]}";

            List<JsonValue> a = Run(7, sc, inp, 700, 50);
            List<JsonValue> b = Run(7, sc, inp, 700, 50);
            AssertEq(Serialize(a), Serialize(b), "two identical runs differ");

            // Determinism must survive id bookkeeping: ids are regenerated per run.
            Assert(EventsNamed(a, "shade_spawned").Count == EventsNamed(b, "shade_spawned").Count, "spawn counts differ");
            Assert(EventsNamed(a, "shade_spawned").Count > 0, "the run should have exercised spawned ids");

            List<JsonValue> c = Run(8, sc, inp, 700, 50);
            Assert(Serialize(a) != Serialize(c), "a different seed must change the world somewhere");
        }

        private static void T02_TickModel()
        {
            // Snapshot 0 reflects the scenario and nothing else: no score, no events.
            List<JsonValue> lines = Run(3,
                "{\"player\":{\"pos\":[1,0,2],\"carried\":4,\"flames\":3},\"random_motes\":false,\"random_shades\":false,\"motes\":[[0,0,5]],\"shades\":[],\"lanterns\":[{\"id\":\"L1\",\"lit\":true}]}",
                null, 1, 1);

            JsonValue s0 = SnapAt(lines, 0);
            AssertEq(s0.Get("sim_tick").I, 0, "sim_tick at tick 0");
            AssertEq(T(s0.Get("state")), "playing", "state at tick 0");
            AssertNear(Axis(s0.Get("player").Get("pos"), 0), 1.0, 1e-9, "scenario player x");
            AssertEq(s0.Get("player").Get("carried").I, 4, "scenario carried");
            AssertEq(s0.Get("motes_active").I, 1, "scenario motes replace the defaults");
            AssertEq(s0.Get("score").I, 0, "setup must not score");
            AssertEq(SimEventsOf(lines).Count, 0, "setup must not emit simulation events (only the runner's night_start line)");
            // A pre-lit lantern earns no +100: one idle tick keeps the score at 0.
            AssertEq(SnapAt(lines, 1).Get("score").I, 0, "score after one idle tick");

            // Step k creates state k+1: one input frame at tick 0 moves the player exactly once.
            lines = Run(3, "{\"random_motes\":false,\"random_shades\":false,\"motes\":[],\"shades\":[]}",
                "{\"frames\":[{\"tick\":0,\"move\":[1,0]}]}", 1, 1);
            AssertNear(Axis(SnapAt(lines, 1).Get("player").Get("pos"), 0), 0.1, 1e-9, "step 0 moves 6 m/s / 60");
        }

        private static void T03_Movement()
        {
            const string sc = "{\"random_motes\":false,\"random_shades\":false,\"motes\":[],\"shades\":[]}";

            // Straight +Z for 60 ticks at 6 m/s = 6.000 m.
            JsonValue p = SnapAt(Run(1, sc, "{\"frames\":[{\"tick\":0,\"move\":[0,1]}]}", 60, 60), 60)
                .Get("player").Get("pos");
            AssertNear(Axis(p, 2), 6.0, 1e-6, "z after 60 ticks at run speed");
            AssertNear(Axis(p, 0), 0.0, 1e-6, "x unchanged on a straight run");

            // Diagonal input is normalised to length 1: both axes advance 6/sqrt(2) m in 1 s.
            p = SnapAt(Run(1, sc, "{\"frames\":[{\"tick\":0,\"move\":[1,1]}]}", 60, 60), 60)
                .Get("player").Get("pos");
            AssertNear(Axis(p, 0), 6.0 / Math.Sqrt(2), 1e-3, "diagonal x");
            AssertNear(Axis(p, 2), 6.0 / Math.Sqrt(2), 1e-3, "diagonal z");

            // With cam_yaw_deg = 90, pushing z = 1 sends the player along +X (x' = z sinθ).
            p = SnapAt(Run(1, sc, "{\"frames\":[{\"tick\":0,\"move\":[0,1],\"cam_yaw_deg\":90}]}", 60, 60), 60)
                .Get("player").Get("pos");
            AssertNear(Axis(p, 0), 6.0, 1e-6, "yawed move x");
            AssertNear(Axis(p, 2), 0.0, 1e-6, "yawed move z");

            // Jump 7 m/s, gravity -20 m/s²: apex t = 0.35 s -> tick 21, height 1.225 m.
            List<JsonValue> lines = Run(1, sc, "{\"frames\":[{\"tick\":0,\"jump\":true}]}", 42, 21);
            Assert(Axis(SnapAt(lines, 21).Get("player").Get("pos"), 1) > 1.0, "airborne near the apex at tick 21");
            AssertNear(Axis(SnapAt(lines, 42).Get("player").Get("pos"), 1), 0.0, 1e-9, "landed by tick 42");

            // Jump only from the ground: a second edge mid-flight does nothing.
            lines = Run(1, sc, "{\"frames\":[{\"tick\":0,\"jump\":true},{\"tick\":10,\"jump\":true}]}", 42, 42);
            AssertNear(Axis(SnapAt(lines, 42).Get("player").Get("pos"), 1), 0.0, 1e-9, "single jump, back on the ground");

            // Air control is full: steering mid-jump changes XZ.
            lines = Run(1, sc, "{\"frames\":[{\"tick\":0,\"jump\":true,\"move\":[0,1]}]}", 21, 21);
            Assert(Axis(SnapAt(lines, 21).Get("player").Get("pos"), 2) > 1.5, "air control moves the player forward mid-jump");
        }

        private static void T04_PickupRadius()
        {
            // A mote exactly at 1.0 m XZ is picked during step 0 -> event stamped tick 1.
            List<JsonValue> ev = EventsNamed(Run(5, "{\"random_shades\":false,\"shades\":[],\"motes\":[[1.0,0,0]]}", null, 1, 1), "mote_picked");
            AssertEq(ev.Count, 1, "pickup at exactly 1.0 m");
            AssertEq(ev[0].Get("tick").I, 1, "pickup event tick");
            AssertEq(ev[0].Get("data").Get("carried").I, 1, "carried after pickup");

            // 1.01 m is outside the radius: nothing happens in the same tick.
            AssertEq(EventsNamed(Run(5, "{\"random_shades\":false,\"shades\":[],\"motes\":[[1.01,0,0]]}", null, 1, 1), "mote_picked").Count, 0,
                "no pickup beyond 1.0 m");

            // Height rule between Mây's feet and the ground point under the mote: ≤ 1.5 m.
            AssertEq(EventsNamed(Run(5, "{\"random_shades\":false,\"shades\":[],\"motes\":[[1.0,1.5,0]]}", null, 1, 1), "mote_picked").Count, 1,
                "pickup at exactly Δy 1.5 m");
            AssertEq(EventsNamed(Run(5, "{\"random_shades\":false,\"shades\":[],\"motes\":[[1.0,1.6,0]]}", null, 1, 1), "mote_picked").Count, 0,
                "no pickup at Δy 1.6 m");
        }

        private static void T05_Capacity()
        {
            List<JsonValue> lines = Run(5, "{\"player\":{\"carried\":10},\"random_shades\":false,\"shades\":[],\"motes\":[[0.5,0,0]]}", null, 2, 1);
            AssertEq(EventsNamed(lines, "mote_picked").Count, 0, "pickup refused at capacity 10");

            // One slot free: the mote lands in it.
            lines = Run(5, "{\"player\":{\"carried\":9},\"random_shades\":false,\"shades\":[],\"motes\":[[0.5,0,0]]}", null, 1, 1);
            AssertEq(EventsNamed(lines, "mote_picked").Count, 1, "pickup with one free slot");
        }

        private static void T06_Interact()
        {
            // Ten motes carried, two unlit lanterns at exactly 2.0 m, equidistant -> the
            // smaller id (L1) is the one that receives them.
            const string sc = "{\"player\":{\"carried\":10},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,2]},{\"id\":\"L2\",\"pos\":[0,0,-2]}]}";
            const string inp = "{\"frames\":[{\"tick\":0,\"interact\":true},{\"tick\":1,\"interact\":false}]}";
            List<JsonValue> lines = Run(5, sc, inp, 2, 1);

            List<JsonValue> dep = EventsNamed(lines, "deposit");
            AssertEq(dep.Count, 1, "one deposit for one button edge");
            AssertEq(dep[0].Get("tick").I, 1, "deposit event tick");
            AssertEq(T(dep[0].Get("data").Get("lantern_id")), "L1", "tie -> smaller id");
            AssertEq(dep[0].Get("data").Get("amount").I, 10, "all 10 motes go in");
            AssertEq(dep[0].Get("data").Get("deposited").I, 10, "deposited total");
            List<JsonValue> lit = EventsNamed(lines, "lantern_lit");
            AssertEq(lit.Count, 1, "L1 lights up");
            AssertEq(T(lit[0].Get("data").Get("lantern_id")), "L1", "lit lantern id");
            // Score: 10 motes * 5 + 100 for lighting. (v1.2: Ticket-03's score table prices a mote
            // at +5, replacing the old +10 - this pinned value updated with reason, not silently.)
            AssertEq(SnapAt(lines, 2).Get("score").I, 150, "score after deposit + light");

            // Partial deposit: the lantern remembers progress (8 stored + 1 carried = 9/10).
            lines = Run(5, "{\"player\":{\"carried\":1},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,2],\"deposited\":8}]}", inp, 2, 1);
            JsonValue s1 = SnapAt(lines, 1);
            JsonValue l1 = null;
            foreach (JsonValue l in s1.Get("lanterns").Arr_) if (T(l.Get("id")) == "L1") l1 = l;
            Assert(l1 != null, "L1 in snapshot");
            AssertEq(l1.Get("deposited").I, 9, "progress 8 + 1 carried");
            AssertEq(B(l1.Get("lit")) ? 1 : 0, 0, "not lit at 9/10");
            AssertEq(EventsNamed(lines, "lantern_lit").Count, 0, "no lantern_lit event below cost");
            // Only the carried mote scores (+5 under v1.2's table); the stored 8 came from the scenario setup.
            AssertEq(s1.Get("score").I, 5, "score for one deposited mote");

            // Out of interact range (2.01 m) -> nothing happens.
            lines = Run(5, "{\"player\":{\"carried\":3},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,2.01]}]}", inp, 2, 1);
            AssertEq(EventsNamed(lines, "deposit").Count, 0, "no deposit beyond 2.0 m");

            // Empty hands press: nothing to deposit, no event.
            lines = Run(5, "{\"player\":{\"carried\":0},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,2],\"deposited\":5}]}", inp, 2, 1);
            AssertEq(EventsNamed(lines, "deposit").Count, 0, "min(0, missing) = 0 deposits nothing");
        }

        private static void T07_Pulse()
        {
            // Shade comfortably inside the 5 m pulse radius (3D distance to the shade centre,
            // measured from feet + 0.8 m).
            const string sc = "{\"player\":{\"carried\":6},\"random_motes\":false,\"random_shades\":false,\"shades\":[{\"pos\":[3,1,0],\"id\":\"S1\"}]}";
            const string inp = "{\"frames\":[{\"tick\":0,\"pulse\":true}]}";
            List<JsonValue> lines = Run(9, sc, inp, 1, 1);

            List<JsonValue> pulses = EventsNamed(lines, "pulse");
            AssertEq(pulses.Count, 1, "pulse fires on the button edge");
            AssertEq(pulses[0].Get("tick").I, 1, "pulse event tick");
            AssertIds(pulses[0].Get("data").Get("stunned_ids"), "S1");
            // Cost 3 motes, +5 score per shade hit.
            AssertEq(SnapAt(lines, 1).Get("player").Get("carried").I, 3, "pulse cost 3 motes");
            AssertEq(SnapAt(lines, 1).Get("score").I, 5, "+5 per shade stunned");
            // Stun 120 ticks, and during the first 15 ticks the shade is pushed a total of
            // 4 m horizontally: at tick 1 it has moved 4/15 m along +X (away from Mây).
            AssertNear(Axis(SnapAt(lines, 1).Get("shades").Arr_[0].Get("pos"), 0), 3.0 + 4.0 / 15.0, 1e-3, "push step at tick 1");
            AssertEq(T(SnapAt(lines, 1).Get("shades").Arr_[0].Get("state")), "stunned", "shade state stunned at tick 1");

            // Boundary: the target at 5.01 m is not stunned; the pulse still fires and costs.
            lines = Run(9, "{\"player\":{\"carried\":6},\"random_motes\":false,\"random_shades\":false,\"shades\":[{\"pos\":[5.01,0.8,0],\"id\":\"S1\"}]}", inp, 1, 1);
            pulses = EventsNamed(lines, "pulse");
            AssertEq(pulses.Count, 1, "pulse fires with nobody in range");
            AssertIds(pulses[0].Get("data").Get("stunned_ids"));
            AssertEq(SnapAt(lines, 1).Get("player").Get("carried").I, 3, "pulse cost paid even with no target");

            // Not enough motes -> no pulse at all.
            lines = Run(9, "{\"player\":{\"carried\":2},\"random_motes\":false,\"random_shades\":false,\"shades\":[{\"pos\":[3,1,0],\"id\":\"S1\"}]}", inp, 1, 1);
            AssertEq(EventsNamed(lines, "pulse").Count, 0, "pulse needs 3 motes");

            // Cooldown 3 s = 180 ticks: a fresh edge (false in between) right after tick 0
            // is still blocked by the cooldown.
            lines = Run(9, sc, "{\"frames\":[{\"tick\":0,\"pulse\":true},{\"tick\":1,\"pulse\":false},{\"tick\":2,\"pulse\":true}]}", 3, 3);
            AssertEq(EventsNamed(lines, "pulse").Count, 1, "cooldown blocks the second pulse");

            // A re-pulse after the cooldown refreshes the stun and scores again (each shade,
            // every pulse — the standard scores stunned shades too). The button must return
            // to false between the two presses so the second press is a real edge.
            lines = Run(9, "{\"player\":{\"carried\":9},\"random_motes\":false,\"random_shades\":false,\"shades\":[{\"id\":\"S1\",\"pos\":[3,1,0]}]}",
                "{\"frames\":[{\"tick\":0,\"pulse\":true},{\"tick\":1,\"pulse\":false},{\"tick\":181,\"pulse\":true}]}", 182, 182);
            pulses = EventsNamed(lines, "pulse");
            AssertEq(pulses.Count, 2, "pulse available again after the cooldown");
            AssertEq(pulses[1].Get("tick").I, 182, "pulse at frame tick 181 -> event tick 182");
            AssertIds(pulses[1].Get("data").Get("stunned_ids"), "S1");
            AssertEq(SnapAt(lines, 182).Get("score").I, 10, "+5 for each pulse hit");
        }

        private static void T08_ShadeHit()
        {
            // Shade inside the contact radius from tick 0 (listed shades act immediately);
            // the contact lands during step 0, so the event is stamped tick 1.
            const string sc = "{\"player\":{\"carried\":5,\"flames\":3},\"random_motes\":false,\"random_shades\":false,\"shades\":[{\"pos\":[0.5,1.0,0],\"id\":\"S1\"}]}";
            List<JsonValue> lines = Run(11, sc, null, 1, 1);

            List<JsonValue> hits = EventsNamed(lines, "player_hit");
            AssertEq(hits.Count, 1, "hit during step 0");
            AssertEq(hits[0].Get("tick").I, 1, "hit event tick");
            AssertEq(T(hits[0].Get("data").Get("by")), "shade", "hit by shade");
            AssertEq(hits[0].Get("data").Get("flames").I, 2, "one flame lost");
            AssertEq(hits[0].Get("data").Get("dropped").I, 2, "floor(5/2) motes dropped");

            // Dropped motes must sit in the 1.5-2.5 m ring around Mây and stay alive (TTL 480).
            JsonValue motes = SnapAt(lines, 1).Get("motes");
            AssertEq(motes.Arr_.Count, 2, "dropped motes exist in the snapshot");
            foreach (JsonValue m in motes.Arr_)
            {
                double r = Math.Sqrt(Axis(m.Get("pos"), 0) * Axis(m.Get("pos"), 0) + Axis(m.Get("pos"), 2) * Axis(m.Get("pos"), 2));
                Assert(r >= 1.5 - 1e-6 && r <= 2.5 + 1e-6, "dropped mote inside the 1.5-2.5 m ring, got " + r.ToString(CultureInfo.InvariantCulture));
            }
            // They sit outside the 1.0 m pickup radius, so a standing player cannot re-grab
            // them ("re-pickable from the next tick" means the rules allow it, not that it
            // happens automatically at arm's length).
            AssertEq(EventsNamed(lines, "mote_picked").Count, 0, "no auto re-pick while standing 1.5+ m away");

            // Recoil pushes the shade 2 m over 60 ticks, chase resumes, contact happens
            // around tick ~84 — but the 90-tick invulnerability holds until tick 91.
            lines = Run(11, sc, null, 95, 95);
            hits = EventsNamed(lines, "player_hit");
            AssertEq(hits.Count, 2, "second hit lands once invulnerability is over");
            Assert(hits[1].Get("tick").I >= 91, "no hit during the 90-tick invulnerability, got tick " + hits[1].Get("tick").I);
            AssertEq(hits[1].Get("data").Get("flames").I, 1, "flames 2 -> 1 on the second hit");
            AssertEq(SnapAt(lines, 95).Get("score").I, 0, "hits never score");

            // Vertical rule: Δy > 1.5 m never damages.
            AssertEq(EventsNamed(Run(11, "{\"player\":{\"carried\":0},\"random_motes\":false,\"random_shades\":false,\"shades\":[{\"pos\":[0.5,1.0,0],\"id\":\"S1\"}]}", null, 1, 1), "player_hit").Count, 1, "Δy 1.0 m hits");
            AssertEq(EventsNamed(Run(11, "{\"player\":{\"carried\":0},\"random_motes\":false,\"random_shades\":false,\"shades\":[{\"pos\":[0.5,2.6,0],\"id\":\"S1\"}]}", null, 5, 5), "player_hit").Count, 0, "Δy 2.6 > 1.5 never hits");

            // The hit recoil is 2 m in 60 ticks: measured at tick 60 the shade sits ~2 m
            // further from the origin than at tick 1 (it cannot get closer during recoil).
            lines = Run(11, sc, null, 60, 30);
            JsonValue s60 = SnapAt(lines, 60).Get("shades").Arr_[0];
            Assert(Axis(s60.Get("pos"), 0) >= 0.5, "recoil pushes the shade away, not towards");
        }

        private static void T09_SafeZone()
        {
            // L1 is lit by the scenario at the origin (no score, no event). A shade parked at
            // 3 m is inside the 6 m safe zone -> pushed out to the edge within 30 ticks.
            const string sc = "{\"random_motes\":false,\"motes\":[],\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,0],\"lit\":true}],\"shades\":[{\"pos\":[3,1,0],\"id\":\"S1\"}]}";
            List<JsonValue> lines = Run(13, sc, null, 60, 15);

            static double ShadeR(JsonValue snap)
            {
                JsonValue pos = snap.Get("shades").Arr_[0].Get("pos");
                return Math.Sqrt(Axis(pos, 0) * Axis(pos, 0) + Axis(pos, 2) * Axis(pos, 2));
            }

            AssertNear(ShadeR(SnapAt(lines, 30)), 6.0, 1e-6, "shade reaches the 6 m edge by tick 30");
            AssertEq(EventsNamed(lines, "player_hit").Count, 0, "no damage while being pushed out");
            Assert(ShadeR(SnapAt(lines, 60)) >= 6.0 - 1e-6, "shade blocked at the 6 m edge afterwards");
            AssertEq(EventsNamed(lines, "player_hit").Count, 0, "the 6 m bubble means no contact damage");

            // A shade that starts outside is blocked at the edge, never inside: chase range
            // (12 m) brings it to the player, so start it between 6 and 12 m and watch it stall.
            lines = Run(13, "{\"random_motes\":false,\"motes\":[],\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,0],\"lit\":true}],\"shades\":[{\"pos\":[10,1,0],\"id\":\"S1\"}]}", null, 120, 60);
            Assert(ShadeR(SnapAt(lines, 120)) >= 6.0 - 1e-3, "chasing shade never enters the lit lantern's bubble");
            AssertEq(EventsNamed(lines, "player_hit").Count, 0, "player resting inside the bubble is safe");
        }

        private static void T10_FallOffIsland()
        {
            // Run straight off the rim (island radius 40 m, Y < -10 triggers): one penalty
            // flame, every mote lost, a 60-tick wait, then respawn at the start with velocity 0.
            const string sc = "{\"player\":{\"pos\":[39.5,0,0],\"carried\":5},\"random_motes\":false,\"motes\":[[0,0,0]],\"random_shades\":false,\"shades\":[]}";
            const string inp = "{\"frames\":[{\"tick\":0,\"move\":[1,0]},{\"tick\":66,\"move\":[0,0]}]}";   // stop right after the fall so the respawn point stays clean
            List<JsonValue> lines = Run(17, sc, inp, 140, 20);

            List<JsonValue> hits = EventsNamed(lines, "player_hit");
            AssertEq(hits.Count, 1, "one fall penalty");
            AssertEq(T(hits[0].Get("data").Get("by")), "fall", "hit by fall");
            AssertEq(hits[0].Get("data").Get("flames").I, 2, "flames 3 -> 2");

            // The carried motes are lost (not dropped on the ground), and the mote sitting at
            // the respawn point may only be picked up after the 60-tick wait ends.
            List<JsonValue> picks = EventsNamed(lines, "mote_picked");
            AssertEq(picks.Count, 1, "the mote at the start is picked exactly once");
            Assert(hits[0].Get("data").Get("dropped").I == 0, "lost motes are not dropped, they vanish (dropped = 0; the lost count is Phase 2's extra field)");
            Assert(hits[0].Get("tick").I == 65, "gravity crosses -10 m at tick 65 (binary accumulation puts the rim step one early vs exact decimal math), got tick " + hits[0].Get("tick").I);
            Assert(picks[0].Get("tick").I >= hits[0].Get("tick").I + 60, "no pickups during the 60-tick wait");

            JsonValue s140 = SnapAt(lines, 140).Get("player");
            AssertNear(Axis(s140.Get("pos"), 0), 0.0, 1e-9, "respawned at the start");
            AssertNear(Axis(s140.Get("pos"), 2), 0.0, 1e-9, "respawned at the start (z)");

            // Falling with the last flame ends the game immediately, no respawn.
            lines = Run(17, "{\"player\":{\"pos\":[39.5,0,0],\"flames\":1},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]}", inp, 140, 70);
            List<JsonValue> end = OfType(lines, "end");
            AssertEq(end.Count, 1, "exactly one end line");
            AssertEq(T(end[0].Get("state")), "lost", "last flame lost on a fall -> loss");
            AssertEq(end[0].Get("ticks").I, 65, "stopped on the fall tick (binary accumulation crosses the rim at tick 5, ~1 s of falling)");
            AssertEq(end[0].Get("score").I, 0, "no win bonus on a loss");
        }

        private static long SnapTickOfEvent(List<JsonValue> lines, string name)
        {
            foreach (JsonValue l in lines)
                if (T(l.Get("type")) == "event" && T(l.Get("name")) == name) return l.Get("tick").I;
            return -1;
        }

        private static void T11_WinLose()
        {
            // v1.2: lighting the last lantern no longer ends the Night early - the win now runs
            // on the full duration. (Pinned block rewritten with reason: Ticket-03 replaces
            // "win immediately when all lit".) Score = 10 motes*5 + 100 first light, no dawn bonus yet.
            const string scWin = "{\"player\":{\"carried\":10},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5],\"deposited\":0},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true},{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}]}";
            const string inpP = "{\"frames\":[{\"tick\":0,\"interact\":true}]}";
            List<JsonValue> lines = Run(21, scWin, inpP, 3, 1);
            List<JsonValue> end = OfType(lines, "end");
            AssertEq(end.Count, 1, "exactly one end line");
            AssertEq(T(end[0].Get("state")), "playing", "no early win on the lighting tick any more");
            AssertEq(end[0].Get("ticks").I, 3, "the run uses all its ticks");
            AssertEq(EventsNamed(lines, "game_over").Count, 0, "no game_over during the Night");
            AssertEq(end[0].Get("score").I, 150, "score: 50 motes + 100 first light, no dawn bonus yet");

            // Loss on 0 flames wins the tie against all_lit (checked first): every lantern
            // is lit by the scenario and the shade contact drains the last flame in step 0.
            const string scBoth = "{\"player\":{\"flames\":1},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"lit\":true},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true},{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}],\"shades\":[{\"pos\":[0.5,1.0,0],\"id\":\"S1\"}]}";
            lines = Run(21, scBoth, null, 2, 1);
            end = OfType(lines, "end");
            AssertEq(T(end[0].Get("state")), "lost", "flames hit to zero -> loss even with all lit");
            AssertEq(end[0].Get("ticks").I, 1, "the loss happens in step 0");
            AssertEq(T(EventsNamed(lines, "game_over")[0].Get("data").Get("reason")), "no_flames", "reason no_flames");
            AssertEq(EventsNamed(lines, "player_hit").Count, 1, "the hit happened");
            AssertEq(end[0].Get("score").I, 0, "the loss pays no bonus");

            // Dawn is a loss in Phase 1 (time runs out with nothing lit).
            lines = Run(21, "{\"player\":{\"flames\":3},\"random_motes\":false,\"random_shades\":false,\"motes\":[],\"shades\":[],\"time_left_s\":0.05}", null, 5, 5);
            end = OfType(lines, "end");
            AssertEq(T(end[0].Get("state")), "lost", "dawn is a loss");
            AssertEq(T(EventsNamed(lines, "game_over")[0].Get("data").Get("reason")), "dawn", "reason dawn");
            AssertEq(end[0].Get("ticks").I, 3, "time_left crosses 0 during step 3");
            AssertEq(end[0].Get("score").I, 0, "no bonus on a dawn loss");

            // Alive with 5/5 lit on the very tick time runs out -> won at dawn: the lighting
            // lands in the same step where time_left crosses zero. (Pinned block rewritten with
            // reason: Ticket-03 replaces the all_lit win - the reason is dawn_survived now, and
            // the dawn bonus pays per lit lantern +50+floor(fuel) plus +50 per flame left.)
            const string scDawn = "{\"player\":{\"carried\":10},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5]},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true},{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}],\"time_left_s\":0.016666666666666666}";
            lines = Run(21, scDawn, "{\"frames\":[{\"tick\":0,\"interact\":true}]}", 5, 1);
            end = OfType(lines, "end");
            AssertEq(T(end[0].Get("state")), "won", "alive + 5/5 lit at the time-up tick -> won");
            AssertEq(end[0].Get("ticks").I, 1, "the winning step is step 0 -> tick 1");
            AssertEq(T(EventsNamed(lines, "game_over")[0].Get("data").Get("reason")), "dawn_survived", "reason dawn_survived");
            AssertEq(EventsNamed(lines, "game_over")[0].Get("data").Get("reason") != null ? 1 : 0, 1, "game_over carries a reason");
            // Score: 50 motes + 100 first light, then the dawn bonus: L1 lit this step is full
            // (50+100), the four scenario lanterns decayed one tick (50+99 each), plus 3*50 flames.
            AssertEq(end[0].Get("score").I, 150 + 150 + 4 * 149 + 150, "dawn bonus per the v1.2 table");
        }

        private static void T12_SpawnSchedule()
        {
            // Defaults: 20 motes at start, +1 at ticks 240, 480, ... below the cap of 30;
            // no shades at tick 0 — the first spawn tick 300 makes two, ids follow the
            // birth order.
            List<JsonValue> lines = Run(31, "{\"lanterns\":[]}", null, 730, 730);
            List<JsonValue> ms = EventsNamed(lines, "mote_spawned");
            AssertEq(ms.Count, 3, "mote spawns at 240, 480, 720");
            AssertEq(ms[0].Get("tick").I, 240, "first mote spawn tick");
            AssertEq(ms[1].Get("tick").I, 480, "second mote spawn tick");
            AssertEq(ms[2].Get("tick").I, 720, "third mote spawn tick");

            List<JsonValue> ss = EventsNamed(lines, "shade_spawned");
            AssertEq(ss.Count, 2, "two shades at tick 300, next batch only at tick 780");
            AssertEq(ss[0].Get("tick").I, 300, "first shade spawn tick");
            AssertEq(ss[1].Get("tick").I, 300, "two shades on the first spawn tick");

            JsonValue s730 = SnapAt(lines, 730);
            AssertEq(s730.Get("motes_active").I, 23, "20 + 3 spawned motes");
            AssertEq(s730.Get("shades").Arr_.Count, 2, "both shades come from the tick 300 spawn");
            // Every spawn point sits on the r = 37 m ring, 12 points 30° apart from 15°.
            // Measured at the tick-300 snapshot, right at spawn; wander drifts them inward later.
            List<JsonValue> at300 = Run(31, "{\"lanterns\":[]}", null, 300, 300);
            foreach (JsonValue sh in SnapAt(at300, 300).Get("shades").Arr_)
            {
                double r = Math.Sqrt(Axis(sh.Get("pos"), 0) * Axis(sh.Get("pos"), 0) + Axis(sh.Get("pos"), 2) * Axis(sh.Get("pos"), 2));
                Assert(r > 36.0, "spawned shade on the r = 37 ring, got " + r.ToString("0.00", CultureInfo.InvariantCulture));
            }
            // Birth order ids: the tick-300 pair is S1, S2 — matching the contract example.
            AssertIds(s730.Get("shades"), "S1", "S2");
            AssertEq(T(ss[0].Get("data").Get("id")), "S1", "first spawned id");
            AssertEq(T(ss[1].Get("data").Get("id")), "S2", "second spawned id");

            // Spawn points must stay ≥ 15 m from the player: park Mây at (2,0,0) — every ring
            // point is still ~35 m away, so nothing is skipped.
            lines = Run(31, "{\"player\":{\"pos\":[2,0,0]},\"lanterns\":[]}", null, 310, 310);
            AssertEq(EventsNamed(lines, "shade_spawned").Count, 2, "all spawn points stay valid");

            // random_shades:false -> listed only, no auto spawns; motes still spawn per schedule.
            lines = Run(31, "{\"random_shades\":false,\"shades\":[]}", null, 310, 310);
            AssertEq(EventsNamed(lines, "shade_spawned").Count, 0, "no auto shades when the list replaces defaults");
            AssertEq(EventsNamed(lines, "mote_spawned").Count, 1, "motes still spawn at 240 (480 is past the run)");

            // random_motes:false -> mote spawns skipped, shade schedule intact.
            lines = Run(31, "{\"random_motes\":false,\"motes\":[]}", null, 310, 310);
            AssertEq(EventsNamed(lines, "mote_spawned").Count, 0, "no auto motes when motes are listed-only");
            AssertEq(EventsNamed(lines, "shade_spawned").Count, 2, "shade schedule intact");

            // Capacity ceiling: 20 starters + one spawn per 240 ticks, so 30 motes cap reached
            // after 10 spawns (tick 2400); a run to 730 sits at 23 — checked above.
        }

        private static void T13_InputSemantics()
        {
            const string sc = "{\"player\":{\"carried\":1},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5]}]}";

            // Defaults before the first frame: standing still. A frame only changes the fields
            // it lists: the move set on tick 1 persists through the button-only frame on tick 2.
            const string inp = "{\"frames\":[{\"tick\":1,\"move\":[1,0]},{\"tick\":2,\"interact\":true},{\"tick\":4,\"interact\":false}]}";
            List<JsonValue> lines = Run(41, sc, inp, 5, 1);
            AssertNear(Axis(SnapAt(lines, 1).Get("player").Get("pos"), 0), 0.0, 1e-9, "no motion before the first frame");
            AssertNear(Axis(SnapAt(lines, 2).Get("player").Get("pos"), 0), 0.1, 1e-9, "move applied during step 1");
            AssertNear(Axis(SnapAt(lines, 3).Get("player").Get("pos"), 0), 0.2, 1e-9, "move persists across the button frame");
            List<JsonValue> dep = EventsNamed(lines, "deposit");
            AssertEq(dep.Count, 1, "one deposit for the button edge on tick 2");
            AssertEq(dep[0].Get("tick").I, 3, "button at tick 2 -> event at tick 3");
            AssertEq(T(dep[0].Get("data").Get("lantern_id")), "L1", "the only unlit lantern");
            AssertEq(dep[0].Get("data").Get("amount").I, 1, "min(carried 1, missing 10) = 1");

            // Buttons fire on the false -> true edge only: jump true at ticks 0 and 1 jumps once
            // (one flight, one landing — back on the ground by tick 60, long after the tick-42 landing).
            lines = Run(41, sc, "{\"frames\":[{\"tick\":0,\"jump\":true},{\"tick\":1,\"jump\":true}]}", 60, 30);
            AssertNear(Axis(SnapAt(lines, 60).Get("player").Get("pos"), 1), 0.0, 1e-9, "one jump, back on the ground");
            AssertNear(Axis(SnapAt(lines, 60).Get("player").Get("pos"), 0), 0.0, 1e-9, "no horizontal drift, nothing else happened");

            // No inputs at all -> the player stands still for the whole run.
            lines = Run(41, sc, null, 3, 3);
            AssertNear(Axis(SnapAt(lines, 3).Get("player").Get("pos"), 0), 0.0, 1e-9, "no inputs means no motion");

            // Strictly increasing frame ticks are enforced by the loader.
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":[{\"tick\":5,\"move\":[1,0]},{\"tick\":5,\"move\":[0,1]}]}", 10), "duplicate tick");
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":[{\"tick\":5,\"move\":[1,0]},{\"tick\":4,\"move\":[0,1]}]}", 10), "decreasing tick");

            // Frames beyond --ticks are validated but not run.
            lines = Run(41, sc, "{\"frames\":[{\"tick\":0,\"move\":[1,0]},{\"tick\":10,\"interact\":true}]}", 3, 3);
            AssertEq(OfType(lines, "snapshot").Count, 2, "snapshots at tick 0 and tick 3");
            AssertNear(Axis(SnapAt(lines, 3).Get("player").Get("pos"), 0), 0.3, 1e-9, "only the first frame ran");
            AssertEq(EventsNamed(lines, "deposit").Count, 0, "the far frame did not run");
        }

        private static void T14_MalformedInput()
        {
            // The runner maps loader errors (bad JSON, wrong types, out-of-range values,
            // unreadable files) to exit code 2; the loader reports them as FormatException.
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"motes\":[[0,0,5]]"), "unterminated scenario JSON");
            ThrowsFormat(() => Loader.ScenarioFromJson("{}not json{}"), "garbage scenario JSON");
            ThrowsFormat(() => Loader.InputsFromJson("not json", 10), "malformed inputs file");
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":[{\"tick\":0,\"move\":[2,0]}]}", 10), "move component out of [-1,1]");
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":[{\"tick\":0,\"jump\":\"yes\"}]}", 10), "button must be boolean");
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":[{\"tick\":\"x\"}]}", 10), "tick must be a number");
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":{}}", 10), "frames must be an array");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"lanterns\":[{\"id\":123}]}"), "lantern id must be a string");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"player\":{\"pos\":[0,0]}}"), "pos needs three numbers");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"motes\":[[0,0]]}"), "motes entries need three numbers");

            // Well-formed documents run clean; empty ones use game defaults
            // (snapshot at tick 0, snapshot at tick 5, then the mandatory end line).
            List<JsonValue> lines = Engine.Run(1, Loader.ScenarioFromJson("{}"), Loader.InputsFromJson("{\"frames\":[]}", 5), 5, 5);
            AssertEq(lines.Count, 4, "empty scenario + empty inputs is a valid run (night_start + 2 snapshots + end)");
        }

        private static void T15_OutputShape()
        {
            List<JsonValue> lines = Run(51, "{\"motes\":[[0,0,5],[10,0,10]],\"random_shades\":false,\"shades\":[{\"pos\":[8,1,0]}]}",
                "{\"frames\":[{\"tick\":0,\"move\":[0,1]},{\"tick\":10,\"pulse\":true}]}", 30, 10);

            AssertEq(OfType(lines, "snapshot").Count, 4, "snapshots at 0, 10, 20, 30");
            AssertEq(OfType(lines, "end").Count, 1, "one end line");
            AssertEq(OfType(lines, "event").Count, 1, "only the runner's night_start line is an event (v1.1: it precedes the tick 0 snapshot)");
            AssertEq(T(OfType(lines, "event")[0].Get("name")), "night_start", "the one event is night_start");
            AssertEq(OfType(lines, "event")[0].Get("data").Get("night").I, 1, "night_start names the Night");
            AssertEq(lines[1].Serialize(), SnapAt(lines, 0).Serialize(), "the second line is the tick 0 snapshot");
            AssertEq(lines[lines.Count - 1].Serialize(), OfType(lines, "end")[0].Serialize(), "the last line is the end line");

            foreach (JsonValue l in lines)
            {
                string ty = T(l.Get("type"));
                Assert(ty == "snapshot" || ty == "event" || ty == "end", "line types are snapshot/event/end, got " + ty);
                Assert(l.Serialize().IndexOf('\n') < 0, "one JSON object per line");
                if (ty == "snapshot")
                {
                    AssertEq(l.Get("sim_tick").I, l.Get("tick").I, "sim_tick == tick in Phase 1");
                    Assert(ty == "snapshot", "");
                    CheckDecimals(l.Get("player").Get("pos"), "player.pos");
                    foreach (JsonValue m in l.Get("motes").Arr_)
                    {
                        Assert(m.Has("id") && m.Has("pos"), "the motes array carries id and pos");
                        CheckDecimals(m.Get("pos"), "mote.pos");
                    }
                    foreach (JsonValue sh in l.Get("shades").Arr_) CheckDecimals(sh.Get("pos"), "shade.pos");
                    foreach (JsonValue ln in l.Get("lanterns").Arr_) CheckDecimals(ln.Get("pos"), "lantern.pos");
                    AssertEq(T(l.Get("state")), "playing", "state is playing mid-run");
                }
            }

            // End line shape on a full run.
            JsonValue end = OfType(lines, "end")[0];
            AssertEq(end.Get("ticks").I, 30, "a full run reports --ticks");
            AssertEq(T(end.Get("state")), "playing", "state field on the end line");
            Assert(end.Has("score"), "the end line carries the score");

            // v1.2: there is no early stop any more - lighting the last lantern does not end the
            // run. (Pinned block rewritten with reason: Ticket-03 removed the all_lit win, so the
            // run now uses all its ticks and snapshots keep coming.)
            List<JsonValue> shortRun = Run(51, "{\"player\":{\"carried\":10},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5]},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true},{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}]}",
                "{\"frames\":[{\"tick\":2,\"interact\":true}]}", 50, 5);
            end = OfType(shortRun, "end")[0];
            AssertEq(end.Get("ticks").I, 50, "the run uses every tick - no early stop");
            AssertEq(T(end.Get("state")), "playing", "the game is still playing at the end line");
            AssertEq(OfType(shortRun, "snapshot").Count, 11, "snapshots at 0, 5, ... 50");
            AssertEq(EventsNamed(shortRun, "lantern_lit").Count, 1, "the lighting still happened, mid-run");
            AssertEq(EventsNamed(shortRun, "game_over").Count, 0, "no game_over during the run");
        }

        private static void CheckDecimals(JsonValue pos, string what)
        {
            string s = pos.Serialize();
            // Arrays of coordinates serialize as [x.xxx,y.yyy,z.zzz] — every fractional part
            // has at most 3 digits.
            foreach (string token in s.Trim('[', ']').Split(','))
            {
                int dot = token.IndexOf('.');
                if (dot >= 0) Assert(token.Length - dot - 1 <= 3, $"{what} exceeds 3 decimals: {s}");
            }
        }

        private static void T16_NaturalIdOrder()
        {
            // Snapshot arrays come out in a stable natural id order (the runner's ids are
            // generated; scenario ids must sort naturally, not lexically).
            const string sc = "{\"random_motes\":false,\"motes\":[],\"shades\":[{\"id\":\"S10\",\"pos\":[10,1,0]},{\"id\":\"S2\",\"pos\":[20,1,0]},{\"id\":\"S1\",\"pos\":[30,1,0]}],\"lanterns\":[]}";
            List<JsonValue> lines = Run(61, sc, null, 1, 1);
            AssertIds(SnapAt(lines, 0).Get("shades"), "S1", "S2", "S10");

            // Auto-generated ids keep their natural birth order too (M1 < M2 < M10).
            lines = Run(61, "{\"motes\":[[1,0,0],[2,0,0],[3,0,0],[4,0,0],[5,0,0],[6,0,0],[7,0,0],[8,0,0],[9,0,0],[10,0,0],[11,0,0],[12,0,0]],\"random_shades\":false,\"shades\":[{\"pos\":[30,1,0]}],\"lanterns\":[]}", null, 1, 1);
            AssertIds(SnapAt(lines, 0).Get("motes"),
                "M1", "M2", "M3", "M4", "M5", "M6", "M7", "M8", "M9", "M10", "M11", "M12");

            // Pulse reports stunned ids in the same natural order.
            const string scP = "{\"player\":{\"carried\":9},\"random_motes\":false,\"motes\":[],\"shades\":[{\"id\":\"S10\",\"pos\":[4,1,0]},{\"id\":\"S2\",\"pos\":[3,1,2]},{\"id\":\"S1\",\"pos\":[0,1,3]}],\"lanterns\":[]}";
            lines = Run(61, scP, "{\"frames\":[{\"tick\":0,\"pulse\":true}]}", 1, 1);
            List<JsonValue> p = EventsNamed(lines, "pulse");
            AssertEq(p.Count, 1, "pulse fired");
            AssertIds(p[0].Get("data").Get("stunned_ids"), "S1", "S2", "S10");
        }

        // ---- v1.1 tests (Ticket-02 core) ---------------------------------------

        private static void T17_Dash()
        {
            // Ground dash: 14 m/s for 12 ticks, direction = movement direction, no invulnerability.
            List<JsonValue> lines = Run(71, "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"lanterns\":[]}",
                "{\"frames\":[{\"tick\":0,\"move\":[0,1],\"dash\":true}]}", 30, 10);
            List<JsonValue> ev = EventsNamed(lines, "dash");
            AssertEq(ev.Count, 1, "one dash event");
            AssertEq(ev[0].Get("tick").I, 1, "the dash event stamps the tick that completes the step");
            // Tick 0: input read, dash starts (step 1); 12 dash ticks end at tick 12... the run has no
            // snapshot between; check total displacement after the window: 12 * 14/60 m, then 6 m/s.
            double z = Axis(SnapAt(lines, 30).Get("player").Get("pos"), 2);
            double expected = 12 * 14.0 / 60.0 + 18 * 6.0 / 60.0;   // dash window, then plain run
            AssertNear(z, expected, 1e-9, "dash window then run speed");

            // Cooldown 90 ticks from the START of the first dash: a second dash pressed before that is ignored.
            lines = Run(72, "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"lanterns\":[]}",
                "{\"frames\":[{\"tick\":0,\"move\":[0,1],\"dash\":true}," +
                "{\"tick\":15,\"dash\":false}," +
                "{\"tick\":30,\"dash\":true}," +                          // mid-cooldown: ignored
                "{\"tick\":90,\"dash\":false}," +
                "{\"tick\":95,\"dash\":true}]}",                          // cooldown done: fires
                120, 10);
            AssertEq(EventsNamed(lines, "dash").Count, 2, "second dash only after the 90-tick cooldown");

            // Air dash keeps the altitude: the window covers snapshots 10 and 15, so both must show
            // the height reached when the dash started — and nothing after it.
            lines = Run(73, "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"lanterns\":[]}",
                "{\"frames\":[{\"tick\":0,\"move\":[0,1],\"jump\":true}," +
                "{\"tick\":4,\"jump\":false}," +
                "{\"tick\":5,\"dash\":true}]}",                            // mid-air dash
                15, 5);
            AssertEq(EventsNamed(lines, "dash").Count, 1, "air dash fires");
            double y = 0, vy = 0;                                          // replay the flight step by step
            for (int s = 0; s <= 9; s++)
            {
                if (s == 0) vy = 7.0;                                      // the jump kick, gravity still applies that step
                if (s >= 5) vy = 0; else vy += -20.0 / 60.0;               // from the dash tick on, the vertical velocity is frozen
                y += vy / 60.0;
            }
            AssertNear(Axis(SnapAt(lines, 10).Get("player").Get("pos"), 1), y, 1e-9, "altitude held during the air dash");
            AssertNear(Axis(SnapAt(lines, 15).Get("player").Get("pos"), 1), y, 1e-9, "still held at the last window tick");
        }

        private static void T18_Pause()
        {
            string sc = "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"lanterns\":[]}";
            // Rising edges only, like every other button: press to pause, press again to resume.
            List<JsonValue> lines = Run(81, sc,
                "{\"frames\":[{\"tick\":0,\"move\":[0,1],\"pause\":true},{\"tick\":7,\"pause\":false},{\"tick\":8,\"pause\":true}]}",
                15, 5);

            List<JsonValue> pev = EventsNamed(lines, "paused");
            List<JsonValue> rev = EventsNamed(lines, "resumed");
            AssertEq(pev.Count, 1, "paused event fired once");
            AssertEq(rev.Count, 1, "resumed event fired once");
            AssertEq(pev[0].Get("tick").I, 1, "paused stamps the tick the step completes");
            AssertEq(rev[0].Get("tick").I, 9, "resumed stamps the tick the step completes");

            AssertEq(SnapAt(lines, 5).Get("state").Text(""), "paused", "snapshot state during the pause");
            AssertEq(SnapAt(lines, 5).Get("sim_tick").I, 0, "sim clock frozen while the tick counter runs");
            AssertEq(SnapAt(lines, 5).Get("tick").I, 5, "tick counter keeps advancing");
            AssertNear(SnapAt(lines, 5).Get("time_left_s").Num(0), 240.0, 1e-9, "time left frozen during the pause (v1.2: Night 1 runs 240 s)");

            AssertEq(SnapAt(lines, 15).Get("state").Text(""), "playing", "playing again after resume");
            AssertEq(SnapAt(lines, 15).Get("sim_tick").I, 7, "seven steps ran after the resume (ticks 8..14)");
            // The paused steps stop before the movement step, so only seven live steps moved Mây.
            AssertNear(Axis(SnapAt(lines, 15).Get("player").Get("pos"), 2), 7 * 0.1, 1e-9, "movement only while unpaused");
        }

        private static void T19_ContractV11()
        {
            // The tuning keys, exactly as the ticket ladder spells them: v1.1's five (durations
            // updated by Ticket-03) plus the eight new v1.2 keys appended after them.
            JsonValue t = Tuning.Standard().ToContractJson();
            AssertNear(t.Get("dash_speed").Num(0), 14.0, 1e-9, "dash_speed");
            AssertNear(t.Get("dash_duration_s").Num(0), 0.2, 1e-9, "dash_duration_s");
            AssertNear(t.Get("dash_cooldown_s").Num(0), 1.5, 1e-9, "dash_cooldown_s");
            AssertEq(t.Get("night1_duration_s").I, 240, "night1_duration_s (v1.2)");
            AssertEq(t.Get("night2_duration_s").I, 270, "night2_duration_s (v1.2)");
            AssertEq(t.Get("night3_duration_s").I, 300, "night3_duration_s (v1.2)");
            AssertEq(t.Get("fuel_max").I, 100, "fuel_max");
            AssertNear(t.Get("fuel_decay_per_s").Num(0), 1.0, 1e-9, "fuel_decay_per_s");
            AssertEq(t.Get("fuel_per_mote").I, 10, "fuel_per_mote");
            AssertEq(t.Get("relight_cost").I, 5, "relight_cost");
            AssertNear(t.Get("win_ratio").Num(0), 0.6, 1e-9, "win_ratio");
            AssertEq(t.Get("drain_per_s").I, 5, "drain_per_s");
            AssertEq(t.Get("low_fuel_threshold").I, 30, "low_fuel_threshold");
            AssertEq(t.Get("capacity_max").I, 20, "capacity_max");
            // The v1.2 score table (Ticket-03 replaces it): the old keys keep their slots with
            // new values, the new bonus keys are appended.
            AssertEq(t.Get("score").Get("per_mote").I, 5, "score per_mote (v1.2)");
            AssertEq(t.Get("score").Get("per_lantern").I, 100, "score per_lantern");
            AssertEq(t.Get("score").Get("per_stun").I, 5, "score per_stun");
            AssertEq(t.Get("score").Get("per_second_left").I, 0, "score per_second_left: the time bonus is gone");
            AssertEq(t.Get("score").Get("per_flame_left").I, 50, "score per_flame_left");
            AssertEq(t.Get("score").Get("per_relight").I, 40, "score per_relight");
            AssertEq(t.Get("score").Get("per_stun_drain").I, 15, "score per_stun_drain");
            AssertEq(t.Get("score").Get("per_dawn_lantern").I, 50, "score per_dawn_lantern");
            AssertEq(t.Get("score").Get("per_dawn_fuel").I, 1, "score per_dawn_fuel");
            // The published contract file must carry exactly this tuning object - key order
            // included, numbers compared by value because the two sides format differently.
            string contractFile = System.IO.File.ReadAllText(
                System.IO.Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "game.contract.json"));
            Assert(SameTree(JsonValue.Parse(contractFile).Get("tuning"), t), "game.contract.json matches the core");

            // A scenario may set the new player counters; the snapshot reports them and the new fields.
            string sc = "{\"player\":{\"pos\":[0,0,0],\"carried\":0,\"flames\":3,\"invuln_ticks\":30,\"dash_cd_ticks\":20,\"pulse_cd_ticks\":40,\"charm_ticks\":50}," +
                "\"random_motes\":false,\"motes\":[[3,0,3]],\"random_shades\":false,\"shades\":[],\"lanterns\":[]," +
                "\"drifters\":[{\"id\":\"D1\",\"pos\":[10,3,0]}],\"charms\":[{\"id\":\"C1\",\"pos\":[5,0,5]}]}";
            List<JsonValue> lines = Run(91, sc, null, 1, 1);
            JsonValue p = SnapAt(lines, 0).Get("player");
            AssertEq(p.Get("invuln").I, 30, "invuln counter");
            AssertNear(p.Get("dash_cd_s").Num(0), 0.333, 1e-9, "dash_cd_s prints seconds");
            AssertNear(p.Get("pulse_cd_s").Num(0), 0.667, 1e-9, "pulse_cd_s prints seconds");
            AssertNear(p.Get("charm_s").Num(0), 0.833, 1e-9, "charm_s prints seconds");
            Assert(p.Has("vel") && p.Has("grounded"), "snapshot player carries vel and grounded");
            AssertIds(SnapAt(lines, 0).Get("drifters"), "D1");
            AssertIds(SnapAt(lines, 0).Get("charms"), "C1");
            AssertEq(SnapAt(lines, 0).Get("night").I, 1, "night field");
            Assert(B(SnapAt(lines, 0).Get("wind").Get("active")) == false, "no wind on Night 1");
            AssertNear(SnapAt(lines, 0).Get("wind").Get("dir_deg").Num(-1), 0.0, 1e-9, "wind dir printed");

            JsonValue m0 = SnapAt(lines, 0).Get("motes").Arr_[0];
            AssertEq(T(m0.Get("kind")), "random", "motes carry kind random/dropped");
            Assert(m0.Get("ttl_s") == null || m0.Get("ttl_s").Type.ToString() == "Null", "no ttl on a random mote");
            JsonValue c0 = SnapAt(lines, 0).Get("charms").Arr_[0];
            Assert(B(c0.Get("available")), "charm starts available");

            // Malformed v1.1 fields go down the exit-2 path.
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"charms\":[{\"id\":\"C1\",\"pos\":[0,0,0],\"available\":\"yes\"}]}"), "charm available");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"player\":{\"charm_ticks\":-5}}"), "negative charm_ticks");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"player\":{\"invuln_ticks\":1.5}}"), "fractional invuln_ticks");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"shades\":[{\"pos\":[0,1,0],\"state\":\"fly\"}]}"), "unknown shade state");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"drifters\":{}}"), "drifters must be an array");
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":[{\"tick\":0,\"dash\":\"yes\"}]}", 1), "dash button");
            ThrowsFormat(() => Loader.InputsFromJson("{\"frames\":[{\"tick\":0,\"pause\":\"yes\"}]}", 1), "pause button");
        }

        // ---- Phase 2: multi-night, pads, drifters, charm, wind ------------------

        private static void T20_MultiNight()
        {
            const string quiet = "{\"player\":{\"invuln_ticks\":4000},\"random_motes\":true,\"random_shades\":true,\"random_drifters\":true}";

            // Night 2: 330 s clock, shade interval 420, drifters from tick 1200, charm on the bridge.
            List<JsonValue> n2 = Run(21, quiet, null, 1210, 1210, 2);
            List<JsonValue> n2ev = OfType(n2, "event");
            AssertEq(T(n2ev[0].Get("name")), "night_start", "night_start leads the file");
            AssertEq(n2ev[0].Get("data").Get("night").I, 2, "night 2 announced");
            AssertNear(SnapAt(n2, 0).Get("time_left_s").Num(-1), 270.0, 1e-9, "night 2 clock (v1.2)");
            AssertIds(SnapAt(n2, 0).Get("charms"), "C1");
            AssertNear(Axis(SnapAt(n2, 0).Get("charms").Arr_[0].Get("pos"), 0), 35.0, 1e-9, "charm C1 sits on the bridge");
            // Shade schedule to tick 1210: 300, 300, 720, 1140 — the 420-tick interval, no drift before it.
            var n2sh = EventsNamed(n2, "shade_spawned");
            AssertEq(n2sh.Count, 4, "night 2 shade schedule: 300, 300, 720, 1140");
            AssertEq(n2sh[2].Get("tick").I, 720, "night 2 shade interval is 420 ticks");
            AssertEq(n2sh[3].Get("tick").I, 1140, "and the next at 1140");
            AssertEq(T(n2sh[2].Get("data").Get("id")), "S3", "ids follow birth order");
            var n2dr = EventsNamed(n2, "drifter_spawned");
            AssertEq(n2dr.Count, 1, "first drifter born at tick 1200");
            AssertEq(n2dr[0].Get("tick").I, 1200, "drifter schedule 1200, 2400, ...");
            AssertEq(EventsNamed(n2, "drifter_steal").Count, 0, "a drifter with empty hands does not chase or steal");

            // Night 3: 360 s clock, shade interval 360, two charms, wind channel open.
            List<JsonValue> n3 = Run(21, quiet, null, 1210, 1210, 3);
            AssertEq(OfType(n3, "event")[0].Get("data").Get("night").I, 3, "night 3 announced");
            AssertNear(SnapAt(n3, 0).Get("time_left_s").Num(-1), 300.0, 1e-9, "night 3 clock (v1.2)");
            var n3sh = EventsNamed(n3, "shade_spawned");
            AssertEq(n3sh.Count, 4, "night 3 shade schedule: 300, 300, 660, 1020");
            AssertEq(n3sh[2].Get("tick").I, 660, "night 3 shade interval is 360 ticks");
            AssertEq(EventsNamed(n3, "drifter_spawned").Count, 1, "drifters run on night 3 too");
            AssertIds(SnapAt(n3, 0).Get("charms"), "C1", "C2");

            // Night 1 is untouched: the 480-tick interval pinned in Phase 1 still holds.
            List<JsonValue> n1 = Run(21, quiet, null, 900, 900, 1);
            var n1sh = EventsNamed(n1, "shade_spawned");
            AssertEq(n1sh.Count, 3, "night 1 schedule: 300, 300, 780");
            AssertEq(n1sh[2].Get("tick").I, 780, "night 1 interval stays 480");
            AssertEq(EventsNamed(n1, "drifter_spawned").Count, 0, "no drifters on night 1");

            AssertEq(Serialize(Run(21, quiet, null, 1210, 1210, 3)), Serialize(Run(21, quiet, null, 1210, 1210, 3)), "per-night determinism holds");
        }

        private static void T21_JumpPads()
        {
            // Standing on the Night-2 pad at (20,0,18): the pad fires at the end of the tick,
            // and the launch velocity (vy 14 + 24 m/s toward island B's centre) owns the flight
            // until landing ~1.4 s later, on island B.
            const string sc = "{\"player\":{\"pos\":[20,0,18]},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]}";
            List<JsonValue> plain = Run(3, sc, null, 120, 120, 2);
            JsonValue p = SnapAt(plain, 120).Get("player");
            double bx = 70.0, bz = 0.0; // island B centre
            double d = Math.Sqrt(Math.Pow(Axis(p.Get("pos"), 0) - bx, 2) + Math.Pow(Axis(p.Get("pos"), 2) - bz, 2));
            Assert(B(p.Get("grounded")), "the flight ends with a landing well before tick 120");
            AssertNear(Axis(p.Get("pos"), 1), 0.0, 1e-9, "landing on island B's sand, Y = 0");
            Assert(d < 22.0, "landed on island B, not on the way");
            // Analytic: dir (50,-18)/53.141, 24 m/s horizontal, ~1.4 s flight → (51.61, 6.62).
            AssertNear(Axis(p.Get("pos"), 0), 51.61, 0.6, "heading toward island B's centre during flight");
            AssertNear(Axis(p.Get("pos"), 2), 6.62, 0.6, "heading toward island B's centre during flight");

            // Movement keys must NOT override the launch velocity mid-flight (dash still may).
            // The zeroing frame at tick 90 (landed ~85) stops the sticky key from adding a walk afterwards.
            List<JsonValue> steered = Run(3, sc, "{\"frames\":[{\"tick\":1,\"move\":[1,0]},{\"tick\":90,\"move\":[0,0]}]}", 120, 120, 2);
            AssertNear(Axis(SnapAt(steered, 120).Get("player").Get("pos"), 0), Axis(p.Get("pos"), 0), 0.7, "held keys do not steer a pad launch");
            AssertNear(Axis(SnapAt(steered, 120).Get("player").Get("pos"), 2), Axis(p.Get("pos"), 2), 0.7, "held keys do not steer a pad launch");

            // A dash mid-flight is allowed and does change where you land (+Z from the start facing).
            List<JsonValue> dashed = Run(3, sc, "{\"frames\":[{\"tick\":30,\"dash\":true}]}", 120, 120, 2);
            AssertEq(EventsNamed(dashed, "dash").Count, 1, "dash fires during the flight");
            Assert(Axis(SnapAt(dashed, 120).Get("player").Get("pos"), 2) > Axis(p.Get("pos"), 2) + 1.5, "the dash window pushes the landing down +Z");
        }

        private static void T22_Drifters()
        {
            // (a) chase → steal once → flee 240 ticks → gone. Carried 6 ≥ 5, drifter at 5 m.
            string scA = "{\"player\":{\"pos\":[0,0,0],\"carried\":6},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"charms\":[]," +
                "\"drifters\":[{\"id\":\"D1\",\"pos\":[0,0,5]}]}";
            List<JsonValue> a = Run(5, scA, null, 320, 160, 2);
            var steal = EventsNamed(a, "drifter_steal");
            AssertEq(steal.Count, 1, "the drifter steals exactly once");
            Assert(steal[0].Get("tick").I >= 25 && steal[0].Get("tick").I <= 45, "steal lands ~tick 33 (7 m/s closing from 5 m)");
            AssertEq(T(steal[0].Get("data").Get("drifter_id")), "D1", "steal names its drifter");
            AssertEq(steal[0].Get("data").Get("amount").I, 3, "steals min(3, carried)");
            AssertEq(steal[0].Get("data").Get("carried").I, 3, "reports the player's carried after the steal");
            var snapA = SnapAt(a, 160);
            AssertEq(T(snapA.Get("drifters").Arr_[0].Get("state")), "flee", "fleeing after the steal");
            AssertEq(snapA.Get("drifters").Arr_[0].Get("stolen").I, 3, "carries the loot in the snapshot");
            JsonValue goneA = SnapAt(a, 320).Get("drifters");
            AssertEq(goneA.Arr_.Count, 1, "the entry stays in the list, frozen, after vanishing");
            AssertEq(T(goneA.Arr_[0].Get("state")), "removed", "gone from the world by tick 320 (240 flee ticks)");
            AssertEq(goneA.Arr_[0].Get("stolen").I, 3, "the loot went with it");

            // (b) pulse a chasing drifter → flee, no steal, no loot.
            string scB = "{\"player\":{\"pos\":[0,0,0],\"carried\":8},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"charms\":[]," +
                "\"drifters\":[{\"id\":\"D1\",\"pos\":[0,0,5]}]}";
            List<JsonValue> b = Run(5, scB, "{\"frames\":[{\"tick\":10,\"pulse\":true}]}", 300, 300, 2);
            AssertEq(EventsNamed(b, "drifter_steal").Count, 0, "a pulsed drifter flees without stealing");
            AssertEq(T(EventsNamed(b, "pulse")[0].Get("name")), "pulse", "the pulse fired");
            JsonValue goneB = SnapAt(b, 300).Get("drifters");
            AssertEq(goneB.Arr_.Count, 1, "the entry stays in the list, frozen");
            AssertEq(T(goneB.Arr_[0].Get("state")), "removed", "it fled its 240 ticks and is gone");
            AssertEq(SnapAt(b, 300).Get("player").Get("carried").I, 5, "pulse motes spent, nothing stolen");

            // (c) steal, then pulse the fleeing looter → drops the loot as motes, vanishes 120 ticks on.
            string scC = "{\"player\":{\"pos\":[0,0,0],\"carried\":7},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"charms\":[]," +
                "\"drifters\":[{\"id\":\"D1\",\"pos\":[0,0,5]}]}";
            List<JsonValue> c = Run(5, scC, "{\"frames\":[{\"tick\":40,\"pulse\":true}]}", 200, 200, 2);
            AssertEq(EventsNamed(c, "drifter_steal").Count, 1, "the steal happens first");
            var drop = EventsNamed(c, "drifter_drop");
            AssertEq(drop.Count, 1, "the pulse makes it drop the loot");
            AssertEq(drop[0].Get("tick").I, 41, "pulse frame at tick 40 -> event tick 41");
            AssertEq(drop[0].Get("data").Get("amount").I, 3, "drops all of it");
            JsonValue snapC = SnapAt(c, 200);
            AssertEq(snapC.Get("drifters").Arr_.Count, 0, "vanishes 120 ticks after the drop");
            AssertEq(snapC.Get("motes_active").I, 3, "the dropped motes lie where they fell");
            AssertEq(T(snapC.Get("motes").Arr_[0].Get("kind")), "dropped", "kind is dropped");
            AssertEq(snapC.Get("player").Get("carried").I, 1, "7 - 3 stolen - 3 pulse; the drop is out of pickup reach");
        }

        private static void T23_Charm()
        {
            // C1 one metre ahead: picked on the first pickup step, charm_s counts down from 600 ticks.
            string sc = "{\"player\":{\"pos\":[0,0,0]},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"random_drifters\":false," +
                "\"charms\":[{\"id\":\"C1\",\"pos\":[0,0,1]}]}";

            // Movement at 7.2 m/s while charmed: 33 and 68 steps of walking after the pickup.
            List<JsonValue> run = Run(11, sc, "{\"frames\":[{\"tick\":2,\"move\":[0,1]}]}", 70, 35, 2);
            var picked = EventsNamed(run, "charm_picked");
            AssertEq(picked.Count, 1, "charm picked up on tick 1");
            AssertEq(picked[0].Get("tick").I, 1, "the pickup is an event");
            AssertEq(picked[0].Get("data").Get("charm_ticks").I, 600, "600 ticks of charm");
            AssertNear(Axis(SnapAt(run, 35).Get("player").Get("pos"), 2), 33 * 0.12, 1e-6, "7.2 m/s while charmed");
            AssertNear(Axis(SnapAt(run, 70).Get("player").Get("pos"), 2), 68 * 0.12, 1e-6, "still charmed at tick 70");
            AssertNear(SnapAt(run, 70).Get("player").Get("charm_s").Num(-1), 531.0 / 60.0, 1e-3, "charm_s counts down in the snapshot");

            // Dash has no cooldown while charmed: two dashes 20 ticks apart (90 would block).
            // Dash is edge-triggered, so the stream must release the button between presses.
            List<JsonValue> dashes = Run(11, sc, "{\"frames\":[{\"tick\":20,\"dash\":true},{\"tick\":35,\"dash\":false},{\"tick\":40,\"dash\":true}]}", 70, 70, 2);
            AssertEq(EventsNamed(dashes, "dash").Count, 2, "the second dash fires inside the cooldown window");

            // Full cycle: charm expires, respawns on its spot after 2700 ticks, picked again next tick.
            List<JsonValue> cycle = Run(11, sc, null, 2710, 2710, 2);
            var both = EventsNamed(cycle, "charm_picked");
            AssertEq(both.Count, 2, "the charm comes back and is picked again");
            AssertEq(both[1].Get("tick").I, 2702, "respawn at tick 2701, pickable from tick 2702");
            AssertEq(OfType(cycle, "end")[0].Get("ticks").I, 2710, "the run completed the full length");
        }

        private static void T24_Wind()
        {
            // Night 3 only. Gust window: the first 300 ticks of every 1800 (1..300, 1801..2100);
            // each gust draws its direction once at its start, from the seeded stream.
            string off = "{\"random_motes\":false,\"random_shades\":false,\"random_drifters\":false,\"wind_enabled\":false}";
            string on = "{\"random_motes\":false,\"random_shades\":false,\"random_drifters\":false,\"wind_enabled\":true}";

            List<JsonValue> a = Run(13, off, null, 2200, 200, 3);
            List<JsonValue> b = Run(13, on, null, 2200, 200, 3);

            Assert(B(SnapAt(a, 200).Get("wind").Get("active")) == false, "wind_enabled false means never");
            AssertNear(Axis(SnapAt(a, 200).Get("player").Get("pos"), 0), 0.0, 1e-9, "wind-off player never moves");

            // Inside gust 1: pushed 200 ticks at 2 m/s along the drawn direction.
            JsonValue p200 = SnapAt(b, 200).Get("player");
            Assert(B(SnapAt(b, 200).Get("wind").Get("active")), "gust 1 covers ticks 1..300");
            AssertNear(Math.Sqrt(p200.Get("pos").Vec(3)[0] * p200.Get("pos").Vec(3)[0] + p200.Get("pos").Vec(3)[2] * p200.Get("pos").Vec(3)[2]), 200 * (2.0 / 60.0), 2e-3, "200 gust ticks at 2 m/s of push");
            Assert(Math.Abs(NormalizeDeg(Math.Atan2(Axis(p200.Get("pos"), 2), Axis(p200.Get("pos"), 0)) * 180.0 / Math.PI) - SnapAt(b, 200).Get("wind").Get("dir_deg").Num(-1000.0)) < 1.5, "the drift follows the drawn gust direction");

            // Quiet in between: gust 1 ended at tick 300, the player has stood still since.
            JsonValue p1000 = SnapAt(b, 1000).Get("player");
            Assert(B(SnapAt(b, 1000).Get("wind").Get("active")) == false, "quiet between the gust windows");
            AssertNear(Math.Sqrt(p1000.Get("pos").Vec(3)[0] * p1000.Get("pos").Vec(3)[0] + p1000.Get("pos").Vec(3)[2] * p1000.Get("pos").Vec(3)[2]), 300 * (2.0 / 60.0), 2e-3, "the gust-1 push is permanent");
            AssertNear(SnapAt(b, 1000).Get("wind").Get("dir_deg").Num(-1), SnapAt(b, 200).Get("wind").Get("dir_deg").Num(-2), 1e-9, "dir_deg keeps the drawn angle");

            // Gust 2 draws its own direction; the leg from tick 1000 to 2000 follows it.
            JsonValue p2000 = SnapAt(b, 2000).Get("player");
            Assert(B(SnapAt(b, 2000).Get("wind").Get("active")), "gust 2 covers ticks 1801..2100");
            double dx = Axis(p2000.Get("pos"), 0) - Axis(p1000.Get("pos"), 0);
            double dz = Axis(p2000.Get("pos"), 2) - Axis(p1000.Get("pos"), 2);
            AssertNear(Math.Sqrt(dx * dx + dz * dz), 200 * (2.0 / 60.0), 2e-3, "200 more gust ticks along the second draw");
            double deg2 = NormalizeDeg(Math.Atan2(dz, dx) * 180.0 / Math.PI);
            Assert(Math.Abs(deg2 - SnapAt(b, 2000).Get("wind").Get("dir_deg").Num(-1000.0)) < 1.5, "the second leg follows the second draw");

            // After the gust the direction is remembered and the player stands still again.
            // The full gust-2 leg is 300 pushes (ticks 1801..2100): 10 m along the second draw.
            Assert(B(SnapAt(b, 2200).Get("wind").Get("active")) == false, "the gust ends by tick 2100");
            AssertNear(SnapAt(b, 2200).Get("wind").Get("dir_deg").Num(-1), SnapAt(b, 2000).Get("wind").Get("dir_deg").Num(-2), 1e-9, "dir_deg keeps the second angle");
            double dxEnd = Axis(SnapAt(b, 2200).Get("player").Get("pos"), 0) - Axis(p1000.Get("pos"), 0);
            double dzEnd = Axis(SnapAt(b, 2200).Get("player").Get("pos"), 2) - Axis(p1000.Get("pos"), 2);
            AssertNear(Math.Sqrt(dxEnd * dxEnd + dzEnd * dzEnd), 300 * (2.0 / 60.0), 2e-3, "the gust runs to tick 2100, then stops");
            Assert(Math.Abs(NormalizeDeg(Math.Atan2(dzEnd, dxEnd) * 180.0 / Math.PI) - SnapAt(b, 2200).Get("wind").Get("dir_deg").Num(-1000.0)) < 1.5, "the full leg follows the second draw");

            AssertEq(Serialize(Run(13, on, null, 2200, 200, 3)), Serialize(b), "the gust draws are deterministic per seed");
        }

        // ---- Phase 3: fuel, drain, new win rules, capacity (TICKET-03 / contract v1.2) --

        private static void T25_Fuel()
        {
            // Top-up at a lit lantern: used = min(carried, ceil(need/10)). Fuel 20.0 decayed one
            // tick (step 2 runs before Interact) -> need 48.0166/10 -> 9 motes, fuel caps at 100.0.
            List<JsonValue> lines = Run(61,
                "{\"player\":{\"carried\":10},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]," +
                "\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5],\"lit\":true,\"fuel\":20.0}]}",
                "{\"frames\":[{\"tick\":0,\"interact\":true}]}", 2, 1);
            List<JsonValue> rf = EventsNamed(lines, "refuel");
            AssertEq(rf.Count, 1, "one refuel for the press");
            AssertEq(rf[0].Get("tick").I, 1, "refuel stamps the step it runs in");
            AssertEq(T(rf[0].Get("data").Get("lantern_id")), "L1", "refuel lantern id");
            AssertEq(rf[0].Get("data").Get("amount").I, 9, "used = ceil((100-99.9834/... )/10) = 9 motes");
            Assert(SnapAt(lines, 2).Get("player").Get("carried") != null, "player ref present");
            AssertEq(SnapAt(lines, 2).Get("player").Get("carried").I, 1, "10 - 9 motes");
            AssertNear(N(SnapAt(lines, 2).Get("lanterns").Arr_[0].Get("fuel")), 100.0, 1e-9, "top-up caps at fuel_max");
            AssertEq(SnapAt(lines, 2).Get("score").I, 45, "each topped mote scores +5");

            // Relight: a lantern that has ever been lit asks only for 5 motes and scores +40.
            lines = Run(61,
                "{\"player\":{\"carried\":5},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]," +
                "\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5],\"ever_lit\":true}]}",
                "{\"frames\":[{\"tick\":0,\"interact\":true}]}", 2, 1);
            List<JsonValue> dep = EventsNamed(lines, "deposit");
            AssertEq(dep.Count, 1, "one deposit event for the press");
            AssertEq(dep[0].Get("data").Get("deposited").I, 5, "all five motes go in (relight cost 5)");
            List<JsonValue> lit = EventsNamed(lines, "lantern_lit");
            AssertEq(lit.Count, 1, "the lantern lights up");
            AssertEq(lit[0].Get("tick").I, 1, "lighting stamp");
            Assert(B(lit[0].Get("data").Get("relight")), "lantern_lit carries relight: true");
            JsonValue l1 = SnapAt(lines, 2).Get("lanterns").Arr_[0];
            Assert(B(l1.Get("lit")), "lit after relight");
            AssertEq(l1.Get("fuel").Num(0) == 100.0 ? 1 : 0, 1, "relit lantern starts at full fuel");
            AssertEq(l1.Get("relit_count").I, 1, "relit_count counts relights");
            AssertEq(SnapAt(lines, 2).Get("player").Get("carried").I, 0, "carried spent");
            AssertEq(SnapAt(lines, 2).Get("score").I, 25 + 40, "5 motes x 5 + 40 relight");
            AssertEq(SnapAt(lines, 2).Get("player").Get("capacity").I, 12, "capacity grew with the successful relight");

            // A lantern topped to the cap on a later step: one mote lifts the tick of decay and
            // pays exactly one +5; a truly full lantern would not be selectable at all.
            lines = Run(61,
                "{\"player\":{\"carried\":1},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]," +
                "\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5],\"lit\":true}]}",
                "{\"frames\":[{\"tick\":0,\"interact\":true}]}", 2, 1);
            rf = EventsNamed(lines, "refuel");
            AssertEq(rf.Count, 1, "one mote tops up a lantern a tick of decay short");
            AssertEq(rf[0].Get("data").Get("amount").I, 1, "used = 1");
            AssertEq(SnapAt(lines, 2).Get("score").I, 5, "+5 for the mote");
            AssertEq(SnapAt(lines, 2).Get("player").Get("carried").I, 0, "carried spent");
        }

        private static void T26_Drain()
        {
            // A wander shade walks to the nearest lantern under the threshold (it may cross the
            // 6 m ring - the low-fuel lantern has lost its safe zone), enters drain within 2 m,
            // stands and sucks 5 fuel/s; a fuel_drain line lands every 60 ticks; the lantern
            // dies at 0 and the event order is drain-lines-then-lantern_out.
            List<JsonValue> lines = Run(63,
                "{\"random_motes\":false,\"motes\":[],\"random_shades\":false," +
                "\"shades\":[{\"id\":\"S1\",\"pos\":[34,0,0],\"state\":\"wander\"}]," +
                "\"lanterns\":[{\"id\":\"L1\",\"pos\":[30,0,0],\"lit\":true,\"fuel\":29.0}]}",
                null, 360, 30);
            AssertNear(N(SnapAt(lines, 0).Get("lanterns").Arr_[0].Get("fuel")), 29.0, 1e-9, "scenario fuel");
            AssertEq(T(SnapAt(lines, 30).Get("shades").Arr_[0].Get("state")), "wander", "still walking at snapshot 30");
            AssertNear(Axis(SnapAt(lines, 30).Get("shades").Arr_[0].Get("pos"), 0), 33.0, 2e-3,
                "one metre of pinned half-speed wander, inside the ring the low-fuel lantern lost");
            AssertEq(T(SnapAt(lines, 60).Get("shades").Arr_[0].Get("state")), "wander", "the 2 m line is reached during step 60, after snapshot 60");
            AssertNear(Axis(SnapAt(lines, 60).Get("shades").Arr_[0].Get("pos"), 0), 32.0, 2e-3, "the entry distance is measured before the step moves");
            AssertEq(T(SnapAt(lines, 90).Get("shades").Arr_[0].Get("state")), "drain", "drain from the step it stood under the lantern");
            AssertNear(Axis(SnapAt(lines, 90).Get("shades").Arr_[0].Get("pos"), 0), 32.0, 2e-3, "drain stands, no movement");
            List<JsonValue> fd = EventsNamed(lines, "fuel_drain");
            AssertEq(fd.Count, 4, "one line per 60 ticks after the first sip at step 61");
            long[] fdTicks = { 120, 180, 240, 300 };
            double[] fdFuel = { 22.1, 16.1, 10.1, 4.1 };   // the beat prints the tank at the top of the minute, entry at step 60
            for (int i = 0; i < 4; i++)
            {
                AssertEq(fd[i].Get("tick").I, fdTicks[i], "fuel_drain stamp");
                AssertEq(T(fd[i].Get("data").Get("lantern_id")), "L1", "fuel_drain lantern id");
                AssertEq(T(fd[i].Get("data").Get("shade_id")), "S1", "fuel_drain shade id");
                AssertEq(Math.Round(N(fd[i].Get("data").Get("fuel")), 1) == fdFuel[i] ? 1 : 0, 1, "fuel_drain fuel");
            }
            List<JsonValue> outev = EventsNamed(lines, "lantern_out");
            AssertEq(outev.Count, 1, "fuel 0 -> the lantern goes out");
            AssertEq(outev[0].Get("tick").I, 341, "the tank runs dry during step 340, the event lands on 341");
            AssertEq(T(outev[0].Get("data").Get("lantern_id")), "L1", "lantern_out lantern id");
            JsonValue dead = SnapAt(lines, 360).Get("lanterns").Arr_[0];
            Assert(B(dead.Get("lit")) == false, "out stays out");
            AssertEq(Math.Round(N(dead.Get("fuel")), 1) == 0.0 ? 1 : 0, 1, "fuel prints as 0.0 when out");
            Assert(B(dead.Get("ever_lit")), "the lantern remembers it was lit");
            AssertEq(dead.Get("deposited").I, 0, "deposited resets to 0");
            AssertEq(T(SnapAt(lines, 360).Get("shades").Arr_[0].Get("state")), "wander",
                "the shade goes back to wander the moment the lantern dies");

            // Multiple shades stack on the same lantern: two drain lines land on one stamp.
            lines = Run(63,
                "{\"random_motes\":false,\"motes\":[],\"random_shades\":false," +
                "\"shades\":[{\"id\":\"S1\",\"pos\":[31,0,0],\"state\":\"drain\",\"target_lantern_id\":\"L1\"}," +
                "{\"id\":\"S2\",\"pos\":[29,0,0],\"state\":\"drain\",\"target_lantern_id\":\"L1\"}]," +
                "\"lanterns\":[{\"id\":\"L1\",\"pos\":[30,0,0],\"lit\":true,\"fuel\":25.0}]}",
                null, 61, 61);
            fd = EventsNamed(lines, "fuel_drain");
            AssertEq(fd.Count, 2, "both shades report on the same 60-tick stamp");
            AssertEq(fd[0].Get("tick").I, 60, "stamp");
            AssertEq(fd[1].Get("tick").I, 60, "stamp");
            AssertEq(T(fd[0].Get("data").Get("shade_id")), "S1", "lines follow the snapshot id order");
            AssertEq(T(fd[1].Get("data").Get("shade_id")), "S2", "second draining shade");
            AssertEq(Math.Round(N(fd[0].Get("data").Get("fuel")), 1) == 14.3 ? 1 : 0, 1, "the beat prints the fuel at the top of the minute, before that minute's subtractions");
            AssertEq(Math.Round(N(fd[1].Get("data").Get("fuel")), 1) == 14.2 ? 1 : 0, 1, "the second line sits one shade-step below the first");

            // Top-up past the threshold pushes the shade that was drinking out to the zone edge
            // within 30 ticks, no damage; and a drained top-up emits no fuel_drain lines on a
            // run that ends before the next 60-tick beat.
            lines = Run(63,
                "{\"player\":{\"pos\":[0,0,-2],\"carried\":10},\"random_motes\":false,\"motes\":[],\"random_shades\":false," +
                "\"shades\":[{\"id\":\"S1\",\"pos\":[6,0,0],\"state\":\"drain\",\"target_lantern_id\":\"L1\"}]," +
                "\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,0],\"lit\":true,\"fuel\":29.0}]}",
                "{\"frames\":[{\"tick\":0,\"interact\":true}]}", 31, 31);
            List<JsonValue> rf3 = EventsNamed(lines, "refuel");
            AssertEq(rf3.Count, 1, "Mây tops the lantern up from the 2.0 m edge");
            AssertEq(rf3[0].Get("tick").I, 1, "the press runs in step 0");
            AssertEq(rf3[0].Get("data").Get("amount").I, 8, "used = ceil((100 - 99.9) / 10) = 8 motes at the beat-1 fuel");
            AssertEq(Math.Round(N(rf3[0].Get("data").Get("fuel")), 1) == 100.0 ? 1 : 0, 1, "fuel caps at 100.0");
            AssertEq(EventsNamed(lines, "fuel_drain").Count, 0, "the drink stops with the top-up");
            AssertEq(EventsNamed(lines, "player_hit").Count, 0, "the pushed shade never damages Mây");
            AssertEq(SnapAt(lines, 31).Get("score").I, 40, "eight topped motes score 8 x 5");
            AssertEq(SnapAt(lines, 31).Get("player").Get("carried").I, 2, "10 - 8 motes");
            AssertEq(OfType(lines, "end")[0].Get("ticks").I, 31, "the Night runs to the last tick");
        }

        private static void T27_WinRules()
        {
            // No early win: five lit lanterns at the start and the Night still runs its full length.
            List<JsonValue> lines = Run(67,
                "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]," +
                "\"lanterns\":[{\"id\":\"L1\",\"lit\":true},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true}," +
                "{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}]}", null, 120, 120);
            AssertEq(EventsNamed(lines, "game_over").Count, 0, "all lit no longer wins early");
            AssertEq(T(OfType(lines, "end")[0].Get("state")), "playing", "the run uses all its ticks");
            AssertEq(SnapAt(lines, 120).Get("lanterns").Arr_.Count, 5, "still five lanterns");
            foreach (JsonValue l in SnapAt(lines, 120).Get("lanterns").Arr_)
            {
                Assert(B(l.Get("lit")), "the lantern is still lit at snapshot 120");
                AssertEq(Math.Round(N(l.Get("fuel")), 1) == 98.0 ? 1 : 0, 1, "2.0 display fuel decayed over 120 ticks");
            }

            // Dawn below the threshold is a loss even with the clock run to the end: both lit
            // lanterns starve to 0 at tick 6000, so 0/5 < ceil(0.6*5) = 3 lit at dawn.
            lines = Run(67,
                "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]," +
                "\"lanterns\":[{\"id\":\"L1\",\"lit\":true},{\"id\":\"L2\",\"lit\":true}]}", null, 14401, 14401);
            List<JsonValue> outev = EventsNamed(lines, "lantern_out");
            AssertEq(outev.Count, 2, "both full lanterns die after 600 s of decay");
            AssertEq(outev[0].Get("tick").I, 6000, "L1 goes out on tick 6000");
            AssertEq(outev[1].Get("tick").I, 6000, "and L2 in the same step, ids in order");
            List<JsonValue> go = EventsNamed(lines, "game_over");
            AssertEq(go.Count, 1, "game over at dawn");
            AssertEq(T(go[0].Get("data").Get("reason")), "dawn", "reason dawn for below-threshold");
            AssertEq(T(OfType(lines, "end")[0].Get("state")), "lost", "the Night is lost");
            AssertEq(OfType(lines, "end")[0].Get("score").I, 0, "no dawn bonus on a loss");

            // Exactly at the threshold: three lit of the island's five (ceil(0.6*5) = 3) and alive
            // at time-up -> won. The scenario lights lanterns on the default island, it never
            // shrinks it (see block B). Each lit lantern decayed one tick: floor(299/60) = 4 points.
            lines = Run(67,
                "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]," +
                "\"lanterns\":[{\"id\":\"L1\",\"lit\":true,\"fuel\":5.0},{\"id\":\"L2\",\"lit\":true,\"fuel\":5.0}," +
                "{\"id\":\"L3\",\"lit\":true,\"fuel\":5.0}]," +
                "\"time_left_s\":0.016666666666666666}", null, 5, 1);
            JsonValue end = OfType(lines, "end")[0];
            AssertEq(T(end.Get("state")), "won", "threshold met at dawn -> won");
            AssertEq(end.Get("ticks").I, 1, "time crosses zero during step 0");
            AssertEq(T(EventsNamed(lines, "game_over")[0].Get("data").Get("reason")), "dawn_survived", "reason dawn_survived");
            AssertEq(end.Get("score").I, 3 * (50 + 4) + 3 * 50, "dawn bonus: +50+floor(fuel) per lantern, +50 per flame");

            // One lantern short at the same dawn (2 lit of five, 2 < 3) -> lost, no bonus.
            lines = Run(67,
                "{\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[]," +
                "\"lanterns\":[{\"id\":\"L1\",\"lit\":true,\"fuel\":5.0},{\"id\":\"L2\",\"lit\":true,\"fuel\":5.0}]," +
                "\"time_left_s\":0.016666666666666666}", null, 5, 1);
            AssertEq(T(OfType(lines, "end")[0].Get("state")), "lost", "one below the threshold at dawn -> lost");
            AssertEq(T(EventsNamed(lines, "game_over")[0].Get("data").Get("reason")), "dawn", "reason dawn below threshold");
            AssertEq(OfType(lines, "end")[0].Get("score").I, 0, "no dawn bonus on a loss");
        }

        private static void T28_CapacityScale()
        {
            StringBuilder motes = new StringBuilder("\"motes\":[");
            for (int i = 1; i <= 18; i++) { if (i > 1) motes.Append(','); motes.Append("[0,0," + i.ToString(CultureInfo.InvariantCulture) + "]"); }
            motes.Append(']');
            string walk = "{\"frames\":[{\"tick\":0,\"move\":[0,1]}]}";

            // successful_lights 3 -> capacity 16: sixteen pickups land, the 17th mote is refused.
            List<JsonValue> lines = Run(71,
                "{\"player\":{\"carried\":0,\"successful_lights\":3},\"random_motes\":false,\"random_shades\":false,\"shades\":[]," +
                motes + "}", walk, 182, 182);
            AssertEq(SnapAt(lines, 0).Get("player").Get("capacity").I, 16, "10 + 2*3");
            AssertEq(EventsNamed(lines, "mote_picked").Count, 16, "pickups stop at the scaled cap");
            AssertEq(SnapAt(lines, 182).Get("player").Get("carried").I, 16, "carried sits at the cap");
            AssertEq(SnapAt(lines, 182).Get("score").I, 0, "pickups score nothing on their own");

            // Plain baseline stays 10 (and the cap of 20): +2 per light only follows the counter.
            lines = Run(71,
                "{\"player\":{\"carried\":0},\"random_motes\":false,\"random_shades\":false,\"shades\":[]," + motes + "}",
                walk, 122, 122);
            AssertEq(SnapAt(lines, 0).Get("player").Get("capacity").I, 10, "baseline capacity");
            AssertEq(EventsNamed(lines, "mote_picked").Count, 10, "pickups stop at 10");
            lines = Run(71,
                "{\"player\":{\"carried\":0,\"successful_lights\":8},\"random_motes\":false,\"motes\":[],\"random_shades\":false,\"shades\":[],\"lanterns\":[]}",
                null, 1, 1);
            AssertEq(SnapAt(lines, 0).Get("player").Get("capacity").I, 20, "10 + 16 is capped at capacity_max");
        }

        private static void T29_FieldsV12()
        {
            // The v1.2 scenario fields round-trip through the snapshot.
            List<JsonValue> lines = Run(73,
                "{\"player\":{\"successful_lights\":2},\"random_motes\":false,\"motes\":[],\"random_shades\":false," +
                "\"shades\":[{\"id\":\"S1\",\"pos\":[31,0,0],\"state\":\"drain\",\"target_lantern_id\":\"L1\"}]," +
                "\"lanterns\":[{\"id\":\"L1\",\"pos\":[30,0,0],\"lit\":true,\"fuel\":25.0},{\"id\":\"L2\",\"pos\":[40,0,0],\"lit\":true}]}",
                null, 61, 61);
            List<JsonValue> fd = EventsNamed(lines, "fuel_drain");
            AssertEq(fd.Count, 1, "the scenario-placed drain shade reports once inside 61 ticks");
            AssertEq(fd[0].Get("tick").I, 60, "fuel_drain lands on the 60-tick beat");
            AssertEq(T(fd[0].Get("data").Get("lantern_id")), "L1", "scenario target_lantern_id wires the drain");
            AssertEq(T(fd[0].Get("data").Get("shade_id")), "S1", "shade id in the event");
            AssertEq(Math.Round(N(fd[0].Get("data").Get("fuel")), 1) == 19.1 ? 1 : 0, 1, "the beat prints the fuel at the top of the minute, before that minute's decay and drain");
            JsonValue snap = SnapAt(lines, 61);
            JsonValue s1 = snap.Get("shades").Arr_[0];
            AssertEq(T(s1.Get("state")), "drain", "snapshot prints the drain state");
            AssertNear(Axis(s1.Get("pos"), 0), 31.0, 2e-3, "a draining shade stands its ground");
            AssertEq(T(snap.Get("lanterns").Arr_[0].Get("id")), "L1", "snapshot lantern order");
            AssertEq(Math.Round(N(snap.Get("lanterns").Arr_[0].Get("fuel")), 1) == 19.0 ? 1 : 0, 1,
                "1500 units - 60 decay - 300 drain = 19.0 once the minute's subtractions have run");
            AssertEq(T(snap.Get("lanterns").Arr_[1].Get("id")), "L2", "second lantern");
            AssertEq(Math.Round(N(snap.Get("lanterns").Arr_[1].Get("fuel")), 1) == 99.0 ? 1 : 0, 1, "lit without fuel starts at 100 and decays");
            Assert(B(snap.Get("lanterns").Arr_[1].Get("ever_lit")) == false, "scenario-placed lit lanterns are not ever_lit");
            AssertEq(snap.Get("lanterns").Arr_[1].Get("relit_count").I, 0, "relit_count starts at 0");
            AssertEq(snap.Get("player").Get("capacity").I, 14, "scenario successful_lights lifts capacity");

            // Malformed v1.2 fields go down the exit-2 path.
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"lanterns\":[{\"id\":\"L1\",\"lit\":true,\"fuel\":-1}]}"), "negative fuel");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"lanterns\":[{\"id\":\"L1\",\"lit\":true,\"fuel\":\"hot\"}]}"), "fuel as a string");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"lanterns\":[{\"id\":\"L1\",\"ever_lit\":\"yes\"}]}"), "ever_lit as a string");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"shades\":[{\"pos\":[0,0,0],\"state\":\"fly\"}]}"), "unknown shade state stays rejected");
            Assert(Loader.ScenarioFromJson("{\"shades\":[{\"pos\":[0,0,0],\"state\":\"drain\",\"target_lantern_id\":42}]}").Shades[0].TargetLanternId == null,
                "a number in a text field falls back to 'unset' like the id fields do - the loader does not throw there");
            ThrowsFormat(() => Loader.ScenarioFromJson("{\"player\":{\"successful_lights\":1.5}}"), "fractional successful_lights");
        }

        private static void T30_Leaderboard()
        {
            // Name rules: the core set is [A-Za-z0-9_]{3,12}; Vietnamese diacritics are the bonus.
            Assert(Leaderboard.ValidName("Ngoc_Anh"), "plain ASCII name");
            Assert(Leaderboard.ValidName("NguyÔn_Thị"), "diacritics accepted as the bonus");
            Assert(Leaderboard.ValidName("abc"), "shortest allowed");
            Assert(Leaderboard.ValidName("abcdefghijkl"), "longest allowed");
            Assert(!Leaderboard.ValidName("ab"), "two letters rejected");
            Assert(!Leaderboard.ValidName("abcdefghijklm"), "thirteen letters rejected");
            Assert(!Leaderboard.ValidName("ngoc anh"), "space is outside the rule");
            Assert(!Leaderboard.ValidName("ngoc!anh"), "punctuation is outside the rule");
            Assert(!Leaderboard.ValidName(null), "null rejected");

            Leaderboard lb = new Leaderboard();
            Assert(lb.Add("Nguyen", 250, 2, "2026-03-01T10:00:00Z", 42), "valid add");
            Assert(lb.Add("Tran", 250, 3, "2026-03-01T09:00:00Z", 7), "valid add");      // achieved earlier than Nguyen
            Assert(lb.Add("LeNg", 300, 1, "2026-03-01T11:00:00Z", 8), "valid add");
            Assert(!lb.Add("xy", 999, 0, "2026-03-01T12:00:00Z", 9), "invalid name returns false");
            AssertEq(lb.Entries.Count, 3, "a rejected name leaves no entry");

            AssertEq(T(lb.ToJson().Get("entries").Arr_[0].Get("name")), "LeNg", "highest score on top");
            AssertEq(T(lb.ToJson().Get("entries").Arr_[1].Get("name")), "Tran", "tied scores: the earlier achieved_at stands above");
            AssertEq(T(lb.ToJson().Get("entries").Arr_[2].Get("name")), "Nguyen", "...then the later one");
            AssertEq(T(lb.ToJson().Get("entries").Arr_[0].Get("achieved_at")), "2026-03-01T11:00:00Z", "achieved_at round-trips");
            AssertEq(lb.ToJson().Get("entries").Arr_[0].Get("nights_won").I, 1, "nights_won round-trips");
            AssertEq(lb.ToJson().Get("entries").Arr_[0].Get("seed").I, 8, "seed round-trips");
            try { lb.Add("Range", 1, 4, "x", 0); throw new Exception("nights_won 4 must throw"); }
            catch (ArgumentOutOfRangeException) { }

            // Top 10: eleven entries, the weakest never reaches the board.
            Leaderboard big = new Leaderboard();
            for (int i = 1; i <= 11; i++)
                big.Add("player" + i, i * 10, 0, "2026-03-02T00:00:0" + i + "Z", i);
            AssertEq(big.Entries.Count, Leaderboard.MaxEntries, "the board holds ten");
            AssertEq(T(big.ToJson().Get("entries").Arr_[0].Get("name")), "player11", "top of the big board");
            AssertEq(T(big.ToJson().Get("entries").Arr_[9].Get("name")), "player2", "the eleventh-weakest is the last line");

            // The export document survives a parse round-trip in the same order.
            Leaderboard again = Leaderboard.FromJson(JsonValue.Parse(big.ToJson().Serialize()));
            AssertEq(again.Entries.Count, 10, "round-trip count");
            for (int i = 0; i < 10; i++)
                AssertEq(T(again.ToJson().Get("entries").Arr_[i].Get("name")), T(big.ToJson().Get("entries").Arr_[i].Get("name")), "round-trip order at " + i);
        }

        private static void T31_SoakInputs()
        {
            // The generator is deterministic: two calls with the same seed are field for field equal.
            List<InputFrame> a = SoakInputs.Generate(8, 500);
            List<InputFrame> b = SoakInputs.Generate(8, 500);
            AssertEq(a.Count, 500, "one frame per tick");
            for (int k = 0; k < 500; k++)
            {
                Assert(a[k].MoveX == b[k].MoveX && a[k].MoveZ == b[k].MoveZ, "move stream equal at tick " + k);
                Assert(a[k].Jump == b[k].Jump && a[k].Dash == b[k].Dash && a[k].Pulse == b[k].Pulse && a[k].Interact == b[k].Interact,
                    "button stream equal at tick " + k);
            }

            // Directions hold between the 30-tick re-rolls (the spec's "choose again every 30 ticks").
            Assert(a[0].MoveX == a[29].MoveX && a[0].MoveZ == a[29].MoveZ, "the direction is held between re-rolls");

            // Another seed feeds another stream, so the ten soak Nights do not play the same game.
            List<InputFrame> c = SoakInputs.Generate(9, 500);
            bool same = true;
            for (int k = 0; k < 500; k++)
                if (a[k].MoveX != c[k].MoveX || a[k].Jump != c[k].Jump) { same = false; break; }
            Assert(!same, "different seeds, different streams");

            AssertEq(SoakInputs.NightTicks(1), 14400, "Night 1 is 240 s of ticks");
            AssertEq(SoakInputs.NightTicks(2), 16200, "Night 2 is 270 s of ticks");
            AssertEq(SoakInputs.NightTicks(3), 18000, "Night 3 is 300 s of ticks");

            // And a whole Night driven by the stream is byte-identical across runs.
            string one = SerializeAll(Engine.Run(8, null, SoakInputs.Generate(8, 600), 600, 100000, 1));
            string two = SerializeAll(Engine.Run(8, null, SoakInputs.Generate(8, 600), 600, 100000, 1));
            Assert(one == two, "the seeded soak stream is deterministic end to end");
        }

        private static string SerializeAll(List<JsonValue> lines)
        {
            var sb = new System.Text.StringBuilder();
            foreach (JsonValue l in lines) sb.Append(l.Serialize()).Append('\n');
            return sb.ToString();
        }

        private static double NormalizeDeg(double deg)
        {
            deg %= 360.0;
            return deg < 0 ? deg + 360.0 : deg;
        }
    }
}
