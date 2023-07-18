namespace LoreMaster.SaveManagement;

/// <summary>
/// Contains save data for specific powers.
/// </summary>
public class LocalPowerSaveData
{
    /// <summary>
    /// Gets or sets the current cost of glory of the wealth.
    /// </summary>
    public int GloryCost { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if stag adoption can spawn a stag.
    /// </summary>
    public bool CanSpawnStag { get; set; }
}
