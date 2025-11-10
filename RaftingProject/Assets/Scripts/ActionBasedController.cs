using UnityEngine;
using UnityEngine.XR;

public class ActionBasedController : MonoBehaviour
{
    public XRNode controllerNode = XRNode.LeftHand;

    void Update()
    {
        // Get position and rotation from XR device
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);

        if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
        {
            transform.localPosition = pos;
        }

        if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
        {
            transform.localRotation = rot;
        }
    }
}