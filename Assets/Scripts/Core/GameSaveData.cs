using System;
using UnityEngine;

// Kayit dosyasinda tutulan oyun verisi (JSON olarak saklanir).
[Serializable]
public class GameSaveData
{
    public double gold;
    public int lastItemIndex = -1;
    public int anvilLevel = 1;

    // Envanter slot indeksleri (-1 = bos). Eski kayitlarda bos olabilir.
    public int[] inventoryItemIndices;

    // Secili envanter slotu; secim yoksa -1.
    public int selectedInventorySlot = -1;

    // Otomasyon ayarlari
    public bool autoForgeEnabled;
    public bool autoSellEnabled;

    // Anvil upgrade bitis zamani (Unix saniye). 0 = upgrade yok.
    // NOT: Simdilik cihaz saati; ileride sunucu zamani kullanilacak.
    public double anvilUpgradeEndsAt;

    // Son cikis zamani (Unix saniye). Offline kazanc hesabi icin.
    // NOT: Simdilik cihaz saati; ileride sunucu zamani kullanilacak.
    public double lastQuitTimestamp;
}
