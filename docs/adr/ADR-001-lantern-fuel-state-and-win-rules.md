# ADR-001 — Lantern fuel state model and the changed win rules (Phase 3)

Date: 2026-11-21. Decided with the human before implementing TICKET-02 Part 3 / TICKET-03.

## Context

v1.2 puts fuel on lanterns (decay 1.0/s, top-up from motes at 10 motes per point, relight
costs 5 motes, shades sip fuel from weak lanterns), moves the win from "all lit" to a dawn
threshold (`ceil(0.6 x lantern_count)`), caps capacity growth at 20 (+2 per successful light),
and replaces the scoring table (per-mote 5, per-lantern 100, relight 40, no time bonus...).
These decisions change byte-level output, so they had to be pinned exactly.

## Decisions

1. **Fuel is stored in integer units, 1 point = 60 units.** Decay (1/s), drain (5/s) and the
   thresholds are per-second numbers; storing whole display points in a `double` would make
   subtraction order visible in the printed decimals. Units keep the arithmetic exact; only
   the display divides by 60 and prints 1 decimal. Scenario fuel above `fuel_max` is accepted
   unclamped (the loader validates `>= 0` only).
2. **The low-fuel threshold is exclusive-below: exactly 30 keeps the safe zone.** Below 30 a
   lit lantern loses its zone, so a drain shade can stand inside the old ring untouched — the
   SafeZones push loop is gated on `lit && fuel >= 30 points` (this replaced `lit` alone).
3. **`drain` is appended at the end of the `ShadeState` enum** (spawning, wander, chase,
   recoil, stunned, drain) — enum numbers appear in no output, but appending keeps every
   Phase-1 scenario code intact.
4. **Beat lines print the tank at the top of the minute, before that minute's subtractions;**
   snapshots print after the step. That is why a `fuel_drain` line and the next snapshot
   differ by exactly one decrement (1500-unit T29 scenario: beat prints 19.1, snapshot 61
   prints 19.0). The order is a pinned convention, not a bug.
5. **Drain entry:** a wander shade within `<= 2 m` XZ of a weak lantern enters drain *measured
   before the step moves*; it then stands and sips. The first sip is the step after entry, so
   a shade that enters during step 60 shows `wander` at snapshot 60 and `drain` from 61.
6. **Win check order is unchanged:** flames-0 loses first; at time-up, lit lanterns must reach
   `ceil(0.6 x count)` -> won with `reason dawn_survived`; below -> lost with `reason dawn`.
   Dawn bonuses (+50 per lantern +50 per fuel point, +50 per flame) apply only on won-at-dawn.
7. **Scenario lanterns are updates to the default island, never a replacement** — so the dawn
   threshold denominator is always five. `all_lit` no longer ends the Night early (T11 updated).
8. **Scoring: per-mote 200 -> 5, no time bonus, per-lantern 100.** The two scoring pins in T06
   and the win-order pins in T11 are the tests this decision had to update (with comments) —
   updated, never skipped.

## Consequences

- `game.contract.json` is v1.2: eight new tuning keys appended after `night3_duration_s` in the
  published order, then the score block; new events `refuel`, `lantern_out`, `fuel_drain`;
  `lantern_lit` gains `relight: true|false`; snapshot lanterns gain `fuel`/`ever_lit`, player
  gains `successful_lights`, snapshot gains `capacity`; scenario lanterns gain `fuel`/`ever_lit`,
  shades gain `target_lantern_id`, player gains `successful_lights`.
- Determinism hashes change (new fields); Phase-1/2 evidence stays as recorded.
- Suite is 29 tests, all green; T25-T29 pin the new rules; T06/T11 updated with reasons.
- The loader keeps its Phase-1 lenient-text convention (a number where an id string is expected
  falls back to "unset" instead of throwing); the T29 rejection list matches what the loader
  actually validates, and the new test asserts the convention instead of fighting it.
