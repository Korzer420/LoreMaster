using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using System;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Peaks;

public class DiamondCorePower : Power
{
    #region Members

    private HealthManager[] _enemies;
    private SpriteRenderer _crystalHeartSprite;
    private Sprite _originalSprite;
    private Sprite _corelessSprite;
    private Sprite _shelllessSprite;
    private Sprite _diamondSprite;
    private tk2dSprite _trailSprite;
    private float _speed = 0f;
    private float _carryingSpeed = 30f;
    private int _carryingDamage = 10;

    #endregion

    #region Constructors

    public DiamondCorePower() : base("Diamond Core", Area.Peaks)
    {
        Hint = "The crystal heart's core absorbed the power of diamond and got even stronger. If you hit a wall, all foes may be stunned shortly. The power of the diamond increases over time, makes you unstoppable once you got enough power.";
        Description = "Crystal Heart snares all enemies in the room if you hit a wall. The duration of the stun and c dash damage increases with the c dash duration. " +
            "(Stun duration is capped at 10 seconds, gain 5 damage and 10% speed per second, gain invincibility after 3 seconds.)";
        CustomText = "Isn't the view just beautiful? When I just look at this, all my thought feel way less heavier than before. It feels almost... empty. Have you already looked around here a bit? " +
            "These crystals here contain an mysterious power. Although it seemed to me, that they people actually looked for something even more powerful. Here, I found that crystal from the remains of another adventurer." +
            " It emits a power far beyond everything else that you probably can mine here. It only needs a fitting vessel, but for an adventurer like you, this shouldn't be a problem.";
    }

    #endregion

    #region Properties

    public bool HasDiamondDash => LoreMaster.Instance.ActivePowers.ContainsKey("MYLA") && LoreMaster.Instance.ActivePowers["MYLA"].Active;

    public override Action SceneAction => () =>
    {
        _enemies = GameObject.FindObjectsOfType<HealthManager>();
        if (_crystalHeartSprite == null)
        {
            _crystalHeartSprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Super Dash").GetComponent<SpriteRenderer>();
            if (_crystalHeartSprite != null)
            {
                _originalSprite = _crystalHeartSprite?.sprite;
                _crystalHeartSprite.sprite = HasDiamondDash ? _diamondSprite : _corelessSprite;
            }
        }
    };

    #endregion

    #region Event Handler

    private bool HeroController_CanTakeDamage(On.HeroController.orig_CanTakeDamage orig, HeroController self)
    {
        if (orig(self))
            return _speed > -39f && _speed < 39f;
        return false;
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        try
        {
            _crystalHeartSprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Super Dash").GetComponent<SpriteRenderer>();
            _originalSprite = _crystalHeartSprite.sprite;
            _corelessSprite = SpriteHelper.CreateSprite("DiamondHeart_Coreless");
            _shelllessSprite = SpriteHelper.CreateSprite("DiamondHeart_Shellless");
            _diamondSprite = SpriteHelper.CreateSprite("DiamondHeart");
            HeroController.instance.superDash.GetState("Dash Start").ReplaceAction(new Lambda(() =>
            {
                HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.SetActive(true);
                if (Active)
                {
                    _carryingSpeed = HeroController.instance.cState.facingRight ? 30f : -30f;
                    _carryingDamage = 10;
                    LoreMaster.Instance.Handler.StartCoroutine(ChargeUp());
                }
            }), 25);

            HeroController.instance.superDash.GetState("Enter Super Dash").ReplaceAction(new Lambda(() =>
            {
                HeroController.instance.SetCState("superDashing", true);
                if (Active)
                    LoreMaster.Instance.Handler.StartCoroutine(ChargeUp());
            }), 6);
            HeroController.instance.superDash.GetState("Hit Wall").ReplaceAction(new Lambda(() =>
            {
                if (Active)
                    foreach (HealthManager enemy in _enemies)
                        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(StunEnemy(enemy.gameObject));

                HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.SetActive(false);
            })
            { Name = "Wall Crash" }, 8);
            _trailSprite = GameObject.Find("Knight").transform.Find("Effects/SD Trail").GetComponent<tk2dSprite>();
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in initialize: " + exception.Message);
        }
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        _enemies = GameObject.FindObjectsOfType<HealthManager>();
        if (_crystalHeartSprite != null)
            _crystalHeartSprite.sprite = HasDiamondDash ? _diamondSprite : _shelllessSprite;
        if (HasDiamondDash)
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value = .2f;
        On.HeroController.CanTakeDamage += HeroController_CanTakeDamage;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (HasDiamondDash)
        {
            _crystalHeartSprite.sprite = _corelessSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value = .5f;
        }
        else
            _crystalHeartSprite.sprite = _originalSprite;
        _enemies = null;
        On.HeroController.CanTakeDamage -= HeroController_CanTakeDamage;
        HeroController.instance.superDash.FsmVariables.FindFsmFloat("Current SD Speed").Value = 30f;
        HeroController.instance.superDash.FsmVariables.FindFsmFloat("Speed").Value = 30f;
        HeroController.instance.superDash.FsmVariables.FindFsmFloat("Superdash Speed").Value = 30f;
        HeroController.instance.superDash.FsmVariables.FindFsmFloat("Superdash Speed neg").Value = -30f;
        HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = 10;
        _trailSprite.color = Color.white;
        _carryingDamage = 10;
        _carryingSpeed = 30;
        _speed = 30;
    }

    #endregion

    #region Private Methods

    private IEnumerator StunEnemy(GameObject enemy)
    {
        float passedTime = 0f;
        // Max speed is 60 so stun time can be 10 seconds.
        float stunTime = 1f + (((_speed < 0 ? _speed * -1 : _speed) - 30) / 3);

        Vector3 positionToLock = enemy.transform.localPosition;
        while (passedTime <= stunTime)
        {
            yield return null;
            passedTime += Time.deltaTime;
            enemy.transform.localPosition = positionToLock;
        }
    }

    private IEnumerator ChargeUp()
    {
        FsmInt damage = HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt");
        int defaultDamage = damage.Value;
        FsmFloat speed = HeroController.instance.superDash.FsmVariables.FindFsmFloat("Current SD Speed");
        float passedTime = 0f;
        speed.Value = (_carryingSpeed < 0 && HeroController.instance.cState.facingRight) || (_carryingSpeed > 0 && !HeroController.instance.cState.facingRight)
            ? -_carryingSpeed
            : _carryingSpeed;
        _speed = speed.Value;
        damage.Value = _carryingDamage;
        while (HeroController.instance.cState.superDashing)
        {
            passedTime += Time.deltaTime;
            if (passedTime >= 0.2f)
            {
                passedTime = 0f;
                if (damage.Value < 60)
                    damage.Value++;
                if (_speed < 60f && _speed > -60f)
                    _speed += HeroController.instance.cState.facingRight ? .6f : -.6f;
                speed.Value = _speed;
                _carryingSpeed = _speed;
                _carryingDamage = damage.Value;
                if (_speed >= 39f || _speed <= -39f)
                    _trailSprite.color = Color.cyan;
            }

            do
                yield return null;
            while (GameManager.instance.IsGamePaused());
        }
        _trailSprite.color = Color.white;
        damage.Value = defaultDamage;
        speed.Value = 30f;
        _speed = 30f;
    }

    #endregion
}