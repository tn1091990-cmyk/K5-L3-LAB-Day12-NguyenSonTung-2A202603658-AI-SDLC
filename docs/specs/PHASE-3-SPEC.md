# Phase 3 spec — acceptance criteria per Part-3 item (TICKET-02 Part 3 / TICKET-03)

Source of truth for the numbers and rules is `Phases.txt` (this file only maps *how each item
is accepted*). Design record for the lantern state model and win rules:
`docs/adr/ADR-001-lantern-fuel-state-and-win-rules.md`. Breaking changes: `CHANGELOG.md`.

## Simulated items — verified by the suite (real output: `docs/proof/phase-3/tests.txt`)

| Item | Acceptance criterion (observable) | Verified by |
|---|---|---|
| Fuel rules | top-up from motes spends `ceil((fuel_max − fuel) / 10)` motes, event `refuel`, capped at 100.0; relight costs 5 motes (`relight: true` on the event); decay 1.0/s | T25, `rules_demo.md` |
| Shade `drain` state | enters from wander at ≤ 2 m XZ of a weak lit lantern, stands and sips 5.0/s; `fuel_drain` printed once per 60 ticks while draining (top-of-minute value, see ADR-001 #4); chase rules apply when Mây comes within 6 m | T26, `rules_demo.md` |
| Low fuel / safe zone | below 30 points a lit lantern loses its zone (shades may enter); the push rule still only runs for lanterns that have a zone; exactly 30 keeps it | T26, T29 |
| `lantern_out` | when a drained tank hits 0 the lantern goes out for the rest of the Night, event `lantern_out` | T26 |
| Win at dawn | `ceil(0.6 × 5) = 3` lit at time-up -> `won` / `dawn_survived` with the dawn score (312 pinned); 2 lit -> `lost` / `dawn`, score 0; flames-0 loss still checked first; `all_lit` no longer wins early | T27 |
| Capacity growth | 10 + 2 per successful light, hard cap 20; scenario `successful_lights` respected | T28 |
| Loader / contract v1.2 | new fields accepted in published shape; malformed guarded fields -> exit 2; lenient text-field convention (number -> unset) asserted, not thrown | T29 |
| Drifter "Gió Lồng" | 7 m/s, steals min(3, carried) on contact (XZ ≤ 1.2 m, once per drifter), flees 240 ticks then vanishes; pulse-flee drops the motes back | T22 (unchanged, green) |
| Charm rafts | 600-tick effect (7.2 m/s run, dash still 14 m/s × 12 ticks), respawn cycle 2700 ticks | T23 (unchanged, green) |
| 3-night chain | 240/270/300 s clocks, per-night schedules, `night_start` events, dawn win path | T20 (unchanged, green) |
| Pause | Esc/P freeze the sim fully; tick advances; `paused`/`resumed` events; input edge-triggered | T18 (unchanged, green) |
| Phase-1/2 regression | all earlier tests green on v1.2 tuning; the six tests whose rules v1.2 replaces were updated with reasons in-file, never skipped | T01–T29 run as a whole |

## Presentation items — Unity layer, manual-check criteria (no headless test possible)

The Unity editor on this machine still blocks batchmode builds on the expired license token
(recorded since Phase 1); these are the checks to run once a Windows build exists.

- **Settings**: language VN/EN switch takes effect immediately; mouse sensitivity applies to
  look; both persist across game sessions.
- **Sound cues** (each fires exactly on its rule): pickup, lantern lit, relight, charm, pulse,
  dash, hit, win, lose, bell-tower appear, last-10-seconds warning.
- **HUD**: Night label top-left; timer mm:ss top-centre, bold and blinking at ≤ 30 s;
  flame / motes / nearby-mote count top-right; mote bar (10 pips) bottom-centre; shade-spotted
  and dawn-approach indicators bottom-right; "E — Nạp ánh sáng (x/10)" prompt when near an
  unlit lantern; label of the nearest unlit lantern when looking at it, hidden off-screen.
- **Palette** (Phases.txt table): every listed pair must differ by ≥ 10 % on each RGB channel
  (screenshot check with the picker); new-light brightness ramp = `0.15 + 0.55 × (lit/total)`.
- **Accessibility**: colourblind-safe variants for the palette pairs; flicker off-switch for
  the low-fuel blink; HUD fuel bar readable at 1080p from a normal sitting distance.
- **Performance**: ≥ 30 FPS average at 1280×720 (formal benchmark harness lands in Phase 4).
