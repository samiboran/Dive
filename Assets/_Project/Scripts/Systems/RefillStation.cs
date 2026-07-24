using UnityEngine;

/// <summary>
/// Sabit dolum istasyonu: oyuncu trigger alanındayken E ile etkileşip
/// oksijenini tamamen doldurur ve zıpkın stoğunu yeniler.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RefillStation : MonoBehaviour
{
    [Header("Refill Settings")]
    [SerializeField] private bool refillOxygen = true;
    [SerializeField] private bool refillHarpoons = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private OxygenSystem playerOxygen;
    private HarpoonWeapon playerWeapon;
    private bool playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        playerOxygen = other.GetComponent<OxygenSystem>();
        // FIX: HarpoonWeapon oyuncu root'unda da duruyor olabilir, child'da da
        playerWeapon = other.GetComponent<HarpoonWeapon>() ?? other.GetComponentInChildren<HarpoonWeapon>();
        playerInRange = playerOxygen != null;

        if (playerInRange)
            Debug.Log("[RefillStation] Menzile girildi — E ile doldur.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<OxygenSystem>() == playerOxygen)
        {
            playerInRange = false;
            playerOxygen = null;
            playerWeapon = null;
        }
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (!Input.GetKeyDown(interactKey)) return;

        Refill();
    }

    private void Refill()
    {
        if (refillOxygen && playerOxygen != null)
        {
            playerOxygen.RefillOxygen();
            Debug.Log("[RefillStation] Oksijen dolduruldu.");
        }

        if (refillHarpoons && playerWeapon != null)
        {
            int missing = playerWeapon.MaxCarriedHarpoons - playerWeapon.CurrentHarpoons;
            if (missing > 0)
            {
                playerWeapon.ReturnHarpoon(missing);
                Debug.Log($"[RefillStation] {missing} zıpkın yenilendi.");
            }
        }
    }
}
