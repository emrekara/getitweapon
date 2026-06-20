#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Gelistirme sirasinda PlayerPrefs kaydini duzenlemek icin Editor menusu.
public static class DebugSaveMenu
{
    private const string SaveKey = "GetItWeapon_Save";

    [MenuItem("GetItWeapon/Debug/Kaydi Sil (Reset Save)")]
    private static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        Debug.Log("[Debug] Kayit silindi. Sonraki Play'de Starting Gold kullanilir.");
    }

    [MenuItem("GetItWeapon/Debug/Gold +1000 Ekle")]
    private static void AddGold1000()
    {
        AddGold(1000);
    }

    [MenuItem("GetItWeapon/Debug/Gold +100 Ekle")]
    private static void AddGold100()
    {
        AddGold(100);
    }

    [MenuItem("GetItWeapon/Debug/Gold Ekle (Input)...")]
    private static void OpenGoldInputWindow()
    {
        GoldInputWindow.ShowWindow();
    }

    [MenuItem("GetItWeapon/Debug/Kaydi Console'a Yaz")]
    private static void PrintSave()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("[Debug] Kayit yok.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        Debug.Log($"[Debug] Kayit JSON:\n{json}");
    }

    /// <summary>Kayittaki gold'a miktar ekler; Play modundaysa ekrani da gunceller.</summary>
    public static void AddGold(double amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[Debug] Eklenecek gold 0'dan buyuk olmali.");
            return;
        }

        GameSaveData data = LoadOrCreate();
        data.gold += amount;
        WriteSave(data);
        ApplyGoldIfPlaying(data.gold);

        Debug.Log($"[Debug] +{amount:0} gold eklendi. Toplam: {data.gold:0}");
    }

    private static void ApplyGoldIfPlaying(double totalGold)
    {
        if (!Application.isPlaying) return;

        EconomyManager economy = Object.FindFirstObjectByType<EconomyManager>();
        if (economy != null)
            economy.SetGold(totalGold);

        GoldDisplayUI goldUI = Object.FindFirstObjectByType<GoldDisplayUI>();
        if (goldUI != null)
            goldUI.RefreshDisplay();
    }

    private static GameSaveData LoadOrCreate()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return new GameSaveData();

        return JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(SaveKey));
    }

    private static void WriteSave(GameSaveData data)
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}

/// <summary>Editor penceresi: istenen miktarda gold ekler.</summary>
public class GoldInputWindow : EditorWindow
{
    private string goldInput = "99999999";

    public static void ShowWindow()
    {
        GoldInputWindow window = GetWindow<GoldInputWindow>("Gold Ekle");
        window.minSize = new Vector2(320, 120);
        window.maxSize = new Vector2(320, 120);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Test icin kayda gold ekle", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(6);

        goldInput = EditorGUILayout.TextField("Eklenecek Gold", goldInput);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Ekle", GUILayout.Height(28)))
        {
            if (!double.TryParse(goldInput, out double amount))
            {
                EditorUtility.DisplayDialog("Hata", "Gecerli bir sayi gir.", "Tamam");
                return;
            }

            DebugSaveMenu.AddGold(amount);
        }
    }
}
#endif
