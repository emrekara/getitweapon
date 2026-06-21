#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Gelistirme sirasinda PlayerPrefs kaydini duzenlemek icin Editor menusu.
public static class DebugSaveMenu
{
    private const string SaveKey = "GetItWeapon_Save";

    [MenuItem("GetItWeapon/Debug/Missing Script Bul (Sahne)")]
    private static void FindMissingScriptsInScene()
    {
        int count = 0;

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) <= 0)
                    continue;

                count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                Debug.LogWarning($"[Debug] Missing script: {GetHierarchyPath(transform)}", transform.gameObject);
            }
        }

        if (count == 0)
            Debug.Log("[Debug] Sahnedeki hicbir objede missing script yok.");
        else
            Debug.LogWarning($"[Debug] Toplam {count} missing script bulundu. Console'daki satira tikla, objeyi sec.");
    }

    [MenuItem("GetItWeapon/Debug/Missing Script Bul (Project Assetleri)")]
    private static void FindMissingScriptsInAssets()
    {
        int count = 0;
        string[] assetGuids = AssetDatabase.FindAssets("t:ScriptableObject t:Prefab", new[] { "Assets" });

        foreach (string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".unity"))
                continue;

            Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (mainAsset == null)
            {
                count++;
                Debug.LogWarning($"[Debug] Asset yuklenemedi: {path}");
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(mainAsset);
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
            if (scriptProperty == null) continue;

            if (scriptProperty.objectReferenceValue == null &&
                scriptProperty.objectReferenceInstanceIDValue != 0)
            {
                count++;
                Debug.LogWarning($"[Debug] Missing script: {path}", mainAsset);
            }
        }

        if (count == 0)
            Debug.Log("[Debug] Project assetlerinde missing script yok.");
        else
            Debug.LogWarning($"[Debug] Toplam {count} missing script asset bulundu.");
    }

    [MenuItem("GetItWeapon/Debug/Missing Script Temizle (Sahne)")]
    private static void RemoveMissingScriptsInScene()
    {
        int removed = 0;

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (missingCount <= 0) continue;

                Undo.RegisterCompleteObjectUndo(transform.gameObject, "Remove Missing Scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
                removed += missingCount;
                Debug.Log($"[Debug] Missing script silindi: {GetHierarchyPath(transform)}", transform.gameObject);
            }
        }

        if (removed == 0)
        {
            Debug.Log("[Debug] Temizlenecek missing script yok.");
            return;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[Debug] {removed} missing script kaldirildi. Sahneyi kaydet (Ctrl+S).");
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        Transform current = transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    [MenuItem("GetItWeapon/Debug/Dil: Turkce")]
    private static void SetLanguageTurkish()
    {
        SetLanguage("tr");
    }

    [MenuItem("GetItWeapon/Debug/Dil: English")]
    private static void SetLanguageEnglish()
    {
        SetLanguage("en");
    }

    private static void SetLanguage(string code)
    {
        LocalizationManager.SetLanguage(code);

        GameSaveData data = LoadOrCreate();
        data.languageCode = code;
        WriteSave(data);

        Debug.Log($"[Debug] Dil '{code}' olarak ayarlandi. Play modundaysa UI yenilendi.");
    }

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
