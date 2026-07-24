using UnityEngine;

/// <summary>
/// Minimal consumable that cancels panic immediately and grants temporary
/// panic immunity. No InventorySystem exists yet (still planned for Phase 1),
/// so this works standalone: pick it up as a world trigger, or call Use()
/// directly once a hotbar/inventory exists to route into it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AdrenalineItem : MonoBehaviour
{
    [SerializeField] private float immunityDuration = 8f;
    [SerializeField] private bool consumeOnTriggerEnter = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!consumeOnTriggerEnter) return;

        var panicState = other.GetComponent<PanicState>() ?? other.GetComponentInParent<PanicState>();
        if (panicState == null) return;

        Use(panicState);
    }

    public void Use(PanicState panicState)
    {
        panicState.ApplyAdrenaline(immunityDuration);
        Destroy(gameObject);
    }
}
