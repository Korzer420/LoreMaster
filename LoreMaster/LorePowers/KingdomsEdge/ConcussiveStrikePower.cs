using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LoreMaster.LorePowers.KingdomsEdge;

public class ConcussiveStrikePower : Power
{
    #region Members

    private GameObject[] _nailArts = new GameObject[3];

    private MethodInfo _invulnerableCall;

    private GameObject _concussion;

    #endregion

    #region Constructors

    public ConcussiveStrikePower() : base("Concussive Strikes", Area.KingdomsEdge)
    {
        CustomText = "Oh hello little thing. Are you the one you tickled my earlier? Don't worry if that's the case, a big guy like me can take this without a problem. These foes here never would dare approach my face. " +
            "My arms crush everything harmful that comes close to them. That's why I'm glad a fellow traveler found their way to me. Let me teach you the secret of my strikes as a sign of friendship.";
    }

    #endregion

    #region Properties

    public GameObject[] NailArts
    {
        get
        {
            if (_nailArts.Any(x => x == null))
                Initialize();
            return _nailArts;
        }
    }

    public MethodInfo InvulnerableCall => _invulnerableCall == null ? _invulnerableCall = HeroController.instance.GetType().GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance) : _invulnerableCall;

    public override PowerRank Rank => PowerRank.Greater;

    #endregion

    #region Event Handler

    private void EnemyTookHit(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        ConcussionEffect concussive = self.GetComponentInChildren<ConcussionEffect>();
        if (concussive == null)
        {
            // Only apply concussion on great slash or dash slash hit
            if (hitInstance.AttackType == AttackTypes.Nail && (NailArts[0].activeSelf || NailArts[1].activeSelf))
            {
                GameObject child = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Mantis Heavy Flyer"], self.transform);
                child.name = "Concussion";
                child.AddComponent<ConcussionEffect>();
                // This should place the concussive sprite on top of the enemy.
                child.transform.localPosition = new(0, 0 + self.transform.GetComponent<BoxCollider2D>().size.y / 2);
            }
        }
        else if (hitInstance.DamageDealt > 0)
        {
            // Nail hits extends the duration of concussion (except cyclone slash) and increase their damage
            if (hitInstance.AttackType == AttackTypes.Nail && !NailArts[2].activeSelf)
            {
                concussive.ConcussiveTime += 1f;
                hitInstance.DamageDealt = Convert.ToInt16(hitInstance.DamageDealt * 1.1f) + 1;
            }

            // Modify knockback
            hitInstance.MagnitudeMultiplier += .5f;
        }
        orig(self, hitInstance);
    }

    private void TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (State == PowerState.Twisted)
        {
            if (self.GetComponentInChildren<ConcussionEffect>() is ConcussionEffect effect)
                effect.ConcussiveTime += .5f;
            else
                Concussion();
        }
        else if (damageAmount > 0 && go.GetComponentInChildren<ConcussionEffect>(true) != null && LoreMaster.Instance.Generator.Next(1, 3) == 1)
        {
            damageAmount--;
            if (damageAmount <= 0)
            {
                // If the enemy deals no damage it will not trigger the i frames and cause the knight to take the damage next frame, which would make this rather pointless.
                // Therefore we doing some witchcraft to trigger the i frames manually if no damage is applied because of this.
                _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine((IEnumerator)InvulnerableCall.Invoke(HeroController.instance, new object[] { 1.5f }));
            }
        }

        orig(self, go, damageSide, damageAmount, hazardType);
    }

    private void HeroController_Move(On.HeroController.orig_Move orig, HeroController self, float move_direction)
    {
        if (_concussion != null)
            move_direction *= -1f;
        orig(self, move_direction);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject attacks = GameObject.Find("Knight/Attacks");
        _nailArts[0] = attacks.transform.Find("Great Slash").gameObject;
        _nailArts[1] = attacks.transform.Find("Dash Slash").gameObject;
        _nailArts[2] = attacks.transform.Find("Cyclone Slash").gameObject;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HealthManager.TakeDamage += EnemyTookHit;
        On.HeroController.TakeDamage += TakeDamage;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= EnemyTookHit;
        On.HeroController.TakeDamage -= TakeDamage;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HeroController.Move += HeroController_Move;
        On.HeroController.TakeDamage += TakeDamage;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HeroController.Move -= HeroController_Move;
        On.HeroController.TakeDamage -= TakeDamage;
    }

    #endregion

    #region Methods

    private void Concussion()
    {
        _concussion = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Mantis Heavy Flyer"], HeroController.instance.transform);
        _concussion.name = "Concussion";
        _concussion.AddComponent<ConcussionEffect>().ConcussiveTime = 1f;
        _concussion.transform.localPosition = new(0f, 1f);
        _concussion.transform.localScale = new(1f, 1f);
    }
    #endregion
}