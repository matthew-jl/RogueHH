using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StatsButtonStateHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Image overlayImage;
    private Color defaultColor = new Color(1f, 1f, 1f, 0f); 
    private Color hoverColor = new Color(1f, 1f, 1f, 0.2f);
    private Color clickColor = new Color(0f, 0f, 0f, 0.3f);

    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    private void Start()
    {
        // get the child overlay image component
        overlayImage = transform.Find("Overlay").GetComponent<Image>();
        if (overlayImage != null)
        {
            overlayImage.color = defaultColor;
        }
        else
        {
            Debug.LogWarning("No overlay image found as child of the button.");
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }

        if (overlayImage != null)
        {
            overlayImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (overlayImage != null)
        {
            overlayImage.color = defaultColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        if (overlayImage != null)
        {
            overlayImage.color = clickColor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (overlayImage != null)
        {
            overlayImage.color = hoverColor;
        }
    }
}
