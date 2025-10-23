using NUnit.Framework;
using System;
using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // Necesario para la carga de escena
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SoundsGameManager : MonoBehaviour
{
    // --- VARIABLES AÑADIDAS PARA PROGRESO Y TRANSICIÓN ---
    [Header("Transición y Progreso")]
    [Tooltip("ID del nivel que esta escena representa (ej: 3 para SonidosYCartas)")]
    [SerializeField] private int currentLevelID = 3; 
    [SerializeField] private string mapSceneName = "Escenas/ProgressBar"; 
    [SerializeField] private float endDelaySeconds = 3.0f; // Pausa después de terminar el nivel
    public FadeScreen fadeScreen; // ARRASTRA EL FADEPLANE AQUÍ
    private const string PROGRESS_KEY = "HighestUnlockedLevel";
    // --------------------------------------------------

    [Header("Spawn inicial del jugador")]
    [SerializeField] private Transform startSpawnPoint;

    [Header("Jugador")]
    [SerializeField] XROrigin playerRig;

    [Header("Clips de audio")]
    [SerializeField] private AudioClip[] clips;

    [Serializable]
    public class level
    {
        [Header("Sockets del nivel")]
        public XRSocketInteractor[] sockets;

        [Header("Orden esperado")]
        public int[] nivelX;

        [Header("Cartas disponibles de este nivel")]
        public SoundCardFather[] cards;

        [Header("Posicion del siguiente nivel")]
        public Transform spawnPoint;

        [HideInInspector] public int colocadas;

        [Header("Audio del nivel")]
        public AudioSource audioSource;
    }

    [Header("Niveles")]
    [SerializeField] private level[] levels;
    private int nivelActual = 0;


    private void Awake()
    {
        // Tepea el jugador a la posicion inicial donde debe aparecer
        if (playerRig != null && startSpawnPoint != null)
        {
            playerRig.MoveCameraToWorldLocation(startSpawnPoint.position);
            playerRig.MatchOriginUpCameraForward(startSpawnPoint.up, startSpawnPoint.forward); // opcional, para rotación
        }

        StartCoroutine(ReproducirAudioActual(5));

        foreach (level lvl in levels)
        {
            if (lvl.sockets == null || lvl.nivelX == null || lvl.sockets.Length != lvl.nivelX.Length)
            {
                Debug.LogError($"[SoundsGameManager] Level {lvl} mal configurado: sockets y expectedOrder deben tener mismo largo.");
                continue;
            }

            foreach (var s in lvl.sockets)
            {
                if (s == null)
                {
                    continue;
                }

                s.selectEntered.AddListener(OnSocketSelectEntered);
            }
        }

        //socket = GetComponent<XRSocketInteractor>();
    }

    private void OnDestroy()
    {
        // Limpieza
        foreach (var lvl in levels)
        {
            if (lvl?.sockets == null) continue;
            foreach (var s in lvl.sockets)
            {
                if (s == null) continue;
                s.selectEntered.RemoveListener(OnSocketSelectEntered);
            }
        }
    }

    public void EntroEnSocket(SelectEnterEventArgs args) => OnSocketSelectEntered(args);

    void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        var socket = args.interactorObject as XRSocketInteractor;
        var cardGO = args.interactableObject.transform.gameObject;
        SoundCardFather card = cardGO.GetComponent<SoundCardFather>();

        if (socket == null || card == null) return;

        if (card.GetColocada()) return; // if true

        var lvl = levels[nivelActual];

        int idx = Array.IndexOf(lvl.sockets, socket);
        if (idx < 0)
        {
            return;
        }

        // Alinea la carta con el socket por si acaso
        var attach = socket.attachTransform != null ? socket.attachTransform : socket.transform;
        card.AlinearEn(attach);

        bool esCorrecta = (card.GetIDCard() == lvl.nivelX[idx]);
        card.SetCorrecta(esCorrecta);

        if (esCorrecta)
        {
            card.BloquearEncontrada();
        }
        else
        {
            card.BloquearErronea();
        }

        socket.allowHover = false;
        socket.allowSelect = false;

        lvl.colocadas++;
        if (lvl.colocadas >= lvl.sockets.Length)
        {
            NivelTerminado();
        }

        //texto.text = $"Si detecto la carta {card.GetIDCard()}, colocada: {card.GetColocada()}, correcta: {card.GetCorrecta()}";
    }

    public void NivelTerminado()
    {
        // Desactiva los sockets por si no se hubiesen desactivado
        foreach (var s in levels[nivelActual].sockets)
        {
            if (s) 
            { 
                s.allowHover = false; 
                s.allowSelect = false;
            }
        }

        if (nivelActual + 1 < levels.Length)
        {
            nivelActual++;
            //texto.text = $"Nivel {nivelActual + 1} de {levels.Length}...";

            var nextSpawn = levels[nivelActual].spawnPoint;
            if (nextSpawn)
            {
                playerRig.MoveCameraToWorldLocation(nextSpawn.position);
                StartCoroutine(ReproducirAudioActual(5));
            }
        }
        else
        {
            // --- AQUÍ TERMINAN TODAS LAS ACTIVIDADES DE ESTA ESCENA ---
            JuegoTerminado();
        }
    }

    // FUNCIÓN MODIFICADA: Ahora guarda el progreso y llama a la transición.
    public void JuegoTerminado()
    {
        Debug.Log("Juego terminado. Iniciando transición al mapa.");
        
        // 1. Guardar el progreso (para que se cargue Pompones).
        SaveLevelProgress(); 
        
        // 2. Inicia la pausa y el Fade Out
        StartCoroutine(TransitionToProgressSceneRoutine());
    }

    // FUNCIÓN NUEVA: Guarda el avance secuencial
    private void SaveLevelProgress()
    {
        // El siguiente nivel a desbloquear es la ID de esta escena + 1
        int nextLevelToUnlock = currentLevelID + 1; 
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);

        if (nextLevelToUnlock > highestUnlocked)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, nextLevelToUnlock);
            PlayerPrefs.Save();
            Debug.Log($"[PROGRESS] Nivel {currentLevelID} completado. Guardando para Nivel {nextLevelToUnlock}");
        }
    }

    // FUNCIÓN NUEVA: Pausa, Fade Out y transición
    private IEnumerator TransitionToProgressSceneRoutine()
    {
        Debug.Log($"Transicionando al mapa en {endDelaySeconds} segundos...");

        // 1. Pausa de X segundos para que el jugador asimile que terminó
        yield return new WaitForSeconds(endDelaySeconds);

        // 2. Ejecutar Fade Out (oscurecer la pantalla)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            // Esperar la duración del Fade Out
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // 3. Cambiar a la escena del mapa
        UnityEngine.SceneManagement.SceneManager.LoadScene(mapSceneName);
    }

    public IEnumerator ReproducirAudioActual(float delay)
    {

        yield return new WaitForSeconds(delay);

        levels[nivelActual].audioSource.clip = clips[nivelActual];
        levels[nivelActual].audioSource.Play();

    }
}