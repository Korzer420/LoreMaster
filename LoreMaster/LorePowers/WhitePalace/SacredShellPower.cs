using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.WhitePalace;

internal class SacredShellPower : Power
{
    #region Constructors

    public SacredShellPower() : base("", Area.WhitePalace)
    {

    }

    #endregion

    #region Public Methods

    protected override void Enable()
    {
        ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
    }

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount) => damageAmount > 1 ? 1 : damageAmount;

    protected override void Disable()
    {
        ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
    }

    #endregion
}
