using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Manager;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

internal class ZoteDeepnestLocation : DialogueLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new("Zote Deepnest", "Cut Down"), CheckIfZoteSpawn);
    }
    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new("Zote Deepnest", "Cut Down"), CheckIfZoteSpawn);
    }

    private void CheckIfZoteSpawn(PlayMakerFSM fsm)
    {
        fsm.AddState(new HutongGames.PlayMaker.FsmState(fsm.Fsm)
        {
            Name = "Spawn Control",
            Actions =
            [
                new Lambda(() => 
                {
                    if(LoreManager.Instance.Traveller[Enums.Traveller.Zote].CurrentStage >= LoreManager.Instance.Traveller[Enums.Traveller.Zote].Locations.IndexOf("/Zote Deepnest/Faller/NPC")
                    && !Placement.Items.All(x => x.IsObtained()))
                        fsm.SendEvent("FINISHED");
                    else
                        fsm.SendEvent("DESTROY");
                })
            ]
        });
        KorzUtils.Helper.FsmHelper.AdjustTransition(fsm.GetState("Pause"), "FINISHED", "Spawn Control");
        fsm.GetState("Spawn Control").AddTransition("DESTROY", "Destroy");
        fsm.GetState("Spawn Control").AddTransition("FINISHED", "Init");
    }
}
