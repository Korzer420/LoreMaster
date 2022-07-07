using ItemChanger.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.KingdomsEdge;

public class ConcussiveStrikePower : Power
{
    #region Members

    private GameObject[] _nailArts = new GameObject[3];

    private MethodInfo _invulnableCall;

    #endregion

    #region Constructors

    public ConcussiveStrikePower() : base("", Area.KingdomsEdge)
    {
        
    }

    #endregion

    #region Event Handler

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        LoreMaster.Instance.Log("Knockback: " + hitInstance.MagnitudeMultiplier);
        ConcussionEffect concussive = self.GetComponentInChildren<ConcussionEffect>();
        if (concussive == null)
        {
            // Only apply concussion on great slash or dash slash hit
            if (hitInstance.AttackType == AttackTypes.Nail && (_nailArts[0].activeSelf || _nailArts[1].activeSelf))
            {
                GameObject child = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Shot Mantis"], self.transform);
                child.name = "Concussion";
                child.AddComponent<ConcussionEffect>();
                // This should place the concussive sprite on top of the enemy.
                child.transform.localPosition = new(0, 0 + self.transform.GetComponent<BoxCollider2D>().size.y / 2);
            }
        }
        else if(hitInstance.DamageDealt > 0)
        {
            // Nail hits extends the duration of concussion (except cyclone slash) and increase their damage
            if (hitInstance.AttackType == AttackTypes.Nail && !_nailArts[2].activeSelf)
            {
                concussive.ConcussiveTime += 1f;
                hitInstance.DamageDealt += Convert.ToInt16(hitInstance.DamageDealt * 1.1f) + 1;
            }

            // Modify knockback
            hitInstance.MagnitudeMultiplier += .5f;
        }
        orig(self, hitInstance);
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (damageAmount > 0 && go.GetComponentInChildren<ConcussionEffect>(true) != null && LoreMaster.Instance.Generator.Next(1, 11) == 1)
        { 
            damageAmount--;
            if (damageAmount <= 0)
            {
                // If the enemy deals no damage it will not trigger the i frames and cause the knight to take the damage next frame, which would make this rather pointless.
                // Therefore we doing some witchcraft to trigger the i frames manually if no damage is applied because of this.
                LoreMaster.Instance.Handler.StartCoroutine((IEnumerator)_invulnableCall.Invoke(HeroController.instance, new object[] { 1.5f }));
            }
        }

        orig(self, go, damageSide, damageAmount, hazardType);
    }

    #endregion

    #region Public Methods

    protected override void Initialize()
    {
        GameObject attacks = GameObject.Find("Knight/Attacks");
        _nailArts[0] = attacks.transform.Find("Great Slash").gameObject;
        _nailArts[1] = attacks.transform.Find("Dash Slash").gameObject;
        _nailArts[2] = attacks.transform.Find("Cyclone Slash").gameObject;
        _invulnableCall = HeroController.instance.GetType().GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    protected override void Enable()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
    }

    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
    }

    #endregion
}


