using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Auto-forge ve auto-sell ayarlarini yonetir; forge sonrasi otomasyon zincirini calistirir.
public class ForgeAutomationManager : MonoBehaviour
{
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ForgeItemPromptUI forgeItemPromptUI;

    private bool autoForgeEnabled;
    private bool autoSellEnabled;
    private bool autoSellTierFilterEnabled;
    private int autoSellMaxTier = 1;
    private bool autoSellEraFilterEnabled;
    private int autoSellMaxEraIndex;
    private bool isWaitingForUserDecision;
    private string pendingForgeFeedback;

    /// <summary>Auto-forge acik mi.</summary>
    public bool AutoForgeEnabled => autoForgeEnabled;

    /// <summary>Auto-sell acik mi.</summary>
    public bool AutoSellEnabled => autoSellEnabled;

    /// <summary>Tier filtresi acik mi.</summary>
    public bool AutoSellTierFilterEnabled => autoSellTierFilterEnabled;

    /// <summary>Otomatik satilabilecek maksimum tier.</summary>
    public int AutoSellMaxTier => autoSellMaxTier;

    /// <summary>Cag filtresi acik mi.</summary>
    public bool AutoSellEraFilterEnabled => autoSellEraFilterEnabled;

    /// <summary>Otomatik satilabilecek maksimum cag indeksi.</summary>
    public int AutoSellMaxEraIndex => autoSellMaxEraIndex;

    /// <summary>Tier filtresi bir adim artirilabilir mi.</summary>
    public bool CanIncreaseAutoSellMaxTier => autoSellMaxTier < AutoSellFilter.MaxTier;

    /// <summary>Tier filtresi bir adim azaltilabilir mi.</summary>
    public bool CanDecreaseAutoSellMaxTier => autoSellMaxTier > AutoSellFilter.MinTier;

    /// <summary>Cag filtresi bir adim artirilabilir mi.</summary>
    public bool CanIncreaseAutoSellMaxEra => autoSellMaxEraIndex < AutoSellFilter.EraOrder.Length - 1;

    /// <summary>Cag filtresi bir adim azaltilabilir mi.</summary>
    public bool CanDecreaseAutoSellMaxEra => autoSellMaxEraIndex > 0;

    /// <summary>Kullanici upgrade sorusuna cevap veriyor mu.</summary>
    public bool IsWaitingForUserDecision => isWaitingForUserDecision;

    /// <summary>Ayar degistiginde tetiklenir.</summary>
    public event System.Action OnSettingsChanged;

    private void Awake()
    {
        EnsureReferences();
    }

    /// <summary>Forge tamamlandiginda ForgeButtonHandler tarafindan cagrilir.</summary>
    public void NotifyForgeCompleted()
    {
        if (autoForgeEnabled && !isWaitingForUserDecision)
            TryTriggerAutoForge();
    }

