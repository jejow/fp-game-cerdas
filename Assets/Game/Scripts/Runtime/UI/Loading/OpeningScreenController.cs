using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

/// <summary>Shows the robot boot screen while the main menu actually loads.</summary>
[RequireComponent(typeof(UIDocument))]
public sealed class OpeningScreenController : MonoBehaviour
{
    [SerializeField, Min(0)] private float minimumDisplaySeconds = 2.4f;
    private VisualElement _fill, _mark;
    private Label _status, _percentage;
    private Button _skip;
    private bool _skipRequested;
    private float _elapsed;

    public void Skip() => _skipRequested = true;

    private IEnumerator Start()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var root = GetComponent<UIDocument>().rootVisualElement;
        GameIdentity.ApplyTo(root);
        _fill = root.Q("OpeningFill");
        _mark = root.Q("RobotMark");
        _status = root.Q<Label>("OpeningStatus");
        _percentage = root.Q<Label>("OpeningPercentage");
        _skip = root.Q<Button>("SkipOpening");
        _skip.clicked += Skip;

        // Present the opening before beginning potentially expensive scene deserialization.
        yield return null;
        yield return null;
        AsyncOperation loading = null;
        try
        {
            loading = SceneManager.LoadSceneAsync("MainMenu");
            if (loading == null) throw new InvalidOperationException("The MainMenu scene could not be loaded.");
            loading.allowSceneActivation = false;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            _status.text = "MENU UNAVAILABLE — RESTART THE GAME";
            _skip.SetEnabled(false);
        }
        if (loading == null) yield break;

        while (loading.progress < 0.9f || (!_skipRequested && _elapsed < minimumDisplaySeconds))
        {
            _elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(loading.progress / 0.9f);
            _fill.style.width = Length.Percent(progress * 100);
            _percentage.text = $"{Mathf.RoundToInt(progress * 100):00}%";
            _status.text = progress >= 1 ? "ALL SYSTEMS READY" : "PREPARING THE OPERATIONS BAY";
            _mark.style.opacity = 0.75f + Mathf.Sin(_elapsed * 3) * 0.2f;
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame ||
                keyboard.spaceKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)) Skip();
            yield return null;
        }
        _fill.style.width = Length.Percent(100);
        _percentage.text = "100%";
        _status.text = "ALL SYSTEMS READY";
        loading.allowSceneActivation = true;
        while (!loading.isDone) yield return null;
    }

    private void OnDestroy()
    {
        if (_skip != null) _skip.clicked -= Skip;
    }
}
