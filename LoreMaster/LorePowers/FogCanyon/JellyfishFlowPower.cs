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

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        PlayMakerFSM knightFSM = GameObject.Find("Knight").LocateMyFSM("Surface Water");
        knightFSM.GetState("Swim Right").ReplaceAction(new Lambda(() =>
        {
            knightFSM.FsmVariables.GetFsmFloat("Swim Speed").Value = Active ? 20f : 5f;
            knightFSM.FsmVariables.GetFsmString("Idle Anim").Value = "Surface Idle";
        })
        { Name = "Jellyfish swim" }, 1);

        knightFSM.GetState("Swim Left").ReplaceAction(new Lambda(() =>
        {
            knightFSM.FsmVariables.GetFsmFloat("Swim Speed neg").Value = Active ? -20f : -5f;
            knightFSM.FsmVariables.GetFsmString("Idle Anim").Value = "Surface Idle";
        })
        { Name = "Jellyfish swim" }, 1);
    }

    #endregion
}

