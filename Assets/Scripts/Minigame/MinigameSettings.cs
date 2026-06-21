using UnityEngine;

/// <summary>Yerel mini oyun ayarlari.</summary>
[CreateAssetMenu(fileName = "MinigameSettings", menuName = "GetItWeapon/Minigame Settings")]
public class MinigameSettings : ScriptableObject
{
    [SerializeField] private MinigameSettingsData values = new MinigameSettingsData();

    public MinigameSettingsData Values => values;

    public void ApplyData(MinigameSettingsData remote)
    {
        if (remote == null) return;
        values = remote.Clone();
        values.Sanitize();
    }
}
