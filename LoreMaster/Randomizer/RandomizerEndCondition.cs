namespace LoreMaster.Randomizer;

/// <summary>
/// Specifies the conditions, which allows the player to enter the black egg temple.
/// </summary>
public enum RandomizerEndCondition
{
    /// <summary>
    /// Default, the player needs 3 dreamers to enter the temple.
    /// </summary>
    Dreamers,

    /// <summary>
    /// The player need certain amounts of acquired lore to enter the temple.
    /// </summary>
    Lore,

    /// <summary>
    /// The player needs all 3 dreamers and a certain amount of lore to enter the temple.
    /// </summary>
    DreamersAndLore
}
