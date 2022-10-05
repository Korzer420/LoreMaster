using ItemChanger;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.Manager;
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
        int currentState = LoreManager.Instance.Traveller[Traveller].CurrentStage > 10 
            ? LoreManager.Instance.Traveller[Traveller].CurrentStage - 10 
            : LoreManager.Instance.Traveller[Traveller].CurrentStage;
        (UIDef as LoreUIDef).name = new BoxedString($"{(UIDef as LoreUIDef).name.Value} ({currentState} / {LoreManager.Instance.Traveller[Traveller].Locations.Length})");
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
