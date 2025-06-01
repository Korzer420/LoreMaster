using HutongGames.PlayMaker;

using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Helper;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class ReturnToUnnPower : Power
{
    #region Members

    private bool _movementSpeedBuff;

    #endregion

    #region Constructors

    public ReturnToUnnPower() : base("Return to Unn", Area.Greenpath) { }

    #endregion

    #region Event handler

    private void SetFloatValue_OnEnter(On.HutongGames.PlayMaker.Actions.SetFloatValue.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetFloatValue self)
    {
        orig(self);
        if (self.IsCorrectContext("Spell Control", "Knight", "Slug Speed") && string.Equals(self.floatVariable?.Name, "Slug Speed L"))
            self.floatVariable.Value += PlayerData.instance.GetBool("equippedCharm_7") ? -6 : -3f;
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() 
    { 
        StartRoutine(() => AdjustMovementSpeed());
        On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += SetFloatValue_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (_movementSpeedBuff)
        {
            HeroController.instance.WALK_SPEED -= 3f;
            HeroController.instance.RUN_SPEED -= 3f;
            HeroController.instance.RUN_SPEED_CH -= 3f;
            HeroController.instance.RUN_SPEED_CH_COMBO -= 3f;
            HeroController.instance.DASH_COOLDOWN += .5f;
            HeroController.instance.DASH_COOLDOWN_CH += .5f;
        }
        _movementSpeedBuff = false;
        On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter -= SetFloatValue_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistEnable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(DragLeft());
    
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

    /// <summary>
    /// Slowly drags the player to the left
    /// </summary>
    /// <returns></returns>
    private IEnumerator DragLeft()
    {
        while (true)
        {
            if (!HeroController.instance.acceptingInput || PlayerData.instance.GetBool("atBench")
                || (!HeroController.instance.cState.facingRight && HeroController.instance.hero_state == GlobalEnums.ActorStates.running))
            {
                HeroController.instance.cState.inConveyorZone = false;
                yield return new WaitUntil(() => HeroController.instance.acceptingInput && (HeroController.instance.cState.facingRight
                || HeroController.instance.hero_state != GlobalEnums.ActorStates.running) && !PlayerData.instance.GetBool("atBench"));
            }
            HeroController.instance.SetConveyorSpeed(-2.5f);
            HeroController.instance.cState.inConveyorZone = true;
            yield return null;
        }
    }

    #endregion
}

