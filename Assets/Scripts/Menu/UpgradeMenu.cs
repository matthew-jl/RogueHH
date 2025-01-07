using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UpgradeMenu : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioSource upgradeSoundEffect;
    public Button exitButton;

    private PlayerData playerData;

    public TMP_Text healthLevelText;
    public TMP_Text attackLevelText;
    public TMP_Text defenseLevelText;
    public TMP_Text critRateLevelText;
    public TMP_Text critDamageLevelText;
    public TMP_Text zhenText;

    public Button healthButton;
    public Button attackButton;
    public Button defenseButton;
    public Button critRateButton;
    public Button critDamageButton;
    public GameObject upgradeDetailsPanel;

    public Image statImage;
    public TMP_Text statNameText;
    public TMP_Text statDescriptionText;
    public TMP_Text statCurrentValueText;
    public TMP_Text statUpgradeValueText;
    public TMP_Text statUpgradeCostText;
    public TMP_Text errorMessageText;
    public Button upgradeButton;

    public Sprite healthSprite;
    public Sprite attackSprite;
    public Sprite defenseSprite;
    public Sprite critRateSprite;
    public Sprite critDamageSprite;

    public TMP_Dropdown floorDropdown;

    void Start()
    {
        Time.timeScale = 1f;
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
            playerData.ResetCurrentHealth();
            LoadUpgradeMenuData();
            InitializeFloorDropdown();
        }
        else
        {
            Debug.LogError("PlayerDataManager instance not found!");
        }

        upgradeDetailsPanel.SetActive(false);
        errorMessageText.gameObject.SetActive(false);

        healthButton.onClick.AddListener(() => OnStatButtonClicked("Health"));
        attackButton.onClick.AddListener(() => OnStatButtonClicked("Attack"));
        defenseButton.onClick.AddListener(() => OnStatButtonClicked("Defense"));
        critRateButton.onClick.AddListener(() => OnStatButtonClicked("CritRate"));
        critDamageButton.onClick.AddListener(() => OnStatButtonClicked("CritDamage"));
    }

    private void LoadUpgradeMenuData()
    {
        healthLevelText.text = $"Lvl:{playerData.healthLevel}/{playerData.maxUpgradeLevel}";
        attackLevelText.text = $"Lvl:{playerData.attackLevel}/{playerData.maxUpgradeLevel}";
        defenseLevelText.text = $"Lvl:{playerData.defenseLevel}/{playerData.maxUpgradeLevel}";
        critRateLevelText.text = $"Lvl:{playerData.critRateLevel}/{playerData.maxUpgradeLevel}";
        critDamageLevelText.text = $"Lvl:{playerData.critDamageLevel}/{playerData.maxUpgradeLevel}";
        zhenText.text = $"{playerData.currentZhen}";
    }

    public void OnExitButtonPressed()
    {
        // save player data before exiting
        PlayerDataManager.Instance.SavePlayerData();

        SceneManager.LoadSceneAsync(0);
    }

    private void OnStatButtonClicked(string stat)
    {
        errorMessageText.gameObject.SetActive(false);

        upgradeDetailsPanel.SetActive(true);

        // update details panel based on stat
        switch (stat)
        {
            case "Health":
                statImage.sprite = healthSprite;
                statNameText.text = "Health Up";
                statDescriptionText.text = "Improve your maximum health. A better heart means better health.";
                statCurrentValueText.text = $"Current: {playerData.baseHealth + (playerData.healthLevel * playerData.healthUpgradeValue)} hp";
                statUpgradeValueText.text = $"Upgrade: +{playerData.healthUpgradeValue} hp";
                statUpgradeCostText.text = $"{GetUpgradeCost("Health")} To upgrade";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => UpgradeStat("Health"));
                break;

            case "Attack":
                statImage.sprite = attackSprite;
                statNameText.text = "Attack Up";
                statDescriptionText.text = "A proper muscle training will allow you to really strengthen your muscles.";
                statCurrentValueText.text = $"Current: {playerData.baseAttack + (playerData.attackLevel * playerData.attackUpgradeValue)} atk";
                statUpgradeValueText.text = $"Upgrade: +{playerData.attackUpgradeValue} atk";
                statUpgradeCostText.text = $"{GetUpgradeCost("Attack")} To upgrade";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => UpgradeStat("Attack"));
                break;

            case "Defense":
                statImage.sprite = defenseSprite;
                statNameText.text = "Defense Up";
                statDescriptionText.text = "Resistance training will improve your toughness. Tougher body means more TPA work :D";
                statCurrentValueText.text = $"Current: {playerData.baseDefense + (playerData.defenseLevel * playerData.defenseUpgradeValue)} def";
                statUpgradeValueText.text = $"Upgrade: +{playerData.defenseUpgradeValue} def";
                statUpgradeCostText.text = $"{GetUpgradeCost("Defense")} To upgrade";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => UpgradeStat("Defense"));
                break;

            case "CritRate":
                statImage.sprite = critRateSprite;
                statNameText.text = "Luck Up";
                statDescriptionText.text = "Increases your chance to strike a critical hit.";
                statCurrentValueText.text = $"Current: {playerData.baseCritRate + (playerData.critRateLevel * playerData.critRateUpgradeValue)}% rate";
                statUpgradeValueText.text = $"Upgrade: +{playerData.critRateUpgradeValue}% rate";
                statUpgradeCostText.text = $"{GetUpgradeCost("CritRate")} To upgrade";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => UpgradeStat("CritRate"));
                break;

            case "CritDamage":
                statImage.sprite = critDamageSprite;
                statNameText.text = "Crit Dmg Up";
                statDescriptionText.text = "Boosts your critical damage to make those lucky hits hurt even more.";
                statCurrentValueText.text = $"Current: {playerData.baseCritDamage + (playerData.critDamageLevel * playerData.critDamageUpgradeValue)}% dmg";
                statUpgradeValueText.text = $"Upgrade: +{playerData.critDamageUpgradeValue}% dmg";
                statUpgradeCostText.text = $"{GetUpgradeCost("CritDamage")} To upgrade";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => UpgradeStat("CritDamage"));
                break;
        }
    }

    private int GetUpgradeCost(string itemName)
    {
        int baseCost = 10;
        int level;

        switch (itemName)
        {
            case "Health":
                level = playerData.healthLevel;
                break;
            case "Attack":
                level = playerData.attackLevel;
                break;
            case "Defense":
                level = playerData.defenseLevel;
                break;
            case "CritRate":
                level = playerData.critRateLevel;
                break;
            case "CritDamage":
                level = playerData.critDamageLevel;
                break;
            default:
                level = 0;
                break;
        }

        int totalUpgrades = playerData.healthLevel + playerData.attackLevel + playerData.defenseLevel + playerData.critRateLevel + playerData.critDamageLevel;
        int otherStatsUpgraded = totalUpgrades - level;

        int cost = baseCost + (level * 50) + (otherStatsUpgraded * 10);
        return cost;
    }

    private void UpgradeStat(string stat)
    {
        int cost = GetUpgradeCost(stat);

        if (playerData.SpendZhen(cost))
        {
            switch (stat)
            {
                case "Health":
                    playerData.healthLevel++;
                    playerData.currentHealth = playerData.CalculateMaxHealth();
                    break;
                case "Attack":
                    playerData.attackLevel++;
                    break;
                case "Defense":
                    playerData.defenseLevel++;
                    break;
                case "CritRate":
                    playerData.critRateLevel++;
                    break;
                case "CritDamage":
                    playerData.critDamageLevel++;
                    break;
            }

            if (upgradeSoundEffect != null)
            {
                upgradeSoundEffect.Play();
            }

            // reload UI
            LoadUpgradeMenuData();
            OnStatButtonClicked(stat);
        }
        else
        {
            errorMessageText.gameObject.SetActive(true);
            Debug.Log("Not enough Zhen to upgrade.");
        }
    }

    // to be called by CheatCodeHandler
    public void UpdateZhenText()
    {
        zhenText.text = $"{playerData.currentZhen}";
    }

    public void OnStartGameButtonClicked()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void InitializeFloorDropdown()
    {
        floorDropdown.onValueChanged.AddListener(OnFloorSelectionChanged);

        floorDropdown.ClearOptions();

        List<string> options = new List<string>
        {
            "Boss"
        };

        for (int i = 1; i <= playerData.maxFloorUnlocked; i++)
        {
            options.Add("Floor " + i);
        }

        floorDropdown.AddOptions(options);

        // set the default selected option to the last unlocked floor
        floorDropdown.value = options.Count - 1;
    }

    public void OnFloorSelectionChanged(int value)
    {
        string selectedFloor = floorDropdown.options[value].text;

        if (selectedFloor == "Boss")
        {
            playerData.currentFloor = 0;
        }
        else
        {
            int floorNumber = int.Parse(selectedFloor.Replace("Floor ", ""));
            playerData.currentFloor = floorNumber;
        }

        Debug.Log($"Current Floor set to: {playerData.currentFloor}");
    }

}
