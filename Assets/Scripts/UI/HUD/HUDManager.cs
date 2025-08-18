using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI dashText;
    public Slider dashCooldownSlider;
    CharacterController2D characterController;
    public GameObject dieScreen;
    public TextMeshProUGUI gameOverText;

    private void Start()
    {
        characterController = GameManager.Instance.playerPrefab.GetComponent<CharacterController2D>();
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        dashText.text = $"{characterController.remainingDashes}/{characterController.characterProfile.maxAllowedDashes}";

        // Update dash cooldown slider
        if (characterController.isDashOnCooldown)
        {
            dashCooldownSlider.value = characterController.dashCooldownTimer / characterController.characterProfile.dashingCooldown;
        }
        else
        {
            // Show full bar when not on cooldown
            dashCooldownSlider.value = 0f;
        }
    }

    public void UpdateDashCooldown(float normalizedValue)
    {
        dashCooldownSlider.value = normalizedValue;
    }

    public void ShowGameOverScreen()
    {
        dieScreen.SetActive(true);
    }

    public void ShowGameWinScreen()
    {
        gameOverText.text = "You reached the end!";
        dieScreen.SetActive(true);
    }
}