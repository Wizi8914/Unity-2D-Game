using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject playerPrefab;
    public Canvas canvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize game state
    }

    private void Update()
    {
        // Handle game logic
    }

    public void EndGame()
    {
        canvas.GetComponent<HUDManager>().ShowGameOverScreen();
        playerPrefab.GetComponent<PlayerInput>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void GameWin()
    {
        StartCoroutine(winCinematic());
    }

    private IEnumerator winCinematic()
    {
        // Play win animation or cutscene
        yield return new WaitForSeconds(5f);
        canvas.GetComponent<HUDManager>().ShowGameWinScreen();
        playerPrefab.GetComponent<PlayerInput>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }
}