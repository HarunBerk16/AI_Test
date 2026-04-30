using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Coin göster
        Label coinLabel = root.Q<Label>("CoinLabel");
        if (coinLabel != null)
            coinLabel.text = $"Coin: {GameData.Coins}";

        // Saldırı başlat
        Button playBtn = root.Q<Button>("PlayButton");
        if (playBtn != null)
            playBtn.clicked += () => SceneManager.LoadScene("SampleScene");

        // Upgrade ekranı
        Button upgradeBtn = root.Q<Button>("UpgradeButton");
        if (upgradeBtn != null)
            upgradeBtn.clicked += () => SceneManager.LoadScene("UpgradeScene");
    }
}
