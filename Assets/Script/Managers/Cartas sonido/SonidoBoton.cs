using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class SonidoBoton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Game Manager")]
    [SerializeField] SoundsGameManager gm;
    private XRSimpleInteractable inter;

    private void Awake()
    {
        inter = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        inter.selectEntered.AddListener(OnPressed);
    }

    private void OnDisable()
    {
        inter.selectEntered.RemoveListener(OnPressed);
    }

    private void OnPressed(SelectEnterEventArgs _)
    {
        StartCoroutine(gm.ReproducirAudioActual(0));
    }
}
