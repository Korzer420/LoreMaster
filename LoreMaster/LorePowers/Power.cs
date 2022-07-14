using LoreMaster.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers
{
    public abstract class Power
    {
        #region Members

        protected bool _initialized;

        #endregion

        #region Constructors

        public Power(string powerName, Area area)
        {
            PowerName = powerName;
            Location = area;
        }

        #endregion

        #region Properties

        public Area Location { get; protected set; }

        public string PowerName { get; set; }

        /// <summary>
        /// Gets or set the clear description of the power.
        /// <para/>Only used if <see cref="LoreMaster.UseHints"/> is <see langword="false"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the hint of the power. It gets displayed after the lore text (or <see cref="CustomText"/> if set) ingame.
        /// <para/>Only used if <see cref="LoreMaster.UseHints"/> is <see langword="true"/>.
        /// </summary>
        public string Hint { get; protected set; }

        /// <summary>
        /// Gets or sets the custom text. If this is filled, it replaces the original text of the source.
        /// </summary>
        public string CustomText { get; set; }

        public bool Active { get; set; }

        public PowerTag Tag { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the power. (Modifies fsm and get prefabs).
        /// </summary>
        protected virtual void Initialize() => _initialized = true;

        /// <summary>
        /// Enables this power in the overworld.
        /// </summary>
        protected virtual void Enable() { }

        /// <summary>
        /// Disables this power in the overworld.
        /// </summary>
        protected virtual void Disable() { }

        public void EnablePower()
        {
            if (Active || Tag == PowerTag.Disabled || Tag == PowerTag.Removed)
                return;
            try
            {
                if (!_initialized)
                { 
                    Initialize();
                    _initialized = true;
                }
                Enable();
                LoreMaster.Instance.LogDebug("Enabled " + PowerName);
                Active = true;
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("Error while loading " + PowerName + ": " + exception.Message);
            }
        }

        /// <summary>
        /// Disables the power
        /// </summary>
        /// <param name="backToMenu">This is used to tell <see cref="EnablePower"/> to do the initialize again, if you reload the game.</param>
        public void DisablePower(bool backToMenu)
        {
            if (!Active)
                return;
            // Disabling power could cause problems when returning to the menu, we ignore them.
            try
            {
                Disable();
                LoreMaster.Instance.LogDebug("Disabled " + PowerName);
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("Error while disabling " + PowerName + ": " + exception.Message);
            }
            Active = false;
            _initialized = !backToMenu;
        }

        #endregion
    }
}
