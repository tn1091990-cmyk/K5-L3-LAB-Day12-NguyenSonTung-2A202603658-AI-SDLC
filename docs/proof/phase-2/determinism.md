evidence captured 2026-11-21 05:58 (+07:00) on Windows, .NET 8 (net8.0)
contract v1.1 format (adds night_start line, night, dash/cooldown/charm snapshot fields)
determinism: same seed, no scenario/inputs, 18000 ticks, two runs:
  dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 42 --ticks 18000 --out a.jsonl
  dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 42 --ticks 18000 --out b.jsonl
  sha256 run_a = 683742B6BC98698647A5F1FC1D1E12E4F3DC939D760439E89A0BCAF0D4B1EC5F
  sha256 run_b = 683742B6BC98698647A5F1FC1D1E12E4F3DC939D760439E89A0BCAF0D4B1EC5F
  byte-identical = True
the runs are saved next to this file as determinism_run_a.jsonl / determinism_run_b.jsonl
headless full-night runtime (budget: <= 30 s):
  dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 7 --ticks 21600 --night 3 --out n3full.jsonl
  measured 0.87 s total (dotnet startup included) on this machine
the phase-1 evidence (docs/proof/phase-1/) documents the v1.0 format and stays as it was;
the hash changed because v1.1 lines added fields — the Night-1 values themselves are pinned
unchanged by tests T02-T16.
