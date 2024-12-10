using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;

[Serializable]
public class GameLogEntry
{
    public string deviceName;          // Device name
    public string timestamp;           // Current timestamp
    public float timer;                // Remaining timer
    public bool isSuccess;             // Whether the player succeeded the jump
    public float distanceToNext;       // Distance to the next endpoint
    public float distanceToPrevious;   // Distance to the previous endpoint
    public string groupName;           // Player's group name

    public GameLogEntry(string deviceName, string timestamp, float timer, bool isSuccess, float distanceToNext, float distanceToPrevious, string groupName)
    {
        this.deviceName = deviceName;
        this.timestamp = timestamp;
        this.timer = timer;
        this.isSuccess = isSuccess;
        this.distanceToNext = distanceToNext;
        this.distanceToPrevious = distanceToPrevious;
        this.groupName = groupName;
    }

    // Converts the data to a CSV-compatible string
    public string ToCsvString()
    {
        return $"{deviceName},{timestamp},{timer},{isSuccess},{distanceToNext},{distanceToPrevious},{groupName}";
    }
}


public class CoyoteController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Maximum speed
    public float jumpForce = 5f; // Jump strength

    [Header("Ground Check")]
    public Transform groundCheck;
    public Transform ScoreGroundCheck;
    public float groundCheckRadius = 0.0001f;
    public LayerMask groundLayer;

    [Header("Sprites")]
    public Sprite upSprite; // Sprite when rising
    public Sprite downSprite; // Sprite when falling

    [Header("UI")]
    public Text scoreText; // Reference to the UI Text for displaying the score
    public Text startScoreText; // Reference to the Start Score Text
    public GameObject startCanvas; // Reference to the Start Canvas
    public Text timerText; // Reference to the UI Text for displaying the timer

    public string groupName;

    private Rigidbody2D rb;
    private bool isGrounded;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private int score = 0; // Player's score
    private Vector3 originalPosition; // Player's starting position
    private bool isPaused = true; // Game starts in paused state
    private float positionXShift = 0.631f;

    private float timer = 0f; // 3-minute timer in seconds
    private bool timerStarted = false; // Tracks if the timer has started

    private PlatformGenerator platformGenerator; // Reference to the PlatformGenerator script

    private GameLogEntry logEntryBuffer;
    
    private string logFilePath;

    private void Start()
    {

         // Set the file path for the CSV log
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{groupName}_{timestamp}.csv";
        logFilePath = Path.Combine(Application.dataPath, fileName);
        Debug.Log(logFilePath);

        // Write the CSV header if the file doesn't exist
        if (!File.Exists(logFilePath))
        {
            string header = "DeviceName,Timestamp,Timer,IsSuccess,DistanceToNext,DistanceToPrevious,GroupName";
            File.AppendAllText(logFilePath, header + Environment.NewLine);
        }


        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Record the original starting position
        originalPosition = transform.position;

        // Pause the game and enable the Start Canvas
        PauseGame();
        startCanvas.SetActive(true);

        // Initialize the timer display
        UpdateTimerText();

        // Get the PlatformGenerator component
        platformGenerator = FindObjectOfType<PlatformGenerator>();
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

        // Start the timer when player position.x > 33
        if (!timerStarted && transform.position.x -  positionXShift > 33f)
        {
            timerStarted = true;
        }

        // Count down the timer if it has started
        if (timerStarted)
        {
            timer += Time.deltaTime;
            UpdateTimerText();
            Debug.Log(timer);

            // Check if the timer has run out
            if (timer >= 120f)
            {
                TimerEnd();
            }
        }

        // Handle Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Apply horizontal velocity
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            float playerX = transform.position.x;
            // float playerX = transform.position.x - positionXShift;

            // Calculate distances
            float nextEndpoint = float.MaxValue;
            float previousEndpoint = float.MinValue;

            foreach (var endpoint in platformGenerator.platformEndpoints)
            {
                if (endpoint > playerX && endpoint < nextEndpoint)
                {
                    nextEndpoint = endpoint;
                }
                if (endpoint <= playerX && endpoint > previousEndpoint)
                {
                    previousEndpoint = endpoint;
                }
            }

            float distanceToNext = nextEndpoint == float.MaxValue ? -1 : nextEndpoint - playerX;
            float distanceToPrevious = previousEndpoint == float.MinValue ? -1 : playerX - previousEndpoint;

            // TODO: if the timer <= 120f, assign every value of class GameLogEntry excepte the isSuccess to the logdataBuffer.
            // Assign values to logEntryBuffer except isSuccess
            logEntryBuffer = new GameLogEntry(
                SystemInfo.deviceName,                  // Device name
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Timestamp
                timer,                                 // Timer
                false,                                 // Placeholder for isSuccess
                distanceToNext,                        // Distance to the next endpoint
                distanceToPrevious,                    // Distance to the previous endpoint
                groupName                              // Group name
            );
            
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
            // TODO: write the isSuccess as false to the logdatabuffer, and 
            // Write the log entry to the CSV file
            //  File.AppendAllText(logFilePath, logdatabuffer.ToCsvString() + Environment.NewLine);
            if (logEntryBuffer != null)
            {
                logEntryBuffer.isSuccess = false; // Update isSuccess in the buffer

                // Write the log entry to the CSV file
                File.AppendAllText(logFilePath, logEntryBuffer.ToCsvString() + Environment.NewLine);
            }


            ResetPlayerPosition(); // Reset player to the starting position
        }

        // Find and log the closest platform endpoint that is greater than the player's position if playerX > 33
        if (platformGenerator != null)
        {
            float playerX = transform.position.x - positionXShift; // Get the player's current X position

            if (playerX > 33) // Perform the logic only if playerX > 33
            {
                float closestEndpoint = float.MaxValue; // Initialize with a large value

                foreach (var endpoint in platformGenerator.platformEndpoints)
                {
                    // Check if the endpoint is greater than the player's position and closer than the current closest
                    if (endpoint > playerX && endpoint < closestEndpoint)
                    {
                        closestEndpoint = endpoint;
                    }
                }

                // Log the difference if a valid endpoint is found
                if (closestEndpoint != float.MaxValue)
                {
                    float difference = closestEndpoint - playerX;
                    Debug.Log($"Closest Endpoint: {closestEndpoint}, Difference: {difference}");
                }
                else
                {
                    Debug.Log("No endpoint found greater than the player's position.");
                }
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player collides with a platform
        if (collision.gameObject.CompareTag("Platform") && Physics2D.OverlapCircle(ScoreGroundCheck.position, groundCheckRadius, groundLayer))
        {
            // TODO: write the isSuccess as true to the logdatabuffer, and 
            // Write the log entry to the CSV file
            //  File.AppendAllText(logFilePath, logdatabuffer.ToCsvString() + Environment.NewLine);
            if (logEntryBuffer != null)
            {
                logEntryBuffer.isSuccess = true; // Update isSuccess in the buffer

                // Write the log entry to the CSV file
                File.AppendAllText(logFilePath, logEntryBuffer.ToCsvString() + Environment.NewLine);
            }

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

    private void UpdateTimerText()
    {
        // Update the timer display on the UI
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
        }
    }

    private void TimerEnd()
    {
        // Actions to perform when the timer ends
        timerStarted = false;
        Debug.Log("Timer ended!");
        Application.Quit();
        // Add logic for when the timer ends, e.g., show game over screen
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

        // Reset the timer
        timerStarted = false;
        UpdateTimerText();
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

        if (ScoreGroundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ScoreGroundCheck.position, groundCheckRadius);
        }
    }

    private void LogDataToCsv(bool isSuccess, float distanceToNext, float distanceToPrevious)
    {
        GameLogEntry logEntry = new GameLogEntry(
            SystemInfo.deviceName,                  // Device name
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Timestamp
            timer,                                 // Remaining timer
            isSuccess,                             // Success or not
            distanceToNext,                        // Distance to the next endpoint
            distanceToPrevious,                    // Distance to the previous endpoint
            groupName                              // Group name
        );

        // Write the log entry to the CSV file
        File.AppendAllText(logFilePath, logEntry.ToCsvString() + Environment.NewLine);
    }

}
