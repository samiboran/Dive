# Lore Bible — UnderwaterExtraction

> **Who reads this:** any AI session assigned the "Writer" role (see
> `Docs/VFX_ATMOSPHERE.md`'s sibling brief for Writer). This file is the single
> source of truth for **established world facts** — not mechanics, not system
> status (that's `PROJECT_STATE.md`), just "what is true about this world."
>
> **Purpose:** multiple Writer sessions will touch this project at different
> times (DiveComputerAI Q&A, item flavor text, X posts, encounter text). None
> of them share memory. Without a shared canon, one session invents "the
> operation is insurance-funded" and another invents "it's a cartel salvage
> job" — both plausible, both contradictory. This file prevents that.
>
> **Rule:** when a Writer session establishes a new fact that other content
> should stay consistent with (even something small — a name, a reason, a
> detail implied by flavor text), add it to "Established Canon" below with a
> one-line note of where it came from. Don't rewrite or contradict existing
> entries — if a fact needs to change, that's a deliberate decision, flag it
> to the user rather than silently overwriting.
>
> Remember the hard rule from the Writer brief: this is **systemic/emergent
> world-building, not a main story.** Canon entries here should stay small and
> implicational (what a detail suggests), not plot beats, acts, or arcs.

---

## Established Canon

*(facts already fixed by earlier design work — CONTEXT.md — treat these as
non-negotiable unless the user explicitly reconsiders them)*

- **Project codename:** "Depth Extraction" (working title; player-facing name
  is UnderwaterExtraction, not yet finalized as a marketing title).
- **Elevator pitch / tone anchor:** "Escape from Tarkov underwater" — oxygen is
  the timer, depth is the risk, death costs your gear. Tense, grounded,
  survival-horror-adjacent. Never campy, sci-fi, or cartoonish.
- **First map:** *The Dredge* — a cargo shipwreck, 3 vertical floors (Upper
  Deck / Middle Deck / Engine Room), rising risk and loot density with depth.
  Floor 1 has a safe, low-reward exit; Floor 3 has a fast, current-swept,
  high-risk exit.
- **Threats are naturalistic, not supernatural:** sharks (hunger-driven AI, not
  monsters with intent) and rival divers (other people doing the same
  desperate job you are, not faction soldiers). No sci-fi creatures, no magic.
- **DiveComputerAI, in-world:** a standard-issue wrist dive computer, no
  name/callsign. Deliberately impersonal — telemetry and information
  retrieval only, explicitly *not* a companion or an AI with a personality.
  When asked who funds/runs the operation, it deflects ("outside operational
  scope") rather than answering — this response *pattern* is canon, the
  underlying answer to who funds the operation is still open below.
  *(Established while drafting `Docs/Content/DiveComputerQA.md`.)*

## Open Questions — not yet decided

*(a Writer session may decide a small number of these when doing
world-building work; once decided, move the answer up into "Established
Canon" with a note on which content first established it — e.g. "decided
while drafting DiveComputerAI entry X")*

- Who actually runs/funds this diving operation? (a company, a black-market
  buyer, a desperate individual crew, something else?)
- Why does this operation exist — what's the economic or personal incentive
  that puts divers at this much risk for salvage?
- Is the player a hired diver, a debtor, a volunteer, something else? (Keep
  this light — the game has no dialogue/cutscenes to deliver a backstory
  through, so this should only ever surface as implication, e.g. in
  DiveComputerAI tone or item flavor text.)
- Why is *this specific* wreck/reef being salvaged — is it recent, historic,
  contested, cursed-by-reputation-only (never actually supernatural)?

## Explicitly NOT canon (avoid contradicting these)

- No supernatural/mystical threat — sanity/hallucination systems (see
  `SanitySystem` in VFX brief, not yet built) are *psychological* (cave-diving
  disorientation), not evidence of anything actually otherworldly in the game
  world. Writer content should never confirm a hallucination was "real."
- No scripted main story, acts, or bosses — per the Writer brief's hard rule.
  If asked to write one, flag it back rather than doing it.
