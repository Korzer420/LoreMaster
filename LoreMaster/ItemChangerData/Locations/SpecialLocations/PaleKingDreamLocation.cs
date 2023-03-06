using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Helper;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

/// <summary>
/// The dream dialogue location of the pale king.
/// </summary>
internal class PaleKingDreamLocation : DreamNailLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new("White King Corpse", "Control"), BlockPaleKingHits);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new("White King Corpse", "Control"), BlockPaleKingHits);
    }

    /// <summary>
    /// Prevents the player from "killing" the pale king until the player achieved the dream dialogue items.
    /// </summary>
    private void BlockPaleKingHits(PlayMakerFSM fsm)
    {
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Check Dream",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => fsm.SendEvent(Placement.Items.All(x => x.IsObtained()) ? "HIT" : "FINISHED"))
            }
        });
        fsm.GetState("Idle").AdjustTransition("HIT", "Check Dream");
        fsm.GetState("Check Dream").AddTransition("FINISHED", "Idle");
        fsm.GetState("Check Dream").AddTransition("HIT", "Hits");
    }
}
