using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    public LayerMask deadlyObstacleLayer;
    public float obstacleCheckDistance = 0.1f;
    
    private CharacterController2D characterController;
    private CharacterRaycaster2D raycaster;
    private HUDManager hudManager;

    void Start()
    {
        currentHealth = maxHealth;
        
        characterController = GetComponent<CharacterController2D>();
        if (characterController != null)
        {
            raycaster = characterController.raycaster;
        }
        
        if (raycaster == null)
        {
            Debug.LogError("CharacterRaycaster2D not found! Make sure CharacterController2D is attached and has a raycaster reference.");
        }
        
        // Find HUD Manager reference
        if (GameManager.Instance != null && GameManager.Instance.canvas != null)
        {
            hudManager = GameManager.Instance.canvas.GetComponentInChildren<HUDManager>();
        }
        
        // Update HUD with initial health
        UpdateHealthUI();
    }

    void Update()
    {
        CheckForDeadlyObstacles();
    }

    void CheckForDeadlyObstacles()
    {
        if (raycaster == null) return;
        
        MovementDirection[] directions = { MovementDirection.Right, MovementDirection.Left, MovementDirection.Above, MovementDirection.Below };
        
        foreach (MovementDirection direction in directions)
        {
            if (CheckDeadlyObstacleInDirection(direction))
            {
                TakeDamage(1); // Instant kill
                return; // Exit early to avoid multiple kills
            }
        }
    }

    bool CheckDeadlyObstacleInDirection(MovementDirection direction)
    {
        if (raycaster == null) return false;
        
        // Get direction vector and corner positions like in CharacterRaycaster2D
        Vector2 directionVector = GetDirectionVector(direction);
        
        Vector2 cornerA = Vector2.zero;
        Vector2 cornerB = Vector2.zero;
        
        // Get box collider bounds (similar to CharacterRaycaster2D logic)
        BoxCollider2D selfBox = raycaster.selfBox;
        Transform self = raycaster.self;
        
        if (direction == MovementDirection.Below)
        {
            cornerA = GetPointPositionInBox(-1, -1, selfBox, self);
            cornerB = GetPointPositionInBox(1, -1, selfBox, self);
            cornerA.x += raycaster.skinWidth;
            cornerB.x -= raycaster.skinWidth;
        }
        else if (direction == MovementDirection.Above)
        {
            cornerA = GetPointPositionInBox(-1, 1, selfBox, self);
            cornerB = GetPointPositionInBox(1, 1, selfBox, self);
            cornerA.x += raycaster.skinWidth;
            cornerB.x -= raycaster.skinWidth;
        }
        else if (direction == MovementDirection.Left)
        {
            cornerA = GetPointPositionInBox(-1, -1, selfBox, self);
            cornerB = GetPointPositionInBox(-1, 1, selfBox, self);
            cornerA.y += raycaster.skinWidth;
            cornerB.y -= raycaster.skinWidth;
        }
        else if (direction == MovementDirection.Right)
        {
            cornerA = GetPointPositionInBox(1, -1, selfBox, self);
            cornerB = GetPointPositionInBox(1, 1, selfBox, self);
            cornerA.y += raycaster.skinWidth;
            cornerB.y -= raycaster.skinWidth;
        }
        
        // Cast multiple rays and check for "Spikes" tag
        for (int i = 0; i < raycaster.accuracy; i++)
        {
            float ratio = ((float)i) / (float)(raycaster.accuracy - 1);
            Vector2 origin = Vector2.Lerp(cornerA, cornerB, ratio);
            origin += directionVector * raycaster.skinWidth;
            
            RaycastHit2D hitResult = Physics2D.Raycast(origin, directionVector, obstacleCheckDistance, deadlyObstacleLayer);
            
            if (hitResult.collider != null && hitResult.collider.CompareTag("Spikes"))
            {
                Debug.Log($"Spikes detected at {hitResult.point} from direction {direction}");
                return true;
            }
        }
        
        return false;
    }
    
    // Helper method to get point position in box (copied from CharacterRaycaster2D logic)
    Vector2 GetPointPositionInBox(float x, float y, BoxCollider2D selfBox, Transform self)
    {
        Vector2 result = self.position;

        result.x += selfBox.offset.x * self.lossyScale.x;
        result.y += selfBox.offset.y * self.lossyScale.y;

        result.x += x * selfBox.size.x * 0.5f * self.lossyScale.x;
        result.y += y * selfBox.size.y * 0.5f * self.lossyScale.y;

        return result;
    }

    Vector2 GetDirectionVector(MovementDirection dir)
    {
        switch (dir)
        {
            case MovementDirection.Right: return Vector2.right;
            case MovementDirection.Above: return Vector2.up;
            case MovementDirection.Left: return Vector2.left;
            case MovementDirection.Below: return Vector2.down;
            default: return Vector2.zero;
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        
        // Update UI
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            RespawnPlayer();
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        currentHealth = 0;
        
        // Update UI
        UpdateHealthUI();
        
        // Show game over screen
        if (hudManager != null)
        {
            GameManager.Instance.EndGame();
        }
        
        // Reset health for respawn
        currentHealth = maxHealth;
        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        transform.position = new Vector3(Camera.main.transform.position.x - 10, Camera.main.transform.position.y, transform.position.z);
        
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (hudManager != null && hudManager.healthText != null)
        {
            hudManager.healthText.text = currentHealth.ToString();
        }
    }

    public void DamagePlayer(int damage = 1)
    {
        TakeDamage(damage);
    }

    public void HealPlayer(int healAmount = 1)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Cap at max health
        
        Debug.Log($"Player healed {healAmount}. Current health: {currentHealth}/{maxHealth}");
        UpdateHealthUI();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player collided with: " + collision.gameObject.name);

        // Check if object has "Spikes" tag and is on damage layer
        if (collision.gameObject.CompareTag("Spikes") && ((1 << collision.gameObject.layer) & deadlyObstacleLayer) != 0)
        {
            Debug.Log("Hit spikes via collision!");
            TakeDamage(1);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player triggered: " + other.gameObject.name);

        // Check if object has "Spikes" tag and is on damage layer
        if (other.CompareTag("Spikes") && ((1 << other.gameObject.layer) & deadlyObstacleLayer) != 0)
        {
            Debug.Log("Hit spikes via trigger!");
            TakeDamage(1);
        }
    }
}
