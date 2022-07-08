using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.CityOfTears;

public class SoulExtractEfficiencyPower : Power
{
    #region Constructors

    public SoulExtractEfficiencyPower() : base("Soul Extract Efficiency", Area.CityOfTears)
    {
        Hint = "Allows you to drain soul more effienct from your foes.";
        Description = "You gain 5 more soul per hit on enemies.";
    }

    #endregion

    #region Event handler

    /// <summary>
    /// Handles the soul gain on hit for enemies.
    /// </summary>
    /// <param name="soulToGain"></param>
    /// <returns></returns>
    private int OnSoulGain(int soulToGain) => soulToGain + 5;

    #endregion

    #region Protected Methods

    protected override void Enable() => ModHooks.SoulGainHook += OnSoulGain;
    
    protected override void Disable() => ModHooks.SoulGainHook -= OnSoulGain;
    
    #endregion
}

