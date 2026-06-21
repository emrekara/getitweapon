using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Auto-forge, auto-sell ve filtre toggle butonlarini runtime'da olusturur.
public class AutoForgePanelUI : MonoBehaviour
{
    [SerializeField] private ForgeAutomationManager automationManager;

    private Toggle autoForgeToggle;
    private Toggle autoSellToggle;
    private Toggle tierFilterToggle;
    private Toggle eraFilterToggle;
    private TextMeshProUGUI autoForgeLabel;
    private TextMeshProUGUI autoSellLabel;
    private TextMeshProUGUI tierFilterLabel;
    private TextMeshProUGUI eraFilterLabel;
    private Button tierPrevButton;
    private Button tierNextButton;
    private Button eraPrevButton;
    private Button eraNextButton;
    private bool isBuilt;
    private bool listenersRegistered;

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
        BuildIfNeeded();
        SubscribeEvents();
        RefreshLabels();
    }

    private void BuildIfNeeded()
    {
        if (isBuilt || automationManager == null) return;

        Transform existing = transform.Find("AutoForgePanel");
        if (existing != null && existing.Find("TierFilterToggle/PrevButton") == null)
        {
            if (Application.isPlaying)
                Destroy(existing.gameObject);
            else
                DestroyImmediate(existing.gameObject);

            existing = null;
        }

        GameObject panelObject = existing != null
            ? existing.gameObject
            : new GameObject("AutoForgePanel", typeof(RectTransform));

        if (existing == null)
            panelObject.transform.SetParent(transform, false);

        RemoveDuplicateRows(panelObject.transform);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        GameUILayout.AnchorTopCenter(
            panelRect,
            UITheme.AutoForgePanelTopOffset,
            new Vector2(920f, UITheme.AutoForgePanelHeight));

        autoForgeToggle = FindOrCreateToggleRow(panelObject.transform, "AutoForgeToggle", GameTexts.AutoForgeOff, 0f,
            out autoForgeLabel, out _, out _);
        autoSellToggle = FindOrCreateToggleRow(panelObject.transform, "AutoSellToggle", GameTexts.AutoSellOff, -44f,
            out autoSellLabel, out _, out _);
        tierFilterToggle = FindOrCreateToggleRow(panelObject.transform, "TierFilterToggle",
            GameTexts.AutoSellTierFilterOffLabel, -88f, out tierFilterLabel, out tierPrevButton, out tierNextButton);
        eraFilterToggle = FindOrCreateToggleRow(panelObject.transform, "EraFilterToggle",
            GameTexts.AutoSellEraFilterOffLabel, -132f, out eraFilterLabel, out eraPrevButton, out eraNextButton);

        if (!listenersRegistered)
        {
            autoForgeToggle.onValueChanged.AddListener(OnAutoForgeToggled);
            autoSellToggle.onValueChanged.AddListener(OnAutoSellToggled);
            tierFilterToggle.onValueChanged.AddListener(OnTierFilterToggled);
            eraFilterToggle.onValueChanged.AddListener(OnEraFilterToggled);

            tierPrevButton.onClick.AddListener(OnTierPrevClicked);
            tierNextButton.onClick.AddListener(OnTierNextClicked);
            eraPrevButton.onClick.AddListener(OnEraPrevClicked);
            eraNextButton.onClick.AddListener(OnEraNextClicked);

            listenersRegistered = true;
        }

        autoForgeToggle.SetIsOnWithoutNotify(automationManager.AutoForgeEnabled);
        autoSellToggle.SetIsOnWithoutNotify(automationManager.AutoSellEnabled);
        tierFilterToggle.SetIsOnWithoutNotify(automationManager.AutoSellTierFilterEnabled);
        eraFilterToggle.SetIsOnWithoutNotify(automationManager.AutoSellEraFilterEnabled);

        isBuilt = true;
    }

    private static void RemoveDuplicateRows(Transform panel)
    {
        var seen = new System.Collections.Generic.HashSet<string>();

        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            Transform child = panel.GetChild(i);
            if (seen.Add(child.name)) continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private Toggle FindOrCreateToggleRow(Transform parent, string objectName, string label, float yOffset,
        out TextMeshProUGUI labelText, out Button prevButton, out Button nextButton)
    {
        Transform existingRow = parent.Find(objectName);
        if (existingRow != null)
        {
            EnsureSingleRowLabel(existingRow);
            labelText = existingRow.Find("Label")?.GetComponent<TextMeshProUGUI>();
            prevButton = existingRow.Find("PrevButton")?.GetComponent<Button>();
            nextButton = existingRow.Find("NextButton")?.GetComponent<Button>();
            return existingRow.GetComponent<Toggle>();
        }

        return CreateToggleRow(parent, objectName, label, yOffset, out labelText, out prevButton, out nextButton);
    }

    private static void EnsureSingleRowLabel(Transform row)
    {
        Transform keepLabel = null;

        for (int i = row.childCount - 1; i >= 0; i--)
        {
            Transform child = row.GetChild(i);
            if (child.name != "Label") continue;

            if (keepLabel == null)
            {
                keepLabel = child;
                continue;
            }

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private void SubscribeEvents()
    {
        if (automationManager == null) return;

        automationManager.OnSettingsChanged -= RefreshLabels;
        automationManager.OnSettingsChanged += RefreshLabels;
        LocalizationManager.OnLanguageChanged -= RefreshLabels;
        LocalizationManager.OnLanguageChanged += RefreshLabels;
    }

    private void UnsubscribeEvents()
    {
        if (automationManager != null)
            automationManager.OnSettingsChanged -= RefreshLabels;
        LocalizationManager.OnLanguageChanged -= RefreshLabels;
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

    private void OnTierFilterToggled(bool isOn)
    {
        automationManager?.SetAutoSellTierFilter(isOn);
        RefreshLabels();
    }

    private void OnEraFilterToggled(bool isOn)
    {
        automationManager?.SetAutoSellEraFilter(isOn);
        RefreshLabels();
    }

    private void OnTierPrevClicked()
    {
        automationManager?.AdjustAutoSellMaxTier(-1);
        RefreshLabels();
    }

    private void OnTierNextClicked()
    {
        automationManager?.AdjustAutoSellMaxTier(1);
        RefreshLabels();
    }

    private void OnEraPrevClicked()
    {
        automationManager?.AdjustAutoSellMaxEraIndex(-1);
        RefreshLabels();
    }

    private void OnEraNextClicked()
    {
        automationManager?.AdjustAutoSellMaxEraIndex(1);
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

        if (tierFilterLabel != null)
        {
            if (automationManager.AutoSellTierFilterEnabled)
            {
                int tier = automationManager.AutoSellMaxTier;
                int tierSpan = AutoSellFilter.MaxTier - AutoSellFilter.MinTier + 1;
                int tierPosition = tier - AutoSellFilter.MinTier + 1;
                tierFilterLabel.text = GameTexts.AutoSellTierFilterLabel(tier) + " " +
                                       GameTexts.FilterRangePosition(tierPosition, tierSpan);
            }
            else
            {
                tierFilterLabel.text = GameTexts.AutoSellTierFilterOffLabel;
            }
        }

        if (eraFilterLabel != null)
        {
            if (automationManager.AutoSellEraFilterEnabled)
            {
                int eraIndex = automationManager.AutoSellMaxEraIndex;
                int eraPosition = eraIndex + 1;
                int eraSpan = AutoSellFilter.EraOrder.Length;
                eraFilterLabel.text = GameTexts.AutoSellEraFilterLabel(eraIndex) + " " +
                                      GameTexts.FilterRangePosition(eraPosition, eraSpan);
            }
            else
            {
                eraFilterLabel.text = GameTexts.AutoSellEraFilterOffLabel;
            }
        }

        bool tierFilterOn = automationManager.AutoSellTierFilterEnabled;
        if (tierPrevButton != null)
        {
            tierPrevButton.gameObject.SetActive(tierFilterOn);
            tierPrevButton.interactable = tierFilterOn && automationManager.CanDecreaseAutoSellMaxTier;
        }

        if (tierNextButton != null)
        {
            tierNextButton.gameObject.SetActive(tierFilterOn);
            tierNextButton.interactable = tierFilterOn && automationManager.CanIncreaseAutoSellMaxTier;
        }

        bool eraFilterOn = automationManager.AutoSellEraFilterEnabled;
        if (eraPrevButton != null)
        {
            eraPrevButton.gameObject.SetActive(eraFilterOn);
            eraPrevButton.interactable = eraFilterOn && automationManager.CanDecreaseAutoSellMaxEra;
        }

        if (eraNextButton != null)
        {
            eraNextButton.gameObject.SetActive(eraFilterOn);
            eraNextButton.interactable = eraFilterOn && automationManager.CanIncreaseAutoSellMaxEra;
        }

        if (autoForgeToggle != null)
            autoForgeToggle.SetIsOnWithoutNotify(automationManager.AutoForgeEnabled);

        if (autoSellToggle != null)
            autoSellToggle.SetIsOnWithoutNotify(automationManager.AutoSellEnabled);

        if (tierFilterToggle != null)
            tierFilterToggle.SetIsOnWithoutNotify(automationManager.AutoSellTierFilterEnabled);

        if (eraFilterToggle != null)
            eraFilterToggle.SetIsOnWithoutNotify(automationManager.AutoSellEraFilterEnabled);
    }

    private Toggle CreateToggleRow(Transform parent, string objectName, string label, float yOffset,
        out TextMeshProUGUI labelText, out Button prevButton, out Button nextButton)
    {
        GameObject rowObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Toggle));
        rowObject.transform.SetParent(parent, false);

        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 1f);
        rowRect.anchorMax = new Vector2(0.5f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, yOffset);
        rowRect.sizeDelta = new Vector2(920f, UITheme.AutoToggleRowHeight);

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

        prevButton = CreateStepButton(rowObject.transform, "PrevButton", "‹", -100f);
        nextButton = CreateStepButton(rowObject.transform, "NextButton", "›", -52f);
        prevButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(rowObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(56f, 0f);
        labelRect.offsetMax = new Vector2(-112f, 0f);

        labelText = labelObject.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 20f;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.color = UITheme.BodyText;
        labelText.raycastTarget = false;

        return toggle;
    }

    private static Button CreateStepButton(Transform parent, string objectName, string symbol, float rightOffset)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(rightOffset, 0f);
        buttonRect.sizeDelta = new Vector2(40f, 36f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = UITheme.UpgradeButton;

        Button button = buttonObject.GetComponent<Button>();

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = symbol;
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        return button;
    }
}
