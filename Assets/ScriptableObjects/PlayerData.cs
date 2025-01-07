using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public int playerLevel = 1;
    public int currentExperience = 0;
    public int baseHealth = 20;
    public int baseAttack = 5;
    public int baseDefense = 5;
    public float baseCritRate = 5.0f; // percentage
    public float baseCritDamage = 150.0f; // percentage

    public int currentHealth;

    public int healthLevel = 0;
    public int attackLevel = 0;
    public int defenseLevel = 0;
    public int critRateLevel = 0;
    public int critDamageLevel = 0;
    public int maxUpgradeLevel = 45;

    public int healthUpgradeValue = 10;
    public int attackUpgradeValue = 2;
    public int defenseUpgradeValue = 5;
    public float critRateUpgradeValue = 2.0f; // Percentage
    public float critDamageUpgradeValue = 5.0f; // Percentage

    public int currentZhen = 0;

    public int maxFloorUnlocked = 1;
    public int currentFloor;
    public int maxFloors = 101;

    public UnityEvent OnLevelUp = new UnityEvent();

    // for attribute scaling based on player level
    public float healthScalingFactor = 2.0f; 
    public float attackScalingFactor = 1.5f;
    public float defenseScalingFactor = 1.2f;
    public float critRateScalingFactor = 0.5f;
    public float critDamageScalingFactor = 5.0f;

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        if (currentExperience >= ExperienceToNextLevel())
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        playerLevel++;
        currentExperience = 0;
        OnLevelUp.Invoke();
        // increase player stats on level up
        currentHealth = CalculateMaxHealth();
    }

    public int ExperienceToNextLevel()
    {
        return playerLevel * 100;
    }

    public void AddZhen(int amount)
    {
        currentZhen += amount;
    }

    public bool SpendZhen(int amount)
    {
        if (currentZhen >= amount)
        {
            currentZhen -= amount;
            return true;
        }
        return false;
    }

    public void UnlockNextFloor()
    {
        if (currentFloor < maxFloors)
        {
            currentFloor++;
            if (currentFloor > maxFloorUnlocked)
            {
                maxFloorUnlocked = currentFloor;
            }
        }
    }

    public void ResetFloorProgression()
    {
        currentFloor = 1;
    }

    public void ResetAll()
    {
        playerLevel = 1;
        currentExperience = 0;
        baseHealth = 20;
        baseAttack = 5;
        baseDefense = 5;
        baseCritRate = 5.0f;
        baseCritDamage = 150.0f;

        healthLevel = 0;
        attackLevel = 0;
        defenseLevel = 0;
        critRateLevel = 0;
        critDamageLevel = 0;

        currentZhen = 0;

        maxFloorUnlocked = 1;
        currentFloor = 1;

        currentHealth = CalculateMaxHealth();
    }

    public void ResetCurrentHealth()
    {
        currentHealth = CalculateMaxHealth();
    }

    public int CalculateMaxHealth()
    {
        return Mathf.RoundToInt(baseHealth + (playerLevel - 1) * healthScalingFactor + (healthLevel * healthUpgradeValue));
    }

    public int CalculateCurrentAttack()
    {
        return Mathf.RoundToInt(baseAttack + (playerLevel - 1) * attackScalingFactor + (attackLevel * attackUpgradeValue));
    }

    public int CalculateCurrentDefense()
    {
        return Mathf.RoundToInt(baseDefense + (playerLevel - 1) * defenseScalingFactor + (defenseLevel * defenseUpgradeValue));
    }

    public float CalculateCurrentCritRate()
    {
        return baseCritRate + (playerLevel - 1) * critRateScalingFactor + (critRateLevel * critRateUpgradeValue);
    }

    public float CalculateCurrentCritDamage()
    {
        return baseCritDamage + (playerLevel - 1) * critDamageScalingFactor + (critDamageLevel * critDamageUpgradeValue);
    }

    public void ApplyDamage(int damage)
    {
        currentHealth -= Mathf.Max(0, Mathf.RoundToInt(damage));
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"Player took {damage} damage.");
    }
}
