# PROJECT_STATE.md — Görev 3 sonrası güncelleme diff'i

Mevcut dosyana şu değişiklikleri uygula (AI SESSION KURALI bloğu aynen kalsın):

## ✅ tablosuna ekle:

| InventorySystem | `Systems/InventorySystem.cs`, `SO_ItemData.cs`, `SO_ConsumableItemData.cs`, `WorldItemPickup.cs` | 5 slot, stack, E pickup / Q drop, consumable kullanımı mevcut sistemlere delege |
| LootManager | `Systems/LootManager.cs`, `SO_LootTableData.cs` | Weighted random (RarityWeight), zon bazlı density, WorldItemPickup spawn |

Ayrıca tablodaki şu satırları güncelle:
- OxygenSystem satırına: `SO_TankData artık SO_ItemData'dan türüyor` notunu ekle
- Harpoon Combat satırına: `SO_HarpoonData artık SO_ItemData'dan türüyor` notunu ekle

## 🔄 bölümünü TEMİZLE:
- `PlayerController.cs` SpeedMultiplier bağlantısı → yapıldı (Görev 1)
- `HarpoonProjectile.PickUp()` interact script'i → yapıldı (`HarpoonPickup.cs`)

## ⏭️ listesinden ÇIKAR:
1. ~~InventorySystem (5 slot, pickup/drop)~~
2. ~~LootManager (procedural spawn, rarity ağırlıkları)~~

Yeni ⏭️ sırası:
1. ExtractionPoint (tek çıkış, aktivasyon timer)
2. RunManager (dalış init, ölümde gear kaybı, stash)
3. Weight-based carry capacity (InventorySystem.GetTotalWeight() hazır)

## ⚠️ bölümünden SİL:
- ~~RefillStation GetComponentInChildren null dönebilir~~ (fallback eklendi)

## ⚠️ bölümüne EKLE:
- `SO_TankData`/`SO_HarpoonData` refactor sonrası mevcut SO asset'lerin inspector'da ItemName/Weight/WorldPrefab alanları yeniden doldurulmalı (SerializeField private set — inheritance sonrası eski asset'ler bu alanları boş gösterebilir)
- LootManager test sahnesi kurulmadı — en az 1 zon + 3 spawn point ile doğrulanmalı

**Son güncelleme tarihini:** 2026-07-24 (veya commit günü) olarak güncelle.
