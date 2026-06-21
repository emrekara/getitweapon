using System.Collections.Generic;

/// <summary>
/// English text table.
/// </summary>
public static class EnglishLocalization
{
    /// <summary>All English translations.</summary>
    public static readonly Dictionary<LocalizationKey, string> Table = new Dictionary<LocalizationKey, string>
    {
        { LocalizationKey.ForgeButton, "FORGE" },
        { LocalizationKey.SellButton, "SELL" },
        { LocalizationKey.UpgradeButton, "UPGRADE" },

        { LocalizationKey.AutoForgeOn, "AUTO FORGE: ON" },
        { LocalizationKey.AutoForgeOff, "AUTO FORGE: OFF" },
        { LocalizationKey.AutoSellOn, "AUTO SELL: ON" },
        { LocalizationKey.AutoSellOff, "AUTO SELL: OFF" },

        { LocalizationKey.AutoSellTierFilterOn, "TIER FILTER: T{0}–{1} · ≤T{2}" },
        { LocalizationKey.AutoSellTierFilterOff, "TIER FILTER: OFF" },
        { LocalizationKey.AutoSellEraFilterOn, "ERA FILTER: {0}–{1} · ≤{2}" },
        { LocalizationKey.AutoSellEraFilterOff, "ERA FILTER: OFF" },
        { LocalizationKey.FilterRangePosition, "({0}/{1})" },
        { LocalizationKey.AutoSoldFeedback, "AUTO SELL: +{0:0}g" },
        { LocalizationKey.ItemAddedToInventory, "Added to inventory: {0}" },

        { LocalizationKey.InventoryFull, "Inventory full!" },
        { LocalizationKey.ItemAlreadyInInventory, "Already in inventory: {0}" },
        { LocalizationKey.SelectFromInventory, "Select an item from inventory" },
        { LocalizationKey.InventoryEmpty, "Inventory empty — forge to fill slots" },

        { LocalizationKey.PromptBetterItem, "Better item found! Keep or sell?" },
        { LocalizationKey.PromptInventoryFullSame, "Inventory full — same stats. Replace one?" },
        { LocalizationKey.PromptNewItem, "NEW" },
        { LocalizationKey.PromptBestInInventory, "BEST IN INVENTORY" },
        { LocalizationKey.PromptKeep, "KEEP (sell weaker)" },
        { LocalizationKey.PromptSellNew, "SELL NEW" },

        { LocalizationKey.Upgrading, "Upgrading..." },
        { LocalizationKey.UpgradingButton, "UPGRADING" },

        { LocalizationKey.GoldAmount, "{0:0} Gold" },
        { LocalizationKey.InventoryCount, "Inventory {0}/{1}" },
        { LocalizationKey.Forging, "Forging... {0}" },
        { LocalizationKey.ItemDetail, "{0}  ·  ATK {1:0}  ·  Sell {2:0}g" },
        { LocalizationKey.ItemStats, "{0}\nATK {1:0}  DEF {2:0}  T{3}  Sell {4:0}g" },
        { LocalizationKey.AnvilInfo, "Anvil Lv.{0} - {1}" },
        { LocalizationKey.AnvilUpgrading, "{0} {1}" },
        { LocalizationKey.UpgradeButtonLabel, "{0} ({1:0}g)" },
        { LocalizationKey.UpgradeButtonLabelWithDuration, "{0} ({1:0}g, {2})" },
        { LocalizationKey.UpgradeInProgressButton, "{0} {1}" },
        { LocalizationKey.NeedGold, "Need: {0:0}g" },
        { LocalizationKey.OfflineWelcome, "Welcome back! +{0:0} gold ({1} offline)" },

        { LocalizationKey.EraStone, "Stone" },
        { LocalizationKey.EraMedieval, "Medieval" },
        { LocalizationKey.EraModern, "Modern" },
        { LocalizationKey.EraSpace, "Space" },

        { LocalizationKey.DurationSeconds, "{0}s" },
        { LocalizationKey.DurationSecondsDecimal, "{0:0.1}s" },
        { LocalizationKey.DurationMinutes, "{0}m" },
        { LocalizationKey.DurationMinutesSeconds, "{0}m {1}s" },
        { LocalizationKey.DurationHours, "{0}h" },
        { LocalizationKey.DurationHoursMinutes, "{0}h {1}m" },

        { LocalizationKey.TechTreeTitle, "RESEARCH" },
        { LocalizationKey.TechNodeForgeSpeed, "Fast Forge" },
        { LocalizationKey.TechNodeUpgradeCost, "Cheaper Upgrade" },
        { LocalizationKey.TechNodeOfflineGold, "Offline Gold" },
        { LocalizationKey.TechNodeLevel, "Lv.{0}/{1}" },
        { LocalizationKey.TechNodeUpgrade, "RESEARCH ({0:0}g)" },
        { LocalizationKey.TechNodeUpgradeWithDuration, "RESEARCH ({0:0}g, {1})" },
        { LocalizationKey.TechNodeResearching, "RESEARCHING {0}" },
        { LocalizationKey.TechNodeMaxLevel, "MAX" },
        { LocalizationKey.TechNodeLocked, "Locked" },

        { LocalizationKey.ForgingPercent, "FORGE {0}%" }
    };
}
