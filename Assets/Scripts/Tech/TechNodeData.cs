using UnityEngine;

/// <summary>
/// Tek bir tech tree dugumu tanimi (ScriptableObject).
/// </summary>
[CreateAssetMenu(fileName = "NewTechNode", menuName = "GetItWeapon/Tech Node")]
public class TechNodeData : ScriptableObject
{
  [SerializeField] private string nodeId = "forge_speed";
  [SerializeField] private LocalizationKey displayNameKey = LocalizationKey.TechNodeForgeSpeed;
  [SerializeField] private TechEffectType effectType = TechEffectType.ForgeSpeed;
  [SerializeField] private int maxLevel = 5;
  [SerializeField] private float valuePerLevel = 0.1f;
  [SerializeField] private double baseUpgradeCost = 50;
  [SerializeField] private double costScalePerLevel = 1.5;
  [SerializeField] private float baseResearchDurationSeconds = 15f;
  [SerializeField] private float durationScalePerLevel = 1.25f;
  [SerializeField] private int requiredAnvilLevel = 1;

  /// <summary>Benzersiz dugum kimligi.</summary>
  public string NodeId => nodeId;

  /// <summary>UI'da gosterilecek lokalizasyon anahtari.</summary>
  public LocalizationKey DisplayNameKey => displayNameKey;

  /// <summary>Etki turu.</summary>
  public TechEffectType EffectType => effectType;

  /// <summary>Maksimum seviye.</summary>
  public int MaxLevel => maxLevel;

  /// <summary>Seviye basina etki degeri (yuzde veya carpansal).</summary>
  public float ValuePerLevel => valuePerLevel;

  /// <summary>Ilk seviye yukseltme maliyeti.</summary>
  public double BaseUpgradeCost => baseUpgradeCost;

  /// <summary>Her seviyede maliyet carpani.</summary>
  public double CostScalePerLevel => costScalePerLevel;

  /// <summary>Ilk seviye arastirma suresi (saniye).</summary>
  public float BaseResearchDurationSeconds => baseResearchDurationSeconds;

  /// <summary>Her seviyede sure carpani.</summary>
  public float DurationScalePerLevel => durationScalePerLevel;

  /// <summary>Acilmasi icin gereken minimum anvil seviyesi.</summary>
  public int RequiredAnvilLevel => requiredAnvilLevel;

  /// <summary>Runtime fallback dugumleri icin alanlari ayarlar.</summary>
  public void InitializeRuntime(string id, LocalizationKey nameKey, TechEffectType type, int maxLvl,
    float perLevel, double baseCost, double costScale, int requiredLevel,
    float baseDurationSeconds, float durationScale)
  {
    nodeId = id;
    displayNameKey = nameKey;
    effectType = type;
    maxLevel = maxLvl;
    valuePerLevel = perLevel;
    baseUpgradeCost = baseCost;
    costScalePerLevel = costScale;
    requiredAnvilLevel = requiredLevel;
    baseResearchDurationSeconds = baseDurationSeconds;
    durationScalePerLevel = durationScale;
  }

  /// <summary>Belirli seviye icin yukseltme maliyetini hesaplar.</summary>
  /// <param name="currentLevel">Mevcut seviye (0 tabanli sonraki yukseltme icin).</param>
  /// <returns>Gold maliyeti.</returns>
  public double GetUpgradeCost(int currentLevel)
  {
    if (currentLevel >= maxLevel) return 0;
    return baseUpgradeCost * System.Math.Pow(costScalePerLevel, currentLevel);
  }

  /// <summary>Belirli seviye icin arastirma suresini hesaplar (saniye).</summary>
  /// <param name="currentLevel">Mevcut seviye.</param>
  /// <returns>Arastirma suresi; 0 = aninda.</returns>
  public float GetResearchDurationSeconds(int currentLevel)
  {
    if (currentLevel >= maxLevel) return 0f;

    float duration = baseResearchDurationSeconds *
                     Mathf.Pow(durationScalePerLevel, currentLevel);

    return Mathf.Max(1f, duration);
  }

  /// <summary>Belirli seviyedeki toplam etki degerini dondurur.</summary>
  /// <param name="level">Mevcut seviye.</param>
  /// <returns>Toplam etki.</returns>
  public float GetTotalEffectValue(int level)
  {
    return valuePerLevel * Mathf.Clamp(level, 0, maxLevel);
  }
}
