using UnityEngine;
using TMPro;

public class CheatCodeHandler : MonoBehaviour
{
    public TMP_InputField cheatCodeInputField;
    public AudioSource cheatCodeActivatedSound;

    private PlayerData playerData;
    private UpgradeMenu upgradeMenu;

    private void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;

            upgradeMenu = FindObjectOfType<UpgradeMenu>();
            if (upgradeMenu == null)
            {
                Debug.LogError("UpgradeMenu instance not found!");
            }
        }
        else
        {
            Debug.LogError("PlayerDataManager instance not found!");
        }
    }

    public void OnCheatCodeEntered()
    {
        string cheatCode = cheatCodeInputField.text.ToLower();

        switch (cheatCode)
        {
            case "hesoyam":
                playerData.AddExperience(500);
                break;
            case "tpagamegampang":
                playerData.AddZhen(1000);
                if (upgradeMenu != null)
                {
                    upgradeMenu.UpdateZhenText();
                }
                break;
            case "opensesame":
                playerData.maxFloorUnlocked = 100;
                if (upgradeMenu != null)
                {
                    upgradeMenu.InitializeFloorDropdown();
                }
                break;
            default:
                Debug.Log("Invalid Cheat Code");
                return;
        }

        if (cheatCodeActivatedSound != null)
        {
            cheatCodeActivatedSound.Play();
        }

        cheatCodeInputField.text = "";
    }
}
