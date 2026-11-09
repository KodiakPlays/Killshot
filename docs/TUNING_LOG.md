# Tuning log

Every change to a game number, newest first. The owner's rule (2026-11-09): change numbers through
**Starwolf → Tuning** first, and always write down the old value, so any change can be checked and undone.
The rule is in `CLAUDE.md`, under "Game numbers".

How to add a row: one row per number. "Where" is the Tuning tab and field, or the file if Tuning can't reach it.

**Before this log began:** `docs/TUNING_AUDIT_2026-11-09.md` lists every number changed from 2026-11-01 to
2026-11-09, with old and new values, and the numbers Tuning can't reach yet.

| Date | Number | Old | New | Where | Who asked, and why |
|---|---|---|---|---|---|
| 2026-11-10 | Glow colour: the light round each square on the intro and loading screens | The square colour, #A2F9A0 (a pale grey-green glow) | #1AFF2E, pure green | `Assets/StarwolfIntro/Resources/StarwolfIntro/glyphs.json` → `glow` | Owner 2026-11-10: "the references had a lovely, slightly richer green. If you can get to that effect, I'll be happier." Matched by measuring `References/Company Logo.png` |
| 2026-11-10 | Square edge colour: each square is pale in the middle and fades to this at its edge | None (each square was one flat colour, #A2F9A0) | #3CA050 at the edge; the middle stays #A2F9A0 | `glyphs.json` → `rim` | Owner 2026-11-10: "the references had a lovely, slightly richer green. If you can get to that effect, I'll be happier." Matched by measuring `References/Company Logo.png` |
| 2026-11-10 | Glow next to a square | 0.015, written in the shader's code | 0.02 | `Assets/StarwolfIntro/Shaders/PhosphorDisplay.shader` → Glow next to a square (`_GlowNear`) | Owner 2026-11-10: "the references had a lovely, slightly richer green. If you can get to that effect, I'll be happier." Matched by measuring `References/Company Logo.png` |
| 2026-11-10 | Glow a little further out | 0.009, written in the shader's code | 0.012 | `PhosphorDisplay.shader` → Glow a little further out (`_GlowFar`) | Owner 2026-11-10: "the references had a lovely, slightly richer green. If you can get to that effect, I'll be happier." Matched by measuring `References/Company Logo.png` |
| 2026-11-10 | Wide halo round the strokes | None | 0.003 | `PhosphorDisplay.shader` → Wide halo round the strokes (`_GlowWide`) | Owner 2026-11-10: "the references had a lovely, slightly richer green. If you can get to that effect, I'll be happier." Matched by measuring `References/Company Logo.png` |
| 2026-11-10 | Corner text on INITIALISING: size, colour, and clearance squares lit | None (no corner text) | Size 24 on the 1920 × 1080 screen; colour #73E18A at 80%; 3 of 5 squares lit. Places measured from the reference picture | `StarwolfIntroController` on `INITIALISING` in `01_Briefing` → Loading screen corners | Owner 2026-11-10: "small text on the screen and clearance squares"; "dim, in Share Tech Mono" |
