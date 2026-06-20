using System;
using UnityEngine;

/// <summary>Anvil upgrade suresi kurallari; level araligi ve carpana gore timer hesaplar.</summary>
[Serializable]
public class AnvilUpgradeLevelRange
{
    [SerializeField] private int minLevel = 1;
    [SerializeField] private int maxLevel = 4;
    [SerializeField] private float durationMultiplier = 0f;

    /// <summary>Bu kuralin gecerli oldugu minimum anvil seviyesi.</summary>
    public int MinLevel => minLevel;

    /// <summary>Bu kuralin gecerli oldugu maksimum seviye (dahil). 0 = ust sinir yok.</summary>
    public int MaxLevel => maxLevel;

    /// <summary>Base sure carpani; 0 = aninda upgrade.</summary>
    public float DurationMultiplier => durationMultiplier;

    /// <summary>Verilen seviye bu araliga dusuyor mu kontrol eder.</summary>
    public bool ContainsLevel(int level)
    {
        if (level < minLevel) return false;
        if (maxLevel <= 0) return true;
        return level <= maxLevel;
    }
}

/// <summary>Anvil upgrade timer ayarlari; yalnizca aktifken level kurallari uygulanir.</summary>
[CreateAssetMenu(fileName = "AnvilUpgradeSettings", menuName = "GetItWeapon/Anvil Upgrade Settings")]
public class AnvilUpgradeSettings : ScriptableObject
{
    [SerializeField] private bool useUpgradeTimer;
    [SerializeField] private float baseUpgradeDurationSeconds = 30f;
    [SerializeField] private AnvilUpgradeLevelRange[] levelRanges =
    {
        new AnvilUpgradeLevelRange()
    };

    /// <summary>Timer sistemi acik mi; kapaliysa tum upgrade'ler aninda tamamlanir.</summary>
    public bool UseUpgradeTimer => useUpgradeTimer;

    /// <summary>Carpanlarin uygulandigi baz upgrade suresi (saniye).</summary>
    public float BaseUpgradeDurationSeconds => baseUpgradeDurationSeconds;

    /// <summary>Tanimli level araligi kurallari.</summary>
    public AnvilUpgradeLevelRange[] LevelRanges => levelRanges;

    /// <summary>Verilen anvil seviyesi icin upgrade suresini dondurur; 0 = aninda.</summary>
    public float GetUpgradeDurationSeconds(int anvilLevel)
    {
        if (!useUpgradeTimer || anvilLevel < 1) return 0f;

        AnvilUpgradeLevelRange rule = FindRuleForLevel(anvilLevel);
        if (rule == null) return 0f;

        float duration = baseUpgradeDurationSeconds * rule.DurationMultiplier;
        return duration > 0f ? duration : 0f;
    }

    private AnvilUpgradeLevelRange FindRuleForLevel(int anvilLevel)
    {
        if (levelRanges == null || levelRanges.Length == 0) return null;

        for (int i = 0; i < levelRanges.Length; i++)
        {
            if (levelRanges[i] != null && levelRanges[i].ContainsLevel(anvilLevel))
                return levelRanges[i];
        }

        return null;
    }
}
