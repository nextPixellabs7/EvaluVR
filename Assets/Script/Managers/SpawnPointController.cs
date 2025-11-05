using UnityEngine;
using Unity.XR.CoreUtils;

public class SpawnPointController : MonoBehaviour
{
    [Header("Spawn inicial del jugador")]
    [SerializeField] private Transform startSpawnPoint;

    [Header("Jugador")]
    [SerializeField] XROrigin playerRig;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Tepea el jugador a la posicion inicial donde debe aparecer
        if (playerRig != null && startSpawnPoint != null)
        {
            playerRig.MoveCameraToWorldLocation(startSpawnPoint.position);
            playerRig.MatchOriginUpCameraForward(startSpawnPoint.up, startSpawnPoint.forward);
        }
    }
}
