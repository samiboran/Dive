using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tüm SO_ItemData asset'lerinin kayıt defteri.
/// Save/load sırasında item'lar ItemName ile kaydedilir ve buradan çözülür.
///
/// Kurulum: Assets'te bir tane oluştur (Create → UnderwaterExtraction → ItemDatabase),
/// tüm item SO'larını Items listesine sürükle. StashSystem'e referans ver.
/// </summary>
[CreateAssetMenu(fileName = "SO_ItemDatabase", menuName = "UnderwaterExtraction/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [field: SerializeField] public List<SO_ItemData> Items { get; private set; }

    private Dictionary<string, SO_ItemData> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<string, SO_ItemData>();
        if (Items == null) return;
        foreach (var item in Items)
        {
            if (item != null && !lookup.ContainsKey(item.ItemName))
                lookup[item.ItemName] = item;
        }
    }

    public SO_ItemData FindByName(string itemName)
    {
        if (lookup != null && lookup.TryGetValue(itemName, out var item))
            return item;

        Debug.LogWarning($"[ItemDatabase] '{itemName}' bulunamadı — database'e ekli mi?");
        return null;
    }
}
