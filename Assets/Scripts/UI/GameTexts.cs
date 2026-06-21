using UnityEngine;

/// <summary>
/// Oyuncuya gosterilen tum metinler; LocalizationManager uzerinden cozulur.
/// </summary>
public static class GameTexts
{
    public static string ForgeButton => LocalizationManager.Get(LocalizationKey.ForgeButton);
    public static string SellButton => LocalizationManager.Get(LocalizationKey.SellButton);
    public static string UpgradeButton => LocalizationManager.Get(LocalizationKey.UpgradeButton);

    public static string AutoForgeOn => LocalizationManager.Get(LocalizationKey.AutoForgeOn);
    public static string AutoForgeOff => LocalizationManager.Get(LocalizationKey.AutoForgeOff);
    public static string AutoSellOn => LocalizationManager.Get(LocalizationKey.AutoSellOn);
    public static string AutoSellOff => LocalizationManager.Get(LocalizationKey.AutoSellOff);

    public static string InventoryFull => LocalizationManager.Get(LocalizationKey.InventoryFull);
    public static string ItemAlreadyInInventory(string itemName) =>
        LocalizationManager.Format(LocalizationKey.ItemAlreadyInInventory, itemName);

    public static string ItemCategoryAlreadyInInventory(ItemCategory category) =>
        LocalizationManager.Format(LocalizationKey.ItemCategoryAlreadyInInventory, GetItemCategoryDisplayName(category));

