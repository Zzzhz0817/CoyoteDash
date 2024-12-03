using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Reference to the coyote
    public Vector2 deadZoneSize = new Vector2(4f, 3f); // Width and height of the dead zone

    [Header("Camera Settings")]
    public float smoothSpeed = 5f; // Smoothness of the camera movement

    private Vector3 targetPosition;

    private void LateUpdate()
    {
        if (target == null) return;

        // Get the camera's position and the target's position
        Vector3 cameraPos = transform.position;
        Vector3 targetPos = target.position;

        // Check if the target is outside the dead zone
        Vector2 offset = new Vector2(
            Mathf.Abs(targetPos.x - cameraPos.x),
            Mathf.Abs(targetPos.y - cameraPos.y)
        );

        if (offset.x > deadZoneSize.x / 2 || offset.y > deadZoneSize.y / 2)
        {
            // Move the camera towards the target but within the bounds of the dead zone
            targetPosition = new Vector3(
                Mathf.Clamp(targetPos.x, cameraPos.x - deadZoneSize.x / 2, cameraPos.x + deadZoneSize.x / 2),
                Mathf.Clamp(targetPos.y, cameraPos.y - deadZoneSize.y / 2, cameraPos.y + deadZoneSize.y / 2),
                cameraPos.z // Keep the Z-axis fixed
            );

            // Smoothly interpolate the camera's position
            transform.position = Vector3.Lerp(cameraPos, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the dead zone in the Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneSize.x, deadZoneSize.y, 0));
    }
}
