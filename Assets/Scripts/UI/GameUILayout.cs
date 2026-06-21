using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Canvas uzerinde ekrana sigan sabit bolgeler (header / forge / envanter / alt bar) kurar.
[DefaultExecutionOrder(-50)]
public class GameUILayout : MonoBehaviour
{
    [SerializeField] private InventoryPanelUI inventoryPanelUI;

    private void Awake()
    {
        ApplyLayout();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        ApplyLayout();
    }

    private void Start()
    {
        ApplyLayout();
    }

    /// <summary>Tema ve yerlesimi uygular; tekrar cagrilabilir.</summary>
    public void ApplyLayout()
    {
        EnsureCanvasScaler();
        EnsureBackground();
        EnsureHeader();
        StyleActionButtons();
        EnsureForgeButtonProgress();
        RepositionCoreElements();
        HideLegacyElements();
        EnsureInventoryPanel();
        EnsureAutoForgePanel();
        EnsureForgeItemPrompt();
        EnsureTechTreePanel();
        inventoryPanelUI?.ApplyResponsiveLayout();
    }

    private void EnsureCanvasScaler()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(UITheme.ReferenceWidth, UITheme.ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        // Editor'de farkli en-boy oraninda da tum UI gorunsun diye denge.
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void EnsureBackground()
    {
        Transform existing = transform.Find("Background");
        RectTransform rect;

        if (existing == null)
        {
            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(transform, false);
            backgroundObject.transform.SetAsFirstSibling();
            rect = backgroundObject.GetComponent<RectTransform>();
            backgroundObject.GetComponent<Image>().color = UITheme.Background;
            backgroundObject.GetComponent<Image>().raycastTarget = false;
        }
        else
        {
            rect = existing.GetComponent<RectTransform>();
        }

        StretchFull(rect);
    }

    private void EnsureHeader()
    {
        Transform existing = transform.Find("HeaderPanel");
        RectTransform headerRect;

        if (existing == null)
        {
            GameObject headerObject = new GameObject("HeaderPanel", typeof(RectTransform), typeof(Image));
            headerObject.transform.SetParent(transform, false);
            headerRect = headerObject.GetComponent<RectTransform>();
            headerObject.GetComponent<Image>().color = UITheme.Header;
            headerObject.GetComponent<Image>().raycastTarget = false;
        }
        else
        {
            headerRect = existing.GetComponent<RectTransform>();
        }

        AnchorTopStretch(headerRect, UITheme.HeaderHeight);
        headerRect.SetAsLastSibling();

        TextMeshProUGUI goldText = EnsureHeaderLabel(headerRect, "GoldText", new Vector2(0.02f, 0.5f),
            TextAlignmentOptions.MidlineLeft, isGold: true);
        TextMeshProUGUI anvilInfoText = EnsureHeaderLabel(headerRect, "AnvilInfoText", new Vector2(0.98f, 0.5f),
            TextAlignmentOptions.MidlineRight, isGold: false);

        EnsureHudBindings(goldText, anvilInfoText);
    }

    private TextMeshProUGUI EnsureHeaderLabel(RectTransform headerRect, string objectName, Vector2 anchor,
        TextAlignmentOptions alignment, bool isGold)
    {
        Transform existing = headerRect.Find(objectName);
        TextMeshProUGUI text;

        if (existing == null)
        {
            GameObject labelObject = new GameObject(objectName, typeof(RectTransform));
            labelObject.transform.SetParent(headerRect, false);
            text = labelObject.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            text = existing.GetComponent<TextMeshProUGUI>();
            if (text == null)
                text = existing.gameObject.AddComponent<TextMeshProUGUI>();
        }

        ReparentToHeader(text, headerRect, anchor, alignment, isGold);
        return text;
    }

    private void EnsureHudBindings(TextMeshProUGUI goldText, TextMeshProUGUI anvilInfoText)
    {
        if (!Application.isPlaying) return;

        EconomyManager economy = FindFirstObjectByType<EconomyManager>();
        if (economy != null && goldText != null)
        {
            GoldDisplayUI goldDisplay = GetComponent<GoldDisplayUI>();
            if (goldDisplay == null)
                goldDisplay = gameObject.AddComponent<GoldDisplayUI>();

            goldDisplay.Configure(economy, goldText);
        }

        if (anvilInfoText != null)
        {
            AnvilUpgradeHandler upgradeHandler = FindFirstObjectByType<AnvilUpgradeHandler>();
            upgradeHandler?.ConfigureHud(anvilInfoText, GetComponent<GoldDisplayUI>());
        }
    }

    private void ReparentToHeader(TextMeshProUGUI text, RectTransform headerRect, Vector2 anchor,
        TextAlignmentOptions alignment, bool isGold)
    {
        if (text == null || headerRect == null) return;

        RectTransform rect = text.rectTransform;
        rect.SetParent(headerRect, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(anchor.x, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(isGold ? 520f : 480f, 80f);
        text.alignment = alignment;

        if (isGold)
        {
            text.color = UITheme.GoldText;
            text.fontSize = 38f;
            text.fontStyle = FontStyles.Bold;
            text.outlineWidth = 0.22f;
            text.outlineColor = new Color(0f, 0f, 0f, 0.75f);
        }
        else
        {
            text.color = UITheme.BodyText;
            text.fontSize = 26f;
            text.fontStyle = FontStyles.Normal;
        }

        text.raycastTarget = false;
    }

    private void StyleActionButtons()
    {
        StyleButton(FindChild<Button>("ForgeButton"), UITheme.ForgeButton, GameTexts.ForgeButton);
        StyleButton(FindChild<Button>("SellButton"), UITheme.SellButton, GameTexts.SellButton);
        StyleButton(FindChild<Button>("UpgradeButton"), UITheme.UpgradeButton, GameTexts.UpgradeButton);
    }

    private void StyleButton(Button button, Color color, string label)
    {
        if (button == null) return;

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = color;

        TextMeshProUGUI labelText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (labelText != null)
        {
            if (!string.IsNullOrEmpty(label))
                labelText.text = label;

            labelText.color = Color.white;
            labelText.fontStyle = FontStyles.Bold;
            labelText.fontSize = 30f;
            labelText.raycastTarget = false;
        }
    }

    private void StyleButton(Button button, Color color)
    {
        StyleButton(button, color, null);
    }

    private void RepositionCoreElements()
    {
        float bottomInset = UITheme.BottomBarHeight + UITheme.SelectedDetailHeight + UITheme.SectionGap;
        float topInset = UITheme.HeaderHeight + UITheme.ForgeZoneHeight + UITheme.SectionGap;

        RectTransform forgeButton = FindChild<RectTransform>("ForgeButton");
        if (forgeButton != null)
        {
            AnchorTopCenter(forgeButton, UITheme.ForgeButtonTopOffset, new Vector2(920f, UITheme.ForgeButtonHeight));
        }

        SetActiveIfExists("ForgeTimerText", false);

        RectTransform sellButton = FindChild<RectTransform>("SellButton");
        if (sellButton != null)
        {
            AnchorBottomCenter(sellButton, 20f, new Vector2(920f, UITheme.ActionButtonHeight));
        }

        RectTransform upgradeButton = FindChild<RectTransform>("UpgradeButton");
        if (upgradeButton != null)
        {
            AnchorBottomCenter(upgradeButton, 20f + UITheme.ActionButtonHeight + 12f, new Vector2(920f, UITheme.ActionButtonHeight));
        }

        RectTransform selectedText = FindChild<RectTransform>("SelectedItemText");
        if (selectedText != null)
        {
            AnchorBottomStretch(selectedText, UITheme.BottomBarHeight + 8f, UITheme.SelectedDetailHeight);
        }

        RectTransform inventoryPanel = FindChild<RectTransform>("InventoryPanel");
        if (inventoryPanel != null)
        {
            StretchBetween(inventoryPanel, bottomInset, topInset);
        }

        RectTransform offlineText = FindChild<RectTransform>("OfflineMessageText");
        if (offlineText != null)
        {
            AnchorTopCenter(offlineText, UITheme.HeaderHeight + 4f, new Vector2(920f, 48f));
            TextMeshProUGUI message = offlineText.GetComponent<TextMeshProUGUI>();
            if (message != null)
            {
                message.color = UITheme.GoldText;
                message.fontSize = 28f;
                message.alignment = TextAlignmentOptions.Center;
                message.raycastTarget = false;
            }
        }
    }

    private void HideLegacyElements()
    {
        SetActiveIfExists("ItemIcon", false);
        SetActiveIfExists("LastItemText", false);
    }

    private void EnsureInventoryPanel()
    {
        if (inventoryPanelUI == null)
            inventoryPanelUI = GetComponent<InventoryPanelUI>();

        if (inventoryPanelUI == null)
            inventoryPanelUI = gameObject.AddComponent<InventoryPanelUI>();

        if (!Application.isPlaying)
            return;

        InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
        AnvilManager anvilManager = FindFirstObjectByType<AnvilManager>();
        inventoryPanelUI.Configure(inventoryManager, anvilManager, null);
    }

    /// <summary>Editor bootstrap icin: scaler, arka plan, buton konumu (envanter kurmaz).</summary>
    public void ApplyEditorSetup()
    {
        EnsureCanvasScaler();
        EnsureBackground();
        EnsureHeader();
        StyleActionButtons();
        RepositionCoreElements();
        HideLegacyElements();
    }

    private void EnsureAutoForgePanel()
    {
        if (!Application.isPlaying) return;

        AutoForgePanelUI autoPanel = GetComponent<AutoForgePanelUI>();
        if (autoPanel == null)
            autoPanel = gameObject.AddComponent<AutoForgePanelUI>();

        ForgeAutomationManager automationManager = FindFirstObjectByType<ForgeAutomationManager>();
        autoPanel.Configure(automationManager);
    }

    private void EnsureForgeItemPrompt()
    {
        if (!Application.isPlaying) return;

        if (GetComponent<ForgeItemPromptUI>() == null)
            gameObject.AddComponent<ForgeItemPromptUI>();
    }

    private void EnsureForgeButtonProgress()
    {
        if (!Application.isPlaying) return;

        ForgeButtonHandler forgeHandler = FindFirstObjectByType<ForgeButtonHandler>();
        forgeHandler?.EnsureProgressVisual();
    }

    private void EnsureTechTreePanel()
    {
        if (!Application.isPlaying) return;

        TechTreePanelUI techPanel = GetComponent<TechTreePanelUI>();
        if (techPanel == null)
            techPanel = gameObject.AddComponent<TechTreePanelUI>();

        TechTreeManager techTreeManager = FindFirstObjectByType<TechTreeManager>();
        EconomyManager economy = FindFirstObjectByType<EconomyManager>();
        techPanel.Configure(techTreeManager, economy);
    }

    private T FindChild<T>(string objectName) where T : Component
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name != objectName) continue;
            T component = children[i].GetComponent<T>();
            if (component != null)
                return component;
        }

        return null;
    }

