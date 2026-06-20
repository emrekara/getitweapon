using UnityEngine;

// Merkezi item listesi; forge edilebilir tum ItemData SO'lar burada toplanir.
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "GetItWeapon/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private ItemData[] items;

    /// <summary>Forge havuzundaki tum item'lar.</summary>
    public ItemData[] Items => items;

    /// <summary>Listede kac item oldugunu dondurur.</summary>
    public int Count => items != null ? items.Length : 0;

    /// <summary>Indeksteki item'i dondurur; gecersiz indeks icin null.</summary>
    public ItemData GetItem(int index)
    {
        if (items == null || index < 0 || index >= items.Length) return null;
        return items[index];
    }

    /// <summary>Item'in listedeki indeksini bulur; bulunamazsa -1.</summary>
    public int IndexOf(ItemData item)
    {
        if (item == null || items == null) return -1;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
                return i;
        }

        return -1;
    }

    /// <summary>Belirtilen caga ait forge item'larini dondurur.</summary>
    public ItemData[] GetItemsForEra(string era)
    {
        if (items == null || string.IsNullOrEmpty(era)) return System.Array.Empty<ItemData>();

        int matchCount = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].Era == era)
                matchCount++;
        }

        if (matchCount == 0) return System.Array.Empty<ItemData>();

        ItemData[] result = new ItemData[matchCount];
        int writeIndex = 0;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].Era == era)
                result[writeIndex++] = items[i];
        }

        return result;
    }

    /// <summary>Caga uygun rastgele item secer; havuz bos ise null.</summary>
    public ItemData GetRandomItemForEra(string era)
    {
        ItemData[] pool = GetItemsForEra(era);
        if (pool.Length == 0) return null;
        return pool[Random.Range(0, pool.Length)];
    }
}
