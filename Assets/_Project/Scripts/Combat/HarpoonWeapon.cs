using System;
using UnityEngine;

public class HarpoonWeapon : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private SO_HarpoonData harpoonData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask hitLayers;

    [Header("Ammo / Inventory")]
    [SerializeField] private int maxCarriedHarpoons = 6; // Taşınabilir toplam zıpkın
    [SerializeField] private int currentHarpoons = 6;

    [Header("Panic-Aim Coupling")]
    [SerializeField] private float maxSpreadAngleDegrees = 20f; // PanicState.AimAccuracyMultiplier=0 durumunda uygulanan sapma

    private float lastFireTime;
    private bool isReloading = false;
    private float reloadEndTime;
    private PanicState panicState;

    private void Awake()
    {
        // Silah oyuncunun root'unda veya bir child'ında olabilir
        panicState = GetComponentInParent<PanicState>();
    }

    public int CurrentHarpoons => currentHarpoons;
    public int MaxCarriedHarpoons => maxCarriedHarpoons;

    public event Action OnFired;
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;
    public event Action<int, int> OnAmmoChanged; // current, max
    public event Action OnOutOfAmmo;

    private void Update()
    {
        if (isReloading && Time.time >= reloadEndTime)
        {
            isReloading = false;
            OnReloadFinished?.Invoke();
        }
    }

    public void Fire(Vector3 direction)
    {
        if (isReloading) return;

        // FIX: stok kontrolü — zıpkın yoksa ateş edemez
        if (currentHarpoons <= 0)
        {
            OnOutOfAmmo?.Invoke();
            Debug.Log("[HarpoonWeapon] Zıpkın yok — suya saplanan zıpkınları topla veya refill istasyonu kullan.");
            return;
        }

        if (Time.time < lastFireTime + (1f / harpoonData.FireRate)) return;

        lastFireTime = Time.time;
        isReloading = true;
        reloadEndTime = Time.time + harpoonData.ReloadDuration;

        currentHarpoons--;
        OnAmmoChanged?.Invoke(currentHarpoons, maxCarriedHarpoons);

        OnFired?.Invoke();
        OnReloadStarted?.Invoke();

        SpawnProjectile(ApplyPanicSpread(direction));
    }

    private Vector3 ApplyPanicSpread(Vector3 direction)
    {
        if (panicState == null) return direction;

        float accuracy = panicState.AimAccuracyMultiplier; // 1 = perfect aim, lower = shakier
        float spreadDegrees = maxSpreadAngleDegrees * (1f - accuracy);
        if (spreadDegrees <= 0f) return direction;

        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spreadDegrees;
        Quaternion spreadRotation = Quaternion.Euler(randomOffset.y, randomOffset.x, 0f);
        return spreadRotation * direction;
    }

    private void SpawnProjectile(Vector3 direction)
    {
        if (harpoonData.ProjectilePrefab == null) return;

        GameObject proj = Instantiate(harpoonData.ProjectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        var projectile = proj.GetComponent<HarpoonProjectile>();

        projectile?.Initialize(
            harpoonData.Damage,
            harpoonData.Range,
            harpoonData.ProjectileSpeed,
            direction,
            gameObject // owner
        );
    }

    // Saplanan zıpkın toplandığında veya refill istasyonunda stok doldurur
    public void ReturnHarpoon(int count)
    {
        currentHarpoons = Mathf.Min(currentHarpoons + count, maxCarriedHarpoons);
        OnAmmoChanged?.Invoke(currentHarpoons, maxCarriedHarpoons);
    }

    public bool CanFire() => !isReloading && currentHarpoons > 0;
    public float GetReloadProgress() => isReloading ? 1f - ((reloadEndTime - Time.time) / harpoonData.ReloadDuration) : 1f;
}
