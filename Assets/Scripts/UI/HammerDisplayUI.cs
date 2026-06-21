using TMPro;
using UnityEngine;

/// <summary>
/// Header'daki cekic sayisini HammerManager ile senkron tutar; yenilenme geri sayimini gosterir.
/// </summary>
public class HammerDisplayUI : MonoBehaviour
{
    [SerializeField] private HammerManager hammerManager;
    [SerializeField] private TextMeshProUGUI hammerText;

    private float refreshTimer;

    private void OnEnable()
    {
        if (hammerManager != null)
            hammerManager.OnHammersChanged += HandleHammersChanged;
    }

    private void OnDisable()
    {
        if (hammerManager != null)
            hammerManager.OnHammersChanged -= HandleHammersChanged;
    }

    private void Start()
    {
        EnsureReferences();
        ApplyHammerStyle();
        RefreshDisplay();
    }

    private void Update()
    {
        if (hammerManager == null) return;

        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = 1f;
            RefreshDisplay();
        }
    }

    /// <summary>Runtime kurulumda referanslari baglar.</summary>
    public void Configure(HammerManager manager, TextMeshProUGUI text)
    {
        if (hammerManager != null)
            hammerManager.OnHammersChanged -= HandleHammersChanged;

        hammerManager = manager;
        hammerText = text;

        if (hammerManager != null)
            hammerManager.OnHammersChanged += HandleHammersChanged;

        ApplyHammerStyle();
        RefreshDisplay();
    }

    /// <summary>Cekic metnini guncel miktar ve geri sayimla yeniler.</summary>
    public void RefreshDisplay()
    {
        EnsureReferences();
        if (hammerManager == null || hammerText == null) return;

        float regenSeconds = hammerManager.GetSecondsUntilNextHammer();
        hammerText.text = GameTexts.HammerAmount(
            hammerManager.CurrentHammers,
            hammerManager.MaxHammers,
            regenSeconds);
    }

    private void HandleHammersChanged()
    {
        RefreshDisplay();
    }

    private void EnsureReferences()
    {
        if (hammerManager == null)
            hammerManager = FindFirstObjectByType<HammerManager>();

        if (hammerText == null)
            hammerText = FindHammerText();
    }

    private static TextMeshProUGUI FindHammerText()
    {
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == "HammerText")
                return texts[i];
        }

        return null;
    }

    private void ApplyHammerStyle()
    {
        if (hammerText == null) return;

        hammerText.color = UITheme.HammerText;
        hammerText.fontSize = 30f;
        hammerText.fontStyle = FontStyles.Bold;
        hammerText.alignment = TextAlignmentOptions.Midline;
        hammerText.outlineWidth = 0.2f;
        hammerText.outlineColor = new Color(0f, 0f, 0f, 0.7f);
        hammerText.raycastTarget = false;
    }
}
