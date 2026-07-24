using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 5 slot'lu envanter. Etkileşim deseni projeyle aynı:
/// trigger alanı + E tuşu (pickup), Q tuşu (seçili slotu drop).
///
/// Event-driven: UI, OnInventoryChanged'e subscribe olur.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public const int SlotCount = 5;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;

    [Serializable]
    public class InventorySlot
    {
        public SO_ItemData Item;
        public int Count;

        public bool IsEmpty => Item == null;
    }

    private readonly List<InventorySlot> slots = new List<InventorySlot>();

    public event Action OnInventoryChanged;
    public event Action OnInventoryFull;

    private WorldItemPickup nearbyItem; // trigger'daki item adayı
    private int selectedSlot = 0;

    public IReadOnlyList<InventorySlot> Slots => slots;
    public int SelectedSlot => selectedSlot;

    private void Awake()
    {
        for (int i = 0; i < SlotCount; i++)
            slots.Add(new InventorySlot());
    }

    private void Update()
    {
        // Slot seçimi (1-5 tuşları)
        for (int i = 0; i < SlotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                selectedSlot = i;
        }

        // Pickup
        if (nearbyItem != null && Input.GetKeyDown(interactKey))
        {
            if (TryAdd(nearbyItem.ItemData, 1))
            {
                nearbyItem.Consume(); // sahneden kaldır
                nearbyItem = null;
            }
            else
            {
                OnInventoryFull?.Invoke();
                Debug.Log("[InventorySystem] Envanter dolu!");
            }
        }

        // Drop
        if (Input.GetKeyDown(dropKey))
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

    public bool TryAdd(SO_ItemData item, int count)
    {
        if (item == null || count <= 0) return false;

        // 1) Stack'lenebiliyorsa önce mevcut stack'e ekle
        if (item.IsStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.Item == item && slot.Count < item.MaxStack)
                {
                    int add = Mathf.Min(count, item.MaxStack - slot.Count);
                    slot.Count += add;
                    count -= add;
                    if (count <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // 2) Boş slot bul
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.Item = item;
                slot.Count = Mathf.Min(count, item.MaxStack);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public void DropSlot(int index)
    {
        var slot = slots[index];
        if (slot.IsEmpty) return;

        // FIX: log'dan önce isim referansını al — slot.Item aşağıda null'a çekiliyor
        string droppedItemName = slot.Item.ItemName;

        // Sahneye world prefab bırak — oyuncunun önüne
        if (slot.Item.WorldPrefab != null)
        {
            Vector3 dropPos = transform.position + transform.forward * 1.5f;
            Instantiate(slot.Item.WorldPrefab, dropPos, Quaternion.identity);
        }

        slot.Count--;
        if (slot.Count <= 0)
        {
            slot.Item = null;
            slot.Count = 0;
        }

        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventorySystem] {droppedItemName} bırakıldı (slot {index}).");
    }

    /// <summary>
    /// Seçili slottaki consumable'ı kullanır (bandaj/adrenalin/yedek tüp).
    /// </summary>
    public bool UseSelectedConsumable()
    {
        var slot = slots[selectedSlot];
        if (slot.Item is not SO_ConsumableItemData consumable) return false;

        bool used = consumable.Type switch
        {
            ConsumableType.Bandage => UseBandage(),
            ConsumableType.AdrenalineShot => UseAdrenaline(consumable.EffectAmount),
            ConsumableType.SpareTank => UseSpareTank(consumable.EffectAmount),
            _ => false
        };

        if (used)
        {
            slot.Count--;
            if (slot.Count <= 0)
            {
                slot.Item = null;
                slot.Count = 0;
            }
            OnInventoryChanged?.Invoke();
        }
        return used;
    }

    private bool UseBandage()
    {
        var injury = GetComponent<InjurySystem>();
        // Zaten bandajlanıyorsa veya yaralı değilse boşa harcama
        if (injury == null || injury.IsBandaging || injury.Severity <= 0f) return false;
        injury.StartBandaging();
        return true;
    }

    private bool UseAdrenaline(float duration)
    {
        var panic = GetComponent<PanicState>();
        if (panic == null) return false;
        panic.ApplyAdrenaline(duration);
        return true;
    }

    private bool UseSpareTank(float amount)
    {
        var oxygen = GetComponent<OxygenSystem>();
        if (oxygen == null) return false;
        oxygen.RefillFromSpareTank(amount);
        return true;
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
