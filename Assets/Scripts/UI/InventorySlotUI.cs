using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tek envanter slotunun gorunumunu ve tiklama davranisini yonetir.
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI indexLabel;
    [SerializeField] private Button slotButton;

    private int slotIndex = -1;
    private InventoryManager inventoryManager;

    /// <summary>Slot indeksini ve bagli envanter referansini ayarlar.</summary>
    public void Initialize(int index, InventoryManager manager)
    {
        slotIndex = index;
        inventoryManager = manager;

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (slotButton == null)
            slotButton = GetComponent<Button>();

        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
                iconImage = iconTransform.GetComponent<Image>();
        }

        if (indexLabel == null)
        {
            Transform labelTransform = transform.Find("Index");
            if (labelTransform != null)
                indexLabel = labelTransform.GetComponent<TextMeshProUGUI>();
        }

        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(HandleClicked);
            slotButton.onClick.AddListener(HandleClicked);
        }

        if (indexLabel != null)
            indexLabel.text = (index + 1).ToString();

        Refresh(false);
    }

    /// <summary>Slot gorunumunu gunceller.</summary>
    public void Refresh(bool isSelected)
    {
        ItemData item = inventoryManager != null ? inventoryManager.GetItemInSlot(slotIndex) : null;
        bool occupied = item != null;

        if (backgroundImage != null)
        {
            if (isSelected)
                backgroundImage.color = UITheme.SlotSelected;
            else if (occupied)
                backgroundImage.color = UITheme.SlotFilled;
            else
                backgroundImage.color = UITheme.SlotEmpty;
        }

        if (iconImage != null)
        {
            Sprite icon = occupied ? item.Icon : null;
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (slotButton != null)
            slotButton.interactable = occupied;
    }

    private void HandleClicked()
    {
        inventoryManager?.SelectSlot(slotIndex);
    }
}
