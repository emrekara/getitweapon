using System;
using UnityEngine;

// Oyun verisini PlayerPrefs + JSON ile kaydeder ve yukler.
[DefaultExecutionOrder(-200)]
public class SaveManager : MonoBehaviour
{
    private const string SaveKey = "GetItWeapon_Save";

    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ForgeAutomationManager automationManager;

    private void Awake()
    {
        EnsureSystems();
        GameUiBootstrap.EnsureApplied();
    }

    private void Start()
    {
        StartCoroutine(LoadAfterFrame());
    }

    private System.Collections.IEnumerator LoadAfterFrame()
    {
        yield return null;
        GameUiBootstrap.EnsureApplied();
        LoadGame();

        if (automationManager != null && automationManager.AutoForgeEnabled)
            automationManager.NotifyForgeCompleted();
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

    /// <summary>Mevcut gold ve envanter durumunu kaydeder.</summary>
    public void SaveGame()
    {
        if (economyManager == null) return;

        EnsureSystems();

        GameSaveData data = new GameSaveData
        {
            gold = economyManager.CurrentGold,
            lastItemIndex = -1,
            anvilLevel = anvilManager != null ? anvilManager.AnvilLevel : 1,
            anvilUpgradeEndsAt = anvilManager != null ? anvilManager.UpgradeEndsAtUtc : 0,
            lastQuitTimestamp = GetUnixTimeNow(),
            inventoryItemIndices = inventoryManager != null ? inventoryManager.ExportItemIndices() : Array.Empty<int>(),
            selectedInventorySlot = inventoryManager != null ? inventoryManager.ExportSelectedSlot() : -1,
            autoForgeEnabled = automationManager != null && automationManager.ExportAutoForgeEnabled(),
            autoSellEnabled = automationManager != null && automationManager.ExportAutoSellEnabled()
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        EnsureSystems();

        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string json = PlayerPrefs.GetString(SaveKey);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null) return;

        economyManager.SetGold(data.gold);

        if (inventoryManager != null)
        {
            inventoryManager.ImportState(
                data.inventoryItemIndices,
                data.selectedInventorySlot,
                data.lastItemIndex);
        }

        if (anvilManager != null)
            anvilManager.LoadState(data.anvilLevel, data.anvilUpgradeEndsAt);

        if (automationManager != null)
            automationManager.ImportState(data.autoForgeEnabled, data.autoSellEnabled);

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();
    }

    private void EnsureSystems()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (inventoryManager == null)
        {
            inventoryManager = gameObject.AddComponent<InventoryManager>();
        }

        if (itemDatabase == null && forgeButtonHandler != null)
            itemDatabase = forgeButtonHandler.ItemDatabase;

        inventoryManager.Configure(itemDatabase);

        if (automationManager == null)
            automationManager = GetComponent<ForgeAutomationManager>();

        if (automationManager == null)
            automationManager = gameObject.AddComponent<ForgeAutomationManager>();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            if (canvas.GetComponent<InventoryPanelUI>() == null)
                canvas.gameObject.AddComponent<InventoryPanelUI>();

            if (canvas.GetComponent<AutoForgePanelUI>() == null)
                canvas.gameObject.AddComponent<AutoForgePanelUI>();

            if (canvas.GetComponent<ForgeItemPromptUI>() == null)
                canvas.gameObject.AddComponent<ForgeItemPromptUI>();

            if (canvas.GetComponent<GameUILayout>() == null)
                canvas.gameObject.AddComponent<GameUILayout>();
        }
    }

    private static double GetUnixTimeNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
