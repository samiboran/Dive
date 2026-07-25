# VFX / Atmosphere Direction — UnderwaterExtraction

> **Scope note:** This is a spec, not code. Every effect below names the C# event
> it hooks into (per the project's event-driven pattern — VFX/audio has zero
> gameplay logic, it only reacts). Kimi (programmer role) implements the actual
> shader/particle/audio work. Nothing here proposes a new gameplay mechanic; where
> an effect implied one, it's flagged back instead of decided.
>
> Checked against the current codebase (2026-07-25) so event names match what
> actually exists, not just what earlier briefs assumed — see "Naming corrections"
> at the bottom.

---

## 0. Atmospheric pillars (applies everywhere, not tied to one event)

**Visibility is the horror, not the shark.** Fog density and color grading should
scale with the CONTEXT.md floor risk table (Floor 1 low-risk / Floor 2 medium /
Floor 3 high-risk, 2x O2 draw). Don't aim for a "pretty ocean" look at any tier —
even Floor 1 should read as slightly unsettling. Fog color: desaturated teal-grey,
never saturated postcard-blue. As depth/floor risk increases, increase fog density
and pull the color grade toward near-monochrome grey-green, crushing contrast so
shapes read as silhouettes before they read as detail.

**The player's own breathing is the metronome.** Regulator bubble audio (looping,
tied to player breath rate, not diegetic per-event) is the constant background
clock the player's subconscious tracks. Every panic/oxygen effect below should
speed up or distort *that* loop rather than introducing a separate new audio bed
— it's the cheapest and most consistent way to communicate rising tension.

**Silence is a cue, not an absence.** The planned BoundaryEncounter sequence
(see §4) explicitly wants "underwater silence" as part of the fear beat. Treat
ambient bed removal as an available effect in its own right, not just "nothing
happening" — cutting the ambient hum/creak layer abruptly is often scarier than
adding a sound.

**Perf default:** prefer color-grading LUT swaps, vignette, and reusing one
ambient sediment/particle system with modulated emission rate over spawning new
particle systems per event. Solo-dev budget — screen-space post effects should be
toggled/blended states on a small fixed set of volumes, not one-off VFX prefabs
per trigger.

---

## 1. Shark (`AI/SharkBehavior.cs`)

### Circling Tell
- **Trigger:** `OnCirclingStarted` (fires entering `Circling` state; duration is
  randomized 4–8s via `circlingDuration`, at `circleRadius` = 8m around the player).
- **Visual:** No full model reveal — this is the game's one guaranteed pre-attack
  signal, so it must be legible but never crisp. As the shark orbits at
  `circleRadius`, the player should catch a fast dark shape crossing the edge of
  visibility every few seconds (timed by `circleSpeed`/`circleRadius`, not a fixed
  interval) rather than a continuously visible model. Kick up a sediment/bubble
  swirl in its wake using the existing ambient particle system, spiking emission
  rate only.
- **Audio:** Low sub-bass drone, filtered/muffled as if heard through water,
  crescendoing slowly across the full tell window. Pan/attenuate it to track the
  shark's actual orbital position so players can feel *direction* without a visual
  lock. Keep this distinct from PanicState's heartbeat cue (different register —
  drone vs. thud) since the two can overlap if oxygen is also low.
- **Reference:** think *Jaws* POV shots (glimpse, not stare) rather than a modern
  "monster reveal" — the tension is in not-quite-seeing it.
- **Perf:** reuse ambient particle system; no new emitters.

### Attack Started
- **Trigger:** `OnAttackStarted` (aggression resets, shark closes at `attackSpeed`
  toward bite range).
- **Visual:** Hard cut from the slow dim/drone of Circling into a fast, short
  (<1s) camera shake + mild lens distortion as the shark closes distance. This is
  the jump-scare beat — keep it brief so it doesn't fatigue on repeat encounters.
- **Audio:** Drone cuts abruptly, replaced by a sharp water-displacement
  "whoosh." Bite connect itself is a separate beat — see `InjurySystem.OnInjured`
  below for the actual hit-confirm sting, since that's what actually fires on
  contact (`OnAttackStarted` only means the shark started closing, not that it hit).

### Flee Started
- **Trigger:** `OnFleeStarted`.
- **Visual:** Blood-cloud particle burst at the hit point, dissipating over
  3–4s; shark visibly recoils and turns tail-first into the murk.
- **Audio:** A single pained thrash/yelp sound, distinct from the attack whoosh.
- **⚠️ Flag (same issue the brief already called out, confirmed in code):**
  `OnFleeStarted` fires identically for a body-shot (temporary, 15s, *will* come
  back through another Circling tell) and a head-shot (`permanentFlee = true`,
  gone for good). If you want a different resolution beat for "it's coming back"
  vs. "it's gone" — e.g. tension staying elevated vs. a genuine relief cue — that
  needs a new bool param or a second event on `SharkBehavior.cs`. Not deciding
  that here; flagging it for the architect/Kimi to add if wanted.

