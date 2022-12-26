namespace LoreMaster.Enums;

/// <summary>
/// Determines the game mode that is currently playing.
/// </summary>
public enum GameMode
{
    /// <summary>
    /// The normal mode. Obtaining powers will grant you their effect.
    /// </summary>
    Normal,

    /// <summary>
    /// "Normal" difficulty in the extra menu page. Just adds a few flavour things.
    /// </summary>
    Extra,

    /// <summary>
    /// The hard mode. Until you obtain a power you get a debuff if you are in the area where the ability would be. After obtaining the power grant you their effect.
    /// </summary>
    Hard,

    /// <summary>
    /// The heroic mode. Until you obtain a power you get a debuff if you are in the area where the ability would be. Obtaining a power DOESN'T give you their positive effect.
    /// </summary>
    Heroic,

    /// <summary>
    /// Modifies nothing.
    /// </summary>
    Disabled,
}
