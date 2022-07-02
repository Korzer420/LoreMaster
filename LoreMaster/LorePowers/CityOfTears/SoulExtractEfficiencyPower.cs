using Modding;
using System;
using System.Collections.Generic;
namespace LoreMaster.LorePowers.CityOfTears;

internal class SoulExtractEfficiencyPower : Power
{
    #region Constructors

    public SoulExtractEfficiencyPower() : base("Something", Area.CityOfTears)
    {
       
    }

    #endregion

    #region Event handler

    private int ModHooks_SoulGainHook(int soulToGain) => soulToGain + 5;

    #endregion

    #region Methods

    public override void Enable()
    {
        ModHooks.SoulGainHook += ModHooks_SoulGainHook;
    }

    public override void Disable()
    {
        ModHooks.SoulGainHook -= ModHooks_SoulGainHook;
    }

    #endregion
}

