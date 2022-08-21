using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.WhitePalace;

public class ShadowForgedPower : Power
{
    #region Members

    private tk2dSpriteAnimationClip _shadow;

    #endregion

    #region Constructors

    public ShadowForgedPower() : base("Shadow Forged", Area.WhitePalace) { }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        // Each scene we try to find the shade cloak clip and adjust it, since it isn't loaded by default.
        if (_shadow == null)
        {
            _shadow = GameObject.Find("Knight/Effects").transform.Find("Shadow Recharge").gameObject.GetComponent<tk2dSpriteAnimator>().CurrentClip;
            if(_shadow != null)
                _shadow.fps += 10f;
        }
    };

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        PlayMakerFSM fsm = GameObject.Find("Knight/Effects").transform.Find("Shadow Recharge").gameObject.LocateMyFSM("Recharge Effect");
        _shadow = fsm.gameObject.GetComponent<tk2dSpriteAnimator>().CurrentClip;
        fsm.GetState("Init").AddLastAction(new Lambda(() =>
        {
            fsm.FsmVariables.GetFsmFloat("Wait time").Value = Active ? .2f : .4f;
        }));
        fsm = GameObject.Find("Knight/Attacks").LocateMyFSM("Set Sharp Shadow Damage");
        fsm.GetState("Set").RemoveFirstActionOfType<SetFsmInt>();
        fsm.GetState("Set").AddLastAction(new Lambda(() =>
        {
            int damage = fsm.FsmVariables.FindFsmInt("nailDamage").Value;
            if (Active)
                damage *= 2;
            fsm.transform.Find("Sharp Shadow").gameObject.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = damage;
        }));
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN -= .2f;
        if (_shadow != null)
            _shadow.fps += 10f;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN += .2f;
        if (_shadow != null)
            _shadow.fps -= 10f;
    }

    #endregion
}
