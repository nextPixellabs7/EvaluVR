using UnityEngine;
using TMPro; // Asegúrate de tener esto si usas TextMeshPro para tu texto

public class ControladorInicioActividad : MonoBehaviour
{
    [Header("1. CONTENIDO (Se cambia en cada Prefab)")]
    [Tooltip("Escribe aquí el nombre de la actividad para el título.")]
    public string nombreDeLaActividad;

    [Header("2. REFERENCIAS (Se asignan una sola vez en el prefab base)")]
    [Tooltip("Arrastra aquí el objeto de Texto de la UI.")]
    public TextMeshProUGUI textoDelTitulo;

    void Start()
    {
        // Esta única línea se asegura de que el título sea el correcto.
        // El GIF se reproducirá automáticamente porque lo asignaremos en el Inspector.
        if (textoDelTitulo != null)
        {
            textoDelTitulo.text = nombreDeLaActividad;
        }
        else
        {
            Debug.LogError("¡ERROR DE REFERENCIA! El campo 'Texto Del Titulo' no está asignado en el Inspector de este prefab.");
        }
    }
}


