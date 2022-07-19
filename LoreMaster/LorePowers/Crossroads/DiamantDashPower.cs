using LoreMaster.Enums;
using LoreMaster.Helper;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Crossroads;

public class DiamantDashPower : Power
{
    #region Members

    private SpriteRenderer _crystalHeartSprite;
    private Sprite _originalSprite;
    private Sprite _corelessSprite;
    private Sprite _shelllessSprite;
    private Sprite _diamondSprite;
    private tk2dSprite _trailSprite;
    private bool _currentlyHold;

    #endregion

    #region Constructors

    public DiamantDashPower() : base("Diamond Dash", Area.Crossroads)
    {
        Hint = "The shell of crystal heart is powered with the might of diamonds, which causes a quicker energie cast and allows you to stop you mid air. Hold up to remain at your position. Drains souls rapidly.";
        Description = "Crystal Heart is cast 0.2 seconds faster and be hold midair with up press. Doubled, if you have Diamond Core unlocked. Drains 20 Soul per second (or 10 if you have Diamond Core).";
        CustomText = "... You know the rules and so do I... Oh! Hello fellow adventurer! Are you also looking for the incredible rare diamond that should be somewhere in this mountain?" +
            " The tales of it's creation are really interesting. I've even made a small shell in which the stone can fit once I obtained it. If I lend you this, would you be helping me finding the stone?" +
            " For me it's just about finding the stone, I don't care how many hours it may take. Even if I have to dig to the core of the mountain. Let's get back to work, lalalala. " +
            "(After this myla lived a happy life for the following 50 years and died of old age." +
            " This is lore master canon and I fight you on that. Nothing bad would ever happen to her.)";
    }

    #endregion

    #region Properties

    public bool HasDiamondCore => LoreMaster.Instance.ActivePowers.ContainsKey("QUIRREL") && LoreMaster.Instance.ActivePowers["QUIRREL"].Active;

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        _crystalHeartSprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Super Dash").GetComponent<SpriteRenderer>();
        _originalSprite = _crystalHeartSprite.sprite;
        _corelessSprite = SpriteHelper.CreateSprite("DiamondHeart_Coreless");
        _shelllessSprite = SpriteHelper.CreateSprite("DiamondHeart_Shellless");
        _diamondSprite = SpriteHelper.CreateSprite("DiamondHeart");
        _trailSprite = GameObject.Find("Knight").transform.Find("Effects/SD Trail").GetComponent<tk2dSprite>();
    }

    protected override void Enable()
    {
        if (HasDiamondCore)
        {
            _crystalHeartSprite.sprite = _diamondSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value -= .6f;
        }
        else
        { 
            _crystalHeartSprite.sprite = _corelessSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value -= .3f;
        }
        _trailSprite.color = Color.cyan;
        On.HeroController.Update += HeroController_Update;
    }

    private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
    {
        orig(self);
        if (!_currentlyHold && HeroController.instance.cState.superDashing && InputHandler.Instance.inputActions.up.IsPressed)
             _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(HoldPosition());
    }

    protected override void Disable()
    {
        if (HasDiamondCore)
        {
            _crystalHeartSprite.sprite = _shelllessSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value += .6f;
        }
        else
        {
            _crystalHeartSprite.sprite = _originalSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value += .3f;
        }
        _currentlyHold = false;
        _trailSprite.color = Color.white;
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
            if(passedTime >= .2f)
            {
                passedTime = 0f;
                HeroController.instance.TakeMP(HasDiamondCore ? 2 : 4);
            }
        }
        _currentlyHold = false;
    }

    #endregion
}
