using System;
using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SortSizeGame : MonoBehaviour
{
    // --- VARIABLES AÑADIDAS PARA TRANSICIÓN Y PROGRESO ---
    [Header("Progreso y Transición")]
    [Tooltip("El ID del nivel que se desbloqueará DESPUÉS de este (ej: Nivel 5 se desbloquea al terminar Nivel 4).")]
    [SerializeField] private int nextLevelToUnlockID = 5; // El nivel que sigue a este. 
    [SerializeField] private string progressSceneName = "Escenas/ProgressBar"; // Nombre de la escena a donde regresa (ProgressBar)
    [SerializeField] private float endDelaySeconds = 3.0f; 
    public FadeScreen fadeScreen;
    private const string PROGRESS_KEY = "HighestUnlockedLevel";
    // ------------------------------------------

    [Header("Spawn inicial del jugador")]
    [SerializeField] private Transform startSpawnPoint;

    [Header("Texto de prueba")]
    [SerializeField] TextMeshProUGUI texto;

    [Header("Jugador")]
    [SerializeField] XROrigin playerRig;

    [Serializable]
    public class level
    {
        [Header("Sockets del nivel")]
        public XRSocketInteractor[] sockets;

        [Header("Orden esperado")]
        public int[] nivelX;

        [Header("Objetos disponibles de este nivel")]
        public ObjectSort[] cards;

        [Header("Posicion del siguiente nivel")]
        public Transform spawnPoint;

        [HideInInspector] public int colocadas;
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
            playerRig.MatchOriginUpCameraForward(startSpawnPoint.up, startSpawnPoint.forward); 
        }

        foreach (level lvl in levels)
        {
            if (lvl.sockets == null || lvl.nivelX == null || lvl.sockets.Length != lvl.nivelX.Length)
            {
                Debug.LogError($"[SortSizeGame] Level {lvl} mal configurado: sockets y expectedOrder deben tener mismo largo.");
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
    }

    public void EntroEnSocket(SelectEnterEventArgs args) => OnSocketSelectEntered(args); 

    void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        var socket = args.interactorObject as XRSocketInteractor;
        var objGO = args.interactableObject.transform.gameObject;
        ObjectSort objeto = objGO.GetComponent<ObjectSort>();

        if (socket == null || objeto == null) return;
        if (objeto.GetColocada()) return;

        var lvl = levels[nivelActual];
        int idx = Array.IndexOf(lvl.sockets, socket);
        if (idx < 0) return;

        var attach = socket.attachTransform != null ? socket.attachTransform : socket.transform;
        objeto.AlinearEn(attach);

        bool esCorrecta = (objeto.GetIDCard() == lvl.nivelX[idx]);
        objeto.SetCorrecta(esCorrecta);

        if (esCorrecta)
        {
            objeto.BloquearEncontrada();
        }
        else
        {
            objeto.BloquearErronea();
        }

        socket.allowHover = false;
        socket.allowSelect = false;

        lvl.colocadas++;
        if (lvl.colocadas >= lvl.sockets.Length)
        {
            NivelTerminado();
        }
    }

    public void NivelTerminado()
    {
        // Desactiva los sockets
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
            // --- CÓDIGO DE MULTI-NIVEL EN UNA ESCENA ---
            nivelActual++;
            //texto.text = $"Nivel {nivelActual + 1} de {levels.Length}...";

            var nextSpawn = levels[nivelActual].spawnPoint;
            if (nextSpawn)
            {
                playerRig.MoveCameraToWorldLocation(nextSpawn.position);
                playerRig.MatchOriginUpCameraForward(nextSpawn.up, nextSpawn.forward);
            }
        }
        else
        {
            // --- EL ÚLTIMO SUB-NIVEL DENTRO DE ESTA ESCENA HA TERMINADO ---
            SaveAndGoToProgressScene(); // LLAMADA CLAVE PARA AVANZAR EL PROGRESO
        }
    }

    // ⭐ FUNCIÓN CLAVE: GUARDA EL PROGRESO Y VUELVE A LA PANTALLA DE PROGRESO ⭐
    public void SaveAndGoToProgressScene()
    {
        // 1. Aumentar el progreso guardado a 'nextLevelToUnlockID'
        int highestUnlocked = PlayerPrefs.GetInt(PROGRESS_KEY, 1);
        if (nextLevelToUnlockID > highestUnlocked)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, nextLevelToUnlockID);
            PlayerPrefs.Save();
            Debug.Log($"[PROGRESS] Nivel '{nextLevelToUnlockID}' desbloqueado. Volviendo a la escena de progreso.");
        }

        // 2. Iniciar la transición
        StartCoroutine(TransitionToProgressSceneRoutine());
    }

    private IEnumerator TransitionToProgressSceneRoutine()
    {
        // Pausa breve para el mensaje final
        yield return new WaitForSeconds(endDelaySeconds); 

        // Fade OUT (oscurecer la pantalla)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // Cargar la escena de Progreso para cargar el siguiente nivel
        SceneManager.LoadScene(progressSceneName);
    }
}