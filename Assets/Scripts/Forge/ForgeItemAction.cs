/// <summary>Forge sonrasi item ile yapilacak islem.</summary>
public enum ForgeItemAction
{
    /// <summary>Envantere ekle (bos kategori slotu veya yeni kategori).</summary>
    AddToInventory,

    /// <summary>OTO SAT acikken otomatik satis.</summary>
    AutoSell,

    /// <summary>OTO SAT kapaliyken zayif/esit kopya — satis yok, OTO DÖV zinciri durur.</summary>
    RejectDuplicate,

    /// <summary>TUT / YENİSİNİ SAT popup.</summary>
    PromptUser,

    /// <summary>Islem yapilamaz (ornegin envanter dolu).</summary>
    None
}

/// <summary>
/// Forge sonrasi karar icin girdi; ProcessForgedItem ile ayni mantik.
/// </summary>
public struct ForgeProcessContext
{
    public bool AutoSellEnabled;
    public bool PassesAutoSellFilter;
    public bool ContainsCategory;
    public ItemData CategoryItem;
    public bool InventoryFull;
    public int UsedSlotCount;
    public ItemData BestReference;
}

/// <summary>
/// Forge sonrasi item kararini verir; ForgeAutomationManager ve testler kullanir.
/// </summary>
public static class ForgeItemActionResolver
{
    /// <summary>Forge edilen item icin yapilacak aksiyonu dondurur.</summary>
    public static ForgeItemAction Decide(ItemData forged, ForgeProcessContext context)
    {
        if (forged == null)
            return ForgeItemAction.None;

        if (context.ContainsCategory && context.CategoryItem != null)
        {
            if (ItemComparer.IsStrictlyWorse(forged, context.CategoryItem) ||
                ItemComparer.AreAllStatsEqual(forged, context.CategoryItem))
                return context.AutoSellEnabled
                    ? ForgeItemAction.AutoSell
                    : ForgeItemAction.RejectDuplicate;

            return ForgeItemAction.PromptUser;
        }

        if (!context.AutoSellEnabled)
            return context.InventoryFull ? ForgeItemAction.None : ForgeItemAction.AddToInventory;

        if (!context.PassesAutoSellFilter)
            return context.InventoryFull ? ForgeItemAction.None : ForgeItemAction.AddToInventory;

        if (context.UsedSlotCount == 0)
            return ForgeItemAction.AddToInventory;

        if (context.BestReference == null)
            return ForgeItemAction.AddToInventory;

        if (ItemComparer.IsStrictlyWorse(forged, context.BestReference))
            return ForgeItemAction.AutoSell;

        if (ItemComparer.HasAnyStatHigher(forged, context.BestReference))
            return ForgeItemAction.PromptUser;

        if (!context.InventoryFull)
            return ForgeItemAction.AddToInventory;

        if (context.InventoryFull && ItemComparer.AreAllStatsEqual(forged, context.BestReference))
            return ForgeItemAction.PromptUser;

        return ForgeItemAction.AutoSell;
    }
}
