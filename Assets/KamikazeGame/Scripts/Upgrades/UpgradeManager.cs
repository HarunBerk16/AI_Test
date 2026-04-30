using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Upgrade sahnesinin kontrolcüsü.
/// 3 upgrade kartı gösterir: Warhead, Yakıt, Kanat.
/// Satın alma işlemlerini yönetir.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    private UIDocument _doc;
    private VisualElement _root;

    // Coin label
    private Label _coinLabel;

    void Start()
    {
        _doc = GetComponent<UIDocument>();
        _root = _doc.rootVisualElement;

        _coinLabel = _root.Q<Label>("CoinLabel");

        SetupCard("Warhead", "WarheadCard");
        SetupCard("Fuel",    "FuelCard");
        SetupCard("Wing",    "WingCard");

        UpdateCoinLabel();

        // Play butonu
        Button playBtn = _root.Q<Button>("PlayButton");
        if (playBtn != null)
            playBtn.clicked += () => SceneManager.LoadScene("SampleScene");
    }

    void SetupCard(string upgradeType, string cardName)
    {
        VisualElement card = _root.Q<VisualElement>(cardName);
        if (card == null) return;

        Label levelLabel  = card.Q<Label>("LevelLabel");
        Label statLabel   = card.Q<Label>("StatLabel");
        Label costLabel   = card.Q<Label>("CostLabel");
        Button buyBtn     = card.Q<Button>("BuyButton");

        RefreshCard(upgradeType, levelLabel, statLabel, costLabel, buyBtn);

        if (buyBtn != null)
            buyBtn.clicked += () =>
            {
                TryBuy(upgradeType);
                RefreshCard(upgradeType, levelLabel, statLabel, costLabel, buyBtn);
                UpdateCoinLabel();
            };
    }

    void RefreshCard(string type, Label levelLabel, Label statLabel, Label costLabel, Button buyBtn)
    {
        int level;
        int cost;
        string statText;

        switch (type)
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

        if (levelLabel != null) levelLabel.text = isMax ? "MAX" : $"Seviye {level}";
        if (statLabel  != null) statLabel.text  = isMax ? "Maksimum seviyeye ulaşıldı" : statText;
        if (costLabel  != null) costLabel.text  = isMax ? "" : $"{cost} Coin";

        if (buyBtn != null)
        {
            buyBtn.SetEnabled(!isMax && GameData.Coins >= cost);
            buyBtn.text = isMax ? "MAX" : "UPGRADE";
        }
    }

    void TryBuy(string type)
    {
        int level, cost;

        switch (type)
        {
            case "Warhead":
                level = GameData.WarheadLevel;
                cost  = UpgradeData.WarheadCost(level);
                if (level < UpgradeData.MaxLevel && GameData.Coins >= cost)
                { GameData.Coins -= cost; GameData.WarheadLevel++; }
                break;
            case "Fuel":
                level = GameData.FuelLevel;
                cost  = UpgradeData.FuelCost(level);
                if (level < UpgradeData.MaxLevel && GameData.Coins >= cost)
                { GameData.Coins -= cost; GameData.FuelLevel++; }
                break;
            case "Wing":
                level = GameData.WingLevel;
                cost  = UpgradeData.WingCost(level);
                if (level < UpgradeData.MaxLevel && GameData.Coins >= cost)
                { GameData.Coins -= cost; GameData.WingLevel++; }
                break;
        }

        GameData.Save();
        GameEvents.OnUpgradePurchased?.Invoke();
    }

    void UpdateCoinLabel()
    {
        if (_coinLabel != null)
            _coinLabel.text = $"Coin: {GameData.Coins}";
    }
}
