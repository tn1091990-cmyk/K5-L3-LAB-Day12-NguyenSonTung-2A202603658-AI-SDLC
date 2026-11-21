# Docker on Linux — real run (clean-ish machine, Docker 29.9.3, WSL2 engine)

Commands (the README's Docker pair):

```
docker build -t dao-den-long .
docker run --rm dao-den-long
```

What the container ran, in order: `dotnet build` (succeeded), the full test suite, the bench
(`--seed 42 --duration 30`), the soak (`--seed 7 --nights 10`). Real output tails:

```
PASS  T28 capacity (v1.2): +2 per successful light, capped at 20
PASS  T29 v1.2 fields: scenario fuel/ever_lit/drain-target round-trip
PASS  T30 leaderboard: name rules, top-10, score desc, ties keep achievement order
PASS  T31 soak inputs: same seed, same stream; the three Night clocks

31/31 tests passed.
{"contract_version":"1.3","mode":"bench","seed":42,"duration_s":30,"window":[1280,720],"avg_fps":7217.0,"p1_low_fps":2817.1,"frame_ms_p95":0.168,"frames":189005,"load_ms":0,"peak_mem_mb":51,"vsync_off":true,"renderer":"headless .NET core (pure-C# simulation, no GPU) — the Unity GUI scene (View/BenchMode.cs) renders the same scene at 1280x720, vsync off, orbit camera 15 s/turn","os":"Ubuntu 24.04.5 LTS"}
{"contract_version":"1.3","mode":"soak","seed":7,"nights":10,"crashes":0,"results":["lost","lost","lost","lost","lost","lost","lost","lost","lost","lost"],"mem_start_mb":24.6,"mem_end_mb":49.6,"duration_s":0.2}
```

Exit code 0. Bench thresholds pass by a mile on Linux too. The soak memory ratio here is
2.02x — the same one-time warm-up story as `notes.md` explains (mem_start is sampled before
JIT/GC settles; the plateau is the steady state, proven by the 20-night run there).

Honest caveat: "clean machine" here means a fresh container build of this repo; the Docker
*daemon* is this lab machine's (no license or account issues — just not a second physical
computer). The GUI game itself still needs the Unity Windows build (license on the editor
path — unchanged honest note from Phase 1).
