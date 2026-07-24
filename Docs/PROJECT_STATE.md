# PROJECT STATE — UnderwaterExtraction

> **AI SESSION KURALI:** Bu projede çalışan her AI (Claude Code, Gemini, ChatGPT, vs.)
> session'a başlarken **SADECE bu dosyayı** okusun. `CONTEXT.md`, `ROADMAP.md`,
> `MAP_DESIGN.md` gibi diğer Docs/ dosyaları yalnızca o spesifik konuda derin
> tasarım çalışması yapılacaksa açılmalı — her seferinde tüm repoyu baştan
> okumak gereksiz token/zaman kaybıdır. Bu dosya her sistem tamamlandığında
> güncellenir; tek doğruluk kaynağı budur.

**Son güncelleme:** 2026-07-24
**Mevcut faz:** Phase 1 — Core Gameplay Loop ✅ (kod tamam, sahne kurulumu + test kaldı) + Phase 2 AI başladı

---

## ✅ Tamamlanan Sistemler

| Sistem | Dosya(lar) | Not |
|---|---|---|
| PlayerController v0.1 | `Core/PlayerController.cs` | Rigidbody buoyancy, yaw/pitch ayrımı, kamera pitch'i `PlayerController` içinde (`HandleRotation()`) yönetiliyor — ayrı bir CameraLook.cs yok |
| OxygenSystem | `Systems/OxygenSystem.cs` | Depth curve, movement/combat çarpanları, air pocket, `RefillOxygen()` + `RefillFromSpareTank()`, zone effect stack'i (`ApplyZoneEffect`/`RemoveZoneEffect`) |
| PanicState | `Systems/PanicState.cs` | v2 — eşik tabanlı: O2 ≤ panicThreshold otomatik panik başlatır/bitirir (`OxygenSystem.OnOxygenChanged` dinler), `MovementSpeedMultiplier`/`AimSwayMultiplier`/`AdsStabilityMultiplier`, `UseAdrenalineShot()` coroutine ile paniği sıfırlar + süreli bağışıklık |
| Harpoon Combat | `Combat/HarpoonWeapon.cs`, `HarpoonProjectile.cs`, `HarpoonBlocker.cs` | Ammo sayacı, at-doldur, saplanma + `Systems/HarpoonPickup.cs` ile toplama. `HarpoonProjectile` artık "Head" tag'li collider'a çarpınca `SharkBehavior.HandleHeadShot()` çağırıyor (kafa vuruşu = kalıcı kaçış). `HarpoonBlocker` bloklanan zıpkını + kendini scrap'e çevirip yok ediyor. Panic-aim-spread coupling bu turda kaldırıldı (PanicState artık `AimAccuracyMultiplier` expose etmiyor; nişan sallantısı `AimSwayMultiplier`/`AdsStabilityMultiplier` üzerinden ayrı bir kamera/nişan script'ine devredilecek — henüz yok) |
| InjurySystem | `Core/InjurySystem.cs` | v2 — Severity/float yerine ayrık `InjuryType` enum'u (None/SharkBite/JellyfishSting/Barotrauma/**KnifeWound**). `ApplyInjury(type)` tek yaralanmayı kilitler, `sharkBleedDps`/`knifeBleedDps` ile kanama, `StartBandage()`/`InterruptBandage()`. **Not:** bu sürümde hasar alınca bandaj otomatik kesilmiyor (eski davranıştı, v2'de yok — istenirse `PlayerHealth.OnHealthChanged`'a tekrar bağlanabilir) |
| RefillStation | `Systems/RefillStation.cs` | O2 + zıpkın dolumu, E etkileşim, GetComponent/GetComponentInChildren fallback'li |
| PlayerHealth / IDamageable | `Core/PlayerHealth.cs`, `Core/IDamageable.cs` | Hasar arayüzü + can sistemi (Interfaces/ ve eski Systems/ konumlarından Core/'a taşındı) |
| PlayerControllerIntegration | `Core/PlayerControllerIntegration.cs` | Rigidbody hızından idle/swim/sprint, combat timer (OnFired+5sn), derinlik `PlayerController.DepthBelowSurface` ile paylaşılıyor (OxygenSystem/buoyancy aynı yüzey varsayımını kullanır), SpeedMultiplier = panic × injury × bandage × **ağırlık** (`weightSpeedCurve`, `OnOverweight` event'i) |
| Grid Inventory | `Systems/InventorySystem.cs` | Grid footprint (`SO_ItemData.GridWidth/GridHeight` kadar hücre), `occupancy` bool grid ile yerleşim kontrolü, E ile pickup / Q ile drop, stack'leme, `UseSelectedConsumable()` ile Bandage/AdrenalineShot/SpareTank'ı InjurySystem/PanicState/OxygenSystem'e delege eder, `ConsumeSlot()` (drop spawn'sız tüketim — LiftBagDeployer kullanır), `ClearAll()`, `GetTotalWeight()` |
| Item SO'ları | `Systems/SO_ItemData.cs`, `SO_TankData.cs`, `SO_HarpoonData.cs`, `SO_ConsumableItemData.cs` | Hepsi `SO_ItemData`'dan türetilmiş; GridWidth/Height + Rarity alanları var. Data/ klasöründen Systems/'e taşındı |
| StashSystem | `Systems/StashSystem.cs` | Kalıcı depo, tier upgrade (20→40), `Slots` UI hook, `WithdrawTo()`, JSON save/load (`ItemDatabase` + `SaveSystem` üzerinden, her değişimde otomatik kayıt) |
| Save/Load | `Systems/SaveSystem.cs`, `Systems/ItemDatabase.cs` | Jenerik JSON altyapısı (`Application.persistentDataPath`), item'lar isimle çözülüyor |
| LootManager v2 | `Systems/LootManager.cs`, `LootSpawnPoint.cs`, `SO_LootTableData.cs` | Marker tabanlı, nokta başına table+density (Floor1/2/3: %30/%60/%90), RarityWeight weighted random |
| ExtractionPoint | `Systems/ExtractionPoint.cs` | 8sn aktivasyon, alandan çıkınca iptal, kendini `RunManager.Instance`'a kaydeder (Extraction/ klasöründen Systems/'e taşındı) |
| RunManager v3 | `Systems/RunManager.cs` | Extraction/ölüm akışı StashSystem'li; event'ler **`ResultPayload` taşır** (snapshot `ClearAll()`'dan ÖNCE alınır) |
| GameFlowController | `Core/GameFlowController.cs` | Hideout/Diving/Results state, sahne geçişi (`SceneManager`), `BeginDive()`, `LastResult` `RunManager` payload'ından dolar |
| Lift Bag Delivery | `Systems/LiftBalloon.cs`, `LiftBagDeployer.cs` | Derinliğe bağlı yükseliş süresi (30sn + 0.5sn/m), deploy/yükleme kendi O2'sinden maliyetli, Obstacle layer'ına takılma veya zıpkınla (IDamageable) patlama, düşen loot dibe saçılıp tekrar toplanabilir WorldItemPickup olur |
| Stash UI | `UI/GridInventoryUI.cs`, `UI/DraggableItemIcon.cs`, `UI/StashMenuUI.cs` | Runtime'da çizilen grid panel, drag & drop ile envanter↔stash transferi, item'lar w×h kaplar. Panel-içi yeniden dizme (Tarkov tarzı) henüz yok |
| DrowningHandler | `Systems/DrowningHandler.cs` | O2 bitince (`OnOxygenDepleted`) saniyede ayarlanabilir boğulma hasarı, O2 > 0 olunca durur |
| AirPocketVolume | `Systems/AirPocketVolume.cs` | Trigger alanı — `OxygenSystem.EnterAirPocket()`/`ExitAirPocket()` tetikler (bu alanda O2 tüketilmez) |
| SharkBehavior (AI) | `AI/SharkBehavior.cs` | Açlık döngüsü (satiety decay + periyodik feeding denemesi) + state machine (Patrol/Feeding/Circling/Attacking/Pursuing/Fleeing). Circling = tek görsel "tell" (4-8sn). Hit-zone: "Head" tag'li collider vuruşu kalıcı kaçış, gövde vuruşu geçici kaçış + tek geri dönüş hakkı. Kan lure: oyuncuda `InjuryType.SharkBite` **veya `InjuryType.KnifeWound`** varsa saldırganlık eşiği düşer. **Henüz sahnede kurulmadı/test edilmedi** — prefab, "Head" child collider + tag, Rigidbody kurulumu gerekiyor |
| BotDiverAI (AI) | `AI/BotDiverAI.cs`, `Systems/SO_BotDiverData.cs` | Rakip dalgıç. Raycast line-of-sight + oyuncunun bakış açısına göre yüzyüze (bot kaçar) / arkadan (bot saldırır) ayrımı. 3 tier: **Yellow** (vur-kaç, CatchWindow içinde yakalanırsa çok yavaşlar), **Red** (vurur, alanı terk etmez, LurkRadius içinde dolaşır), **Spear** (menzilli, `HarpoonProjectile` kullanır, mesafe korur). Yellow/Red saldırı türü rastgele: `InjuryType.KnifeWound` / `PlayerEquipmentState.KnockOffMask()` / `PlayerEquipmentState.PullRegulator()`. Ölünce `SO_BotDiverData.DeathLootTable`'dan ağırlıklı tek item düşürür. **Henüz sahnede kurulmadı/test edilmedi** |
| PlayerEquipmentState | `Systems/PlayerEquipmentState.cs` | BotDiverAI'nin gear-fear hedefi. `KnockOffMask()` → O2 zone-effect artışı + sahneye `MaskPickup` düşürür (zamanla kendi kendine düzelmez); oyuncu maskeyi bulup E'ye basınca `BeginMaskRecovery()` → `RhythmRecoveryQTE` başlar, tamamlanınca zone effect kalkar + kaçırılan vuruş başına ek O2 kaybı uygulanır. `PullRegulator()` → anlık O2 kaybı (`RefillFromSpareTank` negatif), ayrı kurtarma akışı yok |
| RhythmRecoveryQTE | `Systems/RhythmRecoveryQTE.cs` | Maske/regülatör takma için genel amaçlı ritim mini-oyunu — N beat, her biri için kısa vuruş penceresi (F), kaçırılan vuruş asla dizi'yi başarısız etmez, sadece `OnSequenceCompleted(missedCount)` ile dışarıya bildirilir. `PlayerEquipmentState.BeginMaskRecovery()`'ye bağlı |
| MaskPickup | `Systems/MaskPickup.cs` | Suya düşen maske — trigger + E deseni (RefillStation/HarpoonPickup ile aynı), sahibi `PlayerEquipmentState.KnockOffMask()` tarafından instantiate edilip `Init()` ile bağlanır, E'ye basınca `owner.BeginMaskRecovery()` çağırır |
| PlayerLimbHitHandler | `Systems/PlayerLimbHitHandler.cs` | "Limb" tag'li child collider'a eklenir. `HarpoonProjectile` (BotDiverAI Spear tier dahil) bu tag'e çarpınca `HandleLimbHit()` çağrılır: `InjuryType.KnifeWound` uygular (yavaşlama + SharkBehavior kan lure) + anlık O2 kaybı |

## ⏭️ Sıradaki Öncelik — Phase 2

Phase 1 kod tarafı fiilen TAMAMLANDI. Kalan Phase 1 işi: sahnede kurulum + derleme testi.

1. Map blockout (Floor 1/2/3, spawn point'ler, obstacle'lar — LiftBalloon'un `Obstacle` layer kontrolü için de gerekli)
2. SharkBehavior'ı sahnede kur ve playtest et (prefab + "Head" tag + Rigidbody + Player tag kontrolü)
3. BotDiverAI'yı sahnede kur ve playtest et (prefab + "Limb" tag + `SO_BotDiverData` asset'leri her tier için oluşturulmalı — henüz yok)
4. Panic-aim coupling'in yeni PanicState API'sine (AimSwayMultiplier/AdsStabilityMultiplier) göre bir nişan/kamera script'ine yeniden bağlanması — HarpoonWeapon şu an bundan bağımsız çalışıyor

## ⚠️ Bilinen Sorunlar / Kurulum Checklist

- `depthConsumptionCurve` (OxygenSystem) inspector'da elle ayarlanmalı
- Tags & Layers: **"Pickup"**, **"Obstacle"** layer'ları ve **"Head"**, **"Limb"**, **"Player"** tag'leri manuel oluşturulmalı
- `ItemDatabase` asset'i oluşturulmalı, tüm item SO'ları eklenmeli, `StashSystem`'e referans verilmeli (yoksa save yüklenemez, sadece uyarı loglar)
- Grid envanter sonrası SO asset'lerde `GridWidth/GridHeight` + `Rarity` doldurulmalı (default 1x1/Common)
- Stash UI için Canvas'a EventSystem + GraphicRaycaster (Unity otomatik ekler)
- Panel içi drag-rearrange (Tarkov tarzı hücre değiştirme) yok — Phase 2 polish
- InjurySystem v2'de bandaj, hasar alınca artık otomatik kesilmiyor (eski `InterruptBandage()` tetikleyicisi kaldırıldı) — istenirse yeniden bağlanmalı
- SharkBehavior ve BotDiverAI sahnede hiç kurulmadı — prefab/collider/tag setup + playtest gerekiyor
- `PlayerEquipmentState.maskPickupPrefab` inspector'da atanmalı — atanmazsa `KnockOffMask()` sadece uyarı loglar, maske hiç bulunamaz (kalıcı O2 cezası kalır çünkü kurtarma akışı hiç başlamaz)
- `SO_BotDiverData` asset instance'ları (Yellow/Red/Spear için ayrı ayrı) henüz Editor'de oluşturulmadı
- BotDiverAI'nin `PlayerLimbHitHandler`'ı bulabilmesi için oyuncu prefab'ında "Limb" tag'li bir child collider olması gerekiyor

## 🧱 Kalıcı Mimari Kararlar (değişmeyecek varsayımlar)

- Unity 2022.3 LTS + URP, `rb.velocity` (linearVelocity DEĞİL — Unity 6 API'si kullanılmıyor)
- Hareket: Rigidbody + AddForce tabanlı (CharacterController KULLANILMIYOR)
- Motor: Unity'de kalınıyor, Godot'a geçiş değerlendirilip reddedildi
- Naming: PascalCase class/method, camelCase variable, `SO_` prefix, `PR_` prefix (prefab)
- Event-driven loose coupling; etkileşim deseni her yerde trigger + E (`interactKey` serialize)
- Klasörleme: `Core/` (player, entegrasyon, can, hasar arayüzü, oyun akışı), `Systems/` (SO item verileri dahil tüm oyun sistemleri), `Combat/`, `AI/`, `UI/` — `Data/`, `Interfaces/`, `Extraction/` klasörleri kaldırıldı, içerikleri yukarıdakilere taşındı
- Üçüncü parti açık kaynak adapte edilirken önce lisans kontrol edilir (MIT/Apache tercih), kod birebir kopyalanmaz — mantık bizim mimariye göre yeniden yazılır (ilk uygulama: `kukumberman/unity-grid-inventory`, MIT)
- Kapsam genişlemesi (roadmap'te olmayan yeni sistem) hiçbir AI'ın inisiyatifiyle değil, önce PROJECT_STATE.md üzerinden onaylanarak yapılır

---

*Bu dosyayı her yeni sistem eklendiğinde/değiştiğinde güncelle: ✅ tabloya taşı,
⏭️ listesinden çıkar, yeni ⚠️/🧱 varsa ekle. `CONTEXT.md`'yi sadece büyük mimari
değişikliklerde güncellemen yeterli.*
