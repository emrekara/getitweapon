#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sahne UI yerlesimini tek tikla yeniden kurar; missing script ve editor artiklarini temizler.
public static class SceneBootstrap
{
    [MenuItem("GetItWeapon/Setup Main UI")]
    private static void SetupMainUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Bootstrap] Canvas bulunamadi.");
            return;
        }

        SaveManager saveManager = Object.FindFirstObjectByType<SaveManager>();
        if (saveManager == null)
        {
            Debug.LogError("[Bootstrap] SaveManager bulunamadi.");
            return;
        }

        int removedMissing = RemoveMissingScriptsInScene();
        CleanupRuntimeUiArtifacts(canvas.transform);

        EnsureSingleComponent<GameUILayout>(canvas.gameObject);
        EnsureSingleComponent<InventoryPanelUI>(canvas.gameObject);

        InventoryManager inventoryManager = saveManager.GetComponent<InventoryManager>();
        if (inventoryManager == null)
            inventoryManager = saveManager.gameObject.AddComponent<InventoryManager>();

        ForgeButtonHandler forgeHandler = Object.FindFirstObjectByType<ForgeButtonHandler>();
        if (forgeHandler != null && forgeHandler.ItemDatabase != null)
            inventoryManager.Configure(forgeHandler.ItemDatabase);

        GameUILayout layout = canvas.GetComponent<GameUILayout>();
        if (layout == null)
            layout = canvas.gameObject.AddComponent<GameUILayout>();

        layout.ApplyEditorSetup();
        GameUiBootstrap.EnsureApplied();

        HideLegacyObject(canvas.transform, "ItemIcon");
        HideLegacyObject(canvas.transform, "LastItemText");

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[Bootstrap] Main UI kuruldu. {removedMissing} missing script temizlendi. Sahneyi kaydet (Ctrl+S) ve Play'e bas.");
    }

    [MenuItem("GetItWeapon/Fix Missing Scripts (Sahne)")]
    private static void FixMissingScriptsMenu()
    {
        int removed = RemoveMissingScriptsInScene();
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
            CleanupRuntimeUiArtifacts(canvas.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(removed > 0
            ? $"[Bootstrap] {removed} missing script kaldirildi. Ctrl+S ile kaydet."
            : "[Bootstrap] Missing script bulunamadi; runtime UI artiklari temizlendi.");
    }

    /// <summary>Editor'de olusturulmus gecici envanter UI objelerini siler.</summary>
    private static void CleanupRuntimeUiArtifacts(Transform canvas)
    {
        DestroyIfExists(canvas, "InventoryPanel");
        DestroyIfExists(canvas, "SelectedItemText");
        DestroyIfExists(canvas, "Background");
        DestroyIfExists(canvas, "HeaderPanel");
        DestroyIfExists(canvas, "AutoForgePanel");
        DestroyIfExists(canvas, "TechTreeToggle");
        DestroyIfExists(canvas, "TechTreePanel");
    }

    private static void DestroyIfExists(Transform parent, string objectName)
    {
        Transform target = parent.Find(objectName);
        if (target != null)
            Object.DestroyImmediate(target.gameObject);
    }

    private static void EnsureSingleComponent<T>(GameObject gameObject) where T : Component
    {
        T[] components = gameObject.GetComponents<T>();
        if (components.Length == 0)
        {
            gameObject.AddComponent<T>();
            return;
        }

        for (int i = 1; i < components.Length; i++)
            Object.DestroyImmediate(components[i]);
    }

    private static int RemoveMissingScriptsInScene()
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
            }
        }

        return removed;
    }

    private static void HideLegacyObject(Transform canvas, string objectName)
    {
        Transform target = canvas.Find(objectName);
        if (target != null)
            target.gameObject.SetActive(false);
    }
}
#endif
