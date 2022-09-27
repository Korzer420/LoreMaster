namespace LoreMaster.Enums;

/// <summary>
/// The state in which the power currently is.
/// </summary>
public enum PowerState
{
    /// <summary>
    /// The power was not obtained or is not enabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// The power was obtained and is active.
    /// </summary>
    Active,

    /// <summary>
    /// The power is in their twisted state. Hard/Heroic mode only.
    /// </summary>
    Twisted
}
