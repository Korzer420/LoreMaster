using LoreMaster.Enums;
using System.Collections.Generic;

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
    /// Gets or sets the flag, that indicates if the player can purchase treasure charts from iselda.
    /// </summary>
    public bool CanBuyTreasureCharts { get; set; }

    /// <summary>
    /// Gets or sets the obtained treasure charts.
    /// </summary>
    public bool[] TreasureCharts { get; set; } = new bool[14];

    /// <summary>
    /// Gets or sets the state of the treasures.
    /// </summary>
    public Dictionary<string, TreasureState> TreasureStates = new();

    /// <summary>
    /// Gets or sets the value which indicates if stag adoption can spawn a stag.
    /// </summary>
    public bool CanSpawnStag { get; set; }
}
