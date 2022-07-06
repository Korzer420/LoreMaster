using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.WhitePalace;

public class ShadowForgedPower : Power
{
    #region Members

    private GameObject _shadow;

    #endregion

    #region Constructors

    public ShadowForgedPower() : base("",Area.WhitePalace)
    {
        _shadow = GameObject.Find("Knight/Effects").transform.Find("Shadow Recharge").gameObject;
        PlayMakerFSM fsm = _shadow.LocateMyFSM("Recharge Effect");

        fsm.GetState("Init").AddLastAction(new Lambda(() =>
        {
            if (IsCurrentlyActive())
                fsm.FsmVariables.GetFsmFloat("Wait time").Value -= .4f;
        }));
    }

    #endregion

    #region Public Methods

    public override void Enable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN -= .4f;
        _shadow.GetComponent<tk2dSpriteAnimator>().ClipFps += 8f;
    }

    public override void Disable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN += .4f;
        _shadow.GetComponent<tk2dSpriteAnimator>().ClipFps -= 8f;
    }

    #endregion
}
