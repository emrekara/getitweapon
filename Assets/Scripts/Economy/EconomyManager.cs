using UnityEngine;

// Oyuncunun altın (gold) miktarını tutar ve değiştirir.
public class EconomyManager : MonoBehaviour
{
    [SerializeField] private double startingGold = 0;

    private double currentGold;

    /// <summary>O anki altın miktarı.</summary>
    public double CurrentGold => currentGold;

    private void Awake()
    {
        currentGold = startingGold;
    }

    /// <summary>Altın ekler (satış, ödül vb.).</summary>
    public void AddGold(double amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
    }

    /// <summary>Altın harcar; yeterli altın yoksa false döner.</summary>
    public bool TrySpendGold(double amount)
    {
        if (amount <= 0 || currentGold < amount) return false;
        currentGold -= amount;
        return true;
    }
}
