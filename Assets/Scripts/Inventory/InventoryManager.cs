using System;
using System.Collections.Generic;
using UnityEngine;

// Envanter slotlarini tutar; forge ekleme, secim ve satis islemlerini yonetir.
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private int slotCount = UITheme.SlotCount;

    private int[] itemIndices;
    private int selectedSlot = -1;

    /// <summary>Envanter icerigi veya secim degistiginde tetiklenir.</summary>
    public event Action OnInventoryChanged;

    /// <summary>Secili slot degistiginde tetiklenir.</summary>
    public event Action<int> OnSlotSelected;

    /// <summary>Toplam slot sayisi.</summary>
    public int SlotCount => slotCount;

    /// <summary>Secili slot indeksi; secim yoksa -1.</summary>
    public int SelectedSlot => selectedSlot;

    /// <summary>Bos slot var mi.</summary>
    public bool HasFreeSlot => GetFirstFreeSlotIndex() >= 0;

    /// <summary>Envanter tamamen dolu mu.</summary>
    public bool IsFull => !HasFreeSlot;

    /// <summary>Dolu slot sayisi.</summary>
    public int UsedSlotCount
    {
        get
        {
            EnsureInitialized();
            int count = 0;
            for (int i = 0; i < itemIndices.Length; i++)
            {
                if (itemIndices[i] >= 0)
                    count++;
            }

            return count;
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    /// <summary>ItemDatabase ve slot sayisini disaridan baglar.</summary>
    public void Configure(ItemDatabase database, int slots = UITheme.SlotCount)
    {
        if (database != null)
            itemDatabase = database;

        slotCount = Mathf.Max(1, slots);
        EnsureInitialized();
    }

    private void Start()
    {
        if (itemDatabase == null)
        {
            ForgeButtonHandler forgeHandler = FindFirstObjectByType<ForgeButtonHandler>();
            if (forgeHandler != null)
                itemDatabase = forgeHandler.ItemDatabase;
        }
    }

    /// <summary>Slot dolu mu kontrol eder.</summary>
    public bool IsSlotOccupied(int slotIndex)
    {
        EnsureInitialized();
        if (!IsValidSlot(slotIndex)) return false;
        return itemIndices[slotIndex] >= 0;
    }

    /// <summary>Slottaki item'i dondurur; bos slot icin null.</summary>
    public ItemData GetItemInSlot(int slotIndex)
    {
        EnsureInitialized();
        if (!IsValidSlot(slotIndex) || itemIndices[slotIndex] < 0 || itemDatabase == null)
            return null;

        return itemDatabase.GetItem(itemIndices[slotIndex]);
    }

    /// <summary>Secili slottaki item'i dondurur.</summary>
    public ItemData GetSelectedItem()
    {
        return GetItemInSlot(selectedSlot);
    }

    /// <summary>Envanterde ayni item tipi (SO) var mi.</summary>
    public bool ContainsItem(ItemData item)
    {
        return GetSlotIndexOfItem(item) >= 0;
    }

    /// <summary>Item tipinin bulundugu slot indeksi; yoksa -1.</summary>
    public int GetSlotIndexOfItem(ItemData item)
    {
        EnsureInitialized();
        if (item == null || itemDatabase == null) return -1;

        int itemIndex = itemDatabase.IndexOf(item);
        if (itemIndex < 0) return -1;

        for (int i = 0; i < itemIndices.Length; i++)
        {
            if (itemIndices[i] == itemIndex)
                return i;
        }

        return -1;
    }

    /// <summary>Item'i ilk bos slota ekler ve o slotu secer.</summary>
    public bool TryAddItem(ItemData item)
    {
        if (item == null || itemDatabase == null) return false;

        int itemIndex = itemDatabase.IndexOf(item);
        if (itemIndex < 0) return false;

        if (ContainsItemIndex(itemIndex)) return false;

        int freeSlot = GetFirstFreeSlotIndex();
        if (freeSlot < 0) return false;

        itemIndices[freeSlot] = itemIndex;
        SelectSlot(freeSlot);
        NotifyInventoryChanged();
        return true;
    }

    /// <summary>Secili slottaki item'i kaldirir.</summary>
    public bool TryRemoveSelected(out ItemData removedItem)
    {
        return TryRemoveFromSlot(selectedSlot, out removedItem);
    }

    /// <summary>Belirtilen slottaki item'i kaldirir.</summary>
    public bool TryRemoveFromSlot(int slotIndex, out ItemData removedItem)
    {
        removedItem = null;
        EnsureInitialized();

        if (!IsSlotOccupied(slotIndex)) return false;

        removedItem = GetItemInSlot(slotIndex);
        itemIndices[slotIndex] = -1;

        if (selectedSlot == slotIndex)
        {
            if (!SelectNextOccupiedSlot(slotIndex))
                selectedSlot = -1;
        }

        NotifyInventoryChanged();
        return true;
    }

    /// <summary>Envanterdeki en guclu item'i karsilastirma referansi olarak dondurur.</summary>
    public ItemData GetBestReferenceItem()
    {
        EnsureInitialized();

        ItemData best = null;
        for (int i = 0; i < itemIndices.Length; i++)
        {
            ItemData item = GetItemInSlot(i);
            if (item == null) continue;

            if (best == null || IsBetterReferenceItem(item, best))
                best = item;
        }

        return best;
    }

    /// <summary>Yeni item icin yer acmak uzere satilabilecek en zayif slot indeksini bulur.</summary>
    public int GetSlotIndexToReplaceFor(ItemData newItem)
    {
        EnsureInitialized();
        if (newItem == null) return -1;

        int candidateSlot = -1;
        ItemData candidateItem = null;

        for (int i = 0; i < itemIndices.Length; i++)
        {
            ItemData slotItem = GetItemInSlot(i);
            if (slotItem == null) continue;

            bool canReplace = ItemComparer.IsStrictlyWorseOrEqual(slotItem, newItem);
            if (!canReplace) continue;

            if (candidateItem == null || IsWeakerReferenceItem(slotItem, candidateItem))
            {
                candidateItem = slotItem;
                candidateSlot = i;
            }
        }

        return candidateSlot;
    }

    /// <summary>Slot secimini gunceller.</summary>
    public void SelectSlot(int slotIndex)
    {
        EnsureInitialized();
        if (!IsValidSlot(slotIndex)) return;
        if (!IsSlotOccupied(slotIndex)) return;
        if (selectedSlot == slotIndex) return;

        selectedSlot = slotIndex;
        OnSlotSelected?.Invoke(selectedSlot);
        NotifyInventoryChanged();
    }

    /// <summary>Kayit icin slot indekslerini dondurur (-1 = bos).</summary>
    public int[] ExportItemIndices()
    {
        EnsureInitialized();
        int[] copy = new int[itemIndices.Length];
        Array.Copy(itemIndices, copy, itemIndices.Length);
        return copy;
    }

    /// <summary>Kayit icin secili slot indeksini dondurur.</summary>
    public int ExportSelectedSlot() => selectedSlot;

    /// <summary>Kayittan envanter durumunu yukler; eski tek-slot kaydini da tasir.</summary>
    public void ImportState(int[] savedIndices, int savedSelectedSlot, int legacyLastItemIndex)
    {
        EnsureInitialized();
        ClearAllSlots();

        if (savedIndices != null && savedIndices.Length > 0)
        {
            int length = Mathf.Min(savedIndices.Length, itemIndices.Length);
            for (int i = 0; i < length; i++)
                itemIndices[i] = savedIndices[i];
        }
        else if (legacyLastItemIndex >= 0 && itemDatabase != null &&
                 legacyLastItemIndex < itemDatabase.Count)
        {
            itemIndices[0] = legacyLastItemIndex;
        }

        RemoveDuplicateItemTypes();

        if (IsValidSlot(savedSelectedSlot) && IsSlotOccupied(savedSelectedSlot))
            selectedSlot = savedSelectedSlot;
        else
            SelectFirstOccupiedSlot();

        NotifyInventoryChanged();
    }

    /// <summary>Tum slotlari temizler.</summary>
    public void ClearAllSlots()
    {
        EnsureInitialized();
        for (int i = 0; i < itemIndices.Length; i++)
            itemIndices[i] = -1;

        selectedSlot = -1;
        NotifyInventoryChanged();
    }

    private void EnsureInitialized()
    {
        slotCount = Mathf.Max(1, slotCount);
        if (itemIndices != null && itemIndices.Length == slotCount) return;

        itemIndices = new int[slotCount];
        for (int i = 0; i < itemIndices.Length; i++)
            itemIndices[i] = -1;
    }

    private int GetFirstFreeSlotIndex()
    {
        EnsureInitialized();
        for (int i = 0; i < itemIndices.Length; i++)
        {
            if (itemIndices[i] < 0)
                return i;
        }

        return -1;
    }

    private bool SelectNextOccupiedSlot(int startIndex)
    {
        EnsureInitialized();
        for (int i = startIndex; i < itemIndices.Length; i++)
        {
            if (itemIndices[i] >= 0)
            {
                selectedSlot = i;
                return true;
            }
        }

        for (int i = 0; i < startIndex; i++)
        {
            if (itemIndices[i] >= 0)
            {
                selectedSlot = i;
                return true;
            }
        }

        return false;
    }

    private void SelectFirstOccupiedSlot()
    {
        selectedSlot = -1;
        SelectNextOccupiedSlot(0);
    }

    private bool IsValidSlot(int slotIndex)
    {
        EnsureInitialized();
        return slotIndex >= 0 && slotIndex < itemIndices.Length;
    }

    private static bool IsBetterReferenceItem(ItemData candidate, ItemData current)
    {
        if (candidate.BaseAttack != current.BaseAttack)
            return candidate.BaseAttack > current.BaseAttack;

        if (candidate.BaseDefense != current.BaseDefense)
            return candidate.BaseDefense > current.BaseDefense;

        if (candidate.Tier != current.Tier)
            return candidate.Tier > current.Tier;

        return candidate.SellPrice > current.SellPrice;
    }

    private static bool IsWeakerReferenceItem(ItemData candidate, ItemData current)
    {
        if (candidate.BaseAttack != current.BaseAttack)
            return candidate.BaseAttack < current.BaseAttack;

        if (candidate.BaseDefense != current.BaseDefense)
            return candidate.BaseDefense < current.BaseDefense;

        if (candidate.Tier != current.Tier)
            return candidate.Tier < current.Tier;

        return candidate.SellPrice < current.SellPrice;
    }

    private bool ContainsItemIndex(int itemIndex)
    {
        EnsureInitialized();
        if (itemIndex < 0) return false;

        for (int i = 0; i < itemIndices.Length; i++)
        {
            if (itemIndices[i] == itemIndex)
                return true;
        }

        return false;
    }

    private void RemoveDuplicateItemTypes()
    {
        EnsureInitialized();
        HashSet<int> seen = new HashSet<int>();

        for (int i = 0; i < itemIndices.Length; i++)
        {
            int index = itemIndices[i];
            if (index < 0) continue;

            if (!seen.Add(index))
                itemIndices[i] = -1;
        }
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}
