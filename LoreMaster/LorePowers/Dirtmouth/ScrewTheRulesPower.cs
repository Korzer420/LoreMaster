using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using UnityEngine;

namespace LoreMaster.LorePowers;

/// <summary>
/// Fury of the Fallen is active, while you have two health, but only increases your damage by 50%.
/// </summary>
public class ScrewTheRulesPower : Power
{
    #region Constructors

    public ScrewTheRulesPower() : base("Screw the Rules", Area.Dirtmouth)
    {
        Hint = "You rage is weaker but quicker. Still deadly.";
        Description = "Fury of the Fallen is now also active with 2 hp, but the damage buff is decreased to 50%.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        PlayMakerFSM fsm = GameObject.Find("Charm Effects").LocateMyFSM("Fury");
        FsmState state = fsm.GetState("Check HP");

        // Remove "HP Compare" action and add new one, so fury is active while have two OR LESS health
        state.RemoveAction(6);
        state.AddLastAction(new Lambda(() =>
        {
            if (PlayerData.instance.health == 1 || (PlayerData.instance.health == 2 && Active))
                fsm.SendEvent("FURY");
            else
                fsm.SendEvent("CANCEL");
        }));

        // Also changing the recheck for after getting hit or healed
        state = fsm.GetState("Recheck");
        state.RemoveAction(1);
        state.InsertAction(new Lambda(() =>
        {
            if (PlayerData.instance.health == 1 || (PlayerData.instance.health == 2 && Active))
                fsm.SendEvent("RETURN");
        }), 1);

        // We are nerfing the damage increase to 50% (The action 9 to 12 are setting the modifier in the nail fsm)
        state = fsm.GetState("Activate");
        state.AddFirstAction(new Lambda(() =>
        {
            foreach (SetFsmFloat action in state.GetActionsOfType<SetFsmFloat>())
                action.setValue.Value = Active ? 1.5f : 1.75f;
        }));
    }

    #endregion
}
