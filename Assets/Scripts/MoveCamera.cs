using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private float followSpeed;
    [SerializeField] private float rotateSpeed;
    private Player player;
    private Vector3 offset;

    void Start()
    {
        player = GameObject.FindAnyObjectByType<Player>();
        offset = Quaternion.Inverse(player.transform.rotation) * (transform.position - player.transform.position);
    }

    void Update()
    {
        // Desired position
        Vector3 desiredPosition =
            player.transform.position +
            player.transform.rotation * offset;

        // Smooth follow
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Look at player
        Quaternion playerRotation = Quaternion.LookRotation(
            player.transform.position - transform.position
        );

        Vector3 euler = playerRotation.eulerAngles;
        euler.x = 30f;
        euler.z = 0f;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(euler),
            rotateSpeed * Time.deltaTime
        );
    }
}
