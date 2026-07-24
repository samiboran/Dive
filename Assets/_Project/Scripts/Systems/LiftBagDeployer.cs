using UnityEngine;

/// <summary>
/// Oyuncu tarafı lift bag kontrolü: deploy, yükleme, salma.
/// Balon item'i envanterde SO_ItemData olarak durur (WorldPrefab = balon prefab).
///
/// Şişirme maliyeti: kendi tüpünden oksijen harcar (oxygenCost).
/// </summary>
public class LiftBagDeployer : MonoBehaviour
{
    [Header("Deploy")]
    [SerializeField] private KeyCode deployKey = KeyCode.G;   // balonu kur
    [SerializeField] private KeyCode loadKey = KeyCode.E;     // seçili slotu balona yükle
    [SerializeField] private KeyCode releaseKey = KeyCode.F;  // sal
    [SerializeField] private float deployDistance = 1.5f;

    [Header("Oxygen Cost")]
    [SerializeField] private float oxygenCost = 20f; // şişirme kendi havasından

    private InventorySystem inventory;
    private OxygenSystem oxygen;
    private LiftBalloon activeBalloon;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();
        oxygen = GetComponent<OxygenSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(deployKey))
            TryDeploy();

        if (activeBalloon != null)
        {
            if (Input.GetKeyDown(loadKey))
                TryLoadSelected();

            if (Input.GetKeyDown(releaseKey))
            {
                activeBalloon.Release();
                activeBalloon = null;
            }
        }
    }

    private void TryDeploy()
    {
        if (activeBalloon != null) return; // aynı anda tek balon

        // Seçili slotta balon item'ı var mı? (WorldPrefab'ında LiftBalloon olan)
        var slots = inventory.Slots;
        int idx = inventory.SelectedSlot;
        if (idx >= slots.Count) return;

        var slot = slots[idx];
        if (slot.IsEmpty || slot.Item.WorldPrefab == null) return;
        if (slot.Item.WorldPrefab.GetComponent<LiftBalloon>() == null) return;

        // Oksijen yeterli mi?
        if (oxygen != null && oxygen.GetCurrentOxygen() <= oxygenCost)
        {
            Debug.Log("[LiftBagDeployer] Şişirmek için yeterli havan yok!");
            return;
        }

        // Oksijeni harca
        oxygen?.RefillFromSpareTank(-oxygenCost); // negatif = tüketim

        Vector3 pos = transform.position + transform.forward * deployDistance;
        GameObject obj = Instantiate(slot.Item.WorldPrefab, pos, Quaternion.identity);
        activeBalloon = obj.GetComponent<LiftBalloon>();

        // Item'ı envanterden tüket (sahneye prefab bırakmadan, grid temizlenir)
        inventory.ConsumeSlot(idx);

        Debug.Log("[LiftBagDeployer] Balon kuruldu — E ile yükle, F ile sal.");
    }

    private void TryLoadSelected()
    {
        var slots = inventory.Slots;
        int idx = inventory.SelectedSlot;
        if (idx >= slots.Count) return;

        var slot = slots[idx];
        if (slot.IsEmpty) return;

        if (activeBalloon.LoadCargo(slot.Item, 1))
        {
            inventory.ConsumeSlot(idx); // grid temizliği + event güvenli
            Debug.Log($"[LiftBagDeployer] Yüklendi ({activeBalloon.CargoCount}/{activeBalloon.MaxSlots})");
        }
        else
        {
            Debug.Log("[LiftBagDeployer] Balon dolu!");
        }
    }
}
