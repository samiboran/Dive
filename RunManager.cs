using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raid döngüsü yöneticisi: dalış başlatma, extraction/ölüm sonrası envanter
/// transferi, stash (banka) yönetimi.
///
/// KURAL (Tarkov-vari):
/// - Extraction başarılı → envanter stash'e aktarılır, gear (tank/zıpkın) korunur
/// - Ölüm → envanterdeki her şey kaybolur, gear korunur (gear kaybı Phase 2+ tartışması)
///
/// Stash şu an bellek-içi (scene değişince sıfırlanır) — kalıcılık (save/load)
/// ayrı bir görev olarak backlog'da.
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private OxygenSystem playerOxygen;
    [SerializeField] private Transform diveSpawnPoint;

    // Stash — basit liste (kalıcı save sistemi sonra)
    private readonly List<(SO_ItemData item, int count)> stash = new List<(SO_ItemData, int)>();

    public IReadOnlyList<(SO_ItemData item, int count)> Stash => stash;

    // Events — UI bağlanır
    public event Action OnRunStarted;
    public event Action OnRunExtracted;  // başarılı çıkış
    public event Action OnRunFailed;     // ölüm
    public event Action OnStashChanged;

    private bool runActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= HandleDeath;
    }

    /// <summary>Sahnedeki ExtractionPoint'leri dinlemeye başla (Start'ta çağrılabilir).</summary>
    public void RegisterExtractionPoint(ExtractionPoint point)
    {
        point.OnExtractionComplete += HandleExtraction;
    }

    /// <summary>Yeni dalış başlat: oyuncuyu spawn noktasına koy, sistemleri sıfırla.</summary>
    public void StartRun()
    {
        if (runActive) return;
        runActive = true;

        if (diveSpawnPoint != null)
        {
            playerInventory.transform.position = diveSpawnPoint.position;
            playerInventory.transform.rotation = diveSpawnPoint.rotation;
        }

        // Oksijen zaten EquipTank/RefillOxygen ile doldurulmuş varsayılır
        OnRunStarted?.Invoke();
        Debug.Log("[RunManager] Dalış başladı.");
    }

    private void HandleExtraction()
    {
        if (!runActive) return;
        runActive = false;

        // Envanteri stash'e aktar
        TransferInventoryToStash();

        OnRunExtracted?.Invoke();
        Debug.Log($"[RunManager] Extraction başarılı — loot stash'e aktarıldı ({stash.Count} kalem).");
    }

    private void HandleDeath()
    {
        if (!runActive) return;
        runActive = false;

        // Ölüm: envanterdeki her şey kaybolur
        ClearInventory();

        OnRunFailed?.Invoke();
        Debug.Log("[RunManager] Ölüm — envanter kaybedildi. Gear korundu.");
    }

    private void TransferInventoryToStash()
    {
        foreach (var slot in playerInventory.Slots)
        {
            if (slot.IsEmpty) continue;

            // Stack'lenebilirse stash'te birleştir
            bool merged = false;
            if (slot.Item.IsStackable)
            {
                for (int i = 0; i < stash.Count; i++)
                {
                    if (stash[i].item == slot.Item)
                    {
                        stash[i] = (stash[i].item, stash[i].count + slot.Count);
                        merged = true;
                        break;
                    }
                }
            }
            if (!merged)
                stash.Add((slot.Item, slot.Count));
        }

        ClearInventory();
        OnStashChanged?.Invoke();
    }

    private void ClearInventory()
    {
        // InventorySystem.ClearAll() slotları boşaltır ve OnInventoryChanged
        // event'ini tetikler — UI otomatik güncellenir.
        playerInventory.ClearAll();
    }
}
