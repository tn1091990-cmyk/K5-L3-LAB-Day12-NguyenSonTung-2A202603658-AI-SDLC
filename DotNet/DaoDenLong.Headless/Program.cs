using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DaoDenLong.Core;

namespace DaoDenLong.Headless
{
    /// <summary>
    /// The Test Contract runner (section A/B): a Night of the game with no window and no GPU, writing
    /// JSONL. Exit 0 when it runs to the end, 2 when an input or scenario file is malformed, 1 for any
    /// other error with the reason on stderr. Same seed + scenario + inputs always produce the same file.
    /// v1.3 (TICKET-04) adds three modes, dispatched on the first argument: --bench, --soak and
    /// --export-leaderboard.
    /// </summary>
    public static class Program
    {
        private const int ExitOk = 0;
        private const int ExitError = 1;
        private const int ExitBadInput = 2;

        private static readonly Stopwatch ProcessStarted = Stopwatch.StartNew();   // "load_ms" starts here

        public static int Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] == "--bench") { BenchMode(args); return ExitOk; }
                if (args.Length > 0 && args[0] == "--soak") { SoakMode(args); return ExitOk; }
                if (args.Length > 0 && args[0] == "--export-leaderboard") { ExportLeaderboard(args); return ExitOk; }
                Run(args);
                return ExitOk;
            }
            catch (FormatException e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitBadInput;
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitBadInput;
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitBadInput;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitError;
            }
        }

        private static void Run(string[] args)
        {
            int seed = 1;
            int night = 1;
            long ticks = 0;
            int snapshotEvery = 10;
            string scenarioFile = null, inputsFile = null, outPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--seed": seed = ReadInt(args, ref i, "--seed"); break;
                    case "--night": night = ReadInt(args, ref i, "--night"); break;
                    case "--ticks": ticks = ReadInt(args, ref i, "--ticks"); break;
                    case "--snapshot-every": snapshotEvery = ReadInt(args, ref i, "--snapshot-every"); break;
                    case "--scenario": scenarioFile = ReadPath(args, ref i, "--scenario"); break;
                    case "--inputs": inputsFile = ReadPath(args, ref i, "--inputs"); break;
                    case "--out": outPath = ReadPath(args, ref i, "--out"); break;
                    default: throw new FormatException("unknown argument " + args[i]);
                }
            }

            if (string.IsNullOrEmpty(outPath)) throw new FormatException("--out is required");
            if (night < 1 || night > 3) throw new FormatException("--night must be 1, 2 or 3 (got " + night.ToString(CultureInfo.InvariantCulture) + ")");

            Scenario scenario = null;
            if (!string.IsNullOrEmpty(scenarioFile))
                scenario = Loader.ScenarioFromJson(File.ReadAllText(Path.GetFullPath(scenarioFile), Encoding.UTF8));

            List<InputFrame> inputs = null;
            if (!string.IsNullOrEmpty(inputsFile))
                inputs = Loader.InputsFromJson(File.ReadAllText(Path.GetFullPath(inputsFile), Encoding.UTF8), (int)ticks);

            List<JsonValue> lines = Engine.Run(seed, scenario, inputs, ticks, snapshotEvery, night);

            StringBuilder sb = new StringBuilder();
            foreach (JsonValue line in lines)
            {
                sb.Append(line.Serialize());
                sb.Append('\n');
            }
            File.WriteAllText(Path.GetFullPath(outPath), sb.ToString(), new UTF8Encoding(false));
        }

        // v1.3 (TICKET-04). Benchmark: the Night-1 scene with 200 wandering shades and 100 motes,
        // spawn / damage / pickup / fuel decay / end conditions switched off (Simulation.Bench);
        // movement and the normal wander rules run. The window is 1280x720 with vsync off in the
        // Unity scene (View/BenchMode.cs); this headless path steps the same core scene one tick per
        // frame on purpose, so the numbers below are the core's own cost, measured honestly.
        private static void BenchMode(string[] args)
        {
            int seed = 42;
            int durationS = 30;
            string outPath = null;
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--seed": seed = ReadInt(args, ref i, "--seed"); break;
                    case "--duration": durationS = ReadInt(args, ref i, "--duration"); break;
                    case "--out": outPath = ReadPath(args, ref i, "--out"); break;
                    default: throw new FormatException("unknown argument " + args[i]);
                }
            }
            if (string.IsNullOrEmpty(outPath)) throw new FormatException("--out is required");
            if (durationS < 1) throw new FormatException("--duration must be at least 1 (got " + durationS + ")");

            Simulation sim = new Simulation();
            sim.Bench = true;
            sim.Build(seed, 1);
            sim.ApplyScenario(BenchScene.BuildScenario(seed));
            long loadMs = ProcessStarted.ElapsedMilliseconds;    // process start -> first bench frame

            Stopwatch clock = Stopwatch.StartNew();
            InputFrame idle = new InputFrame();

            for (int warm = 0; warm < 3 * Simulation.TicksPerSecond; warm++) sim.Step(idle);   // 3 s warmup, not measured

            List<double> dts = new List<double>();
            long peakMemBytes = Environment.WorkingSet;
            Stopwatch frame = new Stopwatch();
            double warmS = clock.Elapsed.TotalSeconds;
            while (clock.Elapsed.TotalSeconds - warmS < durationS)
            {
                frame.Restart();
                sim.Step(idle);
                dts.Add(frame.Elapsed.TotalMilliseconds);
                long ws = Environment.WorkingSet;                 // the periodic RSS sample (spec)
                if (ws > peakMemBytes) peakMemBytes = ws;
            }

            int n = dts.Count;
            double sum = 0.0;
            foreach (double d in dts) sum += d;
            double avgFps = 1000.0 * n / sum;

            List<double> slowest = new List<double>(dts);
            slowest.Sort(delegate (double a, double b) { return b.CompareTo(a); });
            int k = (int)Math.Ceiling(n * 0.01);
            if (k < 1) k = 1;
            double slowMean = 0.0;
            for (int i = 0; i < k; i++) slowMean += slowest[i];
            double p1LowFps = 1000.0 / (slowMean / k);

            List<double> sorted = new List<double>(dts);
            sorted.Sort();                                        // p95, nearest-rank (documented)
            double p95 = sorted[(int)Math.Ceiling(0.95 * n) - 1];

            JsonValue j = JsonValue.Obj()
                .Set("contract_version", JsonValue.Str("1.3"))
                .Set("mode", JsonValue.Str("bench"))
                .Set("seed", JsonValue.Int(seed))
                .Set("duration_s", JsonValue.Int(durationS))
                .Set("window", JsonValue.Arr().Add(JsonValue.Int(1280)).Add(JsonValue.Int(720)))
                .Set("avg_fps", JsonValue.Real(avgFps, 1))
                .Set("p1_low_fps", JsonValue.Real(p1LowFps, 1))
                .Set("frame_ms_p95", JsonValue.Real(p95, 3))
                .Set("frames", JsonValue.Int(n))
                .Set("load_ms", JsonValue.Int(loadMs))
                .Set("peak_mem_mb", JsonValue.Int(peakMemBytes / 1048576))
                .Set("vsync_off", JsonValue.Bool(true))
                .Set("renderer", JsonValue.Str("headless .NET core (pure-C# simulation, no GPU) — the Unity GUI scene (View/BenchMode.cs) renders the same scene at 1280x720, vsync off, orbit camera 15 s/turn"))
                .Set("os", JsonValue.Str(RuntimeInformation.OSDescription));
            WriteJson(outPath, j);
        }

        // v1.3 (TICKET-04). Soak: ten Nights in one process, cycling 1->2->3 with soak seeds
        // seed+1..seed+nights, each run to its end (the full 240/270/300 s if nothing ends it first)
        // under the seeded random input stream (Core/SoakInputs.cs). crashes must be 0 and
        // mem_end must stay under 1.2 x mem_start (self-measured, same machine, reference values).
        private static void SoakMode(string[] args)
        {
            int seed = 7;
            int nights = 10;
            string outPath = null;
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--seed": seed = ReadInt(args, ref i, "--seed"); break;
                    case "--nights": nights = ReadInt(args, ref i, "--nights"); break;
                    case "--out": outPath = ReadPath(args, ref i, "--out"); break;
                    default: throw new FormatException("unknown argument " + args[i]);
                }
            }
            if (string.IsNullOrEmpty(outPath)) throw new FormatException("--out is required");
            if (nights < 1) throw new FormatException("--nights must be at least 1 (got " + nights + ")");

            JsonValue results = JsonValue.Arr();
            int crashes = 0;
            double memStart = 0.0, memEnd = 0.0;
            Stopwatch clock = Stopwatch.StartNew();

            for (int i = 1; i <= nights; i++)
            {
                int night = ((i - 1) % 3) + 1;
                long soakSeed = (long)seed + i;                  // the spec: soak seeds 8..17 for --seed 7
                long ticks = SoakInputs.NightTicks(night);
                try
                {
                    Simulation sim = new Simulation();
                    sim.Build((int)soakSeed, night);
                    if (i == 1) memStart = Environment.WorkingSet / 1048576.0;   // RSS after the first Night loaded
                    List<InputFrame> inputs = SoakInputs.Generate(soakSeed, ticks);
                    for (long k = 0; k < ticks && !sim.IsOver; k++) sim.Step(inputs[(int)k]);
                    results.Add(JsonValue.Str(sim.State));
                    if (i == nights)
                    {
                        sim.Build((int)soakSeed, night);          // "reset done": the spec's mem_end point
                        memEnd = Environment.WorkingSet / 1048576.0;
                    }
                }
                catch (Exception e)
                {
                    crashes++;
                    results.Add(JsonValue.Str("crash"));
                    Console.Error.WriteLine("soak: night " + i + " crashed: " + e.Message);
                }
            }

            JsonValue j = JsonValue.Obj()
                .Set("contract_version", JsonValue.Str("1.3"))
                .Set("mode", JsonValue.Str("soak"))
                .Set("seed", JsonValue.Int(seed))
                .Set("nights", JsonValue.Int(nights))
                .Set("crashes", JsonValue.Int(crashes))
                .Set("results", results)
                .Set("mem_start_mb", JsonValue.Real(memStart, 1))
                .Set("mem_end_mb", JsonValue.Real(memEnd, 1))
                .Set("duration_s", JsonValue.Real(clock.Elapsed.TotalSeconds, 1));
            WriteJson(outPath, j);
        }

        // v1.3 (TICKET-04). The persisted local leaderboard, exported in order: {"entries":[...]}.
        // The game writes the file as the player wins; --leaderboard-file points at it (default
        // ./leaderboard.json). A missing file exports an empty board — the command must run clean.
        private static void ExportLeaderboard(string[] args)
        {
            int at = 0;
            string outPath = ReadPath(args, ref at, "--export-leaderboard");
            string boardFile = "./leaderboard.json";
            for (int i = 1; i < args.Length; i++)
                if (args[i] == "--leaderboard-file") boardFile = ReadPath(args, ref i, "--leaderboard-file");

            Leaderboard board;
            string full = Path.GetFullPath(boardFile);
            if (File.Exists(full))
                board = Leaderboard.FromJson(JsonValue.Parse(File.ReadAllText(full, Encoding.UTF8)));
            else
                board = new Leaderboard();
            WriteJson(outPath, board.ToJson());
        }

        private static void WriteJson(string path, JsonValue j)
        {
            File.WriteAllText(Path.GetFullPath(path), j.Serialize() + "\n", new UTF8Encoding(false));
        }

        private static int ReadInt(string[] args, ref int i, string name)
        {
            if (i + 1 >= args.Length) throw new FormatException(name + " needs a value");
            int v;
            if (!int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                throw new FormatException(name + " needs an integer, got " + args[i]);
            return v;
        }

        private static string ReadPath(string[] args, ref int i, string name)
        {
            if (i + 1 >= args.Length) throw new FormatException(name + " needs a path");
            return args[++i];
        }
    }
}
