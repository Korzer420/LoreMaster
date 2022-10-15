using ItemChanger;

namespace LoreMaster.ItemChangerData.Other;

internal class ClothTest : IBool
{
    public bool Value => PlayerData.instance.GetBool(nameof(PlayerData.instance.clothKilled));

    public IBool Clone()
    {
        return new ClothTest();
    }
}
