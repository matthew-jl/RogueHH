using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Skill skill; 
    public Image skillIconImage;
    public TMP_Text skillDescriptionText; 
    public GameObject skillDescriptionModal;
    public GameObject overlayLocked; 
    public GameObject overlaySelected;

    private PlayerData playerData;

    void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
        }

        skillDescriptionModal.SetActive(false);
        overlaySelected.SetActive(false);

        if (skill != null)
        {
            skillIconImage.sprite = skill.skillIcon;
            skillIconImage.gameObject.SetActive(true);

            if (playerData.playerLevel < skill.unlockLevel)
            {
                overlayLocked.SetActive(true);
                skillDescriptionText.text = $"Unlocked at level {skill.unlockLevel}";
            }
            else
            {
                overlayLocked.SetActive(false);
                skillDescriptionText.text = $"{skill.skillName} - {skill.skillDescription}";
            }
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skill) skillDescriptionModal.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillDescriptionModal.SetActive(false);
    }
}
