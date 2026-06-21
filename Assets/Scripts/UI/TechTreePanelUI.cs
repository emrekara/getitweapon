using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tech tree paneli; tek slot arastirma timer'i ve yukseltme butonlari.
/// </summary>
public class TechTreePanelUI : MonoBehaviour
{
  [SerializeField] private TechTreeManager techTreeManager;
  [SerializeField] private EconomyManager economyManager;

  private GameObject panelRoot;
  private Transform nodeListParent;
  private bool isBuilt;
  private Coroutine refreshCoroutine;

  private void Start()
  {
    if (techTreeManager == null)
      techTreeManager = FindFirstObjectByType<TechTreeManager>();

    if (economyManager == null)
      economyManager = FindFirstObjectByType<EconomyManager>();

    BuildIfNeeded();
    SubscribeEvents();
    RefreshPanel();
  }

  private void OnDestroy()
  {
    UnsubscribeEvents();
    StopRefreshCoroutine();
  }

  /// <summary>Disaridan manager baglandiginda paneli kurar.</summary>
  public void Configure(TechTreeManager manager, EconomyManager economy)
  {
    techTreeManager = manager;
    economyManager = economy;
    isBuilt = false;
    BuildIfNeeded();
    SubscribeEvents();
    RefreshPanel();
  }

  /// <summary>Paneli ac/kapa.</summary>
  public void TogglePanel()
  {
    BuildIfNeeded();
    if (panelRoot == null) return;

    bool willOpen = !panelRoot.activeSelf;
    panelRoot.SetActive(willOpen);

    if (willOpen)
    {
      panelRoot.transform.SetAsLastSibling();
      RefreshPanel();
      StartRefreshCoroutine();
    }
    else
    {
      StopRefreshCoroutine();
    }
  }

  private void BuildIfNeeded()
  {
    if (isBuilt || techTreeManager == null) return;

    Transform existingToggle = transform.Find("TechTreeToggle");
    if (existingToggle == null)
    {
      GameObject toggleObject = new GameObject("TechTreeToggle", typeof(RectTransform), typeof(Image), typeof(Button));
      toggleObject.transform.SetParent(transform, false);

      RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
      toggleRect.anchorMin = new Vector2(0f, 0f);
      toggleRect.anchorMax = new Vector2(0f, 0f);
      toggleRect.pivot = new Vector2(0f, 0f);
      toggleRect.anchoredPosition = new Vector2(UITheme.HorizontalMargin,
        20f + UITheme.ActionButtonHeight * 2f + 24f);
      toggleRect.sizeDelta = new Vector2(280f, 56f);

      toggleObject.GetComponent<Image>().color = UITheme.Panel;
      toggleObject.GetComponent<Button>().onClick.AddListener(TogglePanel);

      TextMeshProUGUI toggleLabel = CreateLabel(toggleObject.transform,
        LocalizationManager.Get(LocalizationKey.TechTreeTitle), 22f, TextAlignmentOptions.Center);
      StretchLabel(toggleLabel.rectTransform);
    }

    Transform existingPanel = transform.Find("TechTreePanel");
    panelRoot = existingPanel != null
      ? existingPanel.gameObject
      : new GameObject("TechTreePanel", typeof(RectTransform), typeof(Image));

    if (existingPanel == null)
    {
      panelRoot.transform.SetParent(transform, false);
      RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.pivot = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(920f, 720f);
      panelRect.anchoredPosition = Vector2.zero;
      panelRoot.GetComponent<Image>().color = UITheme.Panel;
    }

    Transform titleTransform = panelRoot.transform.Find("Title");
    if (titleTransform == null)
    {
      GameObject titleObject = new GameObject("Title", typeof(RectTransform));
      titleObject.transform.SetParent(panelRoot.transform, false);
      RectTransform titleRect = titleObject.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 1f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.pivot = new Vector2(0.5f, 1f);
      titleRect.anchoredPosition = Vector2.zero;
      titleRect.sizeDelta = new Vector2(0f, 56f);

      TextMeshProUGUI titleText = CreateLabel(titleObject.transform,
        LocalizationManager.Get(LocalizationKey.TechTreeTitle), 28f, TextAlignmentOptions.Center);
      StretchLabel(titleText.rectTransform);
    }

    Transform listTransform = panelRoot.transform.Find("NodeList");
    if (listTransform == null)
    {
      GameObject listObject = new GameObject("NodeList", typeof(RectTransform));
      listObject.transform.SetParent(panelRoot.transform, false);
      RectTransform listRect = listObject.GetComponent<RectTransform>();
      listRect.anchorMin = Vector2.zero;
      listRect.anchorMax = Vector2.one;
      listRect.offsetMin = new Vector2(16f, 16f);
      listRect.offsetMax = new Vector2(-16f, -64f);
      nodeListParent = listObject.transform;
    }
    else
    {
      nodeListParent = listTransform;
    }

    panelRoot.SetActive(false);
    isBuilt = true;
  }

