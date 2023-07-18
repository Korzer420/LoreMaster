using LoreMaster.Enums;
using System.Collections.Generic;

namespace LoreMaster.SaveManagement;

/// <summary>
/// Class for local save data
/// </summary>
public class LoreMasterLocalSaveData
{
    /// <summary>
    /// Gets or sets the tags and twisted value of the powers.
    /// </summary>
    public Dictionary<string, (PowerRank, bool)> Tags { get; set; } = new();

    /// <summary>
    /// Get or set the acquired powers key.
    /// </summary>
    public List<string> ObtainedPowerKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets power specific data.
    /// </summary>
    public LocalPowerSaveData PowerData { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the player can read lore tablets. (Rando only)
    /// </summary>
    public bool HasReadAbility { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the player can read lore tablets. (Rando only)
    /// </summary>
    public bool HasListenAbility { get; set; }

    /// <summary>
    /// Gets or sets the current game mode (for this mod, not steelsoul/god master)
    /// </summary>
    public GameMode GameMode { get; set; }

    /// <summary>
    /// Gets or sets the condition for opening the black egg temple
    /// </summary>
    public BlackEggTempleCondition EndCondition { get; set; }

    /// <summary>
    /// Gets or sets the needed lore for the black egg temple door.
    /// </summary>
    public int NeededLore { get; set; }

    /// <summary>
    /// Gets or sets the current state of elderbug.
    /// </summary>
    public int ElderbugState { get; set; }

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
