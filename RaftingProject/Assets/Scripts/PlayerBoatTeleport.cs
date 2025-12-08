using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class PlayerBoatTeleport : MonoBehaviour
{
    public Transform boatEnterPoint;  // punct in barca
    public Transform boatExitPoint;   // punct langa barca
    public float maxDistance = 100f;

    XROrigin xrOrigin;
    Transform cam;

    bool inBoat = false;

    void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin == null)
            Debug.LogError("[BoatTeleport] XROrigin not found on this object");

        cam = xrOrigin != null && xrOrigin.Camera != null
            ? xrOrigin.Camera.transform
            : Camera.main != null ? Camera.main.transform : null;

        if (boatEnterPoint == null)
            Debug.LogError("[BoatTeleport] boatEnterPoint not set");

        if (boatExitPoint == null)
            Debug.LogError("[BoatTeleport] boatExitPoint not set");

        if (cam == null)
            Debug.LogError("[BoatTeleport] Camera not found");
    }

    void Update()
    {
        bool mPressed = false;

        if (Input.GetKeyDown(KeyCode.M))
            mPressed = true;
        if (!mPressed && Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            mPressed = true;

        if (!mPressed || cam == null || xrOrigin == null || boatEnterPoint == null || boatExitPoint == null)
            return;

        if (!inBoat)
        {
            // urcat in barca
            float dist = Vector3.Distance(cam.position, boatEnterPoint.position);
            Debug.Log("[BoatTeleport] Dist camera -> enterPoint = " + dist);

            if (dist > maxDistance)
            {
                Debug.Log("[BoatTeleport] Too far to enter boat");
                return;
            }

            Debug.Log("[BoatTeleport] ENTER BOAT");
            xrOrigin.MoveCameraToWorldLocation(boatEnterPoint.position);

            Vector3 e = transform.eulerAngles;
            e.y = boatEnterPoint.eulerAngles.y;
            transform.eulerAngles = e;

            inBoat = true;
        }
        else
        {
            // coborat din barca
            Debug.Log("[BoatTeleport] EXIT BOAT");
            xrOrigin.MoveCameraToWorldLocation(boatExitPoint.position);

            Vector3 e = transform.eulerAngles;
            e.y = boatExitPoint.eulerAngles.y;
            transform.eulerAngles = e;

            inBoat = false;
        }
    }
}
