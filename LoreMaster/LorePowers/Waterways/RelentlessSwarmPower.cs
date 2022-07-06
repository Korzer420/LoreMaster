using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.Waterways;

public class RelentlessSwarmPower : Power
{
    #region Constructors

    public RelentlessSwarmPower() : base("", Area.WaterWays)
    {
        
    }

    #endregion

    #region Event handler

    private void SpellFluke_DoDamage(On.SpellFluke.orig_DoDamage orig, SpellFluke self, GameObject obj, int upwardRecursionAmount, bool burst)
    {
        orig(self, obj, upwardRecursionAmount, burst);
        if (obj.GetComponent<HealthManager>().isDead)
            HeroController.instance.AddMPCharge(5);
        else
            HeroController.instance.AddMPCharge(2);
    }

    //private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    //{
    //    orig(self,hitInstance);
    //    if (self.GetComponent<Concussive>() == null)
    //        self.gameObject.AddComponent<Concussive>();
    //}

    #endregion

    #region Public Methods

    protected override void Enable()
    {
        On.SpellFluke.DoDamage += SpellFluke_DoDamage;
        //On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    }

    protected override void Disable()
    {
        On.SpellFluke.DoDamage -= SpellFluke_DoDamage;
    }

    #endregion
}

//public class Concussive : MonoBehaviour
//{
//    private void OnCollisionEnter2D(Collision2D other)
//    {
//        LoreMaster.Instance.Log("Called OnCollisionEnter with " + other.gameObject.name);
//    }

//    private void OnCollisionStay2D(Collision2D other)
//    {
//        LoreMaster.Instance.Log("Called OnColissionStay with " + other.gameObject.name);
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        LoreMaster.Instance.Log("Called OnTriggerEnter with " + other.gameObject.name);
//    }

//    void OnTriggerStay2D(Collider2D other)
//    {
//        LoreMaster.Instance.Log("Called OnTriggerStay with " + other.gameObject.name);
//    }
//}
