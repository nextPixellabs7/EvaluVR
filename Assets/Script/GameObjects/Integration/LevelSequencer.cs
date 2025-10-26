using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSequencer : MonoBehaviour
{
    private const string PROGRESS_KEY = "HighestUnlockedLevel";
    private const string TARGET_INDEX_KEY = "TargetLevelIndex"; // Nueva clave para la intro

    [Header("Referencias")]
    public FadeScreen fadeScreen;
    public TMP_Text countdownText;
    public Animator progressAnimator;

    [Header("Config")]
    public float delayBeforeAnim = 1.0f;
    public float animWaitSeconds = 2.5f;
    public int countdownSeconds = 0;
    public bool forceLevel1IfMissingKey = true;
    
    // El nombre de la escena de introducción (la que se cargará AHORA)
    [Tooltip("El nombre de la escena de introducción a la actividad (startGame)")]
    public string StartGameSceneName = "startGame"; 

    [Tooltip("0=Nivel1, 1=Nivel2, ...")]
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
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (fadeScreen != null && !fadeScreen.fadeOnStart) fadeScreen.FadeIn();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(delayBeforeAnim);

        // 1) Lógica para leer el nivel desbloqueado
        int highestUnlocked = 1;
        if (PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);
        }
        else if (forceLevel1IfMissingKey)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, 1);
        }

        // Clamp y cálculo del índice
        highestUnlocked = Mathf.Clamp(highestUnlocked, 1, LevelSceneNames.Length);
        int sceneIndex = highestUnlocked - 1;
        Debug.Log($"[Seq] HighestUnlockedLevel={highestUnlocked} -> escena destino: '{LevelSceneNames[sceneIndex]}'");

        // 2) Reproducir animación de progreso
        if (progressAnimator != null)
        {
            string anim = GetAnimForReachedLevel(highestUnlocked);
            progressAnimator.Play(anim, 0, 0f);
        }
        if (animWaitSeconds > 0f) yield return new WaitForSeconds(animWaitSeconds);

        // 3) Fade Out
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // 4) Cuenta atrás (opcional)
        if (countdownText != null && countdownSeconds > 0)
        {
            countdownText.gameObject.SetActive(true);
            for (int i = countdownSeconds; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            countdownText.gameObject.SetActive(false);
        }

        // 5) PASO CLAVE: Guardar el índice del nivel real y cargar la escena de INTRODUCCIÓN
        PlayerPrefs.SetInt(TARGET_INDEX_KEY, sceneIndex);
        PlayerPrefs.Save(); 
        
        Debug.Log($"[Seq] Cargando escena de Introduccion: {StartGameSceneName}");
        SceneManager.LoadScene(StartGameSceneName);
    }

    private string GetAnimForReachedLevel(int reachedLevel)
    {
        // Mapea el nivel al nombre de tu clip de animación
        switch (reachedLevel)
        {
            case 1: return "Idle";
            case 2: return "AnimLevel1";
            case 3: return "AnimLevel2";
            case 4: return "AnimLevel3";
            case 5: return "AnimLevel4";
            default: return "Idle";
        }
    }
}