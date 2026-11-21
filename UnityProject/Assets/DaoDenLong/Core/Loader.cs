using System;
using System.Collections.Generic;

namespace DaoDenLong.Core
{
    /// <summary>
    /// Reads the scenario and input files of the Test Contract (sections C and D) into the types the
    /// simulation uses. Every format problem throws FormatException; the runner turns that into exit
    /// code 2. Both the Unity editor and the .NET runner load files through this class, so a scenario
    /// that reproduces a bug in one reproduces it in the other.
    /// </summary>
    public static class Loader
    {
        public static Scenario ScenarioFromJson(string json)
        {
            JsonValue root = ParseObject(json, "scenario");

            Scenario sc = new Scenario();

            JsonValue p = root.Get("player");
            if (p != null)
            {
                if (p.Type != JsonValue.Kind.Obj) throw new FormatException("scenario: \"player\" must be an object");
                sc.HasPlayer = true;
                double[] pos = Vec3(p.Get("pos"), "player.pos");
                if (pos != null) sc.PlayerPos = new V3(pos[0], pos[1], pos[2]);
                if (p.Get("carried") != null)
                {
                    sc.Carried = (int)Need(p.Get("carried"), "player.carried").Num(-1);
                    if (sc.Carried < 0) throw new FormatException("scenario: player.carried must be >= 0");
                }
                if (p.Get("flames") != null)
                {
                    sc.Flames = (int)Need(p.Get("flames"), "player.flames").Num(-1);
                    if (sc.Flames < 0) throw new FormatException("scenario: player.flames must be >= 0");
                }
                // v1.1 setup counters, all integer ticks >= 0.
                if (p.Get("invuln_ticks") != null) { sc.HasPlayerTicks = true; sc.PlayerInvulnTicks = NonNegInt(p.Get("invuln_ticks"), "player.invuln_ticks"); }
                if (p.Get("dash_cd_ticks") != null) { sc.HasPlayerTicks = true; sc.PlayerDashCdTicks = NonNegInt(p.Get("dash_cd_ticks"), "player.dash_cd_ticks"); }
                if (p.Get("pulse_cd_ticks") != null) { sc.HasPlayerTicks = true; sc.PlayerPulseCdTicks = NonNegInt(p.Get("pulse_cd_ticks"), "player.pulse_cd_ticks"); }
                if (p.Get("charm_ticks") != null) { sc.HasPlayerTicks = true; sc.PlayerCharmTicks = NonNegInt(p.Get("charm_ticks"), "player.charm_ticks"); }
                if (p.Get("successful_lights") != null)   // v1.2 setup counter
                {
                    sc.HasPlayerSuccessfulLights = true;
                    sc.PlayerSuccessfulLights = NonNegInt(p.Get("successful_lights"), "player.successful_lights");
                }
            }

            if (root.Get("random_motes") != null)
            {
                RequireBool(root.Get("random_motes"), "random_motes");
                sc.RandomMotes = root.Get("random_motes").B;
            }
            if (root.Get("random_shades") != null)
            {
                RequireBool(root.Get("random_shades"), "random_shades");
                sc.RandomShades = root.Get("random_shades").B;
            }
            if (root.Get("random_drifters") != null)
            {
                RequireBool(root.Get("random_drifters"), "random_drifters");
                sc.HasRandomDrifters = true; sc.RandomDrifters = root.Get("random_drifters").B;
            }
            if (root.Get("wind_enabled") != null)
            {
                RequireBool(root.Get("wind_enabled"), "wind_enabled");
                sc.HasWindEnabled = true; sc.WindEnabled = root.Get("wind_enabled").B;
            }

            JsonValue motes = root.Get("motes");
            if (motes != null)
            {
                if (motes.Type != JsonValue.Kind.Arr) throw new FormatException("scenario: \"motes\" must be an array");
                sc.HasMotes = true;
                for (int i = 0; i < motes.Arr_.Count; i++)
                {
                    double[] v = Vec3(motes.Arr_[i], "motes[" + i + "]");
                    if (v == null) throw new FormatException("scenario: motes[" + i + "] must be [x, y, z]");
                    sc.Motes.Add(new V3(v[0], v[1], v[2]));
                }
            }

            JsonValue shades = root.Get("shades");
            if (shades != null)
            {
                if (shades.Type != JsonValue.Kind.Arr) throw new FormatException("scenario: \"shades\" must be an array");
                sc.HasShades = true;
                for (int i = 0; i < shades.Arr_.Count; i++)
                {
                    JsonValue s = shades.Arr_[i];
                    if (s.Type != JsonValue.Kind.Obj) throw new FormatException("scenario: shades[" + i + "] must be an object");
                    ShadeSpec sp = new ShadeSpec();
                    double[] v = Vec3(s.Get("pos"), "shades[" + i + "].pos");
                    if (v == null) throw new FormatException("scenario: shades[" + i + "].pos must be [x, y, z]");
                    sp.Pos = new V3(v[0], v[1], v[2]);
                    if (s.Get("id") != null) sp.Id = Need(s.Get("id"), "shades[" + i + "].id").Text(null);
                    if (s.Get("state") != null)
                    {
                        string st = Need(s.Get("state"), "shades[" + i + "].state").Text(null);
                        if (st != "spawning" && st != "wander" && st != "chase" && st != "recoil" && st != "stunned" && st != "drain")   // v1.2 adds drain
                            throw new FormatException("scenario: shades[" + i + "].state must be one of spawning/wander/chase/recoil/stunned/drain");
                        sp.HasState = true;
                        sp.State = st == "spawning" ? ShadeState.spawning
                             : st == "wander" ? ShadeState.wander
                             : st == "chase" ? ShadeState.chase
                             : st == "recoil" ? ShadeState.recoil
                             : st == "drain" ? ShadeState.drain : ShadeState.stunned;
                    }
                    if (s.Get("state_ticks_left") != null)
                    {
                        sp.HasStateTicksLeft = true;
                        sp.StateTicksLeft = NonNegInt(s.Get("state_ticks_left"), "shades[" + i + "].state_ticks_left");
                    }
                    if (s.Get("target_lantern_id") != null)   // v1.2
                        sp.TargetLanternId = Need(s.Get("target_lantern_id"), "shades[" + i + "].target_lantern_id").Text(null);
                    sc.Shades.Add(sp);
                }
            }

            JsonValue lanterns = root.Get("lanterns");
            if (lanterns != null)
            {
                if (lanterns.Type != JsonValue.Kind.Arr) throw new FormatException("scenario: \"lanterns\" must be an array");
                for (int i = 0; i < lanterns.Arr_.Count; i++)
                {
                    JsonValue l = lanterns.Arr_[i];
                    if (l.Type != JsonValue.Kind.Obj) throw new FormatException("scenario: lanterns[" + i + "] must be an object");
                    JsonValue id = l.Get("id");
                    if (id == null || id.Type != JsonValue.Kind.Str) throw new FormatException("scenario: lanterns[" + i + "] needs an \"id\"");
                    LanternUpdate lu = new LanternUpdate { Id = id.S };
                    double[] v = Vec3(l.Get("pos"), "lanterns[" + i + "].pos");
                    if (v != null) { lu.HasPos = true; lu.Pos = new V3(v[0], v[1], v[2]); }
                    if (l.Get("lit") != null) { RequireBool(l.Get("lit"), "lanterns[" + i + "].lit"); lu.HasLit = true; lu.Lit = l.Get("lit").B; }
                    if (l.Get("deposited") != null)
                    {
                        lu.HasDeposited = true;
                        lu.Deposited = (int)Need(l.Get("deposited"), "lanterns[" + i + "].deposited").Num(-1);
                        if (lu.Deposited < 0) throw new FormatException("scenario: deposited must be >= 0");
                    }
                    if (l.Get("fuel") != null)    // v1.2: fuel points, a real number (1 decimal is what we print)
                    {
                        double f = Need(l.Get("fuel"), "lanterns[" + i + "].fuel").Num(-1);
                        if (f < 0) throw new FormatException("scenario: fuel must be >= 0");
                        lu.HasFuel = true; lu.Fuel = f;
                    }
                    if (l.Get("ever_lit") != null)    // v1.2
                    {
                        RequireBool(l.Get("ever_lit"), "lanterns[" + i + "].ever_lit");
                        lu.HasEverLit = true; lu.EverLit = l.Get("ever_lit").B;
                    }
                    sc.Lanterns.Add(lu);
                }
            }

            JsonValue drifters = root.Get("drifters");
            if (drifters != null)
            {
                if (drifters.Type != JsonValue.Kind.Arr) throw new FormatException("scenario: \"drifters\" must be an array");
                sc.HasDrifters = true;
                for (int i = 0; i < drifters.Arr_.Count; i++)
                {
                    JsonValue d = drifters.Arr_[i];
                    if (d.Type != JsonValue.Kind.Obj) throw new FormatException("scenario: drifters[" + i + "] must be an object");
                    JsonValue id = d.Get("id");
                    if (id == null || id.Type != JsonValue.Kind.Str) throw new FormatException("scenario: drifters[" + i + "] needs an \"id\"");
                    DrifterSpec ds = new DrifterSpec { Id = id.S };
                    double[] v = Vec3(d.Get("pos"), "drifters[" + i + "].pos");
                    if (v == null) throw new FormatException("scenario: drifters[" + i + "].pos must be [x, y, z]");
                    ds.Pos = new V3(v[0], v[1], v[2]);
                    sc.Drifters.Add(ds);
                }
            }

            JsonValue charms = root.Get("charms");
            if (charms != null)
            {
                if (charms.Type != JsonValue.Kind.Arr) throw new FormatException("scenario: \"charms\" must be an array");
                sc.HasCharms = true;
                for (int i = 0; i < charms.Arr_.Count; i++)
                {
                    JsonValue c = charms.Arr_[i];
                    if (c.Type != JsonValue.Kind.Obj) throw new FormatException("scenario: charms[" + i + "] must be an object");
                    JsonValue id = c.Get("id");
                    if (id == null || id.Type != JsonValue.Kind.Str) throw new FormatException("scenario: charms[" + i + "] needs an \"id\"");
                    CharmSpec cs = new CharmSpec { Id = id.S };
                    double[] v = Vec3(c.Get("pos"), "charms[" + i + "].pos");
                    if (v == null) throw new FormatException("scenario: charms[" + i + "].pos must be [x, y, z]");
                    cs.Pos = new V3(v[0], v[1], v[2]);
                    if (c.Get("available") != null) { RequireBool(c.Get("available"), "charms[" + i + "].available"); cs.HasAvailable = true; cs.Available = c.Get("available").B; }
                    sc.Charms.Add(cs);
                }
            }

            if (root.Get("time_left_s") != null)
            {
                JsonValue t = Need(root.Get("time_left_s"), "time_left_s");
                if (t.Type != JsonValue.Kind.NumInt && t.Type != JsonValue.Kind.NumReal)
                    throw new FormatException("scenario: time_left_s must be a number");
                sc.TimeLeftS = t.Num(double.NaN);
            }

            return sc;
        }

