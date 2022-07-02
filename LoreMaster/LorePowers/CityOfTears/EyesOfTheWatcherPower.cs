using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.CityOfTears
{
    internal class EyesOfTheWatcherPower : Power
    {
        #region Constructors

        public EyesOfTheWatcherPower() : base("",Area.CityOfTears)
        {
            
        }

        #endregion

        #region Eventhandler

        private bool Lantern_Check(string name, bool original)
        {
            if (name.Equals("hasLantern"))
                return true;
            return original;
        }

        #endregion

        #region Methods

        public override void Enable()
        {
            ModHooks.GetPlayerBoolHook += Lantern_Check;
        }

        public override void Disable()
        {
            ModHooks.GetPlayerBoolHook -= Lantern_Check;
        } 

        #endregion
    }
}
