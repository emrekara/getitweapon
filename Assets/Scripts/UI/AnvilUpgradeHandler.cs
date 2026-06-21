using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Anvil bilgisini gosterir; UPGRADE basildiginda timer baslatir ve geri sayimi gosterir.
public class AnvilUpgradeHandler : MonoBehaviour
{
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TextMeshProUGUI anvilInfoText;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    private Button upgradeButton;
    private Coroutine upgradeTimerCoroutine;

    private void Awake()
    {
        upgradeButton = GetComponent<Button>();
    }

    /// <summary>Runtime HUD referanslarini baglar.</summary>
    public void ConfigureHud(TextMeshProUGUI anvilText, GoldDisplayUI goldDisplay)
    {
        if (anvilText != null)
            anvilInfoText = anvilText;

        if (goldDisplay != null)
            goldDisplayUI = goldDisplay;

        RefreshDisplay();
    }

    private void Start()
    {
        if (upgradeButtonText != null)
            upgradeButtonText.raycastTarget = false;

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        StartCoroutine(RefreshAfterLoad());
    }

    private void OnEnable()
    {
        if (anvilManager != null)
            anvilManager.OnUpgradeTimerChanged += HandleUpgradeTimerChanged;

        LocalizationManager.OnLanguageChanged += RefreshDisplay;
    }

    private void OnDisable()
    {
        if (anvilManager != null)
            anvilManager.OnUpgradeTimerChanged -= HandleUpgradeTimerChanged;

        LocalizationManager.OnLanguageChanged -= RefreshDisplay;
    }

    private System.Collections.IEnumerator RefreshAfterLoad()
    {
        yield return null;

        if (anvilManager != null && anvilManager.TryCompleteUpgradeIfReady())
        {
            if (goldDisplayUI != null)
                goldDisplayUI.RefreshDisplay();

            saveManager?.SaveGame();
        }

        RefreshDisplay();

        if (anvilManager != null && anvilManager.IsUpgradeInProgress)
            StartUpgradeTimerRoutine();
    }

    private void HandleUpgradeTimerChanged()
    {
        RefreshDisplay();
    }

    /// <summary>Upgrade butonu OnClick olayina baglanir.</summary>
    public void OnUpgradeClicked()
    {
        if (anvilManager == null || economyManager == null) return;
        if (anvilManager.IsUpgradeInProgress || anvilManager.HasPendingUpgrade) return;

        double cost = anvilManager.GetUpgradeCost();

        if (economyManager.CurrentGold < cost)
        {
            if (upgradeButtonText != null)
                upgradeButtonText.text = GameTexts.NeedGold(cost);
            return;
        }

        if (!anvilManager.TryStartUpgrade(economyManager)) return;

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();

        RefreshDisplay();
        saveManager?.SaveGame();

        if (anvilManager.IsUpgradeInProgress)
            StartUpgradeTimerRoutine();
    }

    private void StartUpgradeTimerRoutine()
    {
        if (upgradeTimerCoroutine != null)
            StopCoroutine(upgradeTimerCoroutine);

        upgradeTimerCoroutine = StartCoroutine(UpgradeTimerRoutine());
    }

    private System.Collections.IEnumerator UpgradeTimerRoutine()
    {
        if (upgradeButton != null)
            upgradeButton.interactable = false;

        while (anvilManager != null && anvilManager.IsUpgradeInProgress)
        {
            RefreshDisplay();
            yield return null;
        }

        if (anvilManager != null && anvilManager.TryCompleteUpgradeIfReady())
        {
            if (goldDisplayUI != null)
                goldDisplayUI.RefreshDisplay();

            saveManager?.SaveGame();
        }

        RefreshDisplay();

        if (upgradeButton != null)
            upgradeButton.interactable = true;

        upgradeTimerCoroutine = null;
    }

    /// <summary>Kayit yuklendikten sonra disaridan cagrilabilir.</summary>
    public void RefreshDisplay()
    {
        if (anvilManager == null) return;

        if (anvilInfoText != null)
        {
            string info = GameTexts.AnvilInfo(anvilManager.AnvilLevel, anvilManager.CurrentEra);

            if (anvilManager.IsUpgradeInProgress)
                info += $"\n{GameTexts.AnvilUpgrading(GameTexts.FormatDuration(anvilManager.GetRemainingUpgradeSeconds()))}";

            anvilInfoText.text = info;
        }

        if (upgradeButtonText != null)
        {
            if (anvilManager.IsUpgradeInProgress)
            {
                upgradeButtonText.text = GameTexts.UpgradeInProgressButton(
                    GameTexts.FormatDuration(anvilManager.GetRemainingUpgradeSeconds()));
            }
            else
            {
                double cost = anvilManager.GetUpgradeCost();
                float nextDuration = anvilManager.GetUpgradeDurationSeconds();

                upgradeButtonText.text = nextDuration > 0f
                    ? GameTexts.UpgradeButtonLabel(cost, GameTexts.FormatDuration(nextDuration))
                    : GameTexts.UpgradeButtonLabel(cost);
            }
        }
    }
}
