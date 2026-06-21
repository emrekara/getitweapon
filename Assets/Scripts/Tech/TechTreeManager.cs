using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tech tree seviyelerini yonetir; tek slot arastirma timer'i ve carpani saglar.
/// </summary>
public class TechTreeManager : MonoBehaviour
{
  [SerializeField] private TechTreeDatabase database;
  [SerializeField] private EconomyManager economyManager;
  [SerializeField] private AnvilManager anvilManager;
  [SerializeField] private SaveManager saveManager;
  [SerializeField] private GoldDisplayUI goldDisplayUI;

  private readonly Dictionary<string, int> nodeLevels = new Dictionary<string, int>();

  // NOT: Simdilik cihaz UTC; ileride sunucu zamani kullanilacak.
  private string activeResearchNodeId = string.Empty;
  private double researchEndsAtUtc;

  /// <summary>Tech seviyesi veya arastirma durumu degistiginde tetiklenir.</summary>
  public event Action OnTechChanged;

  /// <summary>Arastirma timer'i her karede veya bittiginde UI icin tetiklenir.</summary>
  public event Action OnResearchTimerChanged;

  /// <summary>Aktif arastirma dugumu kimligi; yoksa bos.</summary>
  public string ActiveResearchNodeId => activeResearchNodeId;

  /// <summary>Kayit icin arastirma bitis zamani (Unix saniye); 0 = yok.</summary>
  public double ResearchEndsAtUtc => researchEndsAtUtc;

  /// <summary>Baslatilmis ama henuz bitmemis arastirma var mi.</summary>
  public bool HasPendingResearch =>
    !string.IsNullOrEmpty(activeResearchNodeId) && researchEndsAtUtc > 0;

  /// <summary>Arastirma timer'i aktif mi (kalan sure &gt; 0).</summary>
  public bool IsResearchInProgress => GetRemainingResearchSeconds() > 0f;

  private void Awake()
  {
    EnsureReferences();

    if (database == null)
      database = Resources.Load<TechTreeDatabase>("MainTechTreeDatabase");

    if (database == null || database.Nodes == null || database.Nodes.Count == 0)
      database = TechTreeDatabase.CreateRuntimeFallback();
  }

  private void Update()
  {
    if (!HasPendingResearch) return;

    if (GetRemainingResearchSeconds() <= 0f)
      TryCompleteResearchIfReady();
    else
      OnResearchTimerChanged?.Invoke();
  }

  /// <summary>Forge hiz carpani; 1 = degisiklik yok, 1.1 = %10 daha hizli.</summary>
  public float GetForgeSpeedMultiplier()
  {
    float bonus = GetTotalEffectValue(TechEffectType.ForgeSpeed);
    return 1f + bonus;
  }

  /// <summary>Upgrade maliyet carpani; 1 = degisiklik yok, 0.9 = %10 indirim.</summary>
  public double GetUpgradeCostMultiplier()
  {
    float reduction = GetTotalEffectValue(TechEffectType.UpgradeCostReduction);
    return Math.Max(0.1, 1.0 - reduction);
  }

  /// <summary>Offline gold carpani.</summary>
  public double GetOfflineGoldMultiplier()
  {
    float bonus = GetTotalEffectValue(TechEffectType.OfflineGold);
    return 1.0 + bonus;
  }

  /// <summary>Dugum seviyesini dondurur.</summary>
  /// <param name="nodeId">Dugum kimligi.</param>
  public int GetNodeLevel(string nodeId)
  {
    return nodeLevels.TryGetValue(nodeId, out int level) ? level : 0;
  }

  /// <summary>Kalan arastirma suresi (saniye).</summary>
  public float GetRemainingResearchSeconds()
  {
    if (researchEndsAtUtc <= 0) return 0f;

    double remaining = researchEndsAtUtc - GetUnixTimeNow();
    return remaining > 0 ? (float)remaining : 0f;
  }

  /// <summary>Verilen dugum su an arastiriliyor mu.</summary>
  public bool IsNodeResearching(string nodeId)
  {
    return HasPendingResearch && activeResearchNodeId == nodeId && IsResearchInProgress;
  }

  /// <summary>Dugum acik mi (anvil seviyesi yeterli).</summary>
  public bool IsNodeUnlocked(TechNodeData node)
  {
    if (node == null || anvilManager == null) return false;
    return anvilManager.AnvilLevel >= node.RequiredAnvilLevel;
  }