---

## 2. Bot Diver (`AI/BotDiverAI.cs`)

Three tiers exist (`SO_BotDiverData.Tier`: Yellow / Red / Spear) but there's only
one `OnAttackPerformed` event shared across all of them, and the attack itself is
already randomized in code between knife wound / mask knock-off / regulator pull
— so this spec keys off *what actually happened*, not the tier, using the more
specific events fired by those sub-systems (§3).

### Attack Performed
- **Trigger:** `OnAttackPerformed` (fires after the specific attack effect —
  `InjurySystem.ApplyInjury(KnifeWound)`, `PlayerEquipmentState.KnockOffMask()`,
  or `PlayerEquipmentState.PullRegulator()` — has already been applied; there's a
  0.3s freeze in `PerformAttack()` right after).
- **Visual:** A close-quarters "flash of another diver in your space" beat —
  brief motion blur toward the point of contact, since this is a human-shaped
  threat rather than an animal one; keep it colder/more mundane than the shark's
  beats (no blood cloud from this event alone — the specific injury/equipment
  event below carries its own visual).
- **Audio:** A muffled grunt/struggle sound (two divers colliding), not a
  monster stinger — the whole point of BotDiverAI is "another desperate person,"
  not a creature.

### Flee Started (face-to-face detection)
- **Trigger:** `OnFleeStarted` (bot spotted the player looking at it and bailed).
- **Visual/Audio:** Understated — a startled kick-away animation is enough; this
  isn't a scare beat, it's closer to a "you both flinched" moment. Low priority.

### Caught (Yellow tier only)
- **Trigger:** `OnCaught` (player closed the distance during `CatchWindow`).
- **Visual/Audio:** A short "cornered" stinger — reversed tension, since here the
  *player* is now the threat. Sells the moment the bot goes from fleeing to
  helpless (`FleeSpeed * 0.25`).

### Died
- **Trigger:** `OnDied`.
- **Visual:** Loot scatter is already handled in code (`DropLoot()`); pair it
  with a small blood/debris puff, no more than the shark's flee-hit cloud in
  scale — this is a person, keep it grounded rather than gory.
- **Audio:** Muted, no fanfare — matches the game's "no arcade highs" tone.

---

## 3. Gear Fear (`Systems/PlayerEquipmentState.cs`)

### Mask Knocked Off
- **Trigger:** `OnMaskKnockedOff` *(see naming correction below — the brief
  listed `OnMaskRemoved`, actual event is `OnMaskKnockedOff`)*.
- **Visual:** This is the game's vision-impairment beat — screen-space blur/water
  distortion kicks in immediately and stays until recovery (open-ended, per code
  comment: "zamanla kendi kendine düzelmez," it does not clear on its own). Blur
  should read as "no mask seal," not a generic screen blur — bias toward a wobbly
  refraction distortion plus loss of clarity at the edges, not center-focus blur,
  since a real diving mask flood blinds peripheral vision first.
- **Audio:** Muffled regulator breathing gets louder/more ragged (ties into the
  breathing-as-metronome pillar in §0) — the mask coming off should sound like
  panic in the breath, not a sound effect layered on top.
- **Perf:** one persistent post-process volume toggle, no per-frame cost beyond
  what's already budgeted for panic/oxygen grading.

### Mask Recovered
- **Trigger:** `OnMaskRecovered` (fires after `RhythmRecoveryQTE` completes —
  always completes, missed beats only cost extra O2, never fail the sequence).
- **Visual:** Blur/distortion clears — ideally not instantly but over ~0.5–1s, so
  the moment reads as "vision resolving" rather than a hard cut.
- **Audio:** Breathing settles back toward baseline over the same short window.

### Regulator Pulled
- **Trigger:** `OnRegulatorPulled` (instant O2 loss, no separate recovery flow —
  confirmed in code, this is a one-shot punish, not a state).
- **Visual/Audio:** A sharp, single gasp/choke beat plus a quick jolt to the
  oxygen HUD read-out. Should feel like a slap, not a sustained state, since
  there's nothing here for the player to actively resolve.

### Rhythm Recovery QTE beats (`Systems/RhythmRecoveryQTE.cs`)
- Not in the original hook list but directly adjacent to Mask Recovered above and
  worth speccing since it's the player's only interactive moment during a mask
  loss: `OnBeatWindowOpened(beatIndex, totalBeats)` → a short visual pulse/ring
  cue timed to `hitWindow` (0.3s); `OnBeatResult(bool hit)` → a clean hit vs. a
  miss should sound distinctly different (miss should feel costly but not
  punishing, since misses never fail the sequence — avoid a harsh failure
  buzzer, use something closer to "missed beat, keep going").

