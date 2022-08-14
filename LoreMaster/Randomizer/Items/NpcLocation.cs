using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using System.Linq;

namespace LoreMaster.Randomizer.Items;

internal class NpcLocation : AutoLocation
{
    #region Constructors

    public NpcLocation() { }

    #endregion

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
        if (string.Equals(ObjectName, "Queen", System.StringComparison.InvariantCultureIgnoreCase))
        {
            startState = fsm.GetState("NPC Anim");
            transitionEnd = "Summon";
        }
        else
        {
            startState = fsm.GetState("Hero Anim");
            if (startState == null)
                startState = fsm.GetState("Hero Look");
            transitionEnd = "Talk Finish";
        }
        startState.ClearTransitions();
        // We make sure to wait until the item give is done to prevent giving the player control to early. This is just for the case a npc actually has a lore item.
        startState.AddLastAction(new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
        {
            FlingType = flingType,
            Container = Container.Tablet,
            MessageType = MessageType.Any,
        }, callback), "CONVO FINISHED"));
        startState.AddTransition("CONVO FINISHED", transitionEnd);

        // Disable the talk option, if all items are obtained.
        fsm.GetState("End").AddLastAction(new Lambda(() =>
        {
            if (Placement.Items.All(x => x.IsObtained()))
            {
                PlayMakerFSM control = fsm.gameObject.LocateMyFSM("npc_control");
                control.GetState("Idle").ClearTransitions();
                control.GetState("In Range").Actions = new FsmStateAction[]
                {
                    new Lambda(() => fsm.SendEvent("OUT OF RANGE"))
                }; 
            }
        }));

        // Disable the talk option, if all items are obtained.
        fsm.GetState("Init").AddLastAction(new Lambda(() =>
        {
            if (Placement.Items.All(x => x.IsObtained()))
                fsm.gameObject.LocateMyFSM("npc_control").GetState("Idle").ClearTransitions();
        }));
    }

    /// <summary>
    /// Creates a npc locations. This method is used, because the deserialization of IC calls the default constructor (or another constructor, but with <see langword="null"/> values.
    /// </summary>
    public static NpcLocation CreateLocation(string locationName, string scene, string objectName, string fsmName = "Conversation Control")
        => new()
        {
            name = locationName + "_Dialogue",
            sceneName = scene,
            ObjectName = objectName,
            FsmName = fsmName
        };
}
