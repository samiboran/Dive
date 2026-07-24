using UnityEngine;

/// <summary>
/// Attach to objects/gear that should intercept an incoming harpoon
/// before it can reach the entity behind it (e.g. a raised shield).
/// HarpoonProjectile destroys itself right after calling BlockHarpoon().
/// </summary>
public class HarpoonBlocker : MonoBehaviour
{
    [SerializeField] private GameObject blockVfxPrefab;

    public void BlockHarpoon(HarpoonProjectile projectile)
    {
        Debug.Log($"[HarpoonBlocker] Blocked harpoon from {(projectile.Owner != null ? projectile.Owner.name : "unknown")}.");

        if (blockVfxPrefab != null)
            Instantiate(blockVfxPrefab, projectile.transform.position, Quaternion.identity);
    }
}
