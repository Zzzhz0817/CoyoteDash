using UnityEngine;

public class CoyoteController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 16f; // Maximum speed
    public float acceleration = 20f; // How fast the character reaches max speed
    public float deceleration = 25f; // How fast the character slows down
    public float jumpForce = 12f; // Jump strength

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.0001f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float currentVelocityX;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Handle Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        Debug.Log(isGrounded);


        // Horizontal Movement
        float moveInput = Input.GetAxisRaw("Horizontal"); // -1 for left, 1 for right
        if (moveInput != 0)
        {
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, moveInput * moveSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, 0, deceleration * Time.deltaTime);
        }

        // Apply horizontal velocity
        rb.velocity = new Vector2(currentVelocityX, rb.velocity.y);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize ground check in the editor
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
