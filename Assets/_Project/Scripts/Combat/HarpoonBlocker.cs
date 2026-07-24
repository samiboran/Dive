using UnityEngine;

public class HarpoonBlocker : MonoBehaviour
{
    [SerializeField] private GameObject scrapPrefab;

    public void BlockHarpoon(HarpoonProjectile projectile)
    {
        // Item consume (bu script'in bağlı olduğu item'ı yok et)
        Destroy(gameObject);

        // Scrap spawn
        if (scrapPrefab != null)
        {
            Instantiate(scrapPrefab, transform.position, Quaternion.identity);
            Instantiate(scrapPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        Debug.Log("[HarpoonBlocker] Harpoon blocked — both converted to scrap.");
    }
}
