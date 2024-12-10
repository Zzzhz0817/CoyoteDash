using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [System.Serializable]
    public class Platform
    {
        public GameObject prefab; // Platform prefab
        public float forwardLength; // Length forward from the origin
        public float backwardLength; // Length backward from the origin
    }

    public Platform[] platforms; // Array of platform types
    public Transform player; // Reference to the player
    public float generateDistance = 20f; // Distance from the player to trigger platform generation

    public float gapDistance = 10f; // Gap between platforms
    public float initialX = 42f; // Starting X coordinate for the first platform
    public float yPosition = -3f; // Fixed Y coordinate for all platforms

    public System.Collections.Generic.List<float> platformEndpoints = new System.Collections.Generic.List<float>(); // List to store platform endpoints

    private float nextPlatformX; // The X coordinate to place the next platform

    private void Start()
    {
        // Initialize the starting position for the first platform
        nextPlatformX = initialX;
        
        // Add the initial X to platformEndpoints
        platformEndpoints.Add(initialX);
    }

    private void Update()
    {
        // Check if the player is close enough to the next platform position
        if (player.position.x + generateDistance >= nextPlatformX)
        {
            GeneratePlatform();
        }
    }

    private void GeneratePlatform()
    {
        // Randomly select a platform type
        int selectedIndex = Random.Range(0, platforms.Length);
        Platform selectedPlatform = platforms[selectedIndex];

        // Calculate the position of the new platform
        float platformX = nextPlatformX + gapDistance + selectedPlatform.forwardLength;
        Vector3 platformPosition = new Vector3(platformX, yPosition, 0);

        // Instantiate the selected platform
        Instantiate(selectedPlatform.prefab, platformPosition, Quaternion.identity);

        // Update the next platform X position
        nextPlatformX = platformX + selectedPlatform.backwardLength;

        // Add the new endpoint to platformEndpoints
        platformEndpoints.Add(nextPlatformX);
    }
}
