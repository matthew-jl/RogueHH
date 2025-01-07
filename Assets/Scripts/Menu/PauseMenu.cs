using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuModal;
    public UnityEvent<bool> OnPause;

    void Start()
    {
        pauseMenuModal.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        bool isPaused = pauseMenuModal.activeSelf;
        Debug.Log("active: " + isPaused);
        if (isPaused)
        {
            pauseMenuModal.SetActive(false);
            Time.timeScale = 1f; // resume the game

            OnPause?.Invoke(false);
        }
        else
        {
            pauseMenuModal.SetActive(true);
            Time.timeScale = 0f; // pause the game

            OnPause?.Invoke(true);
        }
    }

    public void OnResumeButtonPressed()
    {
        TogglePauseMenu();
    }

    public void OnBackToUpgradeButtonPressed()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void OnExitToMainMenuButtonPressed()
    {
        PlayerDataManager.Instance.SavePlayerData();

        SceneManager.LoadSceneAsync(0);
    }

}
