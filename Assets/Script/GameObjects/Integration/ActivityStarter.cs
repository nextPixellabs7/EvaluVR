using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // Necesario para la Corrutina

public class ActivityStarter : MonoBehaviour
{
    private const string TARGET_INDEX_KEY = "TargetLevelIndex";
    
    [Header("Referencias UI")]
    public TMP_Text levelTitleText;
    public GameObject instructionGifContainer; // Objeto que contiene el GIF/Instrucciones
    public FadeScreen fadeScreen; // Referencia a FadeScreen en esta escena (si aplica)
    
    [Header("Config. de Escenas")]
    [Tooltip("El mismo listado de nombres de escenas que tienes en LevelSequencer")]
    public string[] LevelSceneNames; 
    
    [Tooltip("Nombre de la escena de menú, para regresar si hay un error")]
    public string MenuSceneName = "Menu (Inicio)"; 

    private int targetLevelIndex = -1;

    void Start()
    {
        LoadTargetLevelData();
    }
    
    private void LoadTargetLevelData()
    {
        if (PlayerPrefs.HasKey(TARGET_INDEX_KEY))
        {
            targetLevelIndex = PlayerPrefs.GetInt(TARGET_INDEX_KEY);
            
            if (targetLevelIndex >= 0 && targetLevelIndex < LevelSceneNames.Length)
            {
                // Muestra la información del nivel
                string sceneName = LevelSceneNames[targetLevelIndex];
                
                // Extrae el nombre corto (opcional, para UI)
                string shortName = sceneName.Substring(sceneName.LastIndexOf('/') + 1);
                
                if (levelTitleText != null)
                {
                    levelTitleText.text = $"NIVEL {targetLevelIndex + 1}: {shortName.ToUpper()}";
                }
                
                // Aquí podrías tener lógica para activar/cambiar el GIF/instrucciones
                // basándose en 'targetLevelIndex' o 'shortName'.
                // Ejemplo: instructionGifContainer.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Gifs/{shortName}");
            }
            else
            {
                Debug.LogError($"[ActivityStarter] Índice de nivel inválido: {targetLevelIndex}. Regresando al menú.");
                SceneManager.LoadScene(MenuSceneName);
            }
        }
        else
        {
            Debug.LogError("[ActivityStarter] Clave 'TargetLevelIndex' faltante. Regresando al menú.");
            SceneManager.LoadScene(MenuSceneName);
        }
    }

    // MÉTODO ASOCIADO AL BOTÓN "COMENZAR"
    public void StartActivityButton()
    {
        if (targetLevelIndex >= 0)
        {
            StartCoroutine(LoadLevelRoutine());
        }
    }

    private IEnumerator LoadLevelRoutine()
    {
        // 1. Fade out (si tienes un FadeScreen en esta escena)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }
        
        // 2. Cargar el nivel real
        Debug.Log($"[ActivityStarter] Iniciando nivel: {LevelSceneNames[targetLevelIndex]}");
        SceneManager.LoadScene(LevelSceneNames[targetLevelIndex]);
    }
}