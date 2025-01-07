using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    public GameObject gameOverModal;
    public Button continueButton;

    void OnEnable()
    {
        // subscribe to onPause event
        FindObjectOfType<PlayerController>().OnPlayerDeath.AddListener(HandlePlayerDeath);
    }

    void OnDisable()
    {
        if (FindObjectOfType<PlayerController>())
        {
            // unsub from onPause
            FindObjectOfType<PlayerController>().OnPlayerDeath.RemoveListener(HandlePlayerDeath);
        }
    }

    void Start()
    {
        gameOverModal.SetActive(false);

        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void ShowGameOver()
    {
        gameOverModal.SetActive(true);
    }

    void OnContinueClicked()
    {
        SceneManager.LoadScene(2);
    }

    void HandlePlayerDeath(bool isPlayerDead)
    {
        if (isPlayerDead)
        {
            ShowGameOver();
        }
    }
}
