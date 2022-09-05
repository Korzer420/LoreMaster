using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
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

        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Knight") && string.Equals(self.Fsm.FsmComponent.FsmName, "Spell Control") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Inactive") && Active)
        {
            int currentMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge));
            int maxMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.maxMP));
            int reserveMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPReserve));

            if (currentMP < maxMP && reserveMP > 0)
            {
                int neededMP = maxMP - currentMP;
                if (reserveMP - neededMP >= 0)
                {
                    reserveMP -= neededMP;
                }
            }
            HeroController.instance.TakeReserveMP(reserveMP);
            HeroController.instance.AddMPCharge(reserveMP);
        }

        orig(self);
    }

    #endregion

        #region Protected Methods

    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.SetBoolValue.OnEnter += OnSetBoolValueAction;
    }

    /// <inheritdoc/>
    protected override void Enable() => ModHooks.SoulGainHook += OnSoulGain;
    
    /// <inheritdoc/>
    protected override void Disable() => ModHooks.SoulGainHook -= OnSoulGain;

    #endregion
}

