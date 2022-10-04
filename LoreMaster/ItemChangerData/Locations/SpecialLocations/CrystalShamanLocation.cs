using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Helper;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

/// <summary>
/// The dream dialogue location of the crystalized shaman
/// </summary>
internal class CrystalShamanLocation : DreamNailLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new("Crystal Shaman", "Control"), BlockHits);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new("Crystal Shaman", "Control"), BlockHits);
    }

    private void BlockHits(PlayMakerFSM fsm)
    {
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Check Dream",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => fsm.SendEvent(Placement.Items.All(x => x.IsObtained()) ? "NAIL HIT" : "FINISHED"))
            }
        });
        fsm.GetState("Idle").AdjustTransition("NAIL HIT", "Check Dream");
        fsm.GetState("Check Dream").AddTransition("FINISHED", "Idle");
        fsm.GetState("Check Dream").AddTransition("NAIL HIT", "Hit Recover");
    }
}
