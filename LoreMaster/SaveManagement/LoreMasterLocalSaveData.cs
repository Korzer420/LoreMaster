using LoreMaster.Enums;
using System.Collections.Generic;

namespace LoreMaster.SaveManagement;

/// <summary>
/// Class for local save data
/// </summary>
public class LoreMasterLocalSaveData
{
    /// <summary>
    /// Gets or sets power specific data.
    /// </summary>
    public LocalPowerSaveData PowerData { get; set; }

    /// <summary>
    /// Gets or sets the current game mode (for this mod, not steelsoul/god master)
    /// </summary>
    public GameMode GameMode { get; set; }

    /// <summary>
    /// Gets or sets the amount of joker scrolls the player has.
    /// </summary>
    public int JokerScrolls { get; set; }

    /// <summary>
    /// Gets or sets the amount cleansing scrolls the player has.
    /// </summary>
    public int CleansingScrolls { get; set; }

    /// <summary>
    /// Gets or sets the state of the inventory power page.
    /// </summary>
    public PowerControlState PageState { get; set; }
}
