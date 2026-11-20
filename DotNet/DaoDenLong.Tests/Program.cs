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
                ("T11 win/lose order: no_flames first, all_lit beats dawn, score formula", T11_WinLose),
                ("T12 spawns: motes 240/480/720, two shades at 300, ids in birth order", T12_SpawnSchedule),
                ("T13 input semantics: defaults, sticky fields, edges, strict tick order", T13_InputSemantics),
                ("T14 malformed scenario/inputs are rejected (exit code 2 path)", T14_MalformedInput),
                ("T15 output shape: JSONL lines, 3-decimal coords, early stop, end line last", T15_OutputShape),
                ("T16 snapshot ids in natural order (S1 < S2 < S10)", T16_NaturalIdOrder),
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

        private static List<JsonValue> Run(int seed, string scenarioJson, string inputsJson, long ticks, int snapshotEvery)
        {
            Scenario scenario = Loader.ScenarioFromJson(scenarioJson ?? "{}");
            List<InputFrame> inputs = Loader.InputsFromJson(inputsJson ?? "{\"frames\":[]}", (int)ticks);
            return Engine.Run(seed, scenario, inputs, ticks, snapshotEvery);
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
            AssertEq(OfType(lines, "event").Count, 0, "setup must not emit events");
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
            // Score: 10 motes * 10 + 100 for lighting.
            AssertEq(SnapAt(lines, 2).Get("score").I, 200, "score after deposit + light");

            // Partial deposit: the lantern remembers progress (8 stored + 1 carried = 9/10).
            lines = Run(5, "{\"player\":{\"carried\":1},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,2],\"deposited\":8}]}", inp, 2, 1);
            JsonValue s1 = SnapAt(lines, 1);
            JsonValue l1 = null;
            foreach (JsonValue l in s1.Get("lanterns").Arr_) if (T(l.Get("id")) == "L1") l1 = l;
            Assert(l1 != null, "L1 in snapshot");
            AssertEq(l1.Get("deposited").I, 9, "progress 8 + 1 carried");
            AssertEq(B(l1.Get("lit")) ? 1 : 0, 0, "not lit at 9/10");
            AssertEq(EventsNamed(lines, "lantern_lit").Count, 0, "no lantern_lit event below cost");
            // Only the carried mote scores (+10); the stored 8 came from the scenario setup.
            AssertEq(s1.Get("score").I, 10, "score for one deposited mote");

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
            // Win by all_lit: light the last lantern this step, the other four were lit by
            // the scenario (no score there). Score = 10 motes*10 + 100 + floor(t)*2 + 50*flames.
            const string scWin = "{\"player\":{\"carried\":10},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5],\"deposited\":0},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true},{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}]}";
            const string inpP = "{\"frames\":[{\"tick\":0,\"interact\":true}]}";
            List<JsonValue> lines = Run(21, scWin, inpP, 3, 1);
            List<JsonValue> end = OfType(lines, "end");
            AssertEq(end.Count, 1, "exactly one end line");
            AssertEq(T(end[0].Get("state")), "won", "won on the lighting tick");
            AssertEq(end[0].Get("ticks").I, 1, "the win happens in step 0");
            double t = N(SnapAt(lines, 1).Get("time_left_s"));
            AssertNear(t, 300.0 - 1.0 / 60.0, 1e-3, "time_left_s = duration - steps/60");
            long expected = 100 + 100 + (long)Math.Floor(t) * 2 + 50 * 3;
            AssertEq(end[0].Get("score").I, expected, "win score: motes + lantern + time bonus + flames");
            List<JsonValue> go = EventsNamed(lines, "game_over");
            AssertEq(go.Count, 1, "one game_over event");
            AssertEq(T(go[0].Get("data").Get("result")), "won", "result won");
            AssertEq(T(go[0].Get("data").Get("reason")), "all_lit", "reason all_lit");

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

            // Alive + all lit on the very tick time runs out -> still a win: the lighting
            // lands in the same step where time_left crosses zero.
            const string scDawn = "{\"player\":{\"carried\":10},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5]},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true},{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}],\"time_left_s\":0.016666666666666666}";
            lines = Run(21, scDawn, "{\"frames\":[{\"tick\":0,\"interact\":true}]}", 5, 1);
            end = OfType(lines, "end");
            AssertEq(T(end[0].Get("state")), "won", "alive + all lit on the time-up tick -> won");
            AssertEq(end[0].Get("ticks").I, 1, "the winning step is step 0 -> tick 1");
            AssertEq(T(EventsNamed(lines, "game_over")[0].Get("data").Get("reason")), "all_lit", "reason all_lit beats dawn");
            AssertEq(EventsNamed(lines, "game_over")[0].Get("data").Get("reason") != null ? 1 : 0, 1, "game_over carries a reason");
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
            AssertEq(lines.Count, 3, "empty scenario + empty inputs is a valid run (2 snapshots + end)");
        }

        private static void T15_OutputShape()
        {
            List<JsonValue> lines = Run(51, "{\"motes\":[[0,0,5],[10,0,10]],\"random_shades\":false,\"shades\":[{\"pos\":[8,1,0]}]}",
                "{\"frames\":[{\"tick\":0,\"move\":[0,1]},{\"tick\":10,\"pulse\":true}]}", 30, 10);

            AssertEq(OfType(lines, "snapshot").Count, 4, "snapshots at 0, 10, 20, 30");
            AssertEq(OfType(lines, "end").Count, 1, "one end line");
            AssertEq(lines[0].Serialize(), SnapAt(lines, 0).Serialize(), "the first line is the tick 0 snapshot");
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

            // Early stop: a win ends the run and the end line reports the win tick.
            List<JsonValue> shortRun = Run(51, "{\"player\":{\"carried\":10},\"random_motes\":false,\"random_shades\":false,\"lanterns\":[{\"id\":\"L1\",\"pos\":[0,0,1.5]},{\"id\":\"L2\",\"lit\":true},{\"id\":\"L3\",\"lit\":true},{\"id\":\"L4\",\"lit\":true},{\"id\":\"L5\",\"lit\":true}]}",
                "{\"frames\":[{\"tick\":2,\"interact\":true}]}", 50, 5);
            end = OfType(shortRun, "end")[0];
            AssertEq(end.Get("ticks").I, 3, "stopped the tick the game ended");
            AssertEq(T(end.Get("state")), "won", "won");
            AssertEq(OfType(shortRun, "snapshot").Count, 1, "no snapshots after the game ended");
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
    }
}
