namespace LoreMaster.Enums;

/// <summary>
/// Determines how the player can interact with the inventory page.
/// </summary>
public enum PowerControlState
{
    /// <summary>
    /// The player can't enter the inventory page.
    /// </summary>
    NotObtained,

    /// <summary>
    /// The player can access the inventory page but can't toggle powers.
    /// </summary>
    ReadAccess,

    /// <summary>
    /// The player can access the inventory page and toggle powers on and off.
    /// </summary>
    ToggleAccess
}
