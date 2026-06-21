using TMPro;
using UnityEngine;

/// <summary>
/// Ekrandaki gold yazisini EconomyManager ile senkron tutar.
/// </summary>
public class GoldDisplayUI : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TextMeshProUGUI goldText;

    private void Start()
    {
        EnsureReferences();
        RefreshDisplay();
    }

    /// <summary>Runtime kurulumda referanslari baglar.</summary>
    public void Configure(EconomyManager economy, TextMeshProUGUI text)
    {
        economyManager = economy;
        goldText = text;
        ApplyGoldStyle();
        RefreshDisplay();
    }

    /// <summary>Gold metnini guncel altin miktariyla yeniler.</summary>
    public void RefreshDisplay()
    {
        EnsureReferences();
        if (economyManager == null || goldText == null) return;

        goldText.text = GameTexts.GoldAmount(economyManager.CurrentGold);
    }

    private void EnsureReferences()
    {
        if (economyManager == null)
            economyManager = FindFirstObjectByType<EconomyManager>();

        if (goldText == null)
            goldText = FindGoldText();
    }

    private static TextMeshProUGUI FindGoldText()
    {
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == "GoldText")
                return texts[i];
        }

        return null;
    }

    private void ApplyGoldStyle()
    {
        if (goldText == null) return;

        goldText.color = UITheme.GoldText;
        goldText.fontSize = 38f;
        goldText.fontStyle = FontStyles.Bold;
        goldText.alignment = TextAlignmentOptions.MidlineLeft;
        goldText.outlineWidth = 0.22f;
        goldText.outlineColor = new Color(0f, 0f, 0f, 0.75f);
        goldText.raycastTarget = false;
    }
}
