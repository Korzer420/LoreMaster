using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.FungalWastes
{
    public class ImposterPower : Power
    {
        #region Constructors

        public ImposterPower() : base("FUNG_TAB_02", Area.FungalWastes)
        {
            Description = "<br>[Among Us]<br>While wearing spore shroom, sometimes a lifeblood is added when healing.";
            CustomText = "Pity those bugs. Their society shattered to pieces. While our kind should survive it all, we fear that they are imposter among us, which causes the blue illness upon our colony.";
        }

        #endregion

        #region Methods

        public override void Enable()
        {
            On.HeroController.AddHealth += ExtraHeal;
        }

        public override void Disable()
        {
            On.HeroController.AddHealth -= ExtraHeal;
        }

        private void ExtraHeal(On.HeroController.orig_AddHealth orig, HeroController self, int amount)
        {
            if (PlayerData.instance.GetBool("equippedCharm_17") && LoreMaster.Instance.Generator.Next(0, 5) == 0 && PlayerData.instance.healthBlue < 3)
                EventRegister.SendEvent("ADD BLUE HEALTH");
            
            orig(self, amount);
        }

        #endregion
    }
}
