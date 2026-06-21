using System.Collections.Generic;

/// <summary>
/// Turkce metin tablosu.
/// </summary>
public static class TurkishLocalization
{
    /// <summary>Tum Turkce ceviriler.</summary>
    public static readonly Dictionary<LocalizationKey, string> Table = new Dictionary<LocalizationKey, string>
    {
        { LocalizationKey.ForgeButton, "DÖV" },
        { LocalizationKey.SellButton, "SAT" },
        { LocalizationKey.UpgradeButton, "YÜKSELT" },

        { LocalizationKey.AutoForgeOn, "OTO DÖV: AÇIK" },
        { LocalizationKey.AutoForgeOff, "OTO DÖV: KAPALI" },
        { LocalizationKey.AutoSellOn, "OTO SAT: AÇIK" },
        { LocalizationKey.AutoSellOff, "OTO SAT: KAPALI" },

        { LocalizationKey.AutoSellTierFilterOn, "TİER FİLTRE: Sev.{0}–{1} · ≤Sev.{2}" },
        { LocalizationKey.AutoSellTierFilterOff, "TİER FİLTRE: KAPALI" },
        { LocalizationKey.AutoSellEraFilterOn, "ÇAĞ FİLTRE: {0}–{1} · ≤{2}" },
        { LocalizationKey.AutoSellEraFilterOff, "ÇAĞ FİLTRE: KAPALI" },
        { LocalizationKey.FilterRangePosition, "({0}/{1})" },
        { LocalizationKey.AutoSoldFeedback, "OTO SAT: +{0:0}g" },
        { LocalizationKey.ItemAddedToInventory, "Envantere eklendi: {0}" },

        { LocalizationKey.InventoryFull, "Envanter dolu!" },
        { LocalizationKey.ItemAlreadyInInventory, "Zaten envanterde: {0}" },
        { LocalizationKey.ItemCategoryAlreadyInInventory, "Zaten {0} var" },
        { LocalizationKey.ItemCategoryWeapon, "Silah" },
        { LocalizationKey.ItemCategoryArmor, "Kıyafet" },
        { LocalizationKey.ItemCategoryEarring, "Küpe" },
        { LocalizationKey.ItemCategoryNecklace, "Kolye" },
        { LocalizationKey.SelectFromInventory, "Envanterden bir item seç" },
        { LocalizationKey.InventoryEmpty, "Envanter boş — slot doldurmak için döv" },

        { LocalizationKey.PromptBetterItem, "Daha iyi item bulundu! Tut veya sat?" },
        { LocalizationKey.PromptInventoryFullSame, "Envanter dolu — aynı statlar. Birini değiştir?" },
        { LocalizationKey.PromptNewItem, "YENİ" },
        { LocalizationKey.PromptBestInInventory, "ENVANTERDEKİ EN İYİ" },
        { LocalizationKey.PromptKeep, "TUT (zayıfları sat)" },
        { LocalizationKey.PromptSellNew, "YENİSİNİ SAT" },

        { LocalizationKey.Upgrading, "Yükseltiliyor..." },
        { LocalizationKey.UpgradingButton, "YÜKSELTİLİYOR" },

        { LocalizationKey.GoldAmount, "{0:0} Altın" },
        { LocalizationKey.InventoryCount, "Envanter {0}/{1}" },
        { LocalizationKey.Forging, "Dövülüyor... {0}" },
        { LocalizationKey.ItemDetail, "{0}  ·  SAL {1:0}  ·  Satış {2:0}g" },
        { LocalizationKey.ItemStats, "{0}\nSAL {1:0}  SAV {2:0}  Sev. {3}  Satış {4:0}g" },
        { LocalizationKey.AnvilInfo, "Örs Sv.{0} - {1}" },
        { LocalizationKey.AnvilUpgrading, "{0} {1}" },
        { LocalizationKey.UpgradeButtonLabel, "{0} ({1:0}g)" },
        { LocalizationKey.UpgradeButtonLabelWithDuration, "{0} ({1:0}g, {2})" },
        { LocalizationKey.UpgradeInProgressButton, "{0} {1}" },
        { LocalizationKey.NeedGold, "Gerekli: {0:0}g" },
        { LocalizationKey.OfflineWelcome, "Tekrar hoş geldin! +{0:0} altın ({1} çevrimdışı)" },

        { LocalizationKey.EraStone, "Taş" },
        { LocalizationKey.EraMedieval, "Orta Çağ" },
        { LocalizationKey.EraModern, "Modern" },
        { LocalizationKey.EraSpace, "Uzay" },

        { LocalizationKey.DurationSeconds, "{0} sn" },
        { LocalizationKey.DurationSecondsDecimal, "{0:0.1} sn" },
        { LocalizationKey.DurationMinutes, "{0} dk" },
        { LocalizationKey.DurationMinutesSeconds, "{0} dk {1} sn" },
        { LocalizationKey.DurationHours, "{0} sa" },
        { LocalizationKey.DurationHoursMinutes, "{0} sa {1} dk" },

        { LocalizationKey.TechTreeTitle, "ARAŞTIRMA" },
        { LocalizationKey.PanelClose, "KAPAT" },
        { LocalizationKey.TechNodeForgeSpeed, "Hızlı Dövme" },
        { LocalizationKey.TechNodeUpgradeCost, "Ucuz Yükseltme" },
        { LocalizationKey.TechNodeOfflineGold, "Offline Altın" },
        { LocalizationKey.TechNodeLevel, "Sv.{0}/{1}" },
        { LocalizationKey.TechNodeUpgrade, "ARAŞTIR ({0:0}g)" },
        { LocalizationKey.TechNodeUpgradeWithDuration, "ARAŞTIR ({0:0}g, {1})" },
        { LocalizationKey.TechNodeResearching, "ARAŞTIRILIYOR {0}" },
        { LocalizationKey.TechNodeMaxLevel, "MAKS" },
        { LocalizationKey.TechNodeLocked, "Kilitli" },

        { LocalizationKey.ForgingPercent, "DÖV %{0}" }
    };
}
