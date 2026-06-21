using UnityEngine;

// Item ozelliklerini karsilastirir; auto-sell ve upgrade sorusu icin kullanilir.
public static class ItemComparer
{
    /// <summary>Yeni item'da referansa gore en az bir ozellik daha yuksek mi.</summary>
    public static bool HasAnyStatHigher(ItemData newItem, ItemData reference)
    {
        if (newItem == null || reference == null) return false;

        return newItem.BaseAttack > reference.BaseAttack
               || newItem.BaseDefense > reference.BaseDefense
               || newItem.Tier > reference.Tier
               || newItem.SellPrice > reference.SellPrice;
    }

    /// <summary>Item referanstan en az bir ozellikte dusuk mu.</summary>
    public static bool HasAnyStatLower(ItemData item, ItemData reference)
    {
        if (item == null || reference == null) return false;

        return item.BaseAttack < reference.BaseAttack
               || item.BaseDefense < reference.BaseDefense
               || item.Tier < reference.Tier
               || item.SellPrice < reference.SellPrice;
    }

    /// <summary>Tum karsilastirma ozellikleri esit mi.</summary>
    public static bool AreAllStatsEqual(ItemData left, ItemData right)
    {
        if (left == null || right == null) return false;

        return left.BaseAttack == right.BaseAttack
               && left.BaseDefense == right.BaseDefense
               && left.Tier == right.Tier
               && left.SellPrice == right.SellPrice;
    }

    /// <summary>Item referanstan katı olarak dusuk mu (hicbir ozellik ustun degil, en az biri dusuk).</summary>
    public static bool IsStrictlyWorse(ItemData item, ItemData reference)
    {
        if (item == null || reference == null) return false;
        return !HasAnyStatHigher(item, reference) && HasAnyStatLower(item, reference);
    }

    /// <summary>Item referanstan katı olarak dusuk veya esit mi.</summary>
    public static bool IsStrictlyWorseOrEqual(ItemData item, ItemData reference)
    {
        if (item == null || reference == null) return false;
        return !HasAnyStatHigher(item, reference);
    }

    /// <summary>Auto-sell sirasinda kullaniciya sorulmali mi.</summary>
    public static bool ShouldPromptUser(ItemData newItem, ItemData bestReference, bool inventoryFull)
    {
        if (newItem == null || bestReference == null) return false;

        if (HasAnyStatHigher(newItem, bestReference))
            return true;

        return inventoryFull && AreAllStatsEqual(newItem, bestReference);
    }

    /// <summary>Karsilastirma icin okunabilir ozet metin.</summary>
    public static string FormatItemStats(ItemData item, AnvilManager anvilManager)
    {
        if (item == null) return string.Empty;

        double sellPrice = anvilManager != null
            ? anvilManager.GetScaledSellPrice(item.SellPrice)
            : item.SellPrice;

        return GameTexts.ItemStats(item.ItemName, item.BaseAttack, item.BaseDefense, item.Tier, sellPrice);
    }
}
