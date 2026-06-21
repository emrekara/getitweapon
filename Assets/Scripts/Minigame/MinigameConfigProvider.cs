using System;

/// <summary>Aktif mini oyun ayarlari; ileride API buraya yazilir.</summary>
public static class MinigameConfigProvider
{
    private static MinigameSettingsData active = new MinigameSettingsData();
    private static bool isInitialized;

    public static MinigameSettingsData Active
    {
        get
        {
            if (!isInitialized)
                Initialize(null);
            return active;
        }
    }

    public static event Action OnConfigChanged;

    public static void Initialize(MinigameSettings localSettings)
    {
        active = localSettings != null ? localSettings.Values.Clone() : new MinigameSettingsData();
        active.Sanitize();
        isInitialized = true;
    }

    public static void ApplyRemoteConfig(MinigameSettingsData remote)
    {
        if (remote == null) return;
        active = remote.Clone();
        active.Sanitize();
        isInitialized = true;
        OnConfigChanged?.Invoke();
    }
}
