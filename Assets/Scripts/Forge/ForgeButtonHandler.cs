using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// FORGE butonuna basildiginda sure bekler, sonra rastgele item uretir.
public class ForgeButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lastItemText;
    [SerializeField] private TextMeshProUGUI forgeTimerText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button forgeButton;
    [SerializeField] private ItemData[] forgeableItems;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private float forgeDurationSeconds = 3f;

    private ItemData lastForgedItem;
    private bool isForging;

    /// <summary>Son forge edilen item; sat butonu bunu kullanir.</summary>
    public ItemData LastForgedItem => lastForgedItem;

    /// <summary>Satistan sonra son item kaydini ve ekrani temizler.</summary>
    public void ClearLastItem()
    {
        lastForgedItem = null;
        ClearDisplay();
    }

    /// <summary>Kayit icin son item'in listedeki indeksini dondurur (-1 = yok).</summary>
    public int GetLastItemIndex()
    {
        if (lastForgedItem == null || forgeableItems == null) return -1;

        for (int i = 0; i < forgeableItems.Length; i++)
        {
            if (forgeableItems[i] == lastForgedItem)
                return i;
        }

        return -1;
    }

    /// <summary>Kayittan son forge edilen item'i geri yukler.</summary>
    public void RestoreLastItem(int index)
    {
        if (forgeableItems == null || index < 0 || index >= forgeableItems.Length)
        {
            ClearLastItem();
            return;
        }

        lastForgedItem = forgeableItems[index];
        RefreshLastItemDisplay();
    }

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnForgeClicked()
    {
        if (isForging || forgeableItems == null || forgeableItems.Length == 0) return;

        StartCoroutine(ForgeRoutine());
    }

    private IEnumerator ForgeRoutine()
    {
        isForging = true;

        if (forgeButton != null)
            forgeButton.interactable = false;

        float remaining = forgeDurationSeconds;

        while (remaining > 0f)
        {
            if (forgeTimerText != null)
                forgeTimerText.text = $"Forging... {Mathf.CeilToInt(remaining)}s";

            yield return null;
            remaining -= Time.deltaTime;
        }

        if (forgeTimerText != null)
            forgeTimerText.text = string.Empty;

        int randomIndex = Random.Range(0, forgeableItems.Length);
        lastForgedItem = forgeableItems[randomIndex];

        RefreshLastItemDisplay();
        saveManager?.SaveGame();

        isForging = false;

        if (forgeButton != null)
            forgeButton.interactable = true;
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
