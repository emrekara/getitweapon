using TMPro;
using UnityEngine;

// SELL butonuna basildiginda son forge edilen item satilir, gold artar.
public class SellButtonHandler : MonoBehaviour
{
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private TextMeshProUGUI lastItemText;

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnSellClicked()
    {
        ItemData item = forgeButtonHandler.LastForgedItem;
        if (item == null) return;

        economyManager.AddGold(item.SellPrice);
        goldDisplayUI.RefreshDisplay();

        forgeButtonHandler.ClearLastItem();
    }
}
