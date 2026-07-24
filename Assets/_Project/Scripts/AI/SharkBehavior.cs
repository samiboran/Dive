using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Köpekbalığı davranışı — öngörülemez korku için açlık döngüsü + state machine.
///
/// Döngü: doygunluk zamanla azalır, periyodik (rastgele aralıklı) feeding
/// denemesi başarısız olursa saldırganlık eğilimi artar. Oyuncu bunu göremez;
/// tek görsel ipucu Circling (saldırı öncesi 4-8 sn tur).
///
/// İki bağımsız bölge-hassasiyeti:
/// 1) Kan lure — oyuncuda aktif SharkBite varsa saldırganlık eşiği düşer
/// 2) Hit-zone — kafa ("Head" tag'li child collider) vuruşu = kalıcı kaçış,
///    gövde = geçici kaçış + bir kez geri dönüş (Circling'den başlayarak)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SharkBehavior : MonoBehaviour, IDamageable
{
    public enum SharkState { Patrol, Feeding, Circling, Attacking, Pursuing, Fleeing }

    [Header("Hunger")]
    [SerializeField] private float maxSatiety = 100f;
    [SerializeField] private float satietyDecayPerSecond = 0.5f;
    [SerializeField] private Vector2 feedingAttemptInterval = new Vector2(20f, 45f); // rastgele aralık
    [SerializeField, Range(0f, 1f)] private float feedingSuccessChance = 0.6f;
    [SerializeField] private float feedingSatietyGain = 40f;
    [SerializeField] private float failedFeedingAggressionGain = 15f; // başarısız deneme = +saldırganlık
    [SerializeField] private float attackSatietyThreshold = 35f;      // bu doygunluğun altı tehlikeli

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float circleSpeed = 6f;
    [SerializeField] private float attackSpeed = 9f;
    [SerializeField] private float pursueSpeed = 7f;
    [SerializeField] private float fleeSpeed = 10f;
    [SerializeField] private float circleRadius = 8f;
    [SerializeField] private Vector2 circlingDuration = new Vector2(4f, 8f); // tell penceresi

    [Header("Ranges")]
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float biteRange = 2f;
    [SerializeField] private float pursueGiveUpRange = 40f;
    [SerializeField] private float pursueDuration = 12f;

    [Header("Flee / Hit-Zone")]
    [SerializeField] private float fleeDamageThreshold = 20f; // tek vuruşta bu hasar = kaçış tetikler
    [SerializeField] private float temporaryFleeDuration = 15f;

    // State
    private SharkState state = SharkState.Patrol;
    private float satiety;
    private float aggressionBonus = 0f;  // başarısız feeding birikimi
    [SerializeField] private float aggressionDecayPerSecond = 1.5f; // sonsuz birikimi önler
    private float stateTimer = 0f;
    private bool permanentFlee = false;
    private bool returnedOnce = false;   // gövde vuruşu sonrası tek geri dönüş hakkı

    private Transform player;
    private InjurySystem playerInjury;
    private Rigidbody rb;
    private Vector3 patrolTarget;
    private float circleAngle = 0f;

    public SharkState CurrentState => state;
    public float Satiety => satiety;

    // Events — VFX/Audio bağlanır
    public event Action OnCirclingStarted;   // oyuncunun tek "tell"i
    public event Action OnAttackStarted;
    public event Action OnFleeStarted;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void Start()
    {
        satiety = UnityEngine.Random.Range(40f, 90f); // rastgele başlangıç — kestirilemezlik
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerInjury = playerObj.GetComponent<InjurySystem>();
        }
        StartCoroutine(FeedingLoop());
        PickNewPatrolTarget();
    }

    // ── Açlık döngüsü (oyuncu bunu göremez) ─────────────────────
    private IEnumerator FeedingLoop()
    {
        while (!permanentFlee)
        {
            float wait = UnityEngine.Random.Range(feedingAttemptInterval.x, feedingAttemptInterval.y);
            yield return new WaitForSeconds(wait);

            // FIX: Circling (saldırı tell'i) ve Fleeing (kaçış/geri dönüş kuralı)
            // feeding denemesiyle KESİLEMEZ
            if (state == SharkState.Attacking || state == SharkState.Pursuing
                || state == SharkState.Circling || state == SharkState.Fleeing)
                continue;

            if (UnityEngine.Random.value < feedingSuccessChance)
            {
                state = SharkState.Feeding;
                satiety = Mathf.Min(satiety + feedingSatietyGain, maxSatiety);
                stateTimer = 3f; // kısa feeding animasyon süresi
            }
            else
            {
                aggressionBonus += failedFeedingAggressionGain;
            }
        }
    }

    // Kan lure: hem shark bite hem de BotDiverAI'nin bıçak/zıpkın yaralarından kanama sürüyor
    private bool PlayerIsBleeding =>
        playerInjury != null &&
        (playerInjury.GetCurrentInjury() == InjuryType.SharkBite ||
         playerInjury.GetCurrentInjury() == InjuryType.KnifeWound);

    // Circling tetikleyicisi: doygunluk düşük VEYA oyuncu kanıyor (iki bağımsız OR)
    private bool ShouldCirclePlayer()
    {
        if (player == null) return false;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        bool hungry = (satiety - aggressionBonus) < attackSatietyThreshold;
        bool bloodLure = PlayerIsBleeding;
        return hungry || bloodLure;
    }

    // ── Hasar / hit-zone ─────────────────────────────────────────
    public void TakeDamage(float amount) => HandleHit(amount, isHeadShot: false);

    /// <summary>
    /// Kafa vuruşu için HarpoonProjectile tarafında ayrı çağrı:
    /// projectile, çarptığı collider "Head" tag'li ise bunu çağırır.
    /// </summary>
    public void HandleHeadShot(float amount) => HandleHit(amount, isHeadShot: true);

    private void HandleHit(float amount, bool isHeadShot)
    {
        if (permanentFlee) return;
        if (amount < fleeDamageThreshold) return;

        if (isHeadShot)
        {
            permanentFlee = true;
            EnterState(SharkState.Fleeing);
            Debug.Log("[SharkBehavior] KAFA VURUŞU — kalıcı kaçış, bir daha dönmez.");
        }
        else
        {
            // Gövde: geçici kaçış + tek geri dönüş hakkı
            if (returnedOnce)
            {
                permanentFlee = true; // ikinci kez vuruldu = artık kalıcı
                EnterState(SharkState.Fleeing);
                return;
            }
            returnedOnce = true;
            EnterState(SharkState.Fleeing);
            stateTimer = temporaryFleeDuration;
            Debug.Log("[SharkBehavior] Gövde vuruşu — geçici kaçış, bir kez geri gelecek.");
        }
    }

    // ── State machine ────────────────────────────────────────────
    private void Update()
    {
        if (permanentFlee && state == SharkState.Fleeing)
        {
            MoveAwayFrom(player != null ? player.position : transform.position + transform.forward, fleeSpeed);
            return; // bir daha hiçbir state'e girmez
        }

        satiety = Mathf.Max(0f, satiety - satietyDecayPerSecond * Time.deltaTime);
        // FIX: aggressionBonus zamanla söner — kalıcı saldırganlığa dönüşmez
        aggressionBonus = Mathf.Max(0f, aggressionBonus - aggressionDecayPerSecond * Time.deltaTime);

        switch (state)
        {
            case SharkState.Patrol:
                TickPatrol();
                if (ShouldCirclePlayer()) EnterCircling();
                break;

            case SharkState.Feeding:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f) EnterState(SharkState.Patrol);
                break;

            case SharkState.Circling:
                TickCircling();
                break;

            case SharkState.Attacking:
                TickAttacking();
                break;

            case SharkState.Pursuing:
                TickPursuing();
                break;

            case SharkState.Fleeing:
                TickFleeing();
                break;
        }
    }

    private void EnterState(SharkState newState)
    {
        state = newState;
        if (newState == SharkState.Circling)
        {
            stateTimer = UnityEngine.Random.Range(circlingDuration.x, circlingDuration.y);
            OnCirclingStarted?.Invoke();
        }
        else if (newState == SharkState.Attacking)
        {
            aggressionBonus = 0f; // saldırı gerçekleşti — birikim boşalır
            OnAttackStarted?.Invoke();
        }
        else if (newState == SharkState.Fleeing)
        {
            OnFleeStarted?.Invoke();
        }
    }

    private void EnterCircling() => EnterState(SharkState.Circling);

    private void TickPatrol()
    {
        MoveTowards(patrolTarget, patrolSpeed);
        if (Vector3.Distance(transform.position, patrolTarget) < 2f)
            PickNewPatrolTarget();
    }

    private void TickCircling()
    {
        if (player == null) { EnterState(SharkState.Patrol); return; }

        // Oyuncu etrafında hızlı tur
        circleAngle += (circleSpeed / circleRadius) * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(circleAngle), 0f, Mathf.Sin(circleAngle)) * circleRadius;
        Vector3 targetPos = player.position + offset;
        MoveTowards(targetPos, circleSpeed);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            EnterState(SharkState.Attacking);
    }

    private void TickAttacking()
    {
        if (player == null) { EnterState(SharkState.Patrol); return; }

        MoveTowards(player.position, attackSpeed);

        if (Vector3.Distance(transform.position, player.position) <= biteRange)
        {
            playerInjury?.ApplyInjury(InjuryType.SharkBite);
            EnterState(SharkState.Pursuing);
            stateTimer = pursueDuration;
        }
    }

    private void TickPursuing()
    {
        if (player == null) { EnterState(SharkState.Patrol); return; }

        MoveTowards(player.position, pursueSpeed);
        stateTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        // Isırık menziline tekrar girerse bir ısırık daha
        if (dist <= biteRange)
        {
            playerInjury?.ApplyInjury(InjuryType.SharkBite);
        }

        if (dist > pursueGiveUpRange || stateTimer <= 0f)
        {
            // Gövde vuruşu sonrası geri dönüş hakkını kullandıysa normal döngüye dön
            EnterState(SharkState.Patrol);
        }
    }

    private void TickFleeing()
    {
        Vector3 from = player != null ? player.position : transform.position + transform.forward;
        MoveAwayFrom(from, fleeSpeed);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f && !permanentFlee)
        {
            // Bir kez geri dönüş — Circling'den başlar (oyuncuya adil tell)
            EnterState(SharkState.Circling);
            Debug.Log("[SharkBehavior] Geri döndü — circling başlıyor.");
        }
    }

    // ── Hareket yardımcıları ─────────────────────────────────────
    private void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        rb.velocity = dir * speed;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), 3f * Time.deltaTime);
    }

    private void MoveAwayFrom(Vector3 from, float speed)
    {
        Vector3 dir = (transform.position - from).normalized;
        rb.velocity = dir * speed;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), 3f * Time.deltaTime);
    }

    private void PickNewPatrolTarget()
    {
        Vector2 rand = UnityEngine.Random.insideUnitCircle * 20f;
        patrolTarget = transform.position + new Vector3(rand.x, UnityEngine.Random.Range(-3f, 3f), rand.y);
    }
}
