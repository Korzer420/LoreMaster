using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.WhitePalace;

public class SacredShellPower : Power
{
    #region Constructors

    public SacredShellPower() : base("Sacred Shell", Area.WhitePalace) { }

    #endregion

    #region Event Handler

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount) => State == PowerState.Twisted 
        ? damageAmount *= 2
        : (damageAmount > 1 ? 1 : damageAmount);

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;

    /// <inheritdoc/>
    protected override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;

    /// <inheritdoc/>
    protected override void TwistEnable() => ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;

    /// <inheritdoc/>
    protected override void TwistDisable() => ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;

    #endregion
}
