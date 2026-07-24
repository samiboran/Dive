using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lift Bag (şamandıra) — su altından yüzeye loot taşır.
///
/// Akış: Deploy → item yükle (E) → serbest bırak (F) → yüksel → yüzeyde Stash'e.
/// Risk: Obstacle layer'ına takılır veya zıpkın (IDamageable) vurursa PATLAR;
/// yük aşağı ivmelenir, dibe inince tekrar lootlanabilir WorldItemPickup olur.
///
/// Süre: derinliğe bağlı (baseRiseTime + depth × depthTimeMultiplier).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LiftBalloon : MonoBehaviour, IDamageable
{
    [Header("Ascent")]
    [SerializeField] private float baseRiseTime = 30f;      // yüzey derinliği için
    [SerializeField] private float depthTimeMultiplier = 0.5f; // her metre +0.5 sn
    [SerializeField] private float surfaceY = 0f;           // su yüzeyi

    [Header("Capacity")]
    [SerializeField] private int maxSlots = 4; // balonun taşıma kapasitesi

    [Header("Fall")]
    [SerializeField] private float fallAcceleration = 3f;   // patlayınca aşağı ivme
    [SerializeField] private float popScatterRadius = 1.5f; // düşen loot'un saçılma yarıçapı

    public enum BalloonState { Deployed, Ascending, Popped, Delivered }

    private BalloonState state = BalloonState.Deployed;
    private readonly List<(SO_ItemData item, int count)> cargo = new List<(SO_ItemData, int)>();
    private float deployDepth;
    private float riseSpeed;
    private float fallSpeed = 0f;
    private Rigidbody rb;

    public BalloonState State => state;
    public int CargoCount => cargo.Count;
    public int MaxSlots => maxSlots;

    // Events — UI/VFX bağlanır
    public event Action OnReleased;
    public event Action OnPopped;
    public event Action OnDelivered;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // hareketi script yönetir
    }

    private void Start()
    {
        deployDepth = Mathf.Max(0f, -transform.position.y);
        float totalRiseTime = baseRiseTime + deployDepth * depthTimeMultiplier;
        riseSpeed = deployDepth / Mathf.Max(totalRiseTime, 1f);
    }

    /// <summary>Kargoya item yükle (deploy edilmişken).</summary>
    public bool LoadCargo(SO_ItemData item, int count)
    {
        if (state != BalloonState.Deployed) return false;
        if (cargo.Count >= maxSlots) return false;

        cargo.Add((item, count));
        return true;
    }

    /// <summary>Serbest bırak — yükseliş başlar.</summary>
    public void Release()
    {
        if (state != BalloonState.Deployed) return;
        state = BalloonState.Ascending;
        OnReleased?.Invoke();
        Debug.Log($"[LiftBalloon] Salındı — {cargo.Count} kalem, tahmini {deployDepth / riseSpeed:F0} sn.");
    }

    /// <summary>Zıpkın isabeti (IDamageable) — balon patlar.</summary>
    public void TakeDamage(float amount) => Pop();

    private void OnTriggerEnter(Collider other)
    {
        if (state != BalloonState.Ascending) return;

        // Engele takılma — Obstacle layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            Pop();
    }

    private void Pop()
    {
        if (state == BalloonState.Popped || state == BalloonState.Delivered) return;
        state = BalloonState.Popped;
        fallSpeed = 0f;
        OnPopped?.Invoke();
        Debug.Log("[LiftBalloon] PATLADI — yük düşüyor!");
    }

    private void Update()
    {
        switch (state)
        {
            case BalloonState.Ascending:
                transform.position += Vector3.up * (riseSpeed * Time.deltaTime);
                if (transform.position.y >= surfaceY)
                    Deliver();
                break;

            case BalloonState.Popped:
                fallSpeed += fallAcceleration * Time.deltaTime; // aşağı ivmelenir
                transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
                // Basit dip kontrolü: deploy derinliğinin altına düşünce loot'u bırak
                if (transform.position.y <= -deployDepth)
                    ScatterCargo();
                break;
        }
    }

    private void Deliver()
    {
        state = BalloonState.Delivered;

        if (StashSystem.Instance != null)
        {
            foreach (var (item, count) in cargo)
                StashSystem.Instance.TryStore(item, count);
        }

        OnDelivered?.Invoke();
        Debug.Log($"[LiftBalloon] Yüzeye ulaştı — {cargo.Count} kalem stash'e gönderildi.");
        Destroy(gameObject);
    }

    private void ScatterCargo()
    {
        // Yük dibe saçılır — tekrar lootlanabilir
        foreach (var (item, count) in cargo)
        {
            if (item.WorldPrefab == null) continue;

            Vector2 offset = UnityEngine.Random.insideUnitCircle * popScatterRadius;
            Vector3 pos = transform.position + new Vector3(offset.x, 0f, offset.y);
            Instantiate(item.WorldPrefab, pos, Quaternion.identity);
        }

        Debug.Log($"[LiftBalloon] {cargo.Count} kalem dibe saçıldı — tekrar toplanabilir.");
        Destroy(gameObject);
    }
}
