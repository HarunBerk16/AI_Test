using UnityEngine;
using UnityEngine.UIElements;
using System;

/// <summary>
/// Tek UIDocument ile tüm fazları yönetir:
/// Menu  → upgrade kartları + başlat butonu altta
/// Flying→ sadece pause butonu
/// Result→ sonuç overlay
/// </summary>
public class GameHUD : MonoBehaviour
{
    private Label         _levelLabel;
    private Label         _coinLabel;
    private VisualElement _menuPanel;
    private VisualElement _resultPanel;
    private VisualElement _pausePanel;
    private Button        _pauseButton;
    private Label         _resultTitle;
    private Label         _resultPercent;
    private Label         _resultCoin;
    private Button        _retryButton;
    private Button        _backMenuButton;
    private Button        _resumeButton;
    private Button        _pauseMenuButton;
    private Button        _startButton;
    private Button        _nextLevelButton;

    private UpgradeCardUI _warheadCard;
    private UpgradeCardUI _fuelCard;
    private UpgradeCardUI _wingCard;

    private bool _isPaused;
    private bool _resultShown;

    void OnEnable()
    {
        GameStateManager.OnPhaseChanged += HandlePhaseChange;
        GameEvents.OnMissionComplete    += ShowResult;
        GameEvents.OnPlaneCrash         += ShowCrash;
        GameEvents.OnTargetMissed       += OnTargetMissed;
    }

    void OnDisable()
    {
        GameStateManager.OnPhaseChanged -= HandlePhaseChange;
        GameEvents.OnMissionComplete    -= ShowResult;
        GameEvents.OnPlaneCrash         -= ShowCrash;
        GameEvents.OnTargetMissed       -= OnTargetMissed;
    }

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _levelLabel      = root.Q<Label>("LevelLabel");
        _coinLabel       = root.Q<Label>("CoinLabel");
        _menuPanel       = root.Q<VisualElement>("MenuPanel");
        _resultPanel     = root.Q<VisualElement>("ResultPanel");
        _pausePanel      = root.Q<VisualElement>("PausePanel");
        _pauseButton     = root.Q<Button>("PauseButton");
        _resultTitle     = root.Q<Label>("ResultTitle");
        _resultPercent   = root.Q<Label>("ResultPercent");
        _resultCoin      = root.Q<Label>("ResultCoin");
        _retryButton     = root.Q<Button>("RetryButton");
        _backMenuButton  = root.Q<Button>("BackMenuButton");
        _resumeButton    = root.Q<Button>("ResumeButton");
        _pauseMenuButton = root.Q<Button>("PauseMenuButton");
        _startButton     = root.Q<Button>("StartButton");
        _nextLevelButton = root.Q<Button>("NextLevelButton");

        _warheadCard = new UpgradeCardUI(root.Q<VisualElement>("WarheadCard"), "Warhead", OnBuyWarhead);
        _fuelCard    = new UpgradeCardUI(root.Q<VisualElement>("FuelCard"),    "Fuel",    OnBuyFuel);
        _wingCard    = new UpgradeCardUI(root.Q<VisualElement>("WingCard"),    "Wing",    OnBuyWing);

        _startButton?.RegisterCallback<ClickEvent>(_ =>
            GameStateManager.Instance?.StartGame());

        _nextLevelButton?.RegisterCallback<ClickEvent>(_ =>
        {
            if (GameData.CurrentLevel < 2) { GameData.CurrentLevel++; GameData.Save(); }
            GameStateManager.Instance?.StartGame();
        });

        _retryButton?.RegisterCallback<ClickEvent>(_ =>
            GameStateManager.Instance?.StartGame());

        _backMenuButton?.RegisterCallback<ClickEvent>(_ =>
        {
            ResumeGame();
            GameStateManager.Instance?.ReturnToMenu();
        });

        _pauseButton?.RegisterCallback<ClickEvent>(_ => TogglePause());
        _resumeButton?.RegisterCallback<ClickEvent>(_ => TogglePause());

        _pauseMenuButton?.RegisterCallback<ClickEvent>(_ =>
        {
            ResumeGame();
            GameStateManager.Instance?.ReturnToMenu();
        });

