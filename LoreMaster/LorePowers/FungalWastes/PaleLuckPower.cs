using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.FungalWastes
{
    public class PaleLuckPower: Power 
    {
        #region Constructors

        public PaleLuckPower() : base("FUNG_TAB_02", Area.FungalWastes)
        {
            Description = "<br>[Pale Luck]<br>When someone casts harm on you, sometimes you are blessed by the higher being instead. Especially if you have some artefacts related to him.";
        }

        #endregion

        #region Methods

        public override void Enable()
        {
            ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;
        }

        private int ModHooks_TakeDamageHook(int hazardType, int damage)
        {
            int chance = 1;

            // Chance increases with king's brand and kingssoul
            if (PlayerData.instance.hasKingsBrand)
                chance += 2;
            if (PlayerData.instance.GetBool("equippedCharm_36"))
                chance += 2;

            int rolledValue = LoreMaster.Instance.Generator.Next(0, 100);

            if (rolledValue < chance)
            {
                if (PlayerData.instance.health < PlayerData.instance.maxHealth)
                    HeroController.instance.AddHealth(1);
                damage = 0;
            }
            return damage;
        }

        public override void Disable()
        {
            ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;
        }

        #endregion
    }
}
