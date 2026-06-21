using UnityEngine;

/// <summary>
/// Auto-sell tier ve cag filtrelerini degerlendirir.
/// </summary>
public static class AutoSellFilter
{
  /// <summary>Cag sirasi (dusukten yuksege).</summary>
  public static readonly string[] EraOrder = { "Stone", "Medieval", "Modern", "Space" };

  /// <summary>Maksimum tier degeri.</summary>
  public const int MinTier = 1;

  /// <summary>Maksimum tier degeri.</summary>
  public const int MaxTier = 4;

  /// <summary>Cag kodunun sirasini dondurur; bilinmeyen cag en yuksek kabul edilir.</summary>
  /// <param name="era">Stone, Medieval, Modern, Space.</param>
  /// <returns>0 tabanli cag indeksi.</returns>
  public static int GetEraIndex(string era)
  {
    for (int i = 0; i < EraOrder.Length; i++)
    {
      if (EraOrder[i] == era)
        return i;
    }

    return EraOrder.Length - 1;
  }

  /// <summary>Indeksten cag kodu dondurur.</summary>
  /// <param name="eraIndex">0 tabanli cag indeksi.</param>
  /// <returns>Cag kodu.</returns>
  public static string GetEraByIndex(int eraIndex)
  {
    int clamped = Mathf.Clamp(eraIndex, 0, EraOrder.Length - 1);
    return EraOrder[clamped];
  }

  /// <summary>Item auto-sell filtresinden gecer mi.</summary>
  /// <param name="item">Degerlendirilen item.</param>
  /// <param name="tierFilterEnabled">Tier filtresi acik mi.</param>
  /// <param name="maxTier">Otomatik satilabilecek maksimum tier.</param>
  /// <param name="eraFilterEnabled">Cag filtresi acik mi.</param>
  /// <param name="maxEraIndex">Otomatik satilabilecek maksimum cag indeksi.</param>
  /// <returns>Filtre kapali veya item kriterlere uyuyorsa true.</returns>
  public static bool PassesFilter(ItemData item, bool tierFilterEnabled, int maxTier, bool eraFilterEnabled,
    int maxEraIndex)
  {
    if (item == null) return false;

    if (tierFilterEnabled && item.Tier > maxTier)
      return false;

    if (eraFilterEnabled && GetEraIndex(item.Era) > maxEraIndex)
      return false;

    return true;
  }
}
