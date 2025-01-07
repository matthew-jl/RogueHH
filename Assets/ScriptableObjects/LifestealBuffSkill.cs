using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Lifesteal Buff Skill", menuName = "ScriptableObjects/Lifesteal Buff Skill")]
public class LifestealBuffSkill : Skill
{
    public float lifestealPercentage = 0.2f;
    public int buffDuration = 5;

    public UnityEvent<bool> OnLifestealActivated;

    public LifestealBuffSkill()
    {
        skillName = "Lifesteal";
        skillDescription = "For each successful hit, increase your health by 20% of your attack";
        skillType = SkillType.Buff;
        unlockLevel = 3;
        cooldownTime = 10;
    }

    public override void Activate()
    {
        remainingTurns = buffDuration;
        Debug.Log("Lifesteal Buff activated! Heal 20% of damage dealt each attack.");
        OnLifestealActivated.Invoke(true);
    }

    public override void Deactivate()
    {
        OnLifestealActivated.Invoke(false);
    }

    public void ApplyLifesteal(float damageDealt, PlayerData playerData)
    {
        if (remainingTurns > 0)
        {
            float healAmount = damageDealt * lifestealPercentage;
            playerData.currentHealth += Mathf.RoundToInt(healAmount);

            playerData.currentHealth = Mathf.Min(playerData.currentHealth, playerData.CalculateMaxHealth());

            Debug.Log($"Lifesteal applied: Heal {healAmount} (Remaining Turns: {remainingTurns})");
        }
        else
        {
            Debug.Log("Lifesteal Buff expired.");
        }
    }
}
