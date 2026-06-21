using System;
using UnityEngine;

// Oyun verisini PlayerPrefs + JSON ile kaydeder ve yukler.
[DefaultExecutionOrder(-200)]
public class SaveManager : MonoBehaviour
{
    private const string SaveKey = "GetItWeapon_Save";

    [SerializeField] private HammerSettings hammerSettings;
    [SerializeField] private MinigameSettings minigameSettings;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private HammerManager hammerManager;
    [SerializeField] private MinigameManager minigameManager;
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private HammerDisplayUI hammerDisplayUI;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ForgeAutomationManager automationManager;
    [SerializeField] private TechTreeManager techTreeManager;
    [SerializeField] private TechTreeDatabase techTreeDatabase;

    private void Awake()
    {
        LocalizationManager.Initialize();
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
            hammers = hammerManager != null ? hammerManager.CurrentHammers : 0,
            hammerNextRegenAt = 0,
            hammerLastDailyRefillDate = string.Empty,
            lastItemIndex = -1,
            anvilLevel = anvilManager != null ? anvilManager.AnvilLevel : 1,
            anvilUpgradeEndsAt = anvilManager != null ? anvilManager.UpgradeEndsAtUtc : 0,
            lastQuitTimestamp = GetUnixTimeNow(),
            inventoryItemIndices = inventoryManager != null ? inventoryManager.ExportItemIndices() : Array.Empty<int>(),
            selectedInventorySlot = inventoryManager != null ? inventoryManager.ExportSelectedSlot() : -1,
            autoForgeEnabled = automationManager != null && automationManager.ExportAutoForgeEnabled(),
            autoSellEnabled = automationManager != null && automationManager.ExportAutoSellEnabled(),
            autoSellTierFilterEnabled = automationManager != null && automationManager.ExportAutoSellTierFilterEnabled(),
            autoSellMaxTier = automationManager != null ? automationManager.ExportAutoSellMaxTier() : 1,
            autoSellEraFilterEnabled = automationManager != null && automationManager.ExportAutoSellEraFilterEnabled(),
            autoSellMaxEraIndex = automationManager != null ? automationManager.ExportAutoSellMaxEraIndex() : 0,
            languageCode = LocalizationManager.CurrentLanguage,
            techNodeLevels = techTreeManager != null ? techTreeManager.ExportState() : Array.Empty<TechNodeSaveEntry>(),
            techResearchNodeId = techTreeManager != null ? techTreeManager.ExportResearchNodeId() : string.Empty,
            techResearchEndsAt = techTreeManager != null ? techTreeManager.ExportResearchEndsAt() : 0,
            minigameTotalScore = minigameManager != null ? minigameManager.ExportScore() : 0
        };

        if (hammerManager != null)
        {
            hammerManager.ExportState(out data.hammers, out data.hammerNextRegenAt, out data.hammerLastDailyRefillDate);
        }

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

        LocalizationManager.Initialize(data.languageCode);
        economyManager.SetGold(data.gold);

        if (hammerManager != null)
        {
            bool hasHammerSaveData = data.hammers > 0 ||
                                     data.hammerNextRegenAt > 0 ||
                                     !string.IsNullOrEmpty(data.hammerLastDailyRefillDate);

            if (hasHammerSaveData)
            {
                hammerManager.LoadState(
                    data.hammers,
                    data.hammerNextRegenAt,
                    data.hammerLastDailyRefillDate);
            }

            hammerManager.ProcessOfflineTime(data.lastQuitTimestamp);
        }

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
        {
            automationManager.ImportState(
                data.autoForgeEnabled,
                data.autoSellEnabled,
                data.autoSellTierFilterEnabled,
                data.autoSellMaxTier,
                data.autoSellEraFilterEnabled,
                data.autoSellMaxEraIndex);
        }

        if (techTreeManager != null)
        {
            techTreeManager.ImportState(
                data.techNodeLevels,
                data.techResearchNodeId,
                data.techResearchEndsAt);
        }

        if (minigameManager != null)
            minigameManager.LoadState(data.minigameTotalScore);

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();

        if (hammerDisplayUI == null)
            hammerDisplayUI = FindFirstObjectByType<HammerDisplayUI>();

        if (hammerDisplayUI != null)
            hammerDisplayUI.RefreshDisplay();
    }

    private void EnsureSystems()
    {
        if (hammerManager == null)
            hammerManager = FindFirstObjectByType<HammerManager>();

        if (hammerManager == null && economyManager != null)
            hammerManager = economyManager.gameObject.AddComponent<HammerManager>();
        else if (hammerManager == null)
            hammerManager = gameObject.AddComponent<HammerManager>();

        if (hammerSettings == null)
            hammerSettings = Resources.Load<HammerSettings>("MainHammerSettings");

        hammerManager.Configure(hammerSettings);

        if (minigameManager == null)
            minigameManager = FindFirstObjectByType<MinigameManager>();

        if (minigameManager == null)
            minigameManager = gameObject.AddComponent<MinigameManager>();

        if (minigameSettings == null)
            minigameSettings = Resources.Load<MinigameSettings>("MainMinigameSettings");

        minigameManager.Configure(minigameSettings, this);

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

        if (techTreeManager == null)
            techTreeManager = GetComponent<TechTreeManager>();

        if (techTreeManager == null)
            techTreeManager = gameObject.AddComponent<TechTreeManager>();

        if (techTreeDatabase != null)
            techTreeManager.ConfigureDatabase(techTreeDatabase);
        else
        {
            TechTreeDatabase resourcesDatabase = Resources.Load<TechTreeDatabase>("MainTechTreeDatabase");
            if (resourcesDatabase != null)
                techTreeManager.ConfigureDatabase(resourcesDatabase);
        }

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

            if (canvas.GetComponent<TechTreePanelUI>() == null)
                canvas.gameObject.AddComponent<TechTreePanelUI>();
        }
    }

    private static double GetUnixTimeNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
