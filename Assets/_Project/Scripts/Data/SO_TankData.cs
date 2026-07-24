using UnityEngine;

[CreateAssetMenu(fileName = "SO_TankData", menuName = "UnderwaterExtraction/Tank Data")]
public class SO_TankData : ScriptableObject
{
    [SerializeField] private string tankName = "Standard Tank";
    [SerializeField] private float capacitySeconds = 1800f;

    public string TankName => tankName;
    public float CapacitySeconds => capacitySeconds;
}
