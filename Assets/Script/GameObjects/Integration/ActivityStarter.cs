using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using GifImporter; // NECESARIO para usar las clases Gif y GifPlayer

public class ActivityStarter : MonoBehaviour
{
    private const string TARGET_INDEX_KEY = "TargetLevelIndex";
    private const string GIF_RESOURCE_PATH = "Gifs/"; 

    [Header("Referencias UI")]
    public TMP_Text levelTitleText;

    [Tooltip("El GameObject que contiene el Sprite Renderer y el script GifPlayer.")]
    public GameObject instructionGifContainer; 

    public FadeScreen fadeScreen; 

    [Header("Config. de Escenas")]
    // ⚠️ ¡ARRAY SINCRONIZADO CON LEVELSEQUENCER!
    public string[] LevelSceneNames = new string[]
    {
        "Escenas/Actividades/EscucharYOrdenar", // Nivel 1 (Índice 0)
        "Escenas/Actividades/SonidosYCartas",
        "Escenas/Actividades/Pompones",
        "Escenas/Actividades/Ordenar",
        "Escenas/Actividades/Parejas", // Nivel 5 (Índice 4)
    };

    public string MenuSceneName = "Menu (Inicio)"; 

    private int targetLevelIndex = -1;

    void Start()
    {
        if (instructionGifContainer != null)
        {
            instructionGifContainer.SetActive(false);
        }
        // Fade in al cargar la escena
        if (fadeScreen != null && !fadeScreen.fadeOnStart) fadeScreen.FadeIn();
        
        LoadTargetLevelData();
    }

    private void LoadTargetLevelData()
    {
        if (!PlayerPrefs.HasKey(TARGET_INDEX_KEY))
        {
            Debug.LogError($"[ActivityStarter] Clave '{TARGET_INDEX_KEY}' faltante. Regresando a {MenuSceneName}.");
            SceneManager.LoadScene(MenuSceneName);
            return;
        }

        targetLevelIndex = PlayerPrefs.GetInt(TARGET_INDEX_KEY);

        if (targetLevelIndex >= 0 && targetLevelIndex < LevelSceneNames.Length)
        {
            string sceneName = LevelSceneNames[targetLevelIndex];
            string shortName = sceneName.Substring(sceneName.LastIndexOf('/') + 1);

            if (levelTitleText != null)
            {
                levelTitleText.text = $"{shortName.ToUpper()}";
            }

            // === LÓGICA CLAVE: CARGAR GIF y ASIGNARLO al GifPlayer ===
            if (instructionGifContainer != null)
            {
                GifPlayer gifPlayer = instructionGifContainer.GetComponent<GifPlayer>(); 

                if (gifPlayer != null)
                {
                    string fullPath = GIF_RESOURCE_PATH + shortName;
                    Gif loadedGif = Resources.Load<Gif>(fullPath);

                    if (loadedGif != null)
                    {
                        gifPlayer.Gif = loadedGif;
                        instructionGifContainer.SetActive(true); 
                        Debug.Log($"[ActivityStarter] GIF cargado y activado: {fullPath}");
                    }
                    else
                    {
                        Debug.LogError($"[ActivityStarter] No se encontró el recurso GIF en: Resources/{fullPath}. ¿El nombre del archivo coincide exactamente con '{shortName}'?");
                    }
                }
                else
                {
                    Debug.LogError("[ActivityStarter] instructionGifContainer NO tiene el script 'GifPlayer'. Asegúrate de que está adjunto.");
                }
            }
        }
        else
        {
            Debug.LogError($"[ActivityStarter] Índice de nivel inválido: {targetLevelIndex}. Regresando a {MenuSceneName}.");
            SceneManager.LoadScene(MenuSceneName);
        }
    }

    public void StartActivityButton()
    {
        if (targetLevelIndex >= 0)
        {
            StartCoroutine(LoadLevelRoutine());
        }
    }

    private IEnumerator LoadLevelRoutine()
    {
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            if (instructionGifContainer != null) instructionGifContainer.SetActive(false); 

            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        Debug.Log($"[ActivityStarter] Iniciando nivel: {LevelSceneNames[targetLevelIndex]}");
        SceneManager.LoadScene(LevelSceneNames[targetLevelIndex]);
    }
}