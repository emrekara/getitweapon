using System;

/// <summary>
/// Aktif cekic ekonomisi ayarlarini saglar. Simdilik yerel SO; ileride API remote config buraya yazilir.
/// </summary>
public static class HammerConfigProvider
{
    private static HammerSettingsData active = new HammerSettingsData();
    private static bool isInitialized;

    /// <summary>Oyunun kullandigi guncel cekic ayarlari.</summary>
    public static HammerSettingsData Active
    {
        get
        {
            if (!isInitialized)
                Initialize(null);

            return active;
        }
    }

    /// <summary>Ayarlar degistiginde (remote config vb.) tetiklenir.</summary>
    public static event Action OnConfigChanged;

    /// <summary>Yerel ScriptableObject ile baslatir; SO yoksa kod varsayilanini kullanir.</summary>
    public static void Initialize(HammerSettings localSettings)
    {
        if (localSettings != null)
            active = localSettings.Values.Clone();
        else
            active = new HammerSettingsData();

        active.Sanitize();
        isInitialized = true;
    }

    /// <summary>
    /// API / remote config cevabini uygular.
    /// Ornek: sunucudan JsonUtility.FromJson&lt;HammerSettingsData&gt;(json) sonrasi bu metot cagrilir.
    /// </summary>
    public static void ApplyRemoteConfig(HammerSettingsData remote)
    {
        if (remote == null) return;

        active = remote.Clone();
        active.Sanitize();
        isInitialized = true;
        OnConfigChanged?.Invoke();
    }

    /// <summary>Test ve editor icin aktif ayarlari sifirlar.</summary>
    public static void ResetForTests()
    {
        active = new HammerSettingsData();
        active.Sanitize();
        isInitialized = false;
        OnConfigChanged?.Invoke();
    }
}
