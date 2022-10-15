namespace LoreMaster.Enums;

/// <summary>
/// Defines lore sets which apply tags to all abilities. The option file is still be able to overwrite this.
/// </summary>
public enum LoreSetOption
{
    /// <summary>
    /// All powers use their default behaviour.
    /// </summary>
    Default,

    /// <summary>
    /// All powers become globally available after obtaining.
    /// </summary>
    AllGlobalPowers,

    /// <summary>
    /// Only recommended powers are active.
    /// <para/> Recommended powers are: Tourist, Requiem, Treasure Hunter, Stag adoption and Grass Bombardement.
    /// </summary>
    OnlyRecommended,

    /// <summary>
    /// None power is active besides the tracker.
    /// </summary>
    OnlyTracker,

    /// <summary>
    /// None power is active.
    /// </summary>
    RemoveAllPowers,

    /// <summary>
    /// The player can decide how powers should behave.
    /// </summary>
    Custom
}
