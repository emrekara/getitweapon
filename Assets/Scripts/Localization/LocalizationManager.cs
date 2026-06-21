using System;
using System.Collections.Generic;

/// <summary>
/// Aktif dil tablosunu yukler; metin cozumlemesi ve dil degisimi saglar.
/// </summary>
public static class LocalizationManager
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<LocalizationKey, string>> Tables =
        new Dictionary<string, Dictionary<LocalizationKey, string>>
        {
            { "tr", TurkishLocalization.Table },
            { "en", EnglishLocalization.Table }
        };

    private static string currentLanguage = DefaultLanguage;

    /// <summary>Dil degistiginde UI yenileme icin tetiklenir.</summary>
    public static event Action OnLanguageChanged;

    /// <summary>Aktif dil kodu (tr, en).</summary>
    public static string CurrentLanguage => currentLanguage;

    /// <summary>Kayit veya varsayilan dil ile baslatir.</summary>
    /// <param name="languageCode">tr veya en; bos ise varsayilan tr.</param>
    public static void Initialize(string languageCode = null)
    {
        SetLanguage(string.IsNullOrEmpty(languageCode) ? DefaultLanguage : languageCode);
    }

    /// <summary>Aktif dili degistirir.</summary>
    /// <param name="languageCode">tr veya en.</param>
    public static void SetLanguage(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode) || !Tables.ContainsKey(languageCode))
            languageCode = DefaultLanguage;

        if (currentLanguage == languageCode && Tables.ContainsKey(currentLanguage))
            return;

        currentLanguage = languageCode;
        OnLanguageChanged?.Invoke();
    }

    /// <summary>Anahtar icin cevirilmis metni dondurur.</summary>
    /// <param name="key">Metin anahtari.</param>
    /// <returns>Ceviri veya anahtar adi.</returns>
    public static string Get(LocalizationKey key)
    {
        if (!Tables.TryGetValue(currentLanguage, out Dictionary<LocalizationKey, string> table))
            return key.ToString();

        return table.TryGetValue(key, out string value) ? value : key.ToString();
    }

    /// <summary>Placeholder'li metni formatlar ({0}, {1}...).</summary>
    /// <param name="key">Metin anahtari.</param>
    /// <param name="args">Format argumanlari.</param>
    /// <returns>Formatlanmis metin.</returns>
    public static string Format(LocalizationKey key, params object[] args)
    {
        string template = Get(key);
        return args == null || args.Length == 0 ? template : string.Format(template, args);
    }

    /// <summary>Cag kodunu ekran metnine cevirir.</summary>
    /// <param name="era">Stone, Medieval, Modern, Space.</param>
    /// <returns>Lokalize cag adi.</returns>
    public static string GetEraDisplayName(string era)
    {
        switch (era)
        {
            case "Stone": return Get(LocalizationKey.EraStone);
            case "Medieval": return Get(LocalizationKey.EraMedieval);
            case "Modern": return Get(LocalizationKey.EraModern);
            case "Space": return Get(LocalizationKey.EraSpace);
            default: return era;
        }
    }

    /// <summary>Forge geri sayimi; her zaman ondalikli gosterir (takilmayi onler).</summary>
    public static string FormatForgeCountdown(float totalSeconds)
    {
        float clamped = UnityEngine.Mathf.Max(0f, totalSeconds);

        if (clamped < 60f)
        {
            float tenths = UnityEngine.Mathf.Floor(clamped * 10f + 0.0001f) / 10f;
            return Format(LocalizationKey.DurationSecondsDecimal, tenths);
        }

        return FormatDuration(totalSeconds);
    }

    /// <summary>Saniye cinsinden sureyi kisa metne cevirir.</summary>
    /// <param name="totalSeconds">Toplam saniye.</param>
    /// <returns>Lokalize sure metni.</returns>
    public static string FormatDuration(float totalSeconds)
    {
        int seconds = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.CeilToInt(totalSeconds));

        if (seconds < 60)
            return Format(LocalizationKey.DurationSeconds, seconds);

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        if (minutes < 60)
            return remainingSeconds > 0
                ? Format(LocalizationKey.DurationMinutesSeconds, minutes, remainingSeconds)
                : Format(LocalizationKey.DurationMinutes, minutes);

        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return remainingMinutes > 0
            ? Format(LocalizationKey.DurationHoursMinutes, hours, remainingMinutes)
            : Format(LocalizationKey.DurationHours, hours);
    }

    /// <summary>Offline sureyi kisa metne cevirir.</summary>
    /// <param name="totalSeconds">Toplam saniye.</param>
    /// <returns>Lokalize sure metni.</returns>
    public static string FormatOfflineDuration(double totalSeconds)
    {
        return FormatDuration((float)totalSeconds);
    }
}
