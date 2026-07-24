using UnityEngine;

/// <summary>
/// Suya/yüzeye saplanmış zıpkını E ile toplama.
/// RefillStation ile aynı etkileşim deseni: trigger alanı + E tuşu.
///
/// Kurulum: Oyuncunun üzerinde (veya Player child'ında) bir trigger collider
/// ile birlikte konur. Saplı HarpoonProjectile trigger'a girince aday olur,
/// E'ye basınca PickUp() çağrılır → zıpkın silaha ReturnHarpoon ile geri döner.
/// </summary>
public class HarpoonPickup : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private HarpoonProjectile nearbyHarpoon;

    private void OnTriggerEnter(Collider other)
    {
        var projectile = other.GetComponent<HarpoonProjectile>();
        if (projectile != null && projectile.IsStuck)
        {
            nearbyHarpoon = projectile;
            Debug.Log("[HarpoonPickup] Saplı zıpkın menzilde — E ile topla.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var projectile = other.GetComponent<HarpoonProjectile>();
        if (projectile != null && projectile == nearbyHarpoon)
            nearbyHarpoon = null;
    }

    private void Update()
    {
        if (nearbyHarpoon == null) return;
        if (!Input.GetKeyDown(interactKey)) return;

        // Zıpkın bu arada başka yolla yok edilmiş olabilir
        if (nearbyHarpoon == null || !nearbyHarpoon.IsStuck)
        {
            nearbyHarpoon = null;
            return;
        }

        nearbyHarpoon.PickUp(); // Sahibine ReturnHarpoon(1) gönderir ve kendini yok eder
        nearbyHarpoon = null;
    }
}
