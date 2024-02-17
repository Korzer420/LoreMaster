using KorzUtils.Helper;
using LoreMaster.Enums;
using Modding;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class BlessingOfTheButterflyPower : Power
{
    #region Members

    private GameObject _wings;

    private GameObject _leftHitbox;

    private GameObject _rightHitbox;

    private MethodInfo _takeDamage;

    private bool _doubleJumpOnCooldown;

    #endregion

    #region Constructors

    public BlessingOfTheButterflyPower() : base("Blessing of the Butterfly", Area.CityOfTears)
    {
        Hint += " If it wasn't for the audience power, you can be 100% sure that I'd end your game immediately if you kill her. Just a fair warning, I'll not be that kind for another power in the future. " +
            "Don't mess with the innocent and happy bugs in this game. >:c";
        Description += " If it wasn't for the audience power, you can be 100% sure that I'd end your game immediately if you kill her. Just a fair warning, I'll not be that kind for another power in the future. " +
            "Don't mess with the innocent and happy bugs in this game. >:c";
    }

    #endregion

    #region Properties

    public GameObject Wings
    {
        get
        {
            if (_wings == null)
                Initialize();
            return _wings; 
        }
    }

    public override PowerRank Rank => PowerRank.Medium;

    #endregion

    #region Event handler

    private void HeroController_DoDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
    {
        orig(self);
        LoreMaster.Instance.Handler.StartCoroutine(ActivateHitbox());
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (go.transform.position.y < HeroController.instance.transform.position.y && hazardType == 1 && ReflectionHelper.GetField<HeroController, bool>(HeroController.instance, "doubleJumped"))
        {
            // If it is an enemy, we deal damage to them.
            if (go.GetComponent<HealthManager>() is HealthManager hp)
            {
                HitInstance hitInstance = new()
                {
                    AttackType = AttackTypes.Nail,
                    DamageDealt = 4,
                    MagnitudeMultiplier = 1,
                    Multiplier = 1,
                    Direction = 270,
                    Source = HeroController.instance.gameObject
                };
                _takeDamage.Invoke(hp, new object[] { hitInstance });
            }
            LoreMaster.Instance.Handler.StartCoroutine(FakeJump());
        }
        else
            orig(self, go, damageSide, damageAmount, hazardType);
    }

    /// <summary>
    /// Modify the jump height of double jump slightly.
    /// </summary>
    /// <param name="il"></param>
    private void HeroController_DoubleJump(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);

        // Normal double jump velocity is jump speed times 1.1. We double the bonus.
        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchNewobj<Vector2>()))
            cursor.EmitDelegate<Func<Vector2, Vector2>>(velocity => new(velocity.x, HeroController.instance.JUMP_SPEED * 1.2f));
        else
            LoreMaster.Instance.LogError("Couldn't find Vector2 match for double jump.");
    }

    private bool HeroController_CanDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
    {
        return orig(self) && !_doubleJumpOnCooldown;
    }

    private void DoubleJumpCooldown(On.HeroController.orig_DoDoubleJump orig, HeroController self)
    {
        orig(self);
        LoreMaster.Instance.Handler.StartCoroutine(DoubleJumpCooldown());
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _wings = HeroController.instance.transform.Find("Effects/Double J Wings").gameObject;
        if (_leftHitbox != null)
            GameObject.Destroy(_leftHitbox);
        _leftHitbox = GameObject.Instantiate(HeroController.instance.transform.Find("Charm Effects/Thorn Hit/Hit L").gameObject, Wings.transform);
        _leftHitbox.transform.localPosition = new(-1.0555f, -0.6111f, 0f);
        _leftHitbox.transform.localScale = new(3.5f, 2.3f);
        Component.Destroy(_leftHitbox.GetComponent<PolygonCollider2D>());
        _leftHitbox.AddComponent<BoxCollider2D>().isTrigger = true;
        Component.Destroy(_leftHitbox.LocateMyFSM("set_thorn_damage"));
        PlayMakerFSM fsm = _leftHitbox.LocateMyFSM("damages_enemy");
        fsm.FsmVariables.FindFsmInt("damageDealt").Value = 12;
        fsm.FsmVariables.FindFsmFloat("magnitudeMult").Value = 4;
        fsm.FsmVariables.FindFsmFloat("direction").Value = 270;
        fsm.FsmVariables.FindFsmInt("attackType").Value = 2;

        // Check for terrain
        fsm.GetState("Send Event").InsertActions(2, () =>
        {
            if (fsm.FsmVariables.FindFsmInt("Layer").Value == 8)
                fsm.SendEvent("CANCEL");
        });

        if (_rightHitbox != null)
            GameObject.Destroy(_rightHitbox);
        _rightHitbox = GameObject.Instantiate(HeroController.instance.transform.Find("Charm Effects/Thorn Hit/Hit R").gameObject, Wings.transform);
        _rightHitbox.transform.localPosition = new(1.02f, -0.6111f);
        _rightHitbox.transform.localScale = new(3.5f, 2.3f);
        Component.Destroy(_rightHitbox.GetComponent<PolygonCollider2D>());
        _rightHitbox.AddComponent<BoxCollider2D>().isTrigger = true;
        Component.Destroy(_rightHitbox.LocateMyFSM("set_thorn_damage"));
        fsm = _rightHitbox.LocateMyFSM("damages_enemy");
        fsm.FsmVariables.FindFsmInt("damageDealt").Value = 12;
        fsm.FsmVariables.FindFsmFloat("magnitudeMult").Value = 4;
        fsm.FsmVariables.FindFsmFloat("direction").Value = 270;
        fsm.FsmVariables.FindFsmInt("attackType").Value = 2;

        // Check for terrain
        fsm.GetState("Send Event").InsertActions(2, () =>
        {
            if (fsm.FsmVariables.FindFsmInt("Layer").Value == 8)
                fsm.SendEvent("CANCEL");
        });

        _leftHitbox.gameObject.SetActive(false);
        _rightHitbox.gameObject.SetActive(false);
        _takeDamage = ReflectionHelper.GetMethodInfo(typeof(HealthManager), "TakeDamage");
    }
    
    /// <inheritdoc/>
    protected override void Enable()
    {
        Wings.GetComponent<tk2dSprite>().color = Color.magenta;
        Wings.transform.localScale = new(1.8f, 1.8f, 1f);
        _leftHitbox.gameObject.SetActive(true);
        _rightHitbox.gameObject.SetActive(true);
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        On.HeroController.DoDoubleJump += HeroController_DoDoubleJump;
        IL.HeroController.DoubleJump += HeroController_DoubleJump;
    }
    
    /// <inheritdoc/>
    protected override void Disable()
    {
        Wings.GetComponent<tk2dSprite>().color = Color.white;
        Wings.transform.localScale = new(1f, 1f, 1f);
        _leftHitbox.gameObject.SetActive(false);
        _rightHitbox.gameObject.SetActive(false);
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        On.HeroController.DoDoubleJump -= HeroController_DoDoubleJump;
        IL.HeroController.DoubleJump -= HeroController_DoubleJump;
    }
    
    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HeroController.CanDoubleJump += HeroController_CanDoubleJump;
        On.HeroController.DoDoubleJump += DoubleJumpCooldown;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HeroController.CanDoubleJump -= HeroController_CanDoubleJump;
        On.HeroController.DoDoubleJump -= DoubleJumpCooldown;
    }

    #endregion

    #region Private Methods

    private IEnumerator FakeJump()
    {
        float passedTime = 0f;
        while (passedTime < .2f)
        {
            HeroController.instance.GetComponent<Rigidbody2D>().velocity = new(0f, HeroController.instance.JUMP_SPEED);
            yield return null;
            passedTime += Time.deltaTime;
        }
    }

    private IEnumerator ActivateHitbox()
    {
        _leftHitbox.SetActive(true);
        _rightHitbox.SetActive(true);
        yield return new WaitForSeconds(.15f);
        _leftHitbox.SetActive(false);
        _rightHitbox.SetActive(false);
    }

    private IEnumerator DoubleJumpCooldown()
    {
        _doubleJumpOnCooldown = true;
        yield return new WaitForSeconds(3f);
        Wings.SetActive(true);
        _doubleJumpOnCooldown = false;
    }

    #endregion
}
