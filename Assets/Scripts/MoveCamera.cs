using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    private Player player;
    private float radius;
    private Vector3 previousPlayerPosition;

    void Start()
    {
        player = GameObject.FindAnyObjectByType<Player>();
        radius = Vector3.Distance(player.transform.position, transform.position);
        previousPlayerPosition = player.transform.position;
    }

    void Update()
    {
        Vector3 currentPlayerPosition = player.transform.position;
        Vector3 movementDir = (currentPlayerPosition - previousPlayerPosition).normalized;
        if (movementDir != Vector3.zero) {
            transform.position = currentPlayerPosition - (movementDir * radius);
            transform.forward = movementDir;
            previousPlayerPosition = currentPlayerPosition;
        }
    }
}
