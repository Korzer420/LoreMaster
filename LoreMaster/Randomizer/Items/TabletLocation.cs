using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using System.Linq;

namespace LoreMaster.Randomizer.Items;

internal class TabletLocation : AutoLocation
{
    #region Properties

    public string ObjectName { get; set; }

    public string FsmName { get; set; }

    #endregion

    protected override void OnLoad()
    {
        Events.AddFsmEdit(sceneName, new(ObjectName, FsmName), SkipInspect);
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(sceneName, new(ObjectName, FsmName), SkipInspect);
    }

    private void SkipInspect(PlayMakerFSM fsm)
    {
        FsmState startState = fsm.GetState("Box Up");
        startState.ClearTransitions();
        startState.Actions = new FsmStateAction[] {
        new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
        {
            FlingType = flingType,
            Container = Container.Tablet,
            MessageType = MessageType.Any,
        }, callback), "FINISHED")
        };
        startState.AddTransition("FINISHED", "Convo End");
        FsmState endState = fsm.GetState("Convo End");

        // Disable the talk option, if all items are obtained.
        endState.AddLastAction(new Lambda(() =>
        {
            if (Placement.Items.All(x => x.IsObtained()))
            {
                fsm.GetState("Idle").ClearTransitions();
                fsm.GetState("In Range").Actions = new FsmStateAction[]
                {
                    new Lambda(() => fsm.SendEvent("OUT OF RANGE"))
                };
            }
        }));

        // Disable the talk option, if all items are obtained.
        fsm.GetState("Init").AddLastAction(new Lambda(() =>
        {
            if (Placement.Items.All(x => x.IsObtained()))
                fsm.GetState("Idle").ClearTransitions();
        }));
    }


    /// <summary>
    /// Creates a npc locations. This method is used, because the deserialization of IC calls the default constructor (or another constructor, but with <see langword="null"/> values.
    /// </summary>
    public static TabletLocation CreateLocation(string locationName, string scene, string objectName = "Inspect Region Ghost", string fsmName = "inspect_region")
        => new()
        {
            name = locationName + "_Inspect",
            sceneName = scene,
            ObjectName = objectName,
            FsmName = fsmName
        };
}
