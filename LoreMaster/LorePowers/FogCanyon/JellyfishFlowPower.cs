using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using UnityEngine;

namespace LoreMaster.LorePowers.FogCanyon;

public class JellyfishFlowPower : Power
{
    #region Constructors

    public JellyfishFlowPower() : base("Jellyfish Flow", Area.FogCanyon)
    {
        CustomText = "This is great! When I pass this test, I'll be an official jellyfish spotter! Hey, Kevin. I don't think it's working. Hey, hey, Kevin! She's here! Look, she's here! She's here, Kevin!";
    }

    #endregion

    #region Event Handler

    private void OnSetVelocity2DAction(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Knight") && string.Equals(self.Fsm.FsmComponent.FsmName, "Surface Water"))
        {
            if (string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Swim Right"))
            {
                self.x.Value = Active ? 20f : 5f;
            }
            else if (string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Swim Left"))
            {
                self.x.Value = Active ? -20f : -5f;
            }
        }

        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += OnSetVelocity2DAction;
    }

    #endregion
}

