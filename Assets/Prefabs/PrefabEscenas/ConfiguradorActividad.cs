using UnityEngine;
using TMPro; // Aseg�rate de tener esto si usas TextMeshPro para tu texto

public class ControladorInicioActividad : MonoBehaviour
{
    [Header("1. CONTENIDO (Se cambia en cada Prefab)")]
    [Tooltip("Escribe aqu� el nombre de la actividad para el t�tulo.")]
    public string nombreDeLaActividad;

    [Header("2. REFERENCIAS (Se asignan una sola vez en el prefab base)")]
    [Tooltip("Arrastra aqu� el objeto de Texto de la UI.")]
    public TextMeshProUGUI textoDelTitulo;

    void Start()
    {
        // Esta �nica l�nea se asegura de que el t�tulo sea el correcto.
        // El GIF se reproducir� autom�ticamente porque lo asignaremos en el Inspector.
        if (textoDelTitulo != null)
        {
            textoDelTitulo.text = nombreDeLaActividad;
        }
        else
        {
            Debug.LogError("�ERROR DE REFERENCIA! El campo 'Texto Del Titulo' no est� asignado en el Inspector de este prefab.");
        }
    }
}


