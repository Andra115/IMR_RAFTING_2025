using UnityEngine;

public class VRBoatController : MonoBehaviour
{
    [Header("References")]
    public Transform xrOrigin;        // XR Origin root (player)
    public Rigidbody boatRb;          // Rigidbody of the boat
    public Transform seatOffset;      // Where the player sits inside the boat

    [Header("Boat Settings")]
    public bool lockHeight = true;    // Keep boat at water level
    public float fixedBoatHeight = 5f;

    void FixedUpdate()
    {
        // Repozitioneazã player-ul peste barcã
        if (xrOrigin != null && seatOffset != null)
        {
            xrOrigin.position = boatRb.position + seatOffset.localPosition;
            xrOrigin.rotation = seatOffset.rotation;
        }

        // Lock height
        if (lockHeight)
            LockBoatHeight();
    }

    void LockBoatHeight()
    {
        Vector3 pos = boatRb.position;
        pos.y = fixedBoatHeight;
        boatRb.MovePosition(pos);

        Vector3 vel = boatRb.linearVelocity;
        vel.y = 0f;
        boatRb.linearVelocity = vel;
    }
}
