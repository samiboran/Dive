using UnityEngine;

[CreateAssetMenu(fileName = "SO_HarpoonData", menuName = "UnderwaterExtraction/Harpoon Data")]
public class SO_HarpoonData : ScriptableObject
{
    [Header("Combat")]
    [SerializeField] private float damage = 35f;
    [SerializeField] private float range = 15f;
    [SerializeField] private float projectileSpeed = 25f;

    [Header("Firing")]
    [SerializeField] private float fireRate = 1f; // shots per second
    [SerializeField] private float reloadDuration = 1.2f;

    [Header("Prefab")]
    [SerializeField] private GameObject projectilePrefab;

    public float Damage => damage;
    public float Range => range;
    public float ProjectileSpeed => projectileSpeed;
    public float FireRate => fireRate;
    public float ReloadDuration => reloadDuration;
    public GameObject ProjectilePrefab => projectilePrefab;
}
