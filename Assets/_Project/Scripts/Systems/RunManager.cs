using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// v3 — Event'ler artık payload taşıyor (extraction/ölüm anındaki envanter
/// snapshot'ı). Snapshot, envanter temizlenmeden ÖNCE alınır — sonuç
/// ekranı (GameFlowController.LastResult) doğru dolu gelir.
///
/// Raid döngüsü yöneticisi: dalış başlatma, extraction/ölüm sonrası
/// envanter transferi. Depo StashSystem.Instance üzerinde.
///
/// KURAL (Tarkov-vari):
/// - Extraction başarılı → envanter stash'e aktarılır, gear korunur
/// - Ölüm → envanterdeki her şey kaybolur, gear korunur
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    /// <summary>Sonuç verisi — event ile birlikte gönderilir.</summary>
    [Serializable]
    public class ResultPayload
    {
        public bool extracted;
        public List<ResultItem> items = new List<ResultItem>();
    }

    [Serializable]
    public class ResultItem
    {
        public string itemName;
        public int count;
    }

    [Header("References")]
    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform diveSpawnPoint;

    // Events — UI/GameFlow bağlanır (payload'lı)
    public event Action OnRunStarted;
    public event Action<ResultPayload> OnRunExtracted;
    public event Action<ResultPayload> OnRunFailed;

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

    public void RegisterExtractionPoint(ExtractionPoint point)
    {
        point.OnExtractionComplete += HandleExtraction;
    }

    public void StartRun()
    {
        if (runActive) return;
        runActive = true;

        if (diveSpawnPoint != null)
        {
            playerInventory.transform.position = diveSpawnPoint.position;
            playerInventory.transform.rotation = diveSpawnPoint.rotation;
        }

        OnRunStarted?.Invoke();
        Debug.Log("[RunManager] Dalış başladı.");
    }

    /// <summary>Envanter TEMİZLENMEDEN önce snapshot alır.</summary>
    private ResultPayload SnapshotInventory(bool extracted)
    {
        var payload = new ResultPayload { extracted = extracted };
        foreach (var slot in playerInventory.Slots)
        {
            if (slot.IsEmpty) continue;
            payload.items.Add(new ResultItem
            {
                itemName = slot.Item.ItemName,
                count = slot.Count
            });
        }
        return payload;
    }

    private void HandleExtraction()
    {
        if (!runActive) return;
        runActive = false;

        // 1) ÖNCE snapshot (envanter doluyken)
        ResultPayload payload = SnapshotInventory(extracted: true);

        // 2) Envanteri StashSystem'e aktar
        int stored = 0;
        foreach (var slot in playerInventory.Slots)
        {
            if (slot.IsEmpty) continue;
            if (StashSystem.Instance != null && StashSystem.Instance.TryStore(slot.Item, slot.Count))
                stored++;
        }

        // 3) Envanteri temizle
        playerInventory.ClearAll();

        // 4) EN SON event — payload ile
        OnRunExtracted?.Invoke(payload);
        Debug.Log($"[RunManager] Extraction başarılı — {stored} kalem stash'e aktarıldı.");
    }

    private void HandleDeath()
    {
        if (!runActive) return;
        runActive = false;

        // Önce snapshot (kaybedilen loot'un listesi — sonuç ekranında gösterilir)
        ResultPayload payload = SnapshotInventory(extracted: false);

        playerInventory.ClearAll(); // Envanter kaybolur, gear korunur
        OnRunFailed?.Invoke(payload);
        Debug.Log("[RunManager] Ölüm — envanter kaybedildi. Gear korundu.");
    }
}
