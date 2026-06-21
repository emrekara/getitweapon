using UnityEngine;

// Oyun arayuzunde kullanilan renk, olcu ve yerlesim sabitleri (1080x1920 referans).
public static class UITheme
{
    public static readonly Color Background = new Color(0.08f, 0.1f, 0.14f, 1f);
    public static readonly Color Panel = new Color(0.12f, 0.15f, 0.21f, 0.95f);
    public static readonly Color PanelBorder = new Color(0.28f, 0.32f, 0.4f, 1f);
    public static readonly Color Header = new Color(0.1f, 0.12f, 0.17f, 1f);
    public static readonly Color GoldText = new Color(1f, 0.84f, 0.31f, 1f);
    public static readonly Color HammerText = new Color(0.95f, 0.72f, 0.45f, 1f);
    public static readonly Color ForgeButton = new Color(0.85f, 0.45f, 0.12f, 1f);
    public static readonly Color ForgeProgressFill = new Color(1f, 0.92f, 0.35f, 1f);
    public static readonly Color ForgeButtonForging = new Color(0.55f, 0.3f, 0.08f, 1f);
    public static readonly Color SellButton = new Color(0.18f, 0.62f, 0.38f, 1f);
    public static readonly Color UpgradeButton = new Color(0.22f, 0.42f, 0.78f, 1f);
    public static readonly Color SlotEmpty = new Color(0.16f, 0.19f, 0.25f, 1f);
    public static readonly Color SlotFilled = new Color(0.2f, 0.24f, 0.32f, 1f);
    public static readonly Color SlotSelected = new Color(0.35f, 0.55f, 0.85f, 1f);
    public static readonly Color BodyText = new Color(0.9f, 0.92f, 0.96f, 1f);
    public static readonly Color MutedText = new Color(0.65f, 0.7f, 0.78f, 1f);
    public static readonly Color ToggleOn = new Color(0.25f, 0.72f, 0.45f, 1f);
    public static readonly Color ToggleOff = new Color(0.22f, 0.25f, 0.32f, 1f);
    public static readonly Color MinigameButton = new Color(0.55f, 0.28f, 0.72f, 1f);

    public const int SlotCount = 8;
    public const int SlotColumns = 4;
    public const int SlotRows = 2;

    public const float ReferenceWidth = 1080f;
    public const float ReferenceHeight = 1920f;

    public const float HeaderHeight = 112f;
    public const float SectionGap = 12f;
    public const float AutoToggleRowHeight = 44f;
    public const float AutoForgePanelHeight = 200f;
    public const float ActionButtonHeight = 72f;
    public const float ForgeButtonHeight = 80f;

    public const float ForgeButtonTopOffset = HeaderHeight + 8f;
    public const float AutoForgePanelTopOffset = ForgeButtonTopOffset + ForgeButtonHeight + SectionGap;
    public const float ForgeZoneHeight = AutoForgePanelTopOffset + AutoForgePanelHeight - HeaderHeight;

    public const float BottomBarHeight = 184f;
    public const float SelectedDetailHeight = 48f;
    public const float HorizontalMargin = 24f;
}
