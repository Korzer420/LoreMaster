using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.CityOfTears
{
    public class TouristPower : Power
    {
        #region Constructors

        public TouristPower() : base("FOUNTAIN_PLAQUE_MAIN", Area.CityOfTears)
        {
            Description = "[Tourist Magnet]Want to see the incredible black egg temple? Talk to the sleeping firefly to the right.";
        }

        #endregion

        #region Properties

        public static bool Inspected { get; set; }

        #endregion

        #region Methods

        public override void Enable()
        {
            Inspected = true;
        }

        public override void Disable()
        {
            Inspected = false;
        }

        #endregion
    }
}