    /// <summary>Forge edilen item icin auto-sell veya kullanici sorusu calistirir.</summary>
    public IEnumerator ProcessForgedItem(ItemData forgedItem)
    {
        EnsureReferences();

        if (forgedItem == null)
            yield break;

        if (inventoryManager != null && inventoryManager.ContainsCategory(forgedItem.Category))
        {
            ItemData categoryItem = inventoryManager.GetItemInCategory(forgedItem.Category);
            bool categoryInventoryFull = inventoryManager.IsFull;

            if (ItemComparer.IsStrictlyWorse(forgedItem, categoryItem))
            {
                if (autoSellEnabled)
                    SellForgedItem(forgedItem);
                else
                    QueueForgeFeedback(GameTexts.ItemCategoryAlreadyInInventory(forgedItem.Category));

                yield break;
            }

            if (ItemComparer.AreAllStatsEqual(forgedItem, categoryItem))
            {
                if (autoSellEnabled)
                    SellForgedItem(forgedItem);
                else
                    QueueForgeFeedback(GameTexts.ItemCategoryAlreadyInInventory(forgedItem.Category));

                yield break;
            }

            yield return PromptKeepOrSell(forgedItem, categoryItem, categoryInventoryFull);
            yield break;
        }

        if (!autoSellEnabled)
        {
            if (inventoryManager != null && !inventoryManager.TryAddItem(forgedItem))
                yield break;

            yield break;
        }

        if (!PassesAutoSellFilter(forgedItem))
        {
            if (inventoryManager != null && !inventoryManager.TryAddItem(forgedItem))
                yield break;

            yield break;
        }

        if (inventoryManager == null)
        {
            SellForgedItem(forgedItem);
            yield break;
        }

        if (inventoryManager.UsedSlotCount == 0)
        {
            if (inventoryManager.TryAddItem(forgedItem))
                ShowItemAddedFeedback(forgedItem);
            yield break;
        }

        ItemData referenceItem = inventoryManager.GetBestReferenceItem();
        if (referenceItem == null)
        {
            if (inventoryManager.TryAddItem(forgedItem))
                ShowItemAddedFeedback(forgedItem);
            yield break;
        }

        bool inventoryFull = inventoryManager.IsFull;

        if (ItemComparer.IsStrictlyWorse(forgedItem, referenceItem))
        {
            SellForgedItem(forgedItem);
            yield break;
        }

        if (ItemComparer.HasAnyStatHigher(forgedItem, referenceItem))
        {
            yield return PromptKeepOrSell(forgedItem, referenceItem, inventoryFull);
            yield break;
        }

        // Esit veya farkli tipte item: bos slot varsa envantere al.
        if (!inventoryFull && inventoryManager.TryAddItem(forgedItem))
        {
            ShowItemAddedFeedback(forgedItem);
            yield break;
        }

        if (inventoryFull && ItemComparer.AreAllStatsEqual(forgedItem, referenceItem))
        {
            yield return PromptKeepOrSell(forgedItem, referenceItem, inventoryFull);
            yield break;
        }

        SellForgedItem(forgedItem);
    }

    private IEnumerator PromptKeepOrSell(ItemData forgedItem, ItemData referenceItem, bool inventoryFull)
    {
        if (forgeItemPromptUI == null)
        {
            TryKeepForgedItem(forgedItem);
            yield break;
        }

        isWaitingForUserDecision = true;
        yield return forgeItemPromptUI.PromptForDecision(forgedItem, referenceItem, anvilManager, inventoryFull);
        bool keepNewItem = forgeItemPromptUI.GetLastDecision();
        isWaitingForUserDecision = false;

        if (keepNewItem)
            TryKeepForgedItem(forgedItem);
        else
            SellForgedItem(forgedItem);
    }

    /// <summary>Forge sonrasi gosterilecek kisa mesaji alir ve temizler.</summary>
    public string ConsumePendingForgeFeedback()
    {
        string message = pendingForgeFeedback;
        pendingForgeFeedback = null;
        return message;
    }

    private void QueueForgeFeedback(string message)
    {
        if (!string.IsNullOrEmpty(message))
            pendingForgeFeedback = message;
    }

    private void SellForgedItem(ItemData item)
    {
        double sellGold = TrySellItem(item);
        if (sellGold > 0)
            QueueForgeFeedback(GameTexts.AutoSoldFeedback(sellGold));
    }

    private void ShowItemAddedFeedback(ItemData item)
    {
        if (item == null) return;
        QueueForgeFeedback(GameTexts.ItemAddedToInventory(item.ItemName));
    }

    /// <summary>Yeni item'i envantere alir; gerekirse zayif slot satarak yer acar.</summary>
    private bool TryKeepForgedItem(ItemData forgedItem)
    {
        if (inventoryManager == null || forgedItem == null) return false;

        int categorySlot = inventoryManager.GetSlotIndexOfCategory(forgedItem.Category);
        if (categorySlot >= 0)
        {
            if (inventoryManager.TryRemoveFromSlot(categorySlot, out ItemData categoryReplacedItem))
                TrySellItem(categoryReplacedItem);

            if (inventoryManager.TryAddItem(forgedItem))
                return true;

            TrySellItem(forgedItem);
            return false;
        }

        if (inventoryManager.TryAddItem(forgedItem))
        {
            SellStrictlyWorseInventoryItems(forgedItem, inventoryManager.SelectedSlot);
            return true;
        }

        int replaceSlot = inventoryManager.GetSlotIndexToReplaceFor(forgedItem);
        if (replaceSlot >= 0 &&
            inventoryManager.TryRemoveFromSlot(replaceSlot, out ItemData replacedItem))
        {
            TrySellItem(replacedItem);

            if (inventoryManager.TryAddItem(forgedItem))
            {
                SellStrictlyWorseInventoryItems(forgedItem, inventoryManager.SelectedSlot);
                return true;
            }
        }

        TrySellItem(forgedItem);
        return false;
    }