  /// <summary>Gold harcayarak arastirma baslatir; tek slot.</summary>
  /// <param name="node">Tech dugumu.</param>
  /// <returns>Basarili ise true.</returns>
  public bool TryStartResearch(TechNodeData node)
  {
    if (node == null || economyManager == null) return false;
    if (!IsNodeUnlocked(node)) return false;
    if (HasPendingResearch) return false;

    int currentLevel = GetNodeLevel(node.NodeId);
    if (currentLevel >= node.MaxLevel) return false;

    double cost = node.GetUpgradeCost(currentLevel);
    if (!economyManager.TrySpendGold(cost)) return false;

    if (goldDisplayUI != null)
      goldDisplayUI.RefreshDisplay();

    float duration = node.GetResearchDurationSeconds(currentLevel);
    if (duration <= 0f)
    {
      nodeLevels[node.NodeId] = currentLevel + 1;
      NotifyTechChanged();
      saveManager?.SaveGame();
      return true;
    }

    activeResearchNodeId = node.NodeId;
    researchEndsAtUtc = GetUnixTimeNow() + duration;
    NotifyResearchTimerChanged();
    saveManager?.SaveGame();
    return true;
  }

  /// <summary>Timer bittiyse seviyeyi artirir.</summary>
  public bool TryCompleteResearchIfReady()
  {
    if (!HasPendingResearch) return false;
    if (GetUnixTimeNow() < researchEndsAtUtc) return false;

    if (!nodeLevels.ContainsKey(activeResearchNodeId))
      nodeLevels[activeResearchNodeId] = 0;

    nodeLevels[activeResearchNodeId]++;

    activeResearchNodeId = string.Empty;
    researchEndsAtUtc = 0;

    NotifyTechChanged();
    saveManager?.SaveGame();
    return true;
  }

  /// <summary>Kayit icin tech seviyelerini disa aktarir.</summary>
  public TechNodeSaveEntry[] ExportState()
  {
    if (nodeLevels.Count == 0) return Array.Empty<TechNodeSaveEntry>();

    var entries = new List<TechNodeSaveEntry>();
    foreach (KeyValuePair<string, int> pair in nodeLevels)
    {
      if (pair.Value <= 0) continue;
      entries.Add(new TechNodeSaveEntry { nodeId = pair.Key, level = pair.Value });
    }

    return entries.ToArray();
  }

  /// <summary>Kayit icin aktif arastirma dugum kimligi.</summary>
  public string ExportResearchNodeId() => activeResearchNodeId ?? string.Empty;

  /// <summary>Kayit icin arastirma bitis zamani.</summary>
  public double ExportResearchEndsAt() => researchEndsAtUtc;

  /// <summary>Kayittan tech seviyelerini ve arastirma timer'ini yukler.</summary>
  public void ImportState(TechNodeSaveEntry[] entries, string researchNodeId, double researchEndsAt)
  {
    nodeLevels.Clear();

    if (entries != null)
    {
      for (int i = 0; i < entries.Length; i++)
      {
        TechNodeSaveEntry entry = entries[i];
        if (string.IsNullOrEmpty(entry.nodeId) || entry.level <= 0) continue;
        nodeLevels[entry.nodeId] = entry.level;
      }
    }

    activeResearchNodeId = string.IsNullOrEmpty(researchNodeId) ? string.Empty : researchNodeId;
    researchEndsAtUtc = researchEndsAt > 0 ? researchEndsAt : 0;

    if (TryCompleteResearchIfReady()) return;

    NotifyTechChanged();
  }

  /// <summary>Database referansini dondurur.</summary>
  public TechTreeDatabase Database => database;

  /// <summary>SaveManager veya editor tarafindan database baglar.</summary>
  public void ConfigureDatabase(TechTreeDatabase treeDatabase)
  {
    if (treeDatabase != null)
      database = treeDatabase;
  }

  private float GetTotalEffectValue(TechEffectType effectType)
  {
    if (database == null) return 0f;

    float total = 0f;
    IReadOnlyList<TechNodeData> nodes = database.Nodes;

    for (int i = 0; i < nodes.Count; i++)
    {
      TechNodeData node = nodes[i];
      if (node == null || node.EffectType != effectType) continue;

      int level = GetNodeLevel(node.NodeId);
      total += node.GetTotalEffectValue(level);
    }

    return total;
  }

  private void NotifyTechChanged()
  {
    OnTechChanged?.Invoke();
    OnResearchTimerChanged?.Invoke();
  }

  private void NotifyResearchTimerChanged()
  {
    OnResearchTimerChanged?.Invoke();
  }

  private static double GetUnixTimeNow()
  {
    return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
  }

  private void EnsureReferences()
  {
    if (economyManager == null)
      economyManager = FindFirstObjectByType<EconomyManager>();

    if (anvilManager == null)
      anvilManager = FindFirstObjectByType<AnvilManager>();

    if (saveManager == null)
      saveManager = FindFirstObjectByType<SaveManager>();

    if (goldDisplayUI == null)
      goldDisplayUI = FindFirstObjectByType<GoldDisplayUI>();
  }
}
