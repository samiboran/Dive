using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ana menü stash ekranı: iki grid paneli yan yana —
/// solda oyuncu envanteri (InventorySystem), sağda depo (StashSystem).
/// Drag & drop ile iki yönlü transfer.
///
/// Stash slot'ları liste tabanlı olduğu için sağ panel "akışkan yerleşim"
/// kullanır: her item ilk boş w×h alana otomatik konur (görsel yerleşim,
/// veri değişmez). Sığmayan item gösterilmez — tier upgrade teşviki.
/// </summary>
public class StashMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GridInventoryUI inventoryPanel;
    [SerializeField] private GridInventoryUI stashPanel;

    [Header("Stash Grid Visual")]
    [SerializeField] private int stashGridWidth = 8;
    [SerializeField] private int stashGridHeight = 6;

    private InventorySystem playerInventory;

    private void Start()
    {
        playerInventory = FindObjectOfType<InventorySystem>();

        if (playerInventory != null)
        {
            inventoryPanel.Setup(playerInventory.GridWidth, playerInventory.GridHeight);
            playerInventory.OnInventoryChanged += RefreshAll;
        }

        stashPanel.Setup(stashGridWidth, stashGridHeight);

        if (StashSystem.Instance != null)
            StashSystem.Instance.OnStashChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshAll;
        if (StashSystem.Instance != null)
            StashSystem.Instance.OnStashChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        var invViews = new List<SlotViewData>();
        if (playerInventory != null)
        {
            for (int i = 0; i < playerInventory.Slots.Count; i++)
            {
                var slot = playerInventory.Slots[i];
                if (slot.IsEmpty) continue;
                invViews.Add(new SlotViewData
                {
                    Item = slot.Item,
                    Count = slot.Count,
                    GridX = slot.GridX,
                    GridY = slot.GridY,
                    SourceIndex = i
                });
            }
        }
        inventoryPanel.RefreshIcons(invViews, HandleInventoryDrop);

        stashPanel.RefreshIcons(BuildStashViews(), HandleStashDrop);
    }

    private List<SlotViewData> BuildStashViews()
    {
        var views = new List<SlotViewData>();
        if (StashSystem.Instance == null) return views;

        bool[,] occupied = new bool[stashGridWidth, stashGridHeight];

        for (int i = 0; i < StashSystem.Instance.Slots.Count; i++)
        {
            var slot = StashSystem.Instance.Slots[i];
            if (slot.IsEmpty) continue;

            int w = Mathf.Clamp(slot.Item.GridWidth, 1, stashGridWidth);
            int h = Mathf.Clamp(slot.Item.GridHeight, 1, stashGridHeight);

            if (FindFreeCell(occupied, w, h, out int x, out int y))
            {
                views.Add(new SlotViewData
                {
                    Item = slot.Item,
                    Count = slot.Count,
                    GridX = x,
                    GridY = y,
                    SourceIndex = i
                });

                for (int cy = y; cy < y + h; cy++)
                    for (int cx = x; cx < x + w; cx++)
                        occupied[cx, cy] = true;
            }
        }

        return views;
    }

    private bool FindFreeCell(bool[,] occupied, int w, int h, out int outX, out int outY)
    {
        for (int y = 0; y <= stashGridHeight - h; y++)
            for (int x = 0; x <= stashGridWidth - w; x++)
            {
                bool free = true;
                for (int cy = y; cy < y + h && free; cy++)
                    for (int cx = x; cx < x + w && free; cx++)
                        if (occupied[cx, cy]) free = false;
                if (free) { outX = x; outY = y; return true; }
            }
        outX = -1; outY = -1;
        return false;
    }

    // Envanter paneline bırakılan (stash'ten çekme)
    private void HandleInventoryDrop(SlotViewData data, Vector2 screenPos, GridInventoryUI targetPanel)
    {
        if (StashSystem.Instance == null || playerInventory == null) return;

        if (data.SourceIndex < StashSystem.Instance.Slots.Count &&
            StashSystem.Instance.Slots[data.SourceIndex].Item == data.Item)
        {
            StashSystem.Instance.WithdrawTo(playerInventory, data.SourceIndex);
        }
        // Panel içi yeniden yerleşim Phase 2 polish — şimdilik ikon geri döner
    }

    // Stash paneline bırakılan (envanterden depolama)
    private void HandleStashDrop(SlotViewData data, Vector2 screenPos, GridInventoryUI targetPanel)
    {
        if (StashSystem.Instance == null || playerInventory == null) return;

        if (StashSystem.Instance.TryStore(data.Item, data.Count))
            playerInventory.ConsumeSlot(data.SourceIndex, data.Count);
    }
}