    /// <summary>Auto-forge ac/kapa.</summary>
    public void SetAutoForge(bool enabled)
    {
        if (autoForgeEnabled == enabled) return;

        autoForgeEnabled = enabled;
        NotifySettingsChanged();

        if (autoForgeEnabled && !isWaitingForUserDecision)
            TryTriggerAutoForge();

        saveManager?.SaveGame();
    }

    /// <summary>Auto-sell ac/kapa.</summary>
    public void SetAutoSell(bool enabled)
    {
        if (autoSellEnabled == enabled) return;

        autoSellEnabled = enabled;
        NotifySettingsChanged();

        if (autoForgeEnabled && !isWaitingForUserDecision)
            TryTriggerAutoForge();

        saveManager?.SaveGame();
    }

    /// <summary>Tier filtresini ac/kapa.</summary>
    public void SetAutoSellTierFilter(bool enabled)
    {
        if (autoSellTierFilterEnabled == enabled) return;

        autoSellTierFilterEnabled = enabled;
        NotifySettingsChanged();
        saveManager?.SaveGame();
    }

    /// <summary>Maksimum auto-sell tier degerini ayarlar.</summary>
    public void SetAutoSellMaxTier(int maxTier)
    {
        int clamped = Mathf.Clamp(maxTier, AutoSellFilter.MinTier, AutoSellFilter.MaxTier);
        if (autoSellMaxTier == clamped) return;

        autoSellMaxTier = clamped;
        NotifySettingsChanged();
        saveManager?.SaveGame();
    }

    /// <summary>Cag filtresini ac/kapa.</summary>
    public void SetAutoSellEraFilter(bool enabled)
    {
        if (autoSellEraFilterEnabled == enabled) return;

        autoSellEraFilterEnabled = enabled;
        NotifySettingsChanged();
        saveManager?.SaveGame();
    }

    /// <summary>Maksimum auto-sell cag indeksini ayarlar.</summary>
    public void SetAutoSellMaxEraIndex(int eraIndex)
    {
        int clamped = Mathf.Clamp(eraIndex, 0, AutoSellFilter.EraOrder.Length - 1);
        if (autoSellMaxEraIndex == clamped) return;

        autoSellMaxEraIndex = clamped;
        NotifySettingsChanged();
        saveManager?.SaveGame();
    }

    /// <summary>Tier filtresini sinirli aralikta bir adim kaydirir (dongu yok).</summary>
    /// <param name="delta">+1 veya -1.</param>
    public void AdjustAutoSellMaxTier(int delta)
    {
        if (delta == 0) return;
        SetAutoSellMaxTier(autoSellMaxTier + delta);
    }

    /// <summary>Cag filtresini sinirli aralikta bir adim kaydirir (dongu yok).</summary>
    /// <param name="delta">+1 veya -1.</param>
    public void AdjustAutoSellMaxEraIndex(int delta)
    {
        if (delta == 0) return;
        SetAutoSellMaxEraIndex(autoSellMaxEraIndex + delta);
    }

    /// <summary>Kayit icin auto-forge durumunu dondurur.</summary>
    public bool ExportAutoForgeEnabled() => autoForgeEnabled;

    /// <summary>Kayit icin auto-sell durumunu dondurur.</summary>
    public bool ExportAutoSellEnabled() => autoSellEnabled;

    /// <summary>Kayit icin tier filtresi durumunu dondurur.</summary>
    public bool ExportAutoSellTierFilterEnabled() => autoSellTierFilterEnabled;

