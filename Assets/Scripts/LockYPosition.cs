using UnityEngine;

public class LockYPosition : MonoBehaviour
{
    private float lockedYPosition; // The Y position to lock to

    private void Start()
    {
        // Store the initial Y position of the object at runtime
        lockedYPosition = transform.position.y;
    }

    private void LateUpdate()
    {
        // Lock the Y position to the initial value
        Vector3 currentPosition = transform.position;
        transform.position = new Vector3(currentPosition.x, lockedYPosition, currentPosition.z);
    }
}
