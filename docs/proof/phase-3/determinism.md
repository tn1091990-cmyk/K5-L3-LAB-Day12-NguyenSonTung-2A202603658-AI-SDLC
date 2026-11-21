evidence captured 2026-11-21 09:41 (+07:00) on Windows, .NET 8 (net8.0)
contract v1.2 format (fuel rules, drain state, new events refuel/lantern_out/fuel_drain,
lantern_lit gains the relight flag, game_over gains reason dawn_survived, all_lit is gone)
determinism: same seed, no scenario/inputs, 18000 ticks, two runs:
  dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 42 --ticks 18000 --out a.jsonl
  dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 42 --ticks 18000 --out b.jsonl
  sha256 run_a = 4F2636A777616E6EBAE96BD4864A8DF676927B8945C4A97433B0904F85A69B7D
  sha256 run_b = 4F2636A777616E6EBAE96BD4864A8DF676927B8945C4A97433B0904F85A69B7D
  byte-identical = True
the runs are saved next to this file as determinism_run_a.jsonl / determinism_run_b.jsonl
the phase-1/phase-2 evidence files document the v1.0/v1.1 formats and stay as they were;
the hash changed because v1.2 lines add fields and the tuning moved to the v1.2 numbers -
the Phase-1 core behaviours themselves are pinned unchanged by tests T02-T24
(the six tests whose rules v1.2 explicitly replaces - T06 scoring and T11 win order -
were updated, not skipped; the reasons are in ADR-001 and the CHANGELOG).
