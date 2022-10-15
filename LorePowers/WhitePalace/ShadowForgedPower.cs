using HutongGames.PlayMaker.Actions;
using LoreMaster.Enums;
using LoreMaster.Helper;
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

    #region Event handler

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        HeroController.instance.SHADOW_DASH_COOLDOWN += State == PowerState.Twisted ? .4f : -.2f;
        _shadow = GameObject.Find("Knight/Effects").transform.Find("Shadow Recharge").gameObject.GetComponent<tk2dSpriteAnimator>().GetClipByName("Shadow Recharge");
    }

    private void Wait_OnEnter(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
    {
        if (self.IsCorrectContext("Recharge Effect", "Shadow Recharge", "Wait"))
            self.time.Value += State == PowerState.Active ? -.2f : (State == PowerState.Twisted ? .4f : 0f);
        orig(self);
    }

    private void SetFsmInt_OnEnter(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, SetFsmInt self)
    {
        if (self.IsCorrectContext("Set Sharp Shadow Damage", "Attacks", "Set"))
            self.setValue.Value = State == PowerState.Twisted ? Convert.ToInt32(self.setValue.Value * .5f) : self.setValue.Value * 2;
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _shadow = GameObject.Find("Knight/Effects").transform.Find("Shadow Recharge").gameObject.GetComponent<tk2dSpriteAnimator>().GetClipByName("Shadow Recharge");
        On.HutongGames.PlayMaker.Actions.Wait.OnEnter += Wait_OnEnter;
        
    }

    /// <inheritdoc/>
    protected override void Terminate() => On.HutongGames.PlayMaker.Actions.Wait.OnEnter -= Wait_OnEnter;

    /// <inheritdoc/>
    protected override void Enable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN -= .2f;
        _shadow.fps += 10f;
        On.HeroController.Start += HeroController_Start;
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += SetFsmInt_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN += .2f;
        _shadow.fps -= 10f;
        On.HeroController.Start -= HeroController_Start;
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter -= SetFsmInt_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN += .4f;
        On.HeroController.Start += HeroController_Start;
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += SetFsmInt_OnEnter;
        _shadow.fps -= 15f;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN -= .4f;
        On.HeroController.Start -= HeroController_Start;
        _shadow.fps += 15f;
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter -= SetFsmInt_OnEnter;
    }

    #endregion
}
