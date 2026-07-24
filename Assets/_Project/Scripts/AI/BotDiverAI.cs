using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Rakip dalgıç AI'ı. Raycast tabanlı line-of-sight + oyuncunun bakış
/// açısına göre yüzyüze/arkadan ayrımı yapar:
///   - Yüzyüze (oyuncu botu görüş konisinde görür) -> bot KAÇAR
///   - Arkadan (bot oyuncuyu, oyuncu botu FARK ETMEDEN görür) -> SALDIRIR
///
/// Tier davranışları:
///   Yellow — vur-kaç: saldırı sonrası CatchWindow süresince yavaş kaçar,
///            oyuncu bu sürede yakalarsa (CatchRange) bot çok yavaşlar ve
///            kolay av haline gelir; süre dolarsa alanı tamamen terk eder.
///   Red    — yaralar+kaçar ama alanı terk etmez, LurkRadius içinde dolaşmaya
///            devam eder (Lurking) — tekrar karşılaşma/tehdit oluşturabilir.
///   Spear  — menzilli zıpkın atar, mesafeyi korur. Oyuncunun "Limb" tag'li
///            collider'ına isabet ederse PlayerLimbHitHandler tetiklenir
///            (yavaşlama + ekstra O2 kaybı + SharkBehavior kan lure).
///
/// Saldırı türü seçimi (Yellow/Red): bıçak yarası / maske çıkarma / regülatör
/// çekme arasından rastgele — spec'te kesin ayrım verilmediği için uniform
/// dağılım kullanıldı, dengelemede ayrıştırılabilir.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BotDiverAI : MonoBehaviour, IDamageable
{
    public enum BotState { Patrol, Attacking, CatchWindow, Lurking, Fleeing, Retreating, Dead }

    [Header("Data")]
    [SerializeField] private SO_BotDiverData data;

    [Header("Melee Range (Yellow/Red)")]
    [SerializeField] private float meleeAttackRange = 2f;

    private BotState state = BotState.Patrol;
    private float currentHealth;
    private float stateTimer;
    private bool isCaught = false;
    private Vector3 patrolTarget;
    private Vector3 lurkCenter;
    private float spearCooldownTimer;

    private Transform player;
    private PlayerEquipmentState playerEquipment;
    private InjurySystem playerInjury;
    private Rigidbody rb;

    public BotState CurrentState => state;
    public BotDiverTier Tier => data.Tier;

    // Events — VFX/Audio bağlanır
    public event Action OnAttackPerformed;
    public event Action OnFleeStarted;
    public event Action OnCaught;   // Tier Yellow, CatchWindow içinde yakalanınca
    public event Action OnDied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void Start()
    {
        currentHealth = data.MaxHealth;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerEquipment = playerObj.GetComponent<PlayerEquipmentState>();
            playerInjury = playerObj.GetComponent<InjurySystem>();
        }

        PickNewPatrolTarget();
    }

    private void Update()
    {
        if (state == BotState.Dead) return;

        switch (state)
        {
            case BotState.Patrol:
                TickPatrol();
                break;
            case BotState.CatchWindow:
                TickCatchWindow();
                break;
            case BotState.Lurking:
                TickLurking();
                break;
            case BotState.Fleeing:
                TickFleeing();
                break;
            case BotState.Retreating:
                TickRetreating();
                break;
            // Attacking: PerformAttack coroutine'i yürütüyor, Update'te iş yok
        }

        if (spearCooldownTimer > 0f)
            spearCooldownTimer -= Time.deltaTime;
    }

    // ── Tespit ───────────────────────────────────────────────────

    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > data.DetectionRange) return false;

        if (Physics.Raycast(transform.position, toPlayer.normalized, out RaycastHit hit, dist, data.VisionBlockLayers))
        {
            if (!hit.transform.IsChildOf(player) && hit.transform != player)
                return false; // araya bir engel girdi
        }

        return true;
    }

    /// <summary>Oyuncu botu görüş açısı içinde mi (yüzyüze) yoksa fark etmiyor mu (arkadan)?</summary>
    private bool IsPlayerFacingBot()
    {
        if (player == null) return false;
        Vector3 toBot = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toBot);
        return angle <= data.PlayerViewAngleThreshold * 0.5f;
    }

    // ── Patrol / Lurk ────────────────────────────────────────────

    private void TickPatrol()
    {
        MoveTowards(patrolTarget, data.PatrolSpeed);
        if (Vector3.Distance(transform.position, patrolTarget) < 2f)
            PickNewPatrolTarget();

        EvaluateEncounter();
    }

    private void TickLurking()
    {
        MoveTowards(patrolTarget, data.PatrolSpeed);
        if (Vector3.Distance(transform.position, patrolTarget) < 2f)
            PickNewLurkTarget();

        EvaluateEncounter();
    }

    private void EvaluateEncounter()
    {
        if (!HasLineOfSightToPlayer()) return;

        if (IsPlayerFacingBot())
        {
            EnterFleeing();
            return;
        }

        // Arkadan tespit — menzile girdiyse saldır, girmediyse yaklaş
        float dist = Vector3.Distance(transform.position, player.position);
        float requiredRange = data.Tier == BotDiverTier.Spear ? data.SpearRange : meleeAttackRange;

        if (dist <= requiredRange)
            StartCoroutine(PerformAttack());
        else
            MoveTowards(player.position, data.ApproachSpeed);
    }

    // ── Saldırı ──────────────────────────────────────────────────

    private IEnumerator PerformAttack()
    {
        state = BotState.Attacking;
        rb.velocity = Vector3.zero;

        if (data.Tier == BotDiverTier.Spear)
        {
            FireSpear();
        }
        else
        {
            int roll = UnityEngine.Random.Range(0, 3);
            switch (roll)
            {
                case 0: playerInjury?.ApplyInjury(InjuryType.KnifeWound); break;
                case 1: playerEquipment?.KnockOffMask(); break;
                case 2: playerEquipment?.PullRegulator(); break;
            }
        }

        OnAttackPerformed?.Invoke();
        yield return new WaitForSeconds(0.3f); // vuruş sonrası kısa donma

        if (data.Tier == BotDiverTier.Yellow)
            EnterCatchWindow();
        else if (data.Tier == BotDiverTier.Red)
            EnterLurking();
        else
            state = BotState.Patrol; // Spear: mesafe koruyup tekrar değerlendirir
    }

    private void FireSpear()
    {
        if (spearCooldownTimer > 0f || data.SpearProjectilePrefab == null || player == null) return;
        spearCooldownTimer = data.SpearFireCooldown;

        Vector3 dir = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(data.SpearProjectilePrefab, transform.position, Quaternion.LookRotation(dir));
        var projectile = proj.GetComponent<HarpoonProjectile>();
        projectile?.Initialize(data.SpearDamage, data.SpearRange, data.SpearProjectileSpeed, dir, gameObject);

        // Limb-hit efekti (yavaşlama+O2 drain+shark lure), HarpoonProjectile'ın
        // çarptığı collider "Limb" tag'liyse PlayerLimbHitHandler üzerinden tetiklenir.
    }

    // ── Tier: Yellow — Catch Window ──────────────────────────────

    private void EnterCatchWindow()
    {
        state = BotState.CatchWindow;
        stateTimer = data.CatchWindowDuration;
        isCaught = false;
    }

    private void TickCatchWindow()
    {
        MoveAwayFrom(player.position, data.FleeSpeed);
        stateTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= data.CatchRange)
        {
            isCaught = true;
            OnCaught?.Invoke();
            state = BotState.Retreating;
            return;
        }

        if (stateTimer <= 0f)
            state = BotState.Retreating;
    }

    private void TickRetreating()
    {
        // Yakalandıysa çok yavaş kaçar — harpoon ile kolay avlanabilir hale gelir
        float speed = isCaught ? data.FleeSpeed * 0.25f : data.FleeSpeed;
        MoveAwayFrom(player != null ? player.position : transform.position + transform.forward, speed);

        if (player != null && Vector3.Distance(transform.position, player.position) > data.DetectionRange * 2f)
            Destroy(gameObject); // alandan tamamen ayrıldı
    }

    // ── Tier: Red — Lurking ──────────────────────────────────────

    private void EnterLurking()
    {
        state = BotState.Lurking;
        lurkCenter = transform.position;
        PickNewLurkTarget();
    }

    private void PickNewLurkTarget()
    {
        Vector2 rand = UnityEngine.Random.insideUnitCircle * data.LurkRadius;
        patrolTarget = lurkCenter + new Vector3(rand.x, UnityEngine.Random.Range(-2f, 2f), rand.y);
    }

    // ── Yüzyüze Kaçış ────────────────────────────────────────────

    private void EnterFleeing()
    {
        state = BotState.Fleeing;
        OnFleeStarted?.Invoke();
    }

    private void TickFleeing()
    {
        MoveAwayFrom(player.position, data.FleeSpeed);

        if (Vector3.Distance(transform.position, player.position) > data.DetectionRange * 1.5f)
            state = BotState.Patrol;
    }

    // ── Hasar / Ölüm ─────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (state == BotState.Dead) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        state = BotState.Dead;
        OnDied?.Invoke();
        DropLoot();
        Debug.Log($"[BotDiverAI] {data.BotName} öldürüldü — loot bırakıldı.");
        Destroy(gameObject, 0.1f);
    }

    private void DropLoot()
    {
        // Oksijen tüpü takviyesi her zaman düşer (SpareTank tipi SO_ConsumableItemData
        // WorldPrefab'ı bu bota manuel referans verilmeli — Inspector kurulumu).
        // Rastgele ek item, DeathLootTable'dan ağırlıklı tek çekiliş ile.
        if (data.DeathLootTable == null) return;

        int totalWeight = data.DeathLootTable.GetTotalWeight();
        if (totalWeight <= 0) return;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (var item in data.DeathLootTable.Items)
        {
            if (item == null) continue;
            roll -= Mathf.Max(0, item.RarityWeight);
            if (roll < 0)
            {
                if (item.WorldPrefab != null)
                    Instantiate(item.WorldPrefab, transform.position, Quaternion.identity);
                break;
            }
        }
    }

    // ── Hareket ──────────────────────────────────────────────────

    private void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        rb.velocity = dir * speed;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 3f * Time.deltaTime);
    }

    private void MoveAwayFrom(Vector3 from, float speed)
    {
        Vector3 dir = (transform.position - from).normalized;
        rb.velocity = dir * speed;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 3f * Time.deltaTime);
    }

    private void PickNewPatrolTarget()
    {
        Vector2 rand = UnityEngine.Random.insideUnitCircle * 20f;
        patrolTarget = transform.position + new Vector3(rand.x, UnityEngine.Random.Range(-3f, 3f), rand.y);
    }
}
