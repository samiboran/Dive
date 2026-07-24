using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AirPocketVolume : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var oxygen = other.GetComponent<OxygenSystem>();
        oxygen?.EnterAirPocket();
    }

    private void OnTriggerExit(Collider other)
    {
        var oxygen = other.GetComponent<OxygenSystem>();
        oxygen?.ExitAirPocket();
    }
}