    public static string GetItemCategoryDisplayName(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon: return LocalizationManager.Get(LocalizationKey.ItemCategoryWeapon);
            case ItemCategory.Armor: return LocalizationManager.Get(LocalizationKey.ItemCategoryArmor);
            case ItemCategory.Earring: return LocalizationManager.Get(LocalizationKey.ItemCategoryEarring);
            case ItemCategory.Necklace: return LocalizationManager.Get(LocalizationKey.ItemCategoryNecklace);
            default: return category.ToString();
        }
    }

    public static string SelectFromInventory => LocalizationManager.Get(LocalizationKey.SelectFromInventory);
    public static string InventoryEmpty => LocalizationManager.Get(LocalizationKey.InventoryEmpty);

    public static string PromptBetterItem => LocalizationManager.Get(LocalizationKey.PromptBetterItem);
    public static string PromptInventoryFullSame => LocalizationManager.Get(LocalizationKey.PromptInventoryFullSame);
    public static string PromptNewItem => LocalizationManager.Get(LocalizationKey.PromptNewItem);
    public static string PromptBestInInventory => LocalizationManager.Get(LocalizationKey.PromptBestInInventory);
    public static string PromptKeep => LocalizationManager.Get(LocalizationKey.PromptKeep);
    public static string PromptSellNew => LocalizationManager.Get(LocalizationKey.PromptSellNew);

    public static string Upgrading => LocalizationManager.Get(LocalizationKey.Upgrading);
    public static string UpgradingButton => LocalizationManager.Get(LocalizationKey.UpgradingButton);

    /// <summary>Tier filtresi acikken etiket metni (min–max araligi ile).</summary>
    public static string AutoSellTierFilterLabel(int currentTier) =>
        LocalizationManager.Format(
            LocalizationKey.AutoSellTierFilterOn,
            AutoSellFilter.MinTier,
            AutoSellFilter.MaxTier,
            currentTier);

    /// <summary>Tier filtresi kapaliyken etiket metni.</summary>
    public static string AutoSellTierFilterOffLabel =>
        LocalizationManager.Get(LocalizationKey.AutoSellTierFilterOff);

    /// <summary>Cag filtresi acikken etiket metni (min–max araligi ile).</summary>
    public static string AutoSellEraFilterLabel(int eraIndex)
    {
        string minEra = LocalizationManager.GetEraDisplayName(AutoSellFilter.GetEraByIndex(0));
        string maxEra = LocalizationManager.GetEraDisplayName(
            AutoSellFilter.GetEraByIndex(AutoSellFilter.EraOrder.Length - 1));
        string currentEra = LocalizationManager.GetEraDisplayName(AutoSellFilter.GetEraByIndex(eraIndex));

        return LocalizationManager.Format(LocalizationKey.AutoSellEraFilterOn, minEra, maxEra, currentEra);
    }

    /// <summary>Cag filtresi kapaliyken etiket metni.</summary>
    public static string AutoSellEraFilterOffLabel =>
        LocalizationManager.Get(LocalizationKey.AutoSellEraFilterOff);

    /// <summary>Filtre degerinin aralik icindeki konumu.</summary>
    public static string FilterRangePosition(int currentIndex, int totalCount) =>
        LocalizationManager.Format(LocalizationKey.FilterRangePosition, currentIndex, totalCount);

    /// <summary>Otomatik satis geri bildirimi.</summary>
    public static string AutoSoldFeedback(double gold) =>
        LocalizationManager.Format(LocalizationKey.AutoSoldFeedback, gold);

    /// <summary>Envantere ekleme geri bildirimi.</summary>
    public static string ItemAddedToInventory(string itemName) =>
        LocalizationManager.Format(LocalizationKey.ItemAddedToInventory, itemName);

    /// <summary>Gold miktarini formatlar.</summary>
    public static string GoldAmount(double gold) =>
        LocalizationManager.Format(LocalizationKey.GoldAmount, gold);

    /// <summary>Cekic miktarini formatlar; yenilenme varsa geri sayim ekler.</summary>
    public static string HammerAmount(int current, int max, float regenSeconds)
    {
        if (regenSeconds > 0f)
            return LocalizationManager.Format(
                LocalizationKey.HammerAmountWithRegen,
                current,
                max,
                FormatDuration(regenSeconds));

        return LocalizationManager.Format(LocalizationKey.HammerAmount, current, max);
    }

    /// <summary>Yetersiz cekic uyarisi.</summary>
    public static string NeedHammer(int current, int max) =>
        LocalizationManager.Format(LocalizationKey.NeedHammer, current, max);

    /// <summary>Envanter doluluk sayacini formatlar.</summary>
    public static string InventoryCount(int used, int total) =>
        LocalizationManager.Format(LocalizationKey.InventoryCount, used, total);

    /// <summary>Forge geri sayim metnini formatlar.</summary>
    public static string Forging(string timeText) =>
        LocalizationManager.Format(LocalizationKey.Forging, timeText);

    /// <summary>Forge butonu yuzde gosterimi.</summary>
    public static string ForgingPercent(int percent) =>
        LocalizationManager.Format(LocalizationKey.ForgingPercent, percent);

    /// <summary>Forge geri sayimi (kisa surelerde ondalik).</summary>
    public static string FormatForgeCountdown(float remainingSeconds) =>
        LocalizationManager.FormatForgeCountdown(remainingSeconds);

    /// <summary>Secili item detay satirini formatlar.</summary>
    public static string ItemDetail(string itemName, double attack, double sellPrice) =>
        LocalizationManager.Format(LocalizationKey.ItemDetail, itemName, attack, sellPrice);

    /// <summary>Item stat ozetini formatlar.</summary>
    public static string ItemStats(string itemName, double attack, double defense, int tier, double sellPrice) =>
        LocalizationManager.Format(LocalizationKey.ItemStats, itemName, attack, defense, tier, sellPrice);

    /// <summary>Ors bilgi satirini formatlar.</summary>
    public static string AnvilInfo(int level, string era) =>
        LocalizationManager.Format(LocalizationKey.AnvilInfo, level, LocalizationManager.GetEraDisplayName(era));

    /// <summary>Yukseltme devam satirini formatlar.</summary>
    public static string AnvilUpgrading(string duration) =>
        LocalizationManager.Format(LocalizationKey.AnvilUpgrading, Upgrading, duration);

    /// <summary>Yukseltme buton metnini formatlar.</summary>
    public static string UpgradeButtonLabel(double cost, string duration = null) =>
        string.IsNullOrEmpty(duration)
            ? LocalizationManager.Format(LocalizationKey.UpgradeButtonLabel, UpgradeButton, cost)
            : LocalizationManager.Format(LocalizationKey.UpgradeButtonLabelWithDuration, UpgradeButton, cost,
                duration);

    /// <summary>Yukseltme devam buton metnini formatlar.</summary>
    public static string UpgradeInProgressButton(string duration) =>
        LocalizationManager.Format(LocalizationKey.UpgradeInProgressButton, UpgradingButton, duration);

    /// <summary>Yetersiz gold uyarisi.</summary>
    public static string NeedGold(double cost) =>
        LocalizationManager.Format(LocalizationKey.NeedGold, cost);

    /// <summary>Offline karsilama mesaji.</summary>
    public static string OfflineWelcome(double earnedGold, string duration) =>
        LocalizationManager.Format(LocalizationKey.OfflineWelcome, earnedGold, duration);

    /// <summary>Cag adini ekranda gosterilecek metne cevirir.</summary>
    public static string GetEraDisplayName(string era) => LocalizationManager.GetEraDisplayName(era);

    /// <summary>Sureyi kisa metne cevirir.</summary>
    public static string FormatDuration(float totalSeconds) =>
        LocalizationManager.FormatDuration(totalSeconds);

    /// <summary>Offline sureyi kisa metne cevirir.</summary>
    public static string FormatOfflineDuration(double totalSeconds) =>
        LocalizationManager.FormatOfflineDuration(totalSeconds);

    /// <summary>Tech node adini dondurur (nodeId oncelikli; enum kaymasina karsi).</summary>
    public static string GetTechNodeName(TechNodeData node)
    {
        if (node == null) return string.Empty;
        return GetTechNodeName(node.NodeId, node.DisplayNameKey);
    }

    /// <summary>Tech node adini nodeId veya fallback anahtardan cozer.</summary>
    public static string GetTechNodeName(string nodeId, LocalizationKey fallbackKey)
    {
        switch (nodeId)
        {
            case "forge_speed":
                return LocalizationManager.Get(LocalizationKey.TechNodeForgeSpeed);
            case "upgrade_cost":
                return LocalizationManager.Get(LocalizationKey.TechNodeUpgradeCost);
            case "offline_gold":
                return LocalizationManager.Get(LocalizationKey.TechNodeOfflineGold);
            default:
                return LocalizationManager.Get(fallbackKey);
        }
    }

    /// <summary>Tech node seviye etiketi.</summary>
    public static string TechNodeLevel(int current, int max) =>
        LocalizationManager.Format(LocalizationKey.TechNodeLevel, current, max);

    /// <summary>Tech node yukseltme butonu.</summary>
    public static string TechNodeUpgrade(double cost) =>
        LocalizationManager.Format(LocalizationKey.TechNodeUpgrade, cost);

    /// <summary>Tech node yukseltme butonu (sure ile).</summary>
    public static string TechNodeUpgradeWithDuration(double cost, string duration) =>
        LocalizationManager.Format(LocalizationKey.TechNodeUpgradeWithDuration, cost, duration);

    /// <summary>Devam eden arastirma geri sayimi.</summary>
    public static string TechNodeResearching(string duration) =>
        LocalizationManager.Format(LocalizationKey.TechNodeResearching, duration);
}
