using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations;

/// <summary>
/// A location which gives items in a normal conversation.
/// </summary>
internal class DialogueLocation : AutoLocation
{
    #region Properties

    public string ObjectName { get; set; }

    public string FsmName { get; set; }

    #endregion

    protected override void OnLoad()
    {
        Events.AddFsmEdit(sceneName, new(ObjectName, FsmName), SkipDialog);
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(sceneName, new(ObjectName, FsmName), SkipDialog);
    }

    private void SkipDialog(PlayMakerFSM fsm)
    {
        FsmState startState;
        string transitionEnd;
        if (string.Equals(ObjectName, "Queen", System.StringComparison.CurrentCultureIgnoreCase))
        {
            startState = fsm.GetState("NPC Anim");
            transitionEnd = "Summon";
        }
        else if (string.Equals(fsm.FsmName, "inspect_region"))
        {
            startState = fsm.GetState("Hero Look Up?");
            transitionEnd = "Look Up End?";
        }
        else
        {
            startState = fsm.GetState("Hero Anim");
            if (startState == null)
                startState = fsm.GetState("Hero Look");
            transitionEnd = "Talk Finish";

            // If not all items are obtained ghost npc are unkillable.
            if (fsm.gameObject.LocateMyFSM("ghost_npc_death") is PlayMakerFSM ghostDeath && !Placement.Items.All(x => x.IsObtained()))
                ghostDeath.GetState("Idle").ClearTransitions();
        }
        startState.ClearTransitions();
        startState.AddLastAction(new Lambda(() =>
        {
            PlayMakerFSM control = fsm.gameObject.LocateMyFSM("npc_control");
            control.GetState("Idle").ClearTransitions();
            control.GetState("In Range").Actions = new FsmStateAction[]
            {
                    new Lambda(() => fsm.SendEvent("OUT OF RANGE"))
            };
        }));
        // We make sure to wait until the item give is done to prevent giving the player control to early. This is just for the case a npc actually has a lore item.
        startState.AddLastAction(new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
        {
            FlingType = flingType,
            Container = Container.Tablet,
            MessageType = MessageType.Lore,
        }, callback), "CONVO FINISHED"));
        startState.AddTransition("CONVO FINISHED", transitionEnd);

        // Disable the talk option, if all items are obtained.
        fsm.GetState("Init").AddLastAction(new Lambda(() =>
        {
            if (Placement.Items.All(x => x.IsObtained()))
                fsm.gameObject.LocateMyFSM("npc_control").GetState("Idle").ClearTransitions();
        }));
    }

}
