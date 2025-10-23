using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class GameManager : MonoBehaviour
{
    // --- REFERENCIA AL FADE ---
    [Header("Referencias de Transición")]
    public FadeScreen fadeScreen; // <<-- ARRASTRA EL FADEPLANE AQUÍ
    // -------------------------
    
    // --- VARIABLES DE TRANSICIÓN Y PROGRESO ---
    [Header("Progreso y Transición")]
    [Tooltip("El ID de este nivel. (Ej: 1 para Parejas)")]
    [SerializeField] private int currentLevelID = 1; 
    [SerializeField] private string mapSceneName = "Escenas/ProgressBar"; // Nombre de la escena del mapa
    [SerializeField] private float endDelaySeconds = 5.0f; // 5 segundos de espera
    private const string PROGRESS_KEY = "HighestUnlockedLevel";
    // ------------------------------------------

    [Header("Spawn inicial del jugador")]
    [SerializeField] private Transform startSpawnPoint;

    [Header("Jugador")]
    [SerializeField] XROrigin playerRig;

    // Singleton
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Juego")]
    [SerializeField] private float tiempo = 120f;
    [SerializeField] private int totalPairs = 6;

    private float tiempoRestante;
    private bool gameOver;
    private bool inputLocked;

    private readonly List<Card> reveladas = new List<Card>(2);
    private int parejasEncontradas;

    public bool InputLocked => inputLocked || gameOver;

    void Awake()
    {
        // Asegurar que solo haya una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tepea el jugador a la posicion inicial donde debe aparecer
        if (playerRig != null && startSpawnPoint != null)
        {
            playerRig.MoveCameraToWorldLocation(startSpawnPoint.position);
            playerRig.MatchOriginUpCameraForward(startSpawnPoint.up, startSpawnPoint.forward); // opcional, para rotación
        }
    }

    void Start()
    {
        tiempoRestante = tiempo;
        StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (!gameOver)
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0f)
            {
                tiempoRestante = 0f;
                EndGame(false);
                yield break;
            }
            UpdateTimerLabel();
            yield return null;
        }
    }

    private void UpdateTimerLabel()
    {
        if (timerText)
        {
            int t = Mathf.CeilToInt(tiempoRestante);
            int m = t / 60;
            int s = t % 60;
            timerText.SetText($"{m:0}:{s:00}");
        }
    }

    public void NotifyReveal(Card card)
    {
        if (InputLocked || gameOver) return;
        if (reveladas.Contains(card)) return;

        reveladas.Add(card);
        card.Flip(true, snapHome: false); // Asumimos que la carta se voltea aquí

        if (reveladas.Count == 2)
            StartCoroutine(ResolvePair());
    }

    private IEnumerator ResolvePair()
    {
        inputLocked = true;
        yield return new WaitForSeconds(0.15f);

        var a = reveladas[0];
        var b = reveladas[1];

        // Se asume que la lógica de volteo está en Card.OnGrab

        if (a.PairId == b.PairId && a != b)
        {
            a.SetMatched(true);
            b.SetMatched(true);
            parejasEncontradas++;

            if (statusText)
                statusText.SetText($"¡Pareja encontrada! ({parejasEncontradas}/{totalPairs})");

            if (parejasEncontradas >= totalPairs)
            {
                EndGame(true); // Gana
                yield break;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.7f);
            a.Flip(false, snapHome: true);
            b.Flip(false, snapHome: true);
            if (statusText)
                statusText.SetText("No coinciden, intentalo de nuevo");
        }

        reveladas.Clear();
        inputLocked = false;
    }

    // FUNCIÓN MODIFICADA: Ahora inicia la transición
    private void EndGame(bool win)
    {
        if (gameOver) return;

        gameOver = true;
        inputLocked = true;

        if (statusText)
            statusText.SetText(win ? "¡Ganaste!" : "¡Tiempo agotado!");

        // Guardar el progreso.
        
        SaveLevelProgress();
        

        // Inicia la cuenta regresiva para la transición (Gane o Pierda)
        StartCoroutine(TransitionToProgressSceneRoutine());
    }

    // FUNCIÓN: Guarda el avance
    private void SaveLevelProgress()
    {
        // El siguiente nivel a desbloquear
        int nextLevelToUnlock = currentLevelID + 1;

        // Obtener el progreso guardado (si no existe, se inicializa en 1)
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);

        // Actualizar el progreso solo si este nivel es el más avanzado
        if (nextLevelToUnlock > highestUnlocked)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, nextLevelToUnlock);
            PlayerPrefs.Save();
            Debug.Log($"[PROGRESS] Guardando Nivel: {nextLevelToUnlock}");
        }
    }

    // FUNCIÓN MODIFICADA: Pausa, Fade Out y transición
    private IEnumerator TransitionToProgressSceneRoutine()
    {
        Debug.Log($"Transicionando al mapa en {endDelaySeconds} segundos...");

        // 1. Pausa de 5 segundos
        yield return new WaitForSeconds(endDelaySeconds);

        // 2. Ejecutar Fade Out (oscurecer la pantalla)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();

            // CRUCIAL: Esperar la duración del Fade Out antes de cargar
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // 3. Cambiar a la escena del mapa
        UnityEngine.SceneManagement.SceneManager.LoadScene(mapSceneName);
    }

    public bool CanInteract(Card card)
    {
        return !(gameOver || inputLocked) && !card.Matched;
    }
}