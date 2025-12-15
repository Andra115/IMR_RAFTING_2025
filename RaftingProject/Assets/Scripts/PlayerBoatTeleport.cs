using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class PlayerBoatTeleport : MonoBehaviour
{
    public Transform boatEnterPoint;
    public Transform boatExitPoint;
    public float maxDistance = 8f;

    public Rigidbody boatRigidbody;
    public bool followBoatYaw = true;

    public Behaviour[] disableWhenInBoat;
    public Behaviour[] enableWhenInBoat;

    public XRDeviceSimulator xrDeviceSimulator;

    XROrigin xrOrigin;
    Transform cam;

    bool inBoat = false;

    Vector3 lastBoatPos;
    Quaternion lastBoatRot;

    List<InputAction> simulatorMoveActions = new List<InputAction>();

    void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin == null) { enabled = false; return; }

        cam = xrOrigin.Camera != null ? xrOrigin.Camera.transform : null;
        if (cam == null) { enabled = false; return; }

        if (!xrDeviceSimulator)
            xrDeviceSimulator = FindFirstObjectByType<XRDeviceSimulator>();

        CacheSimulatorWASDActions();
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

        var boatPos = boatEnterPoint.position;
        var boatRot = boatEnterPoint.rotation;

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

        SetSimulatorWASDEnabled(false);
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

        SetSimulatorWASDEnabled(true);
    }

    void SetBehaviours(Behaviour[] list, bool state)
    {
        if (list == null) return;
        for (int i = 0; i < list.Length; i++)
            if (list[i] != null) list[i].enabled = state;
    }

    void CacheSimulatorWASDActions()
    {
        simulatorMoveActions.Clear();
        if (xrDeviceSimulator == null) return;

        var asset = xrDeviceSimulator.deviceSimulatorActionAsset;
        if (asset == null) return;

        foreach (var map in asset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                bool isMovementByName = false;
                var n = action.name.ToLowerInvariant();
                if (n.Contains("move") || n.Contains("translate") || n.Contains("position") || n.Contains("locomotion"))
                    isMovementByName = true;

                bool hasKeyboardWASD = false;
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var p = action.bindings[i].effectivePath;
                    if (string.IsNullOrEmpty(p)) continue;
                    var lp = p.ToLowerInvariant();
                    if (lp.Contains("<keyboard>/w") || lp.Contains("<keyboard>/a") || 
                        lp.Contains("<keyboard>/s") || lp.Contains("<keyboard>/d") || 
                        lp.Contains("<keyboard>/q") || lp.Contains("<keyboard>/e"))
                    {
                        hasKeyboardWASD = true;
                        break;
                    }
                }

                if (isMovementByName && hasKeyboardWASD)
                    simulatorMoveActions.Add(action);
            }
        }
    }

    void SetSimulatorWASDEnabled(bool enabled)
    {
        if (simulatorMoveActions == null || simulatorMoveActions.Count == 0) return;

        for (int i = 0; i < simulatorMoveActions.Count; i++)
        {
            var a = simulatorMoveActions[i];
            if (a == null) continue;

            if (enabled) a.Enable();
            else a.Disable();
        }
    }
}
