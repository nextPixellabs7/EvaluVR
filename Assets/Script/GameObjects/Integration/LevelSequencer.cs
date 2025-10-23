using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Usaremos la definición de TMP_Text

public class LevelSequencer : MonoBehaviour
{
    private const string PROGRESS_KEY = "HighestUnlockedLevel";

    [Header("Referencias")]
    public FadeScreen fadeScreen; 
    public ProgressBarUpdater progressBarUpdater; 
    public TMP_Text countdownText; // Referencia al texto para la cuenta regresiva

    [Header("Tiempos y Escenas")]
    [Tooltip("Tiempo que la barra se muestra antes de pasar a la cuenta regresiva.")]
    public float progressDisplayDuration = 3.0f;

    [Tooltip("Duración de la cuenta regresiva en segundos.")]
    public int countdownSeconds = 3;

    [Tooltip("Lista ORDENADA de las escenas de nivel. (Elemento 0 = Nivel 1, Elemento 1 = Nivel 2, etc.)")]
    public string[] LevelSceneNames = new string[]
    {
        "Escenas/Actividades/Parejas", 
        "Escenas/Actividades/EscucharYOrdenar", 
        "Escenas/Actividades/SonidosYCartas", 
        "Escenas/Actividades/Pompones", 
        "Escenas/Actividades/Ordenar", 
    };

    void Start()
    {
        // --- Lógica de Reinicio del Editor (Mantenida por seguridad durante simulación) ---
        #if UNITY_EDITOR
        if (PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            // Nota: Esta línea se suele mover al botón de INICIO, no a la barra de progreso.
            // La mantengo aquí por el flujo de depuración.
            // PlayerPrefs.DeleteKey(PROGRESS_KEY);
        }
        #endif
        // ----------------------------------------------------------------------------------

        // Ocultar texto de cuenta regresiva al inicio.
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        // El Fade In se maneja con fadeOnStart = true en el FadeScreen, o lo forzamos aquí.
        if (fadeScreen != null && !fadeScreen.fadeOnStart)
        {
            fadeScreen.FadeIn();
        }

        // Asegurarse de que la barra de progreso se actualice al cargar
        if (progressBarUpdater != null)
        {
            progressBarUpdater.UpdateGlobalProgress();
        }

        // Iniciar la secuencia de espera y carga del siguiente nivel
        StartCoroutine(LoadNextLevelRoutine());
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        // 1. Esperar el tiempo de visualización de la barra de progreso.
        yield return new WaitForSeconds(progressDisplayDuration);

        // 2. Determinar el nivel a cargar basado en el progreso guardado
        int nextLevelID = PlayerPrefs.GetInt(PROGRESS_KEY, 1); 
        int sceneArrayIndex = nextLevelID - 1; 

        // --- VERIFICACIÓN DE LÍMITE DE NIVELES ---
        if (sceneArrayIndex >= 0 && sceneArrayIndex < LevelSceneNames.Length)
        {
            string sceneToLoad = LevelSceneNames[sceneArrayIndex];

            // 3. Fade OUT (Oscurecer pantalla)
            if (fadeScreen != null)
            {
                fadeScreen.FadeOut();
                // Esperar a que la transición termine y la pantalla esté completamente negra.
                yield return new WaitForSeconds(fadeScreen.fadeDuration);
            }
            
            // 4. Cuenta Regresiva (Se ejecuta mientras la pantalla está en negro)
            if (countdownText != null)
            {
                // Opcional: Ocultar la barra para mayor limpieza, aunque no se vea.
                if (progressBarUpdater != null) progressBarUpdater.gameObject.SetActive(false);
                
                yield return StartCoroutine(CountdownRoutine());
            }

            // 5. Cargar la escena. La nueva escena se encargará del Fade In.
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            // 6. Todos los niveles completados (ir al menú o escena de final de juego)
            Debug.Log("¡Todos los niveles completados! Volviendo al Menú.");
            if (fadeScreen != null)
            {
                fadeScreen.FadeOut();
                yield return new WaitForSeconds(fadeScreen.fadeDuration);
            }
            SceneManager.LoadScene("Escenas/Menu"); 
        }
    }

    private IEnumerator CountdownRoutine()
    {
        // Activa el objeto de texto para que sea visible sobre la pantalla negra.
        countdownText.gameObject.SetActive(true);

        for (int i = countdownSeconds; i > 0; i--)
        {
            countdownText.text = i.ToString();
            // Opcional: Pausa de 1 segundo
            yield return new WaitForSeconds(1f);
        }

        // Mensaje final breve antes de cargar
        countdownText.text = "¡Vamos!";
        yield return new WaitForSeconds(0.5f); // Pausa más corta
        
        countdownText.gameObject.SetActive(false);
    }
}