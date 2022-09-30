namespace LoreMaster.Enums;

/// <summary>
/// Determines how/if lore is allowed to be cursed (Cursed lore stay/becomes twisted upon obtaining)
/// </summary>
public enum CursedLore
{
    /// <summary>
    /// None of the lore is cursed.
    /// </summary>
    None,

    /// <summary>
    /// A random amount between 1 to 15 is cursed. For hard mode: 5 to 25. For heroic mode: 10 to 40.
    /// </summary>
    Random,

    /// <summary>
    /// The player can decide the amount. The min amount is still bounded to the difficulty.
    /// </summary>
    Fixed
}
