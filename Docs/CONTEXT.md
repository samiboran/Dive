# UnderwaterExtraction - Development Context & Session Summary

> **Not:** Güncel sistem durumu ve "şu an ne bitti / ne bekliyor" için
> [`Docs/PROJECT_STATE.md`](./PROJECT_STATE.md) tek doğruluk kaynağıdır.
> Bu dosya (CONTEXT.md) yalnızca büyük mimari kararlar/derin tasarım
> referansı için tutulur, her session'da okunması gerekmez.

## 📋 Session Overview

**Date:** 2026-07-24  
**Session Type:** Concept + Architecture + MVP Planning  
**Project:** UnderwaterExtraction (Codename: Depth Extraction)  
**Engine:** Unity 2022.3 LTS (URP)  
**Language:** C#

---

## 🎮 Game Concept Summary

**Elevator Pitch:** *Escape from Tarkov underwater. Hardcore extraction shooter where oxygen is your timer, depth is your risk, and every dive is a run where death means losing your gear.*

**Unique Selling Points:**
- Oxygen as primary tension mechanic (not just health)
- Decompression sickness risk (can't surface too fast)
- Buoyancy and underwater physics affect combat/movement
- Vertical level design (depth = risk/reward)
- Gear fear translated to diving equipment (regulators, BCDs, tanks)

**Core Loop:** PLAN → GEAR UP → DIVE → LOOT/FIGHT → EXTRACT → SELL/UPGRADE

---

## 🏗️ Project Architecture

### Folder Structure (Unity)

```
Assets/_Project/
├── Scripts/
│   ├── Core/          - PlayerController, PlayerControllerIntegration, CameraSystem, InputManager
│   ├── Systems/       - OxygenSystem, PlayerHealth, RefillStation, InventorySystem, WorldItemPickup, PanicState, InjurySystem, AdrenalineItem, Loot, Economy, Decompression
│   ├── Combat/        - HarpoonWeapon, HarpoonProjectile, HarpoonBlocker, HarpoonPickup
│   ├── Data/          - SO_ItemData, SO_ConsumableItemData, SO_HarpoonData, SO_TankData (ScriptableObjects)
│   ├── Interfaces/    - IDamageable
│   ├── AI/            - BotDiverAI, SharkBehavior, PatrolSystem, AIPerception
│   ├── Extraction/    - ExtractionPoint, RunManager, SessionTimer, RaidInitializer
│   └── UI/            - HUDController, InventoryUI, MenuSystem, NotificationManager
├── Prefabs/           - PR_ prefix (e.g., PR_Player)
├── Scenes/            - MainMenu, Map_SunkenShip_01, Map_Reef_01, Hideout
├── Art/               - Models, textures, animations, materials
├── Audio/             - SFX, ambient underwater sounds, music
├── Materials/         - URP materials for underwater environment
└── Physics/           - Physics materials, buoyancy profiles

Docs/
├── CONTEXT.md         - Architecture summary for AI sessions
├── ROADMAP.md         - Weekly/phase-based development roadmap
├── MAP_DESIGN.md      - Level design notes, blockout specs
└── GAME_DESIGN.md     - Core mechanics, economy balancing
```

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `PlayerController` |
| Variables | camelCase | `currentOxygen` |
| Methods | PascalCase | `TakeDamage()` |
| Scriptable Objects | SO_ prefix | `SO_WeaponData` |
| Prefabs | PR_ prefix | `PR_Player` |
| Scenes | Map_ prefix | `Map_SunkenShip_01` |
| Git Branches | feature/descriptive-name | `feature/harpoon-system` |

---

## 🗺️ First Map: SunkenShip_01 ("The Dredge")

**Theme:** Cargo shipwreck, 3 floors, vertical progression  
**Dimensions:** ~50m x 30m x 15m (depth)

### Floor Breakdown

| Floor | Name | Risk | Loot Density | AI | Special |
|-------|------|------|--------------|----|---------|
| 1 | Upper Deck | Low | Low (30%) | None | Safe Exit extraction point |
| 2 | Middle Deck | Medium | Medium (60%) | 1-2 Sharks | Loop corridors, dim lighting |
| 3 | Engine Room | High | High (90%) | Bot Diver + Shark | 2x O₂ consumption, currents, Risky Exit |

### Extraction Points

1. **Safe Exit (Floor 1):** Slow ascent, no risk, low reward
2. **Risky Exit (Floor 3):** Current-swept escape, fast but damage risk

### Loot Categories
- **Common:** Scrap metal, Diving mask, Rope
- **Uncommon:** Underwater camera, Harpoon tip, Dive computer
- **Rare:** Antique coin, Military encryption key, Experimental rebreather

---

## 🎯 Roadmap (Flexible, Phase-Based)

### Phase 0: Foundation (2-3 weeks)
- [ ] Repo & folder structure setup
- [ ] Unity project (2022.3 LTS + URP)
- [ ] New Input System integration
- [ ] PlayerController v0.1 (basic swim/move)
- [ ] First-person underwater camera
- **Deliverable:** Playable character moving in water

### Phase 1: Core Gameplay Loop (4-6 weeks)
- [ ] Oxygen system (depletion, refill, UI)
- [ ] Basic inventory (5 slots, pickup, drop)
- [ ] Loot spawn system (3 item types)
- [ ] Single extraction point
- [ ] Death = lose gear (stash persists)
- **Deliverable:** Complete dive-extract loop

### Phase 2: First Map (6-8 weeks)
- [ ] Blockout: Sunken Ship 3 floors (greybox)
- [ ] Procedural loot placement
- [ ] Basic AI: Shark patrol + Bot diver
- [ ] 2 extraction points (safe vs risky)
- **Deliverable:** Playable SunkenShip_01 with AI

### Phase 3: Economy & Progression (Open-ended)
- [ ] Stash system
- [ ] Vendor buy/sell interface
- [ ] Gear upgrades (3 tiers)
- [ ] Insurance system
- **Deliverable:** Full economic loop

### Phase 4: Polish & Demo (Open-ended)
- [ ] Underwater audio
- [ ] UI/UX pass
- [ ] Balancing
- [ ] Post-processing (fog, caustics, distortion)
- **Deliverable:** Steam/itch.io demo build

---

## 🔧 Code Architecture - Current Status

### PlayerController v0.1 (In Progress)

**Location:** `Assets/_Project/Scripts/Core/PlayerController.cs`

**Components Required:**
- Rigidbody (UseGravity: false, Drag: 3, AngularDrag: 1)
- PlayerInput (Actions: PlayerInputActions, Behavior: Send Messages)
- PlayerController (custom script)

**Input Actions:**
| Action | Type | Bindings |
|--------|------|----------|
| Move | 2D Vector | WASD |
| Vertical | Axis (float) | Space (+), Ctrl (-) |
| Look | 2D Vector | Mouse Delta |

**Physics Settings:**
- Buoyancy Force: 2.0
- Water Drag: 3.0
- Swim Speed: 5.0
- Vertical Speed: 3.0
- Look Speed X/Y: 2.0
- Pitch Limits: -80 to 80

**Known Issues (v0.1):**
- [x] Buoyancy feels unbalanced (down too hard) - rewritten as depth-based lerp (weaker near surface, stronger at depth), needs in-editor playtesting to tune values
- [x] Pitch rotates body (should be camera-only) - camera now rotates independently on X, body only rotates on Y
- [ ] No underwater visual effects
- [ ] No audio feedback
- [x] No momentum/inertia - velocity now eases toward target via `acceleration`/`maxVelocity`

**v0.2 Planned Improvements:**
- [x] Separate camera pitch from body rotation - done in v0.1
- [x] Dynamic buoyancy based on depth/gear - depth portion done in v0.1 (`waterSurfaceHeight`/`maxDepth` lerp); gear-based modifier still open
- [ ] Underwater post-processing stack
- [ ] Camera sway and head bob
- [x] Momentum-based movement - done in v0.1

### Planned Systems

| System | Category | Key Mechanics | Status |
|--------|----------|---------------|--------|
| OxygenSystem | Systems | Depletion over time, depth affects consumption, refill stations | ✅ Implemented |
| PlayerHealth | Systems | Basic damage/heal, IDamageable | ✅ Implemented |
| RefillStation | Systems | Trigger + E interact, refills oxygen and harpoons | ✅ Implemented |
| PlayerControllerIntegration | Core | Bridges PlayerController with Oxygen/Panic/Injury (movement state, combat timer, speed multiplier) | ✅ Implemented |
| HarpoonWeapon | Combat | Fire/reload, ammo count, events | ✅ Implemented |
| HarpoonProjectile | Combat | Flight, hit detection, sticks to surfaces, pickup | ✅ Implemented — `PickUp()` has no caller yet (needs an interact script) |
| SO_HarpoonData | Data | Damage/FireRate/ReloadDuration/Range/ProjectileSpeed/ProjectilePrefab | ✅ Implemented |
| SO_TankData | Data | CapacitySeconds per tank type | ✅ Implemented |
| PanicState | Systems | Panics on critical oxygen or taking damage, decays over time, slows movement while panicked | ✅ Implemented (v0.1 default: panic only slows, doesn't boost — tune in Inspector) |
| InjurySystem | Systems | Movement penalty scaling with missing health + bandaging (heals over time, blocks movement further) | ✅ Implemented |
| HarpoonBlocker | Combat | Intercepts incoming harpoons before they reach the entity behind it | ✅ Implemented (VFX hook only, no shield/durability mechanic yet) |
| DecompressionSystem | Systems | Ascent speed tracking, sickness penalty, required stops | ⬜ Planned |
| InventorySystem | Systems | Grid/slot-based, weight affects movement | ⬜ Planned |
| LootManager | Systems | Procedural spawn, rarity weights, zone-based pools | ⬜ Planned |
| EconomyManager | Systems | Buy/sell, stash value, insurance, flea market | ⬜ Planned |
| SharkBehavior | AI | Patrol paths, aggro radius, attack patterns | ⬜ Planned |
| BotDiverAI | AI | Harpoon combat, cover seeking, looting | ⬜ Planned |
| ExtractionPoint | Extraction | Activation timer, contested zones, conditional extraction | ⬜ Planned |
| RunManager | Extraction | Session init, gear loading, death handling | ⬜ Planned |

All scripts referenced by the Core/Systems/Combat code now exist in the repo — no known compile blockers. `PanicState`/`InjurySystem`'s numeric defaults (rise/decay rates, speed floors, bandage timing) are placeholders and need in-editor playtesting to tune.

---

## 🔄 Git Workflow

### Branches
- `main` - Stable, playable demo builds only
- `dev` - Working features, tested but not release-ready
- `feature/*` - Active development (e.g., `feature/harpoon-system`)

### Commit Conventions
| Prefix | Purpose |
|--------|---------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `docs:` | Documentation update |
| `refactor:` | Code restructuring |
| `chore:` | Maintenance, asset imports |

**Merge Rules:** Feature → Dev (working demo) → Main (phase milestone)

---

## 🎯 Next Session Priorities

1. [ ] Create Unity project (2022.3 LTS + URP)
2. [ ] Set up Input Actions asset (PlayerInputActions)
3. [ ] Implement PlayerController v0.1
4. [ ] Create test scene (cube water + player)
5. [ ] Test movement and document "feel" issues
6. [ ] Commit: `feat: PlayerController v0.1 with underwater movement`

---

## 📝 Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Unity over Unreal | Asset availability, C# familiarity |
| URP over Built-in | Underwater lighting flexibility |
| Single-player first | Scope manageable for solo dev |
| Low-poly art style | Solo feasible |
| Flexible roadmap | Part-time development reality |
| Unity'de kalındı (Godot'a geçilmedi) | Repo, roadmap, naming convention ve PlayerController v0.1 zaten Unity/C# üzerine kurulu; motor değişimi kazanç sağlamadan haftalarca geriye gider |

---

## 🧠 Game Design Pillars

1. **Tension** through resource management (oxygen = time + life)
2. **Risk/Reward** through vertical level design
3. **Gear fear** translated to equipment loss
4. **Methodical planning** vs. adrenaline execution

### Tarkov Adaptations
| Tarkov | UnderwaterExtraction |
|--------|---------------------|
| Raid | Dive operation |
| Extract | Decompression + surface |
| Stash | Dive base / equipment locker |
| Flea Market | Dive shop / black market |
| Scav | Emergency dive (random gear) |
| Gear Fear | Equipment loss (regulator, BCD, tank) |
| Insurance | Dive shop insurance (recover basic) |

---

## 📊 Session Completion Status

### ✅ Completed
- [x] Game concept defined
- [x] Folder structure designed
- [x] Git workflow established
- [x] Roadmap created
- [x] First map designed
- [x] PlayerController v0.1 planned
- [x] Input System documented
- [x] Naming conventions established
- [x] CONTEXT.md structure defined
- [x] PlayerController.cs v0.1 implemented (`Assets/_Project/Scripts/Core/PlayerController.cs`)
- [x] OxygenSystem, PlayerHealth, RefillStation implemented (`Scripts/Systems/`)
- [x] HarpoonWeapon, HarpoonProjectile implemented (`Scripts/Combat/`)
- [x] PlayerControllerIntegration implemented, wired into PlayerController via `SpeedMultiplier` (`Scripts/Core/`)
- [x] IDamageable interface implemented (`Scripts/Interfaces/`)
- [x] SO_HarpoonData, SO_TankData implemented (`Scripts/Data/`)
- [x] PanicState, InjurySystem implemented (`Scripts/Systems/`)
- [x] HarpoonBlocker implemented (`Scripts/Combat/`)

### 🔄 In Progress
- [ ] Unity project creation (2022.3 LTS + URP) - scripts exist, project itself still needs to be created via Unity Hub/Editor
- [ ] Input Actions asset creation (PlayerInputActions.inputactions)
- [ ] Test scene creation and movement testing
- [ ] Interact script for picking up stuck `HarpoonProjectile`s (calls `PickUp()` — currently has no caller)
- [ ] Playtest and tune PanicState/InjurySystem numeric defaults (rise/decay rates, speed floors, bandage timing)

### ⏭️ Next Session
- Create the actual Unity project (2022.3 LTS + URP) and drop this repo's `Assets/` into it
- Create `SO_HarpoonData`/`SO_TankData` asset instances (Assets menu) and assign them on prefabs
- Input Actions asset setup (Move, Vertical, Look)
- Wire up PR_Player prefab (Rigidbody, Capsule Collider, PlayerInput, PlayerController, PlayerControllerIntegration, OxygenSystem, PlayerHealth, PanicState, InjurySystem, HarpoonWeapon, child Camera)
- Build test scene (water cube + ground) and playtest movement/buoyancy/panic/injury feel

---

*This document serves as the master reference for AI-assisted development sessions. Update CONTEXT.md before ending each session.*
