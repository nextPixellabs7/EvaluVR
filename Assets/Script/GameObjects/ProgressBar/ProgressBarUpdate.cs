using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUpdater : MonoBehaviour
{
    // Clave de guardado que usamos en el GameEnder para saber qu� nivel desbloqueado
    private const string PROGRESS_KEY = "HighestUnlockedLevel";

    // Arrastra el objeto ProgressBar_Fill (con el componente Image) aqu�.
    // Si adjuntas este script directamente a ProgressBar_Fill, puedes usar GetComponent<Image>().
    public Image fillImage;

    [Header("Configuraci�n del Avance")]
    // Total de niveles en tu juego (incluyendo el nivel de inicio/tutorial, si aplica)
    public int totalGameLevels = 5;

    void Start()
    {
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
        }

        UpdateGlobalProgress();
    }

    // Se llama al cargar el mapa para mostrar el avance global
    public void UpdateGlobalProgress()
    {
        // 1. Obtener el nivel m�s alto que el jugador ha desbloqueado.
        // Se inicializa en 1 si es la primera vez que se juega.
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);

        // 2. Calcular cu�ntos niveles ha completado.
        // Si highestUnlocked es 3, el jugador ha completado 2 niveles (Nivel 1 y Nivel 2).
        int levelsCompleted = Mathf.Max(0, highestUnlocked);

        // 3. Calcular el valor de progreso (0.0 a 1.0)
        // Ejemplo: 2 niveles completados de 10 totales = 0.2
        float progressValue = (float)levelsCompleted / totalGameLevels;

        // 4. Aplicar el progreso a la barra de UI
        fillImage.fillAmount = progressValue;

        Debug.Log($"Niveles Completados: {levelsCompleted}. Progreso: {progressValue:P0}");
    }
}
