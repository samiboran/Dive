# UnderwaterExtraction — Daily Devlog

> **PURPOSE:** Dated, short development summaries for sharing progress publicly
> (X/Twitter, indie dev community). Written in English.
>
> **ROUTINE (any AI session can run this, including a separate/free Claude session):**
> 1. Check `git log` (or the day's changes/commits) since the last dated entry below.
> 2. Append ONE new entry at the TOP of the log (newest first), following the exact
>    template below. Do not rewrite or edit older entries.
> 3. Keep the summary factual — what was built, fixed, or decided. 3-6 bullet points max.
>    No hype language, no filler.
> 4. Keep the X post under 280 characters (hashtags included). It should read like a
>    real solo-dev progress update, not marketing copy.
> 5. Suggested hashtag pool (mix 2-4 per post, vary them — don't reuse the exact same
>    set every day): #gamedev #indiedev #indiegame #unity3d #madewithunity #soloDev
>    #devlog #extractionshooter #gamedesign
> 6. If nothing meaningfully shipped that day (pure discussion/planning), it's fine to
>    skip the entry rather than force one.
>
> **TEMPLATE:**
> ```
> ## YYYY-MM-DD
>
> **Summary:**
> - ...
> - ...
>
> **X post:**
> > [draft tweet text] #hashtag1 #hashtag2
>
> ---
> ```

---

## 2026-07-25

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

**X post:**
> Solo devlog: enemy diver AI is in — they'll flee if they see you coming, but jump you from behind. Also decided to stick with full 3D after seriously considering a 2D pivot. Onward. #gamedev #soloDev #indiegame

---
