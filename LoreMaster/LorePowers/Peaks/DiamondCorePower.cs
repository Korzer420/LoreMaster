using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using LoreMaster.Manager;
using System;
using System.Collections;
using System.Linq;
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
        CustomText = "Isn't the view just beautiful? When I just look at this, all my thought feel way less heavier than before. It feels almost... empty. Have you already looked around here a bit? " +
            "These crystals here contain an mysterious power. Although it seemed to me, that they people actually looked for something even more powerful. Here, I found that crystal from the remains of another adventurer." +
            " It emits a power far beyond everything else that you probably can mine here. It only needs a fitting vessel, but for an adventurer like you, this shouldn't be a problem.";
    }

    #endregion

    #region Properties

    public bool HasDiamondDash => PowerManager.HasObtainedPower("MYLA");

    public SpriteRenderer CrystalHeartSprite => _crystalHeartSprite == null ? _crystalHeartSprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Super Dash").GetComponent<SpriteRenderer>() : _crystalHeartSprite;

    public override Action SceneAction => () =>
    {
        if (State == PowerState.Active)
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

    #region Event handler

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        _trailSprite = GameObject.Find("Knight").transform.Find("Effects/SD Trail").GetComponent<tk2dSprite>();
    }

    private bool HeroController_CanTakeDamage(On.HeroController.orig_CanTakeDamage orig, HeroController self)
    {
        if (orig(self))
            return _speed > -39f && _speed < 39f;
        return false;
    }

    private void CallMethodProper_OnEnter(On.HutongGames.PlayMaker.Actions.CallMethodProper.orig_OnEnter orig, HutongGames.PlayMaker.Actions.CallMethodProper self)
    {
        orig(self);
        if (self.IsCorrectContext("Superdash", "Knight", "Enter Super Dash") && string.Equals(self.methodName.Value, "SetCState"))
            LoreMaster.Instance.Handler.StartCoroutine(ChargeUp());
    }

    private void ActivateGameObject_OnEnter(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, HutongGames.PlayMaker.Actions.ActivateGameObject self)
    {
        orig(self);
        if (State == PowerState.Active && (self.IsCorrectContext("Superdash", "Knight", "Dash Start") || self.IsCorrectContext("Superdash", "Knight", "Hit Wall")) && string.Equals(self.gameObject.GameObject.Name, "SuperDash Damage"))
        {
            if (string.Equals(self.State.Name, "Dash Start"))
            {
                _carryingSpeed = HeroController.instance.cState.facingRight ? 30f : -30f;
                _carryingDamage = 10;
                LoreMaster.Instance.Handler.StartCoroutine(ChargeUp());
            }
            else
                foreach (HealthManager enemy in _enemies.Where(x=> x != null))
                    _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(StunEnemy(enemy?.gameObject));
        }
    }

    private void ListenForJump_OnEnter(On.HutongGames.PlayMaker.Actions.ListenForJump.orig_OnEnter orig, HutongGames.PlayMaker.Actions.ListenForJump self)
    {
        if (self.IsCorrectContext("Superdash", "Knight", "Cancelable"))
            self.wasPressed = State == PowerState.Twisted ? null : FsmEvent.GetFsmEvent("NORM CANCEL");
        orig(self);
    }

    private void ListenForSuperdash_OnEnter(On.HutongGames.PlayMaker.Actions.ListenForSuperdash.orig_OnEnter orig, HutongGames.PlayMaker.Actions.ListenForSuperdash self)
    {
        if (self.IsCorrectContext("Superdash", "Knight", "Cancelable"))
            self.wasPressed = State == PowerState.Twisted ? null : FsmEvent.GetFsmEvent("NORM CANCEL");
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        try
        {
            _originalSprite = CrystalHeartSprite.sprite;
            _corelessSprite = SpriteHelper.CreateSprite("DiamondHeart_Coreless");
            _shelllessSprite = SpriteHelper.CreateSprite("DiamondHeart_Shellless");
            _diamondSprite = SpriteHelper.CreateSprite("DiamondHeart");
            _trailSprite = GameObject.Find("Knight").transform.Find("Effects/SD Trail").GetComponent<tk2dSprite>();
            On.HeroController.Start += HeroController_Start;
            On.HutongGames.PlayMaker.Actions.ListenForJump.OnEnter += ListenForJump_OnEnter;
            On.HutongGames.PlayMaker.Actions.ListenForSuperdash.OnEnter += ListenForSuperdash_OnEnter;
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in initialize: " + exception.Message);
        }
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HeroController.Start -= HeroController_Start;
        On.HutongGames.PlayMaker.Actions.ListenForJump.OnEnter -= ListenForJump_OnEnter;
        On.HutongGames.PlayMaker.Actions.ListenForSuperdash.OnEnter -= ListenForSuperdash_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        _enemies = GameObject.FindObjectsOfType<HealthManager>();
        CrystalHeartSprite.sprite = HasDiamondDash ? _diamondSprite : _shelllessSprite;
        if (HasDiamondDash)
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value = .2f;
        On.HeroController.CanTakeDamage += HeroController_CanTakeDamage;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += CallMethodProper_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (HasDiamondDash)
        {
            CrystalHeartSprite.sprite = _corelessSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value = .5f;
        }
        else
            CrystalHeartSprite.sprite = _originalSprite;
        _enemies = null;
        On.HeroController.CanTakeDamage -= HeroController_CanTakeDamage;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter -= CallMethodProper_OnEnter;
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
        if (enemy == null)
            yield break;
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