using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;

namespace LoreMaster.LorePowers
{
    /// <summary>
    /// Fury of the Fallen is active, while you have two health, but only increases your damage by 50%.
    /// </summary>
    public class ScrewTheRulesPower : Power
    {
        public ScrewTheRulesPower() : base("TUT_TAB_02", Area.Dirtmouth)
        => Hint = "<br>[Screw the Rules]<br>You rage is weaker but quicker. Still deadly.";


        protected override void Disable()
        {
            PlayMakerFSM fsm = GameObject.Find("Charm Effects").LocateMyFSM("Fury");
            FsmState state = fsm.GetState("Check HP");

            // Reset the health check
            state.RemoveAction(6);
            state.AddMethod(() =>
            {
                if (PlayerData.instance.health == 1)
                    fsm.SendEvent("FURY");
                else
                    fsm.SendEvent("CANCEL");
            });

            // Reset the recheck
            state = fsm.GetState("Recheck");
            state.RemoveAction(1);
            state.AddMethod(() =>
            {
                if (PlayerData.instance.health == 1)
                    fsm.SendEvent("RETURN");
            });

            // Reset the damage nerf
            state = fsm.GetState("Activate");
            for (int actionIndex = 9; actionIndex < 13; actionIndex++)
                state.GetAction<SetFsmFloat>(actionIndex).setValue.Value = 1.75f;
        }

        protected override void Enable()
        {
            PlayMakerFSM fsm = GameObject.Find("Charm Effects").LocateMyFSM("Fury");
            FsmState state = fsm.GetState("Check HP");

            // Remove "HP Compare" action and add new one, so fury is active while have two OR LESS health
            state.RemoveAction(6);
            state.AddMethod(() =>
            {
                if (PlayerData.instance.health == 1 || PlayerData.instance.health == 2)
                    fsm.SendEvent("FURY");
                else
                    fsm.SendEvent("CANCEL");
            });

            // Also changing the recheck for after getting hit or healed
            state = fsm.GetState("Recheck");
            state.RemoveAction(1);
            state.AddMethod(() =>
            {
                if (PlayerData.instance.health == 1 || PlayerData.instance.health == 2)
                    fsm.SendEvent("RETURN");
            });

            // We are nerfing the damage increase to 50% (The action 9 to 12 are setting the modifier in the nail fsm)
            state = fsm.GetState("Activate");
            for (int actionIndex = 8; actionIndex < 13; actionIndex++)
                state.GetAction<SetFsmFloat>(actionIndex).setValue.Value = 1.5f;
        }

    }
}
