using System.Collections;
using UnityEngine;

/// <summary>
/// Oksijen bitince boğulma hasarı: OnOxygenDepleted'a subscribe olur,
/// saniyede drowningDps kadar hasar verir. Oksijen tekrar > 0 olursa durur.
/// </summary>
[RequireComponent(typeof(OxygenSystem))]
[RequireComponent(typeof(PlayerHealth))]
public class DrowningHandler : MonoBehaviour
{
    [Header("Drowning")]
    [SerializeField] private float drowningDps = 5f;      // saniyedeki hasar
    [SerializeField] private float tickInterval = 1f;     // hasar aralığı

    private OxygenSystem oxygenSystem;
    private PlayerHealth playerHealth;
    private Coroutine drowningCoroutine;

    private void Awake()
    {
        oxygenSystem = GetComponent<OxygenSystem>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        oxygenSystem.OnOxygenDepleted += StartDrowning;
        oxygenSystem.OnOxygenChanged += HandleOxygenChanged;
    }

    private void OnDisable()
    {
        oxygenSystem.OnOxygenDepleted -= StartDrowning;
        oxygenSystem.OnOxygenChanged -= HandleOxygenChanged;
    }

    private void StartDrowning()
    {
        if (drowningCoroutine != null) return;
        drowningCoroutine = StartCoroutine(DrowningCoroutine());
        Debug.Log("[DrowningHandler] Oksijen bitti — boğulma başladı!");
    }

    private void HandleOxygenChanged(float current, float max)
    {
        if (current > 0f && drowningCoroutine != null)
        {
            StopCoroutine(drowningCoroutine);
            drowningCoroutine = null;
            Debug.Log("[DrowningHandler] Oksijen geri geldi — boğulma durdu.");
        }
    }

    private IEnumerator DrowningCoroutine()
    {
        while (true)
        {
            playerHealth.TakeDamage(drowningDps);
            yield return new WaitForSeconds(tickInterval);
        }
    }
}
