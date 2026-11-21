# Phase 3 rules demo (real runner output)

`rules_demo.jsonl` is unedited output of:

    dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --seed 63 --ticks 95 \
      --scenario probe_demo.json --inputs probe_inputs.json --snapshot-every 30 --out demo.jsonl

scenario: `{"random_motes":false,"motes":[],"random_shades":false,"shades":[{"id":"S1","pos":[3,0,0],"state":"drain","target_lantern_id":"L1"}],"lanterns":[{"id":"L1","pos":[0,0,0],"lit":true,"fuel":29.0}],"player":{"pos":[0,0,-2],"carried":10}}`
inputs: one frame, `{"tick":0,"interact":true}`.

What the lines show (the v1.2 rules working together):

- L1 starts lit with fuel 29.0 — below the threshold 30, so it has **no safe zone**: the
  drain shade stands inside the old 6 m ring undisturbed (snapshot 0).
- tick 1 `refuel {lantern_id L1, amount 8, fuel 100.0}` — the press from the 2 m edge spends
  `ceil((100 - 28.9) / 10) = 8` motes (28.9 = the fuel after step 0's decay; the beat-1 number
  is what the top-up math sees) and caps at 100.0; carried drops 10 -> 2; score +40 (8 x 5).
- The top-up crosses the threshold, so the zone **comes back** and the existing push rule runs:
  inside by 3 m -> walk to the 6 m edge within 30 ticks, **no damage** (the score stays 40 and
  the player keeps all three flames; `player_hit` never fires).
- snapshot 60/90: `S1 pos [5.965, 0.000, -0.647] ... "state":"chase"` — pushed to the ring edge,
  then chasing (Mây is inside 6 m, so the drain shade chases first).
- The full-behaviour pins live in the suite: walk-in drain + one `fuel_drain` line per 60 ticks
  + `lantern_out` when the tank runs dry = T26; beat lines print the tank at the top of the
  minute before that minute's subtractions (pinned by the T29/T26-run2 numbers); relight costs
  5 motes = T25; the dawn threshold is `ceil(0.6 x 5) = 3` lit -> T27 (3 lit wins with score 312,
  2 lit loses with no bonus); capacity `10 + 2 x lights` capped at 20 = T28.

Every line in the jsonl is byte-for-byte runner output; nothing was edited.
