using System;
using UnityEngine;

// Forge islemleri icin harcanan cekic meta parasini yonetir; ayarlar HammerConfigProvider uzerinden gelir.
public class HammerManager : MonoBehaviour
{
    [SerializeField] private HammerSettings localSettings;

    private int currentHammers;
    private double nextRegenAtUtc;
    private string lastDailyRefillDateUtc = string.Empty;

    private HammerSettingsData Config => HammerConfigProvider.Active;

    /// <summary>O anki cekic sayisi.</summary>
    public int CurrentHammers => currentHammers;

    /// <summary>Maksimum cekic kapasitesi.</summary>
    public int MaxHammers => Config.maxHammers;

    /// <summary>Tek forge isleminin cekic maliyeti.</summary>
    public int ForgeHammerCost => Config.forgeHammerCost;

    /// <summary>Forge baslatmak icin yeterli cekic var mi.</summary>
    public bool HasEnoughForForge => currentHammers >= Config.forgeHammerCost;

    /// <summary>Cekic miktari veya yenilenme durumu degistiginde tetiklenir.</summary>
    public event Action OnHammersChanged;

    private void Awake()
    {
        EnsureConfig();
        currentHammers = Mathf.Clamp(Config.startingHammers, 0, Config.maxHammers);
        SyncRegenAnchor();
    }

    private void OnEnable()
    {
        HammerConfigProvider.OnConfigChanged += HandleConfigChanged;
    }

    private void OnDisable()
    {
        HammerConfigProvider.OnConfigChanged -= HandleConfigChanged;
    }

    private void Update()
    {
        bool changed = TryApplyDailyRefill() | TryApplyRegen();
        if (changed)
            OnHammersChanged?.Invoke();
    }

    /// <summary>Yerel SO referansini baglar ve config saglayiciyi baslatir.</summary>
    public void Configure(HammerSettings settings)
    {
        localSettings = settings;
        EnsureConfig();
        ClampToConfigLimits();
        OnHammersChanged?.Invoke();
    }

    /// <summary>Kayit yuklerken cekic durumunu ayarlar.</summary>
    public void LoadState(int hammers, double nextRegenAt, string dailyRefillDate)
    {
        EnsureConfig();
        currentHammers = Mathf.Clamp(hammers, 0, Config.maxHammers);
        nextRegenAtUtc = nextRegenAt;
        lastDailyRefillDateUtc = dailyRefillDate ?? string.Empty;

        if (currentHammers >= Config.maxHammers)
            nextRegenAtUtc = 0;

        TryApplyDailyRefill();
        OnHammersChanged?.Invoke();
    }

    /// <summary>Kayit icin mevcut cekic durumunu disari aktarir.</summary>
    public void ExportState(out int hammers, out double nextRegenAt, out string dailyRefillDate)
    {
        hammers = currentHammers;
        nextRegenAt = nextRegenAtUtc;
        dailyRefillDate = lastDailyRefillDateUtc;
    }

    /// <summary>Cikis sonrasi gecen sureye gore cekic yenilenmesini hesaplar.</summary>
    public void ProcessOfflineTime(double lastQuitTimestamp)
    {
        if (lastQuitTimestamp <= 0) return;

        TryApplyDailyRefill();

        double now = GetUnixTimeNow();
        if (now <= lastQuitTimestamp) return;

        ApplyRegenUntil(now);
        OnHammersChanged?.Invoke();
    }

    /// <summary>Forge maliyeti kadar cekic harcar; yetersizse false.</summary>
    public bool TrySpendForForge()
    {
        if (!HasEnoughForForge) return false;

        currentHammers -= Config.forgeHammerCost;

        if (currentHammers < Config.maxHammers && nextRegenAtUtc <= 0)
            nextRegenAtUtc = GetUnixTimeNow() + Config.regenIntervalSeconds;

        OnHammersChanged?.Invoke();
        return true;
    }

    /// <summary>Test veya odul icin cekic ekler (maksimuma kadar).</summary>
    public void AddHammers(int amount)
    {
        if (amount <= 0) return;

        int before = currentHammers;
        currentHammers = Mathf.Min(Config.maxHammers, currentHammers + amount);

        if (currentHammers >= Config.maxHammers)
            nextRegenAtUtc = 0;
        else if (before >= Config.maxHammers)
            nextRegenAtUtc = GetUnixTimeNow() + Config.regenIntervalSeconds;

        OnHammersChanged?.Invoke();
    }

    /// <summary>Debug veya kayit yukleme icin cekic sayisini dogrudan ayarlar.</summary>
    public void SetHammers(int amount)
    {
        currentHammers = Mathf.Clamp(amount, 0, Config.maxHammers);
        SyncRegenAnchor();
        OnHammersChanged?.Invoke();
    }

    /// <summary>Sonraki cekice kalan saniye; kapasite doluysa 0.</summary>
    public float GetSecondsUntilNextHammer()
    {
        if (currentHammers >= Config.maxHammers || nextRegenAtUtc <= 0) return 0f;

        double remaining = nextRegenAtUtc - GetUnixTimeNow();
        return remaining > 0 ? (float)remaining : 0f;
    }

    private void HandleConfigChanged()
    {
        ClampToConfigLimits();
        OnHammersChanged?.Invoke();
    }

    private void EnsureConfig()
    {
        HammerConfigProvider.Initialize(localSettings);
    }

    private void ClampToConfigLimits()
    {
        currentHammers = Mathf.Clamp(currentHammers, 0, Config.maxHammers);

        if (currentHammers >= Config.maxHammers)
            nextRegenAtUtc = 0;
    }

    private bool TryApplyDailyRefill()
    {
        if (!Config.dailyFullRefillEnabled) return false;

        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (lastDailyRefillDateUtc == today) return false;

        if (string.IsNullOrEmpty(lastDailyRefillDateUtc))
        {
            lastDailyRefillDateUtc = today;
            return false;
        }

        lastDailyRefillDateUtc = today;
        currentHammers = Config.maxHammers;
        nextRegenAtUtc = 0;
        return true;
    }

    private bool TryApplyRegen()
    {
        if (currentHammers >= Config.maxHammers) return false;
        return ApplyRegenUntil(GetUnixTimeNow());
    }

    private bool ApplyRegenUntil(double targetTimeUtc)
    {
        if (currentHammers >= Config.maxHammers) return false;

        float interval = Config.regenIntervalSeconds;
        bool changed = false;

        if (nextRegenAtUtc <= 0)
        {
            nextRegenAtUtc = targetTimeUtc + interval;
            return false;
        }

        while (currentHammers < Config.maxHammers && targetTimeUtc >= nextRegenAtUtc)
        {
            currentHammers++;
            changed = true;

            if (currentHammers >= Config.maxHammers)
            {
                nextRegenAtUtc = 0;
                break;
            }

            nextRegenAtUtc += interval;
        }

        return changed;
    }

    private void SyncRegenAnchor()
    {
        if (currentHammers >= Config.maxHammers)
            nextRegenAtUtc = 0;
        else if (nextRegenAtUtc <= 0)
            nextRegenAtUtc = GetUnixTimeNow() + Config.regenIntervalSeconds;
    }

    private static double GetUnixTimeNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
