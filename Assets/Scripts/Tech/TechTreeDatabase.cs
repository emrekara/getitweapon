using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tech tree dugum listesi (ScriptableObject).
/// </summary>
[CreateAssetMenu(fileName = "NewTechTreeDatabase", menuName = "GetItWeapon/Tech Tree Database")]
public class TechTreeDatabase : ScriptableObject
{
  [SerializeField] private List<TechNodeData> nodes = new List<TechNodeData>();

  /// <summary>Tum dugumler.</summary>
  public IReadOnlyList<TechNodeData> Nodes => nodes;

  /// <summary>NodeId ile dugum arar.</summary>
  public TechNodeData GetNode(string nodeId)
  {
    for (int i = 0; i < nodes.Count; i++)
    {
      if (nodes[i] != null && nodes[i].NodeId == nodeId)
        return nodes[i];
    }

    return null;
  }

  /// <summary>Runtime fallback: bos database icin varsayilan dugumleri olusturur.</summary>
  public static TechTreeDatabase CreateRuntimeFallback()
  {
    TechTreeDatabase database = CreateInstance<TechTreeDatabase>();
    database.nodes = new List<TechNodeData>
    {
      CreateRuntimeNode("forge_speed", LocalizationKey.TechNodeForgeSpeed, TechEffectType.ForgeSpeed,
        5, 0.1f, 50, 1.5, 1, 15f, 1.25f),
      CreateRuntimeNode("upgrade_cost", LocalizationKey.TechNodeUpgradeCost, TechEffectType.UpgradeCostReduction,
        5, 0.05f, 75, 1.6, 3, 20f, 1.3f),
      CreateRuntimeNode("offline_gold", LocalizationKey.TechNodeOfflineGold, TechEffectType.OfflineGold,
        5, 0.08f, 60, 1.5, 2, 18f, 1.25f)
    };
    return database;
  }

  private static TechNodeData CreateRuntimeNode(string nodeId, LocalizationKey nameKey, TechEffectType effectType,
    int maxLevel, float valuePerLevel, double baseCost, double costScale, int requiredAnvilLevel,
    float baseDurationSeconds, float durationScale)
  {
    TechNodeData node = CreateInstance<TechNodeData>();
    node.InitializeRuntime(nodeId, nameKey, effectType, maxLevel, valuePerLevel, baseCost, costScale,
      requiredAnvilLevel, baseDurationSeconds, durationScale);
    return node;
  }
}
