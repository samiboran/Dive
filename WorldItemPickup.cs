using UnityEngine;

/// <summary>
/// Sahnede duran, alınabilir item. InventorySystem trigger'ı bunu bulur.
/// Drop edilen item'lar da bu component'i taşıyan WorldPrefab'dan spawn olur.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WorldItemPickup : MonoBehaviour
{
    [field: SerializeField] public SO_ItemData ItemData { get; private set; }

    public void Consume()
    {
        Destroy(gameObject);
    }
}
