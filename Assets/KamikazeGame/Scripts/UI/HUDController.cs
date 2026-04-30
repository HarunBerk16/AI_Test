using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Uçuş sırasındaki HUD:
/// - Coin göstergesi
/// - Görev tamamlandığında yıkım yüzdesi + kazanılan coin
/// </summary>
public class HUDController : MonoBehaviour
{
    private UIDocument _doc;
    private Label _coinLabel;
    private VisualElement _resultPanel;
    private Label _resultPercent;
    private Label _resultCoin;
    private Button _continueButton;
    private Button _menuButton;

    void OnEnable()
    {
        GameEvents.OnMissionComplete += ShowResult;
    }

    void OnDisable()
    {
        GameEvents.OnMissionComplete -= ShowResult;
    }

    void Start()
    {
        _doc = GetComponent<UIDocument>();
        var root = _doc.rootVisualElement;

        _coinLabel    = root.Q<Label>("CoinLabel");
        _resultPanel  = root.Q<VisualElement>("ResultPanel");
        _resultPercent = root.Q<Label>("ResultPercent");
        _resultCoin   = root.Q<Label>("ResultCoin");
        _continueButton = root.Q<Button>("ContinueButton");

        _menuButton = root.Q<Button>("MenuButton");

        if (_continueButton != null)
            _continueButton.clicked += OnContinueClicked;

        if (_menuButton != null)
            _menuButton.clicked += () =>
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");

        // Başta result paneli gizle
        if (_resultPanel != null)
            _resultPanel.style.display = DisplayStyle.None;

        UpdateCoinDisplay();
    }

    void UpdateCoinDisplay()
    {
        if (_coinLabel != null)
            _coinLabel.text = $"Coin: {GameData.Coins}";
    }

    void ShowResult(float percent, int earned)
    {
        UpdateCoinDisplay();

        if (_resultPanel == null) return;

        _resultPanel.style.display = DisplayStyle.Flex;

        if (_resultPercent != null)
            _resultPercent.text = $"Yikim: %{percent * 100:F0}";

        if (_resultCoin != null)
            _resultCoin.text = $"+{earned} Coin";
    }

    void OnContinueClicked()
    {
        // Upgrade ekranına geç (sonraki adımda yapılacak)
        UnityEngine.SceneManagement.SceneManager.LoadScene("UpgradeScene");
    }
}
