using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Genel amaçlı ritim tabanlı kurtarma mini-oyunu (maske/regülatör takma).
/// N adet "beat" üretir, her biri için kısa bir vuruş penceresi açılır.
/// Zamanında basılan (F) vuruş maliyetsiz, kaçırılan vuruş missedCount'a eklenir.
///
/// Dizi KAÇIRILSA BİLE her zaman tamamlanır — başarısızlık/tekrar deneme yok,
/// sadece kaçırılan vuruş sayısına göre dışarıdan (PlayerEquipmentState) ekstra
/// oksijen maliyeti uygulanır. Bu tasarım bilinçli: oyuncu asla tıkanmaz,
/// sadece ne kadar iyi tepki verdiğine göre fatura değişir.
/// </summary>
public class RhythmRecoveryQTE : MonoBehaviour
{
    [Header("Beat Timing")]
    [SerializeField] private Vector2Int beatCountRange = new Vector2Int(3, 5);
    [SerializeField] private float beatInterval = 0.7f;   // vuruşlar arası bekleme
    [SerializeField] private float hitWindow = 0.3f;       // vuruş penceresi genişliği
    [SerializeField] private KeyCode recoveryKey = KeyCode.F;

    public bool IsActive { get; private set; }

    // UI bağlanır
    public event Action OnSequenceStarted;
    public event Action<int, int> OnBeatWindowOpened;  // (beatIndex, totalBeats)
    public event Action<bool> OnBeatResult;            // true = isabet, false = kaçırıldı
    public event Action<int> OnSequenceCompleted;      // missedCount

    /// <summary>Kurtarma dizisini başlatır. Zaten aktifse yok sayılır.</summary>
    public void StartSequence()
    {
        if (IsActive) return;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        IsActive = true;
        OnSequenceStarted?.Invoke();

        int totalBeats = UnityEngine.Random.Range(beatCountRange.x, beatCountRange.y + 1);
        int missed = 0;

        for (int i = 0; i < totalBeats; i++)
        {
            yield return new WaitForSeconds(beatInterval);
            OnBeatWindowOpened?.Invoke(i, totalBeats);

            bool hit = false;
            float elapsed = 0f;
            while (elapsed < hitWindow)
            {
                if (Input.GetKeyDown(recoveryKey))
                {
                    hit = true;
                    break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!hit) missed++;
            OnBeatResult?.Invoke(hit);
        }

        IsActive = false;
        OnSequenceCompleted?.Invoke(missed);
    }
}
