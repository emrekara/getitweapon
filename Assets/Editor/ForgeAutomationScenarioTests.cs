#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Forge karar matrisini Editor'da dogrular; GetItWeapon/Debug menusunden calistirilir.</summary>
public static class ForgeAutomationScenarioTests
{
    [MenuItem("GetItWeapon/Debug/Forge Senaryolarini Test Et")]
    public static void RunAllScenarios()
    {
        int passed = 0;
        int failed = 0;

        Run("OTO SAT KAPALI + bos envanter → envantere ekle", () =>
        {
            ItemData forged = CreateItem(attack: 10);
            ForgeProcessContext ctx = Context(autoSell: false, containsCategory: false, usedSlots: 0);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.AddToInventory);
        }, ref passed, ref failed);

        Run("OTO SAT KAPALI + ayni kategoride zayif item → reddet (satis yok, OTO DÖV durur)", () =>
        {
            ItemData forged = CreateItem(attack: 5);
            ItemData existing = CreateItem(attack: 20);
            ForgeProcessContext ctx = Context(autoSell: false, containsCategory: true, categoryItem: existing);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.RejectDuplicate);
        }, ref passed, ref failed);

        Run("OTO SAT KAPALI + ayni kategoride esit item → reddet", () =>
        {
            ItemData forged = CreateItem(attack: 15, tier: 2);
            ItemData existing = CreateItem(attack: 15, tier: 2);
            ForgeProcessContext ctx = Context(autoSell: false, containsCategory: true, categoryItem: existing);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.RejectDuplicate);
        }, ref passed, ref failed);

        Run("OTO SAT ACIK + ayni kategoride zayif item → OTO SAT", () =>
        {
            ItemData forged = CreateItem(attack: 5);
            ItemData existing = CreateItem(attack: 20);
            ForgeProcessContext ctx = Context(autoSell: true, containsCategory: true, categoryItem: existing);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.AutoSell);
        }, ref passed, ref failed);

        Run("OTO SAT KAPALI + ayni kategoride daha iyi item → popup", () =>
        {
            ItemData forged = CreateItem(attack: 30);
            ItemData existing = CreateItem(attack: 10);
            ForgeProcessContext ctx = Context(autoSell: false, containsCategory: true, categoryItem: existing);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.PromptUser);
        }, ref passed, ref failed);

        Run("OTO SAT KAPALI + envanter dolu + yeni kategori yok → islem yok", () =>
        {
            ItemData forged = CreateItem(attack: 10);
            ForgeProcessContext ctx = Context(autoSell: false, inventoryFull: true, usedSlots: 8);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.None);
        }, ref passed, ref failed);

        Run("OTO SAT ACIK + bos envanter → envantere ekle", () =>
        {
            ItemData forged = CreateItem(attack: 10);
            ForgeProcessContext ctx = Context(autoSell: true, usedSlots: 0);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.AddToInventory);
        }, ref passed, ref failed);

        Run("OTO SAT ACIK + zayif item (farkli kategori) → OTO SAT", () =>
        {
            ItemData forged = CreateItem(attack: 5);
            ItemData best = CreateItem(attack: 25);
            ForgeProcessContext ctx = Context(autoSell: true, usedSlots: 2, bestReference: best);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.AutoSell);
        }, ref passed, ref failed);

        Run("OTO SAT ACIK + daha iyi item → popup", () =>
        {
            ItemData forged = CreateItem(attack: 40);
            ItemData best = CreateItem(attack: 10);
            ForgeProcessContext ctx = Context(autoSell: true, usedSlots: 1, bestReference: best);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.PromptUser);
        }, ref passed, ref failed);

        Run("OTO SAT ACIK + filtre disi item → envantere ekle", () =>
        {
            ItemData forged = CreateItem(attack: 5, tier: 3);
            ItemData best = CreateItem(attack: 25);
            ForgeProcessContext ctx = Context(autoSell: true, passesFilter: false, usedSlots: 1, bestReference: best);
            return Expect(ForgeItemActionResolver.Decide(forged, ctx), ForgeItemAction.AddToInventory);
        }, ref passed, ref failed);

        Run("Geri bildirim: manuel sat OTO SAT yazmaz", () =>
        {
            string msg = GameTexts.ForgedItemSoldFeedback(100);
            return msg.Contains("OTO SAT") ? "Mesaj OTO SAT icermemeli: " + msg : null;
        }, ref passed, ref failed);

        Run("Geri bildirim: OTO SAT acikken OTO SAT yazar", () =>
        {
            string msg = GameTexts.AutoSoldFeedback(100);
            return msg.Contains("OTO SAT") ? null : "Mesaj OTO SAT icermeli: " + msg;
        }, ref passed, ref failed);

        if (failed == 0)
            Debug.Log($"[ForgeTest] Tum senaryolar gecti ({passed}/{passed + failed}).");
        else
            Debug.LogError($"[ForgeTest] {failed} senaryo basarisiz, {passed} gecti.");
    }

    private static void Run(string name, System.Func<string> test, ref int passed, ref int failed)
    {
        string error = test();
        if (string.IsNullOrEmpty(error))
        {
            passed++;
            Debug.Log($"[ForgeTest] GECTI: {name}");
        }
        else
        {
            failed++;
            Debug.LogError($"[ForgeTest] BASARISIZ: {name} — {error}");
        }
    }

    private static string Expect(ForgeItemAction actual, ForgeItemAction expected)
    {
        return actual == expected ? null : $"beklenen {expected}, gelen {actual}";
    }

    private static ForgeProcessContext Context(
        bool autoSell,
        bool containsCategory = false,
        ItemData categoryItem = null,
        bool inventoryFull = false,
        int usedSlots = 0,
        ItemData bestReference = null,
        bool passesFilter = true)
    {
        return new ForgeProcessContext
        {
            AutoSellEnabled = autoSell,
            PassesAutoSellFilter = passesFilter,
            ContainsCategory = containsCategory,
            CategoryItem = categoryItem,
            InventoryFull = inventoryFull,
            UsedSlotCount = usedSlots,
            BestReference = bestReference
        };
    }

    private static ItemData CreateItem(double attack, double defense = 0, int tier = 1, double sellPrice = 100)
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        SerializedObject serialized = new SerializedObject(item);
        serialized.FindProperty("baseAttack").doubleValue = attack;
        serialized.FindProperty("baseDefense").doubleValue = defense;
        serialized.FindProperty("tier").intValue = tier;
        serialized.FindProperty("sellPrice").doubleValue = sellPrice;
        serialized.FindProperty("category").enumValueIndex = (int)ItemCategory.Weapon;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return item;
    }
}
#endif
