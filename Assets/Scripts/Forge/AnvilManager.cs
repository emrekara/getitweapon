using UnityEngine;

// Anvil (ors) seviyesi; forge suresi ve cag bilgisini yonetir.
public class AnvilManager : MonoBehaviour
{
    [SerializeField] private int anvilLevel = 1;
    [SerializeField] private float baseForgeDuration = 3f;
    [SerializeField] private float durationReductionPerLevel = 0.25f;
    [SerializeField] private double baseUpgradeCost = 25;
    [SerializeField] private double baseOfflineGoldPerSecond = 1;
    [SerializeField] private double goldPerSecondPerLevel = 0.5;

    /// <summary>Mevcut anvil seviyesi.</summary>
    public int AnvilLevel => anvilLevel;

    /// <summary>Seviyeye gore cag adi.</summary>
    public string CurrentEra => GetEraForLevel(anvilLevel);

    /// <summary>Anvil seviyesine gore forge suresi (saniye).</summary>
    public float GetForgeDuration()
    {
        float duration = baseForgeDuration - (anvilLevel - 1) * durationReductionPerLevel;
        return Mathf.Max(1f, duration);
    }

    /// <summary>Sonraki seviye yukseltme maliyeti.</summary>
    public double GetUpgradeCost()
    {
        return baseUpgradeCost * anvilLevel;
    }

    /// <summary>Gold harcayarak anvil seviyesini artirir.</summary>
    public bool TryUpgrade(EconomyManager economyManager)
    {
        if (economyManager == null) return false;

        double cost = GetUpgradeCost();
        if (!economyManager.TrySpendGold(cost)) return false;

        anvilLevel++;
        return true;
    }

    /// <summary>Kayit yuklerken anvil seviyesini ayarlar.</summary>
    public void SetAnvilLevel(int level)
    {
        anvilLevel = Mathf.Max(1, level);
    }

    /// <summary>Anvil seviyesi ve caga gore offline gold/s oranini dondurur.</summary>
    public double GetOfflineGoldPerSecond()
    {
        double levelBonus = baseOfflineGoldPerSecond + (anvilLevel - 1) * goldPerSecondPerLevel;
        return levelBonus * GetEraGoldMultiplier(anvilLevel);
    }

    private static string GetEraForLevel(int level)
    {
        if (level < 5) return "Stone";
        if (level < 10) return "Medieval";
        if (level < 15) return "Modern";
        return "Space";
    }

    private static double GetEraGoldMultiplier(int level)
    {
        if (level < 5) return 1;    // Stone
        if (level < 10) return 1.5; // Medieval
        if (level < 15) return 2;   // Modern
        return 2.5;                 // Space
    }
}
