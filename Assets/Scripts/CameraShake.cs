using UnityEngine;
using System.Collections;

/// <summary>
/// The Script, shaking the camera if the shake amount is set to anything above 0
/// </summary>

public class CameraShake : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform if null
    public Transform camTransform;

    // How long the camera should shake for.
    public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    Vector3 originalPos;

    void Awake()
    {
        // grabs the camera transform
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void OnEnable()
    {
        //grabs the original position of the camera
        originalPos = camTransform.localPosition;
    }

    void Update()
    {
        //offsets the camera in a certain spherical range
        if (shakeDuration > 0)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            //resets the camera to the original position
            shakeDuration = 0f;
            camTransform.localPosition = originalPos;
        }
    }
}