using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    // --- REFERENCIA AL FADE ---
    [Header("Referencias de Transición")]
    public FadeScreen fadeScreen; // <<-- ARRASTRA EL FADEPLANE AQUÍ
    // -------------------------
    
    // --- VARIABLES DE TRANSICIÓN Y PROGRESO ---
    [Header("Finalización y Transición")]
    [SerializeField] private string finalSceneName = "Escenas/Final"; // Nombre de la escena del final
    [SerializeField] private float endDelaySeconds = 5.0f;
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
            playerRig.MatchOriginUpCameraForward(startSpawnPoint.up, startSpawnPoint.forward); 
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
        card.Flip(true, snapHome: false);

        if (reveladas.Count == 2)
            StartCoroutine(ResolvePair());
    }

    private IEnumerator ResolvePair()
    {
        inputLocked = true;
        yield return new WaitForSeconds(0.15f);

        var a = reveladas[0];
        var b = reveladas[1];

        if (a.PairId == b.PairId && a != b)
        {
            a.SetMatched(true);
            b.SetMatched(true);
            parejasEncontradas++;
            
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
        }

        reveladas.Clear();
        inputLocked = false;
    }

    // FUNCIÓN MODIFICADA: Ahora inicia la transición al final
    private void EndGame(bool win)
    {
        if (gameOver) return;

        gameOver = true;
        inputLocked = true;
        
        GoToFinalSceneAndReset(); 
    }

    // FUNCIÓN CLAVE: REINICIA EL PROGRESO Y VA A LA ESCENA FINAL
    public void GoToFinalSceneAndReset()
    {
        // 1. Reiniciar el progreso guardado
        PlayerPrefs.DeleteKey(PROGRESS_KEY);
        PlayerPrefs.Save();
        Debug.Log("Juego de Parejas terminado. Progreso reseteado a Nivel 1.");
        
        // 2. Iniciar la transición al menú
        StartCoroutine(TransitionToFinalSceneRoutine());
    }

    // FUNCIÓN MODIFICADA: Pausa, Fade Out y transición a la escena final
    private IEnumerator TransitionToFinalSceneRoutine()
    {
        Debug.Log($"Transicionando a la escena final en {endDelaySeconds} segundos...");

        // Pausa de 5 segundos
        yield return new WaitForSeconds(endDelaySeconds);

        // Ejecutar Fade Out (oscurecer la pantalla)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();

            // Esperar la duración del Fade Out antes de cargar
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // Cambiar a la escena final
        UnityEngine.SceneManagement.SceneManager.LoadScene(finalSceneName);
    }

    public bool CanInteract(Card card)
    {
        return !(gameOver || inputLocked) && !card.Matched;
    }
}