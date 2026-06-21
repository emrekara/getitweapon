using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Daha iyi forge item'i geldiginde KEEP / SELL sorusu gosterir.
public class ForgeItemPromptUI : MonoBehaviour
{
    [SerializeField] private ForgeAutomationManager automationManager;

    private GameObject panelRoot;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI newItemText;
    private TextMeshProUGUI referenceItemText;
    private bool? pendingDecision;

    /// <summary>Kullanicidan karar bekler; true = tut, false = sat.</summary>
    public IEnumerator PromptForDecision(ItemData newItem, ItemData referenceItem, AnvilManager anvilManager,
        bool inventoryFull)
    {
        BuildIfNeeded();

        pendingDecision = null;
        panelRoot.SetActive(true);

        if (titleText != null)
        {
            titleText.text = inventoryFull && ItemComparer.AreAllStatsEqual(newItem, referenceItem)
                ? GameTexts.PromptInventoryFullSame
                : GameTexts.PromptBetterItem;
        }

        if (newItemText != null)
            newItemText.text = GameTexts.PromptNewItem + "\n" + ItemComparer.FormatItemStats(newItem, anvilManager);

        if (referenceItemText != null)
        {
            referenceItemText.text = GameTexts.PromptBestInInventory + "\n" +
                                     ItemComparer.FormatItemStats(referenceItem, anvilManager);
        }

        yield return new WaitUntil(() => pendingDecision.HasValue);

        panelRoot.SetActive(false);
    }

    /// <summary>Prompt sonucunu dondurur.</summary>
    public bool GetLastDecision() => pendingDecision ?? false;

    private void BuildIfNeeded()
    {
        if (panelRoot != null) return;

        panelRoot = new GameObject("ForgeItemPrompt", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(transform, false);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        GameUILayout.StretchFull(panelRect);

        Image overlay = panelRoot.GetComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.72f);
        overlay.raycastTarget = true;

        GameObject cardObject = new GameObject("PromptCard", typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(panelRoot.transform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(920f, 620f);

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = UITheme.Panel;

        titleText = CreateText(cardObject.transform, "Title", new Vector2(0f, -24f), new Vector2(880f, 48f), 30f,
            FontStyles.Bold, TextAlignmentOptions.Center);

        newItemText = CreateText(cardObject.transform, "NewItem", new Vector2(0f, -100f), new Vector2(880f, 140f), 24f,
            FontStyles.Normal, TextAlignmentOptions.TopLeft);

        referenceItemText = CreateText(cardObject.transform, "ReferenceItem", new Vector2(0f, -260f), new Vector2(880f, 140f),
            24f, FontStyles.Normal, TextAlignmentOptions.TopLeft);

        CreatePromptButton(cardObject.transform, "KeepButton", GameTexts.PromptKeep, new Vector2(0f, -430f),
            UITheme.UpgradeButton, OnKeepClicked);

        CreatePromptButton(cardObject.transform, "SellButton", GameTexts.PromptSellNew, new Vector2(0f, -510f),
            UITheme.SellButton, OnSellClicked);

        panelRoot.SetActive(false);
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size,
        float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = UITheme.BodyText;
        text.raycastTarget = false;
        return text;
    }

    private void CreatePromptButton(Transform parent, string name, string label, Vector2 anchoredPosition, Color color,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(880f, 64f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelText = labelObject.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 26f;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        labelText.raycastTarget = false;
    }

    private void OnKeepClicked()
    {
        pendingDecision = true;
    }

    private void OnSellClicked()
    {
        pendingDecision = false;
    }
}
