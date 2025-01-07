using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FloorClearedMenu : MonoBehaviour
{
    public GameObject floorClearedModal;
    public Button continueButton;

    private PlayerData playerData;

    private void OnEnable()
    {
        FindObjectOfType<EnemyManager>().OnAllEnemiesDefeated.AddListener(HandleAllEnemiesDefeated);
    }

    private void OnDisable()
    {
        if (FindObjectOfType<EnemyManager>())
        {
            FindObjectOfType<EnemyManager>().OnAllEnemiesDefeated.RemoveListener(HandleAllEnemiesDefeated);
        }
    }

    void Start()
    {

        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
        }

        floorClearedModal.SetActive(false);

        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void ShowFloorCleared()
    {
        floorClearedModal.SetActive(true); 
    }

    void OnContinueClicked()
    {
        playerData.UnlockNextFloor();

        playerData.ResetCurrentHealth();

        SceneManager.LoadScene(1);
    }


    void HandleAllEnemiesDefeated(bool onAllEnemiesDefeated)
    {
        if (onAllEnemiesDefeated)
        {
            ShowFloorCleared();
        }
    }
}
