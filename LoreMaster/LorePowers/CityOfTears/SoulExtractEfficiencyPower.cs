using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.CityOfTears;

public class SoulExtractEfficiencyPower : Power
{
    #region Constructors

    public SoulExtractEfficiencyPower() : base("Soul Extract Efficiency", Area.CityOfTears) { }

    #endregion

    #region Event handler

    /// <summary>
    /// Handles the soul gain on hit for enemies.
    /// </summary>
    private int OnSoulGain(int soulToGain) => soulToGain + 5;

    private void OnSetBoolValueAction(On.HutongGames.PlayMaker.Actions.SetBoolValue.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetBoolValue self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Knight") && string.Equals(self.Fsm.Name, "Spell Control") && string.Equals(self.State.Name, "Inactive"))
        {
            int currentMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge));
            int maxMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.maxMP));
            int reserveMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPReserve));

            if (currentMP < maxMP && reserveMP > 0)
            {
                int neededMP = maxMP - currentMP;
                if (reserveMP - neededMP >= 0)
                    reserveMP -= neededMP;
            }
            HeroController.instance.TakeReserveMP(reserveMP);
            HeroController.instance.AddMPCharge(reserveMP);
        }
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() 
    { 
        ModHooks.SoulGainHook += OnSoulGain;
        On.HutongGames.PlayMaker.Actions.SetBoolValue.OnEnter += OnSetBoolValueAction;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.SoulGainHook -= OnSoulGain;
        On.HutongGames.PlayMaker.Actions.SetBoolValue.OnEnter -= OnSetBoolValueAction;
    }

    #endregion
}

