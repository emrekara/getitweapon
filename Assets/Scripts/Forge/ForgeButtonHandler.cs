using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// FORGE butonuna basildiginda sure bekler, sonra rastgele item uretir ve envantere ekler.
public class ForgeButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI forgeTimerText;
    [SerializeField] private Button forgeButton;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ForgeAutomationManager automationManager;

    private bool isForging;
    private Coroutine blockedMessageCoroutine;

    /// <summary>Item havuzu referansi; baska sistemler icin paylasilir.</summary>
    public ItemDatabase ItemDatabase => itemDatabase;

    /// <summary>Forge devam ediyor mu; satis bu surede engellenir.</summary>
    public bool IsForging => isForging;

    /// <summary>Buton OnClick olayina baglanir.</summary>
    public void OnForgeClicked()
    {
        TryStartForge();
    }

    /// <summary>Forge baslatmayi dener; auto-forge ve manuel forge bunu kullanir.</summary>
    public bool TryStartForge()
    {
        if (isForging || itemDatabase == null) return false;

        EnsureAutomationManager();
        EnsureInventoryManager();

        if (automationManager != null && automationManager.IsWaitingForUserDecision)
            return false;

        if (!CanStartForge())
        {
            ShowStatusMessage(GameTexts.InventoryFull);
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
        EnsureInventoryManager();
        EnsureAutomationManager();
        RefreshForgeButtonState();

        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged += HandleInventoryChanged;
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= HandleInventoryChanged;
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
        isForging = true;
        RefreshForgeButtonState();

        float duration = anvilManager != null ? anvilManager.GetForgeDuration() : 3f;
        float remaining = duration;

        while (remaining > 0f)
        {
            if (forgeTimerText != null)
                forgeTimerText.text = GameTexts.Forging(GameTexts.FormatDuration(remaining));

            yield return null;
            remaining -= Time.deltaTime;
        }

        if (forgeTimerText != null)
            forgeTimerText.text = string.Empty;

        ItemData forgedItem = itemDatabase.GetRandomItemForEra(GetCurrentEra());
        if (forgedItem == null)
        {
            isForging = false;
            RefreshForgeButtonState();
            yield break;
        }

        EnsureAutomationManager();
        EnsureInventoryManager();

        yield return automationManager != null
            ? automationManager.ProcessForgedItem(forgedItem)
            : AddItemWithoutAutomation(forgedItem);

        saveManager?.SaveGame();

        isForging = false;
        RefreshForgeButtonState();

        automationManager?.NotifyForgeCompleted();
    }

    /// <summary>Forge baslatilabilir mi (envanter veya auto-sell durumuna gore).</summary>
    private bool CanStartForge()
    {
        EnsureAutomationManager();
        EnsureInventoryManager();

        if (automationManager != null && automationManager.CanBypassInventoryForForge())
            return true;

        return inventoryManager == null || inventoryManager.HasFreeSlot;
    }

    /// <summary>Forge butonunun etkilesim durumunu forge ve envanter kapasitesine gore gunceller.</summary>
    private void RefreshForgeButtonState()
    {
        if (forgeButton == null) return;
        forgeButton.interactable = !isForging && CanStartForge();
    }

    private void ShowStatusMessage(string message)
    {
        if (blockedMessageCoroutine != null)
            StopCoroutine(blockedMessageCoroutine);

        blockedMessageCoroutine = StartCoroutine(StatusMessageRoutine(message));
    }

    private IEnumerator StatusMessageRoutine(string message)
    {
        if (forgeTimerText != null)
            forgeTimerText.text = message;

        yield return new WaitForSeconds(2f);

        if (!isForging && forgeTimerText != null)
            forgeTimerText.text = string.Empty;

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

    private IEnumerator AddItemWithoutAutomation(ItemData forgedItem)
    {
        if (inventoryManager == null || !inventoryManager.TryAddItem(forgedItem))
            ShowStatusMessage(GameTexts.InventoryFull);

        yield break;
    }
}
