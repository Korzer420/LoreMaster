using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using KorzUtils.Helper;
using LoreMaster.Helper;

namespace LoreMaster.ItemChangerData;

internal class PathOfPainLocation : AutoLocation
{
    protected override void OnLoad() => Events.AddFsmEdit(sceneName, new("End Scene", "Conversation Control"), ModifyPathOfPainScene);
    
    protected override void OnUnload() => Events.RemoveFsmEdit(sceneName, new("End Scene", "Conversation Control"), ModifyPathOfPainScene);

    private void ModifyPathOfPainScene(PlayMakerFSM fsm)
    {
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Grant Items",
            Actions = new FsmStateAction[]
            {
                new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
                {
                    FlingType = flingType,
                    Container = Container.Tablet,
                    MessageType = MessageType.Any,
                }, callback), "CONVO FINISHED")
            }
        });
        fsm.GetState("Grant Items").AddTransition("CONVO FINISHED", "New Scene");
        fsm.GetState("Journal").AdjustTransition("FINISHED", "Grant Items");
    }
}
