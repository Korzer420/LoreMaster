using LoreMaster.Enums;
using LoreMaster.Helper;

namespace LoreMaster.LorePowers.FungalWastes;

public class ImposterPower : Power
{
    #region Constructors

    public ImposterPower() : base("Imposter", Area.FungalWastes)
    {
        CustomText = "Pity those bugs. Their society shattered to pieces. While our kind should survive it all, we fear that they are imposter among us, which causes the blue illness upon our colony.";
    }

    #endregion

    #region Event Handler

    private void CallMethodProper_OnEnter(On.HutongGames.PlayMaker.Actions.CallMethodProper.orig_OnEnter orig, HutongGames.PlayMaker.Actions.CallMethodProper self)
    {
        if((self.IsCorrectContext("Spell Control", "Knight", "Focus Heal") || self.IsCorrectContext("Spell Control", "Knight", "Focus Heal 2"))
            && string.Equals(self.methodName.Value, "AddHealth"))
        {
            if (State == PowerState.Twisted && LoreMaster.Instance.Generator.Next(0, PlayerData.instance.GetBool("equippedCharm_17") ? 10 : 5) == 0)
                self.parameters[0].SetValue(0);
            else if (State == PowerState.Active && PlayerData.instance.GetBool("equippedCharm_17") && LoreMaster.Instance.Generator.Next(0, 5) == 0 && PlayerData.instance.healthBlue < 5)
                EventRegister.SendEvent("ADD BLUE HEALTH");
        }
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += CallMethodProper_OnEnter;

    /// <inheritdoc/>
    protected override void Disable() => On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter -= CallMethodProper_OnEnter;

    /// <inheritdoc/>
    protected override void TwistEnable() => Enable();

    /// <inheritdoc/>
    protected override void TwistDisable() => Disable();

    #endregion
}