    /// <summary>Kayit icin maksimum tier degerini dondurur.</summary>
    public int ExportAutoSellMaxTier() => autoSellMaxTier;

    /// <summary>Kayit icin cag filtresi durumunu dondurur.</summary>
    public bool ExportAutoSellEraFilterEnabled() => autoSellEraFilterEnabled;

    /// <summary>Kayit icin maksimum cag indeksini dondurur.</summary>
    public int ExportAutoSellMaxEraIndex() => autoSellMaxEraIndex;

    /// <summary>Kayittan otomasyon ayarlarini yukler.</summary>
    public void ImportState(bool forgeEnabled, bool sellEnabled, bool tierFilterEnabled, int maxTier,
        bool eraFilterEnabled, int maxEraIndex)
    {
        autoForgeEnabled = forgeEnabled;
        autoSellEnabled = sellEnabled;
        autoSellTierFilterEnabled = tierFilterEnabled;
        autoSellMaxTier = Mathf.Clamp(maxTier, AutoSellFilter.MinTier, AutoSellFilter.MaxTier);
        autoSellEraFilterEnabled = eraFilterEnabled;
        autoSellMaxEraIndex = Mathf.Clamp(maxEraIndex, 0, AutoSellFilter.EraOrder.Length - 1);
        NotifySettingsChanged();
    }

    /// <summary>Forge sirasinda envanter dolu olsa bile satisa izin var mi.</summary>
    public bool CanBypassInventoryForForge()
    {
        return autoSellEnabled;
    }

    /// <summary>Item auto-sell filtresinden gecer mi.</summary>
    public bool PassesAutoSellFilter(ItemData item)
    {
        return AutoSellFilter.PassesFilter(item, autoSellTierFilterEnabled, autoSellMaxTier,
            autoSellEraFilterEnabled, autoSellMaxEraIndex);
    }

    /// <summary>Item'i aninda satar; gold artirir.</summary>
    /// <returns>Kazanilan gold; basarisizsa 0.</returns>
    public double TrySellItem(ItemData item)
    {
        if (item == null || economyManager == null) return 0;

        double sellGold = anvilManager != null
            ? anvilManager.GetScaledSellPrice(item.SellPrice)
            : item.SellPrice;

        economyManager.AddGold(sellGold);

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();

        return sellGold;
    }

    private void SellStrictlyWorseInventoryItems(ItemData keeper, int keeperSlot)
    {
        if (inventoryManager == null || keeper == null) return;

        List<int> slotsToSell = new List<int>();

        for (int i = 0; i < inventoryManager.SlotCount; i++)
        {
            if (i == keeperSlot) continue;

            ItemData slotItem = inventoryManager.GetItemInSlot(i);
            if (slotItem == null) continue;
            if (slotItem.Category != keeper.Category) continue;

            if (ItemComparer.IsStrictlyWorseOrEqual(slotItem, keeper))
                slotsToSell.Add(i);
        }

        for (int i = 0; i < slotsToSell.Count; i++)
        {
            if (!inventoryManager.TryRemoveFromSlot(slotsToSell[i], out ItemData soldItem))
                continue;

            TrySellItem(soldItem);
        }
    }

    private void TryTriggerAutoForge()
    {
        EnsureReferences();

        if (isWaitingForUserDecision) return;

        forgeButtonHandler?.TryStartForge();
    }

    private void NotifySettingsChanged()
    {
        OnSettingsChanged?.Invoke();
    }

    private void EnsureReferences()
    {
        if (forgeButtonHandler == null)
            forgeButtonHandler = FindFirstObjectByType<ForgeButtonHandler>();

        if (economyManager == null)
            economyManager = FindFirstObjectByType<EconomyManager>();

        if (goldDisplayUI == null)
            goldDisplayUI = FindFirstObjectByType<GoldDisplayUI>();

        if (anvilManager == null)
            anvilManager = FindFirstObjectByType<AnvilManager>();

        if (saveManager == null)
            saveManager = FindFirstObjectByType<SaveManager>();

        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (forgeItemPromptUI == null)
            forgeItemPromptUI = FindFirstObjectByType<ForgeItemPromptUI>();
    }
}
