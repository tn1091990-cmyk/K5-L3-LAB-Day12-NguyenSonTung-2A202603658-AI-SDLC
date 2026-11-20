# RETRO — what worked, what didn't (updated per phase; final at day's end)

## Phase 1
- Worked: making the core a pure-C# library *before* touching Unity — the headless runner and
  tests were a 10-minute job afterwards, and determinism fell out for free.
- Worked: running the test suite for real before writing the proof files; the proofs are
  captured outputs, nothing pasted from memory.
- Didn't work: trusting that Unity's license would stay valid — it expired mid-phase and the
  batchmode build now fails at the license step even though the probe passed earlier.
  Fix: re-activate in Unity Hub, rebuild (nothing in the code needs to change).
- Honest gap at this checkpoint: the Unity Windows build artifact does not exist yet on this
  machine because of the license issue above. Marked "not met yet" rather than glossed over.
