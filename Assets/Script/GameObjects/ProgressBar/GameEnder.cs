using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnder : MonoBehaviour
{
    // --- Configuración Requerida en el Inspector ---

    // Asigna el ID numérico de la escena de este nivel (ej: 1 para el Nivel 1).
    public int currentLevelID;

    // El nombre de la escena de tu mapa de progresión.
    public string mapSceneName = "MapScene";

    // Clave de guardado (debe coincidir con la usada en LevelMapManager)
    private const string PROGRESS_KEY = "HighestUnlockedLevel";

    // --- Función Principal ---

    /// <summary>
    /// Se llama cuando el jugador completa exitosamente este nivel.
    /// Registra el progreso y regresa al mapa.
    /// </summary>
    public void LevelCompleted()
    {
        // 1. Determinar el siguiente nivel a desbloquear
        int nextLevelToUnlock = currentLevelID + 1;

        // 2. Obtener el progreso guardado
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);

        // 3. Actualizar el progreso solo si este nivel ha avanzado más que antes
        if (nextLevelToUnlock > highestUnlocked)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, nextLevelToUnlock);
            PlayerPrefs.Save(); // ¡Importante! Guardar los cambios inmediatamente

            Debug.Log($"¡Progreso guardado! Nivel {nextLevelToUnlock} desbloqueado.");
        }

        // 4. Volver a la escena del mapa
        SceneManager.LoadScene(mapSceneName);
    }
}