        /// <summary>
        /// Read an input file into one frame per tick for ticks 0..tickCount-1. Contract section C:
        /// defaults before the first frame are move [0,0], cam_yaw_deg 0 and every button false, and a
        /// frame only changes the fields it contains. Frame ticks must strictly increase.
        /// </summary>
        public static List<InputFrame> InputsFromJson(string json, int tickCount)
        {
            JsonValue root = ParseObject(json, "inputs");
            JsonValue frames = root.Get("frames");
            if (frames != null && frames.Type != JsonValue.Kind.Arr)
                throw new FormatException("inputs: \"frames\" must be an array");

            List<InputFrame> outFrames = new List<InputFrame>();
            if (tickCount <= 0) return outFrames;

            double mx = 0, mz = 0, yaw = 0;
            bool jump = false, interact = false, pulse = false, dash = false, pause = false;
            int next = 0;
            long prevTick = -1;

            for (int t = 0; t < tickCount; t++)
            {
                while (frames != null && next < frames.Arr_.Count)
                {
                    JsonValue f = frames.Arr_[next];
                    if (f.Type != JsonValue.Kind.Obj) throw new FormatException("inputs: frames[" + next + "] must be an object");
                    JsonValue jt = f.Get("tick");
                    if (jt == null || (jt.Type != JsonValue.Kind.NumInt)) throw new FormatException("inputs: frames[" + next + "] needs an integer \"tick\"");
                    long ft = jt.I;
                    if (ft < prevTick) throw new FormatException("inputs: frame ticks must increase");
                    if (ft == prevTick) throw new FormatException("inputs: frame ticks must be strictly increasing");
                    if (ft != t) break;
                    prevTick = ft;

                    if (f.Get("move") != null)
                    {
                        double[] mv = f.Get("move").Vec(2);
                        if (mv == null) throw new FormatException("inputs: frames[" + next + "].move must be [x, z]");
                        if (mv[0] < -1 || mv[0] > 1 || mv[1] < -1 || mv[1] > 1)
                            throw new FormatException("inputs: move components must be inside [-1, 1]");
                        mx = mv[0]; mz = mv[1];
                    }
                    if (f.Get("cam_yaw_deg") != null)
                    {
                        JsonValue y = Need(f.Get("cam_yaw_deg"), "cam_yaw_deg");
                        if (y.Type != JsonValue.Kind.NumInt && y.Type != JsonValue.Kind.NumReal)
                            throw new FormatException("inputs: cam_yaw_deg must be a number");
                        yaw = y.Num(0);
                    }
                    if (f.Get("jump") != null) { RequireBool(f.Get("jump"), "jump"); jump = f.Get("jump").B; }
                    if (f.Get("interact") != null) { RequireBool(f.Get("interact"), "interact"); interact = f.Get("interact").B; }
                    if (f.Get("pulse") != null) { RequireBool(f.Get("pulse"), "pulse"); pulse = f.Get("pulse").B; }
                    if (f.Get("dash") != null) { RequireBool(f.Get("dash"), "dash"); dash = f.Get("dash").B; }
                    if (f.Get("pause") != null) { RequireBool(f.Get("pause"), "pause"); pause = f.Get("pause").B; }
                    next++;
                }

                outFrames.Add(new InputFrame
                {
                    Tick = t,
                    MoveX = mx, MoveZ = mz, CamYawDeg = yaw,
                    Jump = jump, Interact = interact, Pulse = pulse, Dash = dash, Pause = pause
                });
            }

            // Frames past the last tick are valid but never run; their ticks are still checked.
            for (int i = next; i < (frames == null ? 0 : frames.Arr_.Count); i++)
            {
                JsonValue f = frames.Arr_[i];
                if (f.Type != JsonValue.Kind.Obj) throw new FormatException("inputs: frames[" + i + "] must be an object");
                JsonValue jt = f.Get("tick");
                if (jt == null || jt.Type != JsonValue.Kind.NumInt) throw new FormatException("inputs: frames[" + i + "] needs an integer \"tick\"");
                if (jt.I <= prevTick) throw new FormatException("inputs: frame ticks must be strictly increasing");
                prevTick = jt.I;
            }

            return outFrames;
        }

        private static JsonValue ParseObject(string json, string what)
        {
            JsonValue root = JsonValue.Parse(json);
            if (root.Type != JsonValue.Kind.Obj) throw new FormatException(what + ": the file must contain a JSON object");
            return root;
        }

        private static JsonValue Need(JsonValue v, string what)
        {
            if (v == null) throw new FormatException(what + " is missing");
            return v;
        }

        private static void RequireBool(JsonValue v, string what)
        {
            if (v.Type != JsonValue.Kind.Bool) throw new FormatException(what + " must be true or false");
        }

        private static int NonNegInt(JsonValue v, string what)
        {
            if (v.Type != JsonValue.Kind.NumInt) throw new FormatException(what + " must be a whole number");
            if (v.I < 0) throw new FormatException(what + " must be >= 0");
            return (int)v.I;
        }

        private static double[] Vec3(JsonValue v, string what)
        {
            if (v == null) return null;
            double[] r = v.Vec(3);
            if (r == null) throw new FormatException(what + " must be [x, y, z]");
            return r;
        }
    }
}