  private void SubscribeEvents()
  {
    if (techTreeManager == null) return;

    techTreeManager.OnTechChanged -= HandleTechChanged;
    techTreeManager.OnTechChanged += HandleTechChanged;
    techTreeManager.OnResearchTimerChanged -= HandleResearchTimerChanged;
    techTreeManager.OnResearchTimerChanged += HandleResearchTimerChanged;
    LocalizationManager.OnLanguageChanged -= RefreshPanel;
    LocalizationManager.OnLanguageChanged += RefreshPanel;
  }

  private void UnsubscribeEvents()
  {
    if (techTreeManager == null) return;

    techTreeManager.OnTechChanged -= HandleTechChanged;
    techTreeManager.OnResearchTimerChanged -= HandleResearchTimerChanged;
    LocalizationManager.OnLanguageChanged -= RefreshPanel;
  }

  private void HandleTechChanged()
  {
    RefreshPanel();
    UpdateRefreshCoroutine();
  }

  private void HandleResearchTimerChanged()
  {
    if (panelRoot != null && panelRoot.activeSelf)
      RefreshPanel();
  }

  private void StartRefreshCoroutine()
  {
    StopRefreshCoroutine();
    refreshCoroutine = StartCoroutine(RefreshWhileOpen());
  }

  private void StopRefreshCoroutine()
  {
    if (refreshCoroutine == null) return;
    StopCoroutine(refreshCoroutine);
    refreshCoroutine = null;
  }

  private void UpdateRefreshCoroutine()
  {
    if (panelRoot == null || !panelRoot.activeSelf) return;

    if (techTreeManager != null && techTreeManager.IsResearchInProgress)
      StartRefreshCoroutine();
    else
      StopRefreshCoroutine();
  }

  private IEnumerator RefreshWhileOpen()
  {
    while (panelRoot != null && panelRoot.activeSelf &&
           techTreeManager != null && techTreeManager.IsResearchInProgress)
    {
      RefreshPanel();
      yield return null;
    }

    refreshCoroutine = null;
  }

  private void RefreshPanel()
  {
    if (!isBuilt || nodeListParent == null || techTreeManager == null) return;

    for (int i = nodeListParent.childCount - 1; i >= 0; i--)
      Destroy(nodeListParent.GetChild(i).gameObject);

    TechTreeDatabase database = techTreeManager.Database;
    if (database == null) return;

    IReadOnlyList<TechNodeData> nodes = database.Nodes;
    float yOffset = 0f;

    for (int i = 0; i < nodes.Count; i++)
    {
      TechNodeData node = nodes[i];
      if (node == null) continue;

      CreateNodeRow(node, yOffset);
      yOffset -= 140f;
    }
  }

