using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;

public class GameHUD : MonoBehaviour
{
    private VisualElement _topBar;
    private Label         _levelLabel;
    private Label         _coinLabel;
    private Label         _distanceLabel;
    private VisualElement _menuPanel;
    private VisualElement _tapArea;
    private Label         _tapToStartLabel;
    private VisualElement _resultPanel;
    private VisualElement _pausePanel;
    private Button        _pauseButton;
    private Label         _resultTitle;
    private Label         _resultPercent;
    private Label         _resultCoin;
    private Label         _tapLabel;
    private VisualElement _resultBarFill;
    private Button        _resumeButton;
    private Button        _pauseMenuButton;

    private UpgradeCardUI _warheadCard;
    private UpgradeCardUI _fuelCard;
    private UpgradeCardUI _wingCard;

    private bool      _isPaused;
    private bool      _resultShown;
    private bool      _resultInteractive;
    private bool      _isFlying;
    private bool      _returned;
    private bool      _shouldAdvanceLevel;
    private Transform _planeTf;
    private Transform _targetTf;

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

        _topBar          = root.Q<VisualElement>("TopBar");
        _levelLabel      = root.Q<Label>("LevelLabel");
        _coinLabel       = root.Q<Label>("CoinLabel");
        _distanceLabel   = root.Q<Label>("DistanceLabel");
        _menuPanel       = root.Q<VisualElement>("MenuPanel");
        _tapArea         = root.Q<VisualElement>("TapArea");
        _tapToStartLabel = root.Q<Label>("TapToStartLabel");
        _resultPanel     = root.Q<VisualElement>("ResultPanel");
        _pausePanel      = root.Q<VisualElement>("PausePanel");
        _pauseButton     = root.Q<Button>("PauseButton");
        _resultTitle     = root.Q<Label>("ResultTitle");
        _resultPercent   = root.Q<Label>("ResultPercent");
        _resultCoin      = root.Q<Label>("ResultCoin");
        _tapLabel        = root.Q<Label>("TapLabel");
        _resultBarFill   = root.Q<VisualElement>("ResultBarFill");
        _resumeButton    = root.Q<Button>("ResumeButton");
        _pauseMenuButton = root.Q<Button>("PauseMenuButton");

        // Tap area = başlamak için ekrana dokun
        _tapArea?.RegisterCallback<ClickEvent>(_ =>
        {
            if (GameStateManager.Instance?.CurrentPhase == GamePhase.Menu)
                GameStateManager.Instance?.StartGame();
        });

        // Upgrade kartları: tüm kart tıklanabilir, kendi click'ini yukarı taşımaz
        _warheadCard = new UpgradeCardUI(root.Q<VisualElement>("WarheadCard"), "Warhead", OnBuyWarhead);
        _fuelCard    = new UpgradeCardUI(root.Q<VisualElement>("FuelCard"),    "Fuel",    OnBuyFuel);
        _wingCard    = new UpgradeCardUI(root.Q<VisualElement>("WingCard"),    "Wing",    OnBuyWing);

        _resultPanel?.RegisterCallback<ClickEvent>(_ => TryReturnToMenu());

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
                _isFlying          = false;
                _resultShown       = false;
                _resultInteractive = false;
                _topBar?.Show();
                _distanceLabel?.Hide();
                _menuPanel?.Show();
                _tapArea?.Show();
                _tapToStartLabel?.Show();
                _pauseButton?.Hide();
                _resultPanel?.Hide();
                UpdateTopBar();
                RefreshCards();
                break;

            case GamePhase.Flying:
                _isFlying           = true;
                _resultShown        = false;
                _returned           = false;
                _shouldAdvanceLevel = false;
                _resultInteractive  = false;
                _planeTf            = null;
                _targetTf           = null;
                _topBar?.Hide();
                _tapToStartLabel?.Hide();
                _distanceLabel?.Show();
                _menuPanel?.Hide();
                _tapArea?.Hide();
                _pauseButton?.Show();
                _resultPanel?.Hide();
                break;

