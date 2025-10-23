using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class Card : MonoBehaviour
{
    [Header("Juego")]
    public int parId;
    [SerializeField] private Transform _homeAnchor;
    public bool useFrontBackToggle = true;

    [Header("Leash")]
    public float maxPull = 0.12f;
    public Vector2 lateralClamp = new Vector2(0.05f, 0.05f);
    public float returnDuration = 0.15f;

    [Header("Flip")]
    public float flipDuration = 0.18f;

    public bool FaceUp { get; private set; }
    public bool Matched { get; private set; }

    XRGrabInteractable grab;
    Rigidbody rb;
    Coroutine anim;
    IXRSelectInteractor currentHand;

    // Plano base del anchor
    Vector3 p0; Quaternion r0; Vector3 n, ax, ay;

    Transform front, back;

    public Transform HomeAnchor => _homeAnchor;
    public int PairId => parId;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        if (useFrontBackToggle)
        {
            front = transform.Find("Front");
            back = transform.Find("Back");
        }
    }

    void Start()
    {
        if (_homeAnchor != null) RebindHomeFromAnchor();
    }

    void Update()
    {
        if (currentHand != null && !Matched)
        {
            Vector3 want = grab.firstInteractorSelecting.GetAttachTransform(grab).position;

            Vector3 d = want - p0;
            Vector3 dPlane = Vector3.ProjectOnPlane(d, n);

            float dx = Mathf.Clamp(Vector3.Dot(dPlane, ax), -lateralClamp.x, lateralClamp.x);
            float dy = Mathf.Clamp(Vector3.Dot(dPlane, ay), -lateralClamp.y, lateralClamp.y);

            float dn = Mathf.Clamp(Vector3.Dot(d, n), 0f, maxPull);

            Vector3 clamped = p0 + ax * dx + ay * dy + n * dn;
            transform.position = Vector3.Lerp(transform.position, clamped, 0.5f);
        }


    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (!GameManager.Instance.CanInteract(this))
        {
            grab.interactionManager.SelectExit(args.interactorObject, grab);
            return;
        }
        currentHand = args.interactorObject;

        if (!FaceUp)
        {
            Flip(true, snapHome: false);
            GameManager.Instance.NotifyReveal(this);
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        currentHand = null;
        if (!Matched) StartCoroutine(MoveTo(p0, transform.rotation, returnDuration));
    }

    public void SetMatched(bool value)
    {
        Matched = value;
        if (Matched)
        {
            FaceUp = true;
            StopAnims();
            SnapToHome();
            grab.enabled = false;
            if (useFrontBackToggle) SetFrontBackVisible(true);
        }
    }

    public void Flip(bool toFaceUp, bool snapHome)
    {
        StopAnims();
        anim = StartCoroutine(FlipRoutine(toFaceUp, snapHome));
    }

    IEnumerator FlipRoutine(bool toFaceUp, bool snapHome)
    {
        if (snapHome) yield return MoveTo(p0, transform.rotation, returnDuration * 0.8f);

        Quaternion startRot = transform.rotation;
        Vector3 targetForward = toFaceUp ? n : -n;             
        Quaternion endRot = Quaternion.LookRotation(targetForward, ay);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flipDuration;
            float k = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, k);
            yield return null;
        }
        transform.rotation = endRot;

        if (useFrontBackToggle) SetFrontBackVisible(toFaceUp);

        FaceUp = toFaceUp;
        anim = null;
    }

    IEnumerator MoveTo(Vector3 pos, Quaternion keepRot, float dur)
    {
        Vector3 pS = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, dur);
            float k = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(pS, pos, k);
            yield return null;
        }
        transform.position = pos;
    }

    void SnapToHome()
    {
        transform.position = p0;
        transform.rotation = Quaternion.LookRotation(FaceUp ? n : -n, ay);
    }

    void StopAnims()
    {
        if (anim != null) StopCoroutine(anim);
        anim = null;
    }

    void SetFrontBackVisible(bool showFront)
    {
        if (front) front.gameObject.SetActive(showFront);
        if (back) back.gameObject.SetActive(!showFront);
    }

    public void SetHomeAnchor(Transform anchor)
    {
        _homeAnchor = anchor;
        RebindHomeFromAnchor();
    }

    void RebindHomeFromAnchor()
    {
        if (_homeAnchor == null) return;

        p0 = _homeAnchor.position;

        
        n = _homeAnchor.forward; n.Normalize();                         
        ax = Vector3.ProjectOnPlane(_homeAnchor.right, n); ax.Normalize(); 
        ay = Vector3.Cross(n, ax); ay.Normalize();                         

        // Estado inicial: boca abajo
        FaceUp = false;
        if (useFrontBackToggle) SetFrontBackVisible(false);
        SnapToHome();
    }
}
