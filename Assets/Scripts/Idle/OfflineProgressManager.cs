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
    [SerializeField] private float offlineMessageDurationSeconds = 3.5f;

    private Coroutine hideMessageCoroutine;

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

        string timeLabel = GameTexts.FormatOfflineDuration(elapsed);
        string message = GameTexts.OfflineWelcome(earnedGold, timeLabel);

        if (offlineMessageText != null)
        {
            offlineMessageText.text = message;

            if (hideMessageCoroutine != null)
                StopCoroutine(hideMessageCoroutine);

            hideMessageCoroutine = StartCoroutine(HideOfflineMessageAfterDelay());
        }

        // Ayni offline surenin tekrar odememesi icin hemen kaydet
        if (saveManager != null)
            saveManager.SaveGame();

        Debug.Log($"[Offline] {message}");
    }

    /// <summary>Offline mesajini birkac saniye sonra ekrandan kaldirir.</summary>
    private IEnumerator HideOfflineMessageAfterDelay()
    {
        yield return new WaitForSeconds(offlineMessageDurationSeconds);

        if (offlineMessageText != null)
            offlineMessageText.text = string.Empty;

        hideMessageCoroutine = null;
    }
}
