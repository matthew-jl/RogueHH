using UnityEngine;

public class MainMenuCameraPan : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float maxRotationAngle = 30f;
    public AudioSource backgroundMusic;

    private float currentAngle = 0f;
    private bool rotatingRight = true;

    void Update()
    {
        float rotationAmount = rotationSpeed * Time.deltaTime;

        if (rotatingRight)
        {
            currentAngle += rotationAmount;
            if (currentAngle >= maxRotationAngle)
            {
                rotatingRight = false;
            }
        }
        else
        {
            currentAngle -= rotationAmount;
            if (currentAngle <= -maxRotationAngle)
            {
                rotatingRight = true;
            }
        }

        transform.Rotate(0, rotatingRight ? rotationAmount : -rotationAmount, 0);
    }
}
