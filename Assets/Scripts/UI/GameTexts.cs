using UnityEngine;

// Oyuncuya gosterilen tum metinler (simdilik Turkce; ileride i18n buradan yonetilecek).
public static class GameTexts
{
    public const string ForgeButton = "DÖV";
    public const string SellButton = "SAT";
    public const string UpgradeButton = "YÜKSELT";

    public const string AutoForgeOn = "OTO DÖV: AÇIK";
    public const string AutoForgeOff = "OTO DÖV: KAPALI";
    public const string AutoSellOn = "OTO SAT: AÇIK";
    public const string AutoSellOff = "OTO SAT: KAPALI";

    public const string InventoryFull = "Envanter dolu!";
    public const string SelectFromInventory = "Envanterden bir item seç";
    public const string InventoryEmpty = "Envanter boş — slot doldurmak için döv";

    public const string PromptBetterItem = "Daha iyi item bulundu! Tut veya sat?";
    public const string PromptInventoryFullSame = "Envanter dolu — aynı statlar. Birini değiştir?";
    public const string PromptNewItem = "YENİ";
    public const string PromptBestInInventory = "ENVANTERDEKİ EN İYİ";
    public const string PromptKeep = "TUT (zayıfları sat)";
    public const string PromptSellNew = "YENİSİNİ SAT";

    public const string Upgrading = "Yükseltiliyor...";
    public const string UpgradingButton = "YÜKSELTİLİYOR";

    /// <summary>Gold miktarini formatlar.</summary>
    public static string GoldAmount(double gold) => $"{gold:0} Altın";

    /// <summary>Envanter doluluk sayacini formatlar.</summary>
    public static string InventoryCount(int used, int total) => $"Envanter {used}/{total}";

    /// <summary>Forge geri sayim metnini formatlar.</summary>
    public static string Forging(string timeText) => $"Dövülüyor... {timeText}";

    /// <summary>Secili item detay satirini formatlar.</summary>
    public static string ItemDetail(string itemName, double attack, double sellPrice) =>
        $"{itemName}  ·  SAL {attack:0}  ·  Satış {sellPrice:0}g";

    /// <summary>Item stat ozetini formatlar.</summary>
    public static string ItemStats(string itemName, double attack, double defense, int tier, double sellPrice) =>
        $"{itemName}\nSAL {attack:0}  SAV {defense:0}  Sev. {tier}  Satış {sellPrice:0}g";

    /// <summary>Örs bilgi satirini formatlar.</summary>
    public static string AnvilInfo(int level, string era) => $"Örs Sv.{level} - {GetEraDisplayName(era)}";

    /// <summary>Yükseltme devam satirini formatlar.</summary>
    public static string AnvilUpgrading(string duration) => $"{Upgrading} {duration}";

    /// <summary>Yükseltme buton metnini formatlar.</summary>
    public static string UpgradeButtonLabel(double cost, string duration = null) =>
        string.IsNullOrEmpty(duration)
            ? $"{UpgradeButton} ({cost:0}g)"
            : $"{UpgradeButton} ({cost:0}g, {duration})";

    /// <summary>Yükseltme devam buton metnini formatlar.</summary>
    public static string UpgradeInProgressButton(string duration) => $"{UpgradingButton} {duration}";

    /// <summary>Yetersiz gold uyarisi.</summary>
    public static string NeedGold(double cost) => $"Gerekli: {cost:0}g";

    /// <summary>Offline karsilama mesaji.</summary>
    public static string OfflineWelcome(double earnedGold, string duration) =>
        $"Tekrar hoş geldin! +{earnedGold:0} altın ({duration} çevrimdışı)";

    /// <summary>Cag adini ekranda gosterilecek Turkce metne cevirir.</summary>
    public static string GetEraDisplayName(string era)
    {
        switch (era)
        {
            case "Stone": return "Taş";
            case "Medieval": return "Orta Çağ";
            case "Modern": return "Modern";
            case "Space": return "Uzay";
            default: return era;
        }
    }

    /// <summary>Sureyi kisa Turkce metne cevirir.</summary>
    public static string FormatDuration(float totalSeconds)
    {
        int seconds = Mathf.Max(0, Mathf.CeilToInt(totalSeconds));

        if (seconds < 60)
            return $"{seconds} sn";

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        if (minutes < 60)
            return remainingSeconds > 0 ? $"{minutes} dk {remainingSeconds} sn" : $"{minutes} dk";

        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return remainingMinutes > 0 ? $"{hours} sa {remainingMinutes} dk" : $"{hours} sa";
    }

    /// <summary>Offline sureyi kisa Turkce metne cevirir.</summary>
    public static string FormatOfflineDuration(double totalSeconds)
    {
        int seconds = (int)System.Math.Floor(totalSeconds);

        if (seconds < 60)
            return $"{seconds} sn";

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        if (minutes < 60)
            return remainingSeconds > 0 ? $"{minutes} dk {remainingSeconds} sn" : $"{minutes} dk";

        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return remainingMinutes > 0 ? $"{hours} sa {remainingMinutes} dk" : $"{hours} sa";
    }
}
