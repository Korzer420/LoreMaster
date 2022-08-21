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
    /// None power is active besides the tracker.
    /// </summary>
    RemoveAllPowersExceptTracker,

    /// <summary>
    /// None power is active.
    /// </summary>
    RemoveAllPowers
}
