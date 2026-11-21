// v1.3 (TICKET-04) — the benchmark scene, Unity side. Same spec as the headless runner path:
// 1280x720, vsync off, Night 1 with the BenchScene (200 wandering shades, 100 motes, L1-L3 lit
// at fuel 100), no input, Mây stands still, the camera orbits the island centre once per 15 s
// (presentation only), 3 s warmup then exactly --duration seconds of measurement, and the same
// bench.json fields. This is the real GPU path; it needs a licensed Unity editor/player to run.
// On a machine without the Unity license, the headless runner's --bench mode measures the same
// core scene and is the canonical number (docs/proof/phase-4/notes.md explains both paths).
//
// Drop this on an empty GameObject in the game scene (GameBootstrap draws the island).
// Adjust the two public fields (or the command line -seed/-duration style args Unity passes)
// and press Play; bench_unity.json lands next to the project root.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using DaoDenLong.Core;

namespace DaoDenLong.View
{
    [DisallowMultipleComponent]
    public sealed class BenchMode : MonoBehaviour
    {
        public int Seed = 42;
        public int DurationS = 30;
        public string OutPath = "bench_unity.json";

        private Simulation _sim;
        private System.Diagnostics.Stopwatch _clock = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch _processStart = System.Diagnostics.Stopwatch.StartNew();
        private double[] _dts = new double[200000];
        private int _n;
        private long _peakBytes;
        private double _loadMs;
        private bool _warmup = true;

        private void Awake()
        {
            Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);   // the spec's window
            QualitySettings.vSyncCount = 0;                                     // vsync off
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

            _sim = new Simulation();
            _sim.Bench = true;
            _sim.Build(Seed, 1);
            _sim.ApplyScenario(BenchScene.BuildScenario(Seed));
            _loadMs = _processStart.ElapsedMilliseconds;    // process start -> first bench frame
            _clock.Start();
        }

        private void Update()
        {
            double t = _clock.Elapsed.TotalSeconds;
            if (_warmup)
            {
                _sim.Step(null);                            // 3 s warmup, not measured
                if (t >= 3.0) { _warmup = false; _clock.Restart(); }
            }
            else
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                _sim.Step(null);                            // one rendered frame = one core tick
                _dts[_n++] = sw.Elapsed.TotalMilliseconds;
                long ws = Environment.WorkingSet;           // the periodic RSS sample (spec)
                if (ws > _peakBytes) _peakBytes = ws;
                if (t < DurationS) return;
                Report();
            }

            // the orbit camera is presentation only: 15 s per turn around the island centre
            double deg = (t / 15.0) * 360.0;
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(
                    (float)(45.0 * Math.Cos(deg * Math.PI / 180.0)), 25.0f,
                    (float)(45.0 * Math.Sin(deg * Math.PI / 180.0)));
                Camera.main.transform.LookAt(Vector3.zero);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 600, 30), string.Format(CultureInfo.InvariantCulture,
                "bench: {0} frames measured / seed {1} / {2} s", _n, Seed, DurationS));
        }

        private void Report()
        {
            double sum = 0.0;
            for (int i = 0; i < _n; i++) sum += _dts[i];
            double avgFps = 1000.0 * _n / sum;

            double[] slow = new double[_n];
            Array.Copy(_dts, slow, _n);
            Array.Sort(slow);                                                   // ascending
            int k = Math.Max(1, (int)Math.Ceiling(_n * 0.01));
            double slowMean = 0.0;
            for (int i = 0; i < k; i++) slowMean += slow[_n - 1 - i];
            double p1Low = 1000.0 / (slowMean / k);
            double p95 = slow[(int)Math.Ceiling(0.95 * _n) - 1];                // nearest rank

            JsonValue j = JsonValue.Obj()
                .Set("contract_version", JsonValue.Str("1.3"))
                .Set("mode", JsonValue.Str("bench"))
                .Set("seed", JsonValue.Int(Seed))
                .Set("duration_s", JsonValue.Int(DurationS))
                .Set("window", JsonValue.Arr().Add(JsonValue.Int(1280)).Add(JsonValue.Int(720)))
                .Set("avg_fps", JsonValue.Real(avgFps, 1))
                .Set("p1_low_fps", JsonValue.Real(p1Low, 1))
                .Set("frame_ms_p95", JsonValue.Real(p95, 3))
                .Set("frames", JsonValue.Int(_n))
                .Set("load_ms", JsonValue.Int((long)_loadMs))
                .Set("peak_mem_mb", JsonValue.Int(_peakBytes / 1048576))
                .Set("vsync_off", JsonValue.Bool(true))
                .Set("renderer", JsonValue.Str("Unity (built-in pipeline, procedural primitives — the same core the tests run)"))
                .Set("os", JsonValue.Str(RuntimeInformation.OSDescription));
            File.WriteAllText(OutPath, j.Serialize() + "\n");
            print("bench wrote " + OutPath + ": avg " + avgFps.ToString("F1", CultureInfo.InvariantCulture) + " fps");
            Application.Quit();     // exit 0 when the window closes
        }
    }
}
