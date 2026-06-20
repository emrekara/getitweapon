using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Oyun acilisinda offline gecen sureye gore pasif gold ekler.
public class OfflineProgressManager : MonoBehaviour
{
    private const string SaveKey = "GetItWeapon_Save";

    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private TextMeshProUGUI offlineMessageText;
    [SerializeField] private double maxOfflineSeconds = 28800; // 8 saat

    private void Start()
    {
        if (offlineMessageText != null)
            offlineMessageText.text = string.Empty;

        StartCoroutine(ApplyOfflineProgressAfterLoad());
    }

    private IEnumerator ApplyOfflineProgressAfterLoad()
    {
        // SaveManager once yuklemesini bekler
        yield return null;

        if (economyManager == null) yield break;
        if (!PlayerPrefs.HasKey(SaveKey)) yield break;

        GameSaveData data = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(SaveKey));
        if (data == null || data.lastQuitTimestamp <= 0) yield break;

        double now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        double elapsed = now - data.lastQuitTimestamp;
        if (elapsed <= 1) yield break;

        elapsed = Math.Min(elapsed, maxOfflineSeconds);
        double goldPerSecond = anvilManager != null ? anvilManager.GetOfflineGoldPerSecond() : 1;
        double earnedGold = elapsed * goldPerSecond;
        if (earnedGold <= 0) yield break;

        economyManager.AddGold(earnedGold);

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();

        string timeLabel = FormatOfflineDuration(elapsed);
        string message = $"Welcome back! +{earnedGold:0} gold ({timeLabel} offline)";

        if (offlineMessageText != null)
            offlineMessageText.text = message;

        // Ayni offline surenin tekrar odememesi icin hemen kaydet
        if (saveManager != null)
            saveManager.SaveGame();

        Debug.Log($"[Offline] {message}");
    }

    /// <summary>Offline sureyi okunabilir metne cevirir.</summary>
    private static string FormatOfflineDuration(double totalSeconds)
    {
        int seconds = (int)Math.Floor(totalSeconds);

        if (seconds < 60)
            return $"{seconds}s";

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        if (minutes < 60)
            return remainingSeconds > 0 ? $"{minutes}m {remainingSeconds}s" : $"{minutes}m";

        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return remainingMinutes > 0 ? $"{hours}h {remainingMinutes}m" : $"{hours}h";
    }
}
