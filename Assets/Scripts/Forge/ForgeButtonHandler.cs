using TMPro;
using UnityEngine;
using UnityEngine.UI;

// FORGE butonuna basildiginda listeden rastgele item uretir; metin ve ikon gosterir.
public class ForgeButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lastItemText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private ItemData[] forgeableItems;

    private ItemData lastForgedItem;

    /// <summary>Son forge edilen item; sat butonu bunu kullanir.</summary>
    public ItemData LastForgedItem => lastForgedItem;

    /// <summary>Satistan sonra son item kaydini ve ekrani temizler.</summary>
    public void ClearLastItem()
    {
        lastForgedItem = null;
        ClearDisplay();
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
        if (lastForgedItem == null) return;

        if (lastItemText != null)
        {
            lastItemText.text =
                $"{lastForgedItem.ItemName} (ATK {lastForgedItem.BaseAttack:0}) - Sell: {lastForgedItem.SellPrice:0}g";
        }

        if (itemIcon != null)
        {
            Sprite icon = lastForgedItem.Icon;
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
        }
    }

    private void ClearDisplay()
    {
        if (lastItemText != null)
            lastItemText.text = "No item yet";

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
    }
}
