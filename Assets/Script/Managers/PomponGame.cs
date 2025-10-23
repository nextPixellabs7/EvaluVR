using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para la carga de escena


public class PomponGame : MonoBehaviour
{
    // --- VARIABLES DE TRANSICIÓN Y PROGRESO ---
    [Header("Transición y Progreso")]
    [Tooltip("El ID de este nivel. (Debe ser 4)")]
    [SerializeField] private int currentLevelID = 4; // ID del Nivel Pompones
    [SerializeField] private string mapSceneName = "Escenas/ProgressBar"; 
    [SerializeField] private float endDelaySeconds = 3.0f; // Pausa después de terminar el nivel
    public FadeScreen fadeScreen; // ARRASTRA EL FADEPLANE AQUÍ
    private const string PROGRESS_KEY = "HighestUnlockedLevel";
    // ------------------------------------------

    private List<Pompon> Totales;
    private List<Pompon> pompones;
    
    [Header("Textos del juego")]
    public TextMeshProUGUI TextoBig;
    public TextMeshProUGUI TextoSmall;
    public TextMeshProUGUI TextoTitulo;

    [Header("Spawn inicial del jugador")]
    [SerializeField] private Transform startSpawnPoint;

    [Header("Jugador")]
    [SerializeField] XROrigin playerRig;

    private void Start()
    {
        pompones = new List<Pompon>();
        Totales = new List<Pompon>();

        // CORRECCIÓN DE SINTAXIS: FindGameObjectsWithTag (sin 's' al final de Tag)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("SmallPompon"))
            Totales.Add(go.GetComponent<Pompon>());

        // CORRECCIÓN DE SINTAXIS: FindGameObjectsWithTag (sin 's' al final de Tag)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("BigPompon"))
            Totales.Add(go.GetComponent<Pompon>());
        
        // Tepea el jugador a la posicion inicial donde debe aparecer
        if (playerRig != null && startSpawnPoint != null)
        {
            playerRig.MoveCameraToWorldLocation(startSpawnPoint.position);
            playerRig.MatchOriginUpCameraForward(startSpawnPoint.up, startSpawnPoint.forward);
        }
    }

    public void RegistrarPompon(Pompon pompon, string tipoCanasta)
    {
        if (pompones.Contains(pompon)) return; // Evitar doble registro

        if (pompon.GetSize() == tipoCanasta)
        {
            pompon.setCorrecta(true);
        }
        else
        {
            pompon.setCorrecta(false);
        }
        
        // Desactivamos la interacción para la carta colocada
        pompon.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = false;
        
        pompones.Add(pompon);

        JuegoTerminada();
    }

    private void JuegoTerminada()
    {
        if(pompones.Count == Totales.Count)
        {
            TextoTitulo.text = "¡Felicidades, lo has completado!";
            
            // --- INICIA LÓGICA DE TRANSICIÓN AL NIVEL 5 ---
            SaveLevelProgress();
            StartCoroutine(TransitionToMapRoutine());
            // --- FIN LÓGICA DE TRANSICIÓN ---
        }
    }
    
    // --- FUNCIONES DE PROGRESO Y TRANSICIÓN ---
    
    private void SaveLevelProgress()
    {
        // Guardamos el Nivel 5 (4 + 1)
        int nextLevelToUnlock = currentLevelID + 1; 
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);

        if (nextLevelToUnlock > highestUnlocked)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, nextLevelToUnlock);
            PlayerPrefs.Save();
            Debug.Log($"[PROGRESS] Nivel {currentLevelID} completado. Guardando para Nivel {nextLevelToUnlock}");
        }
    }

    private IEnumerator TransitionToMapRoutine()
    {
        // 1. Pausa de 3s
        yield return new WaitForSeconds(endDelaySeconds); 

        // 2. Fade OUT (oscurecer la pantalla)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // 3. Cargar la escena de la barra de progreso
        SceneManager.LoadScene(mapSceneName);
    }
}