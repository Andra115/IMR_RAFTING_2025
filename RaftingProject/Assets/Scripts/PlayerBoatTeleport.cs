using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

using XRInputDevice = UnityEngine.XR.InputDevice;
using XRNode = UnityEngine.XR.XRNode;
using XRCommonUsages = UnityEngine.XR.CommonUsages;

public class PlayerBoatTeleport : MonoBehaviour
{
    [Header("Boat Points")]
    public Transform boatEnterPoint;
    public Transform boatExitPoint;
    public float maxDistance = 8f;

    [Header("Boat Rigidbody")]
    public Rigidbody boatRigidbody;

    [Header("Disable While In Boat")]
    public Behaviour[] disableWhenInBoat;

    [Header("Enable While In Boat")]
    public Behaviour[] enableWhenInBoat;

    private XROrigin xrOrigin;
    private Transform cam;

    private bool inBoat = false;

    void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError("No XROrigin found.");
            enabled = false;
            return;
        }

        cam = xrOrigin.Camera.transform;
    }

    void Update()
    {
        if (!InteractPressed())
            return;

        if (!inBoat)
            TryEnterBoat();
        else
            ExitBoat();
    }

    bool InteractPressed()
    {
        // Keyboard (Editor)
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            return true;

        // Quest A button
        XRInputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (!rightHand.isValid)
            return false;

        bool pressed;
        if (rightHand.TryGetFeatureValue(XRCommonUsages.primaryButton, out pressed))
            return pressed;

        return false;
    }

    void TryEnterBoat()
    {
        if (boatEnterPoint == null || boatRigidbody == null)
            return;

        float dist = Vector3.Distance(cam.position, boatEnterPoint.position);

        if (dist > maxDistance)
            return;

        Debug.Log("Entering boat...");

        //  Parent FIRST
        xrOrigin.transform.SetParent(boatRigidbody.transform);

        //  Move the whole rig root (NOT the headset!)
        xrOrigin.transform.position = boatEnterPoint.position;

        //  Align yaw rotation only
        Vector3 rot = xrOrigin.transform.eulerAngles;
        rot.y = boatEnterPoint.eulerAngles.y;
        xrOrigin.transform.eulerAngles = rot;

        // Disable locomotion/scripts
        SetBehaviours(disableWhenInBoat, false);
        SetBehaviours(enableWhenInBoat, true);

        inBoat = true;
    }

    void ExitBoat()
    {
        if (boatExitPoint == null)
            return;

        Debug.Log("Exiting boat...");

        //  Unparent
        xrOrigin.transform.SetParent(null);

        //  Move rig root to exit point
        xrOrigin.transform.position = boatExitPoint.position;

        // Align yaw
        Vector3 rot = xrOrigin.transform.eulerAngles;
        rot.y = boatExitPoint.eulerAngles.y;
        xrOrigin.transform.eulerAngles = rot;

        // Re-enable locomotion/scripts
        SetBehaviours(disableWhenInBoat, true);
        SetBehaviours(enableWhenInBoat, false);

        inBoat = false;
    }

    void SetBehaviours(Behaviour[] list, bool state)
    {
        if (list == null) return;

        foreach (var b in list)
        {
            if (b != null)
                b.enabled = state;
        }
    }
}
