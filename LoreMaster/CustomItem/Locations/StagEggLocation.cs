using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Util;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.Randomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.CustomItem.Locations;

public class StagEggLocation : AutoLocation
{
    protected override void OnLoad()
    {
        Events.AddFsmEdit(sceneName, new("Eggs Inspect", "Conversation Control"), ModifyEgg);
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(sceneName, new("Eggs Inspect", "Conversation Control"), ModifyEgg);
    }

    private void ModifyEgg(PlayMakerFSM fsm)
    {
        // Make the egg available 300 seconds after is has been used.
        if (!StagAdoptionPower.Instance.CanSpawnStag && Placement.Items.FirstOrDefault(x => string.Equals(x.name, "Stag_Egg")) is BoolItem item
            && item.IsObtained() && StagAdoptionPower.Instance.HatchMoment < Time.realtimeSinceStartupAsDouble - 300)
            item.RefreshObtained();

        FsmState currentWorkingState = fsm.GetState("Idle");
        if (RandomizerManager.PlayingRandomizer && Placement.Items.All(x => x.IsObtained()))
        {
            currentWorkingState.ClearTransitions();
            return;
        }
        currentWorkingState = new FsmState(fsm.Fsm)
        {
            Name = "Skip",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => PlayerData.instance.SetBool("stagEggInspected", true)),
                new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
                {
                    FlingType = flingType,
                    Container = Container.Tablet,
                    MessageType = MessageType.Any,
                }, callback), "CONVO FINISHED")
            }
        };
        currentWorkingState.AddTransition("CONVO FINISHED", "Talk Finish");
        fsm.AddState(currentWorkingState);
        currentWorkingState = fsm.GetState("Hero Anim");
        currentWorkingState.ClearTransitions();
        currentWorkingState.AddTransition("FINISHED", "Skip");
    }
}
