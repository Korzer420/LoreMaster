namespace LoreMaster.Enums;

/// <summary>
/// The state in which the power currently is.
/// </summary>
public enum PowerState
{
    /// <summary>
    /// The power was obtained but is not enabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// The power is active.
    /// </summary>
    Active,

    /// <summary>
    /// The power is in their twisted state. Hard/Heroic mode only.
    /// </summary>
    Twisted
}
