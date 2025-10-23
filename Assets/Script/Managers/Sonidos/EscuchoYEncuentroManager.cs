using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para la carga de escena

public class EscuchoYEncuentroManager : MonoBehaviour
{
    // --- VARIABLES AÑADIDAS PARA PROGRESO Y TRANSICIÓN ---
    [Header("Transición y Progreso")]
    [Tooltip("El ID de este nivel. (Debe ser 2)")]
    [SerializeField] private int currentLevelID = 2; 
    [SerializeField] private string mapSceneName = "Escenas/ProgressBar"; 
    [SerializeField] private float endDelaySeconds = 3.0f; // Pausa después de terminar el nivel
    public FadeScreen fadeScreen; // Arrastra el FadePlane aquí
    private const string PROGRESS_KEY = "HighestUnlockedLevel";
    // --------------------------------------------------
    
    [Header("Spawn inicial del jugador")]
    [SerializeField] private Transform startSpawnPoint;

    [Header("Jugador")]
    [SerializeField] XROrigin playerRig;

    public GameObject[] panelesPalabras; // Array con los paneles de cada palabra
    public AudioSource audioSource; 
    public AudioClip[] clipsAudio; 

    public int indiceActual = 0;

    private Coroutine reproducirRutina;


    private void Awake()
    {
        // Tepea el jugador a la posicion inicial
        if (playerRig != null && startSpawnPoint != null)
        {
            playerRig.MoveCameraToWorldLocation(startSpawnPoint.position);
            playerRig.MatchOriginUpCameraForward(startSpawnPoint.up, startSpawnPoint.forward); 
        }
    }

    void Start()
    {
        // ... (Tu lógica de inicio)
        for (int i = 0; i < panelesPalabras.Length; i++)
        {
            panelesPalabras[i].SetActive(i == 0);
        }
        StartCoroutine(ReproducirAudioActual(5));
    }

    // Método que se llama desde los botones de opciones
    public void SeleccionarOpcion(bool esCorrecta)
    {
        if (esCorrecta)
        {
            // Opción correcta: desactivar panel actual y avanzar al siguiente
            panelesPalabras[indiceActual].SetActive(false);
            indiceActual++;

            if (indiceActual < panelesPalabras.Length)
            {
                panelesPalabras[indiceActual].SetActive(true);
                StartCoroutine(ReproducirAudioActual(5));
            }
            else
            {
                // --- LÓGICA DE FIN DE NIVEL INVOCADA AQUÍ ---
                Debug.Log("Actividad finalizada. Transicionando...");
                EndLevelSequence();
                // -------------------------------------------
            }
        }
        else
        {
            // Opción incorrecta: Bloqueo/Lógica de intento fallido
            Debug.Log("Opción incorrecta.");
        }
    }

    // --- NUEVA LÓGICA DE FIN DE NIVEL Y TRANSICIÓN ---

    private void EndLevelSequence()
    {
        SaveLevelProgress(); 
        StartCoroutine(TransitionToMapRoutine());
    }

    private void SaveLevelProgress()
    {
        // Guardamos el siguiente nivel a desbloquear (CurrentLevelID + 1)
        int nextLevelToUnlock = currentLevelID + 1;
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);

        if (nextLevelToUnlock > highestUnlocked)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, nextLevelToUnlock);
            PlayerPrefs.Save();
            Debug.Log($"[PROGRESS] Nivel {currentLevelID} completado. Siguiente desbloqueado: {nextLevelToUnlock}");
        }
    }

    private IEnumerator TransitionToMapRoutine()
    {
        // 1. Pausa antes del Fade Out para que el jugador vea el panel final
        yield return new WaitForSeconds(endDelaySeconds); 

        // 2. Fade OUT (oscurecer la pantalla)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            // Esperar la duración del Fade Out antes de cargar la escena
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // 3. Cambiar a la escena de la barra de progreso
        SceneManager.LoadScene(mapSceneName);
    }

    // --- FIN NUEVA LÓGICA ---


    // Reproducir el audio de la palabra actual
    public IEnumerator ReproducirAudioActual(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        if (indiceActual < clipsAudio.Length && clipsAudio[indiceActual] != null)
        {
            audioSource.clip = clipsAudio[indiceActual];
            audioSource.Play();
        }
    }
}