            case GamePhase.Result:
                _isFlying = false;
                _topBar?.Hide();
                _tapToStartLabel?.Hide();
                _distanceLabel?.Hide();
                _pauseButton?.Hide();
                _menuPanel?.Hide();
                _tapArea?.Hide();
                _resultPanel?.Hide();
                break;
        }
    }

    void Update()
    {
        if (!_isFlying || _distanceLabel == null) return;

        if (_planeTf == null)
        {
            var pc = FindFirstObjectByType<PlaneController>();
            if (pc != null) _planeTf = pc.transform;
        }
        if (_targetTf == null)
        {
            var tb = FindFirstObjectByType<TargetBuilding>();
            if (tb != null) _targetTf = tb.transform;
        }

        if (_planeTf != null && _targetTf != null)
        {
            float dist = Vector3.Distance(_planeTf.position, _targetTf.position);
            _distanceLabel.text = $"Hedefe: {dist:F0}m";
        }
    }

    void UpdateTopBar()
    {
        if (_levelLabel != null) _levelLabel.text = $"LEVEL {GameData.CurrentLevel}";
        if (_coinLabel  != null) _coinLabel.text  = $"Coin: {GameData.Coins}";
    }

    // ──────────── Sonuç ────────────

    void ShowResult(float percent, int earned)
    {
        if (_resultShown) return;
        _resultShown        = true;
        _shouldAdvanceLevel = (percent >= 1f);

        GameStateManager.Instance?.EndMission();

        string title = percent >= 1f ? "MUKEMMEL! %100 YIKIM!"
                     : percent > 0f  ? "GOREV TAMAMLANDI"
                     : "HASAR YOK";
        StartCoroutine(DestructionResultRoutine(percent, earned, title));
    }

    void ShowCrash()
    {
        if (_resultShown) return;
        _resultShown = true;
        GameStateManager.Instance?.EndMission();
        StartCoroutine(DestructionResultRoutine(0f, 0, "DUSTUN!"));
    }

    IEnumerator DestructionResultRoutine(float percent, int earned, string title)
    {
        yield return new WaitForSeconds(2.5f);

        if (_resultTitle != null) _resultTitle.text = title;
        if (_resultCoin  != null)
        {
            _resultCoin.text = $"+{earned} Coin";
            _resultCoin.style.color = new StyleColor(earned > 0
                ? new Color(0.4f, 1f, 0.4f)
                : new Color(1f, 0.4f, 0.4f));
        }

        if (_resultBarFill != null)
        {
            Color barColor = percent < 0.5f ? new Color(1f, 0.32f, 0.08f)
                           : percent < 0.8f ? new Color(1f, 0.75f, 0.10f)
                           : new Color(0.20f, 0.90f, 0.30f);
            _resultBarFill.style.backgroundColor = new StyleColor(barColor);
        }

        if (_resultPercent != null) _resultPercent.text = "%0";
        if (_resultBarFill != null)
            _resultBarFill.style.width = new StyleLength(new Length(0f, LengthUnit.Percent));
        _tapLabel?.Hide();
        _resultPanel?.Show();

        float duration = 1.5f, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / duration);
            float eased   = 1f - Mathf.Pow(1f - t, 3f);
            float current = eased * percent;

            if (_resultBarFill != null)
                _resultBarFill.style.width = new StyleLength(new Length(current * 100f, LengthUnit.Percent));
            if (_resultPercent != null)
                _resultPercent.text = $"%{current * 100f:F0}";

            yield return null;
        }

        if (_resultBarFill != null)
            _resultBarFill.style.width = new StyleLength(new Length(percent * 100f, LengthUnit.Percent));
        if (_resultPercent != null)
            _resultPercent.text = $"%{percent * 100f:F0}";

        yield return new WaitForSeconds(0.4f);
        _tapLabel?.Show();
        _resultInteractive = true;
    }

    void OnTargetMissed()
    {
        _isFlying = false;
        _pauseButton?.Hide();
        _distanceLabel?.Hide();
    }

    void TryReturnToMenu()
    {
        if (!_resultInteractive || _returned) return;
        _returned = true;
        StopAllCoroutines();

        if (_shouldAdvanceLevel && GameData.CurrentLevel < 4)
        {
            GameData.CurrentLevel++;
            GameData.Save();
        }

        ResumeGame();
        GameStateManager.Instance?.ReturnToMenu();
    }

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
    private readonly VisualElement _card;
    private readonly Label         _levelLabel;
    private readonly Label         _statLabel;
    private readonly Label         _costLabel;
    private readonly Label         _buyButtonLabel;
    private readonly VisualElement _buyButtonVis;
    private readonly string        _type;
    private readonly Action        _onBuy;

    public UpgradeCardUI(VisualElement card, string type, Action onBuy)
    {
        _card           = card;
        _type           = type;
        _onBuy          = onBuy;
        _levelLabel     = card?.Q<Label>("LevelLabel");
        _statLabel      = card?.Q<Label>("StatLabel");
        _costLabel      = card?.Q<Label>("CostLabel");
        _buyButtonVis   = card?.Q<VisualElement>("BuyButtonVis");
        _buyButtonLabel = _buyButtonVis?.Q<Label>("BuyButtonLabel");

        // Tüm kart tıklanabilir
        card?.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            _onBuy?.Invoke();
        });

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
                statText = $"Patlama: {UpgradeData.WarheadRadius(level):F0}m -> {UpgradeData.WarheadRadius(level + 1):F0}m";
                break;
            case "Fuel":
                level    = GameData.FuelLevel;
                cost     = UpgradeData.FuelCost(level);
                statText = $"Hiz: {UpgradeData.FuelSpeed(level):F0} -> {UpgradeData.FuelSpeed(level + 1):F0}";
                break;
            case "Wing":
                level    = GameData.WingLevel;
                cost     = UpgradeData.WingCost(level);
                statText = $"Manevra: {UpgradeData.WingTurnSpeed(level):F0} -> {UpgradeData.WingTurnSpeed(level + 1):F0}";
                break;
            default: return;
        }

        bool isMax = level >= UpgradeData.MaxLevel;
        bool canBuy = !isMax && GameData.Coins >= cost;

        if (_levelLabel     != null) _levelLabel.text     = isMax ? "MAX"             : $"Seviye {level}";
        if (_statLabel      != null) _statLabel.text      = isMax ? "Maksimum seviye" : statText;
        if (_costLabel      != null) _costLabel.text      = isMax ? ""                : $"{cost} Coin";
        if (_buyButtonLabel != null) _buyButtonLabel.text = isMax ? "MAX"             : "UPGRADE";

        // Kart opaklığı: satın alınamazsa soluk görün
        if (_card != null)
            _card.style.opacity = canBuy || isMax ? 1f : 0.5f;

        // Buton arkaplanı: karşılanamaz veya max ise soluklaş
        if (_buyButtonVis != null)
        {
            var col = _buyButtonVis.style.backgroundColor.value;
            _buyButtonVis.style.opacity = canBuy || isMax ? 1f : 0.4f;
        }
    }
}
