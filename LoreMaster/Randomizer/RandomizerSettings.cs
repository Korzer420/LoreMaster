using System;

namespace LoreMaster.Randomizer;

/// <summary>
/// Contains the settings for a randomizer.
/// </summary>
public class RandomizerSettings
{
    #region Properties

    /// <summary>
    /// Gets or sets the value which indicates, if the text of lore npc should be randomized. (Uses the same ones as the mod itself)
    /// </summary>
    public bool RandomizeNpc { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the ability to read lore tablets, should be randomized.
    /// </summary>
    public bool CursedReading { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the ability to talk to npc's, should be randomized.
    /// </summary>
    public bool CursedTalking { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, what condition the black egg temple has.
    /// </summary>
    public RandomizerEndCondition TempleCondition { get; set; }

    /// <summary>
    /// Gets or set the amount of lore needed if <see cref="TempleCondition"/> is not <see cref="RandomizerEndCondition.Dreamers"/> for opening black egg temple.
    /// </summary>
    public int NeededLore { get; set; } = 10;

    #endregion
}