---

## 4. Panic & Oxygen (`Systems/PanicState.cs`, `Systems/OxygenSystem.cs`)

### Panic Started / Ended
- **Trigger:** `OnPanicStarted` / `OnPanicEnded` (auto-triggers when oxygen ≤ 60s
  remaining, per `panicThreshold`).
- **Visual:** Tunnel vision (vignette closing in) + slight desaturation, easing
  in over ~1s on start, easing back out on end rather than snapping — panic is a
  ramp, not a switch.
- **Audio:** Heartbeat audio bed, tempo tied to how close to 0 oxygen is (get
  faster as oxygen keeps dropping *while* still panicking, not just a fixed
  loop), plus the breathing-metronome loop from §0 getting audibly ragged.
- **⚠️ Flag:** `PanicState.cs` also declares a `warningThreshold = 90f` that is
  currently unused/undead code — nothing subscribes to it or fires at that
  threshold. If the intent was a pre-panic "oxygen getting low" ambient cue
  (a warning beat *before* full tunnel-vision panic), that needs a new event
  wired up on `PanicState.cs` — flagging for architect/Kimi rather than assuming
  it should exist.

### Adrenaline Used
- **Trigger:** `OnAdrenalineUsed` (immediately clears panic for `adrenalineDuration`
  = 18s, then panic can re-trigger if oxygen is still low when it wears off).
- **Visual:** A brief bright flash/clarity snap opposite to the panic vignette —
  vision snaps open, not fades open, to sell "chemical override." Consider a
  faint color-grade tint (cold blue-white) for the duration so the player has a
  passive read on "adrenaline is still active" without checking a HUD timer.
- **Audio:** A sharp inhale/heart-rate spike-then-steady sound, then the
  breathing loop should sound artificially calm/steady for the duration —
  unsettling-calm, since the player knows it's borrowed time.

### Oxygen Critical / Depleted
- **Trigger:** `OxygenSystem.OnOxygenCritical` (≤90s remaining) /
  `OnOxygenDepleted` (0).
- **Visual:** Critical: color grade starts crushing toward grey/desaturated
  ahead of the panic vignette itself, so there's a readable escalation ladder
  (critical → panic → depleted) rather than everything triggering at once.
  Depleted: screen darkens further and DrowningHandler's per-second damage
  should get a visible pulse/red-flash per tick so damage over time doesn't feel
  silent.
