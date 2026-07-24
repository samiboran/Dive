using UnityEngine;

/// <summary>
/// Suya düşen maske — bulanık görüş içinde oyuncu yaklaşıp E ile bulur,
/// bulunca PlayerEquipmentState üzerinden ritim QTE'si başlar.
/// RefillStation/HarpoonPickup ile aynı etkileşim deseni: trigger + E.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MaskPickup : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private PlayerEquipmentState owner;
    private bool playerInRange = false;

    public void Init(PlayerEquipmentState equipmentOwner)
    {
        owner = equipmentOwner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.GetComponent<PlayerEquipmentState>() == owner)
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (owner != null && other.GetComponent<PlayerEquipmentState>() == owner)
            playerInRange = false;
    }

    private void Update()
    {
        if (!playerInRange || owner == null) return;
        if (!Input.GetKeyDown(interactKey)) return;

        owner.BeginMaskRecovery();
    }
}
