using ItemChanger;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.Randomizer;
using System.Linq;

namespace LoreMaster.ItemChangerData.Items;

/// <summary>
/// An item which can play a sound and increases the stage of a traveller NPC.
/// </summary>
internal class TravellerItem : SoundItem
{
    /// <summary>
    /// Gets or sets the traveller, which this item should add a stage to.
    /// </summary>
    public Traveller Traveller { get; set; }

    public override void GiveImmediate(GiveInfo info)
    {
        base.GiveImmediate(info);
        PlayerData.instance.IntAdd(Traveller.ToString(), 1);
        (UIDef as LoreUIDef).name = new BoxedString($"{(UIDef as LoreUIDef).name} ({RandomizerManager.TravellerStages[Traveller]} / {RandomizerManager.TravellerOrder[Traveller].Length})");
    }

    public override AbstractItem Clone()
    {
        return new TravellerItem()
        {
            name = name,
            ClipName = ClipName,
            Traveller = Traveller,
            UIDef = UIDef.Clone(),
            tags = tags?.Select((Tag t) => t.Clone())?.ToList()
        };
    }
}
