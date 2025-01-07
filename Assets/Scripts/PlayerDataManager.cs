using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    public PlayerData playerData;

    private string saveFilePath;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Application.persistentDataPath + "/playerdata.save";
        LoadPlayerData();
    }

    public void SavePlayerData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream file = File.Create(saveFilePath))
        {
            PlayerDataSerializable dataToSave = new PlayerDataSerializable(playerData);
            formatter.Serialize(file, dataToSave);
        }
        Debug.Log("Player data saved to: " + saveFilePath);
    }

    public void LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream file = File.Open(saveFilePath, FileMode.Open))
            {
                PlayerDataSerializable loadedData = (PlayerDataSerializable)formatter.Deserialize(file);
                loadedData.ApplyTo(playerData);
            }
        }
    }
}

// Serializable class for saving/loading player data
[System.Serializable]
public class PlayerDataSerializable
{
    public int playerLevel;
    public int currentExperience;
    public int baseHealth;
    public int baseAttack;
    public int baseDefense;
    public float baseCritRate;
    public float baseCritDamage;

    public int healthLevel;
    public int attackLevel;
    public int defenseLevel;
    public int critRateLevel;
    public int critDamageLevel;

    public int currentZhen;

    public int maxFloorUnlocked;
    public int currentFloor;

    // Constructor to create a serializable version of PlayerData
    public PlayerDataSerializable(PlayerData data)
    {
        playerLevel = data.playerLevel;
        currentExperience = data.currentExperience;
        baseHealth = data.baseHealth;
        baseAttack = data.baseAttack;
        baseDefense = data.baseDefense;
        baseCritRate = data.baseCritRate;
        baseCritDamage = data.baseCritDamage;

        healthLevel = data.healthLevel;
        attackLevel = data.attackLevel;
        defenseLevel = data.defenseLevel;
        critRateLevel = data.critRateLevel;
        critDamageLevel = data.critDamageLevel;

        currentZhen = data.currentZhen;

        maxFloorUnlocked = data.maxFloorUnlocked;
        currentFloor = data.currentFloor;
    }

    // Method to apply loaded data back to the PlayerData ScriptableObject
    public void ApplyTo(PlayerData data)
    {
        data.playerLevel = playerLevel;
        data.currentExperience = currentExperience;
        data.baseHealth = baseHealth;
        data.baseAttack = baseAttack;
        data.baseDefense = baseDefense;
        data.baseCritRate = baseCritRate;
        data.baseCritDamage = baseCritDamage;

        data.healthLevel = healthLevel;
        data.attackLevel = attackLevel;
        data.defenseLevel = defenseLevel;
        data.critRateLevel = critRateLevel;
        data.critDamageLevel = critDamageLevel;

        data.currentZhen = currentZhen;

        data.maxFloorUnlocked = maxFloorUnlocked;
        data.currentFloor = currentFloor;

        data.currentHealth = data.CalculateMaxHealth();
    }
}
