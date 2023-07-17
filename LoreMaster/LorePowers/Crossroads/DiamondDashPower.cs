using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using LoreMaster.Enums;

using LoreMaster.Manager;
using System;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Crossroads;

public class DiamondDashPower : Power
{
    #region Members

    private SpriteRenderer _crystalHeartSprite;
    private Sprite _originalSprite;
    private Sprite _corelessSprite;
    private Sprite _shelllessSprite;
    private Sprite _diamondSprite;
    private bool _currentlyHold;

    #endregion

    #region Constructors

    public DiamondDashPower() : base("Diamond Dash", Area.Crossroads)
    {
       CustomText = "... You know the rules and so do I... Oh! Hello fellow adventurer! Are you also looking for the incredible rare diamond that should be somewhere in this mountain?" +
            " The tales of it's creation are really interesting. I've even made a small shell in which the stone can fit once I obtained it. If I lend you this, would you be helping me finding the stone?" +
            " For me it's just about finding the stone, I don't care how many hours it may take. Even if I have to dig to the core of the mountain. Let's get back to work, lalalala. " +
            "(After this myla lived a happy life for the following 50 years and died of old age." +
            " This is lore master canon and I fight you on that. Nothing bad would ever happen to her.)";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the indicator if diamond core has also been acquired.
    /// </summary>
    public bool HasDiamondCore => PowerManager.HasObtainedPower("QUIRREL");

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    { 
        if(_crystalHeartSprite == null)
        {
            _crystalHeartSprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Super Dash").GetComponent<SpriteRenderer>();
            if (_crystalHeartSprite != null)
            {
                _originalSprite = _crystalHeartSprite.sprite;
                _crystalHeartSprite.sprite = HasDiamondCore ? _diamondSprite : _corelessSprite;
            }
        }
    };

    #endregion

    #region Event handler

    private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
    {
        orig(self);
        if (!_currentlyHold && HeroController.instance.cState.superDashing && InputHandler.Instance.inputActions.up.IsPressed)
            _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(HoldPosition());
    }

    private void Wait_OnEnter(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
    {
        if (self.IsCorrectContext("Superdash", "Knight", "Wall Charge") || self.IsCorrectContext("Superdash", "Knight", "Ground Charge"))
            self.time.Value += State == PowerState.Active ? (HasDiamondCore ? -.5f : -.25f) : (State == PowerState.Twisted ? .6f : 0f);
        orig(self);
    }

    private void FsmStateAction_Finish(On.HutongGames.PlayMaker.FsmStateAction.orig_Finish orig, HutongGames.PlayMaker.FsmStateAction self)
    {
        orig(self);
        if (self is Wait waitAction && (self.IsCorrectContext("Superdash", "Knight", "Wall Charge") || self.IsCorrectContext("Superdash", "Knight", "Ground Charge")))
            waitAction.time.Value -= State == PowerState.Active ? (HasDiamondCore ? -.5f : -.25f) : (State == PowerState.Twisted ? .6f : 0f);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _crystalHeartSprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Super Dash").GetComponent<SpriteRenderer>();
        _originalSprite = _crystalHeartSprite?.sprite;
        _corelessSprite = SpriteHelper.CreateSprite<LoreMaster>("Base.DiamondHeart_Coreless");
        _shelllessSprite = SpriteHelper.CreateSprite<LoreMaster>("Base.DiamondHeart_Shellless");
        _diamondSprite = SpriteHelper.CreateSprite<LoreMaster>("Base.DiamondHeart");
        On.HutongGames.PlayMaker.Actions.Wait.OnEnter += Wait_OnEnter;
        On.HutongGames.PlayMaker.FsmStateAction.Finish += FsmStateAction_Finish;
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.Wait.OnEnter -= Wait_OnEnter;
        On.HutongGames.PlayMaker.FsmStateAction.Finish -= FsmStateAction_Finish;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        if (_crystalHeartSprite != null)
            _crystalHeartSprite.sprite = HasDiamondCore ? _diamondSprite : _corelessSprite;
        On.HeroController.Update += HeroController_Update;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (_crystalHeartSprite != null)
            _crystalHeartSprite.sprite = HasDiamondCore ? _shelllessSprite : _originalSprite;
        _currentlyHold = false;
        On.HeroController.Update -= HeroController_Update;
    }

    #endregion

    #region Private Methods

    private IEnumerator HoldPosition()
    {
        _currentlyHold = true;
        Vector3 heroPosition = HeroController.instance.transform.localPosition;
        float passedTime = 0f;
        while (PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge)) > 3 && HeroController.instance.cState.superDashing && InputHandler.Instance.inputActions.up.IsPressed)
        {
            yield return null;
            HeroController.instance.transform.localPosition = heroPosition;
            passedTime += Time.deltaTime;
            if (passedTime >= .2f)
            {
                passedTime = 0f;
                HeroController.instance.TakeMP(HasDiamondCore ? 2 : 4);
            }
        }
        _currentlyHold = false;
    }

    #endregion
}
