using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameplayUIUpdater : MonoBehaviour
{
    public TMP_Text zhenText;
    public TMP_Text floorIndicatorText;
    public TMP_Text enemyCountText;
    public TMP_Text playerLevelText;
    public Slider playerHPBar;
    public Slider expBar;
    public TMP_Text hpBarText;
    public TMP_Text expBarText;

    private PlayerData playerData;
    private EnemyManager enemyManager;

    void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerDataManager instance not found!");
            return;
        }

        enemyManager = FindObjectOfType<EnemyManager>();

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (playerData != null)
        {
            zhenText.text = playerData.currentZhen.ToString();

            floorIndicatorText.text = "Floor: " + playerData.currentFloor;

            playerLevelText.text = "Level " + playerData.playerLevel;

            int maxHealth = playerData.CalculateMaxHealth();
            playerHPBar.maxValue = maxHealth;
            playerHPBar.value = playerData.currentHealth;
            hpBarText.text = $"{playerData.currentHealth}/{maxHealth}";

            int experienceToNextLevel = playerData.ExperienceToNextLevel();
            expBar.maxValue = experienceToNextLevel;
            expBar.value = playerData.currentExperience;
            expBarText.text = $"{playerData.currentExperience}/{experienceToNextLevel}";
        }

        if (enemyManager != null)
        {
            enemyCountText.text = "Enemy left: " + enemyManager.spawnedEnemies.Count;
        }
    }
}
