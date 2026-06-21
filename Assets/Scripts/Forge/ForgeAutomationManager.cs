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
    private bool isWaitingForUserDecision;

    /// <summary>Auto-forge acik mi.</summary>
    public bool AutoForgeEnabled => autoForgeEnabled;

    /// <summary>Auto-sell acik mi.</summary>
    public bool AutoSellEnabled => autoSellEnabled;

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

        if (!autoSellEnabled)
        {
            if (inventoryManager != null && !inventoryManager.TryAddItem(forgedItem))
                yield break;

            yield break;
        }

        if (inventoryManager == null)
        {
            TrySellItem(forgedItem);
            yield break;
        }

        if (inventoryManager.UsedSlotCount == 0)
        {
            inventoryManager.TryAddItem(forgedItem);
            yield break;
        }

        ItemData referenceItem = inventoryManager.GetBestReferenceItem();
        if (referenceItem == null)
        {
            inventoryManager.TryAddItem(forgedItem);
            yield break;
        }

        if (ItemComparer.IsStrictlyWorse(forgedItem, referenceItem))
        {
            TrySellItem(forgedItem);
            yield break;
        }

        bool inventoryFull = inventoryManager.IsFull;
        if (!ItemComparer.ShouldPromptUser(forgedItem, referenceItem, inventoryFull))
        {
            TrySellItem(forgedItem);
            yield break;
        }

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
            TrySellItem(forgedItem);
    }

    /// <summary>Yeni item'i envantere alir; gerekirse zayif slot satarak yer acar.</summary>
    private bool TryKeepForgedItem(ItemData forgedItem)
    {
        if (inventoryManager == null || forgedItem == null) return false;

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

    /// <summary>Kayit icin auto-forge durumunu dondurur.</summary>
    public bool ExportAutoForgeEnabled() => autoForgeEnabled;

    /// <summary>Kayit icin auto-sell durumunu dondurur.</summary>
    public bool ExportAutoSellEnabled() => autoSellEnabled;

    /// <summary>Kayittan otomasyon ayarlarini yukler.</summary>
    public void ImportState(bool forgeEnabled, bool sellEnabled)
    {
        autoForgeEnabled = forgeEnabled;
        autoSellEnabled = sellEnabled;
        NotifySettingsChanged();
    }

    /// <summary>Forge sirasinda envanter dolu olsa bile satisa izin var mi.</summary>
    public bool CanBypassInventoryForForge()
    {
        return autoSellEnabled;
    }

    /// <summary>Item'i aninda satar; gold artirir.</summary>
    public bool TrySellItem(ItemData item)
    {
        if (item == null || economyManager == null) return false;

        double sellGold = anvilManager != null
            ? anvilManager.GetScaledSellPrice(item.SellPrice)
            : item.SellPrice;

        economyManager.AddGold(sellGold);

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();

        return true;
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
