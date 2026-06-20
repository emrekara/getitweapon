using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Anvil bilgisini gosterir ve UPGRADE butonuna basildiginda seviye yukseltir.
public class AnvilUpgradeHandler : MonoBehaviour
{
    [SerializeField] private AnvilManager anvilManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GoldDisplayUI goldDisplayUI;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TextMeshProUGUI anvilInfoText;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    private void Start()
    {
        if (upgradeButtonText != null)
            upgradeButtonText.raycastTarget = false;

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnUpgradeClicked);
            button.onClick.AddListener(OnUpgradeClicked);
        }

        // SaveManager yuklemesini bekle, sonra guncel maliyeti goster
        StartCoroutine(RefreshAfterLoad());
    }

    private IEnumerator RefreshAfterLoad()
    {
        yield return null;
        RefreshDisplay();
    }

    /// <summary>Upgrade butonu OnClick olayina baglanir.</summary>
    public void OnUpgradeClicked()
    {
        if (anvilManager == null || economyManager == null) return;

        double cost = anvilManager.GetUpgradeCost();

        if (economyManager.CurrentGold < cost)
        {
            if (upgradeButtonText != null)
                upgradeButtonText.text = $"Need {cost:0}g";
            return;
        }

        if (!anvilManager.TryUpgrade(economyManager)) return;

        if (goldDisplayUI != null)
            goldDisplayUI.RefreshDisplay();

        RefreshDisplay();
        saveManager?.SaveGame();
    }

    /// <summary>Kayit yuklendikten sonra disaridan cagrilabilir.</summary>
    public void RefreshDisplay()
    {
        if (anvilManager == null) return;

        if (anvilInfoText != null)
        {
            anvilInfoText.text = $"Anvil Lv.{anvilManager.AnvilLevel} - {anvilManager.CurrentEra}";
        }

        if (upgradeButtonText != null)
        {
            upgradeButtonText.text = $"UPGRADE ({anvilManager.GetUpgradeCost():0}g)";
        }
    }
}
