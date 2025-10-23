using System;
using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para la carga de escena
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SortSizeGame : MonoBehaviour
{
    // --- VARIABLES AÑADIDAS PARA TRANSICIÓN ---
    [Header("Transición Final")]
    [SerializeField] private string finalSceneName = "Escenas/Final"; // Nombre de la escena del final 
    [SerializeField] private float endDelaySeconds = 3.0f; // Pausa después de terminar el nivel
    public FadeScreen fadeScreen; // ARRASTRA EL FADEPLANE AQUÍ
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

            // --- CORRECCIÓN: ASIGNAR LISTENERS AQUÍ, YA QUE 'Start' NO SE EJECUTA SI EL JUEGO NO EMPIEZA AQUÍ ---
            foreach (var s in lvl.sockets)
            {
                if (s == null)
                {
                    continue;
                }
                s.selectEntered.AddListener(OnSocketSelectEntered); // Asignación de listener
            }
        }
    }

    // --- FUNCIÓN QUE DEBES LLAMAR DESDE EL ON SELECT DEL SOCKET ---
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
            objeto.BloquearEncontrada();
        else
            objeto.BloquearErronea();

        socket.allowHover = false;
        socket.allowSelect = false;

        lvl.colocadas++;
        if (lvl.colocadas >= lvl.sockets.Length)
        {
            NivelTerminado();
        }

        texto.text = $"Objeto {objeto.GetIDCard()}, colocada: {objeto.GetColocada()}, correcta: {objeto.GetCorrecta()}";
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
            texto.text = $"Nivel {nivelActual + 1} de {levels.Length}...";

            var nextSpawn = levels[nivelActual].spawnPoint;
            if (nextSpawn)
            {
                playerRig.MoveCameraToWorldLocation(nextSpawn.position);
            }
        }
        else
        {
            // --- EL ÚLTIMO NIVEL DEL ARRAY HA TERMINADO ---
            JuegoTerminado();
        }
    }

    // FUNCIÓN MODIFICADA: Ahora reinicia el progreso y transiciona al Menú
    public void JuegoTerminado()
    {
        texto.text = "¡Aventura Terminada!";
        
        // Reiniciamos el progreso para que la próxima partida inicie en el Nivel 1.
        GoToMenuAndReset();
    }
    
    // --- FUNCIÓN DE TRANSICIÓN FINAL Y REINICIO ---
    public void GoToMenuAndReset()
    {
        // 1. Reiniciar el progreso guardado
        PlayerPrefs.DeleteKey(PROGRESS_KEY);
        Debug.Log("Aventura terminada. Progreso reseteado a Nivel 1.");
        
        // 2. Iniciar la transición al menú
        StartCoroutine(TransitionToMenuRoutine());
    }

    private IEnumerator TransitionToMenuRoutine()
    {
        // Pausa breve para el mensaje final
        yield return new WaitForSeconds(endDelaySeconds); 

        // Fade OUT (oscurecer la pantalla)
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // Cargar la escena del Final
        SceneManager.LoadScene(finalSceneName);
    }
}