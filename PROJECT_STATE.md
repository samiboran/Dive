# PROJECT_STATE — UnderwaterExtraction

> Son güncelleme: Phase 1 kapanışı (entegrasyon + pickup)

## Tamamlanan Sistemler

| Sistem | Durum | Notlar |
|--------|-------|--------|
| OxygenSystem | ✅ Done | Depth curve, 3 tank tier'ı, AirPocketVolume, `RefillOxygen()` + `RefillFromSpareTank()` |
| PanicState | ✅ Done | Inverse panic (speed↑, aim↓), AdrenalineShot, adrenaline sonrası panik geri dönüşü |
| Harpoon Combat | ✅ Done | Fire-reload, **ammo/stok sayacı** (sonsuz ateş yok), saplanan zıpkın toplanabilir, HarpoonBlocker |
| InjurySystem | ✅ Done | Tarkov-vari bleeding, kesilebilir bandaj, movement penalty |
| RefillStation | ✅ Done | Oksijen + zıpkın dolumu, E-tuşu etkileşim deseni |
| HarpoonPickup | ✅ Done | Saplı zıpkın E ile toplanır → `ReturnHarpoon(1)` |
| PlayerControllerIntegration | ✅ Done | Rigidbody hızından idle/swim/sprint, combat timer (OnFired + 5sn), SpeedMultiplier blending |

## Bilinen Sorunlar / Teknik Borç

- [ ] HarpoonProjectile saplanma VFX/SFX eksik
- [ ] PanicState `AimSwayMultiplier` henüz aim sistemine bağlı değil (aim sistemi yok)
- [ ] InjurySystem sadece SharkBite implemente — JellyfishSting ve Barotrauma case'leri boş
- [ ] DepthCurve asset'i elle oluşturulmalı (AnimationCurve inspector'dan)

## Manuel Kurulum Adımları (Unity Editor)

1. **Tags & Layers → "Pickup"** layer'ı oluştur
2. Harpoon prefab'ı: Rigidbody (UseGravity=false) + trigger Collider + HarpoonProjectile
3. Oyuncu prefab'ı: OxygenSystem, PanicState, InjurySystem, PlayerHealth, PlayerControllerIntegration, HarpoonPickup (trigger collider'lı child)
4. SO asset'leri: `Create → UnderwaterExtraction → TankData / HarpoonData`

## Sıradaki Öncelik

1. **InventorySystem** — 5 slot, `SO_ItemData` base, pickup/drop (Görev 2)
2. Weight-based carry capacity (Phase 3 hazırlığı)
3. LootManager — `SO_ItemData` ile uyumlu loot table
4. PvP netcode sync (harpoon reload window)

## Mimari Kararlar (kısa)

- Event-driven loose coupling: sistemler birbirini `event Action` üzerinden dinler, doğrudan referans minimum
- Veri katmanı ScriptableObject (`SO_` prefix), asset'ler `Assets/_Project/Data/`
- Etkileşim deseni her yerde aynı: trigger alanı + E tuşu (`interactKey` serialize edilmiş)
- Naming: PascalCase class/method, camelCase field
