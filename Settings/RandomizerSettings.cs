using LoreMaster.Enums;
using MenuChanger.Attributes;
using System.Collections.Generic;

namespace LoreMaster.Settings;

/// <summary>
/// Contains the settings for a randomizer.
/// </summary>
public class RandomizerSettings
{
    #region Properties

    /// <summary>
    /// Gets or sets the value which indicates, if the locations/items should be placed even if not randomized.
    /// </summary>
    public bool DefineRefs { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the text of lore npc should be randomized. (Uses the same ones as the mod itself)
    /// </summary>
    public bool RandomizeNpc { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the statues and corpses of the dream warrior should be randomized.
    /// </summary>
    public bool RandomizeDreamWarriorStatues { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the statues/corpses of the dream warrior should be randomized.
    /// </summary>
    public bool RandomizePointsOfInterest { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if dream nail dialogue should be randomized (not counting Tisos corpse)
    /// </summary>
    public bool RandomizeDreamDialogue { get; set; }

    /// <summary>
    /// Gets or sets the value that indicates, if the rewards of Elderbug's quest should be randomized. (Reading and Listening locations not included.)
    /// </summary>
    public bool RandomizeElderbugRewards { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if Quirrel, Cloth, Tiso and Zote dialogues should be randomized.
    /// </summary>
    public bool RandomizeTravellers { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates, in which order traveller should appear.
    /// </summary>
    public TravelOrder TravellerOrder { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the buried treasures should be randomized.
    /// </summary>
    public bool RandomizeTreasures { get; set; }

    /// <summary>
    /// Gets or sets the flag which indicates, if the compass has to be obtained first, for treasures to be in logic.
    /// </summary>
    public bool ForceCompassForTreasure { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the ability to read lore tablets, should be randomized.
    /// </summary>
    public bool CursedReading { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the ability to talk to npc's, should be randomized.
    /// </summary>
    public bool CursedListening { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, what condition the black egg temple has.
    /// </summary>
    public BlackEggTempleCondition BlackEggTempleCondition { get; set; }

    /// <summary>
    /// Gets or set the amount of lore needed if <see cref="BlackEggTempleCondition"/> is not <see cref="BlackEggTempleCondition.Dreamers"/> for opening black egg temple.
    /// </summary>
    public int NeededLore { get; set; } = 10;

    /// <summary>
    /// Gets or sets, how powers should behave.
    /// </summary>
    public LoreSetOption PowerBehaviour { get; set; }

    #endregion
}
