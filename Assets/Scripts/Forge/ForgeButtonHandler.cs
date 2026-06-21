using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// FORGE butonuna basildiginda sure bekler, sonra rastgele item uretir ve envantere ekler.
public class ForgeButtonHandler : MonoBehaviour
{
    [SerializeField] private Button forgeButton;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ForgeAutomationManager automationManager;
    [SerializeField] private HammerManager hammerManager;
    [SerializeField] private MinigameManager minigameManager;

    private bool isForging;

    /// <summary>Forge basladi/bittiiginde UI guncellemesi icin.</summary>
    public event System.Action OnForgeStateChanged;
    private Coroutine blockedMessageCoroutine;
    private TextMeshProUGUI forgeButtonLabel;
    private Image forgeButtonImage;
    private Image forgeProgressFill;
    private string defaultForgeButtonLabel = string.Empty;

    /// <summary>Item havuzu referansi; baska sistemler icin paylasilir.</summary>
    public ItemDatabase ItemDatabase => itemDatabase;

    /// <summary>Forge devam ediyor mu; satis bu surede engellenir.</summary>
    public bool IsForging => isForging;

    /// <summary>Buton uzerinde ilerleme cubugu olusturur (GameUILayout tarafindan cagrilir).</summary>
    public void EnsureProgressVisual()
    {
        if (forgeButton == null)
            forgeButton = GetComponent<Button>();

        if (forgeButtonImage == null)
            forgeButtonImage = forgeButton != null ? forgeButton.GetComponent<Image>() : null;

        if (forgeButtonLabel == null && forgeButton != null)
            forgeButtonLabel = forgeButton.GetComponentInChildren<TextMeshProUGUI>();

        if (forgeButtonLabel != null && string.IsNullOrEmpty(defaultForgeButtonLabel))
            defaultForgeButtonLabel = GameTexts.ForgeButton;

        Transform buttonTransform = forgeButton != null ? forgeButton.transform : transform;
        Transform existingFill = buttonTransform.Find("ForgeProgressFill");

        if (existingFill == null)
        {
            GameObject fillObject = new GameObject("ForgeProgressFill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(buttonTransform, false);
            fillObject.transform.SetAsFirstSibling();

            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);

            forgeProgressFill = fillObject.GetComponent<Image>();
            forgeProgressFill.color = UITheme.ForgeProgressFill;
            forgeProgressFill.type = Image.Type.Filled;
            forgeProgressFill.fillMethod = Image.FillMethod.Horizontal;
            forgeProgressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            forgeProgressFill.fillAmount = 0f;
            forgeProgressFill.raycastTarget = false;
        }
        else
        {
            forgeProgressFill = existingFill.GetComponent<Image>();
        }

        SetForgeVisualProgress(0f, false);
    }

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnForgeClicked()
    {
        EnsureAutomationManager();
        automationManager?.ClearAutoForgeChainPause();
        TryStartForge();
    }

    /// <summary>Forge baslatmayi dener; auto-forge ve manuel forge bunu kullanir.</summary>
    public bool TryStartForge()
    {
        if (isForging || itemDatabase == null) return false;

        EnsureAutomationManager();
        EnsureInventoryManager();
        EnsureProgressVisual();

        if (automationManager != null && automationManager.IsWaitingForUserDecision)
            return false;

        if (!CanStartForge())
        {
            return false;
        }

        EnsureHammerManager();
        if (hammerManager != null && !hammerManager.HasEnoughForForge)
        {
            ShowStatusMessage(GameTexts.NeedHammer(
                hammerManager.CurrentHammers,
                hammerManager.MaxHammers));
            return false;
        }

        if (itemDatabase.GetItemsForEra(GetCurrentEra()).Length == 0) return false;

        StartCoroutine(ForgeRoutine());
        return true;
    }

    private void OnEnable()
    {
        if (anvilManager != null)
            anvilManager.OnAnvilLevelChanged += HandleAnvilLevelChanged;
    }

    private void OnDisable()
    {
        if (anvilManager != null)
            anvilManager.OnAnvilLevelChanged -= HandleAnvilLevelChanged;
    }

    private void Start()
    {
        if (forgeButton == null)
            forgeButton = GetComponent<Button>();

        EnsureProgressVisual();
        EnsureInventoryManager();
        EnsureAutomationManager();
        RefreshForgeButtonState();

        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged += HandleInventoryChanged;

        EnsureHammerManager();
        if (hammerManager != null)
            hammerManager.OnHammersChanged += HandleHammersChanged;
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= HandleInventoryChanged;

        if (hammerManager != null)
            hammerManager.OnHammersChanged -= HandleHammersChanged;
    }

    private void HandleHammersChanged()
    {
        RefreshForgeButtonState();
    }

    private void HandleInventoryChanged()
    {
        RefreshForgeButtonState();
    }

    private void HandleAnvilLevelChanged()
    {
        if (isForging) return;
    }

