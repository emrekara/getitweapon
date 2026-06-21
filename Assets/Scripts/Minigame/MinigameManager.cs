using System;
using UnityEngine;

// Forge beklerken sans mini oyunu; toplam puan kaydi.
public class MinigameManager : MonoBehaviour
{
    [SerializeField] private MinigameSettings localSettings;
    [SerializeField] private SaveManager saveManager;

    private int totalScore;
    private bool canPlayThisForge;

    private MinigameSettingsData Config => MinigameConfigProvider.Active;

    /// <summary>Toplam sans puani.</summary>
    public int TotalScore => totalScore;

    /// <summary>Bu forge turunda oynanabilir mi.</summary>
    public bool CanPlayThisForge => canPlayThisForge;

    /// <summary>Puan veya oynanabilirlik degistiginde.</summary>
    public event Action OnMinigameStateChanged;

    private void Awake()
    {
        EnsureConfig();
    }

    public void Configure(MinigameSettings settings, SaveManager save)
    {
        localSettings = settings;
        saveManager = save;
        EnsureConfig();
        OnMinigameStateChanged?.Invoke();
    }

    public void LoadState(int score)
    {
        totalScore = Math.Max(0, score);
        OnMinigameStateChanged?.Invoke();
    }

    public int ExportScore() => totalScore;

    /// <summary>Forge basladiginda cagrilir.</summary>
    public void NotifyForgeStarted()
    {
        canPlayThisForge = true;
        OnMinigameStateChanged?.Invoke();
    }

    /// <summary>Forge bittiginde cagrilir.</summary>
    public void NotifyForgeEnded()
    {
        canPlayThisForge = false;
        OnMinigameStateChanged?.Invoke();
    }

    /// <summary>Sans turu oynar; bu forge'ta zaten oynandiysa -1.</summary>
    public int TryPlayLuckRound()
    {
        if (!canPlayThisForge)
            return -1;

        canPlayThisForge = false;
        int points = UnityEngine.Random.Range(Config.minPointsPerPlay, Config.maxPointsPerPlay + 1);
        totalScore += points;
        OnMinigameStateChanged?.Invoke();
        saveManager?.SaveGame();
        return points;
    }

    private void EnsureConfig()
    {
        MinigameConfigProvider.Initialize(localSettings);
    }
}
