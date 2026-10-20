using GameNetwork;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Properties;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset _settingsMenuAsset;
    [SerializeField] private GameManager _gameManager;
    private VisualElement _root, _logo, _mainMenuElements, _container, _botMenu, _feedback, _fill;
    private Label _feedbackLabel, _matchSummary, _botDescription;
    private DropdownField _team, _allies, _enemies, _difficulty;
    private Button _startButton, _backButton;
    private SessionData _loadingData;
    private bool _starting;
    public Task StartingMatch { get; private set; } = Task.CompletedTask;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        GameIdentity.ApplyTo(_root);
        _container = _root.Q("Container");
        _mainMenuElements = _root.Q("MainMenuElements");
        _logo = _root.Q("Logo");
        _root.Q<Button>("PlayButton").clicked += OpenBotSetup;
        _root.Q<Button>("SettingsButton").clicked += OnSettings;
        _root.Q<Button>("QuitButton").clicked += OnQuit;
        _botMenu = _root.Q("BotSetup");
        _team = _root.Q<DropdownField>("PlayerTeam");
        _team.choices = new() { "Corpo robots — Plant the bomb", "Natif robots — Defuse the bomb" };
        _allies = _root.Q<DropdownField>("AllyBots");
        _allies.choices = Enumerable.Range(0, OfflineMatchOptions.MaxAllyBots + 1).Select(x => x.ToString()).ToList();
        _enemies = _root.Q<DropdownField>("EnemyBots");
        _enemies.choices = Enumerable.Range(1, OfflineMatchOptions.MaxEnemyBots).Select(x => x.ToString()).ToList();
        _difficulty = _root.Q<DropdownField>("BotDifficulty");
        _difficulty.choices = new() { "Easy", "Normal", "Hard" };
        _matchSummary = _root.Q<Label>("MatchSummary");
        _botDescription = _root.Q<Label>("BotDescription");
        _startButton = _root.Q<Button>("StartOffline");
        _backButton = _root.Q<Button>("BackFromBotSetup");
        _startButton.clicked += OnStart;
        _backButton.clicked += CloseBotSetup;
        _root.Q<Button>("DuelPreset").clicked += () => SetCounts(0, 1);
        _root.Q<Button>("FullTeamsPreset").clicked += () => SetCounts(4, 5);
        _team.RegisterValueChangedCallback(_ => UpdateSummary());
        _allies.RegisterValueChangedCallback(_ => UpdateSummary());
        _enemies.RegisterValueChangedCallback(_ => UpdateSummary());
        _difficulty.RegisterValueChangedCallback(_ => UpdateSummary());
        _feedback = _root.Q("ConnectionFeedback");
        _feedbackLabel = _feedback.Q<Label>();
        _fill = _feedback.Q("Fill");
        _feedback.style.display = DisplayStyle.None;
        _botMenu.style.display = DisplayStyle.None;
        _loadingData = SessionData.Instance;
        _loadingData.propertyChanged += OnLoadingChanged;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SoundManager.Instance.Play("Music", "MainMenu", Vector3.zero);
    }

    private void Update()
    {
        if (_starting || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (_botMenu.style.display == DisplayStyle.Flex) CloseBotSetup();
    }

    private void OnDestroy()
    {
        if (_loadingData != null) _loadingData.propertyChanged -= OnLoadingChanged;
    }

    public void OpenBotSetup()
    {
        if (_starting) return;
        CloseSettings();
        _container.Clear();
        var options = OfflineMatchOptions.Load();
        _team.index = options.PlayerTeam == TeamSideType.Corpo ? 0 : 1;
        _allies.index = options.AllyBots;
        _enemies.index = options.EnemyBots - 1;
        _difficulty.index = (int)options.Difficulty;
        UpdateSummary();
        ShowMainMenu(false);
        _feedback.style.display = DisplayStyle.None;
        _botMenu.style.display = DisplayStyle.Flex;
        _startButton.Focus();
    }

    private OfflineMatchOptions SelectedOptions() => new OfflineMatchOptions(
        _allies.index, _enemies.index + 1, (BotDifficulty)_difficulty.index,
        _team.index == 1 ? TeamSideType.Natif : TeamSideType.Corpo).Normalized();

    private void SetCounts(int allies, int enemies)
    {
        _allies.index = allies;
        _enemies.index = enemies - 1;
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (_matchSummary == null || _botDescription == null) return;
        var options = SelectedOptions();
        _matchSummary.text = $"{options.AllyBots + 1} vs {options.EnemyBots}  ·  {options.PlayerTeam}  ·  {options.Difficulty}";
        _botDescription.text = options.Difficulty switch
        {
            BotDifficulty.Easy => "Slower reactions and forgiving aim. Good for learning the map.",
            BotDifficulty.Hard => "Faster reactions and sharper aim. Expect a tougher fight.",
            _ => "Balanced reactions and aim. Bots fight and play the bomb objective."
        };
    }

    private void CloseBotSetup()
    {
        if (_starting) return;
        SelectedOptions().Save();
        _botMenu.style.display = DisplayStyle.None;
        _feedback.style.display = DisplayStyle.None;
        ShowMainMenu(true);
        _root.Q<Button>("PlayButton").Focus();
    }

    private void OnStart()
    {
        if (!_starting) StartingMatch = StartMatchAsync();
    }

    private async Task StartMatchAsync()
    {
        _starting = true;
        var options = SelectedOptions();
        options.Save();
        _botMenu.SetEnabled(false);
        _feedback.style.display = DisplayStyle.Flex;
        SoundManager.Instance.Play("Music", "Stop", Vector3.zero);
        try
        {
            if (_gameManager == null) _gameManager = FindFirstObjectByType<GameManager>();
            if (_gameManager == null) throw new InvalidOperationException("The main menu is missing its game manager.");
            await _gameManager.Play(options);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (this != null)
            {
                _feedbackLabel.text = "The offline match could not start. Try again.";
                SoundManager.Instance.Play("Music", "MainMenu", Vector3.zero);
            }
        }
        finally
        {
            if (this != null)
            {
                _starting = false;
                _botMenu.SetEnabled(true);
            }
        }
    }

    private void OnLoadingChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if (!_starting) return;
        _feedback.style.display = DisplayStyle.Flex;
        _feedbackLabel.text = _loadingData.LoadingStatusText;
        _fill.style.width = Length.Percent(_loadingData.LoadingProgress * 100f);
    }

    private void ShowMainMenu(bool visible)
    {
        _logo.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        _mainMenuElements.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void CloseSettings()
    {
        if (!TryGetComponent<SettingsMenuController>(out var controller)) return;
        controller.SaveAndCloseSettings();
        Destroy(controller);
    }

    private void OnSettings()
    {
        CloseSettings();
        _container.Clear();
        var controller = gameObject.AddComponent<SettingsMenuController>();
        controller.root = UIDocumentUtils.InstantiateFullSize(_settingsMenuAsset);
        _container.Add(controller.root);
    }

    private void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}
