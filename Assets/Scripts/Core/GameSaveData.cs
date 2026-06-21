using System;
using UnityEngine;

/// <summary>
/// Kayit dosyasinda tutulan oyun verisi (JSON olarak saklanir).
/// </summary>
[Serializable]
public class GameSaveData
{
    public double gold;

    // Cekic (hammer) meta parasi
    public int hammers;
    public double hammerNextRegenAt;
    public string hammerLastDailyRefillDate = string.Empty;

    public int lastItemIndex = -1;
    public int anvilLevel = 1;

    // Envanter slot indeksleri (-1 = bos). Eski kayitlarda bos olabilir.
    public int[] inventoryItemIndices;

    // Secili envanter slotu; secim yoksa -1.
    public int selectedInventorySlot = -1;

    // Otomasyon ayarlari
    public bool autoForgeEnabled;
    public bool autoSellEnabled;

    // Auto-sell filtreleri (tier / cag)
    public bool autoSellTierFilterEnabled;
    public int autoSellMaxTier = 1;
    public bool autoSellEraFilterEnabled;
    public int autoSellMaxEraIndex;

    // i18n: tr veya en
    public string languageCode = "tr";

    // Tech tree dugum seviyeleri
    public TechNodeSaveEntry[] techNodeLevels;

    // Tek slot aktif arastirma (nodeId bos = yok)
    public string techResearchNodeId = string.Empty;

    // Arastirma bitis zamani (Unix saniye). 0 = arastirma yok.
    public double techResearchEndsAt;

    // Anvil upgrade bitis zamani (Unix saniye). 0 = upgrade yok.
    // NOT: Simdilik cihaz saati; ileride sunucu zamani kullanilacak.
    public double anvilUpgradeEndsAt;

    // Son cikis zamani (Unix saniye). Offline kazanc hesabi icin.
    // NOT: Simdilik cihaz saati; ileride sunucu zamani kullanilacak.
    public double lastQuitTimestamp;
}

/// <summary>
/// Tech tree dugum seviyesi kayit girdisi.
/// </summary>
[Serializable]
public class TechNodeSaveEntry
{
    public string nodeId;
    public int level;
}
