using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers
{
    public abstract class Power
    {
        #region Constructors

        public Power(string tabletName, Area area)
        {
            TabletName = tabletName;
            Location = area;
        }

        #endregion

        #region Properties

        public Area Location { get; protected set; }

        public string TabletName { get; protected set; }

        /// <summary>
        /// Gets or sets the description of the power. It gets displayed after the lore text (or <see cref="CustomText"/> if set) ingame. It should be hold a bit more ambigiuos.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Gets or sets the custom text. If this is filled, it replaces the original text of the source
        /// </summary>
        public string CustomText { get; set; }

        public bool Acquired { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Enables this power in the overworld.
        /// </summary>
        public abstract void Enable();

        public abstract void Disable();

        public bool IsCurrentlyActive()
        {
            return true;
        }

        #endregion
    }
}
