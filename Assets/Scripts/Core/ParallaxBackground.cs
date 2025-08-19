using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Range(0f, 1f)]
    public float parallaxSpeed = 0.5f;
    
    [Header("Movement Options")]
    public bool moveHorizontal = true;
    public bool moveVertical = false;
    
    [Header("Infinite Scroll (Optional)")]
    public bool infiniteScroll = false;
    
    [Header("Debug")]
    public bool debugMode = false;
    
    // Private variables
    private Transform cameraTransform;
    private Vector3 startPosition;
    private Vector3 lastCameraPosition;
    private float spriteWidth;
    private float spriteHeight;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Automatically find the main camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
            if (debugMode)
                Debug.Log($"Parallax: Found camera '{mainCamera.name}' for object '{gameObject.name}'", this);
        }
        else
        {
            Debug.LogError($"Parallax: No main camera found for object '{gameObject.name}'. Make sure your camera has the 'MainCamera' tag.", this);
            enabled = false; // Disable this component if no camera found
            return;
        }

        // Store the starting position for reference
        startPosition = transform.position;
        lastCameraPosition = cameraTransform.position;

        // Get sprite renderer component for infinite scroll calculations
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Calculate sprite dimensions for infinite scrolling
        if (infiniteScroll && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteWidth = spriteRenderer.bounds.size.x;
            spriteHeight = spriteRenderer.bounds.size.y;
            
            if (debugMode)
                Debug.Log($"Parallax: Sprite size for '{gameObject.name}' - Width: {spriteWidth}, Height: {spriteHeight}", this);
        }
        else if (infiniteScroll && spriteRenderer == null)
        {
            Debug.LogWarning($"Parallax: Infinite scroll enabled but no SpriteRenderer found on '{gameObject.name}'", this);
        }

        // Clamp parallax speed to prevent moving faster than camera
        parallaxSpeed = Mathf.Clamp01(parallaxSpeed);
    }

    void LateUpdate()
    {
        // Safety check for camera reference
        if (cameraTransform == null) 
        {
            if (debugMode)
                Debug.LogWarning($"Parallax: Camera reference lost for '{gameObject.name}'", this);
            return;
        }

        // Calculate camera movement since last frame
        Vector3 cameraMovement = cameraTransform.position - lastCameraPosition;
        
        // Apply parallax effect with speed limitation
        Vector3 parallaxMovement = Vector3.zero;
        
        if (moveHorizontal)
        {
            parallaxMovement.x = cameraMovement.x * parallaxSpeed;
        }
        
        if (moveVertical)
        {
            parallaxMovement.y = cameraMovement.y * parallaxSpeed;
        }

        // Move the object with parallax effect
        transform.position += parallaxMovement;

        // Handle infinite scrolling if enabled
        if (infiniteScroll)
        {
            HandleInfiniteScroll();
        }

        // Debug information
        if (debugMode && cameraMovement.magnitude > 0.001f)
        {
            Debug.Log($"Parallax '{gameObject.name}': Camera moved {cameraMovement.magnitude:F3}, Object moved {parallaxMovement.magnitude:F3}", this);
        }

        // Update camera position for next frame
        lastCameraPosition = cameraTransform.position;
    }

    private void HandleInfiniteScroll()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) 
        {
            if (debugMode)
                Debug.LogWarning($"Parallax: Cannot perform infinite scroll on '{gameObject.name}' - no valid sprite", this);
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 cameraPosition = cameraTransform.position;

        // Handle horizontal infinite scrolling
        if (moveHorizontal && spriteWidth > 0)
        {
            float distanceX = cameraPosition.x - currentPosition.x;
            if (Mathf.Abs(distanceX) >= spriteWidth)
            {
                float offset = Mathf.Sign(distanceX) * spriteWidth;
                currentPosition.x += offset;
                
                if (debugMode)
                    Debug.Log($"Parallax: Infinite scroll X for '{gameObject.name}' - Offset: {offset}", this);
            }
        }

        // Handle vertical infinite scrolling
        if (moveVertical && spriteHeight > 0)
        {
            float distanceY = cameraPosition.y - currentPosition.y;
            if (Mathf.Abs(distanceY) >= spriteHeight)
            {
                float offset = Mathf.Sign(distanceY) * spriteHeight;
                currentPosition.y += offset;
                
                if (debugMode)
                    Debug.Log($"Parallax: Infinite scroll Y for '{gameObject.name}' - Offset: {offset}", this);
            }
        }

        transform.position = currentPosition;
    }

    /// <param name="newSpeed">New speed value (will be clamped between 0 and 1)</param>
    public void SetParallaxSpeed(float newSpeed)
    {
        float oldSpeed = parallaxSpeed;
        parallaxSpeed = Mathf.Clamp01(newSpeed);
        
        if (debugMode)
            Debug.Log($"Parallax: Speed changed for '{gameObject.name}' from {oldSpeed:F2} to {parallaxSpeed:F2}", this);
    }

    public void ResetPosition()
    {
        Vector3 oldPosition = transform.position;
        transform.position = startPosition;
        
        if (cameraTransform != null)
            lastCameraPosition = cameraTransform.position;
            
        if (debugMode)
            Debug.Log($"Parallax: Position reset for '{gameObject.name}' from {oldPosition} to {startPosition}", this);
    }
    

    void OnValidate()
    {
        // Ensure parallax speed is within valid range
        parallaxSpeed = Mathf.Clamp01(parallaxSpeed);
    }
    
    void OnDrawGizmosSelected()
    {
        if (cameraTransform != null)
        {
            // Draw line to camera
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, cameraTransform.position);
            
            // Draw parallax speed indicator
            Gizmos.color = Color.Lerp(Color.red, Color.green, parallaxSpeed);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        
        // Draw sprite bounds for infinite scroll
        if (infiniteScroll && spriteRenderer != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(spriteWidth, spriteHeight, 0));
        }
    }
}
