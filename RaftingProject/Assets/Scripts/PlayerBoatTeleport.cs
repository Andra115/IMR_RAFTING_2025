using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class PlayerBoatTeleport : MonoBehaviour
{
    public Transform boatEnterPoint;
    public Transform boatExitPoint;
    public float maxDistance = 8f;

    public Rigidbody boatRigidbody;
    public bool followBoatYaw = true;

    public Behaviour[] disableWhenInBoat;
    public Behaviour[] enableWhenInBoat;

    XROrigin xrOrigin;
    Transform cam;

    bool inBoat = false;

    Vector3 lastBoatPos;
    Quaternion lastBoatRot;

    void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin == null) { enabled = false; return; }

        cam = xrOrigin.Camera != null ? xrOrigin.Camera.transform : null;
        if (cam == null) { enabled = false; return; }
    }

    void Start()
    {
        if (boatExitPoint != null)
        {
            xrOrigin.MoveCameraToWorldLocation(boatExitPoint.position);
            var e = transform.eulerAngles;
            e.y = boatExitPoint.eulerAngles.y;
            transform.eulerAngles = e;
        }
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.mKey.wasPressedThisFrame) return;

        if (!inBoat) EnterBoat();
        else ExitBoat();
    }

    void FixedUpdate()
    {
        if (!inBoat || boatRigidbody == null) return;

        var boatPos = boatRigidbody.position;
        var boatRot = boatRigidbody.rotation;

        var deltaPos = boatPos - lastBoatPos;
        transform.position += deltaPos;

        if (followBoatYaw)
        {
            var deltaRot = boatRot * Quaternion.Inverse(lastBoatRot);
            var yaw = deltaRot.eulerAngles.y;
            transform.Rotate(0f, yaw, 0f, Space.World);
        }

        lastBoatPos = boatPos;
        lastBoatRot = boatRot;
    }

    void EnterBoat()
    {
        if (boatEnterPoint == null || boatRigidbody == null) return;

        float dist = Vector3.Distance(cam.position, boatEnterPoint.position);
        if (dist > maxDistance) return;

        xrOrigin.MoveCameraToWorldLocation(boatEnterPoint.position);

        var e = transform.eulerAngles;
        e.y = boatEnterPoint.eulerAngles.y;
        transform.eulerAngles = e;

        SetBehaviours(disableWhenInBoat, false);
        SetBehaviours(enableWhenInBoat, true);

        lastBoatPos = boatRigidbody.position;
        lastBoatRot = boatRigidbody.rotation;

        inBoat = true;
    }

    void ExitBoat()
    {
        if (boatExitPoint == null) return;

        xrOrigin.MoveCameraToWorldLocation(boatExitPoint.position);

        var e = transform.eulerAngles;
        e.y = boatExitPoint.eulerAngles.y;
        transform.eulerAngles = e;

        SetBehaviours(disableWhenInBoat, true);
        SetBehaviours(enableWhenInBoat, false);

        inBoat = false;
    }

    void SetBehaviours(Behaviour[] list, bool state)
    {
        if (list == null) return;
        for (int i = 0; i < list.Length; i++)
            if (list[i] != null) list[i].enabled = state;
    }
}
