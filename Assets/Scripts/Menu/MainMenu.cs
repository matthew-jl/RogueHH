using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject newGameModal;
    public Button continueButton;
    private string saveFilePath;

    private void Start()
    {
        Time.timeScale = 1f;
        newGameModal.SetActive(false);
        saveFilePath = Application.persistentDataPath + "/playerdata.save";

        if (File.Exists(saveFilePath))
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    public void OnContinueButtonPressed()
    {
        if (File.Exists(saveFilePath))
        {
            PlayerDataManager.Instance.LoadPlayerData(); 
            SceneManager.LoadSceneAsync(2);
        }
    }

    public void OnNewGameButtonPressed()
    {
        if (File.Exists(saveFilePath))
        {
            newGameModal.SetActive(true);
        }
        else
        {
            SceneManager.LoadSceneAsync(2);
        }
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }

    public void OnModalContinueButtonPressed()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
        PlayerDataManager.Instance.playerData.ResetAll();
        PlayerDataManager.Instance.SavePlayerData();
        SceneManager.LoadSceneAsync(2);
    }

    public void OnModalBackButtonPressed()
    {
        newGameModal.SetActive(false);
    }
}
