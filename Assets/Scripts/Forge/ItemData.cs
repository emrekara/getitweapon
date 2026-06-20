using UnityEngine;

// Silah/zirh tanimi — ScriptableObject ile Inspector'dan yeni item eklenir.
[CreateAssetMenu(fileName = "NewItem", menuName = "GetItWeapon/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName = "Stone Sword";
    [SerializeField] private Sprite icon;
    [SerializeField] private double baseAttack = 1;
    [SerializeField] private double baseDefense = 0;
    [SerializeField] private double sellPrice = 5;
    [SerializeField] private int tier = 1;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public double BaseAttack => baseAttack;
    public double BaseDefense => baseDefense;
    public double SellPrice => sellPrice;
    public int Tier => tier;
}
