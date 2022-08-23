namespace LoreMaster.Enums;

/// <summary>
/// The state in which the power currently is.
/// </summary>
public enum PowerState
{
    /// <summary>
    /// The power was obtained but is not active.
    /// </summary>
    Disabled,

    /// <summary>
    /// The power was obtained and is active.
    /// </summary>
    Active,

    /// <summary>
    /// The power hasn't been obtained but has their twisted effect active. Hard/Heroic mode only.
    /// </summary>
    Twisted
}
