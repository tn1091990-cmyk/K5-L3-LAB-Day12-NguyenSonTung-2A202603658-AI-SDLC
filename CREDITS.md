# CREDITS — assets & sources

Per the lab rules: everything here is self-made, AI-made, or CC0. Nothing needs a paid license.
In this project I chose to use **no third-party assets at all** (and no AI-made assets yet) —
it is fully self-made, which keeps the determinism guarantees clean:

| Item | Source | License |
|---|---|---|
| Deterministic core (pure C# lib, shared Unity/headless) | self-made (by me, with my AI harness — see docs/notes/AI_JOURNAL.md) | MIT (this repo) |
| Headless Test Contract runner + test suite | self-made | MIT (this repo) |
| Unity scene & 3D visuals | built from Unity primitives (procedural), no imported art | Unity Editor tools are licensed to me (personal license) |
| Audio | none yet (phase 2+) | — |
| Fonts/text | Unity default UI font | Unity (free for projects) |

No NuGet packages. No `.env`, no keys anywhere. See `docs/notes/AI_JOURNAL.md` for which
decisions were made by the human vs the agent.
