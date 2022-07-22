using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.WhitePalace;

public class SacredShellPower : Power
{
    #region Constructors

    public SacredShellPower() : base("Sacred Shell", Area.WhitePalace)
    {
        Hint = "Infuse your shell with the pale power of the monarch which grants you tenacity against strong strikes.";
        Description = "You can longer take more than one damage per hit (excluding overcharmed).";
    }

    #endregion

    #region Event Handler

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount) => damageAmount > 1 ? 1 : damageAmount;

    #endregion

    #region Protected Methods

    protected override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
    
    protected override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
    
    #endregion
}
