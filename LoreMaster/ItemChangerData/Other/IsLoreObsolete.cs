using ItemChanger;

namespace LoreMaster.ItemChangerData.Other;

/// <summary>
/// Check for curse randomizer, if lore can be replaced.
/// </summary>
internal class IsLoreObsolete : IBool
{
    public bool Value => Randomizer.RandomizerManager.Settings.BlackEggTempleCondition != Enums.BlackEggTempleCondition.Dreamers;

    public IBool Clone() => new IsLoreObsolete();
}
