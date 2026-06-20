using TMPro;
using UnityEngine;

// FORGE butonuna basildiginda listeden rastgele item uretir ve ekranda gosterir.
public class ForgeButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lastItemText;
    [SerializeField] private ItemData[] forgeableItems;

    private ItemData lastForgedItem;

    /// <summary>Son forge edilen item; sat butonu bunu kullanir.</summary>
    public ItemData LastForgedItem => lastForgedItem;

    /// <summary>Satistan sonra son item kaydini temizler.</summary>
    public void ClearLastItem()
    {
        lastForgedItem = null;
    }

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnForgeClicked()
    {
        if (forgeableItems == null || forgeableItems.Length == 0) return;

        int randomIndex = Random.Range(0, forgeableItems.Length);
        lastForgedItem = forgeableItems[randomIndex];

        RefreshLastItemDisplay();
    }

    private void RefreshLastItemDisplay()
    {
        if (lastItemText == null || lastForgedItem == null) return;

        lastItemText.text =
            $"{lastForgedItem.ItemName} (ATK {lastForgedItem.BaseAttack:0}) - Sell: {lastForgedItem.SellPrice:0}g";
    }
}
