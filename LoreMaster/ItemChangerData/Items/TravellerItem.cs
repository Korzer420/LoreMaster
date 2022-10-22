using ItemChanger;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData.UIDefs;
using LoreMaster.Manager;

namespace LoreMaster.ItemChangerData.Items;

/// <summary>
/// An item which can play a sound and increases the stage of a traveller NPC.
/// </summary>
internal class TravellerItem : PowerLoreItem
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
        int maxStage = LoreManager.Instance.Traveller[Traveller].Locations.Length + (Traveller == Traveller.Cloth ? -1 : 0);
        if (UIDef is LoreUIDef lore)
            lore.name = new BoxedString($"{lore.name.Value} ({currentState} / {maxStage})");
        else
            (UIDef as DreamLoreUIDef).name = new BoxedString($"{(UIDef as DreamLoreUIDef).name.Value} ({currentState} / {maxStage})");

        // Allow cloth to enter the traitor lord fight (not sure if this is necessary, but just in case)
        if (Traveller == Traveller.Cloth && LoreManager.Instance.Traveller[Traveller.Cloth].CurrentStage >= 3)
            PlayerData.instance.SetBool(nameof(PlayerData.instance.savedCloth), true);
    }
}
