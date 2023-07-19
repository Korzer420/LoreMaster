using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Manager;
using System;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class CamouflagePower : Power
{
    #region Members

    private bool _isCamouflaged;

    #endregion

    #region Constructors

    public CamouflagePower() : base("Camouflage", Area.Greenpath) { }

    #endregion

    #region Event handler

    private bool HeroController_CanTakeDamage(On.HeroController.orig_CanTakeDamage orig, HeroController self)
    {
        bool result = orig(self);

        if (_isCamouflaged)
            return false;
        return result;
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(WaitForCamouflage());
        On.HeroController.CanTakeDamage += HeroController_CanTakeDamage;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (_runningCoroutine != null)
            LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        _isCamouflaged = false;
        PlayerData.instance.SetBool(nameof(PlayerData.instance.isInvincible), false);
        HeroHelper.Sprite.color = Color.white;
        On.HeroController.CanTakeDamage -= HeroController_CanTakeDamage;
    }

    /// <inheritdoc/>
    protected override void TwistEnable() => StartRoutine(() => HideHero());

    #endregion

    #region Private Methods

    private IEnumerator WaitForCamouflage()
    {
        while (true)
        {
            float passedTime = 0f;
            while (passedTime < 5f)
            {
                yield return null;
                passedTime += Time.deltaTime;
                if (HeroController.instance.hero_state != GlobalEnums.ActorStates.idle || InputHandler.Instance.inputActions.attack.IsPressed
                    || InputHandler.Instance.inputActions.dash.IsPressed || InputHandler.Instance.inputActions.superDash.IsPressed
                    || InputHandler.Instance.inputActions.dreamNail.IsPressed || InputHandler.Instance.inputActions.quickCast.IsPressed
                    || ((InputHandler.Instance.inputActions.focus.IsPressed
                    || InputHandler.Instance.inputActions.cast.IsPressed) && !PlayerData.instance.GetBool("equippedCharm_28"))
                    || InputHandler.Instance.inputActions.quickMap.IsPressed || !HeroController.instance.acceptingInput)
                    break;
            }
            if (passedTime >= 5f)
            {
                HeroHelper.Sprite.color = Color.green;
                _isCamouflaged = true;
            }
            else
            {
                HeroHelper.Sprite.color = Color.white;
                _isCamouflaged = false;
            }
        }
    }

    private IEnumerator HideHero()
    {
        while (true)
        {
            if (HeroController.instance == null || HeroHelper.Sprite == null)
                yield return new WaitUntil(() => HeroController.instance != null && HeroHelper.Sprite);
            Color color = HeroHelper.Sprite.color;
            color.a = 1f;
            HeroHelper.Sprite.color = color;
            yield return null;
        }
    }

    #endregion
}