    private IEnumerator ForgeRoutine()
    {
        CancelStatusMessage();
        BeginForging();

        EnsureHammerManager();
        if (hammerManager != null && !hammerManager.TrySpendForForge())
        {
            EndForging();
            ShowStatusMessage(GameTexts.NeedHammer(
                hammerManager.CurrentHammers,
                hammerManager.MaxHammers));
            yield break;
        }

        float duration = anvilManager != null ? anvilManager.GetForgeDuration() : 3f;
        float elapsed = 0f;

        SetForgeVisualProgress(0f, true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            int percent = Mathf.Min(99, Mathf.RoundToInt(progress * 100f));

            SetForgeVisualProgress(progress, true);

            if (forgeButtonLabel != null)
                forgeButtonLabel.text = GameTexts.ForgingPercent(percent);

            yield return null;
        }

        SetForgeVisualProgress(1f, true);

        if (forgeButtonLabel != null)
            forgeButtonLabel.text = GameTexts.ForgingPercent(100);

        ItemData forgedItem = itemDatabase.GetRandomItemForEra(GetCurrentEra());
        if (forgedItem == null)
        {
            EndForging();
            SetForgeVisualProgress(0f, false);
            yield break;
        }

        EnsureAutomationManager();
        EnsureInventoryManager();

        yield return automationManager != null
            ? automationManager.ProcessForgedItem(forgedItem)
            : AddItemWithoutAutomation(forgedItem);

        saveManager?.SaveGame();

        EndForging();
        SetForgeVisualProgress(0f, false);

        if (automationManager != null)
        {
            string feedback = automationManager.ConsumePendingForgeFeedback();
            bool willAutoChain = automationManager.AutoForgeEnabled &&
                                 !automationManager.IsWaitingForUserDecision;

            if (!string.IsNullOrEmpty(feedback) && !willAutoChain)
                ShowForgeStatus(feedback, 0.7f);
        }

        automationManager?.NotifyForgeCompleted();
    }

    private void SetForgeVisualProgress(float progress, bool forging)
    {
        if (forgeProgressFill != null)
        {
            forgeProgressFill.gameObject.SetActive(forging);
            forgeProgressFill.fillAmount = progress;
        }

        if (forgeButtonImage != null)
            forgeButtonImage.color = forging ? UITheme.ForgeButtonForging : UITheme.ForgeButton;

        if (!forging && forgeButtonLabel != null)
            forgeButtonLabel.text = string.IsNullOrEmpty(defaultForgeButtonLabel)
                ? GameTexts.ForgeButton
                : defaultForgeButtonLabel;
    }

    /// <summary>Forge baslatilabilir mi (cekic ve popup bekleme durumuna gore).</summary>
    private bool CanStartForge()
    {
        EnsureAutomationManager();

        if (automationManager != null && automationManager.IsWaitingForUserDecision)
            return false;

        return true;
    }

    /// <summary>Forge butonunun etkilesim durumunu forge ve envanter kapasitesine gore gunceller.</summary>
    private void RefreshForgeButtonState()
    {
        if (forgeButton == null) return;

        EnsureHammerManager();
        bool hasHammers = hammerManager == null || hammerManager.HasEnoughForForge;
        forgeButton.interactable = !isForging && CanStartForge() && hasHammers;
    }

    private void ShowStatusMessage(string message)
    {
        if (blockedMessageCoroutine != null)
            StopCoroutine(blockedMessageCoroutine);

        blockedMessageCoroutine = StartCoroutine(StatusMessageRoutine(message));
    }

    /// <summary>Forge bolgesinde kisa durum mesaji gosterir.</summary>
    public void ShowForgeStatus(string message, float durationSeconds = 1.5f)
    {
        if (isForging) return;

        CancelStatusMessage();
        blockedMessageCoroutine = StartCoroutine(StatusMessageRoutine(message, durationSeconds));
    }

    private void CancelStatusMessage()
    {
        if (blockedMessageCoroutine == null) return;
        StopCoroutine(blockedMessageCoroutine);
        blockedMessageCoroutine = null;
    }

    private IEnumerator StatusMessageRoutine(string message, float durationSeconds = 2f)
    {
        EnsureProgressVisual();

        if (forgeButtonLabel != null)
            forgeButtonLabel.text = message;

        yield return new WaitForSeconds(durationSeconds);

        if (!isForging)
            SetForgeVisualProgress(0f, false);

        blockedMessageCoroutine = null;
    }

    private string GetCurrentEra()
    {
        return anvilManager != null ? anvilManager.CurrentEra : "Stone";
    }

    private void EnsureInventoryManager()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();
    }

    private void EnsureAutomationManager()
    {
        if (automationManager == null)
            automationManager = FindFirstObjectByType<ForgeAutomationManager>();
    }

    private void EnsureHammerManager()
    {
        if (hammerManager == null)
            hammerManager = FindFirstObjectByType<HammerManager>();
    }

    private void EnsureMinigameManager()
    {
        if (minigameManager == null)
            minigameManager = FindFirstObjectByType<MinigameManager>();
    }

    private void BeginForging()
    {
        isForging = true;
        EnsureMinigameManager();
        minigameManager?.NotifyForgeStarted();
        RefreshForgeButtonState();
        OnForgeStateChanged?.Invoke();
    }

    private void EndForging()
    {
        isForging = false;
        EnsureMinigameManager();
        minigameManager?.NotifyForgeEnded();
        RefreshForgeButtonState();
        OnForgeStateChanged?.Invoke();
    }

    private IEnumerator AddItemWithoutAutomation(ItemData forgedItem)
    {
        if (inventoryManager == null || forgedItem == null) yield break;

        if (inventoryManager.ContainsCategory(forgedItem.Category))
        {
            ItemData categoryItem = inventoryManager.GetItemInCategory(forgedItem.Category);
            if (categoryItem != null &&
                (ItemComparer.IsStrictlyWorse(forgedItem, categoryItem) ||
                 ItemComparer.AreAllStatsEqual(forgedItem, categoryItem)))
            {
                yield break;
            }
        }

        if (!inventoryManager.TryAddItem(forgedItem))
            ShowStatusMessage(GameTexts.InventoryFull);

        yield break;
    }
}
