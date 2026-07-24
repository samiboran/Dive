using UnityEngine;

/// <summary>
/// Oksijen tüpü verisi. SO_ItemData'dan türer ki envanter/loot sistemine
/// diğer item'lar gibi girebilsin (ItemName tank ismini de karşılar).
/// </summary>
[CreateAssetMenu(fileName = "SO_TankData", menuName = "UnderwaterExtraction/Tank Data")]
public class SO_TankData : SO_ItemData
{
    [SerializeField] private float capacitySeconds = 1800f;

    public float CapacitySeconds => capacitySeconds;
}
