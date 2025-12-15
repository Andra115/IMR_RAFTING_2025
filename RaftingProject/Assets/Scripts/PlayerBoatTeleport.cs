using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class PlayerBoatTeleport : MonoBehaviour
{
    public Transform boatEnterPoint;
    public Transform boatExitPoint;
    public float maxDistance = 12f;

    XROrigin xrOrigin;
    Transform cam;
    bool inBoat = false;

    void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
        cam = xrOrigin.Camera.transform;

        if (!boatEnterPoint) Debug.LogWarning("[BoatTeleport] boatEnterPoint not set");
        if (!boatExitPoint) Debug.LogWarning("[BoatTeleport] boatExitPoint not set");
    }

    void Start()
    {
        if (boatExitPoint != null)
        {
            xrOrigin.MoveCameraToWorldLocation(boatExitPoint.position);

            Vector3 e = transform.eulerAngles;
            e.y = boatExitPoint.eulerAngles.y;
            transform.eulerAngles = e;
        }
    }

    void Update()
    {
        // === DISTANTA FATA DE BARCA ===
        if (!inBoat && boatEnterPoint != null && cam != null)
        {
            float dist = Vector3.Distance(cam.position, boatEnterPoint.position);
            Debug.Log($"[BoatTeleport] Distanta pana la barca: {dist:F2} m");
        }

        if (!Keyboard.current.mKey.wasPressedThisFrame) return;

        if (!inBoat)
            EnterBoat();
        else
            ExitBoat();
    }

    void EnterBoat()
    {
        float dist = Vector3.Distance(cam.position, boatEnterPoint.position);

        if (dist > maxDistance)
        {
            Debug.Log($"[BoatTeleport] Prea departe ca sa intri in barca ({dist:F2} m)");
            return;
        }

        xrOrigin.MoveCameraToWorldLocation(boatEnterPoint.position);

        Vector3 e = transform.eulerAngles;
        e.y = boatEnterPoint.eulerAngles.y;
        transform.eulerAngles = e;

        inBoat = true;
        Debug.Log("[BoatTeleport] Ai intrat in barca");
    }

    void ExitBoat()
    {
        xrOrigin.MoveCameraToWorldLocation(boatExitPoint.position);

        Vector3 e = transform.eulerAngles;
        e.y = boatExitPoint.eulerAngles.y;
        transform.eulerAngles = e;

        inBoat = false;
        Debug.Log("[BoatTeleport] Ai iesit din barca");
    }
}
