using UnityEngine;

// SELL butonuna basildiginda son forge edilen item satilir, gold artar.
public class SellButtonHandler : MonoBehaviour
{
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AnvilManager anvilManager;

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnSellClicked()
    {
        if (forgeButtonHandler.IsForging) return;

        ItemData item = forgeButtonHandler.LastForgedItem;
        if (item == null) return;

        double sellGold = anvilManager != null
            ? anvilManager.GetScaledSellPrice(item.SellPrice)
            : item.SellPrice;

        economyManager.AddGold(sellGold);
        goldDisplayUI.RefreshDisplay();

        forgeButtonHandler.ClearLastItem();
        saveManager?.SaveGame();
    }
}
