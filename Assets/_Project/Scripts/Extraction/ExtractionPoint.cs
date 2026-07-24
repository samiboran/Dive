using System;
using UnityEngine;

/// <summary>
/// Tek çıkış noktası. Oyuncu trigger alanına girince aktivasyon timer'ı başlar;
/// süre dolmadan alandan çıkarsa iptal olur, dolarsa extraction gerçekleşir.
/// Tarkov loop'unun "çıkış gerilimi" parçası — süre boyunca oyuncu savunmasız
/// kalır (hâlâ yüzebilir ama alanı terk edemez).
///
/// RunManager, OnExtractionComplete event'ine subscribe olur.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExtractionPoint : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private float activationDuration = 8f; // Çıkış için bekleme süresi
    [SerializeField] private bool cancelOnLeave = true;     // Alandan çıkınca iptal

    private bool playerInZone = false;
    private bool isActivating = false;
    private float activationTimer = 0f;

    // UI bağlanır: kalan süre / toplam süre
    public float ActivationProgress => isActivating ? 1f - (activationTimer / activationDuration) : 0f;
    public bool IsActivating => isActivating;

    // Events
    public event Action OnActivationStarted;
    public event Action OnActivationCancelled;
    public event Action OnExtractionComplete; // RunManager dinler

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<InventorySystem>() == null) return; // Sadece oyuncu
        playerInZone = true;

        if (!isActivating)
        {
            isActivating = true;
            activationTimer = activationDuration;
            OnActivationStarted?.Invoke();
            Debug.Log($"[ExtractionPoint] Çıkış aktifleşti — {activationDuration} sn bekle.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<InventorySystem>() == null) return;
        playerInZone = false;

        if (isActivating && cancelOnLeave)
        {
            isActivating = false;
            OnActivationCancelled?.Invoke();
            Debug.Log("[ExtractionPoint] Çıkış iptal — alandan ayrıldın.");
        }
    }

    private void Update()
    {
        if (!isActivating) return;

        activationTimer -= Time.deltaTime;

        if (activationTimer <= 0f)
        {
            isActivating = false;
            OnExtractionComplete?.Invoke();
            Debug.Log("[ExtractionPoint] EXTRACTION COMPLETE — dalış başarılı!");
        }
    }
}
