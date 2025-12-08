using UnityEngine;

public class PlayerRaftController : MonoBehaviour
{
    public GameObject locomotionRoot;
    public Transform seatPoint;
    public Transform exitPoint;
    public Transform boatRoot;
    public Transform distancePoint;
    public KeyCode useKey = KeyCode.M;
    public float enterDistance = 7f;

    bool isOnRaft;
    CharacterController characterController;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (distancePoint == null && Camera.main != null)
            distancePoint = Camera.main.transform;
    }

    void Update()
    {
        if (!Input.GetKeyDown(useKey)) return;

        Debug.Log("M pressed, isOnRaft = " + isOnRaft);

        if (!isOnRaft)
            TryEnterRaft();
        else
            ExitRaft();
    }

    void TryEnterRaft()
    {
        if (seatPoint == null)
        {
            Debug.LogWarning("SeatPoint not assigned!");
            return;
        }

        if (boatRoot == null)
            boatRoot = seatPoint.parent;

        Transform p = distancePoint != null ? distancePoint : transform;

        // eu as folosi seatPoint ca referinta, nu centrul barcii
        Vector3 targetPos = seatPoint.position;

        float dist = Vector3.Distance(p.position, targetPos);

        Debug.Log($"Distance Check | Player: {p.position} | Seat: {targetPos} | Dist = {dist}");

        if (dist > enterDistance)
        {
            Debug.Log("Too far from boat to enter.");
            return;
        }

        EnterRaft();
    }

    void TeleportTo(Transform target)
    {
        if (target == null) return;

        if (characterController != null)
            characterController.enabled = false;

        transform.position = target.position;
        transform.rotation = target.rotation;

        if (characterController != null)
            characterController.enabled = true;
    }

    void EnterRaft()
    {
        isOnRaft = true;

        if (locomotionRoot != null)
            locomotionRoot.SetActive(false);

        TeleportTo(seatPoint);
        transform.SetParent(seatPoint);

        Debug.Log($"🚤 ENTERED RAFT | New Position: {transform.position} | Rotation: {transform.rotation.eulerAngles}");
    }

    void ExitRaft()
    {
        if (!isOnRaft) return;

        isOnRaft = false;
        transform.SetParent(null);

        if (exitPoint != null)
            TeleportTo(exitPoint);

        if (locomotionRoot != null)
            locomotionRoot.SetActive(true);

        Debug.Log($"🏊 EXITED RAFT | New Position: {transform.position} | Rotation: {transform.rotation.eulerAngles}");
    }
}
