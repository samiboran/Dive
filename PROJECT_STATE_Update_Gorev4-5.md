# PROJECT_STATE.md — Görev 4 + 5 güncellemesi

## ✅ tablosuna ekle:

| StashSystem | `Systems/StashSystem.cs` | Kalıcı ana depo, tier upgrade (20→40 slot), UI hook: `Slots` + `OnStashChanged` |
| Grid Inventory | `Systems/InventorySystem.cs` (v2) | Grid footprint (item boyutu kadar hücre), liste API korundu — RunManager etkilenmedi |
| LootSpawnPoint | `Systems/LootSpawnPoint.cs` | Elle yerleştirilen marker, nokta başına table + density, kendini manager'a kaydeder |

Ve güncelle:
- InventorySystem satırı → "grid tabanlı v2" notu
- LootManager satırı → "LootSpawnPoint marker tabanlı v2, Floor 1/2/3 density (%30/%60/%90)"

## ⏭️ listesinden çıkar:
- ~~LootManager~~ (zaten çıkarılmıştı, v2 ile tamamlandı)

Yeni ⏭️ sırası:
1. Weight-based carry capacity (`GetTotalWeight()` hazır)
2. Stash save/load (JSON kalıcılık — StashSystem hook'ları hazır)
3. Stash UI (hook noktası hazır: `StashSystem.Slots`)

## ⚠️ ekle:
- Grid envanter v2 sonrası SO asset'lerde `GridWidth/GridHeight` ve `Rarity` alanları doldurulmalı (default 1x1/Common)
- RunManager hâlâ kendi bellek-içi stash'ini kullanıyor — StashSystem.Instance'a migrate edilmeli (küçük refactor)

## 🧱 "Kalıcı Mimari Kararlar"a ekle:
- Üçüncü parti açık kaynak kod adapte edilirken önce lisans kontrol edilir (MIT/Apache tercih edilir), kod birebir kopyalanmaz — mantık bizim naming convention/mimarimize göre yeniden yazılır. (İlk uygulama: `kukumberman/unity-grid-inventory`, MIT doğrulandı, sadece grid/footprint mantığı referans alındı.)
