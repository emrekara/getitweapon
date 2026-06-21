using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Auto-forge ve auto-sell toggle butonlarini runtime'da olusturur.
public class AutoForgePanelUI : MonoBehaviour
{
    [SerializeField] private ForgeAutomationManager automationManager;

    private Toggle autoForgeToggle;
    private Toggle autoSellToggle;
    private TextMeshProUGUI autoForgeLabel;
    private TextMeshProUGUI autoSellLabel;
    private bool isBuilt;

    private void Start()
    {
        if (automationManager == null)
            automationManager = FindFirstObjectByType<ForgeAutomationManager>();

        BuildIfNeeded();
        SubscribeEvents();
        RefreshLabels();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    /// <summary>Disaridan automation referansi baglandiginda paneli kurar.</summary>
    public void Configure(ForgeAutomationManager manager)
    {
        automationManager = manager;
        isBuilt = false;
        BuildIfNeeded();
        SubscribeEvents();
        RefreshLabels();
    }

    private void BuildIfNeeded()
    {
        if (isBuilt || automationManager == null) return;

        Transform existing = transform.Find("AutoForgePanel");
        GameObject panelObject = existing != null
            ? existing.gameObject
            : new GameObject("AutoForgePanel", typeof(RectTransform));

        if (existing == null)
            panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        GameUILayout.AnchorTopCenter(
            panelRect,
            UITheme.AutoForgePanelTopOffset,
            new Vector2(920f, UITheme.AutoToggleRowHeight));

        autoForgeToggle = CreateToggleRow(panelObject.transform, "AutoForgeToggle", GameTexts.AutoForgeOff, 0f,
            out autoForgeLabel);
        autoSellToggle = CreateToggleRow(panelObject.transform, "AutoSellToggle", GameTexts.AutoSellOff, -52f,
            out autoSellLabel);

        autoForgeToggle.onValueChanged.AddListener(OnAutoForgeToggled);
        autoSellToggle.onValueChanged.AddListener(OnAutoSellToggled);

        autoForgeToggle.SetIsOnWithoutNotify(automationManager.AutoForgeEnabled);
        autoSellToggle.SetIsOnWithoutNotify(automationManager.AutoSellEnabled);

        isBuilt = true;
    }

    private void SubscribeEvents()
    {
        if (automationManager == null) return;

        automationManager.OnSettingsChanged -= RefreshLabels;
        automationManager.OnSettingsChanged += RefreshLabels;
    }

    private void UnsubscribeEvents()
    {
        if (automationManager == null) return;
        automationManager.OnSettingsChanged -= RefreshLabels;
    }

    private void OnAutoForgeToggled(bool isOn)
    {
        automationManager?.SetAutoForge(isOn);
        RefreshLabels();
    }

    private void OnAutoSellToggled(bool isOn)
    {
        automationManager?.SetAutoSell(isOn);
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        if (automationManager == null) return;

        if (autoForgeLabel != null)
        {
            autoForgeLabel.text = automationManager.AutoForgeEnabled
                ? GameTexts.AutoForgeOn
                : GameTexts.AutoForgeOff;
        }

        if (autoSellLabel != null)
        {
            autoSellLabel.text = automationManager.AutoSellEnabled
                ? GameTexts.AutoSellOn
                : GameTexts.AutoSellOff;
        }

        if (autoForgeToggle != null)
            autoForgeToggle.SetIsOnWithoutNotify(automationManager.AutoForgeEnabled);

        if (autoSellToggle != null)
            autoSellToggle.SetIsOnWithoutNotify(automationManager.AutoSellEnabled);
    }

    private Toggle CreateToggleRow(Transform parent, string objectName, string label, float yOffset,
        out TextMeshProUGUI labelText)
    {
        GameObject rowObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Toggle));
        rowObject.transform.SetParent(parent, false);

        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 1f);
        rowRect.anchorMax = new Vector2(0.5f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, yOffset);
        rowRect.sizeDelta = new Vector2(920f, 44f);

        Image background = rowObject.GetComponent<Image>();
        background.color = UITheme.Panel;

        Toggle toggle = rowObject.GetComponent<Toggle>();
        toggle.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = toggle.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.selectedColor = UITheme.ToggleOn;
        toggle.colors = colors;

        GameObject checkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkObject.transform.SetParent(rowObject.transform, false);
        RectTransform checkRect = checkObject.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0f, 0.5f);
        checkRect.anchorMax = new Vector2(0f, 0.5f);
        checkRect.pivot = new Vector2(0f, 0.5f);
        checkRect.anchoredPosition = new Vector2(16f, 0f);
        checkRect.sizeDelta = new Vector2(28f, 28f);

        Image checkImage = checkObject.GetComponent<Image>();
        checkImage.color = UITheme.ToggleOn;

        toggle.graphic = checkImage;
        toggle.targetGraphic = background;
        toggle.isOn = false;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(rowObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(56f, 0f);
        labelRect.offsetMax = new Vector2(-16f, 0f);

        labelText = labelObject.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 24f;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.color = UITheme.BodyText;
        labelText.raycastTarget = false;

        return toggle;
    }
}
