using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using LoreMaster.Helper;
using LoreMaster.ItemChangerData.Other;
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
        try
        {
            if (Placement.Items.All(x => x.IsObtained()))
            {
                fsm.gameObject.LocateMyFSM("npc_control").GetState("Idle").ClearTransitions();
                return;
            }
            FsmState startState;
            string transitionEnd;
            if (string.Equals(ObjectName, "Queen", System.StringComparison.CurrentCultureIgnoreCase))
            {
                startState = fsm.GetState("NPC Anim");
                transitionEnd = "Summon";
            }
            else
            {
                startState = fsm.GetState("Hero Anim");
                if (startState == null)
                    startState = fsm.GetState("Hero Look");
                // Zote Greenpath/City
                if (startState == null)
                    startState = fsm.GetState("Talk Back");
                transitionEnd = "Talk Finish";
            }

            if (fsm.GetState("Give Items") is null)
            {
                // If not all items are obtained ghost npc are unkillable.
                if (fsm.gameObject.LocateMyFSM("ghost_npc_death") is PlayMakerFSM ghostDeath)
                    ghostDeath.GetState("Idle").ClearTransitions();
                fsm.AddState(new FsmState(fsm.Fsm)
                {
                    Name = "Give Items",
                    Actions = new FsmStateAction[]
                    {
                        new Lambda(() =>
                        {
                            PlayMakerFSM control = fsm.gameObject.LocateMyFSM("npc_control");
                            control.GetState("Idle").ClearTransitions();
                        }),
                        new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
                        {
                            FlingType = flingType,
                            Container = Container.Tablet,
                            MessageType = name == LocationList.Dreamer_Tablet ? MessageType.Big : MessageType.Lore,
                        }, callback), "CONVO_FINISH")
                    }
                });

                startState.AdjustTransition("FINISHED", "Give Items");
                // As if Zote wasn't annoying enough...
                if (fsm.GetState("Talk R") is FsmState state)
                    state.AdjustTransition("FINISHED", "Give Items");
                if (fsm.GetState("Check Active") is FsmState state2)
                    state2.Actions = new FsmStateAction[1]
                    {
                        new Lambda(() => fsm.SendEvent("FINISHED"))
                    };

                fsm.GetState("Give Items").AddTransition("CONVO_FINISH", transitionEnd);
            }
        }
        catch (System.Exception exception)
        {
            LoreMaster.Instance.LogError(exception.Message);
        }
    }
}
