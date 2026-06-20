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
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AnvilManager anvilManager;

    private ItemData lastForgedItem;
    private bool isForging;

    /// <summary>Son forge edilen item; sat butonu bunu kullanir.</summary>
    public ItemData LastForgedItem => lastForgedItem;

    /// <summary>Forge devam ediyor mu; satis bu surede engellenir.</summary>
    public bool IsForging => isForging;

    /// <summary>Satistan sonra son item kaydini ve ekrani temizler.</summary>
    public void ClearLastItem()
    {
        lastForgedItem = null;
        ClearDisplay();
    }

    /// <summary>Kayit icin son item'in listedeki indeksini dondurur (-1 = yok).</summary>
    public int GetLastItemIndex()
    {
        if (lastForgedItem == null || itemDatabase == null) return -1;
        return itemDatabase.IndexOf(lastForgedItem);
    }

    /// <summary>Kayittan son forge edilen item'i geri yukler.</summary>
    public void RestoreLastItem(int index)
    {
        if (itemDatabase == null || index < 0 || index >= itemDatabase.Count)
        {
            ClearLastItem();
            return;
        }

        lastForgedItem = itemDatabase.GetItem(index);
        RefreshLastItemDisplay();
    }

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnForgeClicked()
    {
        if (isForging || itemDatabase == null || itemDatabase.Count == 0) return;

        StartCoroutine(ForgeRoutine());
    }

    private IEnumerator ForgeRoutine()
    {
        isForging = true;

        if (forgeButton != null)
            forgeButton.interactable = false;

        HideItemDisplay();

        float duration = anvilManager != null ? anvilManager.GetForgeDuration() : 3f;
        float remaining = duration;

        while (remaining > 0f)
        {
            if (forgeTimerText != null)
            {
                // 1 saniyeden kisa surelerde ondalik goster (or. 0.5s)
                string timeText = remaining < 1f
                    ? $"{remaining:0.0}s"
                    : $"{Mathf.CeilToInt(remaining)}s";
                forgeTimerText.text = $"Forging... {timeText}";
            }

            yield return null;
            remaining -= Time.deltaTime;
        }

        if (forgeTimerText != null)
            forgeTimerText.text = string.Empty;

        int randomIndex = Random.Range(0, itemDatabase.Count);
        lastForgedItem = itemDatabase.GetItem(randomIndex);

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

    private void HideItemDisplay()
    {
        if (lastItemText != null)
            lastItemText.text = string.Empty;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
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
