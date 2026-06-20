using System;
using UnityEngine;

// Anvil (ors) seviyesi; forge suresi, cag ve upgrade timer yonetir.
public class AnvilManager : MonoBehaviour
{
    [SerializeField] private AnvilUpgradeSettings upgradeSettings;
    [SerializeField] private int anvilLevel = 1;
    [SerializeField] private float baseForgeDuration = 3f;
    [SerializeField] private float durationReductionPerLevel = 0.25f;
    [SerializeField] private double baseUpgradeCost = 25;
    [SerializeField] private double baseOfflineGoldPerSecond = 1;
    [SerializeField] private double goldPerSecondPerLevel = 0.5;
    [SerializeField] private double sellPriceMultiplierPerLevel = 0.5;

    // NOT: Simdilik cihaz UTC; ileride sunucu zamani kullanilacak.
    private double upgradeEndsAtUtc;

    /// <summary>Mevcut anvil seviyesi.</summary>
    public int AnvilLevel => anvilLevel;

    /// <summary>Seviyeye gore cag adi.</summary>
    public string CurrentEra => GetEraForLevel(anvilLevel);

    /// <summary>Config'te timer sistemi acik mi.</summary>
    public bool IsUpgradeTimerSystemEnabled =>
        upgradeSettings != null && upgradeSettings.UseUpgradeTimer;

    /// <summary>Upgrade timer aktif mi (henuz bitmemis).</summary>
    public bool IsUpgradeInProgress => GetRemainingUpgradeSeconds() > 0f;

    /// <summary>Upgrade baslatildi ama henuz tamamlanmadi.</summary>
    public bool HasPendingUpgrade => upgradeEndsAtUtc > 0;

    /// <summary>Kayit icin upgrade bitis zamani (Unix saniye); 0 = yok.</summary>
    public double UpgradeEndsAtUtc => upgradeEndsAtUtc;

    /// <summary>Anvil seviyesi degistiginde tetiklenir (UI yenileme icin).</summary>
    public event Action OnAnvilLevelChanged;

    /// <summary>Upgrade basladiginda veya bittiginde tetiklenir.</summary>
    public event Action OnUpgradeTimerChanged;

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

    /// <summary>Mevcut seviye icin upgrade suresi (saniye); 0 = aninda.</summary>
    public float GetUpgradeDurationSeconds()
    {
        if (upgradeSettings == null) return 0f;
        return upgradeSettings.GetUpgradeDurationSeconds(anvilLevel);
    }

    /// <summary>Kalan upgrade suresi (saniye); upgrade yoksa 0.</summary>
    public float GetRemainingUpgradeSeconds()
    {
        if (upgradeEndsAtUtc <= 0) return 0f;

        double remaining = upgradeEndsAtUtc - GetUnixTimeNow();
        return remaining > 0 ? (float)remaining : 0f;
    }

    /// <summary>Gold harcayarak upgrade baslatir; config'e gore aninda veya timer ile tamamlanir.</summary>
    public bool TryStartUpgrade(EconomyManager economyManager)
    {
        if (economyManager == null || IsUpgradeInProgress || HasPendingUpgrade) return false;

        double cost = GetUpgradeCost();
        if (!economyManager.TrySpendGold(cost)) return false;

        float duration = GetUpgradeDurationSeconds();
        if (duration <= 0f)
        {
            anvilLevel++;
            NotifyAnvilLevelChanged();
            return true;
        }

        upgradeEndsAtUtc = GetUnixTimeNow() + duration;
        NotifyUpgradeTimerChanged();
        return true;
    }

    /// <summary>Timer bittiyse seviyeyi artirir.</summary>
    public bool TryCompleteUpgradeIfReady()
    {
        if (upgradeEndsAtUtc <= 0) return false;
        if (GetUnixTimeNow() < upgradeEndsAtUtc) return false;

        upgradeEndsAtUtc = 0;
        anvilLevel++;
        NotifyUpgradeTimerChanged();
        NotifyAnvilLevelChanged();
        return true;
    }

    /// <summary>Kayit yuklerken seviye ve upgrade timer durumunu uygular.</summary>
    public void LoadState(int level, double upgradeEndsAt)
    {
        anvilLevel = Mathf.Max(1, level);
        upgradeEndsAtUtc = upgradeEndsAt > 0 ? upgradeEndsAt : 0;

        if (TryCompleteUpgradeIfReady()) return;

        if (HasPendingUpgrade)
            NotifyUpgradeTimerChanged();
    }

    /// <summary>Kayit yuklerken yalnizca seviye ayarlar (timer sifirlanir).</summary>
    public void SetAnvilLevel(int level)
    {
        int clampedLevel = Mathf.Max(1, level);
        bool levelChanged = anvilLevel != clampedLevel;
        bool hadTimer = HasPendingUpgrade;

        anvilLevel = clampedLevel;
        upgradeEndsAtUtc = 0;

        if (hadTimer)
            NotifyUpgradeTimerChanged();

        if (levelChanged)
            NotifyAnvilLevelChanged();
    }

    private void NotifyAnvilLevelChanged()
    {
        OnAnvilLevelChanged?.Invoke();
    }

    private void NotifyUpgradeTimerChanged()
    {
        OnUpgradeTimerChanged?.Invoke();
    }

    /// <summary>Anvil seviyesi ve caga gore offline gold/s oranini dondurur.</summary>
    public double GetOfflineGoldPerSecond()
    {
        double levelBonus = baseOfflineGoldPerSecond + (anvilLevel - 1) * goldPerSecondPerLevel;
        return levelBonus * GetEraGoldMultiplier(anvilLevel);
    }

    /// <summary>Item baz satis fiyatina uygulanan anvil carpani.</summary>
    public double GetSellPriceMultiplier()
    {
        double levelBonus = 1 + (anvilLevel - 1) * sellPriceMultiplierPerLevel;
        return levelBonus * GetEraGoldMultiplier(anvilLevel);
    }

    /// <summary>Anvil seviyesine gore olceklenmis satis fiyatini dondurur.</summary>
    public double GetScaledSellPrice(double baseSellPrice)
    {
        return baseSellPrice * GetSellPriceMultiplier();
    }

    private static double GetUnixTimeNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
