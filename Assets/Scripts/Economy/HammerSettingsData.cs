using System;

/// <summary>
/// Cekic ekonomisi ayarlari; JSON/API remote config ile ayni semada tasinir.
/// </summary>
[Serializable]
public class HammerSettingsData
{
    public int startingHammers = 5;
    public int maxHammers = 10;
    public int forgeHammerCost = 1;
    public float regenIntervalSeconds = 45f;
    public bool dailyFullRefillEnabled = true;

    /// <summary>Verilen degerleri kopyalar.</summary>
    public HammerSettingsData Clone()
    {
        return new HammerSettingsData
        {
            startingHammers = startingHammers,
            maxHammers = maxHammers,
            forgeHammerCost = forgeHammerCost,
            regenIntervalSeconds = regenIntervalSeconds,
            dailyFullRefillEnabled = dailyFullRefillEnabled
        };
    }

    /// <summary>Gecersiz degerleri guvenli araliklara ceker.</summary>
    public void Sanitize()
    {
        maxHammers = Math.Max(1, maxHammers);
        startingHammers = Math.Max(0, Math.Min(startingHammers, maxHammers));
        forgeHammerCost = Math.Max(1, forgeHammerCost);
        regenIntervalSeconds = Math.Max(1f, regenIntervalSeconds);
    }
}
