using System;
using UnityEngine;

public static class SensorEvents
{
    [Serializable]
    public struct GyroscopeData
    {
        public float qx, qy, qz, qw;
        public long  timestamp;
    }

    [Serializable]
    public struct JoystickData
    {
        public float horizontal;
        public float vertical;
    }

    public static event Action<GyroscopeData> OnGyroscopeDataReceived;
    public static event Action<Vector3>       OnAccelerationReceived;
    public static event Action<JoystickData>  OnJoystickReceived;
    public static event Action                OnGrabPressed;
    public static event Action                OnGrabReleased;

    public static void RaiseGyroscopeDataReceived(GyroscopeData data) => OnGyroscopeDataReceived?.Invoke(data);
    public static void RaiseAccelerationReceived(Vector3 accel)       => OnAccelerationReceived?.Invoke(accel);
    public static void RaiseJoystickReceived(JoystickData data)       => OnJoystickReceived?.Invoke(data);
    public static void RaiseGrabPressed()                             => OnGrabPressed?.Invoke();
    public static void RaiseGrabReleased()                            => OnGrabReleased?.Invoke();
}
