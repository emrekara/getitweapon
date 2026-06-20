using TMPro;
using UnityEngine;

// Ekrandaki gold yazisini EconomyManager ile senkron tutar.
public class GoldDisplayUI : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TextMeshProUGUI goldText;

    private void Start()
    {
        RefreshDisplay();
    }

    /// <summary>Gold metnini guncel altin miktariyla yeniler.</summary>
    public void RefreshDisplay()
    {
        if (economyManager == null || goldText == null) return;

        goldText.text = $"Gold: {economyManager.CurrentGold:0}";
    }
}
