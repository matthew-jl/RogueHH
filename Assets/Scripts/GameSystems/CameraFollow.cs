using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; 
    public float followSpeed = 10f;
    public Vector3 offset;

    public AudioSource backgroundMusic;

    private float shakeMagnitude = 0.2f; 
    private float shakeDuration = 0.2f;
    private Vector3 originalPosition;
    private bool isShaking = false;

    void Start()
    {
        // set the initial offset based on the camera's starting position
        offset = transform.position - player.position;
    }

    void Update()
    {
        if (isShaking)
        {
            Vector3 shakeOffset = new Vector3(Random.Range(-shakeMagnitude, shakeMagnitude), Random.Range(-shakeMagnitude, shakeMagnitude), 0);
            transform.position = originalPosition + shakeOffset;
        } 
        else
        {
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }

    public void TriggerScreenShake()
    {
        if (!isShaking)
        {
            StartCoroutine(ScreenShake());
        }
    }

    private IEnumerator ScreenShake()
    {
        yield return new WaitForSeconds(0.2f);

        isShaking = true;
        float elapsed = 0f;

        originalPosition = transform.position;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        isShaking = false;
    }
}
