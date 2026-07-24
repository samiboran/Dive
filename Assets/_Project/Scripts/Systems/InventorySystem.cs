using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GRID tabanlı envanter (Görev 4 — unity-grid-inventory MIT reposunun
/// grid/footprint mantığı referans alınarak bizim mimariye göre yazıldı).
///
/// - Item'lar SO_ItemData.GridWidth/GridHeight kadar hücre kaplar
/// - Yerleşim kontrolü: bool[,] occupancy grid — üst üste binme olmaz
/// - Liste API'si (Slots) korundu → RunManager/Consumable akışı etkilenmez
///
/// Etkileşim deseni aynı: E pickup, Q drop, 1-9 slot seçimi.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField] private int gridWidth = 3;   // 3x2 = 6 hücrelik dalış çantası
    [SerializeField] private int gridHeight = 2;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;

    [Serializable]
    public class InventorySlot
    {
        public SO_ItemData Item;
        public int Count;
        public int GridX;   // yerleşim sol-üst köşesi
        public int GridY;

        public bool IsEmpty => Item == null;
    }

    private readonly List<InventorySlot> slots = new List<InventorySlot>();
    private bool[,] occupancy; // [x, y]

    public event Action OnInventoryChanged;
    public event Action OnInventoryFull;

    private WorldItemPickup nearbyItem;
    private int selectedSlot = 0;

    public IReadOnlyList<InventorySlot> Slots => slots;
    public int SelectedSlot => selectedSlot;
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;

    private void Awake()
    {
        occupancy = new bool[gridWidth, gridHeight];
    }

    private void Update()
    {
        for (int i = 0; i < slots.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                selectedSlot = i;
        }

        if (nearbyItem != null && Input.GetKeyDown(interactKey))
        {
            if (TryAdd(nearbyItem.ItemData, 1))
            {
                nearbyItem.Consume();
                nearbyItem = null;
            }
            else
            {
                OnInventoryFull?.Invoke();
                Debug.Log("[InventorySystem] Envanter dolu — bu boyutta item için yer yok!");
            }
        }

        if (Input.GetKeyDown(dropKey) && selectedSlot < slots.Count)
            DropSlot(selectedSlot);
    }

    private void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<WorldItemPickup>();
        if (item != null)
        {
            nearbyItem = item;
            Debug.Log($"[InventorySystem] {item.ItemData.ItemName} menzilde — E ile al.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var item = other.GetComponent<WorldItemPickup>();
        if (item != null && item == nearbyItem)
            nearbyItem = null;
    }

    /// <summary>
    /// FIX: eskiden fazlalık tek bir slota (MaxStack ile sınırlı) konup geri
    /// kalanı sessizce kaybediliyordu, üstelik yine de true (başarı) dönüyordu.
    /// Artık iki aşamalı: önce hiçbir şeyi değiştirmeden "hepsi sığar mı?" diye
    /// dener (occupancy'nin geçici kopyası üzerinde), ancak hepsi sığıyorsa
    /// gerçekten uygular. Böylece kısmi/atomik olmayan bir ekleme StashSystem.
    /// WithdrawTo() gibi "false dönerse hiçbir şey değişmedi" varsayan
    /// çağıranlarda çiftleme/veri kaybına yol açmaz.
    /// </summary>
    public bool TryAdd(SO_ItemData item, int count)
    {
        if (item == null || count <= 0) return false;

        int remaining = count;
        var plannedMerges = new List<(InventorySlot slot, int add)>();
        var plannedNewSlots = new List<(int x, int y, int amount)>();

        // 1) Mevcut stack'lere ne kadarı sığar — henüz UYGULAMADAN hesapla
        if (item.IsStackable)
        {
            foreach (var slot in slots)
            {
                if (remaining <= 0) break;
                if (slot.Item == item && slot.Count < item.MaxStack)
                {
                    int add = Mathf.Min(remaining, item.MaxStack - slot.Count);
                    plannedMerges.Add((slot, add));
                    remaining -= add;
                }
            }
        }

        // 2) Kalanı yeni grid slotlarına yerleştirebilir miyiz — occupancy'nin
        // geçici bir kopyası üzerinde dene, gerçek grid'i henüz değiştirme
        if (remaining > 0)
        {
            bool[,] scratch = (bool[,])occupancy.Clone();

            while (remaining > 0)
            {
                int stackSize = item.IsStackable ? Mathf.Min(remaining, item.MaxStack) : 1;

                if (!FindFreeRect(scratch, item.GridWidth, item.GridHeight, out int x, out int y))
                    break; // yer kalmadı

                MarkCells(scratch, x, y, item.GridWidth, item.GridHeight, true);
                plannedNewSlots.Add((x, y, stackSize));
                remaining -= stackSize;
            }
        }

        if (remaining > 0)
            return false; // tamamı sığmıyor — envanter DEĞİŞMEDEN false dön

        // 3) Hepsi sığıyor — şimdi gerçekten uygula
        foreach (var (slot, add) in plannedMerges)
            slot.Count += add;

        foreach (var (x, y, amount) in plannedNewSlots)
        {
            slots.Add(new InventorySlot { Item = item, Count = amount, GridX = x, GridY = y });
            MarkCells(occupancy, x, y, item.GridWidth, item.GridHeight, true);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>Referans repodaki temel mantık: w×h'lik boş dikdörtgen taraması.</summary>
    private bool FindFreeRect(bool[,] grid, int w, int h, out int outX, out int outY)
    {
        w = Mathf.Clamp(w, 1, gridWidth);
        h = Mathf.Clamp(h, 1, gridHeight);

        for (int y = 0; y <= gridHeight - h; y++)
        {
            for (int x = 0; x <= gridWidth - w; x++)
            {
                if (IsRectFree(grid, x, y, w, h))
                {
                    outX = x;
                    outY = y;
                    return true;
                }
            }
        }

        outX = -1;
        outY = -1;
        return false;
    }

    private bool IsRectFree(bool[,] grid, int startX, int startY, int w, int h)
    {
        for (int y = startY; y < startY + h; y++)
            for (int x = startX; x < startX + w; x++)
                if (grid[x, y])
                    return false;
        return true;
    }

    private void MarkCells(bool[,] grid, int startX, int startY, int w, int h, bool occupied)
    {
        for (int y = startY; y < startY + h; y++)
            for (int x = startX; x < startX + w; x++)
                grid[x, y] = occupied;
    }

    public void DropSlot(int index)
    {
        var slot = slots[index];
        if (slot.IsEmpty) return;

        if (slot.Item.WorldPrefab != null)
        {
            Vector3 dropPos = transform.position + transform.forward * 1.5f;
            Instantiate(slot.Item.WorldPrefab, dropPos, Quaternion.identity);
        }

        slot.Count--;
        if (slot.Count <= 0)
            RemoveSlot(slot);

        OnInventoryChanged?.Invoke();
    }

    private void RemoveSlot(InventorySlot slot)
    {
        MarkCells(occupancy, slot.GridX, slot.GridY, slot.Item.GridWidth, slot.Item.GridHeight, false);
        slots.Remove(slot);
    }

    /// <summary>
    /// Seçili slottaki consumable'ı kullanır (bandaj/adrenalin/yedek tüp).
    /// </summary>
    public bool UseSelectedConsumable()
    {
        if (selectedSlot >= slots.Count) return false;

        var slot = slots[selectedSlot];
        if (slot.Item is not SO_ConsumableItemData consumable) return false;

        bool used = consumable.Type switch
        {
            ConsumableType.Bandage => UseBandage(),
            ConsumableType.AdrenalineShot => UseAdrenaline(),
            ConsumableType.SpareTank => UseSpareTank(consumable.EffectAmount),
            _ => false
        };

        if (used)
        {
            slot.Count--;
            if (slot.Count <= 0)
                RemoveSlot(slot);
            OnInventoryChanged?.Invoke();
        }
        return used;
    }

    private bool UseBandage()
    {
        var injury = GetComponent<InjurySystem>();
        if (injury == null || injury.GetCurrentInjury() == InjuryType.None) return false;
        injury.StartBandage();
        return true;
    }

    private bool UseAdrenaline()
    {
        var panic = GetComponent<PanicState>();
        if (panic == null) return false;
        panic.UseAdrenalineShot();
        return true;
    }

    private bool UseSpareTank(float amount)
    {
        var oxygen = GetComponent<OxygenSystem>();
        if (oxygen == null) return false;
        oxygen.RefillFromSpareTank(amount);
        return true;
    }

    /// <summary>
    /// Slot'tan count kadar item tüketir — sahneye prefab BIRAKMAZ, grid'i temizler.
    /// LiftBagDeployer gibi sistemler kullanır (DropSlot'tan farkı: drop spawn yok).
    /// </summary>
    public bool ConsumeSlot(int index, int count = 1)
    {
        if (index >= slots.Count) return false;

        var slot = slots[index];
        if (slot.IsEmpty || slot.Count < count) return false;

        slot.Count -= count;
        if (slot.Count <= 0)
            RemoveSlot(slot);

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>RunManager extraction/ölüm akışı kullanır — grid de temizlenir.</summary>
    public void ClearAll()
    {
        slots.Clear();
        occupancy = new bool[gridWidth, gridHeight];
        OnInventoryChanged?.Invoke();
    }

    public float GetTotalWeight()
    {
        float total = 0f;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
                total += slot.Item.Weight * slot.Count;
        }
        return total;
    }
}
