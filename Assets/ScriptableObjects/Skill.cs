using UnityEngine;

public enum SkillType
{
    Active,
    Buff
}

[CreateAssetMenu(fileName = "New Skill", menuName = "ScriptableObjects/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public Sprite skillIcon;
    public int unlockLevel; 
    public int cooldownTime;
    public int remainingTurns;

    public SkillType skillType;

    public bool isUnlocked = false;

    public virtual void Activate()
    {
        
    }

    public virtual void Deactivate()
    {

    }

    public bool IsBuffActive()
    {
        return remainingTurns > 0; // check if the buff is still active
    }

    public void DecreaseRemainingTurnsByOne()
    {
        if (remainingTurns > 0)
        {
            remainingTurns -= 1;
        }
    }
}
