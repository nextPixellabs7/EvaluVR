using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotonSonido : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Game Manager")]
    [SerializeField] EscuchoYEncuentroManager gm;
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
