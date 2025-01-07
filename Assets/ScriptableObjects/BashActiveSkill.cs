using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Bash Active Skill", menuName = "ScriptableObjects/Bash Active Skill")]
public class BashActiveSkill : Skill
{
    public float damageMultiplier = 1.5f;

    public UnityEvent<bool> OnBashActivated;

    public bool isActive;

    public BashActiveSkill()
    {
        skillName = "Bash";
        skillDescription = "Deal 150% damage on the next physical attack.";
        skillType = SkillType.Active;
        unlockLevel = 4;
        cooldownTime = 5;
        isActive = false;
    }

    public override void Activate()
    {
        isActive = true;
        Debug.Log("Bash Activated! Next attack will deal 150% damage.");
        OnBashActivated.Invoke(true);
    }

    public override void Deactivate()
    {
        isActive = false;
        OnBashActivated.Invoke(false);
    }
}
