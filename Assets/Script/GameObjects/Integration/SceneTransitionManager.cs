// --- MODIFICACIÓN DE SCENETRANSITIONMANAGER.CS ---
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;

    // Cambiado de 'int sceneIndex' a 'string sceneName' para usar los nombres de tu Build Settings
    public void GoToScene(string sceneName)
    {
        StartCoroutine(GoToSceneRoutine(sceneName));
    }

    IEnumerator GoToSceneRoutine(string sceneName)
    {
        // 1. Fade OUT (Oscurecer pantalla)
        fadeScreen.FadeOut();
        // Esperar a que el FadeOut termine.
        yield return new WaitForSeconds(fadeScreen.fadeDuration);

        // 2. Cargar la nueva escena
        SceneManager.LoadScene(sceneName);

        // NOTA: El FadeIn se manejará en el método Start() de la nueva escena,
        // usando el script FadeScreen.
    }
}