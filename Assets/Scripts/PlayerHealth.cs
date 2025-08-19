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
        //CheckForDeadlyObstacles();
    }
    

    void TakeDamage(int damage)
    {
        currentHealth -= damage;

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

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Spikes"))
        {
            Debug.Log("Hit spikes via trigger!");
            TakeDamage(1);
        }
    }
}
