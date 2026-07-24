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
| SO_HarpoonData / SO_TankData | `Data/SO_HarpoonData.cs`, `Data/SO_TankData.cs` | ScriptableObject veri konteynerleri — sınıflar hazır, asset instance'ları henüz Editor'de oluşturulmadı |

## 🔄 Şu An Devam Eden

- `HarpoonProjectile.PickUp()` için interact/trigger script'i (henüz hiçbir yerden çağrılmıyor)
- Gerçek Unity projesinin oluşturulması (ProjectSettings, Packages/manifest.json) — repoda şu an sadece `Assets/` altındaki scriptler var

## ⏭️ Sıradaki Öncelik (Phase 1 tamamlanması için)

1. InventorySystem (5 slot, pickup/drop) — AdrenalineItem gibi item'ları da buraya bağla
2. LootManager (procedural spawn, rarity ağırlıkları)
3. ExtractionPoint (tek çıkış, aktivasyon timer)
4. RunManager (dalış init, ölümde gear kaybı, stash)

## ⚠️ Bilinen Sorunlar / Riskler

- `depthConsumptionCurve` (OxygenSystem) inspector'da elle ayarlanmalı, kod tarafında default değeri yok
- `"Pickup"` layer'ı Unity Editor'de manuel oluşturulmalı (Tags & Layers)
- `SO_HarpoonData`/`SO_TankData` asset instance'ları henüz oluşturulmadı — prefab'lara atanmadan `HarpoonWeapon`/`OxygenSystem` null referansla çalışır (`Fire()` NullReferenceException atar)
- `PanicState`/`InjurySystem`/`AdrenalineItem`'ın sayısal varsayılanları (panik yükselme/düşme oranı, hız/nişan çarpanları, bandaj süresi, adrenalin bağışıklık süresi) placeholder — in-editor playtest ile ayarlanmalı

## 🧱 Kalıcı Mimari Kararlar (değişmeyecek varsayımlar)

- Unity 2022.3 LTS + URP, `rb.velocity` (linearVelocity DEĞİL — Unity 6 API'si kullanılmıyor)
- Hareket: Rigidbody + AddForce tabanlı (CharacterController KULLANILMIYOR)
- Motor: Unity'de kalınıyor, Godot'a geçiş değerlendirilip reddedildi (bkz. CONTEXT.md Key Design Decisions)
- Naming: PascalCase class/method, camelCase variable, `SO_` prefix (ScriptableObject), `PR_` prefix (prefab)

---

*Bu dosyayı her yeni sistem eklendiğinde/değiştiğinde güncelle: ✅ tabloya taşı,
⏭️ listesinden çıkar, yeni ⚠️/🧱 varsa ekle. `CONTEXT.md`'yi sadece büyük mimari
değişikliklerde güncellemen yeterli.*
