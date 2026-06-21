using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Envanter grid'i ve secili item detay metnini olusturup gunceller.
public class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private TextMeshProUGUI selectedItemText;
    [SerializeField] private TextMeshProUGUI inventoryCountText;

    private InventorySlotUI[] slotViews;
    private RectTransform panelRect;
    private GridLayoutGroup slotGrid;
    private bool isBuilt;

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (anvilManager == null)
            anvilManager = FindFirstObjectByType<AnvilManager>();

        BuildIfNeeded();
        ApplyResponsiveLayout();
        SubscribeEvents();
        RefreshAll();
        StartCoroutine(ApplyLayoutNextFrame());
    }

    private System.Collections.IEnumerator ApplyLayoutNextFrame()
    {
        yield return null;
        ApplyResponsiveLayout();
        RefreshAll();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    /// <summary>Disaridan envanter referansi baglandiginda UI'yi kurar.</summary>
    public void Configure(InventoryManager manager, AnvilManager anvil, RectTransform container)
    {
        inventoryManager = manager;
        anvilManager = anvil;

        if (container != null)
            slotContainer = container;

        if (isBuilt && slotContainer != null)
        {
            ApplyResponsiveLayout();
            RefreshAll();
            return;
        }

        isBuilt = false;
        BuildIfNeeded();
        ApplyResponsiveLayout();
        SubscribeEvents();
        RefreshAll();
    }

    /// <summary>Panel ve grid boyutunu ekran bolgelerine gore yeniden hesaplar.</summary>
    public void ApplyResponsiveLayout()
    {
        if (panelRect == null)
        {
            Transform panel = transform.Find("InventoryPanel");
            if (panel != null)
                panelRect = panel.GetComponent<RectTransform>();
        }

        if (panelRect != null)
        {
            float bottomInset = UITheme.BottomBarHeight + UITheme.SelectedDetailHeight + UITheme.SectionGap;
            float topInset = UITheme.HeaderHeight + UITheme.ForgeZoneHeight + UITheme.SectionGap;
            GameUILayout.StretchBetween(panelRect, bottomInset, topInset);
        }

        if (selectedItemText != null)
        {
            RectTransform detailRect = selectedItemText.rectTransform;
            GameUILayout.AnchorBottomStretch(detailRect, UITheme.BottomBarHeight + 8f, UITheme.SelectedDetailHeight);
        }

        UpdateGridCellSize();
    }

    private void BuildIfNeeded()
    {
        if (!Application.isPlaying) return;
        if (isBuilt || inventoryManager == null) return;

        if (slotContainer == null)
            slotContainer = CreateSlotContainer();

        CreateSlotViews();
        CreateDetailTexts();
        isBuilt = true;
    }

    private void SubscribeEvents()
    {
        if (inventoryManager == null) return;

        inventoryManager.OnInventoryChanged -= RefreshAll;
        inventoryManager.OnInventoryChanged += RefreshAll;
        inventoryManager.OnSlotSelected -= HandleSlotSelected;
        inventoryManager.OnSlotSelected += HandleSlotSelected;

        if (anvilManager != null)
        {
            anvilManager.OnAnvilLevelChanged -= RefreshAll;
            anvilManager.OnAnvilLevelChanged += RefreshAll;
        }

        LocalizationManager.OnLanguageChanged -= RefreshAll;
        LocalizationManager.OnLanguageChanged += RefreshAll;
    }

    private void UnsubscribeEvents()
    {
        if (inventoryManager == null) return;

        inventoryManager.OnInventoryChanged -= RefreshAll;
        inventoryManager.OnSlotSelected -= HandleSlotSelected;

        if (anvilManager != null)
            anvilManager.OnAnvilLevelChanged -= RefreshAll;

        LocalizationManager.OnLanguageChanged -= RefreshAll;
    }

    private void HandleSlotSelected(int slotIndex)
    {
        RefreshAll();
    }

    /// <summary>Tum slotlari ve detay metnini yeniler.</summary>
    public void RefreshAll()
    {
        if (inventoryManager == null) return;

        if (slotViews != null)
        {
            for (int i = 0; i < slotViews.Length; i++)
            {
                if (slotViews[i] != null)
                    slotViews[i].Refresh(i == inventoryManager.SelectedSlot);
            }
        }

        RefreshSelectedItemText();
        RefreshCountText();
    }

    private void RefreshSelectedItemText()
    {
        if (selectedItemText == null) return;

        ItemData item = inventoryManager.GetSelectedItem();
        if (item == null)
        {
            selectedItemText.text = inventoryManager.UsedSlotCount > 0
                ? GameTexts.SelectFromInventory
                : GameTexts.InventoryEmpty;
            return;
        }

        double sellPrice = anvilManager != null
            ? anvilManager.GetScaledSellPrice(item.SellPrice)
            : item.SellPrice;

        selectedItemText.text = GameTexts.ItemDetail(item.ItemName, item.BaseAttack, sellPrice);
    }

    private void RefreshCountText()
    {
        if (inventoryCountText == null) return;
        inventoryCountText.text =
            GameTexts.InventoryCount(inventoryManager.UsedSlotCount, inventoryManager.SlotCount);
    }

    private void UpdateGridCellSize()
    {
        if (slotGrid == null || slotContainer == null) return;

        float gridWidth = slotContainer.rect.width;
        float gridHeight = slotContainer.rect.height;
        if (gridWidth <= 0f || gridHeight <= 0f) return;

        const float spacing = 10f;
        float cellWidth = (gridWidth - spacing * (UITheme.SlotColumns - 1)) / UITheme.SlotColumns;
        float cellHeight = (gridHeight - spacing * (UITheme.SlotRows - 1)) / UITheme.SlotRows;
        cellHeight = Mathf.Min(cellHeight, cellWidth * 0.82f);

        slotGrid.spacing = new Vector2(spacing, spacing);
        slotGrid.cellSize = new Vector2(Mathf.Max(48f, cellWidth), Mathf.Max(48f, cellHeight));
    }

    private RectTransform CreateSlotContainer()
    {
        Transform existing = transform.Find("InventoryPanel");
        GameObject panelObject;

        if (existing != null)
        {
            panelObject = existing.gameObject;
        }
        else
        {
            panelObject = new GameObject("InventoryPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(transform, false);
        }

        panelRect = panelObject.GetComponent<RectTransform>();
        float bottomInset = UITheme.BottomBarHeight + UITheme.SelectedDetailHeight + UITheme.SectionGap;
        float topInset = UITheme.HeaderHeight + UITheme.ForgeZoneHeight + UITheme.SectionGap;
        GameUILayout.StretchBetween(panelRect, bottomInset, topInset);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = UITheme.Panel;

        Outline outline = panelObject.GetComponent<Outline>();
        if (outline == null)
            outline = panelObject.AddComponent<Outline>();
        outline.effectColor = UITheme.PanelBorder;
        outline.effectDistance = new Vector2(2f, -2f);

        Transform titleTransform = panelObject.transform.Find("InventoryTitle");
        GameObject titleObject = titleTransform != null
            ? titleTransform.gameObject
            : new GameObject("InventoryTitle", typeof(RectTransform));

        if (titleTransform == null)
            titleObject.transform.SetParent(panelObject.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);
        titleRect.sizeDelta = new Vector2(-32f, 36f);

        inventoryCountText = titleObject.GetComponent<TextMeshProUGUI>();
        if (inventoryCountText == null)
            inventoryCountText = titleObject.AddComponent<TextMeshProUGUI>();

        inventoryCountText.text = GameTexts.InventoryCount(0, UITheme.SlotCount);
        inventoryCountText.fontSize = 24f;
        inventoryCountText.alignment = TextAlignmentOptions.MidlineLeft;
        inventoryCountText.color = UITheme.MutedText;
        inventoryCountText.raycastTarget = false;

        Transform gridTransform = panelObject.transform.Find("SlotGrid");
        GameObject gridObject = gridTransform != null
            ? gridTransform.gameObject
            : new GameObject("SlotGrid", typeof(RectTransform), typeof(GridLayoutGroup));

        if (gridTransform == null)
            gridObject.transform.SetParent(panelObject.transform, false);

        RectTransform gridRect = gridObject.GetComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = new Vector2(16f, 16f);
        gridRect.offsetMax = new Vector2(-16f, -44f);

        slotGrid = gridObject.GetComponent<GridLayoutGroup>();
        slotGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        slotGrid.constraintCount = UITheme.SlotColumns;
        slotGrid.childAlignment = TextAnchor.UpperCenter;

        slotContainer = gridRect;
        return gridRect;
    }

    private void CreateSlotViews()
    {
        if (slotContainer == null || inventoryManager == null) return;

        if (slotViews != null && slotViews.Length == inventoryManager.SlotCount)
            return;

        for (int i = slotContainer.childCount - 1; i >= 0; i--)
            Destroy(slotContainer.GetChild(i).gameObject);

        slotViews = new InventorySlotUI[inventoryManager.SlotCount];

        for (int i = 0; i < inventoryManager.SlotCount; i++)
            slotViews[i] = CreateSlotView(i);

        UpdateGridCellSize();
    }

    private InventorySlotUI CreateSlotView(int index)
    {
        GameObject slotObject = new GameObject($"Slot_{index + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
        slotObject.transform.SetParent(slotContainer, false);

        Image background = slotObject.GetComponent<Image>();
        background.color = UITheme.SlotEmpty;

        Button button = slotObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        button.colors = colors;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(slotObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.55f);
        iconRect.anchorMax = new Vector2(0.5f, 0.55f);
        iconRect.sizeDelta = new Vector2(64f, 64f);
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.raycastTarget = false;
        iconImage.enabled = false;
        iconImage.preserveAspect = true;

        GameObject labelObject = new GameObject("Index", typeof(RectTransform));
        labelObject.transform.SetParent(slotObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 6f);
        labelRect.sizeDelta = new Vector2(0f, 24f);

        TextMeshProUGUI indexLabel = labelObject.AddComponent<TextMeshProUGUI>();
        indexLabel.fontSize = 18f;
        indexLabel.alignment = TextAlignmentOptions.Center;
        indexLabel.color = UITheme.MutedText;
        indexLabel.raycastTarget = false;

        InventorySlotUI slotView = slotObject.AddComponent<InventorySlotUI>();
        slotView.Initialize(index, inventoryManager);
        return slotView;
    }

    private void CreateDetailTexts()
    {
        Transform existing = transform.Find("SelectedItemText");
        if (existing != null)
        {
            selectedItemText = existing.GetComponent<TextMeshProUGUI>();
            return;
        }

        GameObject detailObject = new GameObject("SelectedItemText", typeof(RectTransform));
        detailObject.transform.SetParent(transform, false);

        RectTransform detailRect = detailObject.GetComponent<RectTransform>();
        GameUILayout.AnchorBottomStretch(detailRect, UITheme.BottomBarHeight + 8f, UITheme.SelectedDetailHeight);

        selectedItemText = detailObject.AddComponent<TextMeshProUGUI>();
        selectedItemText.fontSize = 24f;
        selectedItemText.alignment = TextAlignmentOptions.Center;
        selectedItemText.color = UITheme.BodyText;
        selectedItemText.raycastTarget = false;
    }
}
