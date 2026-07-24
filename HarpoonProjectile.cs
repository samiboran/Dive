using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HarpoonProjectile : MonoBehaviour
{
    private float damage;
    private float maxRange;
    private float speed;
    private Vector3 fireDirection;
    private GameObject owner;
    private Vector3 startPosition;
    private bool isStuck = false;
    private Rigidbody rb;

    public bool IsStuck => isStuck;

    // Zıpkın bu zıpkını atan silaha geri dönebilsin diye owner'a referans
    public GameObject Owner => owner;

    public void Initialize(float dmg, float range, float spd, Vector3 dir, GameObject ownerRef)
    {
        damage = dmg;
        maxRange = range;
        speed = spd;
        fireDirection = dir.normalized;
        owner = ownerRef;
        startPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        // FIX: Unity 2022.3 LTS — linearVelocity yerine velocity
        rb.velocity = fireDirection * speed;
    }

    private void Update()
    {
        if (isStuck) return;

        // Max range check
        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            StickToSurface(transform.position, -fireDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isStuck) return;
        if (other.gameObject == owner) return;

        // HarpoonBlocker check
        var blocker = other.GetComponent<HarpoonBlocker>();
        if (blocker != null)
        {
            blocker.BlockHarpoon(this);
            Destroy(gameObject);
            return;
        }

        // Hit entity
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        StickToSurface(transform.position, -fireDirection);
    }

    private void StickToSurface(Vector3 pos, Vector3 normal)
    {
        isStuck = true;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        transform.position = pos;

        // Suya veya yüzeye saplanma — toplanabilir hale gelir
        int pickupLayer = LayerMask.NameToLayer("Pickup");
        if (pickupLayer >= 0)
            gameObject.layer = pickupLayer;
        else
            Debug.LogWarning("[HarpoonProjectile] 'Pickup' layer tanımlı değil — Tags & Layers'tan ekle.");
    }

    public void PickUp()
    {
        // Toplayan silaha zıpkını geri kazandır
        if (owner != null)
        {
            var weapon = owner.GetComponent<HarpoonWeapon>();
            weapon?.ReturnHarpoon(1);
        }
        Destroy(gameObject);
    }
}
