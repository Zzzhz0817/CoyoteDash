using UnityEngine;
using UnityEngine.UI; // Required for UI elements

public class CoyoteController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Maximum speed
    public float jumpForce = 5f; // Jump strength

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.0001f;
    public LayerMask groundLayer;

    [Header("Sprites")]
    public Sprite upSprite; // Sprite when rising
    public Sprite downSprite; // Sprite when falling

    [Header("UI")]
    public Text scoreText; // Reference to the UI Text for displaying the score
    public Text startScoreText; // Reference to the Start Score Text
    public GameObject startCanvas; // Reference to the Start Canvas

    private Rigidbody2D rb;
    private bool isGrounded;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private int score = 0; // Player's score
    private Vector3 originalPosition; // Player's starting position
    private bool isPaused = true; // Game starts in paused state

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Record the original starting position
        originalPosition = transform.position;

        // Initialize the score display
        //UpdateScoreText();

        // Pause the game and enable the Start Canvas
        PauseGame();
        startCanvas.SetActive(true);
    }

    private void Update()
    {
        // Resume game on Space key if paused
        if (isPaused && Input.GetKeyDown(KeyCode.Space))
        {
            ResumeGame();
            startCanvas.SetActive(false);
        }

        // Skip the rest of the logic if the game is paused
        if (isPaused) return;

        // Handle Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Apply horizontal velocity
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Handle Animator and Sprite Logic
        if (isGrounded)
        {
            // Enable Animator when on the ground
            animator.enabled = true;
        }
        else
        {
            // Disable Animator when in the air
            animator.enabled = false;

            // Change sprite based on vertical velocity
            if (rb.velocity.y > 0)
            {
                spriteRenderer.sprite = upSprite; // Rising sprite
            }
            else
            {
                spriteRenderer.sprite = downSprite; // Falling sprite
            }
        }

        // Check for death condition
        if (transform.position.y < -10f)
        {
            ResetPlayerPosition(); // Reset player to the starting position
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player collides with a platform
        if (collision.gameObject.CompareTag("Platform"))
        {
            score++; // Increment the score
            UpdateScoreText(); // Update the UI
        }
    }

    private void UpdateScoreText()
    {
        // Update the score display on the UI
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }

        // Update the start score text on the Start Canvas
        if (startScoreText != null)
        {
            startScoreText.text = "Score: " + score.ToString();
        }
    }

    private void ResetPlayerPosition()
    {
        // Reset the player's position and velocity
        transform.position = originalPosition;
        rb.velocity = Vector2.zero; // Stop the player's motion

        // Reset the score
        startScoreText.text = "Score: " + score.ToString();
        scoreText.text = " ";
        score = 0;

        // Pause the game and show the Start Canvas
        PauseGame();
        startCanvas.SetActive(true);
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Pause game
        isPaused = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // Resume game
        isPaused = false;
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
