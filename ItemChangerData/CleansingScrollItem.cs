using ItemChanger.Items;
using LoreMaster.Manager;
using System.Linq;

namespace LoreMaster.ItemChangerData;

internal class CleansingScrollItem : IntItem
{
    public override bool Redundant() => PowerManager.GetAllPowers().All(x => !x.StayTwisted);
}
