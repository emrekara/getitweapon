using System;
using UnityEngine;

// Oyun verisini PlayerPrefs + JSON ile kaydeder ve yukler.
public class SaveManager : MonoBehaviour
{
    private const string SaveKey = "GetItWeapon_Save";

    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private AnvilManager anvilManager;

    private void Start()
    {
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // Mobilde uygulama arka plana gecince kaydet
    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            SaveGame();
    }

    /// <summary>Mevcut gold ve son item durumunu kaydeder.</summary>
    public void SaveGame()
    {
        if (economyManager == null || forgeButtonHandler == null) return;

        GameSaveData data = new GameSaveData
        {
            gold = economyManager.CurrentGold,
            lastItemIndex = forgeButtonHandler.GetLastItemIndex(),
            anvilLevel = anvilManager != null ? anvilManager.AnvilLevel : 1,
            lastQuitTimestamp = GetUnixTimeNow()
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string json = PlayerPrefs.GetString(SaveKey);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null) return;

        economyManager.SetGold(data.gold);
        forgeButtonHandler.RestoreLastItem(data.lastItemIndex);

        if (anvilManager != null)
            anvilManager.SetAnvilLevel(data.anvilLevel);

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();
    }

    private static double GetUnixTimeNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
