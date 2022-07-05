using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

public class FlowerRingPower : Power
{
    #region Constructors

    public FlowerRingPower() : base("", Area.QueensGarden)
    {

    }

    #endregion

    #region Public Methods

    public override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.FsmName.Equals("nailart_damage"))
        {
            if (self.GetState("Set").Actions[0] is not Lambda)
                self.GetState("Set").AddFirstAction(new Lambda(() =>
                 {
                     if (IsCurrentlyActive())
                     {
                         float damageMultiplier = 1f;

                         if (PlayerData.instance.elderbugGaveFlower)
                             damageMultiplier += .1f;
                         if (PlayerData.instance.givenEmilitiaFlower)
                             damageMultiplier += .1f;
                         if (PlayerData.instance.givenGodseekerFlower)
                             damageMultiplier += .1f;
                         if(PlayerData.instance.givenOroFlower)
                             damageMultiplier += .1f;
                         if(PlayerData.instance.givenWhiteLadyFlower)
                             damageMultiplier += .1f;

                         // This applies onto the already modified damage value. This means that if the multiplier is 1.5f, the end damage is 3.75 times the nail damage.
                         // With fury about 7 times (if I'm not that bad in math).
                         self.FsmVariables.FindFsmFloat("Damage Float").Value *= damageMultiplier;
                     }
                 }));
        }
        orig(self);
    }

    public override void Disable() 
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }

    #endregion
}