    private void SetActiveIfExists(string objectName, bool active)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name != objectName) continue;
            children[i].gameObject.SetActive(active);
            return;
        }
    }

    /// <summary>RectTransform'i tum ekrana gerer.</summary>
    public static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    /// <summary>Ust kenara sabit yukseklikte panel yerlestirir.</summary>
    public static void AnchorTopStretch(RectTransform rect, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);
    }

    /// <summary>Ust bolgede ortalanmis kutu yerlestirir.</summary>
    public static void AnchorTopCenter(RectTransform rect, float topOffset, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -topOffset);
        rect.sizeDelta = size;
    }

    /// <summary>Alt bolgede ortalanmis kutu yerlestirir.</summary>
    public static void AnchorBottomCenter(RectTransform rect, float bottomOffset, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, bottomOffset);
        rect.sizeDelta = size;
    }

    /// <summary>Alt kenarda genislik boyunca metin alani yerlestirir.</summary>
    public static void AnchorBottomStretch(RectTransform rect, float bottomOffset, float height)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, bottomOffset);
        rect.sizeDelta = new Vector2(-UITheme.HorizontalMargin * 2f, height);
    }

    /// <summary>Alt ve ust inset arasinda esneyen alan.</summary>
    public static void StretchBetween(RectTransform rect, float bottomInset, float topInset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(UITheme.HorizontalMargin, bottomInset);
        rect.offsetMax = new Vector2(-UITheme.HorizontalMargin, -topInset);
    }
}
