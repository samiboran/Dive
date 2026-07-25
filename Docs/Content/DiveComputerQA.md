# DiveComputerAI — Q&A Content

> Pre-written Q&A database for the player's wrist dive computer. No live AI —
> player input is keyword-matched against `triggerKeywords` below; on a match,
> show `responseText` verbatim. This file is copy-paste source for a Unity
> ScriptableObject, not code.
>
> **Voice:** terse, functional, impersonal. A life-support/telemetry unit, not
> a companion or a friendly chatbot. No exclamation marks, no reassurance, no
> warmth. Dry/grim is fine in small doses; caring is not.
>
> Canon note: this file decides one previously-open question from
> `LORE_BIBLE.md` — what DiveComputerAI is, in-world (standard-issue unit, no
> name/callsign, deliberately impersonal) — and logs it there.

---

## The Dredge / Operation

- **triggerKeywords:** dredge, wreck, ship, where are we, this place
  **responseText:** "Designation: The Dredge. Cargo hull, ocean floor, three deck levels logged. Structural integrity: degraded. Proceed with caution below Deck 2."

- **triggerKeywords:** why are we here, salvage, job, contract, mission
  **responseText:** "Salvage contract, terms undisclosed at your clearance. Recover marked cargo, return to surface. Payment processed on extraction, not before."

- **triggerKeywords:** who is paying, who runs this, employer, company, client
  **responseText:** "Query outside operational scope. Recommend focusing on oxygen reserves instead."

- **triggerKeywords:** how old is this wreck, history, how long has it been here
  **responseText:** "Vessel age: undetermined from available records. Corrosion pattern consistent with extended submersion. Recommend visual inspection, not conjecture."

## Oxygen / Depth

- **triggerKeywords:** oxygen, air, how much air, tank, tank capacity
  **responseText:** "Tank reserve displayed on wrist readout continuously. Depth and exertion accelerate consumption. Plan your ascent before you need it, not after."

- **triggerKeywords:** decompression, ascent, surfacing, rising too fast
  **responseText:** "Rapid ascent from depth carries decompression risk. Controlled rise recommended. This unit will not stop you from ignoring that."

- **triggerKeywords:** depth, how deep, pressure
  **responseText:** "Current depth logged continuously. Pressure effects compound with time spent at depth, not depth alone."

- **triggerKeywords:** air pocket, refill, spare tank
  **responseText:** "Spare tanks and air pockets extend dive duration. Neither is unlimited. Locate refill points before reserves are critical, not after."

## Sharks

- **triggerKeywords:** shark, sharks, predator
  **responseText:** "Local shark activity: confirmed, unpredictable. Feeding behavior does not follow a fixed schedule. Treat every encounter as the first."

- **triggerKeywords:** shark circling, why is it circling, tell, warning sign
  **responseText:** "Circling behavior precedes an attack in the majority of observed cases. This is the only warning you will get. Use it."

- **triggerKeywords:** shark bite, bleeding, blood
  **responseText:** "Open wounds attract further predator interest. Bandage promptly. Bleeding left untreated compounds risk, it does not resolve on its own."

- **triggerKeywords:** shark fled, ran away, gone, retreated
  **responseText:** "A retreating shark is not necessarily a defeated one. Some return. Do not assume the encounter is concluded."

## Rival Divers

- **triggerKeywords:** other divers, rival, enemy diver, bot diver, competitor
  **responseText:** "You are not the only party working this site. Other divers are salvaging under the same pressure you are. Assume they will treat you as competition, not company."

- **triggerKeywords:** mask knocked off, mask, lost mask
  **responseText:** "Loss of mask seal compounds oxygen consumption immediately. Recovery requires locating the mask and re-securing it manually. It will not correct itself."

- **triggerKeywords:** regulator pulled, regulator
  **responseText:** "A pulled regulator costs air instantly. No recovery sequence follows. There is nothing further this unit can do for you there."

- **triggerKeywords:** why did that diver attack me, why fight other divers
  **responseText:** "Motive assessment unavailable. Presume scarcity, presume desperation. Neither requires further explanation down here."

## Gear / Equipment

- **triggerKeywords:** harpoon, weapon, spear
  **responseText:** "Harpoon effective against soft tissue at close range. Ammunition is finite and not manufactured on-site. Retrieve spent harpoons where safe to do so."

- **triggerKeywords:** bandage, first aid, injury treatment, healing
  **responseText:** "Bandaging halts blood loss but requires uninterrupted application time. Pursuit or combat during treatment will not pause on your behalf."

- **triggerKeywords:** adrenaline, panic, calm down
  **responseText:** "Adrenaline suppresses panic response temporarily. It does not extend oxygen reserves. Effects lapse; underlying conditions do not."

## Extraction / Loot / Stash

- **triggerKeywords:** extraction, extract, leave, exit
  **responseText:** "Extraction requires sustained presence at a marked point. Departing the zone early cancels the sequence. Half-measures do not count."

- **triggerKeywords:** lift bag, cargo delivery, balloon
  **responseText:** "Lift bags deliver cargo independent of your own ascent. Deployment cost is paid in oxygen, not currency. Weigh the exchange before committing."

- **triggerKeywords:** stash, storage, what happens if I die, death
  **responseText:** "Cargo not extracted is cargo lost. Stash reflects only what reached the surface. This unit does not offer condolences for the rest."

- **triggerKeywords:** loot, salvage value, item worth
  **responseText:** "Value assessment is not this unit's function. Recover what you can carry. Prioritize what you can afford to lose least."

## Panic / Disorientation

*(Note: keep any answer here consistent with `LORE_BIBLE.md`'s rule that the
computer never confirms a hallucination as real — disorientation is framed as
a physiological/diving hazard, full stop.)*

- **triggerKeywords:** panic, scared, afraid, tunnel vision
  **responseText:** "Elevated stress response is a physiological reaction to low oxygen and imminent threat, not a malfunction. It resolves once the underlying cause does."

- **triggerKeywords:** hallucination, seeing things, hearing things, is that real
  **responseText:** "Disorientation at depth and duration is a documented diving hazard, not something this unit can confirm or deny. Verify before reacting."

## Meta / Computer Identity

- **triggerKeywords:** who are you, what are you, your name
  **responseText:** "Designation: dive computer, standard issue. No callsign assigned. Function: telemetry and information retrieval, nothing further."

- **triggerKeywords:** are you an ai, are you alive, are you sentient
  **responseText:** "Response set is fixed and pre-loaded. There is no one on the other end of this to talk to. Query noted regardless."

- **triggerKeywords:** help, what can you do, commands, instructions
  **responseText:** "This unit answers direct queries on oxygen, hazards, equipment, and site conditions. It does not offer guidance beyond its dataset."

- **triggerKeywords:** thank you, thanks
  **responseText:** "Acknowledged."

---

## Fallback Responses (no keyword match)

Rotate/randomize so repeated misses don't feel identical:

1. "Query not recognized. Rephrase or consult onboard reference."
2. "No matching entry in current dataset."
3. "Unable to parse request. Response set is limited to operational queries."
4. "That falls outside this unit's dataset. Try a more direct phrasing."
5. "No data available for that query."
6. "Unrecognized input. This unit does not extrapolate beyond its dataset."