- **Audio:** Critical: breathing loop gets audibly strained. Depleted: breathing
  loop should stop/choke — silence here (per §0's "silence is a cue" pillar) is
  more effective than a generic alarm sound.

---

## 5. Injury (`Core/InjurySystem.cs`)

### Injured
- **Trigger:** `OnInjured(InjuryType)` — currently fires for `SharkBite` and
  `KnifeWound` (JellyfishSting/Barotrauma exist in the enum but aren't applied
  anywhere yet in code — don't spec effects for those until something calls
  `ApplyInjury` with them).
- **Visual:** Blood-in-water particle wisp trailing from the player's position,
  persisting at low intensity for as long as `currentInjury != None` (matches the
  bleed-over-time in code) — this is also the shark's blood-lure signal, so
  making it visually readable to the player doubles as foreshadowing their own
  increased shark aggro risk.
- **Audio:** SharkBite vs. KnifeWound should sound different at the moment of
  injury — bite = a heavier, wetter impact; knife = a sharper, thinner stab
  sound — even though the mechanical effect (bleed DPS + move penalty) is
  structurally similar in code.

### Bandage Applied / Interrupted
- **Trigger:** `OnBandageApplied` (after `bandageApplyTime` = 4s) /
  `OnBandageInterrupted`.
- **Visual:** Blood wisp fades out on Applied. On Interrupted, cut the bandage
  visual abruptly rather than fading — it should read as "that didn't work,"
  distinctly different from a successful heal.
- **Audio:** A satisfying short "sealed" sound on Applied; a sharp negative stinger
  on Interrupted so the player doesn't need to check a progress bar to know it failed.
- **Note:** per PROJECT_STATE.md, nothing currently calls `InterruptBandage()` on
  taking damage (that auto-interrupt was removed in v2) — so today this event
  only fires if something explicit triggers it. Spec stands for whenever that's
  wired back up; not something to build around as a frequent occurrence yet.

---

## 6. Death & Extraction

### Player Death
- **Trigger:** `PlayerHealth.OnDeath`.
- **Visual:** Fade to black over ~1.5s, no dramatic slow-mo — keep it matter-of-
  fact given the extraction-shooter "you lose your gear" stakes are already the
  punishment; the death beat itself shouldn't compete with that.
- **Audio:** Ambient bed and breathing loop cut hard to silence, then a single
  low tone. Consistent with §0's silence-as-cue pillar.

### Extraction Activation Started / Cancelled / Complete
- **Trigger:** `ExtractionPoint.OnActivationStarted` / `OnActivationCancelled` /
  `OnExtractionComplete` (8s hold, cancels if player leaves the zone).
- **Visual:** Started: a warm light beacon effect at the extraction point,
  intensifying over the 8s hold — the one "safe/warm" visual color note against
  the otherwise cold/murky palette, so it reads as sanctuary. Cancelled: light
  guttering out abruptly. Complete: brief bright flash + rising bubble/light
  shaft as the ascent begins.
- **Audio:** Started: a rising, hopeful tone (contrast deliberately with every
  other cue in this doc, which are all tension-building). Cancelled: the tone
  drops/sours. This is the only place in the game's VFX language that should
  feel actively good — worth protecting that contrast rather than reusing tense
  stingers here.

---

## 7. Lift Bag Delivery (`Systems/LiftBalloon.cs`) — lower priority

- **Trigger:** `OnReleased` / `OnPopped` / `OnDelivered`.
- **Visual/Audio:** Released: a stream of rising bubbles following the balloon's
  ascent. Popped (hit obstacle or harpooned): a burst + falling-loot scatter,
  reuse the sediment particle system rather than a bespoke one. Delivered: a
  short "cargo secured" chime at the surface, audible even underwater as a muffled
  thud so players below know it made it.
- **Perf:** low priority relative to the fear beats above — simple particle
  reuse is enough, don't invest custom shader work here.

---

## 8. Not yet buildable — flagged, spec is forward-looking only

These two systems from the original hook list **do not exist in the codebase
yet** (confirmed via file search — no `BoundaryEncounter.cs` or
`SanitySystem.cs` under `Assets/_Project/Scripts/`). Specs below describe intent
for whenever they're built; they are not actionable today.

### Boundary Encounter — "left the map" fear sequence
- **Needs:** a new `BoundaryEncounter.cs` (or similar) exposing
  `OnBoundaryCrossed` / `OnBoundaryReturned` — doesn't exist yet.
- **Intent (per brief):** murky water + shark swarm + heartbeat + underwater
  silence. Escalation order matters: cut ambient audio first (silence, per §0),
  *then* bring in the heartbeat and visual murk thickening, then reveal
  multiple shark silhouettes at the edge of visibility (plural — distinct from
  the single-shark Circling tell in §1, this should read as "you are somewhere
  you shouldn't be," not "a shark found you"). On `OnBoundaryReturned`, reverse
  in the same order, but slower — relief should feel earned, not instant.
- **Flag:** this implies a "shark swarm" — multiple SharkBehavior instances or a
  distinct swarm behavior — spawning/despawning at boundary edges. That's a
  gameplay/spawning decision (do these use real SharkBehavior instances with
  full AI, or are they cheaper non-attacking visual-only sharks?) — not mine to
  decide, flagging for the architect role.

### Sanity System — hallucinations (cave-diving psychological system)
- **Needs:** a new `SanitySystem.cs` exposing `OnMildHallucinationTriggered` /
  `OnSevereHallucinationTriggered` — doesn't exist yet.
- **Intent (per brief):** fake shark silhouette, phantom sounds. Mild: a
  silhouette glimpse in the exact visual language of §1's Circling tell (reuse
  that language deliberately — the player should have a genuine moment of "wait,
  is that real?" because it looks identical to a real tell) but with no actual
  drone/audio buildup behind it, and no real SharkBehavior instance driving it.
  Severe: layer in a phantom sound cue (a bite-impact sound, or a whispered
  panicked voice) with no visual at all, or vice versa — the mismatch between
  channels (visual with no matching audio, or audio with no source) is the scare,
  not intensity.
- **Flag:** what drives severity thresholds (sanity meter? depth/time-based?) is
  a gameplay decision that doesn't exist yet — flagging rather than assuming a
  design.

---

## Naming corrections vs. the original brief

The role brief listed some event names from an earlier assumption pass. Checked
against current code — use these:

| Brief said | Actual event | File |
|---|---|---|
| `PlayerEquipmentState.OnMaskRemoved` | `OnMaskKnockedOff` | `Systems/PlayerEquipmentState.cs` |
| `PlayerEquipmentState.OnMaskRecovered` | `OnMaskRecovered` (correct as-is) | `Systems/PlayerEquipmentState.cs` |

`SharkBehavior.OnCirclingStarted` / `OnFleeStarted` and `BotDiverAI.OnAttackPerformed`
/ `OnFleeStarted` all match the brief exactly. `PanicState.OnPanicStarted` /
`OnPanicEnded` match exactly.
