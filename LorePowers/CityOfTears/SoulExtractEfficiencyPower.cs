using LoreMaster.Enums;
using Modding;
using System.Collections;
using UnityEngine;

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
    private int OnSoulGain(int soulToGain) => State == PowerState.Active ? soulToGain + 5 : soulToGain / 2;

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

    /// <inheritdoc/>
    protected override void TwistEnable() 
    { 
        ModHooks.SoulGainHook += OnSoulGain;
        StartRoutine(() => LeakingVessel());
    }

    /// <inheritdoc/>
    protected override void TwistDisable() => ModHooks.SoulGainHook += OnSoulGain;

    #endregion

    #region Methods

    /// <summary>
    /// Removes souls slowly over time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator LeakingVessel()
    {
        float passedTime = 0f;
        while (true)
        {
            passedTime += Time.deltaTime;
            if(passedTime >= 1f)
            {
                passedTime = 0f;
                if (PlayerData.instance.GetInt("MPReserve") > 0)
                    HeroController.instance.TakeReserveMP(1);
                else if (PlayerData.instance.GetInt("MPCharge") > 0)
                    HeroController.instance.TakeMP(1);
            }
            yield return null;
        }
    }

    #endregion
}

