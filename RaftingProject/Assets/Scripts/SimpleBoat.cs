using UnityEngine;

public class SimpleBoat : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 40f;

    void Update()
    {
        float move = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.Translate(Vector3.forward * move);

        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;
        transform.Rotate(0f, turn, 0f);
    }
}
