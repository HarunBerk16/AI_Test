using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UpgradeManager : MonoBehaviour
{
    private UIDocument   _doc;
    private VisualElement _root;
    private Label         _coinLabel;

    void Start()
    {
        _doc  = GetComponent<UIDocument>();
        _root = _doc.rootVisualElement;

        _coinLabel = _root.Q<Label>("CoinLabel");

        SetupCard("Warhead",   "WarheadCard");
        SetupCard("Hull",      "FuelCard");      // aynı kart adı, farklı tip
        SetupCard("Stability", "WingCard");

        UpdateCoinLabel();

        Button playBtn = _root.Q<Button>("PlayButton");
        if (playBtn != null)
            playBtn.clicked += () => SceneManager.LoadScene("SampleScene");

        Button nextBtn = _root.Q<Button>("NextLevelButton");
        if (nextBtn != null)
        {
            if (GameData.CurrentLevel >= 2)
                nextBtn.style.display = DisplayStyle.None;
            else
                nextBtn.clicked += () =>
                {
                    GameData.CurrentLevel++;
                    GameData.Save();
                    SceneManager.LoadScene("SampleScene");
                };
        }
    }

    void SetupCard(string upgradeType, string cardName)
    {
        VisualElement card = _root.Q<VisualElement>(cardName);
        if (card == null) return;

        Label  levelLabel = card.Q<Label>("LevelLabel");
        Label  statLabel  = card.Q<Label>("StatLabel");
        Label  costLabel  = card.Q<Label>("CostLabel");
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
        int    level;
        int    maxLevel;
        int    cost;
        string statText;
        string lockedText = null;

        switch (type)
        {
            case "Warhead":
                level    = GameData.WarheadLevel;
                maxLevel = UpgradeData.MaxWarheadLevel;
                int cap  = GameData.MaxWarheadForHull;
                cost     = UpgradeData.WarheadCost(level);
                statText = $"Patlama: {UpgradeData.WarheadRadius(level):F0}m → {UpgradeData.WarheadRadius(level + 1):F0}m";
                if (level >= cap && level < maxLevel)
                    lockedText = $"Gövde yükselt!";
                break;

            case "Hull":
                level    = GameData.HullLevel;
                maxLevel = UpgradeData.MaxHullLevel;
                cost     = UpgradeData.HullCost(level);
                statText = $"{UpgradeData.HullName(level)} → {UpgradeData.HullNextName(level)}  |  Hız +{UpgradeData.HullSpeed(level + 1) - UpgradeData.HullSpeed(level):F0}";
                break;

            case "Stability":
                level    = GameData.StabilityLevel;
                maxLevel = UpgradeData.MaxStabilityLevel;
                cost     = UpgradeData.StabilityCost(level);
                statText = $"Stabilite: {UpgradeData.StabilityTurnSpeed(level):F0} → {UpgradeData.StabilityTurnSpeed(level + 1):F0}";
                break;

            default: return;
        }

        bool isMax    = level >= maxLevel;
        bool isLocked = lockedText != null;

        if (levelLabel != null) levelLabel.text = isMax ? "MAX" : $"Seviye {level}";
        if (statLabel  != null) statLabel.text  = isMax    ? "Maksimum seviyeye ulaşıldı"
                                                : isLocked ? lockedText
                                                : statText;
        if (costLabel  != null) costLabel.text  = (isMax || isLocked) ? "" : $"{cost} Coin";

        if (buyBtn != null)
        {
            buyBtn.SetEnabled(!isMax && !isLocked && GameData.Coins >= cost);
            buyBtn.text = isMax ? "MAX" : isLocked ? "KİLİTLİ" : "UPGRADE";
        }
    }

    void TryBuy(string type)
    {
        switch (type)
        {
            case "Warhead":
            {
                int lvl  = GameData.WarheadLevel;
                int cap  = GameData.MaxWarheadForHull;
                int cost = UpgradeData.WarheadCost(lvl);
                if (lvl < cap && lvl < UpgradeData.MaxWarheadLevel && GameData.Coins >= cost)
                { GameData.Coins -= cost; GameData.WarheadLevel++; }
                break;
            }
            case "Hull":
            {
                int lvl  = GameData.HullLevel;
                int cost = UpgradeData.HullCost(lvl);
                if (lvl < UpgradeData.MaxHullLevel && GameData.Coins >= cost)
                { GameData.Coins -= cost; GameData.HullLevel++; }
                break;
            }
            case "Stability":
            {
                int lvl  = GameData.StabilityLevel;
                int cost = UpgradeData.StabilityCost(lvl);
                if (lvl < UpgradeData.MaxStabilityLevel && GameData.Coins >= cost)
                { GameData.Coins -= cost; GameData.StabilityLevel++; }
                break;
            }
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
