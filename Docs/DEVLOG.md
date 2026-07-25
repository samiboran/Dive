# UnderwaterExtraction — Daily Devlog

> **PURPOSE:** Dated, short development summaries for sharing progress publicly
> (X/Twitter, indie dev community). The written summary below stays in English
> (repo/docs convention, for future contributors). The **X post text itself is
> written in Turkish** — X auto-translates for other-language readers, and
> Turkish is the primary audience for now.
>
> **ROUTINE (any AI session can run this, including a separate/free Claude session):**
> 1. Check `git log` (or the day's changes/commits) since the last dated entry below.
> 2. Compute the day counter: `N = number of existing entries below + 1` (this is a
>    running post count, not a calendar day count — skipped days don't break the
>    sequence, they just mean no entry was added that day).
> 3. Append ONE new entry at the TOP of the log (newest first), following the exact
>    template below. Do not rewrite or edit older entries.
> 4. Keep the summary factual — what was built, fixed, or decided. 3-6 bullet points
>    max. No hype language, no filler. Written in English.
> 5. Write the X post in Turkish. Prefix it with "Gün N —" (Day N). Keep it under
>    280 characters including hashtags, and make it read like a real solo-dev update,
>    not marketing copy.
> 6. Suggested hashtag pool (mix 2-4 per post, vary them — don't reuse the exact same
>    set every day): #gamedev #indiedev #indiegame #unity3d #madewithunity #soloDev
>    #devlog #extractionshooter #gamedesign
> 7. If more than one post is going out the same day, keep them as separate,
>    standalone posts (not a thread) — each should be readable on its own, since
>    threaded replies get less algorithmic reach than standalone posts.
> 8. If nothing meaningfully shipped that day (pure discussion/planning), it's fine to
>    skip the entry rather than force one.
>
> **TEMPLATE:**
> ```
> ## YYYY-MM-DD — Day N
>
> **Summary:**
> - ...
> - ...
>
> **X post(s):**
> > Gün N — [Turkish post text] #hashtag1 #hashtag2
>
> ---
> ```

---

## 2026-07-26 — Day 2

**Summary:**
- Wrote the VFX/atmosphere direction spec: fog density and color grading scale
  with floor risk, regulator bubble loop acts as the tension metronome, ambient
  removal treated as an effect in its own right
- Constrained VFX to a solo-dev budget: LUT/vignette blends on a small fixed set
  of volumes instead of per-trigger particle prefabs
- Drafted DiveComputerAI Q&A content (keyword-matched, pre-written, no live AI)
  in a deliberately cold telemetry voice; locked its in-world identity as a
  standard-issue unnamed unit
- Added a LORE_BIBLE scaffold so Writer-role sessions stay consistent

**X post(s):**
> Gün 2 — Bugün VFX/atmosfer yönünü spec'ledim: sis yoğunluğu ve renk grading kata göre artıyor, oyuncunun kendi nefes sesi gerilimin metronomu oluyor. Solo-dev bütçesine göre ucuz, tekrar kullanılabilir efektler seçtim. #gamedev #indiedev #devlog

> Gün 2 — Bilek dalış bilgisayarı için soru-cevap içeriği yazdım (gerçek AI değil, önceden yazılmış — kelime eşleşmeli). Soğuk, resmi bir telemetri sesi verdim. Tutarlılık için bir 'lore bible' dosyası da açtım. #indiegame #gamedesign #soloDev

---

## 2026-07-25 — Day 1

**Summary:**
- Implemented BotDiverAI: raycast line-of-sight detection with face-to-face vs.
  from-behind distinction, three behavior tiers (hit-and-run, wound-and-lurk,
  ranged spear), knife/mask/regulator disarm attacks, and death loot drops
- Added a rhythm-based recovery mini-game (RhythmRecoveryQTE) for re-equipping a
  knocked-off mask or pulled regulator — missed beats cost extra oxygen but the
  sequence always completes, never blocks the player
- Fixed two SharkBehavior bugs: the hunger feeding-loop could interrupt the attack
  "tell" and flee states, and aggression could build up forever instead of decaying
- Evaluated switching the whole project to a 2D side-view style (Dave the Diver-ish)
  for faster iteration — decided to stay fully 3D and see the process through
- Started researching art/environment asset pipeline (Unity Asset Store packages,
  AI text-to-3D tools) and confirmed render pipeline compatibility matters a lot
  (URP vs Built-in) before buying anything
- Set up this devlog system and a public sharing plan (X posts, Turkish, "Gün N"
  format, 1-2 posts/day)

**X post(s):**
> Gün 1 — Su altı bir extraction shooter geliştiriyorum, solo. Bugün rakip dalgıç AI'ını bitirdim (seni görürse kaçar, arkandan yakalarsa saldırır), köpekbalığı davranışındaki birkaç hatayı düzelttim. Süreci buradan paylaşacağım. #gamedev #indiedev #soloDev

> Gün 1 — 2D'ye geçmeyi ciddi ciddi düşündüm (Dave the Diver tarzı, daha hızlı olurdu) ama 3D'de kalmaya karar verdim — tek başıma bunu ne kadar sürede bitirebilirim, süreci görmek istiyorum. #indiegame #gamedev #devlog

---
