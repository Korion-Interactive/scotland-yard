﻿using UnityEngine;

/// <summary>
/// Adds parallax effect by using device orientation.
/// </summary>
public class TrueParallax : MonoBehaviour
{
    /// <summary>
    /// Speed of the object.
    /// </summary>
    /// <remarks>When speed increase, object appears to be closer to viewer.</remarks>
    public float Speed;

    /// <summary>
    /// Max offset relative to the current position.
    /// </summary>
    public Vector3 MaxOffset = new Vector3(0.5f, 0.5f, 0);

    /// <summary>
    /// Whether gyro is enabled.
    /// </summary>
    public bool IsGyroEnabled { get; private set; }

    /// <summary>
    /// Use gyro if available.
    /// </summary>
    public static bool UseGyroscope = true;

    /// <summary>
    /// Value to smooth a gyro/accelerometer data.
    /// </summary>
    private const float _lerpFactor = 0.4f;

    private Vector3 _lastAcceleration;

    void Start()
    {
        if (UseGyroscope && SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            IsGyroEnabled = true;
        }
        
        _lastAcceleration = IsGyroEnabled ? Input.gyro.gravity : Input.acceleration;
    }

    void Update()
    {
        var acceleration = IsGyroEnabled ? Input.gyro.gravity : Input.acceleration;

        // Get new objects position in accordance with the sensor.
        var offset = new Vector3(Speed * (_lastAcceleration.x - acceleration.x), Speed * (_lastAcceleration.y - acceleration.y), 0);

        _lastAcceleration = acceleration;

        var tempPosition = transform.position + offset;
        var maxPosition = transform.position + MaxOffset;
        var minPosition = transform.position - MaxOffset;

        var newPosition = new Vector3(Mathf.Clamp(tempPosition.x, minPosition.x, maxPosition.x),
                                      Mathf.Clamp(tempPosition.y, minPosition.y, maxPosition.y), 
                                      tempPosition.z);

        // Flatten the value obtained with the previous position and set the object to a new position.
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, newPosition, _lerpFactor);
    }

#if !(UNITY_ANDROID || UNITY_IPHONE)
    void OnGUI()
    {
        //GUI.Label(new Rect(0, 0, 300, 20), "TrueParallax only works on iOS and Android platforms.");
    }

#endif

}
