using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class ReturnToUnnPower : Power
{
    #region Members

    private bool _movementSpeedBuff;

    #endregion

    #region Constructors

    public ReturnToUnnPower() : base("Return to Unn", Area.Greenpath)
    {
        Hint = "Reject Bugness, return to Slug. You move faster to Unn.";
        Description = "Your Movement speed is increased by 3 and your dash cooldown is reduces by 0.5 seconds, while you facing left.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        FsmState slugSpeed = GameObject.Find("Knight").LocateMyFSM("Spell Control").GetState("Start MP Drain");

        slugSpeed.AddLastAction(new Lambda(() =>
        {
            if (Active)
                slugSpeed.Fsm.Variables.FindFsmFloat("Slug Speed L").Value -= PlayerData.instance.GetBool("equippedCharm_7") ? 6f : 3f;
        }));
    }

    protected override void Enable() =>  _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(AdjustMovementSpeed());
    
    protected override void Disable()
    {
        LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        if (_movementSpeedBuff)
        { 
            HeroController.instance.WALK_SPEED -= 3f;
            HeroController.instance.RUN_SPEED -= 3f;
            HeroController.instance.RUN_SPEED_CH -= 3f;
            HeroController.instance.RUN_SPEED_CH_COMBO -= 3f;
            HeroController.instance.DASH_COOLDOWN += .5f;
            HeroController.instance.DASH_COOLDOWN_CH += .5f;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Increase the movement speed when looking to the left.
    /// </summary>
    /// <returns></returns>
    private IEnumerator AdjustMovementSpeed()
    {
        while (true)
        {
            yield return null;
            if (!HeroController.instance.cState.facingRight)
            {
                if (!_movementSpeedBuff)
                {
                    HeroController.instance.WALK_SPEED += 3f;
                    HeroController.instance.RUN_SPEED += 3f;
                    HeroController.instance.RUN_SPEED_CH += 3f;
                    HeroController.instance.RUN_SPEED_CH_COMBO += 3f;
                    HeroController.instance.DASH_COOLDOWN -= .5f;
                    HeroController.instance.DASH_COOLDOWN_CH -= .5f;
                }
                _movementSpeedBuff = true;
            }
            else if (_movementSpeedBuff)
            {
                HeroController.instance.WALK_SPEED -= 3f;
                HeroController.instance.RUN_SPEED -= 3f;
                HeroController.instance.RUN_SPEED_CH -= 3f;
                HeroController.instance.RUN_SPEED_CH_COMBO -= 3f;
                HeroController.instance.DASH_COOLDOWN += .5f;
                HeroController.instance.DASH_COOLDOWN_CH += .5f;
                _movementSpeedBuff = false;
            }
        }
    }

    #endregion
}

