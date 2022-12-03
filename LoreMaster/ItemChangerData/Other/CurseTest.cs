using ItemChanger;
using LoreMaster.Randomizer;

namespace LoreMaster.ItemChangerData.Other;

internal class CurseTest : IBool
{
    public bool IsReading { get; set; }

    public bool Value => IsReading ? RandomizerManager.Settings.CursedReading : RandomizerManager.Settings.CursedListening;

    public IBool Clone() => new CurseTest() { IsReading = IsReading };
}
