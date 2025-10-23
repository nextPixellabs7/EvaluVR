using UnityEngine;
using UnityEngine.UI;

public class OpcionBoton : MonoBehaviour
{
    public bool esCorrecta; // Indica si esta opción es la correcta
    private Button boton;
    private EscuchoYEncuentroManager gestor;

    void Start()
    {
        boton = GetComponent<Button>();
        // Buscar el gestor en la escena
        gestor = FindFirstObjectByType<EscuchoYEncuentroManager>();

        // Agregar listener al botón
        boton.onClick.AddListener(OnClickOpcion);
    }

    void OnClickOpcion()
    {
        if (esCorrecta)
        {
            // Notificar al gestor que se seleccionó correcta
            gestor.SeleccionarOpcion(true);
        }
        else
        {
            // Bloquear botón porque es incorrecta
            boton.interactable = false;

            // Notificar al gestor que fue incorrecta para posible gestión (opcional)
            gestor.SeleccionarOpcion(false);
        }
    }
}

