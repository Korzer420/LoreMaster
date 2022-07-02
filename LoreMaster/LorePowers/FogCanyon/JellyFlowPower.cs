using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.FogCanyon;

public class JellyFlowPower : Power
{
    #region Members

    private bool _enabled;

    #endregion

    #region Constructors

    public JellyFlowPower() : base("",Area.FogCanyon)
    {
        PlayMakerFSM knightFSM = GameObject.Find("Knight").LocateMyFSM("Surface Water");
        knightFSM.GetState("Swim Right").AddFirstAction(new Lambda(() =>
        {
            LoreMaster.Instance.Log("Called fsm");
            knightFSM.FsmVariables.GetFsmFloat("Swim Speed").Value = _enabled ? 15f : 5f;
        }));

        knightFSM.GetState("Swim Left").AddFirstAction(new Lambda(() =>
        {
            LoreMaster.Instance.Log("Called fsm");
            knightFSM.FsmVariables.GetFsmFloat("Swim Speed neg").Value = _enabled ? -15f : -5f;
        }));
    }

    #endregion

    #region Public Methods

    public override void Enable()
    {
        _enabled = true;
    }

    public override void Disable()
    {
        _enabled = false;
    }

    #endregion

}

