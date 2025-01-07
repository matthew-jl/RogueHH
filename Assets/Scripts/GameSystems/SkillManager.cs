using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public SkillContainer[] skillSlots = new SkillContainer[9]; 

    public GameObject remainingTurnsUI;  
    public TMP_Text remainingTurnsText; 
    public Image skillIconImage;         

    private PlayerData playerData;       
    private PlayerController player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        playerData = PlayerDataManager.Instance.playerData;

        remainingTurnsUI.SetActive(false);
    }

    private void Update()
    {
        // listen for skill input (1-9 keys)
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ActivateSkill(i);
            }
        }

        UpdateRemainingTurnsUI(skillSlots[0].skill);
    }

    public void ActivateSkill(int slotIndex)
    {
        SkillContainer selectedSkillContainer = skillSlots[slotIndex];

        if (selectedSkillContainer.skill != null)
        {
            Skill skill = selectedSkillContainer.skill;

            if (playerData.playerLevel >= skill.unlockLevel)
            {

                if (skill.skillType == SkillType.Buff)
                {
                    skill.Activate();
                    ShowRemainingTurnsUI(skill);
                }
                else if (skill.skillType == SkillType.Active)
                {
                    if (skill.skillName == "Bash")
                    {
                        if (((BashActiveSkill) skill).isActive)
                        {
                            skill.Deactivate();
                            HideSkillSelectionUI(selectedSkillContainer);
                            return;
                        }
                    }
                    skill.Activate();
                    ShowSkillSelectionUI(selectedSkillContainer);
                }
    
            }
        }
    }

    void ShowRemainingTurnsUI(Skill skill)
    {
        remainingTurnsUI.SetActive(true);
        remainingTurnsText.text = $"{skill.remainingTurns}";
        skillIconImage.sprite = skill.skillIcon;
    }

    void UpdateRemainingTurnsUI(Skill skill)
    {
        if (skill.remainingTurns <= 0)
        {
            skill.Deactivate();
            HideRemainingTurnsUI(skill);
            return;
        }
        remainingTurnsText.text = $"{skill.remainingTurns}";
    }

    void HideRemainingTurnsUI(Skill skill)
    {
        remainingTurnsUI.SetActive(false);
    }

    void ShowSkillSelectionUI(SkillContainer skillContainer)
    {
        skillContainer.overlaySelected.SetActive(true);  // Show the selected skill UI
    }

    void HideSkillSelectionUI(SkillContainer skillContainer)
    {
        skillContainer.overlaySelected.SetActive(false);
    }
}
