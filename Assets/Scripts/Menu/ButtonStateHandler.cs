using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonStateHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;
    private Button button;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button.interactable && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
