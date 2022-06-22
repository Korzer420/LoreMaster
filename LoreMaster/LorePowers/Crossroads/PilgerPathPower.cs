using GlobalEnums;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.Crossroads
{
    public class PilgerPathPower : Power
    {

        public bool IsPlayerGrounded
        {
            get => (int)HeroController.instance.hero_state < 3;
        }

        public PilgerPathPower() : base("PILGRIM_TAB_01", Area.Crossroads)
        => Description = "<br>[Reluctant Pilger]<br>While you stay on the path, your nail shall receive the gift of the grubfather.";


        public override void Enable()
        {
            ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
            ModHooks.GetPlayerIntHook += GetBeamDamage;
        }

        public override void Disable()
        {
            ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
            ModHooks.GetPlayerIntHook -= GetBeamDamage;
        }

        private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
        {
            if (name.Equals("equippedCharm_35") && !orig)
                orig = IsPlayerGrounded;
            return orig;
        }

        /// <summary>
        /// Event handler for getting the beam damage of grubber fly. If the player is grounded, it damage get's doubled.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="orig"></param>
        /// <returns></returns>
        private int GetBeamDamage(string name, int orig)
            => name.Equals("beamDamage") && IsPlayerGrounded ? orig * 2 : orig;
    }
}
