using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Forge sirasinda sans mini oyunu cubugu ve puan gosterimi.</summary>
public class MinigamePanelUI : MonoBehaviour
{
    [SerializeField] private MinigameManager minigameManager;
    [SerializeField] private ForgeButtonHandler forgeButtonHandler;

    private GameObject barRoot;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI feedbackText;
    private TextMeshProUGUI playButtonLabel;
    private Button playButton;
    private float feedbackTimer;

    private void Start()
    {
        if (!MinigameFeatures.UiEnabled)
        {
            DestroyBarIfExists();
            enabled = false;
            return;
        }

        EnsureReferences();
        RebuildBar();
        Subscribe();
        Refresh();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0f)
                RefreshScoreLine();
        }

        RefreshPlayButton();
    }

    public void Configure(MinigameManager manager, ForgeButtonHandler forgeHandler)
    {
        if (!MinigameFeatures.UiEnabled)
        {
            DestroyBarIfExists();
            return;
        }

        Unsubscribe();
        minigameManager = manager;
        forgeButtonHandler = forgeHandler;
        RebuildBar();
        Subscribe();
        Refresh();
    }

    private void Subscribe()
    {
        if (minigameManager != null)
            minigameManager.OnMinigameStateChanged += Refresh;

        if (forgeButtonHandler != null)
            forgeButtonHandler.OnForgeStateChanged += Refresh;

        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Unsubscribe()
    {
        if (minigameManager != null)
            minigameManager.OnMinigameStateChanged -= Refresh;

        if (forgeButtonHandler != null)
            forgeButtonHandler.OnForgeStateChanged -= Refresh;

        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        RefreshPlayButton();
        RefreshScoreLine();
    }

    private void EnsureReferences()
    {
        if (minigameManager == null)
            minigameManager = FindFirstObjectByType<MinigameManager>();

        if (forgeButtonHandler == null)
            forgeButtonHandler = FindFirstObjectByType<ForgeButtonHandler>();
    }

    private void RebuildBar()
    {
        DestroyBarIfExists();

        barRoot = new GameObject("MinigameBar", typeof(RectTransform), typeof(Image));
        barRoot.transform.SetParent(transform, false);

        RectTransform barRect = barRoot.GetComponent<RectTransform>();
        GameUILayout.AnchorTopCenter(barRect, UITheme.ForgeButtonTopOffset + UITheme.ForgeButtonHeight + UITheme.SectionGap,
            new Vector2(920f, 52f));

        Image barImage = barRoot.GetComponent<Image>();
        barImage.color = UITheme.Panel;
        barImage.raycastTarget = false;

        GameObject scoreObject = new GameObject("MinigameScoreText", typeof(RectTransform));
        scoreObject.transform.SetParent(barRoot.transform, false);
        scoreText = scoreObject.AddComponent<TextMeshProUGUI>();
        ApplyStretch(scoreText.rectTransform, 0f, 0.54f, 16f, 8f, -8f, -8f);
        scoreText.fontSize = 24f;
        scoreText.color = UITheme.BodyText;
        scoreText.alignment = TextAlignmentOptions.MidlineLeft;
        scoreText.raycastTarget = false;

        GameObject feedbackObject = new GameObject("MinigameFeedbackText", typeof(RectTransform));
        feedbackObject.transform.SetParent(barRoot.transform, false);
        feedbackText = feedbackObject.AddComponent<TextMeshProUGUI>();
        ApplyStretch(feedbackText.rectTransform, 0f, 0.54f, 16f, 8f, -8f, -8f);
        feedbackText.fontSize = 24f;
        feedbackText.color = UITheme.GoldText;
        feedbackText.alignment = TextAlignmentOptions.MidlineLeft;
        feedbackText.raycastTarget = false;
        feedbackText.gameObject.SetActive(false);

        GameObject buttonObject = new GameObject("MinigamePlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(barRoot.transform, false);
        ApplyStretch(buttonObject.GetComponent<RectTransform>(), 0.56f, 1f, 8f, 6f, -16f, -6f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = UITheme.ToggleOff;
        playButton = buttonObject.GetComponent<Button>();
        playButton.onClick.AddListener(OnPlayClicked);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);
        playButtonLabel = labelObject.AddComponent<TextMeshProUGUI>();
        ApplyStretch(playButtonLabel.rectTransform, 0f, 1f, 8f, 0f, -8f, 0f);
        playButtonLabel.text = GameTexts.MinigameWaitForge;
        playButtonLabel.fontSize = 22f;
        playButtonLabel.fontStyle = FontStyles.Bold;
        playButtonLabel.color = Color.white;
        playButtonLabel.alignment = TextAlignmentOptions.Center;
        playButtonLabel.raycastTarget = false;
    }

    private static void ApplyStretch(RectTransform rect, float anchorMinX, float anchorMaxX,
        float left, float top, float right, float bottom)
    {
        rect.anchorMin = new Vector2(anchorMinX, 0f);
        rect.anchorMax = new Vector2(anchorMaxX, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(right, -top);
    }

    private void DestroyBarIfExists()
    {
        Transform existing = transform.Find("MinigameBar");
        if (existing == null) return;

        if (Application.isPlaying)
            Destroy(existing.gameObject);
        else
            DestroyImmediate(existing.gameObject);

        barRoot = null;
        scoreText = null;
        feedbackText = null;
        playButton = null;
        playButtonLabel = null;
    }

    private void OnPlayClicked()
    {
        if (minigameManager == null) return;

        int points = minigameManager.TryPlayLuckRound();
        if (points < 0)
        {
            ShowFeedback(GameTexts.MinigameAlreadyPlayed);
            return;
        }

        ShowFeedback(GameTexts.MinigameResult(points));
        Refresh();
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null || scoreText == null) return;

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(false);
        feedbackTimer = 1.8f;
    }

    private void RefreshScoreLine()
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
            feedbackText.gameObject.SetActive(false);
        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            if (minigameManager != null)
                scoreText.text = GameTexts.MinigameScore(minigameManager.TotalScore);
        }
    }

    private void Refresh()
    {
        RefreshScoreLine();
        RefreshPlayButton();
    }

    private void RefreshPlayButton()
    {
        if (playButton == null || playButtonLabel == null ||
            minigameManager == null || forgeButtonHandler == null)
            return;

        bool forging = forgeButtonHandler.IsForging;
        bool canPlay = forging && minigameManager.CanPlayThisForge;
        playButton.interactable = canPlay;
        playButtonLabel.text = canPlay ? GameTexts.MinigamePlayButton : GameTexts.MinigameWaitForge;

        Image image = playButton.GetComponent<Image>();
        if (image != null)
            image.color = canPlay ? UITheme.MinigameButton : UITheme.ToggleOff;
    }
}
