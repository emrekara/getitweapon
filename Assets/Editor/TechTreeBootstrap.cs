#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tech tree ScriptableObject assetlerini ve Resources database'ini olusturur.
/// </summary>
public static class TechTreeBootstrap
{
    private const string TechFolder = "Assets/ScriptableObjects/TechTree";
    private const string ResourcesFolder = "Assets/Resources";
    private const string DatabaseResourcePath = "Assets/Resources/MainTechTreeDatabase.asset";

    [MenuItem("GetItWeapon/Setup Tech Tree")]
    public static void SetupTechTree()
    {
        EnsureFolder(TechFolder);
        EnsureFolder(ResourcesFolder);

        TechNodeData forgeSpeed = CreateOrLoadNode(
            $"{TechFolder}/TechNode_ForgeSpeed.asset",
            "forge_speed",
            LocalizationKey.TechNodeForgeSpeed,
            TechEffectType.ForgeSpeed,
            5,
            0.1f,
            50,
            1.5,
            1);

        TechNodeData upgradeCost = CreateOrLoadNode(
            $"{TechFolder}/TechNode_UpgradeCost.asset",
            "upgrade_cost",
            LocalizationKey.TechNodeUpgradeCost,
            TechEffectType.UpgradeCostReduction,
            5,
            0.05f,
            75,
            1.6,
            3);

        TechNodeData offlineGold = CreateOrLoadNode(
            $"{TechFolder}/TechNode_OfflineGold.asset",
            "offline_gold",
            LocalizationKey.TechNodeOfflineGold,
            TechEffectType.OfflineGold,
            5,
            0.08f,
            60,
            1.5,
            2);

        TechTreeDatabase database = AssetDatabase.LoadAssetAtPath<TechTreeDatabase>(DatabaseResourcePath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<TechTreeDatabase>();
            AssetDatabase.CreateAsset(database, DatabaseResourcePath);
        }

        SerializedObject serializedDatabase = new SerializedObject(database);
        SerializedProperty nodesProperty = serializedDatabase.FindProperty("nodes");
        nodesProperty.ClearArray();

        TechNodeData[] orderedNodes = { forgeSpeed, upgradeCost, offlineGold };
        for (int i = 0; i < orderedNodes.Length; i++)
        {
            nodesProperty.InsertArrayElementAtIndex(i);
            nodesProperty.GetArrayElementAtIndex(i).objectReferenceValue = orderedNodes[i];
        }

        serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(database);

        SaveManager saveManager = Object.FindFirstObjectByType<SaveManager>();
        if (saveManager != null)
        {
            SerializedObject serializedSave = new SerializedObject(saveManager);
            serializedSave.FindProperty("techTreeDatabase").objectReferenceValue = database;
            serializedSave.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(saveManager);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[TechTree] 3 dugum + MainTechTreeDatabase olusturuldu. SaveManager baglandi (varsa). Ctrl+S ile kaydet.");
    }

    private static TechNodeData CreateOrLoadNode(string path, string nodeId, LocalizationKey nameKey,
        TechEffectType effectType, int maxLevel, float valuePerLevel, double baseCost, double costScale,
        int requiredAnvilLevel)
    {
        TechNodeData node = AssetDatabase.LoadAssetAtPath<TechNodeData>(path);
        if (node == null)
        {
            node = ScriptableObject.CreateInstance<TechNodeData>();
            AssetDatabase.CreateAsset(node, path);
        }

        SerializedObject serializedNode = new SerializedObject(node);
        serializedNode.FindProperty("nodeId").stringValue = nodeId;
        serializedNode.FindProperty("displayNameKey").enumValueIndex = (int)nameKey;
        serializedNode.FindProperty("effectType").enumValueIndex = (int)effectType;
        serializedNode.FindProperty("maxLevel").intValue = maxLevel;
        serializedNode.FindProperty("valuePerLevel").floatValue = valuePerLevel;
        serializedNode.FindProperty("baseUpgradeCost").doubleValue = baseCost;
        serializedNode.FindProperty("costScalePerLevel").doubleValue = costScale;
        serializedNode.FindProperty("requiredAnvilLevel").intValue = requiredAnvilLevel;
        serializedNode.FindProperty("baseResearchDurationSeconds").floatValue = 15f;
        serializedNode.FindProperty("durationScalePerLevel").floatValue = 1.25f;
        serializedNode.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(node);

        return node;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
