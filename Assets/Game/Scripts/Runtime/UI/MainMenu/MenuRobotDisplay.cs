using UnityEngine;

/// <summary>A small idle turn for the robot showcased in the operations bay.</summary>
public sealed class MenuRobotDisplay : MonoBehaviour
{
    private Quaternion _restRotation;
    private void OnEnable() => _restRotation = transform.localRotation;
    private void Update() => transform.localRotation =
        _restRotation * Quaternion.Euler(0, Mathf.Sin(Time.unscaledTime * 0.25f) * 3.5f, 0);
}
