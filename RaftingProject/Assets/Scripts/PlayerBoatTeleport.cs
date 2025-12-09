using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class PlayerBoatTeleport : MonoBehaviour
{
    public Transform boatEnterPoint;
    public Transform boatExitPoint;
    public float maxDistance = 100f;

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
        // fix: pozitionam playerul controlat la inceput
        if (boatExitPoint != null)
        {
            xrOrigin.MoveCameraToWorldLocation(boatExitPoint.position);

            // aliniem rotatia
            Vector3 e = transform.eulerAngles;
            e.y = boatExitPoint.eulerAngles.y;
            transform.eulerAngles = e;

            Debug.Log("[BoatTeleport] Spawned player at exit point (fix for first teleport)");
        }
    }

    void Update()
    {
        if (!Keyboard.current.mKey.wasPressedThisFrame) return;

        if (!inBoat)
            EnterBoat();
        else
            ExitBoat();
    }

    void EnterBoat()
    {
        xrOrigin.MoveCameraToWorldLocation(boatEnterPoint.position);

        Vector3 e = transform.eulerAngles;
        e.y = boatEnterPoint.eulerAngles.y;
        transform.eulerAngles = e;

        inBoat = true;
    }

    void ExitBoat()
    {
        xrOrigin.MoveCameraToWorldLocation(boatExitPoint.position);

        Vector3 e = transform.eulerAngles;
        e.y = boatExitPoint.eulerAngles.y;
        transform.eulerAngles = e;

        inBoat = false;
    }
}