  private void CreateNodeRow(TechNodeData node, float yOffset)
  {
    GameObject rowObject = new GameObject($"Node_{node.NodeId}", typeof(RectTransform), typeof(Image));
    rowObject.transform.SetParent(nodeListParent, false);

    RectTransform rowRect = rowObject.GetComponent<RectTransform>();
    rowRect.anchorMin = new Vector2(0.5f, 1f);
    rowRect.anchorMax = new Vector2(0.5f, 1f);
    rowRect.pivot = new Vector2(0.5f, 1f);
    rowRect.anchoredPosition = new Vector2(0f, yOffset);
    rowRect.sizeDelta = new Vector2(880f, 128f);

    rowObject.GetComponent<Image>().color = UITheme.SlotFilled;

    int level = techTreeManager.GetNodeLevel(node.NodeId);
    bool unlocked = techTreeManager.IsNodeUnlocked(node);
    bool maxed = level >= node.MaxLevel;
    bool isResearching = techTreeManager.IsNodeResearching(node.NodeId);
    bool researchBusy = techTreeManager.HasPendingResearch && !isResearching;

    string nameText = GameTexts.GetTechNodeName(node.DisplayNameKey);
    string levelText = GameTexts.TechNodeLevel(level, node.MaxLevel);
    string effectText = GetEffectDescription(node, level);

    if (isResearching)
      effectText += $"\n{GameTexts.TechNodeResearching(GameTexts.FormatDuration(techTreeManager.GetRemainingResearchSeconds()))}";

    TextMeshProUGUI infoLabel = CreateLabel(rowObject.transform, $"{nameText}\n{levelText}\n{effectText}",
      22f, TextAlignmentOptions.MidlineLeft);
    RectTransform infoRect = infoLabel.rectTransform;
    infoRect.anchorMin = Vector2.zero;
    infoRect.anchorMax = Vector2.one;
    infoRect.offsetMin = new Vector2(16f, 8f);
    infoRect.offsetMax = new Vector2(-220f, -8f);

    GameObject buttonObject = new GameObject("UpgradeButton", typeof(RectTransform), typeof(Image), typeof(Button));
    buttonObject.transform.SetParent(rowObject.transform, false);

    RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
    buttonRect.anchorMin = new Vector2(1f, 0.5f);
    buttonRect.anchorMax = new Vector2(1f, 0.5f);
    buttonRect.pivot = new Vector2(1f, 0.5f);
    buttonRect.anchoredPosition = new Vector2(-16f, 0f);
    buttonRect.sizeDelta = new Vector2(180f, 56f);

    Image buttonImage = buttonObject.GetComponent<Image>();

    string buttonLabel;
    bool canStart = unlocked && !maxed && !researchBusy && !isResearching;

    if (!unlocked)
    {
      buttonLabel = LocalizationManager.Get(LocalizationKey.TechNodeLocked);
      buttonImage.color = UITheme.ToggleOff;
    }
    else if (maxed)
    {
      buttonLabel = LocalizationManager.Get(LocalizationKey.TechNodeMaxLevel);
      buttonImage.color = UITheme.ToggleOff;
    }
    else if (isResearching)
    {
      buttonLabel = GameTexts.TechNodeResearching(
        GameTexts.FormatDuration(techTreeManager.GetRemainingResearchSeconds()));
      buttonImage.color = UITheme.UpgradeButton;
    }
    else if (researchBusy)
    {
      buttonLabel = GameTexts.TechNodeResearching(
        GameTexts.FormatDuration(techTreeManager.GetRemainingResearchSeconds()));
      buttonImage.color = UITheme.ToggleOff;
    }
    else
    {
      double cost = node.GetUpgradeCost(level);
      string duration = GameTexts.FormatDuration(node.GetResearchDurationSeconds(level));
      buttonLabel = GameTexts.TechNodeUpgradeWithDuration(cost, duration);
      buttonImage.color = UITheme.UpgradeButton;
    }

    TextMeshProUGUI buttonText = CreateLabel(buttonObject.transform, buttonLabel, 16f, TextAlignmentOptions.Center);
    StretchLabel(buttonText.rectTransform);
    buttonText.fontStyle = FontStyles.Bold;

    Button button = buttonObject.GetComponent<Button>();
    button.interactable = canStart;
    TechNodeData capturedNode = node;
    button.onClick.AddListener(() => OnResearchClicked(capturedNode));
  }

  private void OnResearchClicked(TechNodeData node)
  {
    if (techTreeManager == null) return;

    if (techTreeManager.TryStartResearch(node))
    {
      RefreshPanel();
      UpdateRefreshCoroutine();
    }
  }

  private static string GetEffectDescription(TechNodeData node, int level)
  {
    float total = node.GetTotalEffectValue(level);
    switch (node.EffectType)
    {
      case TechEffectType.ForgeSpeed:
        return $"+{total * 100f:0}% forge";
      case TechEffectType.UpgradeCostReduction:
        return $"-{total * 100f:0}% upgrade";
      case TechEffectType.OfflineGold:
        return $"+{total * 100f:0}% offline";
      default:
        return string.Empty;
    }
  }

  private static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize,
    TextAlignmentOptions alignment)
  {
    GameObject labelObject = new GameObject("Label", typeof(RectTransform));
    labelObject.transform.SetParent(parent, false);
    TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
    label.text = text;
    label.fontSize = fontSize;
    label.alignment = alignment;
    label.color = UITheme.BodyText;
    label.raycastTarget = false;
    return label;
  }

  private static void StretchLabel(RectTransform rect)
  {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
  }
}
