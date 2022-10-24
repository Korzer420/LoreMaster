using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using LoreMaster.Helper;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LoreMaster.ItemChangerData.Locations;

/// <summary>
/// A location which needs to be dream nailed to give the items.
/// </summary>
internal class DreamNailLocation : AutoLocation
{
    public string GameObjectName { get; set; }

    protected override void OnLoad()
    {
        Events.AddFsmEdit(sceneName, new(GameObjectName, "npc_dream_dialogue"), ModifyDreamDialogue);
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(sceneName, new(GameObjectName, "npc_dream_dialogue"), ModifyDreamDialogue);
    }

    private void ModifyDreamDialogue(PlayMakerFSM fsm)
    {
        if (!Placement.Items.All(x => x.IsObtained()))
        {
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Give Items",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() => fsm.GetState("Idle").ClearTransitions()),
                    new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
                    {
                        FlingType = flingType,
                        Container = Container.Tablet,
                        MessageType = MessageType.Any
                    }, callback), "CONVO_FINISH")
                }
            });
            fsm.GetState("Give Items").AddTransition("CONVO_FINISH", "Box Down");
            fsm.GetState("Impact").AdjustTransition("FINISHED", "Give Items");
            // Remove the box down event (the textbox will be handled in the UIDef)
            fsm.GetState("Box Down").RemoveAction(0);
        }
        else
            fsm.GetState("Idle").ClearTransitions();
    }
}
