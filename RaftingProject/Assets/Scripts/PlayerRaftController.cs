using UnityEngine;

public class PlayerRaftController : MonoBehaviour
{
    public GameObject locomotionRoot;
    public Transform seatPoint;
    public Transform exitPoint;
    public KeyCode useKey = KeyCode.M;

    bool isOnRaft;

    void Update()
    {
        if (Input.GetKeyDown(useKey))
        {
            Debug.Log("M pressed, isOnRaft = " + isOnRaft);

            if (!isOnRaft)
                EnterRaft();
            else
                ExitRaft();
        }
    }

    void EnterRaft()
    {
        if (seatPoint == null)
        {
            Debug.LogWarning("SeatPoint not assigned!");
            return;
        }

        isOnRaft = true;

        if (locomotionRoot != null)
            locomotionRoot.SetActive(false);

        transform.position = seatPoint.position;
        transform.rotation = seatPoint.rotation;
        transform.SetParent(seatPoint);

        Debug.Log("Entered raft");
    }

    void ExitRaft()
    {
        if (!isOnRaft)
            return;

        isOnRaft = false;
        transform.SetParent(null);

        if (exitPoint != null)
        {
            transform.position = exitPoint.position;
            transform.rotation = exitPoint.rotation;
        }
        else
        {
            Debug.LogWarning("ExitPoint not assigned!");
        }

        if (locomotionRoot != null)
            locomotionRoot.SetActive(true);

        Debug.Log("Exited raft");
    }
}