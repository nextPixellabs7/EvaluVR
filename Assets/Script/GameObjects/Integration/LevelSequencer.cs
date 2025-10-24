using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSequencer : MonoBehaviour
{
    private const string PROGRESS_KEY = "HighestUnlockedLevel"; // guardado externo (1..N)

    [Header("Referencias")]
    public FadeScreen fadeScreen;
    public TMP_Text countdownText;
    public Animator progressAnimator;

    [Header("Config")]
    public float delayBeforeAnim = 1.0f;     // pequeña pausa al entrar
    public float animWaitSeconds = 2.5f;     // lo que dura tu clip (ajústalo)
    public int countdownSeconds = 0;         // 0 = sin cuenta regresiva
    public bool forceLevel1IfMissingKey = true; // si no existe la clave, arranca en 1

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

        // 1) Leer progreso EXTERNO (NO incrementamos aquí)
        int highestUnlocked;
        if (!PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            highestUnlocked = forceLevel1IfMissingKey ? 1 : 1;
            PlayerPrefs.SetInt(PROGRESS_KEY, highestUnlocked);
            PlayerPrefs.Save();
            Debug.Log($"[Seq] No había clave. Set -> {highestUnlocked}");
        }
        else
        {
            highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);
        }

        // Clamp duro
        highestUnlocked = Mathf.Clamp(highestUnlocked, 1, LevelSceneNames.Length);
        int sceneIndex = highestUnlocked - 1;
        Debug.Log($"[Seq] HighestUnlockedLevel={highestUnlocked} -> escena '{LevelSceneNames[sceneIndex]}' (idx {sceneIndex})");

        // 2) Reproducir la animación que corresponde a HABER LLEGADO a ese nivel
        //    1 -> Idle (posición en el nodo 1)
        //    2 -> AnimLevel1 (salto 1->2)
        //    3 -> AnimLevel2 (salto 2->3) ... etc.
        if (progressAnimator != null)
        {
            string anim = GetAnimForReachedLevel(highestUnlocked);
            Debug.Log($"[Seq] Reproduciendo anim '{anim}'");
            progressAnimator.Play(anim, 0, 0f); // reproducir desde el inicio
        }

        // 3) Espera a que termine tu clip (ajusta animWaitSeconds)
        if (animWaitSeconds > 0f) yield return new WaitForSeconds(animWaitSeconds);

        // 4) Fade + cuenta atrás (opcional) y CARGA la escena correspondiente
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

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

        Debug.Log($"[Seq] Cargando escena '{LevelSceneNames[sceneIndex]}'");
        SceneManager.LoadScene(LevelSceneNames[sceneIndex]);
    }

    // Mapea "nivel alcanzado" -> nombre del estado/clip en tu Animator
    private string GetAnimForReachedLevel(int reachedLevel)
    {
        switch (reachedLevel)
        {
            case 1: return "Idle";        // ya estás en el nodo 1
            case 2: return "AnimLevel1";  // 1 -> 2
            case 3: return "AnimLevel2";  // 2 -> 3
            case 4: return "AnimLevel3";  // 3 -> 4
            case 5: return "AnimLevel4";  // 4 -> 5
            default: return "Idle";
        }
    }
}