        HandlePhaseChange(GamePhase.Menu);
    }

    void HandlePhaseChange(GamePhase phase)
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _pausePanel?.Hide();

        switch (phase)
        {
            case GamePhase.Menu:
                _resultShown = false;
                _menuPanel?.Show();
                _pauseButton?.Hide();
                _resultPanel?.Hide();
                RefreshCards();
                UpdateNextLevelButton();
                break;

            case GamePhase.Flying:
                _resultShown = false;
                _menuPanel?.Hide();
                _pauseButton?.Show();
                _resultPanel?.Hide();
                break;

            case GamePhase.Result:
                _pauseButton?.Hide();
                break;
        }

        UpdateTopBar();
    }

    void UpdateTopBar()
    {
        if (_levelLabel != null) _levelLabel.text = $"LEVEL {GameData.CurrentLevel}";
        if (_coinLabel  != null) _coinLabel.text  = $"Coin: {GameData.Coins}";
    }

    void UpdateNextLevelButton()
    {
        if (_nextLevelButton == null) return;
        _nextLevelButton.style.display = GameData.CurrentLevel >= 2
            ? DisplayStyle.None
            : DisplayStyle.Flex;
    }

    // ──────────── Sonuç ────────────

    void ShowResult(float percent, int earned)
    {
        if (_resultShown) return;
        _resultShown = true;

        UpdateTopBar();
        _resultPanel?.Show();

        if (_resultTitle   != null) _resultTitle.text   = percent > 0f ? "GÖREV TAMAMLANDI" : "HASAR YOK";
        if (_resultPercent != null) _resultPercent.text = $"Yıkım: %{percent * 100f:F0}";
        if (_resultCoin    != null)
        {
            _resultCoin.text = $"+{earned} Coin";
            _resultCoin.style.color = new StyleColor(earned > 0
                ? new Color(0.4f, 1f, 0.4f)
                : new Color(1f, 0.4f, 0.4f));
        }

        GameStateManager.Instance?.EndMission();
    }

    void ShowCrash()
    {
        if (_resultShown) return;
        _resultShown = true;

        UpdateTopBar();
        _resultPanel?.Show();

        if (_resultTitle   != null) _resultTitle.text   = "DÜŞTÜN!";
        if (_resultPercent != null) _resultPercent.text = "Yıkım: %0";
        if (_resultCoin    != null)
        {
            _resultCoin.text = "+0 Coin";
            _resultCoin.style.color = new StyleColor(new Color(1f, 0.4f, 0.4f));
        }

        GameStateManager.Instance?.EndMission();
    }

    void OnTargetMissed() => _pauseButton?.Hide();

    // ──────────── Pause ────────────

    void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        if (_isPaused) _pausePanel?.Show(); else _pausePanel?.Hide();
        GameEvents.OnPauseChanged?.Invoke(_isPaused);
    }

    void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _pausePanel?.Hide();
    }

    // ──────────── Upgrade ────────────

    void OnBuyWarhead()
    {
        int lvl  = GameData.WarheadLevel;
        int cost = UpgradeData.WarheadCost(lvl);
        if (lvl < UpgradeData.MaxLevel && GameData.Coins >= cost)
            { GameData.Coins -= cost; GameData.WarheadLevel++; GameData.Save(); }
        RefreshCards();
    }

    void OnBuyFuel()
    {
        int lvl  = GameData.FuelLevel;
        int cost = UpgradeData.FuelCost(lvl);
        if (lvl < UpgradeData.MaxLevel && GameData.Coins >= cost)
            { GameData.Coins -= cost; GameData.FuelLevel++; GameData.Save(); }
        RefreshCards();
    }

    void OnBuyWing()
    {
        int lvl  = GameData.WingLevel;
        int cost = UpgradeData.WingCost(lvl);
        if (lvl < UpgradeData.MaxLevel && GameData.Coins >= cost)
            { GameData.Coins -= cost; GameData.WingLevel++; GameData.Save(); }
        RefreshCards();
    }

    void RefreshCards()
    {
        UpdateTopBar();
        _warheadCard?.Refresh();
        _fuelCard?.Refresh();
        _wingCard?.Refresh();
    }
}

// ──────────── Kart Yardımcısı ────────────

class UpgradeCardUI
{
    private readonly Label  _levelLabel;
    private readonly Label  _statLabel;
    private readonly Label  _costLabel;
    private readonly Button _buyButton;
    private readonly string _type;

    public UpgradeCardUI(VisualElement card, string type, Action onBuy)
    {
        _type       = type;
        _levelLabel = card?.Q<Label>("LevelLabel");
        _statLabel  = card?.Q<Label>("StatLabel");
        _costLabel  = card?.Q<Label>("CostLabel");
        _buyButton  = card?.Q<Button>("BuyButton");
        if (_buyButton != null) _buyButton.clicked += onBuy;
        Refresh();
    }

    public void Refresh()
    {
        int    level;
        int    cost;
        string statText;

        switch (_type)
        {
            case "Warhead":
                level    = GameData.WarheadLevel;
                cost     = UpgradeData.WarheadCost(level);
                statText = $"Patlama: {UpgradeData.WarheadRadius(level):F0}m → {UpgradeData.WarheadRadius(level + 1):F0}m";
                break;
            case "Fuel":
                level    = GameData.FuelLevel;
                cost     = UpgradeData.FuelCost(level);
                statText = $"Hız: {UpgradeData.FuelSpeed(level):F0} → {UpgradeData.FuelSpeed(level + 1):F0}";
                break;
            case "Wing":
                level    = GameData.WingLevel;
                cost     = UpgradeData.WingCost(level);
                statText = $"Manevra: {UpgradeData.WingTurnSpeed(level):F0} → {UpgradeData.WingTurnSpeed(level + 1):F0}";
                break;
            default: return;
        }

        bool isMax = level >= UpgradeData.MaxLevel;

        if (_levelLabel != null) _levelLabel.text = isMax ? "MAX"              : $"Seviye {level}";
        if (_statLabel  != null) _statLabel.text  = isMax ? "Maksimum seviye"  : statText;
        if (_costLabel  != null) _costLabel.text  = isMax ? ""                 : $"{cost} Coin";

        if (_buyButton != null)
        {
            _buyButton.SetEnabled(!isMax && GameData.Coins >= cost);
            _buyButton.text = isMax ? "MAX" : "UPGRADE";
        }
    }
}
