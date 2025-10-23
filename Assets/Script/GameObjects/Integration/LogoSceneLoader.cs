using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoSceneLoader : MonoBehaviour
{
    [Header("Configuración de Carga")]
    [Tooltip("Tiempo de espera en la escena del logo antes de la transición.")]
    public float displayDuration = 3.0f; 

    [Tooltip("Nombre EXACTO de la siguiente escena.")]
    public string nextSceneName = "Menu"; // Nombre de tu escena de menú

    [Header("Referencias (Opcional)")]
    public SceneTransitionManager transitionManager; 

    void Start()
    {
        // El Fade In inicial lo maneja el FadeScreen de esta escena.
        
        // Inicia la cuenta regresiva y la transición.
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        // 1. Esperar la duración de la pantalla del logo.
        yield return new WaitForSeconds(displayDuration);

        // 2. Ejecutar la transición.
        if (transitionManager != null)
        {
            // Usar el gestor para aplicar el Fade Out y cargar la escena.
            // Nota: Usamos el nombre exacto de la escena (Menu).
            transitionManager.GoToScene(nextSceneName);
        }
        else
        {
            // Si el TransitionManager no está asignado (fallback sin fade).
            SceneManager.LoadScene(nextSceneName);
        }
    }
}