using ItemChanger;

namespace LoreMaster.ItemChangerData.Other;

/// <summary>
/// Check for curse randomizer, if the treasure can be mimicked.
/// </summary>
internal class CanTreasureMimic : IBool
{
    public bool Value => Randomizer.RandomizerManager.Settings.DefineRefs || Randomizer.RandomizerManager.Settings.RandomizeTreasures;

    public IBool Clone() => new CanTreasureMimic();
}
