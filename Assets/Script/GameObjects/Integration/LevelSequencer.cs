using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Usaremos la definición de TMP_Text

public class LevelSequencer : MonoBehaviour
{
    private const string PROGRESS_KEY = "HighestUnlockedLevel";
    private const string FIRST_RUN_KEY = "ProgressBar_FirstRun";

    [Header("Referencias")]
    public FadeScreen fadeScreen;
    public TMP_Text countdownText;
    public Animator progressAnimator;

    [Header("Configuración")]
    public float progressDisplayDuration = 1.5f;  // espera antes de animar/cargar
    public float animPlayWaitSeconds = 5.5f;  // cuanto dura tu animación de salto
    public int countdownSeconds = 3;     // cuenta regresiva


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

        // Iniciar la secuencia de espera y carga del siguiente nivel
        StartCoroutine(LoadNextLevelRoutine());
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        yield return new WaitForSeconds(progressDisplayDuration);

        // --- CAMBIO: leer SIN incrementar. highestUnlocked es 1..N ---
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);
        highestUnlocked = Mathf.Clamp(highestUnlocked, 1, LevelSceneNames.Length);

        // Índice de escena a cargar (0-based)
        int sceneArrayIndex = highestUnlocked - 1;
        Debug.Log($"[LevelSequencer] HighestUnlockedLevel={highestUnlocked} -> {LevelSceneNames[sceneArrayIndex]}");

        // --- CAMBIO: reproducir la animación que corresponde a haber llegado a este nivel ---
        // Caso especial: si highestUnlocked == 1 (recién empezando) muestra Idle.
        if (progressAnimator != null)
        {
            string animName = GetAnimationNameForProgress(highestUnlocked);
            if (!string.IsNullOrEmpty(animName))
                progressAnimator.Play(animName, 0, 0f);
        }
        // ------------------------------------------------------------------------------------

        yield return new WaitForSeconds(animPlayWaitSeconds);

        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        if (countdownText != null) yield return StartCoroutine(CountdownRoutine());

        SceneManager.LoadScene(LevelSceneNames[sceneArrayIndex]);
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

    private string GetAnimationNameForProgress(int index)
    {
        switch (index)
        {
            case 0: return "Idle";        // posición inicial
            case 1: return "AnimLevel1";  // 1 -> 2
            case 2: return "AnimLevel2";  // 2 -> 3
            case 3: return "AnimLevel3";  // 3 -> 4
            case 4: return "AnimLevel4";  // 4 -> 5
            default: return "Idle";
        }
    }
}