using System;

/// <summary>Mini oyun ayarlari; API remote config ile ayni sema.</summary>
[Serializable]
public class MinigameSettingsData
{
    public int minPointsPerPlay = 1;
    public int maxPointsPerPlay = 10;

    public MinigameSettingsData Clone()
    {
        return new MinigameSettingsData
        {
            minPointsPerPlay = minPointsPerPlay,
            maxPointsPerPlay = maxPointsPerPlay
        };
    }

    public void Sanitize()
    {
        minPointsPerPlay = Math.Max(1, minPointsPerPlay);
        maxPointsPerPlay = Math.Max(minPointsPerPlay, maxPointsPerPlay);
    }
}
