using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// v2 — Sonuç verisi artık RunManager'ın payload'lı event'inden geliyor
/// (envanter taraması kaldırıldı — snapshot transfer ÖNCESİ alınıyor).
///
/// Genel oyun akışı: Hideout → Diving → Results döngüsü.
/// Singleton + DontDestroyOnLoad — sahne geçişlerinde yaşar.
/// </summary>
public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    public enum GameState { Hideout, Diving, Results }

    [Header("Scene Names (CONTEXT.md ile birebir)")]
    [SerializeField] private string hideoutSceneName = "Hideout";
    [SerializeField] private string diveSceneName = "Map_SunkenShip_01";

    [Serializable]
    public class ResultEntry
    {
        public string itemName;
        public int count;
    }

    [Serializable]
    public class ResultsData
    {
        public bool extracted;
        public List<ResultEntry> items = new List<ResultEntry>();
    }

    public GameState CurrentState { get; private set; } = GameState.Hideout;
    public ResultsData LastResult { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => SubscribeRunManager();

    private void OnEnable() => SceneManager.sceneLoaded += HandleSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => SubscribeRunManager();

    private void SubscribeRunManager()
    {
        if (RunManager.Instance == null) return;

        RunManager.Instance.OnRunExtracted -= HandleExtracted; // çift abonelik koruması
        RunManager.Instance.OnRunFailed -= HandleFailed;
        RunManager.Instance.OnRunExtracted += HandleExtracted;
        RunManager.Instance.OnRunFailed += HandleFailed;
    }

    /// <summary>Hideout sahnesindeki "Dalışa Başla" butonu buna bağlanır.</summary>
    public void BeginDive()
    {
        if (CurrentState == GameState.Diving) return;
        CurrentState = GameState.Diving;
        SceneManager.LoadScene(diveSceneName);
        // StartRun'ı sahnedeki RunManager kendi Start'ında çağırır.
    }

    private void HandleExtracted(RunManager.ResultPayload payload) => CaptureResult(payload);
    private void HandleFailed(RunManager.ResultPayload payload) => CaptureResult(payload);

    private void CaptureResult(RunManager.ResultPayload payload)
    {
        var result = new ResultsData { extracted = payload.extracted };

        foreach (var item in payload.items)
        {
            result.items.Add(new ResultEntry
            {
                itemName = item.itemName,
                count = item.count
            });
        }

        LastResult = result;
        CurrentState = GameState.Results;
        SceneManager.LoadScene(hideoutSceneName); // sonuç ekranı hideout içinde gösterilir
    }
}
