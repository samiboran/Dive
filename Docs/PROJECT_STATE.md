# PROJECT STATE — UnderwaterExtraction

> **AI SESSION KURALI:** Bu projede çalışan her AI (Claude Code, Gemini, ChatGPT, vs.)
> session'a başlarken **SADECE bu dosyayı** okusun. `CONTEXT.md`, `ROADMAP.md`,
> `MAP_DESIGN.md` gibi diğer Docs/ dosyaları yalnızca o spesifik konuda derin
> tasarım çalışması yapılacaksa açılmalı — her seferinde tüm repoyu baştan
> okumak gereksiz token/zaman kaybıdır. Bu dosya her sistem tamamlandığında
> güncellenir; tek doğruluk kaynağı budur.

**Son güncelleme:** 2026-07-24
**Mevcut faz:** Phase 1 — Core Gameplay Loop

---

## ✅ Tamamlanan Sistemler

| Sistem | Dosya(lar) | Not |
|---|---|---|
| PlayerController v0.1 | `Core/PlayerController.cs` | Rigidbody buoyancy (yüzey referanslı, derinliğe göre lerp), yaw/pitch ayrımı (camera-only pitch), momentum/inertia, `SpeedMultiplier` entegrasyonu |
| OxygenSystem | `Systems/OxygenSystem.cs` | Depth curve, movement/combat çarpanları, air pocket desteği |
| PanicState | `Systems/PanicState.cs` | Ters mantık: panikte hız↑ (`MovementSpeedMultiplier`), nişan↓ (`AimAccuracyMultiplier`); adrenalin item ile iptal edilebilir |
| AdrenalineItem | `Systems/AdrenalineItem.cs` | `PanicState.ApplyAdrenaline()`'i tetikler — paniği sıfırlar + geçici bağışıklık verir. InventorySystem henüz yok, şimdilik standalone trigger/`Use()` |
| Harpoon Combat | `Combat/HarpoonWeapon.cs`, `Combat/HarpoonProjectile.cs` | Ammo sayacı, at-doldur, `PanicState.AimAccuracyMultiplier`'a bağlı nişan sapması, suya saplanma + toplama altyapısı |
| HarpoonBlocker | `Combat/HarpoonBlocker.cs` | Gelen zıpkını engeller (şimdilik sadece VFX hook'u, kalkan/dayanıklılık mekaniği yok) |
| InjurySystem | `Systems/InjurySystem.cs` | Cana orantılı hız cezası, bandaj (süre dolunca otomatik iyileştirir), hasar alınca bandaj otomatik kesintiye uğrar |
| RefillStation | `Systems/RefillStation.cs` | O2 + zıpkın dolumu, E ile etkileşim, `GetComponent`→`GetComponentInChildren` fallback (silah player root'ta da child'da da olsa çalışır) |
| PlayerHealth / IDamageable | `Systems/PlayerHealth.cs`, `Interfaces/IDamageable.cs` | Hasar arayüzü + stub can sistemi |
| PlayerControllerIntegration | `Core/PlayerControllerIntegration.cs` | Oxygen/Panic/Injury ↔ hareket köprüsü, combat timer, `SpeedMultiplier` artık `PlayerController.ApplyMovement()`'ta gerçekten çarpılıyor |
| SO_HarpoonData / SO_TankData | `Data/SO_HarpoonData.cs`, `Data/SO_TankData.cs` | `SO_ItemData`'dan türer (envanter/loot uyumlu), `[field: SerializeField]` property stiline geçti (SO_ItemData/SO_ConsumableItemData ile tutarlı). Tank artık `TierName`/`TierLevel` taşıyor, harpoon `HarpoonName` taşıyor — asset instance'ları henüz Editor'de oluşturulmadı |
| SO_ItemData / SO_ConsumableItemData | `Data/SO_ItemData.cs`, `Data/SO_ConsumableItemData.cs` | Envanterdeki tüm item'ların base'i; Consumable alt sınıfı Bandage/AdrenalineShot/SpareTank tiplerini taşır |
| WorldItemPickup | `Systems/WorldItemPickup.cs` | Sahnede duran alınabilir item; InventorySystem trigger'ı bunu bulur, drop edilenler de buradan spawn olur |
| InventorySystem | `Systems/InventorySystem.cs` (v2, grid tabanlı) | Grid footprint (`SO_ItemData.GridWidth/GridHeight` kadar hücre), `occupancy` bool grid ile yerleşim kontrolü, E ile pickup / Q ile drop, stack'leme, `UseSelectedConsumable()` ile Bandage/AdrenalineShot/SpareTank'ı InjurySystem/PanicState/OxygenSystem'e delege eder, `ClearAll()` (RunManager kullanır). Liste API'si (`Slots`) korundu, RunManager/consumable akışı etkilenmedi |
| HarpoonPickup | `Combat/HarpoonPickup.cs` | Saplı zıpkını E ile toplar → `HarpoonProjectile.PickUp()` artık gerçekten çağrılıyor |
| LootManager | `Systems/LootManager.cs` (v2, marker tabanlı), `Systems/LootSpawnPoint.cs`, `Data/SO_LootTableData.cs` | `LootSpawnPoint` marker'ları kendini `LootManager.Instance`'a kaydeder, nokta başına table + density (Floor 1/2/3 ~%30/%60/%90), `RarityWeight`'e göre ağırlıklı rastgele item seçimi, `WorldItemPickup` ile spawn |
| StashSystem | `Systems/StashSystem.cs` | Kalıcı ana depo, tier upgrade (20→40 slot), UI hook: `Slots` + `OnStashChanged`. Şu an bellek-içi; save/load henüz yok |
| ExtractionPoint | `Extraction/ExtractionPoint.cs` | Trigger + aktivasyon timer'ı (alandan çıkarsa iptal), `OnExtractionComplete` event'i |
| RunManager | `Extraction/RunManager.cs` | Dalış başlatma, extraction'da envanter→stash transferi, ölümde envanter kaybı (gear korunur) |

## 🔄 Şu An Devam Eden

- Gerçek Unity projesinin oluşturulması (ProjectSettings, Packages/manifest.json) — repoda şu an sadece `Assets/` altındaki scriptler var

## ⏭️ Sıradaki Öncelik (Phase 1 tamamlanması için)

1. Weight-based carry capacity (InventorySystem.GetTotalWeight() zaten var, hareket/oxygen'e henüz bağlanmadı)
2. Stash save/load (JSON kalıcılık — StashSystem hook'ları hazır)
3. Stash UI (hook noktası hazır: `StashSystem.Slots`)
4. AI: SharkBehavior, BotDiverAI (Phase 2 kapsamı ama Phase 1 test sahnesi için erken bir düşman gerekebilir)
5. UI: HUDController, InventoryUI (O2/panik/injury/ammo/extraction timer'ı görselleştirmek için)
6. Test sahnesi: en az 1 loot zone (3+ `LootSpawnPoint`) + 1 ExtractionPoint + RunManager kurup uçtan uca playtest

## ⚠️ Bilinen Sorunlar / Riskler

- `depthConsumptionCurve` (OxygenSystem) inspector'da elle ayarlanmalı, kod tarafında default değeri yok
- `"Pickup"` layer'ı Unity Editor'de manuel oluşturulmalı (Tags & Layers)
- `SO_HarpoonData`/`SO_TankData`/`SO_ItemData`/`SO_ConsumableItemData`/`SO_LootTableData` asset instance'ları henüz oluşturulmadı — prefab'lara atanmadan `HarpoonWeapon`/`OxygenSystem` null referansla çalışır (`Fire()` NullReferenceException atar)
- `SO_TankData`/`SO_HarpoonData` inheritance refactor'ünden sonra, bu tiplerden önceden oluşturulmuş asset varsa Inspector'da `ItemName`/`Weight`/`WorldPrefab` alanlarının yeniden doldurulması gerekebilir (henüz asset oluşturulmadığı için şu an teorik risk)
- `PanicState`/`InjurySystem`/`AdrenalineItem`'ın sayısal varsayılanları (panik yükselme/düşme oranı, hız/nişan çarpanları, bandaj süresi, adrenalin bağışıklık süresi) placeholder — in-editor playtest ile ayarlanmalı
- LootManager test sahnesi kurulmadı — en az 1 zon + birkaç `LootSpawnPoint` ile doğrulanmalı
- Grid envanter v2 sonrası SO asset'lerde `GridWidth/GridHeight` ve `Rarity` alanları doldurulmalı (default 1x1/Common)
- RunManager hâlâ kendi bellek-içi stash'ini kullanıyor — `StashSystem.Instance`'a migrate edilmeli (küçük refactor)
- **Farklı AI session'ları arasında API senkron kontrolü şart:** GPT tarafından ardışık iki teslimatta da `InventorySystem.cs`, gerçek `InjurySystem`/`PanicState` API'leriyle uyuşmayan metotlar varsaydı (`GetCurrentInjury()`/`InjuryType`/`StartBandage()`/`UseAdrenalineShot()` — hiçbiri repoda yok; gerçek API'ler: `Severity`, `IsBandaging`, `StartBandaging()`, `ApplyAdrenaline(float)`). Her iki seferde de bu dosya olduğu gibi uygulanmadı, sadece gerçekten yeni olan kısımlar (`ClearAll()`) mevcut düzeltilmiş dosyaya taşındı. Aynı şekilde `HarpoonWeapon.cs` ve `PlayerControllerIntegration.cs` de art arda iki teslimatta, bu depodaki panik-nişan sapması ve ortak derinlik referansı fix'lerinden önceki eski haliyle geldi — uygulanmadı. Yeni bir AI session bir dosyayı "hazır" olarak teslim ettiğinde, referans verdiği tiplerin bu dosyadaki ✅ tabloyla birebir eştiğini doğrulamadan uygulama.

## 🧱 Kalıcı Mimari Kararlar (değişmeyecek varsayımlar)

- Unity 2022.3 LTS + URP, `rb.velocity` (linearVelocity DEĞİL — Unity 6 API'si kullanılmıyor)
- Hareket: Rigidbody + AddForce tabanlı (CharacterController KULLANILMIYOR)
- Motor: Unity'de kalınıyor, Godot'a geçiş değerlendirilip reddedildi (bkz. CONTEXT.md Key Design Decisions)
- Naming: PascalCase class/method, camelCase variable, `SO_` prefix (ScriptableObject), `PR_` prefix (prefab)
- Üçüncü parti açık kaynak kod adapte edilirken önce lisans kontrol edilir (MIT/Apache tercih edilir), kod birebir kopyalanmaz — mantık bizim naming convention/mimarimize göre yeniden yazılır. (İlk uygulama: `kukumberman/unity-grid-inventory`, MIT doğrulandı, sadece grid/footprint mantığı referans alındı.)

---

*Bu dosyayı her yeni sistem eklendiğinde/değiştiğinde güncelle: ✅ tabloya taşı,
⏭️ listesinden çıkar, yeni ⚠️/🧱 varsa ekle. `CONTEXT.md`'yi sadece büyük mimari
değişikliklerde güncellemen yeterli.*
