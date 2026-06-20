using System;

// Kayit dosyasinda tutulan oyun verisi (JSON olarak saklanir).
[Serializable]
public class GameSaveData
{
    public double gold;
    public int lastItemIndex = -1;
    public int anvilLevel = 1;
}
