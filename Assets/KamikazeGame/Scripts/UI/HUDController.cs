using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    private VisualElement _root;
    private Label _coinLabel;
    private VisualElement _resultPanel;
    private Label _resultTitle;
    private Label _resultPercent;
    private Label _resultCoin;
    private Button _continueButton;
    private Button _menuButton;
    private Button _pauseButton;
    private VisualElement _pausePanel;
    private Button _resumeButton;
    private Button _pauseMenuButton;

    private bool _isPaused = false;

    void OnEnable()
    {
        GameEvents.OnMissionComplete += ShowResult;
        GameEvents.OnPlaneCrash      += ShowCrash;
        GameEvents.OnTargetMissed    += OnTargetMissed;
    }

    void OnDisable()
    {
        GameEvents.OnMissionComplete -= ShowResult;
        GameEvents.OnPlaneCrash      -= ShowCrash;
        GameEvents.OnTargetMissed    -= OnTargetMissed;
    }

    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _coinLabel       = _root.Q<Label>("CoinLabel");
        _resultPanel     = _root.Q<VisualElement>("ResultPanel");
        _resultTitle     = _root.Q<Label>("ResultTitle");
        _resultPercent   = _root.Q<Label>("ResultPercent");
        _resultCoin      = _root.Q<Label>("ResultCoin");
        _continueButton  = _root.Q<Button>("ContinueButton");
        _menuButton      = _root.Q<Button>("MenuButton");
        _pauseButton     = _root.Q<Button>("PauseButton");
        _pausePanel      = _root.Q<VisualElement>("PausePanel");
        _resumeButton    = _root.Q<Button>("ResumeButton");
        _pauseMenuButton = _root.Q<Button>("PauseMenuButton");

        _resultPanel?.Hide();
        _pausePanel?.Hide();

        _continueButton?.RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene("UpgradeScene"));
        _menuButton?.RegisterCallback<ClickEvent>(_     => { ResumeGame(); SceneManager.LoadScene("MainMenu"); });
        _pauseButton?.RegisterCallback<ClickEvent>(_    => TogglePause());
        _resumeButton?.RegisterCallback<ClickEvent>(_   => TogglePause());
        _pauseMenuButton?.RegisterCallback<ClickEvent>(_ => { ResumeGame(); SceneManager.LoadScene("MainMenu"); });

        UpdateCoinDisplay();
    }

    void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        if (_isPaused) _pausePanel?.Show();
        else           _pausePanel?.Hide();
        GameEvents.OnPauseChanged?.Invoke(_isPaused);
    }

    void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _pausePanel?.Hide();
    }

    void UpdateCoinDisplay()
    {
        if (_coinLabel != null)
            _coinLabel.text = $"Coin: {GameData.Coins}";
    }

    void ShowResult(float percent, int earned)
    {
        ResumeGame();
        UpdateCoinDisplay();
        if (_resultPanel == null) return;

        _resultPanel.Show();
        if (_resultTitle   != null) _resultTitle.text   = "GÖREV TAMAMLANDI";
        if (_resultPercent != null) _resultPercent.text = $"Yikim: %{percent * 100:F0}";
        if (_resultCoin    != null) _resultCoin.text    = $"+{earned} Coin";
        if (_resultCoin    != null) _resultCoin.style.color = new StyleColor(new Color(0.4f, 1f, 0.4f));
    }

    void ShowCrash()
    {
        ResumeGame();
        UpdateCoinDisplay();
        if (_resultPanel == null) return;

        _resultPanel.Show();
        if (_resultTitle   != null) _resultTitle.text   = "DÜŞTÜN!";
        if (_resultPercent != null) _resultPercent.text = "Yikim: %0";
        if (_resultCoin    != null) { _resultCoin.text = "+0 Coin"; _resultCoin.style.color = new StyleColor(new Color(1f, 0.4f, 0.4f)); }
    }

    void OnTargetMissed()
    {
        // Hedef kaçırıldı — süzülme devam ediyor, yere düşünce ShowCrash çağrılır
        // Pause butonunu gizle (artık kontrol yok)
        if (_pauseButton != null) _pauseButton.style.display = DisplayStyle.None;
    }
}

// VisualElement için kısa yardımcılar
static class VEExtensions
{
    public static void Show(this VisualElement ve) => ve.style.display = DisplayStyle.Flex;
    public static void Hide(this VisualElement ve) => ve.style.display = DisplayStyle.None;
}
