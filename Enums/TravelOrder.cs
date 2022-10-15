namespace LoreMaster.Enums;

/// <summary>
/// Determines, how travel npc (Cloth, Zote, Tiso and Quirrel) should appear.
/// </summary>
public enum TravelOrder
{
    /// <summary>
    /// Appear in the order that would be applied in the normal game. Note that the conditions when enemies disappear don't apply.
    /// </summary>
    Vanilla,

    /// <summary>
    /// Appear in a randomized order. The last location remains the same.
    /// </summary>
    Shuffled,

    /// <summary>
    /// The npc are always present at any location where the would appear. At least as they have items left.
    /// </summary>
    Everywhere
}
