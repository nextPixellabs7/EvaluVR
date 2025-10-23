using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SoundCardFather : MonoBehaviour
{

    [Header("Identidad")]
    [SerializeField] private int id;
    [SerializeField] private bool correcta;
    [SerializeField] bool colocada;



    // Componentes
    Rigidbody rb;
    Collider col;

    // Getters and Setters
    public int GetIDCard() => id;
    public void SetIDCard(int v) => id = v;
    public bool GetCorrecta() => correcta;
    public void SetCorrecta(bool val) => correcta = val;
    public bool GetColocada() => colocada;
    public void SetColocada(bool val) => colocada = val;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    // Alinear al socket
    public void AlinearEn(Transform destino)
    {
        transform.SetPositionAndRotation(destino.position, destino.rotation);
        transform.SetParent(destino);
    }

    public void BloquearEncontrada()
    {
        correcta = true;
        colocada = true;
        if (rb) rb.useGravity = false;
        if (rb) rb.isKinematic = true;

        var grab = GetComponent<XRGrabInteractable>();
        if (grab) grab.enabled = false;
    }

    public void BloquearErronea()
    {
        correcta = false;
        colocada = true;
        if (rb) rb.useGravity = false;
        if (rb) rb.isKinematic = true;

        var grab = GetComponent<XRGrabInteractable>();
        if (grab) grab.enabled = false;
    }

}
