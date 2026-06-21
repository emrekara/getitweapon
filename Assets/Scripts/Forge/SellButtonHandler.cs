using UnityEngine;

// SELL butonuna basildiginda secili envanter slotundaki item satilir.
public class SellButtonHandler : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnSellClicked()
    {
        if (forgeButtonHandler != null && forgeButtonHandler.IsForging) return;

        EnsureInventoryManager();
        if (inventoryManager == null) return;

        if (!inventoryManager.TryRemoveSelected(out ItemData item) || item == null) return;

        double sellGold = anvilManager != null
            ? anvilManager.GetScaledSellPrice(item.SellPrice)
            : item.SellPrice;

        economyManager.AddGold(sellGold);
        goldDisplayUI.RefreshDisplay();
        saveManager?.SaveGame();
    }

    private void EnsureInventoryManager()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();
    }
}
