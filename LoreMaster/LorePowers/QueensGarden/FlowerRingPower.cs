using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System;

namespace LoreMaster.LorePowers.QueensGarden;

public class FlowerRingPower : Power
{
    #region Constructors

    public FlowerRingPower() : base("Ring of Flowers", Area.QueensGarden) { }

    #endregion

    #region Event Handler

    private void OnConvertFloatToIntAction(On.HutongGames.PlayMaker.Actions.ConvertFloatToInt.orig_OnEnter orig, ConvertFloatToInt self)
    {
        if (string.Equals(self.Fsm.FsmComponent.FsmName, "nailart_damage") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Set"))
        {
            if (Active)
            {
                float damageMultiplier = 1f;

                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.elderbugGaveFlower)))
                    damageMultiplier += .1f;
                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenEmilitiaFlower)))
                    damageMultiplier += .1f;
                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenGodseekerFlower)))
                    damageMultiplier += .1f;
                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenOroFlower)))
                    damageMultiplier += .1f;
                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenWhiteLadyFlower)))
                    damageMultiplier += .1f;

            // This applies onto the already modified damage value. This means that if the multiplier is 1.5f, the end damage is 3.75 times the nail damage.
            // With fury about 7 times (if I'm not that bad in math).
            self.floatVariable.Value *= damageMultiplier;
            }
        }
        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Enable() => On.HutongGames.PlayMaker.Actions.ConvertFloatToInt.OnEnter += OnConvertFloatToIntAction;

    /// <inheritdoc/>
    protected override void Disable() => On.HutongGames.PlayMaker.Actions.ConvertFloatToInt.OnEnter -= OnConvertFloatToIntAction;

    #endregion
}
