using UnityEngine;

/// <summary>
/// Yerel cekic ayarlari; oyun basinda yuklenir, API gelene kadar varsayilan kaynak.
/// </summary>
[CreateAssetMenu(fileName = "HammerSettings", menuName = "GetItWeapon/Hammer Settings")]
public class HammerSettings : ScriptableObject
{
    [SerializeField] private HammerSettingsData values = new HammerSettingsData();

    /// <summary>Aktif ayar degerleri.</summary>
    public HammerSettingsData Values => values;

    /// <summary>Remote config veya API cevabini bu asset uzerine yazar (editor/test).</summary>
    public void ApplyData(HammerSettingsData remote)
    {
        if (remote == null) return;

        values = remote.Clone();
        values.Sanitize();
    }
}
